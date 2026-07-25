namespace MathVerse.Math.DataScience.DistributedAnalytics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Provides parallel computation of statistical metrics across datasets.
/// </summary>
public static class DistributedStatistics
{
    /// <summary>
    /// Computes column-level statistics (mean, std, min, max, etc.) for a single numeric column in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name to analyze.</param>
    /// <returns>A <see cref="ColumnStatistics"/> with the computed metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no numeric values are found in the column.</exception>
    public static ColumnStatistics ComputeColumnStatistics(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        var values = new ConcurrentBag<double>();

        Parallel.ForEach(
            ds.Rows,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            row =>
            {
                if (row.TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
                    values.Add(Convert.ToDouble(val));
            });

        double[] arr = values.ToArray();
        if (arr.Length == 0)
            throw new ArgumentException($"No numeric values found in column '{column}'.");

        Array.Sort(arr);

        double mean = ComputeMean(arr);
        double stdDev = ComputeStdDev(arr, mean);
        double q1 = ComputePercentile(arr, 25.0);
        double q3 = ComputePercentile(arr, 75.0);
        double skewness = ComputeSkewness(arr, mean, stdDev);
        double kurtosis = ComputeKurtosis(arr, mean, stdDev);

        return new ColumnStatistics
        {
            Mean = mean,
            Median = ComputePercentile(arr, 50.0),
            StdDev = stdDev,
            Min = arr[0],
            Max = arr[arr.Length - 1],
            Q1 = q1,
            Q3 = q3,
            Skewness = skewness,
            Kurtosis = kurtosis,
            MissingCount = ds.Rows.Count - arr.Length,
            DistinctCount = new HashSet<double>(arr).Count
        };
    }

    /// <summary>
    /// Computes a correlation matrix for multiple numeric columns in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="columns">The column names to include.</param>
    /// <returns>A 2D array representing the correlation matrix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="columns"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when fewer than 2 columns are provided.</exception>
    public static double[,] ComputeCorrelationMatrix(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length < 2)
            throw new ArgumentException("At least 2 columns are required.", nameof(columns));

        int n = columns.Length;
        double[][] columnValues = new double[n][];

        Parallel.For(0, n, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
        {
            var bag = new ConcurrentBag<double>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(columns[i], out object? val) && val is not null && IsNumeric(val))
                    bag.Add(Convert.ToDouble(val));
            }
            columnValues[i] = bag.ToArray();
        });

        int minLen = columnValues.Min(c => c.Length);
        if (minLen < 2)
            throw new ArgumentException("Insufficient data points for correlation computation.");

        double[,] matrix = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double corr = PearsonCorrelation(
                    columnValues[i].Take(minLen).ToArray(),
                    columnValues[j].Take(minLen).ToArray());
                matrix[i, j] = corr;
                matrix[j, i] = corr;
            }
        }

        return matrix;
    }

    /// <summary>
    /// Computes the covariance matrix for multiple numeric columns in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="columns">The column names to include.</param>
    /// <returns>A 2D array representing the covariance matrix.</returns>
    public static double[,] ComputeCovarianceMatrix(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length < 2)
            throw new ArgumentException("At least 2 columns are required.", nameof(columns));

        int n = columns.Length;
        double[][] columnValues = new double[n][];

        Parallel.For(0, n, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
        {
            var bag = new ConcurrentBag<double>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(columns[i], out object? val) && val is not null && IsNumeric(val))
                    bag.Add(Convert.ToDouble(val));
            }
            columnValues[i] = bag.ToArray();
        });

        int minLen = columnValues.Min(c => c.Length);
        double[,] matrix = new double[n, n];

        double[] means = new double[n];
        for (int i = 0; i < n; i++)
            means[i] = ComputeMean(columnValues[i].Take(minLen).ToArray());

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double cov = 0.0;
                for (int k = 0; k < minLen; k++)
                    cov += (columnValues[i][k] - means[i]) * (columnValues[j][k] - means[j]);
                cov /= (minLen - 1);
                matrix[i, j] = cov;
                matrix[j, i] = cov;
            }
        }

        return matrix;
    }

    private static double ComputeMean(double[] values)
    {
        double sum = 0.0;
        foreach (double v in values) sum += v;
        return sum / values.Length;
    }

    private static double ComputeStdDev(double[] values, double mean)
    {
        double sumSq = 0.0;
        foreach (double v in values)
        {
            double d = v - mean;
            sumSq += d * d;
        }
        return System.Math.Sqrt(sumSq / (values.Length - 1));
    }

    private static double ComputePercentile(double[] sorted, double percentile)
    {
        if (sorted.Length == 1) return sorted[0];
        double rank = (percentile / 100.0) * (sorted.Length - 1);
        int lower = (int)System.Math.Floor(rank);
        int upper = (int)System.Math.Ceiling(rank);
        if (lower == upper) return sorted[lower];
        double frac = rank - lower;
        return sorted[lower] + frac * (sorted[upper] - sorted[lower]);
    }

    private static double ComputeSkewness(double[] values, double mean, double stdDev)
    {
        if (stdDev < 1e-15) return 0.0;
        int n = values.Length;
        double sum = 0.0;
        foreach (double v in values)
        {
            double z = (v - mean) / stdDev;
            sum += z * z * z;
        }
        return sum / n;
    }

    private static double ComputeKurtosis(double[] values, double mean, double stdDev)
    {
        if (stdDev < 1e-15) return 0.0;
        int n = values.Length;
        double sum = 0.0;
        foreach (double v in values)
        {
            double z = (v - mean) / stdDev;
            sum += z * z * z * z;
        }
        return (sum / n) - 3.0;
    }

    private static double PearsonCorrelation(double[] x, double[] y)
    {
        int n = x.Length;
        double mx = ComputeMean(x);
        double my = ComputeMean(y);

        double sumXY = 0.0, sumX2 = 0.0, sumY2 = 0.0;
        for (int i = 0; i < n; i++)
        {
            double dx = x[i] - mx;
            double dy = y[i] - my;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        double denom = System.Math.Sqrt(sumX2 * sumY2);
        return denom < 1e-15 ? 0.0 : sumXY / denom;
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
