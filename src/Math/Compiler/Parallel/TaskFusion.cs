namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Identifies small tasks that can be fused together to reduce scheduling overhead.
/// Groups tasks by data affinity — tasks that operate on the same data are merged
/// into a single fused task.
/// </summary>
public sealed class TaskFusion
{
    private readonly int _fusionThreshold;
    private readonly int _maxFusedInputs;

    /// <summary>
    /// Initializes the task fuser.
    /// </summary>
    /// <param name="fusionThreshold">Maximum number of nodes to fuse into a single task.</param>
    /// <param name="maxFusedInputs">Maximum total input count for a fused task.</param>
    public TaskFusion(int fusionThreshold = 4, int maxFusedInputs = 16)
    {
        _fusionThreshold = fusionThreshold;
        _maxFusedInputs = maxFusedInputs;
    }

    /// <summary>
    /// Gets the fusion threshold.
    /// </summary>
    public int FusionThreshold => _fusionThreshold;

    /// <summary>
    /// Analyzes the computation graph and returns fused task groups.
    /// Each group represents nodes that should be executed as a single task.
    /// </summary>
    /// <param name="graph">The computation graph to analyze.</param>
    /// <returns>List of fused task groups, each containing node IDs to fuse.</returns>
    public List<FusedTaskGroup> Analyze(ComputationGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        var fusableGroups = new List<FusedTaskGroup>();
        var assigned = new HashSet<string>();

        var executionOrder = TopologicalSort(graph);

        var affinityGroups = ComputeDataAffinity(graph);

        foreach (var affinityGroup in affinityGroups)
        {
            var candidates = affinityGroup
                .Where(id => !assigned.Contains(id) && nodeMap.ContainsKey(id))
                .Select(id => nodeMap[id])
                .ToList();

            if (candidates.Count < 2)
                continue;

            var fused = TryFuseGroup(candidates, nodeMap, assigned);
            if (fused != null && fused.NodeIds.Count >= 2)
            {
                fusableGroups.Add(fused);
            }
        }

        foreach (var node in graph.Nodes)
        {
            if (!assigned.Contains(node.Id))
            {
                fusableGroups.Add(new FusedTaskGroup
                {
                    NodeIds = new List<string> { node.Id },
                    EstimatedCost = 1
                });
                assigned.Add(node.Id);
            }
        }

        return fusableGroups;
    }

    /// <summary>
    /// Applies the fusion plan to the computation graph, creating fused nodes.
    /// </summary>
    /// <param name="graph">The computation graph to modify.</param>
    /// <param name="groups">The fused task groups to apply.</param>
    /// <returns>A new computation graph with fused tasks.</returns>
    public ComputationGraph ApplyFusion(ComputationGraph graph, List<FusedTaskGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(groups);

        var fusedGraph = new ComputationGraph();
        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        var fusedNodeMap = new Dictionary<string, string>();

        foreach (var group in groups)
        {
            if (group.NodeIds.Count <= 1)
            {
                foreach (var nodeId in group.NodeIds)
                {
                    if (nodeMap.TryGetValue(nodeId, out var originalNode))
                    {
                        fusedGraph.AddNode(originalNode);
                        fusedNodeMap[nodeId] = nodeId;
                    }
                }
                continue;
            }

            var fusedId = string.Join("_fused_", group.NodeIds);
            var fusedDependencies = new List<string>();
            var fusedOperation = new List<GraphNode>();

            foreach (var nodeId in group.NodeIds)
            {
                if (!nodeMap.TryGetValue(nodeId, out var node))
                    continue;

                fusedOperation.Add(node);

                foreach (var dep in node.Dependencies)
                {
                    if (!group.NodeIds.Contains(dep))
                    {
                        var resolvedDep = fusedNodeMap.TryGetValue(dep, out var mapped)
                            ? mapped : dep;
                        if (!fusedDependencies.Contains(resolvedDep))
                            fusedDependencies.Add(resolvedDep);
                    }
                }
            }

            var fusedNode = new GraphNode
            {
                Id = fusedId,
                Dependencies = fusedDependencies,
                Execute = inputs => ExecuteFusedOperation(fusedOperation, inputs)
            };

            fusedGraph.AddNode(fusedNode);

            foreach (var nodeId in group.NodeIds)
                fusedNodeMap[nodeId] = fusedId;
        }

        foreach (var node in graph.Nodes)
        {
            if (!fusedNodeMap.ContainsKey(node.Id))
            {
                fusedGraph.AddNode(node);
            }
        }

        return fusedGraph;
    }

    private static double ExecuteFusedOperation(
        List<GraphNode> operations,
        List<double> inputs)
    {
        var results = new Dictionary<string, double>();
        var inputIdx = 0;

        foreach (var op in operations)
        {
            var opInputs = new List<double>();
            foreach (var dep in op.Dependencies)
            {
                if (results.TryGetValue(dep, out var val))
                    opInputs.Add(val);
                else if (inputIdx < inputs.Count)
                    opInputs.Add(inputs[inputIdx++]);
            }

            var result = op.Execute(opInputs);
            results[op.Id] = result;
        }

        return operations.Count > 0 ? results[operations[^1].Id] : 0;
    }

    private static List<string> TopologicalSort(ComputationGraph graph)
    {
        var inDegree = new Dictionary<string, int>();
        foreach (var node in graph.Nodes)
            inDegree[node.Id] = node.Dependencies.Count;

        var queue = new Queue<string>();
        foreach (var (id, deg) in inDegree)
        {
            if (deg == 0)
                queue.Enqueue(id);
        }

        var order = new List<string>();
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            order.Add(id);

            foreach (var node in graph.Nodes)
            {
                if (node.Dependencies.Contains(id))
                {
                    inDegree[node.Id]--;
                    if (inDegree[node.Id] == 0)
                        queue.Enqueue(node.Id);
                }
            }
        }

        return order;
    }

    private static List<List<string>> ComputeDataAffinity(ComputationGraph graph)
    {
        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        var affinity = new Dictionary<string, HashSet<string>>();
        foreach (var node in graph.Nodes)
        {
            affinity[node.Id] = new HashSet<string> { node.Id };
        }

        foreach (var node in graph.Nodes)
        {
            foreach (var dep in node.Dependencies)
            {
                if (affinity.TryGetValue(node.Id, out var nodeSet) &&
                    affinity.TryGetValue(dep, out var depSet))
                {
                    if (nodeSet.Count + depSet.Count <= 16)
                    {
                        var merged = new HashSet<string>(nodeSet);
                        merged.UnionWith(depSet);

                        foreach (var id in merged)
                            affinity[id] = merged;
                    }
                }
            }
        }

        var groups = new List<HashSet<string>>();
        var visited = new HashSet<string>();

        foreach (var (id, group) in affinity)
        {
            if (!visited.Contains(id))
            {
                groups.Add(group);
                foreach (var memberId in group)
                    visited.Add(memberId);
            }
        }

        return groups.Select(g => g.ToList()).ToList();
    }

    private FusedTaskGroup? TryFuseGroup(
        List<GraphNode> candidates,
        Dictionary<string, GraphNode> nodeMap,
        HashSet<string> assigned)
    {
        if (candidates.Count < 2 || candidates.Count > _fusionThreshold)
            return null;

        var totalInputs = candidates.Sum(n => n.Dependencies.Count);
        if (totalInputs > _maxFusedInputs)
            return null;

        var fusedIds = new List<string>();
        foreach (var node in candidates)
        {
            if (assigned.Contains(node.Id))
                return null;
            fusedIds.Add(node.Id);
        }

        foreach (var id in fusedIds)
            assigned.Add(id);

        return new FusedTaskGroup
        {
            NodeIds = fusedIds,
            EstimatedCost = candidates.Count
        };
    }
}

/// <summary>
/// Represents a group of computation graph nodes that should be fused into a single task.
/// </summary>
public sealed class FusedTaskGroup
{
    /// <summary>The IDs of nodes in this fused group.</summary>
    public List<string> NodeIds { get; set; } = new();

    /// <summary>Estimated computational cost of the fused task.</summary>
    public int EstimatedCost { get; set; }
}
