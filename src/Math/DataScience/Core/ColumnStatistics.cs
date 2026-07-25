namespace MathVerse.Math.DataScience.Core;

/// <summary>
/// Statistical properties of a single column.
/// </summary>
public sealed class ColumnStatistics
{
    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    public double Mean { get; set; }

    /// <summary>
    /// Gets or sets the median value.
    /// </summary>
    public double Median { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation.
    /// </summary>
    public double StdDev { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets the first quartile.
    /// </summary>
    public double Q1 { get; set; }

    /// <summary>
    /// Gets or sets the third quartile.
    /// </summary>
    public double Q3 { get; set; }

    /// <summary>
    /// Gets or sets the skewness.
    /// </summary>
    public double Skewness { get; set; }

    /// <summary>
    /// Gets or sets the kurtosis.
    /// </summary>
    public double Kurtosis { get; set; }

    /// <summary>
    /// Gets or sets the number of missing values.
    /// </summary>
    public int MissingCount { get; set; }

    /// <summary>
    /// Gets or sets the number of distinct values.
    /// </summary>
    public int DistinctCount { get; set; }
}