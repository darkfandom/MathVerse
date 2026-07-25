using System.Collections.Immutable;
using MathVerse.Math.Geometry.Mesh;

namespace MathVerse.Math.Geometry.Plotting;

/// <summary>Represents the result of a plot generation operation.</summary>
/// <param name="Configuration">The plot configuration used.</param>
/// <param name="Lines">The line plot data.</param>
/// <param name="ScatterPlots">The scatter plot data.</param>
/// <param name="Bars">The bar plot data.</param>
/// <param name="SurfaceMesh">Optional mesh data for surface plots.</param>
/// <param name="Success">Whether the plot was generated successfully.</param>
/// <param name="Error">The error message if generation failed.</param>
public record PlotResult(
    PlotConfiguration Configuration,
    IReadOnlyList<LinePlotData> Lines,
    IReadOnlyList<ScatterPlotData> ScatterPlots,
    IReadOnlyList<BarPlotData> Bars,
    TriangleMesh? SurfaceMesh,
    bool Success,
    string? Error)
{
    /// <summary>Gets the configuration used for this result.</summary>
    public PlotConfiguration Configuration { get; } = Configuration;

    /// <summary>Gets the line plot data in this result.</summary>
    public IReadOnlyList<LinePlotData> Lines { get; } = Lines;

    /// <summary>Gets the scatter plot data in this result.</summary>
    public IReadOnlyList<ScatterPlotData> ScatterPlots { get; } = ScatterPlots;

    /// <summary>Gets the bar plot data in this result.</summary>
    public IReadOnlyList<BarPlotData> Bars { get; } = Bars;

    /// <summary>Gets the optional surface mesh data.</summary>
    public TriangleMesh? SurfaceMesh { get; } = SurfaceMesh;

    /// <summary>Gets whether the plot was generated successfully.</summary>
    public bool Success { get; } = Success;

    /// <summary>Gets the error message if generation failed.</summary>
    public string? Error { get; } = Error;

    /// <summary>Creates a failed plot result with the specified error message.</summary>
    /// <param name="error">The error message.</param>
    /// <returns>A new <see cref="PlotResult"/> indicating failure.</returns>
    public static PlotResult Failed(string error) => new(
        new PlotConfiguration(),
        ImmutableArray<LinePlotData>.Empty,
        ImmutableArray<ScatterPlotData>.Empty,
        ImmutableArray<BarPlotData>.Empty,
        null,
        false,
        error);
}
