namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Computes per-column statistics for datasets.
/// </summary>
public sealed class DatasetStatistics
{
    /// <summary>
    /// Computes statistics for all numeric columns in the dataset.
    /// </summary>
    /// <param name="dataset">The dataset to compute statistics for.</param>
    /// <returns>A dictionary mapping column names to their statistics.</returns>
    public Dictionary<string, ColumnStatistics> Compute(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var result = new Dictionary<string, ColumnStatistics>();

        foreach (var colDef in dataset.Schema.Columns)
        {
            var values = dataset.GetColumn(colDef.Name);
            var stats = new ColumnStatistics
            {
                MissingCount = values.Count(v => v == null),
                DistinctCount = values.Where(v => v != null).Select(v => v!.ToString()).Distinct().Count()
            };

            var numericValues = values
                .Where(v => v != null && double.TryParse(v?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                .Select(v => double.Parse(v!.ToString()!, NumberStyles.Any, CultureInfo.InvariantCulture))
                .OrderBy(v => v)
                .ToList();

            if (numericValues.Count > 0)
            {
                stats.Mean = numericValues.Average();
                stats.Min = numericValues[0];
                stats.Max = numericValues[^1];
                stats.Median = ComputeMedian(numericValues);

                double variance = numericValues.Sum(v => (v - stats.Mean) * (v - stats.Mean)) / System.Math.Max(1, numericValues.Count - 1);
                stats.StdDev = System.Math.Sqrt(variance);

                stats.Q1 = ComputePercentile(numericValues, 25.0);
                stats.Q3 = ComputePercentile(numericValues, 75.0);
                stats.Skewness = ComputeSkewness(numericValues, stats.Mean, stats.StdDev);
                stats.Kurtosis = ComputeKurtosis(numericValues, stats.Mean, stats.StdDev);
            }

            result[colDef.Name] = stats;
        }

        return result;
    }

    private static double ComputeMedian(List<double> sortedValues)
    {
        int n = sortedValues.Count;
        if (n % 2 == 0)
        {
            return (sortedValues[n / 2 - 1] + sortedValues[n / 2]) / 2.0;
        }
        return sortedValues[n / 2];
    }

    private static double ComputePercentile(List<double> sortedValues, double percentile)
    {
        double index = (percentile / 100.0) * (sortedValues.Count - 1);
        int lower = (int)System.Math.Floor(index);
        int upper = (int)System.Math.Ceiling(index);

        if (lower == upper)
        {
            return sortedValues[lower];
        }

        double fraction = index - lower;
        return sortedValues[lower] * (1.0 - fraction) + sortedValues[upper] * fraction;
    }

    private static double ComputeSkewness(List<double> values, double mean, double stdDev)
    {
        if (stdDev < 1e-10 || values.Count < 3) return 0.0;
        int n = values.Count;
        double sum = values.Sum(v => System.Math.Pow((v - mean) / stdDev, 3));
        return sum * n / ((n - 1) * (n - 2));
    }

    private static double ComputeKurtosis(List<double> values, double mean, double stdDev)
    {
        if (stdDev < 1e-10 || values.Count < 4) return 0.0;
        int n = values.Count;
        double sum = values.Sum(v => System.Math.Pow((v - mean) / stdDev, 4));
        return (sum * n * (n + 1) / ((n - 1) * (n - 2) * (n - 3))) - (3.0 * (n - 1) * (n - 1) / ((n - 2) * (n - 3)));
    }
}