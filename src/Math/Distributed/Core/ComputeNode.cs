namespace MathVerse.Math.Distributed.Core;

/// <summary>Status of a compute node.</summary>
public enum NodeStatus
{
    /// <summary>Node is actively processing tasks.</summary>
    Active,

    /// <summary>Node is idle and available for work.</summary>
    Idle,

    /// <summary>Node is offline and unreachable.</summary>
    Offline,

    /// <summary>Node is overloaded and should not receive new work.</summary>
    Overloaded
}

/// <summary>Represents a compute node in the distributed cluster.</summary>
public sealed class ComputeNode
{
    /// <summary>Unique identifier for this node.</summary>
    public string NodeId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>The hostname or IP address of this node.</summary>
    public string HostName { get; init; } = "localhost";

    /// <summary>Number of CPU cores available on this node.</summary>
    public int CoreCount { get; init; } = Environment.ProcessorCount;

    /// <summary>Available memory in megabytes on this node.</summary>
    public long AvailableMemory { get; init; } = 4096;

    /// <summary>Current CPU load as a value between 0 and 1.</summary>
    public double CurrentLoad { get; set; }

    /// <summary>Current operational status of this node.</summary>
    public NodeStatus Status { get; set; } = NodeStatus.Idle;

    /// <summary>Capabilities supported by this node (e.g., "SIMD", "GPU").</summary>
    public List<string> Capabilities { get; init; } = new();

    /// <summary>Timestamp of the last heartbeat received from this node.</summary>
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;

    /// <summary>Number of tasks currently assigned to this node.</summary>
    public int AssignedTaskCount { get; set; }

    /// <summary>Returns true if the node is available for work.</summary>
    public bool IsAvailable =>
        Status == NodeStatus.Idle || Status == NodeStatus.Active;

    /// <summary>Whether the node supports the specified capability.</summary>
    /// <param name="capability">The capability to check.</param>
    /// <returns>True if the node has the capability.</returns>
    public bool HasCapability(string capability)
    {
        return Capabilities.Contains(capability, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Updates the heartbeat timestamp to the current time.</summary>
    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    /// <summary>Calculates the effective score for load balancing (lower is better).</summary>
    /// <returns>A score between 0 and 1 where lower indicates more available capacity.</returns>
    public double GetLoadScore()
    {
        double loadPenalty = CurrentLoad;
        double taskPenalty = System.Math.Min(AssignedTaskCount / (double)System.Math.Max(CoreCount, 1), 1.0);
        return (loadPenalty + taskPenalty) * 0.5;
    }
}
