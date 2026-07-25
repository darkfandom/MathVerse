using System.Collections.Immutable;
using MathVerse.Math.Geometry.Colors;

namespace MathVerse.Math.Geometry.Charts;

/// <summary>Represents a data series for charts.</summary>
/// <param name="Name">The series name.</param>
/// <param name="Color">The series color.</param>
/// <param name="Points">The data points.</param>
public record Series(
    string Name,
    Color Color,
    ImmutableArray<(double X, double Y)> Points);

/// <summary>Represents a slice of a pie chart.</summary>
/// <param name="Label">The slice label.</param>
/// <param name="Value">The slice value.</param>
/// <param name="Color">The slice color.</param>
public record PieSlice(
    string Label,
    double Value,
    Color Color);

/// <summary>Represents data for a box plot.</summary>
/// <param name="Label">The box plot label.</param>
/// <param name="Min">The minimum value.</param>
/// <param name="Q1">The first quartile.</param>
/// <param name="Median">The median value.</param>
/// <param name="Q3">The third quartile.</param>
/// <param name="Max">The maximum value.</param>
/// <param name="Outliers">The outlier values.</param>
public record BoxPlotData(
    string Label,
    double Min,
    double Q1,
    double Median,
    double Q3,
    double Max,
    ImmutableArray<double> Outliers);

/// <summary>Represents data for a candlestick chart.</summary>
/// <param name="Date">The date of the data point.</param>
/// <param name="Open">The opening price.</param>
/// <param name="High">The highest price.</param>
/// <param name="Low">The lowest price.</param>
/// <param name="Close">The closing price.</param>
public record CandlestickData(
    DateTime Date,
    double Open,
    double High,
    double Low,
    double Close);
