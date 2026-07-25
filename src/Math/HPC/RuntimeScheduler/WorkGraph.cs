namespace MathVerse.Math.HPC.RuntimeScheduler;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using MathVerse.Math.HPC.Core;

public sealed record WorkNode(
    Guid Id,
    string Name,
    WorkKind Kind,
    ImmutableDictionary<string, object> Metadata,
    double EstimatedCost,
    long EstimatedMemory,
    AffinityMask Affinity
)
{
    public static WorkNode CreateCompute(string name, double estimatedCost, long estimatedMemory, AffinityMask affinity = default, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, WorkKind.Compute, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, estimatedCost, estimatedMemory, affinity);

    public static WorkNode CreateMemory(string name, long estimatedMemory, AffinityMask affinity = default, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, WorkKind.Memory, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, 0, estimatedMemory, affinity);

    public static WorkNode CreateSynchronization(string name, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, WorkKind.Synchronization, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, 0, 0, AffinityMask.Any);

    public static WorkNode CreateDataTransfer(string name, long bytes, AffinityMask from, AffinityMask to, IReadOnlyDictionary<string, object>? metadata = null)
        => new(Guid.NewGuid(), name, WorkKind.DataTransfer, metadata?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, 0, bytes, from | to);

    public WorkNode WithMetadata(string key, object value)
        => this with { Metadata = Metadata.SetItem(key, value) };

    public WorkNode WithAffinity(AffinityMask affinity)
        => this with { Affinity = affinity };

    public WorkNode WithCost(double cost)
        => this with { EstimatedCost = cost };
}

public enum WorkKind
{
    Compute,
    Memory,
    Synchronization,
    DataTransfer,
    KernelLaunch,
    SynchronizationBarrier,
    MemoryFence
}

[Flags]
public enum AffinityMask : ulong
{
    Any = 0xFFFFFFFFFFFFFFFF,
    CPU = 0x1,
    GPU0 = 0x2,
    GPU1 = 0x4,
    GPU2 = 0x8,
    GPU3 = 0x10,
    GPU = GPU0 | GPU1 | GPU2 | GPU3,
    NUMA0 = 0x100,
    NUMA1 = 0x200,
    NUMA2 = 0x400,
    NUMA3 = 0x800,
    NUMA = NUMA0 | NUMA1 | NUMA2 | NUMA3,
    Accelerator = 0x1000,
    Network = 0x2000
}

public sealed record WorkEdge(Guid From, Guid To, DependencyKind Kind, double Weight = 1.0)
{
    public static WorkEdge Data(Guid from, Guid to, double weight = 1.0) => new(from, to, DependencyKind.Data, weight);
    public static WorkEdge Control(Guid from, Guid to) => new(from, to, DependencyKind.Control, 1.0);
    public static WorkEdge Anti(Guid from, Guid to) => new(from, to, DependencyKind.Anti, 0.0);
    public static WorkEdge Output(Guid from, Guid to) => new(from, to, DependencyKind.Output, 0.0);
}

public enum DependencyKind
{
    Data,
    Control,
    Anti,
    Output
}

public sealed class WorkGraph
{
    private readonly Dictionary<Guid, WorkNode> _nodes = new();
    private readonly List<WorkEdge> _edges = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _adjacency = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _reverseAdjacency = new();

    public IReadOnlyList<WorkNode> Nodes => _nodes.Values.ToImmutableList();
    public IReadOnlyList<WorkEdge> Edges => _edges.ToImmutableList();
    public int NodeCount => _nodes.Count;
    public int EdgeCount => _edges.Count;

    public WorkNode AddNode(WorkNode node)
    {
        _nodes[node.Id] = node;
        _adjacency[node.Id] = new HashSet<Guid>();
        _reverseAdjacency[node.Id] = new HashSet<Guid>();
        return node;
    }

    public WorkEdge AddEdge(WorkEdge edge)
    {
        if (!_nodes.ContainsKey(edge.From) || !_nodes.ContainsKey(edge.To))
            throw new ArgumentException("Edge references non-existent node");

        _edges.Add(edge);
        _adjacency[edge.From].Add(edge.To);
        _reverseAdjacency[edge.To].Add(edge.From);
        return edge;
    }

    public bool TryGetNode(Guid id, out WorkNode? node) => _nodes.TryGetValue(id, out node);

    public IReadOnlyList<WorkNode> GetRoots()
    {
        return _nodes.Values
            .Where(n => _reverseAdjacency[n.Id].Count == 0)
            .ToImmutableList();
    }

    public IReadOnlyList<WorkNode> GetLeaves()
    {
        return _nodes.Values
            .Where(n => _adjacency[n.Id].Count == 0)
            .ToImmutableList();
    }

    public IReadOnlyList<WorkNode> GetSuccessors(Guid nodeId)
    {
        if (!_adjacency.TryGetValue(nodeId, out var succ))
            return ImmutableList<WorkNode>.Empty;
        return succ.Select(id => _nodes[id]).ToImmutableList();
    }

    public IReadOnlyList<WorkNode> GetPredecessors(Guid nodeId)
    {
        if (!_reverseAdjacency.TryGetValue(nodeId, out var pred))
            return ImmutableList<WorkNode>.Empty;
        return pred.Select(id => _nodes[id]).ToImmutableList();
    }

    public IReadOnlyList<WorkEdge> GetOutgoingEdges(Guid nodeId)
    {
        return _edges.Where(e => e.From == nodeId).ToImmutableList();
    }

    public IReadOnlyList<WorkEdge> GetIncomingEdges(Guid nodeId)
    {
        return _edges.Where(e => e.To == nodeId).ToImmutableList();
    }

    public IReadOnlyList<WorkNode> TopologicalSort()
    {
        var inDegree = new Dictionary<Guid, int>();
        foreach (var node in _nodes.Values)
        {
            inDegree[node.Id] = _reverseAdjacency[node.Id].Count;
        }

        var queue = new Queue<Guid>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
        var result = new List<WorkNode>();

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
            throw new InvalidOperationException("Graph contains cycles");

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

    public WorkGraphBuilder ToBuilder() => new(this);

    public sealed class WorkGraphBuilder
    {
        private readonly WorkGraph _graph;

        internal WorkGraphBuilder(WorkGraph graph) => _graph = graph;

        public WorkGraphBuilder AddNode(WorkNode node)
        {
            _graph.AddNode(node);
            return this;
        }

        public WorkGraphBuilder AddEdge(WorkEdge edge)
        {
            _graph.AddEdge(edge);
            return this;
        }

        public WorkGraphBuilder AddEdge(Guid from, Guid to, DependencyKind kind, double weight = 1.0)
        {
            _graph.AddEdge(new WorkEdge(from, to, kind, weight));
            return this;
        }

        public WorkGraph Build() => _graph;
    }
}

public static class WorkGraphBuilder
{
    public static WorkGraph.WorkGraphBuilder Create() => new WorkGraph().ToBuilder();

    public static WorkGraph.WorkGraphBuilder CreateCompute(string name, double cost, long memory, AffinityMask affinity = default, Action<WorkNode>? configure = null)
    {
        var node = WorkNode.CreateCompute(name, cost, memory, affinity);
        if (configure != null) configure(node);
        return Create().AddNode(node);
    }

    public static WorkGraph.WorkGraphBuilder AddEdge(this WorkGraph.WorkGraphBuilder builder, Guid from, Guid to, DependencyKind kind, double weight = 1.0)
        => builder.AddEdge(new WorkEdge(from, to, kind, weight));
}