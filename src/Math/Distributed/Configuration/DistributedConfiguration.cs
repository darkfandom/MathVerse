namespace MathVerse.Math.Distributed.Configuration;

using MathVerse.Math.Distributed.Core;

/// <summary>Top-level configuration for the distributed execution system.</summary>
public sealed class DistributedConfiguration
{
    /// <summary>The execution options.</summary>
    public ExecutionOptions Execution { get; init; } = ExecutionOptions.Default;

    /// <summary>Cluster configuration settings.</summary>
    public ClusterConfiguration Cluster { get; init; } = ClusterConfiguration.Default;

    /// <summary>Scheduler configuration settings.</summary>
    public SchedulerConfiguration Scheduler { get; init; } = SchedulerConfiguration.Default;

    /// <summary>Whether diagnostics are enabled.</summary>
    public bool EnableDiagnostics { get; init; } = true;

    /// <summary>Log level for distributed operations.</summary>
    public string LogLevel { get; init; } = "Information";

    /// <summary>Gets the default distributed configuration.</summary>
    public static DistributedConfiguration Default => new();
}
