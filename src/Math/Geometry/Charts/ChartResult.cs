using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Charts;

/// <summary>Represents the result of a chart generation operation.</summary>
/// <param name="Configuration">The chart configuration used.</param>
/// <param name="Success">Whether the chart was generated successfully.</param>
/// <param name="Error">The error message if generation failed.</param>
/// <param name="Series">The data series in the chart.</param>
/// <param name="PieSlices">The pie slices (for pie charts).</param>
public record ChartResult(
    ChartConfiguration Configuration,
    bool Success,
    string? Error,
    IReadOnlyList<Series> Series,
    IReadOnlyList<PieSlice> PieSlices)
{
    /// <summary>Gets the configuration used for this result.</summary>
    public ChartConfiguration Configuration { get; } = Configuration;

    /// <summary>Gets whether the chart was generated successfully.</summary>
    public bool Success { get; } = Success;

    /// <summary>Gets the error message if generation failed.</summary>
    public string? Error { get; } = Error;

    /// <summary>Gets the data series in the chart.</summary>
    public IReadOnlyList<Series> Series { get; } = Series;

    /// <summary>Gets the pie slices for pie charts.</summary>
    public IReadOnlyList<PieSlice> PieSlices { get; } = PieSlices;

    /// <summary>Creates a failed chart result with the specified error message.</summary>
    /// <param name="error">The error message.</param>
    /// <returns>A new <see cref="ChartResult"/> indicating failure.</returns>
    public static ChartResult Failed(string error) => new(
        new ChartConfiguration(),
        false,
        error,
        ImmutableArray<Series>.Empty,
        ImmutableArray<PieSlice>.Empty);
}
