namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Executes computation graph nodes in parallel where dependencies allow.
/// Uses topological ordering and the execution schedule to maximize throughput.
/// </summary>
public sealed class ParallelGraphExecutor
{
    private readonly int _maxDegreeOfParallelism;

    /// <summary>
    /// Initializes the parallel graph executor.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Maximum concurrent tasks.</param>
    public ParallelGraphExecutor(int maxDegreeOfParallelism = 0)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism > 0
            ? maxDegreeOfParallelism
            : Environment.ProcessorCount;
    }

    /// <summary>
    /// Executes the computation graph, running independent nodes in parallel.
    /// </summary>
    /// <param name="graph">The computation graph to execute.</param>
    /// <returns>A dictionary mapping node IDs to their computed results.</returns>
    public Dictionary<string, double> Execute(ComputationGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var results = new ConcurrentDictionary<string, double>();
        var completedEvents = new Dictionary<string, ManualResetEventSlim>();
        var lockObj = new object();

        foreach (var node in graph.Nodes)
        {
            completedEvents[node.Id] = new ManualResetEventSlim(false);
        }

        var executionOrder = ComputeExecutionOrder(graph);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };

        foreach (var batch in executionOrder)
        {
            Parallel.ForEach(batch, options, node =>
            {
                foreach (var dep in node.Dependencies)
                {
                    if (completedEvents.TryGetValue(dep, out var evt))
                    {
                        evt.Wait();
                    }
                }

                var inputValues = new List<double>();
                foreach (var dep in node.Dependencies)
                {
                    if (results.TryGetValue(dep, out var val))
                        inputValues.Add(val);
                }

                var result = node.Execute(inputValues);
                results[node.Id] = result;

                if (completedEvents.TryGetValue(node.Id, out var nodeEvt))
                {
                    nodeEvt.Set();
                }
            });
        }

        foreach (var evt in completedEvents.Values)
            evt.Dispose();

        return new Dictionary<string, double>(results);
    }

    /// <summary>
    /// Executes the graph asynchronously with cancellation support.
    /// </summary>
    /// <param name="graph">The computation graph to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task yielding the results dictionary.</returns>
    public async Task<Dictionary<string, double>> ExecuteAsync(
        ComputationGraph graph,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var results = new ConcurrentDictionary<string, double>();
        var completedSources = new Dictionary<string, TaskCompletionSource<bool>>();

        foreach (var node in graph.Nodes)
        {
            completedSources[node.Id] = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        var executionOrder = ComputeExecutionOrder(graph);
        var allTasks = new List<Task>();

        var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);

        foreach (var batch in executionOrder)
        {
            var batchTasks = new List<Task>();

            foreach (var node in batch)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                var nodeTask = ExecuteNodeAsync(
                    node, results, completedSources, semaphore, cancellationToken);
                batchTasks.Add(nodeTask);
            }

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

        return new Dictionary<string, double>(results);
    }

    private static async Task ExecuteNodeAsync(
        GraphNode node,
        ConcurrentDictionary<string, double> results,
        Dictionary<string, TaskCompletionSource<bool>> completedSources,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        try
        {
            var depTasks = new List<Task>();
            foreach (var dep in node.Dependencies)
            {
                if (completedSources.TryGetValue(dep, out var tcs))
                    depTasks.Add(tcs.Task);
            }

            if (depTasks.Count > 0)
                await Task.WhenAll(depTasks).ConfigureAwait(false);

            var inputValues = new List<double>();
            foreach (var dep in node.Dependencies)
            {
                if (results.TryGetValue(dep, out var val))
                    inputValues.Add(val);
            }

            var result = node.Execute(inputValues);
            results[node.Id] = result;

            if (completedSources.TryGetValue(node.Id, out var nodeTcs))
                nodeTcs.TrySetResult(true);
        }
        catch (Exception ex)
        {
            if (completedSources.TryGetValue(node.Id, out var nodeTcs))
                nodeTcs.TrySetException(ex);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static List<List<GraphNode>> ComputeExecutionOrder(ComputationGraph graph)
    {
        var inDegree = new Dictionary<string, int>();
        var dependents = new Dictionary<string, List<string>>();

        foreach (var node in graph.Nodes)
        {
            inDegree[node.Id] = node.Dependencies.Count;
            dependents[node.Id] = new List<string>();
        }

        foreach (var node in graph.Nodes)
        {
            foreach (var dep in node.Dependencies)
            {
                if (dependents.TryGetValue(dep, out var deps))
                    deps.Add(node.Id);
            }
        }

        var batches = new List<List<GraphNode>>();
        var queue = new Queue<string>();

        foreach (var (id, deg) in inDegree)
        {
            if (deg == 0)
                queue.Enqueue(id);
        }

        var nodeMap = new Dictionary<string, GraphNode>();
        foreach (var node in graph.Nodes)
            nodeMap[node.Id] = node;

        while (queue.Count > 0)
        {
            var batch = new List<GraphNode>();
            var nextQueue = new Queue<string>();

            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (nodeMap.TryGetValue(id, out var node))
                    batch.Add(node);

                if (dependents.TryGetValue(id, out var deps))
                {
                    foreach (var depId in deps)
                    {
                        if (inDegree.TryGetValue(depId, out var deg) && deg > 0)
                        {
                            inDegree[depId] = deg - 1;
                            if (inDegree[depId] == 0)
                                nextQueue.Enqueue(depId);
                        }
                    }
                }
            }

            if (batch.Count > 0)
                batches.Add(batch);

            while (nextQueue.Count > 0)
                queue.Enqueue(nextQueue.Dequeue());
        }

        return batches;
    }

    /// <summary>
    /// Gets execution statistics for the parallel graph execution.
    /// </summary>
    /// <param name="graph">The graph that was executed.</param>
    /// <returns>Execution statistics including batch counts and timing.</returns>
    public ExecutionStats GetExecutionStats(ComputationGraph graph)
    {
        var order = ComputeExecutionOrder(graph);
        var totalNodes = graph.Nodes.Count;
        var batchSize = order.Count > 0 ? order.Max(b => b.Count) : 0;

        return new ExecutionStats
        {
            TotalNodes = totalNodes,
            TotalBatches = order.Count,
            MaxBatchSize = batchSize,
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };
    }
}

/// <summary>
/// Statistics about parallel graph execution.
/// </summary>
public sealed class ExecutionStats
{
    /// <summary>Total number of nodes in the graph.</summary>
    public int TotalNodes { get; set; }

    /// <summary>Number of execution batches.</summary>
    public int TotalBatches { get; set; }

    /// <summary>Maximum number of nodes in any single batch.</summary>
    public int MaxBatchSize { get; set; }

    /// <summary>Maximum degree of parallelism used.</summary>
    public int MaxDegreeOfParallelism { get; set; }
}
