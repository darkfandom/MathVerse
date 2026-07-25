using MathVerse.Math.Geometry.Colors;

namespace MathVerse.Math.Geometry.Plotting;

/// <summary>Configures the appearance and layout of a plot.</summary>
/// <param name="Title">The plot title.</param>
/// <param name="XLabel">The X-axis label.</param>
/// <param name="YLabel">The Y-axis label.</param>
/// <param name="Width">The plot width in pixels.</param>
/// <param name="Height">The plot height in pixels.</param>
/// <param name="BackgroundColor">The background color.</param>
/// <param name="ShowGrid">Whether to display the grid.</param>
/// <param name="ShowLegend">Whether to display the legend.</param>
/// <param name="AxisLimits">Optional explicit axis limits.</param>
public record PlotConfiguration(
    string Title = "",
    string XLabel = "",
    string YLabel = "",
    double Width = 800,
    double Height = 600,
    Color BackgroundColor = default,
    bool ShowGrid = true,
    bool ShowLegend = true,
    (double XMin, double XMax, double YMin, double YMax)? AxisLimits = null)
{
    /// <summary>Gets a default plot configuration.</summary>
    public static PlotConfiguration Default { get; } = new();

    /// <summary>Initializes a new instance of the <see cref="PlotConfiguration"/> class with default values.</summary>
    public PlotConfiguration() : this("", "", "", 800, 600, default, true, true, null)
    {
    }
}
