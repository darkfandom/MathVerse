namespace MathVerse.Math.Distributed.Configuration;

/// <summary>Configuration for the compute cluster.</summary>
public sealed class ClusterConfiguration
{
    /// <summary>Heartbeat interval in seconds.</summary>
    public int HeartbeatIntervalSeconds { get; init; } = 5;

    /// <summary>Number of missed heartbeats before a node is marked offline.</summary>
    public int MissedHeartbeatThreshold { get; init; } = 3;

    /// <summary>Maximum number of nodes in the cluster.</summary>
    public int MaxNodeCount { get; init; } = 128;

    /// <summary>Default load threshold for overload detection (0 to 1).</summary>
    public double OverloadThreshold { get; init; } = 0.9;

    /// <summary>Whether to enable automatic node discovery.</summary>
    public bool EnableAutoDiscovery { get; init; }

    /// <summary>Whether to enable load balancing across nodes.</summary>
    public bool EnableLoadBalancing { get; init; } = true;

    /// <summary>Default port for node communication.</summary>
    public int DefaultPort { get; init; } = 9000;

    /// <summary>Connection timeout in milliseconds.</summary>
    public int ConnectionTimeoutMs { get; init; } = 5000;

    /// <summary>Gets the default cluster configuration.</summary>
    public static ClusterConfiguration Default => new();
}
