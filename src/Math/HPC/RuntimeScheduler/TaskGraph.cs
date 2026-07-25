namespace MathVerse.Math.HPC.RuntimeScheduler;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using MathVerse.Math.HPC.Core;

public sealed record TaskNode(
    Guid Id,
    string Name,
    TaskKind Kind,
    ImmutableDictionary<string, object> Metadata,
    double EstimatedCycles,
    long EstimatedMemoryBytes,
    AffinityMask Affinity,
    Vector<int> LoopBounds,
    int VectorWidth,
    IReadOnlyList<Guid> InputBuffers,
    IReadOnlyList<Guid> OutputBuffers,
    KernelType KernelType
)
{
public static TaskNode CreateCompute(string name, double cycles, long memory, AffinityMask affinity, Vector<int> loopBounds, int vectorWidth, KernelType kernel, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, TaskKind.Compute, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, cycles, memory, affinity, loopBounds, vectorWidth, ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty, kernel);

    public static TaskNode CreateMemory(string name, long bytes, AffinityMask affinity, IReadOnlyList<Guid> inputs, IReadOnlyList<Guid> outputs, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, TaskKind.Memory, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, 0, bytes, affinity, Vector<int>.Zero, 0, inputs, outputs, KernelType.Memory);

    public static TaskNode CreateKernelLaunch(string name, KernelType kernel, Vector<int> bounds, int vectorWidth, AffinityMask affinity, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, TaskKind.KernelLaunch, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, 1000, 0, affinity, bounds, vectorWidth, ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty, kernel);

    public TaskNode WithMetadata(string key, object value) => this with { Metadata = Metadata.SetItem(key, value) };
    public TaskNode WithCycles(double cycles) => this with { EstimatedCycles = cycles };
    public TaskNode WithMemory(long bytes) => this with { EstimatedMemoryBytes = bytes };
    public TaskNode WithAffinity(AffinityMask affinity) => this with { Affinity = affinity };
    public TaskNode WithBuffers(IReadOnlyList<Guid> inputs, IReadOnlyList<Guid> outputs) => this with { InputBuffers = inputs, OutputBuffers = outputs };
}

public enum TaskKind
{
    Compute,
    Memory,
    KernelLaunch,
    Synchronization,
    Reduction,
    Scan,
    Sort,
    Custom
}

public enum KernelType
{
    Generic,
    GEMM,
    Convolution,
    Reduction,
    Scan,
    Sort,
    FFT,
    Memory,
    Custom
}

public sealed record TaskEdge(Guid From, Guid To, TaskDependencyKind Kind, double Weight = 1.0)
{
    public static TaskEdge Data(Guid from, Guid to, double weight = 1.0) => new(from, to, TaskDependencyKind.Data, weight);
    public static TaskEdge Control(Guid from, Guid to) => new(from, to, TaskDependencyKind.Control, 1.0);
    public static TaskEdge Anti(Guid from, Guid to) => new(from, to, TaskDependencyKind.Anti, 0.0);
    public static TaskEdge Output(Guid from, Guid to) => new(from, to, TaskDependencyKind.Output, 0.0);
    public static TaskEdge Reduction(Guid from, Guid to) => new(from, to, TaskDependencyKind.Reduction, 1.0);
}

public enum TaskDependencyKind
{
    Data,
    Control,
    Anti,
    Output,
    Reduction
}

public sealed class TaskGraph
{
    private readonly Dictionary<Guid, TaskNode> _nodes = new();
    private readonly List<TaskEdge> _edges = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _adjacency = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _reverseAdjacency = new();
    private readonly Dictionary<Guid, TaskExecutionState> _nodeStates = new();

    public IReadOnlyList<TaskNode> Nodes => _nodes.Values.ToImmutableList();
    public IReadOnlyList<TaskEdge> Edges => _edges.ToImmutableList();
    public int NodeCount => _nodes.Count;
    public int EdgeCount => _edges.Count;

    public TaskNode AddNode(TaskNode node)
    {
        _nodes[node.Id] = node;
        _adjacency[node.Id] = new HashSet<Guid>();
        _reverseAdjacency[node.Id] = new HashSet<Guid>();
        _nodeStates[node.Id] = TaskExecutionState.Pending;
        return node;
    }

    public TaskEdge AddEdge(TaskEdge edge)
    {
        if (!_nodes.ContainsKey(edge.From) || !_nodes.ContainsKey(edge.To))
            throw new ArgumentException("Edge references non-existent node");

        _edges.Add(edge);
        _adjacency[edge.From].Add(edge.To);
        _reverseAdjacency[edge.To].Add(edge.From);
        return edge;
    }

    public bool TryGetNode(Guid id, out TaskNode? node) => _nodes.TryGetValue(id, out node);
    public TaskExecutionState GetNodeState(Guid id) => _nodeStates.TryGetValue(id, out var s) ? s : TaskExecutionState.Pending;
    public void SetNodeState(Guid id, TaskExecutionState state) => _nodeStates[id] = state;

    public IReadOnlyList<TaskNode> GetReadyNodes(IReadOnlySet<Guid> completedNodes)
    {
        return _nodes.Values
            .Where(n => _nodeStates[n.Id] == TaskExecutionState.Pending)
            .Where(n => _reverseAdjacency[n.Id].All(pred => completedNodes.Contains(pred)))
            .ToImmutableList();
    }

    public IReadOnlyList<TaskNode> GetSuccessors(Guid nodeId)
    {
        if (!_adjacency.TryGetValue(nodeId, out var succ))
            return ImmutableList<TaskNode>.Empty;
        return succ.Select(id => _nodes[id]).ToImmutableList();
    }

    public IReadOnlyList<TaskNode> GetPredecessors(Guid nodeId)
    {
        if (!_reverseAdjacency.TryGetValue(nodeId, out var pred))
            return ImmutableList<TaskNode>.Empty;
        return pred.Select(id => _nodes[id]).ToImmutableList();
    }

    public IReadOnlyList<TaskNode> TopologicalSort()
    {
        var inDegree = new Dictionary<Guid, int>();
        foreach (var node in _nodes.Values)
        {
            inDegree[node.Id] = _reverseAdjacency[node.Id].Count;
        }

        var queue = new Queue<Guid>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
        var result = new List<TaskNode>();

        while (queue.Count > 0)
        {
            var nodeId = queue.Dequeue();
            result.Add(_nodes[nodeId]);

            foreach (var succId in _adjacency[nodeId])
            {
                inDegree[succId]--;
                if (inDegree[succId] == 0)
                    queue.Enqueue(succId);
            }
        }

        if (result.Count != _nodes.Count)
            throw new InvalidOperationException("Task graph contains cycles");

        return result.ToImmutableList();
    }

    public bool HasCycles()
    {
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();

        foreach (var node in _nodes.Values)
        {
            if (HasCycleDfs(node.Id, visited, recursionStack))
                return true;
        }
        return false;
    }

    private bool HasCycleDfs(Guid nodeId, HashSet<Guid> visited, HashSet<Guid> recursionStack)
    {
        visited.Add(nodeId);
        recursionStack.Add(nodeId);

        foreach (var succId in _adjacency[nodeId])
        {
            if (!visited.Contains(succId))
            {
                if (HasCycleDfs(succId, visited, recursionStack))
                    return true;
            }
            else if (recursionStack.Contains(succId))
            {
                return true;
            }
        }

        recursionStack.Remove(nodeId);
        return false;
    }

    public TaskGraphBuilder ToBuilder() => new(this);

    public sealed class TaskGraphBuilder
    {
        private readonly TaskGraph _graph;

        internal TaskGraphBuilder(TaskGraph graph) => _graph = graph;

        public TaskGraphBuilder AddNode(TaskNode node)
        {
            _graph.AddNode(node);
            return this;
        }

        public TaskGraphBuilder AddEdge(TaskEdge edge)
        {
            _graph.AddEdge(edge);
            return this;
        }

        public TaskGraphBuilder AddEdge(Guid from, Guid to, TaskDependencyKind kind, double weight = 1.0)
        {
            _graph.AddEdge(new TaskEdge(from, to, kind, weight));
            return this;
        }

        public TaskGraph Build() => _graph;
    }
}

public enum TaskExecutionState
{
    Pending,
    Ready,
    Running,
    Completed,
    Failed,
    Cancelled
}

public static class TaskGraphBuilder
{
    public static TaskGraph.TaskGraphBuilder Create() => new TaskGraph().ToBuilder();

    public static TaskGraph.TaskGraphBuilder CreateCompute(string name, double cycles, long memory, AffinityMask affinity, Vector<int> bounds, int vectorWidth, KernelType kernel, Action<TaskNode>? configure = null)
    {
        var node = TaskNode.CreateCompute(name, cycles, memory, affinity, bounds, vectorWidth, kernel);
        if (configure != null) configure(node);
        return Create().AddNode(node);
    }

    public static TaskGraph.TaskGraphBuilder CreateKernelLaunch(string name, KernelType kernel, Vector<int> bounds, int vectorWidth, AffinityMask affinity, Action<TaskNode>? configure = null)
    {
        var node = TaskNode.CreateKernelLaunch(name, kernel, bounds, vectorWidth, affinity);
        if (configure != null) configure(node);
        return Create().AddNode(node);
    }
}