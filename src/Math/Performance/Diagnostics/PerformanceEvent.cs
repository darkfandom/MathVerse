namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Records a single performance measurement for a tracked operation.
/// </summary>
/// <param name="Operation">The name of the operation.</param>
/// <param name="DurationTicks">The elapsed time in <see cref="Stopwatch"/> ticks.</param>
/// <param name="AllocatedBytes">The number of bytes allocated during the operation.</param>
/// <param name="Success">Whether the operation completed successfully.</param>
/// <param name="Details">Optional additional details about the operation.</param>
public sealed record PerformanceEvent(
    string Operation,
    long DurationTicks,
    long AllocatedBytes,
    bool Success,
    string? Details)
{
    /// <summary>
    /// Gets the duration of the operation in milliseconds.
    /// </summary>
    public double DurationMs => (double)DurationTicks / Stopwatch.Frequency * 1000.0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{Operation}] Duration={DurationMs:F2}ms, Allocated={AllocatedBytes}B, Success={Success}";
}
