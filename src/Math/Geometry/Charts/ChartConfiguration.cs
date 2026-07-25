using MathVerse.Math.Geometry.Colors;

namespace MathVerse.Math.Geometry.Charts;

/// <summary>Configures the appearance and layout of a chart.</summary>
/// <param name="Title">The chart title.</param>
/// <param name="Width">The chart width in pixels.</param>
/// <param name="Height">The chart height in pixels.</param>
/// <param name="BackgroundColor">The background color.</param>
/// <param name="ShowLegend">Whether to display the legend.</param>
/// <param name="ShowGrid">Whether to display the grid.</param>
public record ChartConfiguration(
    string Title = "",
    double Width = 800,
    double Height = 600,
    Color BackgroundColor = default,
    bool ShowLegend = true,
    bool ShowGrid = true)
{
    /// <summary>Gets a default chart configuration.</summary>
    public static ChartConfiguration Default { get; } = new();

    /// <summary>Initializes a new instance of the <see cref="ChartConfiguration"/> class with default values.</summary>
    public ChartConfiguration() : this("", 800, 600, default, true, true)
    {
    }
}
