namespace MathVerse.Math.DataScience.DistributedAnalytics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Provides parallel column transformation operations for datasets.
/// </summary>
public static class ParallelTransformation
{
    /// <summary>
    /// Applies a transformation function to specified columns in parallel.
    /// Each column is transformed independently across all rows.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="cols">The column names to transform.</param>
    /// <param name="func">The transformation function applied to each numeric value.</param>
    /// <returns>The modified dataset with transformed column values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/>, <paramref name="cols"/>, or <paramref name="func"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the columns array is empty.</exception>
    public static Dataset TransformColumns(Dataset ds, string[] cols, Func<double, double> func)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));
        if (func is null) throw new ArgumentNullException(nameof(func));

        Parallel.For(0, cols.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, colIdx =>
        {
            string col = cols[colIdx];
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    row[col] = func(Convert.ToDouble(val));
                }
            }
        });

        return ds;
    }

    /// <summary>
    /// Creates new columns by applying transformation functions in parallel.
    /// Does not modify existing columns; instead creates new ones.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="transforms">A dictionary mapping new column names to (source column, function) tuples.</param>
    /// <returns>The modified dataset with new columns added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="transforms"/> is null.</exception>
    public static Dataset AddTransformedColumns(Dataset ds, Dictionary<string, (string SourceColumn, Func<double, double> Transform)> transforms)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (transforms is null) throw new ArgumentNullException(nameof(transforms));

        foreach (var kvp in transforms)
        {
            string newColName = kvp.Key;
            string sourceCol = kvp.Value.SourceColumn;
            Func<double, double> transform = kvp.Value.Transform;

            if (!ds.Schema.HasColumn(sourceCol))
                throw new ArgumentException($"Source column '{sourceCol}' does not exist in the dataset.");

            ds.Schema.AddColumn(newColName, ColumnType.Double);
        }

        var entries = new List<KeyValuePair<string, (string SourceColumn, Func<double, double> Transform)>>(transforms);

        Parallel.For(0, entries.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, entryIdx =>
        {
            var entry = entries[entryIdx];
            string newColName = entry.Key;
            string sourceCol = entry.Value.SourceColumn;
            Func<double, double> transform = entry.Value.Transform;

            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(sourceCol, out object? val) && val is not null && IsNumeric(val))
                {
                    row[newColName] = transform(Convert.ToDouble(val));
                }
                else
                {
                    row[newColName] = null;
                }
            }
        });

        return ds;
    }

    /// <summary>
    /// Normalizes a column in parallel using z-score normalization: (x - mean) / stdDev.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column to normalize.</param>
    /// <returns>The modified dataset with the normalized column.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    public static Dataset ZScoreNormalize(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        var values = new ConcurrentBag<double>();
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
                values.Add(Convert.ToDouble(val));
        }

        double[] arr = values.ToArray();
        if (arr.Length == 0) return ds;

        double mean = 0.0;
        foreach (double v in arr) mean += v;
        mean /= arr.Length;

        double variance = 0.0;
        foreach (double v in arr)
        {
            double d = v - mean;
            variance += d * d;
        }
        double stdDev = System.Math.Sqrt(variance / arr.Length);
        if (stdDev < 1e-15) stdDev = 1.0;

        TransformColumns(ds, new[] { column }, x => (x - mean) / stdDev);
        return ds;
    }

    /// <summary>
    /// Min-Max normalizes a column in parallel: (x - min) / (max - min).
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column to normalize.</param>
    /// <returns>The modified dataset with the normalized column.</returns>
    public static Dataset MinMaxNormalize(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        double min = double.MaxValue;
        double max = double.MinValue;
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
            {
                double d = Convert.ToDouble(val);
                if (d < min) min = d;
                if (d > max) max = d;
            }
        }

        double range = max - min;
        if (range < 1e-15) range = 1.0;

        TransformColumns(ds, new[] { column }, x => (x - min) / range);
        return ds;
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
