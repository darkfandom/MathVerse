namespace MathVerse.Math.Visualization._2DPlotting;
using System.Collections.Immutable;

/// <summary>Result of a 2D plotting operation.</summary>
public sealed class Plot2DResult
{
    /// <summary>Series of data points to render.</summary>
    public List<PlotSeries> Series { get; init; } = [];

    /// <summary>Line series for the plot.</summary>
    public List<Line2DSeries> Lines { get; init; } = [];

    /// <summary>Point series for the plot.</summary>
    public List<Point2DSeries> Points { get; init; } = [];

    /// <summary>Annotations on the plot.</summary>
    public List<Annotation2D> Annotations { get; init; } = [];

    /// <summary>X axis configuration.</summary>
    public PlotAxis XAxis { get; init; } = new();

    /// <summary>Y axis configuration.</summary>
    public PlotAxis YAxis { get; init; } = new();

    /// <summary>Title displayed above the plot.</summary>
    public string Title { get; init; } = "";

    /// <summary>X axis label.</summary>
    public string XLabel { get; set; } = "";

    /// <summary>Y axis label.</summary>
    public string YLabel { get; set; } = "";

    /// <summary>Minimum X value.</summary>
    public double XMin { get; set; } = double.NaN;

    /// <summary>Maximum X value.</summary>
    public double XMax { get; set; } = double.NaN;

    /// <summary>Minimum Y value.</summary>
    public double YMin { get; set; } = double.NaN;

    /// <summary>Maximum Y value.</summary>
    public double YMax { get; set; } = double.NaN;

    /// <summary>Whether the Y axis uses logarithmic scale.</summary>
    public bool LogScaleY { get; set; }

    /// <summary>Whether to render grid lines.</summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>Whether to render a legend.</summary>
    public bool ShowLegend { get; init; } = true;

    /// <summary>Background color as a hex string.</summary>
    public string BackgroundColor { get; init; } = "#FFFFFF";

    /// <summary>Computed bounding box of the plot area.</summary>
    public BoundingBox2D Bounds { get; init; }
}

/// <summary>A single data series within a 2D plot.</summary>
public sealed class PlotSeries
{
    /// <summary>Label for this series in the legend.</summary>
    public string Label { get; init; } = "";

    /// <summary>Data points in the series.</summary>
    public List<Point2D> Points { get; init; } = [];

    /// <summary>Stroke or marker color as a hex string.</summary>
    public string Color { get; init; } = "#007ACC";

    /// <summary>Width of the line connecting points.</summary>
    public double LineWidth { get; init; } = 2.0;

    /// <summary>Style of the connecting line.</summary>
    public LineStyle LineStyle { get; init; }

    /// <summary>Shape of markers at data points.</summary>
    public MarkerStyle Marker { get; init; }

    /// <summary>Size of markers in points.</summary>
    public double MarkerSize { get; init; } = 4.0;

    /// <summary>Whether the area under the line is filled.</summary>
    public bool IsFilled { get; init; }

    /// <summary>Fill color as a hex string when <see cref="IsFilled"/> is true.</summary>
    public string FillColor { get; init; } = "#007ACC33";
}

/// <summary>A point in 2D Cartesian space.</summary>
/// <param name="X">X coordinate.</param>
/// <param name="Y">Y coordinate.</param>
public readonly record struct Point2D(double X, double Y);

/// <summary>Axis-aligned bounding box in 2D space.</summary>
/// <param name="XMin">Minimum X value.</param>
/// <param name="YMin">Minimum Y value.</param>
/// <param name="XMax">Maximum X value.</param>
/// <param name="YMax">Maximum Y value.</param>
public readonly record struct BoundingBox2D(double XMin, double YMin, double XMax, double YMax);

/// <summary>Configuration for a single plot axis.</summary>
public sealed class PlotAxis
{
    /// <summary>Label displayed along the axis.</summary>
    public string Label { get; init; } = "";

    /// <summary>Minimum visible value on the axis.</summary>
    public double Min { get; init; }

    /// <summary>Maximum visible value on the axis.</summary>
    public double Max { get; init; }

    /// <summary>Whether the axis uses a logarithmic scale.</summary>
    public bool IsLogarithmic { get; init; }

    /// <summary>Tick marks along the axis.</summary>
    public List<TickMark> Ticks { get; init; } = [];
}

/// <summary>A single tick mark on an axis.</summary>
/// <param name="Value">Numeric position of the tick.</param>
/// <param name="Label">Text label displayed at the tick.</param>
public readonly record struct TickMark(double Value, string Label);

/// <summary>Style of lines connecting data points.</summary>
public enum LineStyle
{
    /// <summary>Solid continuous line.</summary>
    Solid,
    /// <summary>Dashed line.</summary>
    Dashed,
    /// <summary>Dotted line.</summary>
    Dotted,
    /// <summary>Alternating dash-dot pattern.</summary>
    DashDot
}

/// <summary>Shape of markers at data points.</summary>
public enum MarkerStyle
{
    /// <summary>No marker rendered.</summary>
    None,
    /// <summary>Circle marker.</summary>
    Circle,
    /// <summary>Square marker.</summary>
    Square,
    /// <summary>Triangle marker.</summary>
    Triangle,
    /// <summary>Diamond marker.</summary>
    Diamond,
    /// <summary>Cross (X) marker.</summary>
    Cross,
    /// <summary>Plus (+) marker.</summary>
    Plus,
    /// <summary>Star marker.</summary>
    Star
}

/// <summary>A series of 2D line data.</summary>
public sealed class Line2DSeries
{
    /// <summary>Series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>X coordinates.</summary>
    public ImmutableArray<double> X { get; init; }

    /// <summary>Y coordinates.</summary>
    public ImmutableArray<double> Y { get; init; }

    /// <summary>Stroke color as a hex string.</summary>
    public string Color { get; init; } = "#007ACC";

    /// <summary>Line width.</summary>
    public double LineWidth { get; init; } = 1.0;

    /// <summary>Line style.</summary>
    public LineStyle Style { get; init; }
}

/// <summary>A series of 2D scatter point data.</summary>
public sealed class Point2DSeries
{
    /// <summary>Series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>X coordinates.</summary>
    public ImmutableArray<double> X { get; init; }

    /// <summary>Y coordinates.</summary>
    public ImmutableArray<double> Y { get; init; }

    /// <summary>Marker color as a hex string.</summary>
    public string Color { get; init; } = "#E74C3C";

    /// <summary>Point size.</summary>
    public double PointSize { get; init; } = 5.0;

    /// <summary>Marker shape name.</summary>
    public string Marker { get; init; } = "circle";
}

/// <summary>A 2D text annotation on a plot.</summary>
public sealed class Annotation2D
{
    /// <summary>X coordinate of the annotation.</summary>
    public double X { get; init; }

    /// <summary>Y coordinate of the annotation.</summary>
    public double Y { get; init; }

    /// <summary>Annotation text.</summary>
    public string Text { get; init; } = "";

    /// <summary>Text color as a hex string.</summary>
    public string Color { get; init; } = "#000000";
}
