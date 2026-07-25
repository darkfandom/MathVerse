namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the operational status of a worker node in the cluster.
/// </summary>
public enum WorkerStatus
{
    /// <summary>The worker is idle and ready to accept tasks.</summary>
    Idle,

    /// <summary>The worker is currently processing a task.</summary>
    Busy,

    /// <summary>The worker is offline and unreachable.</summary>
    Offline,

    /// <summary>The worker is draining existing tasks before shutting down.</summary>
    Draining
}

/// <summary>
/// Represents a worker node in the distributed computing cluster.
/// </summary>
public sealed class WorkerNode
{
    /// <summary>Gets the unique identifier of the worker node.</summary>
    public string WorkerId { get; }

    /// <summary>Gets the hostname or network address of the worker.</summary>
    public string HostName { get; }

    /// <summary>Gets the number of CPU cores available on the worker.</summary>
    public int CoreCount { get; }

    /// <summary>Gets the available memory in bytes on the worker.</summary>
    public long AvailableMemory { get; }

    /// <summary>Gets the current load factor ranging from 0.0 (idle) to 1.0 (fully utilized).</summary>
    public double CurrentLoad { get; }

    /// <summary>Gets the current operational status of the worker.</summary>
    public WorkerStatus Status { get; }

    /// <summary>Gets the UTC timestamp of the last heartbeat received from the worker.</summary>
    public DateTime LastHeartbeat { get; }

    /// <summary>Gets the list of named capabilities supported by the worker (e.g., "gpu", "sse4").</summary>
    public IReadOnlyList<string> Capabilities { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerNode"/> class.
    /// </summary>
    /// <param name="workerId">Unique identifier for the worker.</param>
    /// <param name="hostname">Hostname or address of the worker.</param>
    /// <param name="coreCount">Number of CPU cores.</param>
    /// <param name="availableMemory">Available memory in bytes.</param>
    /// <param name="currentLoad">Current load factor between 0.0 and 1.0.</param>
    /// <param name="status">Current operational status.</param>
    /// <param name="lastHeartbeat">UTC timestamp of the last heartbeat.</param>
    /// <param name="capabilities">List of worker capabilities.</param>
    public WorkerNode(
        string workerId,
        string hostname,
        int coreCount,
        long availableMemory,
        double currentLoad,
        WorkerStatus status,
        DateTime lastHeartbeat,
        IReadOnlyList<string>? capabilities)
    {
        WorkerId = workerId ?? throw new ArgumentNullException(nameof(workerId));
        HostName = hostname ?? throw new ArgumentNullException(nameof(hostname));
        CoreCount = coreCount;
        AvailableMemory = availableMemory;
        CurrentLoad = System.Math.Clamp(currentLoad, 0.0, 1.0);
        Status = status;
        LastHeartbeat = lastHeartbeat;
        Capabilities = capabilities ?? Array.Empty<string>();
    }
}
