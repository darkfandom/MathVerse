namespace MathVerse.Math.Distributed.Configuration;

using MathVerse.Math.Distributed.Core;

/// <summary>Configuration for the work-stealing task scheduler.</summary>
public sealed class SchedulerConfiguration
{
    /// <summary>The scheduling strategy to use.</summary>
    public SchedulerStrategy Strategy { get; init; } = SchedulerStrategy.WorkStealing;

    /// <summary>Maximum number of tasks per thread-local queue.</summary>
    public int MaxLocalQueueSize { get; init; } = 256;

    /// <summary>Number of work items to steal at once.</summary>
    public int StealBatchSize { get; init; } = 4;

    /// <summary>Whether to enable work stealing across threads.</summary>
    public bool EnableWorkStealing { get; init; } = true;

    /// <summary>Thread pool minimum thread count.</summary>
    public int MinThreadCount { get; init; } = 4;

    /// <summary>Thread pool maximum thread count.</summary>
    public int MaxThreadCount { get; init; } = Environment.ProcessorCount * 2;

    /// <summary>Idle timeout before threads return to the pool in milliseconds.</summary>
    public int IdleTimeoutMs { get; init; } = 1000;

    /// <summary>Gets the default scheduler configuration.</summary>
    public static SchedulerConfiguration Default => new();
}
