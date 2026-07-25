namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Captures statistics from a parallel evaluation run.
/// </summary>
public readonly record struct ParallelExecutionStatistics
{
    /// <summary>
    /// Gets the total number of tasks that were scheduled.
    /// </summary>
    public long TotalTasks { get; init; }

    /// <summary>
    /// Gets the number of tasks that completed successfully.
    /// </summary>
    public long CompletedTasks { get; init; }

    /// <summary>
    /// Gets the number of tasks that failed with an exception.
    /// </summary>
    public long FailedTasks { get; init; }

    /// <summary>
    /// Gets the average wall-clock time per task in milliseconds.
    /// </summary>
    public double AverageTaskTimeMs { get; init; }

    /// <summary>
    /// Gets the total elapsed time in milliseconds for the entire run.
    /// </summary>
    public long TotalTimeMs { get; init; }

    /// <summary>
    /// Gets the peak number of tasks executing concurrently.
    /// </summary>
    public int PeakConcurrency { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"TotalTasks={TotalTasks}, CompletedTasks={CompletedTasks}, FailedTasks={FailedTasks}, " +
        $"AverageTaskTimeMs={AverageTaskTimeMs:F2}, TotalTimeMs={TotalTimeMs}, PeakConcurrency={PeakConcurrency}";
}
