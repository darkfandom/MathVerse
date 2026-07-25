namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Represents a read-write conflict between two nodes.</summary>
/// <param name="NodeId">The node that creates the conflict.</param>
/// <param name="ConflictWith">The node it conflicts with.</param>
/// <param name="Resource">The shared resource name.</param>
/// <param name="ConflictKind">Whether it is a read-read, read-write, or write-write conflict.</param>
public sealed record DependencyConflict(int NodeId, int ConflictWith, string Resource, ConflictKind ConflictKind);

/// <summary>Enumerates kinds of data conflicts.</summary>
public enum ConflictKind
{
    /// <summary>Both nodes read the same resource (no conflict).</summary>
    ReadRead,

    /// <summary>One node reads and another writes the same resource.</summary>
    ReadWrite,

    /// <summary>Both nodes write the same resource.</summary>
    WriteWrite,
}

/// <summary>Result of dependency analysis.</summary>
/// <param name="Conflicts">All detected conflicts.</param>
/// <param name="ParallelismOpportunities">Groups of nodes that can run in parallel.</param>
/// <param name="BottleneckNodes">Nodes on the critical path.</param>
/// <param name="IndependentSubgraphs">Sets of nodes with no inter-dependencies.</param>
public sealed record DependencyAnalysisResult(
    IReadOnlyList<DependencyConflict> Conflicts,
    IReadOnlyList<IReadOnlyList<int>> ParallelismOpportunities,
    IReadOnlyList<int> BottleneckNodes,
    IReadOnlyList<IReadOnlyList<int>> IndependentSubgraphs);

/// <summary>Analyzes dependencies in a computation graph to find conflicts, parallelism opportunities, and bottlenecks.</summary>
public sealed class DependencyAnalyzer
{
    /// <summary>Performs full dependency analysis on the graph.</summary>
    public DependencyAnalysisResult Analyze(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var conflicts = FindConflicts(graph);
        var parallelism = FindParallelismOpportunities(graph);
        var bottlenecks = FindBottlenecks(graph);
        var subgraphs = FindIndependentSubgraphs(graph);

        return new DependencyAnalysisResult(conflicts, parallelism, bottlenecks, subgraphs);
    }

    /// <summary>Finds read-write and write-write conflicts between nodes.</summary>
    public IReadOnlyList<DependencyConflict> FindConflicts(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var conflicts = new List<DependencyConflict>();
        var nodeResources = new Dictionary<int, (HashSet<string> reads, HashSet<string> writes)>();

        foreach (var node in graph.Nodes.Values)
        {
            var reads = new HashSet<string>();
            var writes = new HashSet<string>();

            if (node.Operation == GraphOperation.Input)
            {
                writes.Add($"var_{node.Id}");
            }
            else
            {
                foreach (int inputId in node.Inputs)
                    reads.Add($"var_{inputId}");
                writes.Add($"var_{node.Id}");
            }

            nodeResources[node.Id] = (reads, writes);
        }

        var nodes = nodeResources.Keys.ToArray();
        for (int i = 0; i < nodes.Length; i++)
        {
            for (int j = i + 1; j < nodes.Length; j++)
            {
                var (readsA, writesA) = nodeResources[nodes[i]];
                var (readsB, writesB) = nodeResources[nodes[j]];

                foreach (string res in writesA.Intersect(writesB))
                    conflicts.Add(new DependencyConflict(nodes[i], nodes[j], res, ConflictKind.WriteWrite));

                foreach (string res in writesA.Intersect(readsB))
                    conflicts.Add(new DependencyConflict(nodes[i], nodes[j], res, ConflictKind.ReadWrite));

                foreach (string res in readsA.Intersect(writesB))
                    conflicts.Add(new DependencyConflict(nodes[j], nodes[i], res, ConflictKind.ReadWrite));
            }
        }

        return conflicts;
    }

    /// <summary>Finds groups of nodes that can be executed in parallel (no data dependencies between them).</summary>
    public IReadOnlyList<IReadOnlyList<int>> FindParallelismOpportunities(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var scheduler = new GraphScheduler();
        var schedule = scheduler.ScheduleLevelBased(graph);

        return schedule.Levels
            .Where(l => l.NodeIds.Count > 1)
            .Select(l => (IReadOnlyList<int>)l.NodeIds)
            .ToArray();
    }

    /// <summary>Finds bottleneck nodes on the critical path.</summary>
    public IReadOnlyList<int> FindBottlenecks(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetTopologicalOrder();
        var depth = new Dictionary<int, int>();
        var height = new Dictionary<int, int>();

        foreach (int nodeId in order)
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;
            int d = 0;
            foreach (int inputId in node.Inputs)
            {
                if (depth.TryGetValue(inputId, out int id) && id + 1 > d)
                    d = id + 1;
            }
            depth[nodeId] = d;
        }

        for (int i = order.Count - 1; i >= 0; i--)
        {
            int nodeId = order[i];
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;
            int h = 0;
            foreach (int outputId in node.Outputs)
            {
                if (height.TryGetValue(outputId, out int oh) && oh + 1 > h)
                    h = oh + 1;
            }
            height[nodeId] = h;
        }

        int criticalLength = depth.Values.DefaultIfEmpty(0).Max() + height.Values.DefaultIfEmpty(0).Max();
        int threshold = (int)(criticalLength * 0.8);

        return depth.Keys
            .Where(id => depth.ContainsKey(id) && height.ContainsKey(id) && depth[id] + height[id] >= threshold)
            .OrderByDescending(id => depth[id] + height[id])
            .ToArray();
    }

    /// <summary>Finds independent subgraphs (maximal sets of nodes with no inter-dependencies).</summary>
    public IReadOnlyList<IReadOnlyList<int>> FindIndependentSubgraphs(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var adj = new Dictionary<int, HashSet<int>>();
        foreach (var node in graph.Nodes.Values)
        {
            adj[node.Id] = new HashSet<int>();
            foreach (int inputId in node.Inputs)
            {
                if (graph.Nodes.ContainsKey(inputId))
                {
                    adj[node.Id].Add(inputId);
                    if (!adj.ContainsKey(inputId))
                        adj[inputId] = [];
                    adj[inputId].Add(node.Id);
                }
            }
        }

        var visited = new HashSet<int>();
        var subgraphs = new List<IReadOnlyList<int>>();

        foreach (int nodeId in graph.Nodes.Keys)
        {
            if (visited.Contains(nodeId)) continue;
            var component = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(nodeId);
            visited.Add(nodeId);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                component.Add(current);

                if (adj.TryGetValue(current, out var neighbors))
                {
                    foreach (int neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            subgraphs.Add(component);
        }

        return subgraphs;
    }

    /// <summary>Gets all direct and transitive dependencies of a node.</summary>
    public IReadOnlyList<int> GetAllDependencies(ComputationGraph graph, int nodeId)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var result = new HashSet<int>();
        var queue = new Queue<int>();

        if (graph.TryGetNode(nodeId, out var node) && node is not null)
        {
            foreach (int inputId in node.Inputs)
                queue.Enqueue(inputId);
        }

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();
            if (!result.Add(id)) continue;

            if (graph.TryGetNode(id, out var n) && n is not null)
            {
                foreach (int inputId in n.Inputs)
                    queue.Enqueue(inputId);
            }
        }

        return result.ToArray();
    }

    /// <summary>Gets all nodes that depend on the given node (directly or transitively).</summary>
    public IReadOnlyList<int> GetAllDependents(ComputationGraph graph, int nodeId)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var result = new HashSet<int>();
        var queue = new Queue<int>();

        if (graph.TryGetNode(nodeId, out var node) && node is not null)
        {
            foreach (int outputId in node.Outputs)
                queue.Enqueue(outputId);
        }

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();
            if (!result.Add(id)) continue;

            if (graph.TryGetNode(id, out var n) && n is not null)
            {
                foreach (int outputId in n.Outputs)
                    queue.Enqueue(outputId);
            }
        }

        return result.ToArray();
    }
}
