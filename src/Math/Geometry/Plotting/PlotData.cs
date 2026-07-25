using System.Collections.Immutable;
using MathVerse.Math.Geometry.Colors;

namespace MathVerse.Math.Geometry.Plotting;

/// <summary>Represents data for a line plot.</summary>
/// <param name="Label">The label for this line series.</param>
/// <param name="LineColor">The color of the line.</param>
/// <param name="Points">The data points.</param>
/// <param name="LineWidth">The width of the line.</param>
/// <param name="Style">The line style.</param>
public record LinePlotData(
    string Label,
    Color LineColor,
    ImmutableArray<(double X, double Y)> Points,
    double LineWidth = 1.0,
    PlotLineStyle Style = PlotLineStyle.Solid);

/// <summary>Represents data for a scatter plot.</summary>
/// <param name="Label">The label for this scatter series.</param>
/// <param name="MarkerColor">The color of the markers.</param>
/// <param name="Points">The data points.</param>
/// <param name="MarkerSize">The size of the markers.</param>
/// <param name="Marker">The marker type.</param>
public record ScatterPlotData(
    string Label,
    Color MarkerColor,
    ImmutableArray<(double X, double Y)> Points,
    double MarkerSize = 6.0,
    ScatterMarkerType Marker = ScatterMarkerType.Circle);

/// <summary>Represents data for a bar plot.</summary>
/// <param name="Label">The label for this bar series.</param>
/// <param name="BarColor">The color of the bars.</param>
/// <param name="Bars">The bar data (X = position, Y = height).</param>
public record BarPlotData(
    string Label,
    Color BarColor,
    ImmutableArray<(double X, double Y)> Bars);

/// <summary>Represents data for a histogram plot.</summary>
/// <param name="Label">The label for this histogram.</param>
/// <param name="BarColor">The color of the bars.</param>
/// <param name="Values">The raw values to bin.</param>
/// <param name="BinCount">The number of bins.</param>
public record HistogramPlotData(
    string Label,
    Color BarColor,
    ImmutableArray<double> Values,
    int BinCount = 20);

/// <summary>Represents data for a contour plot.</summary>
/// <param name="F">The function to contour.</param>
/// <param name="XMin">The minimum X value.</param>
/// <param name="XMax">The maximum X value.</param>
/// <param name="YMin">The minimum Y value.</param>
/// <param name="YMax">The maximum Y value.</param>
/// <param name="Levels">The number of contour levels.</param>
public record ContourPlotData(
    Func<double, double, double> F,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    int Levels);

/// <summary>Represents data for a vector field plot.</summary>
/// <param name="Field">The vector field function.</param>
/// <param name="XMin">The minimum X value.</param>
/// <param name="XMax">The maximum X value.</param>
/// <param name="YMin">The minimum Y value.</param>
/// <param name="YMax">The maximum Y value.</param>
/// <param name="GridRes">The grid resolution.</param>
public record VectorFieldData(
    Func<double, double, (double, double)> Field,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    int GridRes);

/// <summary>Represents data for a surface plot.</summary>
/// <param name="F">The surface function.</param>
/// <param name="XMin">The minimum X value.</param>
/// <param name="XMax">The maximum X value.</param>
/// <param name="YMin">The minimum Y value.</param>
/// <param name="YMax">The maximum Y value.</param>
/// <param name="Resolution">The grid resolution.</param>
public record SurfacePlotData(
    Func<double, double, double> F,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    int Resolution);
