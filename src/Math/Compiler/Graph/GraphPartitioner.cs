namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Represents a partition of the graph for parallel or GPU execution.</summary>
/// <param name="PartitionId">Unique partition identifier.</param>
/// <param name="NodeIds">IDs of nodes in this partition.</param>
/// <param name="CrossPartitionEdges">Edges that cross partition boundaries.</param>
/// <param name="IsGpuEligible">Whether this partition can be offloaded to a GPU.</param>
public sealed record GraphPartition(
    int PartitionId,
    IReadOnlyList<int> NodeIds,
    IReadOnlyList<GraphEdge> CrossPartitionEdges,
    bool IsGpuEligible);

/// <summary>Partitions a computation graph into subgraphs for parallel execution or GPU offloading.</summary>
public sealed class GraphPartitioner
{
    /// <summary>Partitions the graph into balanced subgraphs using a level-based strategy.</summary>
    public IReadOnlyList<GraphPartition> Partition(ComputationGraph graph, int maxPartitions)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (maxPartitions < 1) throw new ArgumentOutOfRangeException(nameof(maxPartitions));

        if (graph.NodeCount <= maxPartitions)
        {
            var singlePartitions = new List<GraphPartition>();
            int pid = 0;
            foreach (var node in graph.Nodes.Values)
            {
                singlePartitions.Add(new GraphPartition(pid++, [node.Id], [], true));
            }
            return singlePartitions;
        }

        return PartitionByLevels(graph, maxPartitions);
    }

    /// <summary>Partitions using level-based grouping: consecutive topological levels are grouped.</summary>
    public IReadOnlyList<GraphPartition> PartitionByLevels(ComputationGraph graph, int maxPartitions)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (maxPartitions < 1) throw new ArgumentOutOfRangeException(nameof(maxPartitions));

        var scheduler = new GraphScheduler();
        var schedule = scheduler.ScheduleLevelBased(graph);

        if (schedule.Levels.Count == 0) return [];

        int partitionsPerLevel = Math.Max(1, schedule.Levels.Count / maxPartitions);
        var partitions = new List<GraphPartition>();
        int partitionId = 0;

        for (int i = 0; i < schedule.Levels.Count; i += partitionsPerLevel)
        {
            var nodeIds = new List<int>();
            for (int j = i; j < Math.Min(i + partitionsPerLevel, schedule.Levels.Count); j++)
            {
                nodeIds.AddRange(schedule.Levels[j].NodeIds);
            }
            partitions.Add(new GraphPartition(partitionId++, nodeIds, [], true));
        }

        AddCrossPartitionEdges(graph, partitions);
        return partitions;
    }

    /// <summary>Partitions by splitting the graph at bottleneck edges (high fan-out or fan-in).</summary>
    public IReadOnlyList<GraphPartition> PartitionByBottlenecks(ComputationGraph graph, int maxPartitions)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (maxPartitions < 1) throw new ArgumentOutOfRangeException(nameof(maxPartitions));

        var analyzer = new DependencyAnalyzer();
        var bottlenecks = analyzer.FindBottlenecks(graph);
        var cutEdges = new HashSet<(int, int)>();

        foreach (int bottleneckId in bottlenecks.Take(maxPartitions - 1))
        {
            if (!graph.TryGetNode(bottleneckId, out var node) || node is null) continue;
            foreach (int outputId in node.Outputs)
                cutEdges.Add((bottleneckId, outputId));
        }

        var visited = new HashSet<int>();
        var partitions = new List<GraphPartition>();
        int partitionId = 0;

        foreach (int nodeId in graph.GetTopologicalOrder())
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

                if (!graph.TryGetNode(current, out var n) || n is null) continue;
                foreach (int outputId in n.Outputs)
                {
                    if (!visited.Contains(outputId) && !cutEdges.Contains((current, outputId)))
                    {
                        visited.Add(outputId);
                        queue.Enqueue(outputId);
                    }
                }
            }

            if (component.Count > 0)
                partitions.Add(new GraphPartition(partitionId++, component, [], true));

            if (partitions.Count >= maxPartitions) break;
        }

        AddCrossPartitionEdges(graph, partitions);
        return partitions;
    }

    /// <summary>Partitions using recursive bisection to minimize cross-partition communication.</summary>
    public IReadOnlyList<GraphPartition> PartitionBisection(ComputationGraph graph, int maxPartitions)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (maxPartitions < 1) throw new ArgumentOutOfRangeException(nameof(maxPartitions));

        var order = graph.GetTopologicalOrder();
        if (order.Count == 0) return [];

        var partitions = new List<GraphPartition>();
        int nodesPerPartition = (order.Count + maxPartitions - 1) / maxPartitions;

        for (int i = 0; i < order.Count; i += nodesPerPartition)
        {
            var chunk = order.Skip(i).Take(nodesPerPartition).ToArray();
            partitions.Add(new GraphPartition(partitions.Count, chunk, [], true));
        }

        AddCrossPartitionEdges(graph, partitions);
        return partitions;
    }

    /// <summary>Determines which partitions are GPU-eligible based on operation types.</summary>
    public IReadOnlyList<GraphPartition> MarkGpuEligible(ComputationGraph graph, IReadOnlyList<GraphPartition> partitions)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (partitions is null) throw new ArgumentNullException(nameof(partitions));

        var gpuOps = new HashSet<GraphOperation>
        {
            GraphOperation.MatMul, GraphOperation.Conv, GraphOperation.Relu,
            GraphOperation.Softmax, GraphOperation.Sigmoid, GraphOperation.Tanh,
            GraphOperation.Add, GraphOperation.Mul, GraphOperation.Sub, GraphOperation.Div,
        };

        var result = new List<GraphPartition>(partitions.Count);
        foreach (var partition in partitions)
        {
            bool allGpuEligible = partition.NodeIds.All(id =>
                graph.TryGetNode(id, out var node) && node is not null && gpuOps.Contains(node.Operation));
            result.Add(partition with { IsGpuEligible = allGpuEligible });
        }

        return result;
    }

    private static void AddCrossPartitionEdges(ComputationGraph graph, List<GraphPartition> partitions)
    {
        var nodeToPartition = new Dictionary<int, int>();
        for (int i = 0; i < partitions.Count; i++)
        {
            foreach (int nodeId in partitions[i].NodeIds)
                nodeToPartition[nodeId] = i;
        }

        for (int i = 0; i < partitions.Count; i++)
        {
            var crossEdges = new List<GraphEdge>();
            foreach (int nodeId in partitions[i].NodeIds)
            {
                if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;
                foreach (int outputId in node.Outputs)
                {
                    if (nodeToPartition.TryGetValue(outputId, out int targetPartition) && targetPartition != i)
                    {
                        crossEdges.Add(new GraphEdge(nodeId, outputId));
                    }
                }
            }
            partitions[i] = partitions[i] with { CrossPartitionEdges = crossEdges };
        }
    }
}
