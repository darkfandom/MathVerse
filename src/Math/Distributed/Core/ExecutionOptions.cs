namespace MathVerse.Math.Distributed.Core;

/// <summary>Defines the strategy used for scheduling tasks.</summary>
public enum SchedulerStrategy
{
    /// <summary>Work-stealing scheduler that distributes tasks across thread-local queues.</summary>
    WorkStealing,

    /// <summary>Round-robin distribution across available threads.</summary>
    RoundRobin,

    /// <summary>First-available thread assignment.</summary>
    FirstAvailable,

    /// <summary>Priority-based scheduling.</summary>
    Priority
}

/// <summary>Execution priority levels for task scheduling.</summary>
public enum ExecutionPriority
{
    /// <summary>Lowest priority, executed when resources are available.</summary>
    Lowest = 0,

    /// <summary>Low priority.</summary>
    Low = 1,

    /// <summary>Normal priority.</summary>
    Normal = 2,

    /// <summary>High priority.</summary>
    High = 3,

    /// <summary>Highest priority, executed before all others.</summary>
    Highest = 4
}

/// <summary>Configuration options for distributed execution.</summary>
public sealed class ExecutionOptions
{
    /// <summary>Maximum degree of parallelism for parallel operations.</summary>
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

    /// <summary>The scheduling strategy to use for task distribution.</summary>
    public SchedulerStrategy Scheduler { get; init; } = SchedulerStrategy.WorkStealing;

    /// <summary>Maximum time allowed for a single execution before timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Whether cancellation support is enabled.</summary>
    public bool CancellationEnabled { get; init; } = true;

    /// <summary>Default execution priority for tasks.</summary>
    public ExecutionPriority Priority { get; init; } = ExecutionPriority.Normal;

    /// <summary>Whether execution profiling and timing is enabled.</summary>
    public bool EnableProfiling { get; init; }

    /// <summary>Whether result caching is enabled.</summary>
    public bool EnableCaching { get; init; }

    /// <summary>Number of elements per chunk for parallel processing.</summary>
    public int ChunkSize { get; init; } = 1024;

    /// <summary>Whether SIMD vectorization is enabled for compatible operations.</summary>
    public bool EnableSIMD { get; init; } = true;

    /// <summary>Gets the default execution options.</summary>
    public static ExecutionOptions Default => new();
}
