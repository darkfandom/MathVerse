namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Represents a level of execution where all nodes can run in parallel.</summary>
/// <param name="Level">The level index (0-based).</param>
/// <param name="NodeIds">IDs of nodes that can execute concurrently at this level.</param>
public sealed record ExecutionLevel(int Level, IReadOnlyList<int> NodeIds);

/// <summary>The execution schedule produced by the graph scheduler.</summary>
/// <param name="Levels">Ordered execution levels.</param>
/// <param name="CriticalPathLength">Length of the critical path.</param>
public sealed record ExecutionSchedule(IReadOnlyList<ExecutionLevel> Levels, int CriticalPathLength);

/// <summary>Schedules graph execution using topological sort, critical path, or level-based parallel strategies.</summary>
public sealed class GraphScheduler
{
    private readonly Dictionary<int, int> _depthCache = new();
    private readonly Dictionary<int, int> _longestPathCache = new();

    /// <summary>Schedules the graph using level-based parallel scheduling.</summary>
    public ExecutionSchedule Schedule(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        return ScheduleLevelBased(graph);
    }

    /// <summary>Schedules using simple topological ordering (each node in its own level).</summary>
    public ExecutionSchedule ScheduleTopological(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetTopologicalOrder();
        var levels = new List<ExecutionLevel>(order.Count);

        for (int i = 0; i < order.Count; i++)
            levels.Add(new ExecutionLevel(i, [order[i]]));

        int criticalPath = ComputeCriticalPath(graph);
        return new ExecutionSchedule(levels, criticalPath);
    }

    /// <summary>Schedules using critical-path-aware ordering.</summary>
    public ExecutionSchedule ScheduleCriticalPath(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var criticalPath = ComputeCriticalPath(graph);
        var depthMap = ComputeDepths(graph);
        var maxDepth = depthMap.Values.DefaultIfEmpty(0).Max();

        var levels = new List<ExecutionLevel>();
        for (int d = 0; d <= maxDepth; d++)
        {
            var levelNodes = depthMap.Where(kv => kv.Value == d).Select(kv => kv.Key).ToArray();
            if (levelNodes.Length > 0)
                levels.Add(new ExecutionLevel(d, levelNodes));
        }

        return new ExecutionSchedule(levels, criticalPath);
    }

    /// <summary>Schedules with level-based parallelism: nodes at the same depth execute together.</summary>
    public ExecutionSchedule ScheduleLevelBased(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var depthMap = ComputeDepths(graph);
        if (depthMap.Count == 0)
            return new ExecutionSchedule([], 0);

        int maxDepth = depthMap.Values.Max();
        var levels = new List<ExecutionLevel>(maxDepth + 1);

        for (int d = 0; d <= maxDepth; d++)
        {
            var levelNodes = depthMap.Where(kv => kv.Value == d).Select(kv => kv.Key).ToArray();
            if (levelNodes.Length > 0)
                levels.Add(new ExecutionLevel(d, levelNodes));
        }

        int criticalPath = ComputeCriticalPath(graph);
        return new ExecutionSchedule(levels, criticalPath);
    }

    /// <summary>Computes the length of the critical path (longest path through the graph).</summary>
    public int ComputeCriticalPath(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        _longestPathCache.Clear();

        int maxPath = 0;
        foreach (var node in graph.Nodes.Values)
        {
            int path = ComputeLongestPath(graph, node.Id);
            if (path > maxPath) maxPath = path;
        }

        return maxPath;
    }

    /// <summary>Computes the depth of each node (longest distance from any root).</summary>
    public Dictionary<int, int> ComputeDepths(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var depths = new Dictionary<int, int>();
        foreach (int nodeId in graph.GetTopologicalOrder())
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;

            int depth = 0;
            foreach (int inputId in node.Inputs)
            {
                if (depths.TryGetValue(inputId, out int inputDepth) && inputDepth + 1 > depth)
                    depth = inputDepth + 1;
            }
            depths[nodeId] = depth;
        }

        return depths;
    }

    private int ComputeLongestPath(ComputationGraph graph, int nodeId)
    {
        if (_longestPathCache.TryGetValue(nodeId, out int cached))
            return cached;

        if (!graph.TryGetNode(nodeId, out var node) || node is null)
            return 0;

        if (node.Outputs.Count == 0)
        {
            _longestPathCache[nodeId] = 0;
            return 0;
        }

        int maxChild = 0;
        foreach (int outputId in node.Outputs)
        {
            int childPath = ComputeLongestPath(graph, outputId);
            if (childPath > maxChild) maxChild = childPath;
        }

        int result = maxChild + 1;
        _longestPathCache[nodeId] = result;
        return result;
    }
}
