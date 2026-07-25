namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Options for configuring 2D plot appearance and behavior.</summary>
public sealed class Plot2DOptions
{
    /// <summary>Title displayed above the plot area.</summary>
    public string? Title { get; init; }

    /// <summary>Label for the horizontal axis.</summary>
    public string? XAxisLabel { get; init; }

    /// <summary>Label for the vertical axis.</summary>
    public string? YAxisLabel { get; init; }

    /// <summary>Whether to render grid lines behind the data.</summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>Whether to render a legend identifying each series.</summary>
    public bool ShowLegend { get; init; } = true;

    /// <summary>Background color as a hex string.</summary>
    public string BackgroundColor { get; init; } = "#FFFFFF";

    /// <summary>Explicit minimum X axis value, or null for automatic scaling.</summary>
    public double? XMin { get; init; }

    /// <summary>Explicit maximum X axis value, or null for automatic scaling.</summary>
    public double? XMax { get; init; }

    /// <summary>Explicit minimum Y axis value, or null for automatic scaling.</summary>
    public double? YMin { get; init; }

    /// <summary>Explicit maximum Y axis value, or null for automatic scaling.</summary>
    public double? YMax { get; init; }

    /// <summary>Maximum number of tick marks per axis.</summary>
    public int MaxTicks { get; init; } = 10;

    /// <summary>Whether the X axis uses a logarithmic scale.</summary>
    public bool IsXLogarithmic { get; init; }

    /// <summary>Whether the Y axis uses a logarithmic scale.</summary>
    public bool IsYLogarithmic { get; init; }
}
