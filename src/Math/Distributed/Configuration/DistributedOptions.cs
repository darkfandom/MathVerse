namespace MathVerse.Math.Distributed.Configuration;

/// <summary>Advanced options for distributed system behavior.</summary>
public sealed class DistributedOptions
{
    /// <summary>Maximum number of concurrent sessions.</summary>
    public int MaxConcurrentSessions { get; init; } = 64;

    /// <summary>Default timeout for remote operations in milliseconds.</summary>
    public int RemoteTimeoutMs { get; init; } = 30000;

    /// <summary>Number of retry attempts for failed remote calls.</summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>Delay between retry attempts in milliseconds.</summary>
    public int RetryDelayMs { get; init; } = 100;

    /// <summary>Whether to enable automatic failover.</summary>
    public bool EnableFailover { get; init; } = true;

    /// <summary>Whether to enable result caching across sessions.</summary>
    public bool EnableResultCaching { get; init; }

    /// <summary>Maximum cache size in megabytes.</summary>
    public long MaxCacheSizeMb { get; init; } = 256;

    /// <summary>Serialization format for inter-node communication.</summary>
    public string SerializationFormat { get; init; } = "Json";

    /// <summary>Gets the default distributed options.</summary>
    public static DistributedOptions Default => new();
}
