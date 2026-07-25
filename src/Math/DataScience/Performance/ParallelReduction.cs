namespace MathVerse.Math.DataScience.Performance;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Provides parallel reduction operations over datasets for high-performance aggregation.
/// </summary>
public sealed class ParallelReduction
{
    /// <summary>
    /// Computes the sum of a numeric column using parallel reduction.
    /// </summary>
    /// <param name="ds">The dataset to aggregate.</param>
    /// <param name="column">The column name to sum.</param>
    /// <returns>The sum of all numeric values in the column.</returns>
    public static double ColumnSum(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        double[] values = ExtractNumericColumn(ds, column);
        if (values.Length == 0) return 0.0;

        if (values.Length < 1024)
        {
            double sum = 0.0;
            for (int i = 0; i < values.Length; i++) sum += values[i];
            return sum;
        }

        int coreCount = System.Environment.ProcessorCount;
        int chunkSize = (values.Length + coreCount - 1) / coreCount;
        double[] partialSums = new double[coreCount];

        Parallel.For(0, coreCount, delegate (int workerIndex)
        {
            int start = workerIndex * chunkSize;
            int end = System.Math.Min(start + chunkSize, values.Length);
            double localSum = 0.0;
            for (int i = start; i < end; i++)
            {
                localSum += values[i];
            }
            partialSums[workerIndex] = localSum;
        });

        double total = 0.0;
        for (int i = 0; i < coreCount; i++) total += partialSums[i];
        return total;
    }

    /// <summary>
    /// Computes the mean of a numeric column using parallel reduction.
    /// </summary>
    /// <param name="ds">The dataset to aggregate.</param>
    /// <param name="column">The column name to average.</param>
    /// <returns>The arithmetic mean of the column values.</returns>
    public static double ColumnMean(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        double[] values = ExtractNumericColumn(ds, column);
        if (values.Length == 0) return 0.0;

        double sum = 0.0;
        if (values.Length >= 1024)
        {
            int coreCount = System.Environment.ProcessorCount;
            int chunkSize = (values.Length + coreCount - 1) / coreCount;
            double[] partialSums = new double[coreCount];

            Parallel.For(0, coreCount, delegate (int workerIndex)
            {
                int start = workerIndex * chunkSize;
                int end = System.Math.Min(start + chunkSize, values.Length);
                double localSum = 0.0;
                for (int i = start; i < end; i++) localSum += values[i];
                partialSums[workerIndex] = localSum;
            });

            for (int i = 0; i < coreCount; i++) sum += partialSums[i];
        }
        else
        {
            for (int i = 0; i < values.Length; i++) sum += values[i];
        }

        return sum / values.Length;
    }

    /// <summary>
    /// Groups rows by a key column and applies an aggregation function to a value column.
    /// </summary>
    /// <param name="ds">The dataset to aggregate.</param>
    /// <param name="groupCol">The column to group by.</param>
    /// <param name="aggCol">The column to aggregate.</param>
    /// <param name="agg">The aggregation function that receives an array of values and returns a single result.</param>
    /// <returns>A dictionary mapping group keys to aggregated values.</returns>
    public static Dictionary<string, double> GroupAggregate(Dataset ds, string groupCol, string aggCol, Func<double[], double> agg)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(groupCol)) throw new ArgumentException("Group column name cannot be null or empty.", nameof(groupCol));
        if (string.IsNullOrEmpty(aggCol)) throw new ArgumentException("Aggregation column name cannot be null or empty.", nameof(aggCol));
        if (agg is null) throw new ArgumentNullException(nameof(agg));

        Dictionary<string, List<double>> groups = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            string? groupKey = null;
            if (row.TryGetValue(groupCol, out object? gVal) && gVal is not null)
            {
                groupKey = gVal.ToString();
            }

            if (groupKey is null) continue;

            if (row.TryGetValue(aggCol, out object? aVal) && aVal is not null && IsNumeric(aVal))
            {
                if (!groups.ContainsKey(groupKey))
                    groups[groupKey] = new List<double>();
                groups[groupKey].Add(Convert.ToDouble(aVal));
            }
        }

        Dictionary<string, double> results = new(groups.Count);
        object lockObj = new();

        Parallel.ForEach(groups, delegate (KeyValuePair<string, List<double>> kvp)
        {
            double result = agg(kvp.Value.ToArray());
            lock (lockObj)
            {
                results[kvp.Key] = result;
            }
        });

        return results;
    }

    /// <summary>
    /// Computes the standard deviation of a numeric column using parallel reduction.
    /// </summary>
    /// <param name="ds">The dataset to aggregate.</param>
    /// <param name="column">The column name.</param>
    /// <returns>The population standard deviation.</returns>
    public static double ColumnStdDev(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        double[] values = ExtractNumericColumn(ds, column);
        if (values.Length < 2) return 0.0;

        double mean = 0.0;
        for (int i = 0; i < values.Length; i++) mean += values[i];
        mean /= values.Length;

        double m2 = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            double diff = values[i] - mean;
            m2 += diff * diff;
        }

        return System.Math.Sqrt(m2 / values.Length);
    }

    private static double[] ExtractNumericColumn(Dataset ds, string column)
    {
        List<double> values = new();
        for (int i = 0; i < ds.Count; i++)
        {
            if (ds.Rows[i].TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
            {
                values.Add(Convert.ToDouble(val));
            }
        }
        return values.ToArray();
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
