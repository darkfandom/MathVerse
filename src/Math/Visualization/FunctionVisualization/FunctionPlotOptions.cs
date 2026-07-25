namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Options for configuring function plots.</summary>
public sealed class FunctionPlotOptions
{
    /// <summary>Gets or sets the title of the plot.</summary>
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the line color as RGBA in [0,1].</summary>
    public Vector4 LineColor { get; set; } = new(0.2f, 0.4f, 0.8f, 1f);

    /// <summary>Gets or sets the line width in pixels.</summary>
    public float LineWidth { get; set; } = 2.0f;

    /// <summary>Gets or sets the background color as RGBA in [0,1].</summary>
    public Vector4 BackgroundColor { get; set; } = new(1f, 1f, 1f, 1f);

    /// <summary>Gets or sets the X-axis label.</summary>
    public string XLabel { get; set; } = "x";

    /// <summary>Gets or sets the Y-axis label.</summary>
    public string YLabel { get; set; } = "y";

    /// <summary>Gets or sets whether to show the grid.</summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>Gets or sets whether to show axis ticks.</summary>
    public bool ShowTicks { get; set; } = true;

    /// <summary>Gets or sets the X range to display (auto-computed if NaN).</summary>
    public double XMin { get; set; } = double.NaN;

    /// <summary>Gets or sets the X range to display (auto-computed if NaN).</summary>
    public double XMax { get; set; } = double.NaN;

    /// <summary>Gets or sets the Y range to display (auto-computed if NaN).</summary>
    public double YMin { get; set; } = double.NaN;

    /// <summary>Gets or sets the Y range to display (auto-computed if NaN).</summary>
    public double YMax { get; set; } = double.NaN;

    /// <summary>Gets or sets the number of adaptive subdivisions for curvature.</summary>
    public int AdaptiveDepth { get; set; } = 4;

    /// <summary>Gets or sets the minimum angle threshold in radians for adaptive subdivision.</summary>
    public double CurvatureThreshold { get; set; } = 0.1;

    /// <summary>Gets or sets the fill color for area fills.</summary>
    public Vector4 FillColor { get; set; } = new(0.3f, 0.6f, 0.9f, 0.3f);

    /// <summary>Gets or sets whether to fill the area under the curve.</summary>
    public bool FillArea { get; set; }
}
