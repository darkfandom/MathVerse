namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Creates execution plans that maximize parallelism while respecting data dependencies.
/// Produces an <see cref="ExecutionPlan"/> containing ordered batches of nodes that
/// can be executed in parallel within each batch.
/// </summary>
public sealed class ExecutionPlanner
{
    private readonly int _maxDegreeOfParallelism;

    /// <summary>
    /// Initializes the execution planner.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Maximum parallelism per batch.</param>
    public ExecutionPlanner(int maxDegreeOfParallelism = 0)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism > 0
            ? maxDegreeOfParallelism
            : Environment.ProcessorCount;
    }

    /// <summary>
    /// Creates an execution plan for the given computation graph.
    /// </summary>
    /// <param name="graph">The computation graph to plan.</param>
    /// <returns>An execution plan with ordered parallel batches.</returns>
    public ExecutionPlan Plan(ComputationGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var batches = ComputeBatches(graph);
        var criticalPath = ComputeCriticalPath(graph);

        return new ExecutionPlan
        {
            Batches = batches,
            CriticalPathLength = criticalPath,
            MaxParallelism = batches.Count > 0 ? batches.Max(b => b.Count) : 0,
            TotalNodes = graph.Nodes.Count,
            EstimatedBatches = batches.Count
        };
    }

    /// <summary>
    /// Creates an execution plan optimized for a target throughput.
    /// </summary>
    /// <param name="graph">The computation graph to plan.</param>
    /// <param name="targetLatencyMs">Target latency in milliseconds.</param>
    /// <returns>An execution plan optimized for the target latency.</returns>
    public ExecutionPlan PlanWithTarget(
        ComputationGraph graph,
        double targetLatencyMs)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var basePlan = Plan(graph);

        if (basePlan.CriticalPathLength <= targetLatencyMs)
            return basePlan;

        var batches = ComputeBatches(graph);
        var optimizedBatches = OptimizeForLatency(batches, graph);

        return new ExecutionPlan
        {
            Batches = optimizedBatches,
            CriticalPathLength = basePlan.CriticalPathLength,
            MaxParallelism = optimizedBatches.Count > 0 ? optimizedBatches.Max(b => b.Count) : 0,
            TotalNodes = graph.Nodes.Count,
            EstimatedBatches = optimizedBatches.Count
        };
    }

    /// <summary>
    /// Validates that an execution plan is correct for the given graph.
    /// </summary>
    /// <param name="graph">The computation graph.</param>
    /// <param name="plan">The execution plan to validate.</param>
    /// <returns>True if the plan is valid.</returns>
    public bool ValidatePlan(ComputationGraph graph, ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(plan);

        var allNodeIds = new HashSet<string>(graph.Nodes.Select(n => n.Id));
        var plannedNodes = new HashSet<string>();

        foreach (var batch in plan.Batches)
        {
            foreach (var nodeId in batch.NodeIds)
            {
                if (!allNodeIds.Contains(nodeId))
                    return false;

                if (!plannedNodes.Add(nodeId))
                    return false;

                var node = graph.GetNode(nodeId);
                if (node == null)
                    return false;

                foreach (var dep in node.Dependencies)
                {
                    if (allNodeIds.Contains(dep) && !plannedNodes.Contains(dep))
                        return false;
                }
            }
        }

        return plannedNodes.Count == allNodeIds.Count;
    }

    private static List<ExecutionBatch> ComputeBatches(ComputationGraph graph)
    {
        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        var inDegree = new Dictionary<string, int>();
        foreach (var node in graph.Nodes)
            inDegree[node.Id] = node.Dependencies.Count(d => nodeMap.ContainsKey(d));

        var batches = new List<ExecutionBatch>();
        var completed = new HashSet<string>();

        var queue = new Queue<string>();
        foreach (var (id, deg) in inDegree)
        {
            if (deg == 0)
                queue.Enqueue(id);
        }

        while (queue.Count > 0)
        {
            var batch = new ExecutionBatch { BatchIndex = batches.Count };
            var nextQueue = new Queue<string>();

            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (completed.Contains(id))
                    continue;

                batch.NodeIds.Add(id);
                completed.Add(id);

                foreach (var node in graph.Nodes)
                {
                    if (node.Dependencies.Contains(id))
                    {
                        inDegree[node.Id]--;
                        if (inDegree[node.Id] == 0)
                            nextQueue.Enqueue(node.Id);
                    }
                }
            }

            if (batch.NodeIds.Count > 0)
                batches.Add(batch);

            while (nextQueue.Count > 0)
                queue.Enqueue(nextQueue.Dequeue());
        }

        return batches;
    }

    private static int ComputeCriticalPath(ComputationGraph graph)
    {
        if (graph.Nodes.Count == 0)
            return 0;

        var longestPath = new Dictionary<string, int>();
        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        var sorted = TopologicalSort(graph);

        foreach (var nodeId in sorted)
        {
            if (!nodeMap.TryGetValue(nodeId, out var node))
                continue;

            longestPath[nodeId] = 1;

            foreach (var dep in node.Dependencies)
            {
                if (longestPath.TryGetValue(dep, out var depPath))
                {
                    longestPath[nodeId] = Math.Max(longestPath[nodeId], depPath + 1);
                }
            }
        }

        return longestPath.Values.DefaultIfEmpty(0).Max();
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

        var result = new List<string>();
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            result.Add(id);

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

        return result;
    }

    private static List<ExecutionBatch> OptimizeForLatency(
        List<ExecutionBatch> batches,
        ComputationGraph graph)
    {
        var optimized = new List<ExecutionBatch>();

        foreach (var batch in batches)
        {
            if (batch.NodeIds.Count <= 1)
            {
                optimized.Add(batch);
                continue;
            }

            var subBatches = SplitLargeBatch(batch, graph);
            optimized.AddRange(subBatches);
        }

        return optimized;
    }

    private static List<ExecutionBatch> SplitLargeBatch(
        ExecutionBatch batch,
        ComputationGraph graph)
    {
        if (batch.NodeIds.Count <= Environment.ProcessorCount)
            return new List<ExecutionBatch> { batch };

        var subBatches = new List<ExecutionBatch>();
        var chunkSize = Environment.ProcessorCount;

        for (var i = 0; i < batch.NodeIds.Count; i += chunkSize)
        {
            var chunk = batch.NodeIds.Skip(i).Take(chunkSize).ToList();
            subBatches.Add(new ExecutionBatch
            {
                BatchIndex = batch.BatchIndex,
                NodeIds = chunk
            });
        }

        return subBatches;
    }
}

/// <summary>
/// Represents a batch of computation graph nodes that can be executed in parallel.
/// </summary>
public sealed class ExecutionBatch
{
    /// <summary>The zero-based index of this batch in the execution plan.</summary>
    public int BatchIndex { get; set; }

    /// <summary>The IDs of nodes in this batch.</summary>
    public List<string> NodeIds { get; set; } = new();

    /// <summary>Number of nodes in this batch.</summary>
    public int Count => NodeIds.Count;
}

/// <summary>
/// Represents a complete execution plan for a computation graph.
/// </summary>
public sealed class ExecutionPlan
{
    /// <summary>Ordered list of parallel execution batches.</summary>
    public List<ExecutionBatch> Batches { get; set; } = new();

    /// <summary>Length of the critical path in the graph.</summary>
    public int CriticalPathLength { get; set; }

    /// <summary>Maximum number of nodes that can execute in parallel.</summary>
    public int MaxParallelism { get; set; }

    /// <summary>Total number of nodes in the graph.</summary>
    public int TotalNodes { get; set; }

    /// <summary>Estimated number of batches needed.</summary>
    public int EstimatedBatches { get; set; }

    /// <summary>
    /// Returns a string summary of the execution plan.
    /// </summary>
    public override string ToString()
    {
        return $"ExecutionPlan: {TotalNodes} nodes, {Batches.Count} batches, " +
               $"max parallelism={MaxParallelism}, critical path={CriticalPathLength}";
    }
}
