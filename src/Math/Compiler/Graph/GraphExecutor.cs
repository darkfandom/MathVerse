namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Executes a computation graph, supporting sequential, parallel, and incremental execution.</summary>
public sealed class GraphExecutor
{
    private readonly GraphScheduler _scheduler;
    private readonly ConcurrentDictionary<int, double[]> _cachedResults = new();
    private readonly HashSet<int> _changedNodes = new();
    private readonly object _changeLock = new();

    /// <summary>Initializes a new graph executor with a default scheduler.</summary>
    public GraphExecutor() : this(new GraphScheduler()) { }

    /// <summary>Initializes a new graph executor with the specified scheduler.</summary>
    public GraphExecutor(GraphScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    /// <summary>Executes the full graph and returns outputs for all nodes.</summary>
    public IReadOnlyDictionary<int, double[]> Execute(ComputationGraph graph, IReadOnlyDictionary<int, double[]> inputs)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        var order = graph.GetTopologicalOrder();
        var results = new Dictionary<int, double[]>();

        foreach (int nodeId in order)
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;

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

            double[] result = EvaluateNode(node.Operation, inputValues);
            results[nodeId] = result;
            _cachedResults[nodeId] = result;
        }

        return results;
    }

    /// <summary>Executes the graph using parallel scheduling where possible.</summary>
    public IReadOnlyDictionary<int, double[]> ExecuteParallel(ComputationGraph graph, IReadOnlyDictionary<int, double[]> inputs)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        var schedule = _scheduler.Schedule(graph);
        var results = new ConcurrentDictionary<int, double[]>();

        foreach (var level in schedule.Levels)
        {
            Parallel.ForEach(level.NodeIds, nodeId =>
            {
                if (!graph.TryGetNode(nodeId, out var node) || node is null) return;

                if (node.IsInput || node.Operation == GraphOperation.Input)
                {
                    if (inputs.TryGetValue(nodeId, out var inputVal))
                        results[nodeId] = inputVal;
                    return;
                }

                var inputValues = new List<double[]>();
                foreach (int inputId in node.Inputs)
                {
                    if (results.TryGetValue(inputId, out var val))
                        inputValues.Add(val);
                }

                results[nodeId] = EvaluateNode(node.Operation, inputValues);
            });
        }

        return results;
    }

    /// <summary>Incrementally re-executes only the changed subgraph.</summary>
    public IReadOnlyDictionary<int, double[]> ExecuteIncremental(
        ComputationGraph graph,
        IReadOnlyDictionary<int, double[]> inputs,
        IReadOnlyCollection<int> changedNodeIds)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        var affected = new HashSet<int>();
        lock (_changeLock)
        {
            _changedNodes.Clear();
            if (changedNodeIds is not null)
            {
                foreach (int id in changedNodeIds)
                    _changedNodes.Add(id);
            }
        }

        FindAffectedNodes(graph, _changedNodes, affected);
        affected.UnionWith(_changedNodes);

        var order = graph.GetTopologicalOrder();
        var results = new Dictionary<int, double[]>(_cachedResults);

        foreach (int nodeId in order)
        {
            if (!affected.Contains(nodeId)) continue;
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;

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

            double[] result = EvaluateNode(node.Operation, inputValues);
            results[nodeId] = result;
            _cachedResults[nodeId] = result;
        }

        return results;
    }

    /// <summary>Clears all cached results.</summary>
    public void ClearCache()
    {
        _cachedResults.Clear();
    }

    /// <summary>Gets the cached result for a node, if available.</summary>
    public bool TryGetCachedResult(int nodeId, out double[]? result) =>
        _cachedResults.TryGetValue(nodeId, out result);

    private static void FindAffectedNodes(ComputationGraph graph, HashSet<int> changed, HashSet<int> affected)
    {
        var queue = new Queue<int>(changed);
        while (queue.Count > 0)
        {
            int nodeId = queue.Dequeue();
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;

            foreach (int outputId in node.Outputs)
            {
                if (!affected.Contains(outputId) && graph.TryGetNode(outputId, out _))
                {
                    affected.Add(outputId);
                    queue.Enqueue(outputId);
                }
            }
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
            result[i] = fn(i < a.Length ? a[i] : 0, i < b.Length ? b[i] : 0);
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
}
