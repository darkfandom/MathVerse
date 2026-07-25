namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Represents a single diagnostic event captured by the performance monitoring subsystem.
/// </summary>
/// <param name="Warning">The type of performance warning.</param>
    /// <param name="Message">A human-readable description of the diagnostic.</param>
/// <param name="Timestamp">The UTC time when the diagnostic was recorded.</param>
/// <param name="Category">An optional category for grouping diagnostics.</param>
public sealed record PerformanceDiagnostic(
    PerformanceWarning Warning,
    string Message,
    DateTime Timestamp,
    string? Category)
{
    /// <summary>
    /// Creates a new <see cref="PerformanceDiagnostic"/> with the current UTC timestamp.
    /// </summary>
    /// <param name="warning">The type of performance warning.</param>
    /// <param name="message">A human-readable description of the diagnostic.</param>
    /// <param name="category">An optional category for grouping diagnostics.</param>
    /// <returns>A new diagnostic instance.</returns>
    public static PerformanceDiagnostic Create(PerformanceWarning warning, string message, string? category = null) =>
        new(warning, message, DateTime.UtcNow, category);
}
