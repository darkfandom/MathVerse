namespace MathVerse.Math.Distributed.IncrementalComputation;

using System.Collections.Concurrent;

/// <summary>Schedules incremental recomputation of dirty computation nodes.</summary>
public sealed class IncrementalScheduler
{
    private readonly DependencyGraphExecutor _executor;
    private readonly ConcurrentQueue<int> _pendingQueue = new();
    private readonly ConcurrentDictionary<int, bool> _enqueued = new();
    private readonly object _scheduleLock = new();

    /// <summary>Initializes a new instance of the <see cref="IncrementalScheduler"/> class.</summary>
    /// <param name="executor">The dependency graph executor to schedule recomputations for.</param>
    public IncrementalScheduler(DependencyGraphExecutor executor)
    {
        _executor = executor;
    }

    /// <summary>Gets the number of nodes currently pending execution.</summary>
    public int PendingCount => _pendingQueue.Count;

    /// <summary>Schedules a node and its dependencies for recomputation.</summary>
    /// <param name="nodeId">The ID of the node to schedule.</param>
    public void Schedule(int nodeId)
    {
        lock (_scheduleLock)
        {
            _executor.MarkDirty(nodeId);
            EnqueueIfNeeded(nodeId);
        }
    }

    /// <summary>Schedules multiple nodes for recomputation.</summary>
    /// <param name="nodeIds">The IDs of the nodes to schedule.</param>
    public void ScheduleBatch(IEnumerable<int> nodeIds)
    {
        lock (_scheduleLock)
        {
            foreach (var nodeId in nodeIds)
            {
                _executor.MarkDirty(nodeId);
                EnqueueIfNeeded(nodeId);
            }
        }
    }

    /// <summary>Gets the list of node IDs currently pending execution, without removing them.</summary>
    /// <returns>An array of pending node IDs.</returns>
    public int[] GetPendingNodes()
    {
        return _pendingQueue.ToArray();
    }

    /// <summary>Executes all pending nodes using the provided input function.</summary>
    /// <param name="inputProvider">A function that provides input data for a given node ID.</param>
    /// <returns>The number of nodes that were executed.</returns>
    public int ExecutePending(Func<int, double[]> inputProvider)
    {
        int executed = 0;

        while (_pendingQueue.TryDequeue(out var nodeId))
        {
            _enqueued.TryRemove(nodeId, out _);

            try
            {
                double[] input = inputProvider(nodeId);
                _executor.Execute(nodeId, input);
                executed++;
            }
            catch (KeyNotFoundException)
            {
                continue;
            }
        }

        return executed;
    }

    /// <summary>Executes all pending nodes with the specified default input.</summary>
    /// <param name="defaultInput">The default input to use for all nodes.</param>
    /// <returns>The number of nodes that were executed.</returns>
    public int ExecutePending(double[] defaultInput)
    {
        return ExecutePending(_ => defaultInput);
    }

    /// <summary>Returns true if the specified node is currently pending execution.</summary>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if the node is pending.</returns>
    public bool IsPending(int nodeId)
    {
        return _enqueued.ContainsKey(nodeId);
    }

    /// <summary>Clears all pending nodes without executing them.</summary>
    public void ClearPending()
    {
        while (_pendingQueue.TryDequeue(out _)) { }
        _enqueued.Clear();
    }

    /// <summary>Drains all pending nodes, returning their IDs and clearing the queue.</summary>
    /// <returns>An array of node IDs that were drained.</returns>
    public int[] DrainPending()
    {
        var drained = new List<int>();
        while (_pendingQueue.TryDequeue(out var nodeId))
        {
            _enqueued.TryRemove(nodeId, out _);
            drained.Add(nodeId);
        }
        return drained.ToArray();
    }

    private void EnqueueIfNeeded(int nodeId)
    {
        if (_enqueued.TryAdd(nodeId, true))
        {
            _pendingQueue.Enqueue(nodeId);
        }
    }
}
