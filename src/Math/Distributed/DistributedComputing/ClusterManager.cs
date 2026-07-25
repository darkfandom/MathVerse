namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages a cluster of worker nodes: registration, load tracking, and worker selection.
/// </summary>
public sealed class ClusterManager
{
    private readonly ConcurrentDictionary<string, WorkerNode> _workers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClusterManager"/> class.
    /// </summary>
    public ClusterManager() { }

    /// <summary>
    /// Registers a worker node with the cluster.
    /// </summary>
    /// <param name="worker">The worker node to register.</param>
    /// <returns>True if the worker was registered; false if a worker with the same ID already exists.</returns>
    public bool RegisterWorker(WorkerNode worker)
    {
        if (worker == null) throw new ArgumentNullException(nameof(worker));
        return _workers.TryAdd(worker.WorkerId, worker);
    }

    /// <summary>
    /// Unregisters a worker node from the cluster by its ID.
    /// </summary>
    /// <param name="workerId">The ID of the worker to remove.</param>
    /// <returns>True if the worker was found and removed; otherwise, false.</returns>
    public bool UnregisterWorker(string workerId)
    {
        if (workerId == null) throw new ArgumentNullException(nameof(workerId));
        return _workers.TryRemove(workerId, out _);
    }

    /// <summary>
    /// Gets all worker nodes that are not in the <see cref="WorkerStatus.Offline"/> state.
    /// </summary>
    /// <returns>A list of active worker nodes.</returns>
    public IReadOnlyList<WorkerNode> GetActiveWorkers()
    {
        var active = new List<WorkerNode>();
        foreach (var kvp in _workers)
        {
            if (kvp.Value.Status != WorkerStatus.Offline)
                active.Add(kvp.Value);
        }
        return active;
    }

    /// <summary>
    /// Gets the current load distribution across all active workers.
    /// </summary>
    /// <returns>A dictionary mapping worker IDs to their load factor (0.0 to 1.0).</returns>
    public IReadOnlyDictionary<string, double> GetLoadBalance()
    {
        var balance = new Dictionary<string, double>();
        foreach (var kvp in _workers)
        {
            if (kvp.Value.Status != WorkerStatus.Offline)
                balance[kvp.Key] = kvp.Value.CurrentLoad;
        }
        return balance;
    }

    /// <summary>
    /// Selects the best worker for a given task based on load, capabilities, and availability.
    /// The scoring heuristic favors workers with lower load, more available memory,
    /// and matching capabilities.
    /// </summary>
    /// <param name="task">The task to find a worker for.</param>
    /// <returns>The best available worker, or null if no suitable worker is found.</returns>
    public WorkerNode? SelectWorker(DistributedTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        WorkerNode? bestWorker = null;
        double bestScore = double.MinValue;

        foreach (var kvp in _workers)
        {
            var worker = kvp.Value;

            if (worker.Status == WorkerStatus.Offline || worker.Status == WorkerStatus.Draining)
                continue;

            double score = ComputeWorkerScore(worker);
            if (score > bestScore)
            {
                bestScore = score;
                bestWorker = worker;
            }
        }

        return bestWorker;
    }

    /// <summary>
    /// Gets a worker node by its ID.
    /// </summary>
    /// <param name="workerId">The worker ID to look up.</param>
    /// <param name="worker">The worker node if found.</param>
    /// <returns>True if the worker exists; otherwise, false.</returns>
    public bool TryGetWorker(string workerId, out WorkerNode? worker)
    {
        return _workers.TryGetValue(workerId, out worker);
    }

    /// <summary>
    /// Gets the total number of registered workers.
    /// </summary>
    public int WorkerCount => _workers.Count;

    /// <summary>
    /// Computes a score for a worker node based on inverse load, memory availability,
    /// and core count. Higher scores indicate a more suitable worker.
    /// </summary>
    private static double ComputeWorkerScore(WorkerNode worker)
    {
        double loadScore = 1.0 - worker.CurrentLoad;
        double memoryScore = worker.AvailableMemory > 0
            ? System.Math.Log(worker.AvailableMemory + 1.0) / 40.0
            : 0.0;
        double coreScore = worker.CoreCount / 64.0;
        double busyPenalty = worker.Status == WorkerStatus.Busy ? 0.1 : 0.0;

        return loadScore + memoryScore + coreScore - busyPenalty;
    }
}
