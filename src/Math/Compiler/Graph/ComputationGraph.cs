namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>A directed acyclic graph (DAG) of computation nodes and edges, thread-safe for concurrent modifications.</summary>
public sealed class ComputationGraph
{
    private readonly ConcurrentDictionary<int, GraphNode> _nodes = new();
    private readonly ConcurrentBag<GraphEdge> _edges = new();
    private readonly List<GraphEdge> _edgeList = [];
    private int _nextNodeId;
    private readonly object _edgeLock = new();

    /// <summary>All nodes in the graph.</summary>
    public IReadOnlyDictionary<int, GraphNode> Nodes => _nodes;

    /// <summary>All edges in the graph.</summary>
    public IReadOnlyList<GraphEdge> Edges
    {
        get
        {
            lock (_edgeLock) return [.. _edgeList];
        }
    }

    /// <summary>The number of nodes.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>The number of edges.</summary>
    public int EdgeCount
    {
        get { lock (_edgeLock) return _edgeList.Count; }
    }

    /// <summary>Gets the next unique node ID.</summary>
    public int NextNodeId() => Interlocked.Increment(ref _nextNodeId);

    /// <summary>Adds a node and returns its ID.</summary>
    public int AddNode(GraphOperation operation, IReadOnlyList<int>? inputs = null, IReadOnlyDictionary<string, object>? metadata = null)
    {
        int id = NextNodeId();
        var node = new GraphNode(id, operation, inputs ?? [], [], metadata);
        _nodes[id] = node;
        return id;
    }

    /// <summary>Adds a pre-built node to the graph.</summary>
    public void AddNode(GraphNode node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        _nodes[node.Id] = node;
    }

    /// <summary>Adds a directed edge between two nodes and updates their input/output lists.</summary>
    public void AddEdge(GraphEdge edge)
    {
        if (edge is null) throw new ArgumentNullException(nameof(edge));
        if (!_nodes.ContainsKey(edge.From))
            throw new ArgumentException($"Source node {edge.From} does not exist.", nameof(edge));
        if (!_nodes.ContainsKey(edge.To))
            throw new ArgumentException($"Destination node {edge.To} does not exist.", nameof(edge));

        lock (_edgeLock)
        {
            _edgeList.Add(edge);
        }

        _nodes.AddOrUpdate(edge.From,
            _ => throw new InvalidOperationException(),
            (_, existing) => existing.WithAddedOutput(edge.To));

        _nodes.AddOrUpdate(edge.To,
            _ => throw new InvalidOperationException(),
            (_, existing) => existing.WithAddedInput(edge.From));
    }

    /// <summary>Removes a node and all its connected edges from the graph.</summary>
    public bool RemoveNode(int nodeId)
    {
        if (!_nodes.TryRemove(nodeId, out _)) return false;

        lock (_edgeLock)
        {
            _edgeList.RemoveAll(e => e.From == nodeId || e.To == nodeId);
        }

        foreach (var kv in _nodes)
        {
            if (kv.Value.Inputs.Contains(nodeId) || kv.Value.Outputs.Contains(nodeId))
            {
                var updated = new GraphNode(
                    kv.Value.Id,
                    kv.Value.Operation,
                    kv.Value.Inputs.Where(id => id != nodeId).ToArray(),
                    kv.Value.Outputs.Where(id => id != nodeId).ToArray(),
                    kv.Value.Metadata);
                _nodes[kv.Key] = updated;
            }
        }

        return true;
    }

    /// <summary>Gets a node by ID.</summary>
    public bool TryGetNode(int nodeId, out GraphNode? node) =>
        _nodes.TryGetValue(nodeId, out node);

    /// <summary>Gets all input nodes (no incoming edges).</summary>
    public IReadOnlyList<GraphNode> GetInputNodes() =>
        _nodes.Values.Where(n => n.IsInput || n.Inputs.Count == 0).ToArray();

    /// <summary>Gets all output nodes (no outgoing edges).</summary>
    public IReadOnlyList<GraphNode> GetOutputNodes() =>
        _nodes.Values.Where(n => n.IsOutput || n.Outputs.Count == 0).ToArray();

    /// <summary>Computes a topological ordering of all nodes using Kahn's algorithm.</summary>
    public IReadOnlyList<int> GetTopologicalOrder()
    {
        var inDegree = new Dictionary<int, int>();
        var adj = new Dictionary<int, List<int>>();

        foreach (var kv in _nodes)
        {
            inDegree[kv.Key] = kv.Value.Inputs.Count(id => _nodes.ContainsKey(id));
            if (!adj.ContainsKey(kv.Key))
                adj[kv.Key] = [];
        }

        var queue = new Queue<int>();
        foreach (var kv in inDegree)
        {
            if (kv.Value == 0)
                queue.Enqueue(kv.Key);
        }

        var result = new List<int>(_nodes.Count);
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            result.Add(current);

            if (!adj.TryGetValue(current, out var neighbors))
            {
                if (_nodes.TryGetValue(current, out var node))
                {
                    foreach (int outId in node.Outputs)
                    {
                        if (!_nodes.ContainsKey(outId)) continue;
                        inDegree[outId]--;
                        if (inDegree[outId] == 0)
                            queue.Enqueue(outId);
                    }
                }
            }
            else
            {
                foreach (int neighbor in neighbors)
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }
        }

        if (result.Count != _nodes.Count)
            throw new InvalidOperationException("Graph contains a cycle.");

        return result;
    }

    /// <summary>Checks if the graph has a cycle using DFS.</summary>
    public bool HasCycle()
    {
        var visited = new HashSet<int>();
        var inStack = new HashSet<int>();

        bool Dfs(int nodeId)
        {
            if (inStack.Contains(nodeId)) return true;
            if (visited.Contains(nodeId)) return false;

            visited.Add(nodeId);
            inStack.Add(nodeId);

            if (_nodes.TryGetValue(nodeId, out var node))
            {
                foreach (int outputId in node.Outputs)
                {
                    if (_nodes.ContainsKey(outputId) && Dfs(outputId))
                        return true;
                }
            }

            inStack.Remove(nodeId);
            return false;
        }

        foreach (int id in _nodes.Keys)
        {
            if (Dfs(id)) return true;
        }

        return false;
    }

    /// <summary>Executes the graph by evaluating nodes in topological order.</summary>
    public IReadOnlyDictionary<int, double[]> Execute(IReadOnlyDictionary<int, double[]> inputs)
    {
        var order = GetTopologicalOrder();
        var results = new Dictionary<int, double[]>(_nodes.Count);

        foreach (int nodeId in order)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) continue;

            if (node.IsInput || node.Operation == GraphOperation.Input)
            {
                if (inputs.TryGetValue(nodeId, out var inputVal))
                    results[nodeId] = inputVal;
                continue;
            }

            var inputValues = new List<double[]>();
            foreach (int inputId in node.Inputs)
            {
                if (results.TryGetValue(inputId, out var val))
                    inputValues.Add(val);
            }

            results[nodeId] = EvaluateNode(node.Operation, inputValues);
        }

        return results;
    }

    /// <summary>Cleans up edges that reference non-existent nodes.</summary>
    public int CleanOrphanedEdges()
    {
        lock (_edgeLock)
        {
            int removed = _edgeList.RemoveAll(e => !_nodes.ContainsKey(e.From) || !_nodes.ContainsKey(e.To));
            return removed;
        }
    }

    private static double[] EvaluateNode(GraphOperation op, List<double[]> inputs)
    {
        if (inputs.Count == 0)
            return [];

        var a = inputs.Count > 0 ? inputs[0] : [];
        var b = inputs.Count > 1 ? inputs[1] : [];

        return op switch
        {
            GraphOperation.Add => ElementWise(a, b, (x, y) => x + y),
            GraphOperation.Sub => ElementWise(a, b, (x, y) => x - y),
            GraphOperation.Mul => ElementWise(a, b, (x, y) => x * y),
            GraphOperation.Div => ElementWise(a, b, (x, y) => x / y),
            GraphOperation.Neg => ElementWise(a, x => -x),
            GraphOperation.Exp => ElementWise(a, Math.Exp),
            GraphOperation.Log => ElementWise(a, Math.Log),
            GraphOperation.Sqrt => ElementWise(a, Math.Sqrt),
            GraphOperation.Abs => ElementWise(a, Math.Abs),
            GraphOperation.Relu => ElementWise(a, x => Math.Max(0, x)),
            GraphOperation.Sigmoid => ElementWise(a, x => 1.0 / (1.0 + Math.Exp(-x))),
            GraphOperation.Tanh => ElementWise(a, Math.Tanh),
            GraphOperation.Pow when b.Length > 0 => ElementWise(a, b, Math.Pow),
            GraphOperation.MatMul => MatMul(a, b),
            GraphOperation.Sum => [a.Sum()],
            GraphOperation.Mean => [a.Length > 0 ? a.Average() : 0.0],
            GraphOperation.Max => [a.Length > 0 ? a.Max() : 0.0],
            GraphOperation.Reshape => a,
            GraphOperation.Transpose => a,
            _ => a.Length > 0 ? [.. a] : [],
        };
    }

    private static double[] ElementWise(double[] a, Func<double, double> fn)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = fn(a[i]);
        return result;
    }

    private static double[] ElementWise(double[] a, double[] b, Func<double, double, double> fn)
    {
        int len = Math.Max(a.Length, b.Length);
        var result = new double[len];
        for (int i = 0; i < len; i++)
            result[i] = fn(
                i < a.Length ? a[i] : 0,
                i < b.Length ? b[i] : 0);
        return result;
    }

    private static double[] MatMul(double[] a, double[] b)
    {
        if (a.Length == 0 || b.Length == 0) return [];
        double sum = 0;
        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
            sum += a[i] * b[i];
        return [sum];
    }

    /// <summary>
    /// Creates a ComputationGraph from an IR module by extracting operations.
    /// </summary>
    public static ComputationGraph FromIR(MathVerse.Math.Compiler.IR.IRModule module)
    {
        var graph = new ComputationGraph();
        foreach (var func in module.Functions)
        {
            foreach (var block in func.Blocks)
            {
                foreach (var inst in block.Instructions)
                {
                    if (inst.Result == null) continue;
                    var op = inst.OpCode switch
                    {
                        MathVerse.Math.Compiler.IR.IROpCode.Add => GraphOperation.Add,
                        MathVerse.Math.Compiler.IR.IROpCode.Sub => GraphOperation.Sub,
                        MathVerse.Math.Compiler.IR.IROpCode.Mul => GraphOperation.Mul,
                        MathVerse.Math.Compiler.IR.IROpCode.Div => GraphOperation.Div,
                        MathVerse.Math.Compiler.IR.IROpCode.Neg => GraphOperation.Neg,
                        MathVerse.Math.Compiler.IR.IROpCode.Exp => GraphOperation.Exp,
                        MathVerse.Math.Compiler.IR.IROpCode.Log => GraphOperation.Log,
                        MathVerse.Math.Compiler.IR.IROpCode.Sqrt => GraphOperation.Sqrt,
                        MathVerse.Math.Compiler.IR.IROpCode.Abs => GraphOperation.Abs,
                        _ => GraphOperation.Add
                    };
                    graph.AddNode(op);
                }
            }
        }
        return graph;
    }
}
