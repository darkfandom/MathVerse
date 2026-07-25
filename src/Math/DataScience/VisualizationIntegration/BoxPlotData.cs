namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the computed statistics for a box plot visualization.
/// </summary>
public sealed class BoxPlotData
{
    /// <summary>
    /// Gets or sets the label identifying this box plot.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first quartile (25th percentile).
    /// </summary>
    public double Q1 { get; set; }

    /// <summary>
    /// Gets or sets the second quartile (median, 50th percentile).
    /// </summary>
    public double Median { get; set; }

    /// <summary>
    /// Gets or sets the third quartile (75th percentile).
    /// </summary>
    public double Q3 { get; set; }

    /// <summary>
    /// Gets or sets the interquartile range (Q3 - Q1).
    /// </summary>
    public double IQR { get; set; }

    /// <summary>
    /// Gets or sets the lower whisker value (minimum non-outlier).
    /// </summary>
    public double LowerWhisker { get; set; }

    /// <summary>
    /// Gets or sets the upper whisker value (maximum non-outlier).
    /// </summary>
    public double UpperWhisker { get; set; }

    /// <summary>
    /// Gets or sets the outlier values outside the whisker range.
    /// </summary>
    public List<double> Outliers { get; set; } = new();

    /// <summary>
    /// Computes box plot data from an array of values using the IQR method.
    /// </summary>
    /// <param name="data">The data values to compute statistics from.</param>
    /// <param name="label">The label for this box plot.</param>
    /// <param name="whiskerFactor">The IQR multiplier for whisker bounds (default 1.5).</param>
    /// <returns>A new <see cref="BoxPlotData"/> instance with computed statistics.</returns>
    public static BoxPlotData Compute(double[] data, string label = "", double whiskerFactor = 1.5)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) throw new ArgumentException("Data array cannot be empty.", nameof(data));

        double[] sorted = (double[])data.Clone();
        Array.Sort(sorted);

        double q1 = Percentile(sorted, 25.0);
        double median = Percentile(sorted, 50.0);
        double q3 = Percentile(sorted, 75.0);
        double iqr = q3 - q1;

        double lowerBound = q1 - whiskerFactor * iqr;
        double upperBound = q3 + whiskerFactor * iqr;

        double lowerWhisker = sorted[0];
        for (int i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] >= lowerBound)
            {
                lowerWhisker = sorted[i];
                break;
            }
        }

        double upperWhisker = sorted[^1];
        for (int i = sorted.Length - 1; i >= 0; i--)
        {
            if (sorted[i] <= upperBound)
            {
                upperWhisker = sorted[i];
                break;
            }
        }

        List<double> outliers = new();
        for (int i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] < lowerBound || sorted[i] > upperBound)
            {
                outliers.Add(sorted[i]);
            }
        }

        return new BoxPlotData
        {
            Label = label,
            Q1 = q1,
            Median = median,
            Q3 = q3,
            IQR = iqr,
            LowerWhisker = lowerWhisker,
            UpperWhisker = upperWhisker,
            Outliers = outliers
        };
    }

    /// <summary>
    /// Computes the value at a given percentile using linear interpolation.
    /// </summary>
    /// <param name="sortedValues">The sorted data array.</param>
    /// <param name="percentile">The percentile value (0-100).</param>
    /// <returns>The interpolated value at the specified percentile.</returns>
    public static double Percentile(double[] sortedValues, double percentile)
    {
        if (sortedValues.Length == 0) throw new ArgumentException("Array cannot be empty.", nameof(sortedValues));
        if (sortedValues.Length == 1) return sortedValues[0];

        double index = (percentile / 100.0) * (sortedValues.Length - 1);
        int lower = (int)System.Math.Floor(index);
        int upper = (int)System.Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        double fraction = index - lower;
        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }
}
