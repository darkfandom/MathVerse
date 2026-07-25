namespace MathVerse.Math.DataScience.DistributedAnalytics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Provides distributed aggregation operations for datasets including group-by and column aggregation.
/// </summary>
public static class DistributedAggregator
{
    /// <summary>
    /// Groups dataset rows by a column value in parallel, then applies an aggregation function to each group.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name to group by.</param>
    /// <param name="agg">The aggregation function applied to each group as a sub-dataset.</param>
    /// <returns>A new dataset containing the aggregated results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/>, <paramref name="column"/>, or <paramref name="agg"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the column does not exist in the dataset.</exception>
    public static Dataset GroupByParallel(Dataset ds, string column, Func<Dataset, Dataset> agg)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));
        if (agg is null) throw new ArgumentNullException(nameof(agg));
        if (ds.Count == 0) throw new ArgumentException("Dataset is empty.", nameof(ds));

        var groups = new Dictionary<string, List<Dictionary<string, object?>>>();

        foreach (var row in ds.Rows)
        {
            string key = row.TryGetValue(column, out object? val) && val is not null
                ? val.ToString() ?? "null"
                : "null";

            if (!groups.ContainsKey(key))
                groups[key] = new List<Dictionary<string, object?>>();

            groups[key].Add(row);
        }

        var result = new Dataset
        {
            Name = $"{ds.Name}_grouped_{column}",
            Schema = ds.Schema
        };

        var partitioner = Partitioner.Create(groups.ToList(), true);

        var bag = new ConcurrentBag<Dictionary<string, object?>>();

        Parallel.ForEach(
            partitioner,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            kvp =>
            {
                var groupDs = new Dataset
                {
                    Name = ds.Name,
                    Schema = ds.Schema
                };
                groupDs.Rows.AddRange(kvp.Value);

                Dataset aggregated = agg(groupDs);

                if (aggregated.Rows.Count > 0)
                {
                    foreach (var row in aggregated.Rows)
                        bag.Add(row);
                }
            });

        result.Rows.AddRange(bag);
        return result;
    }

    /// <summary>
    /// Aggregates values in a numeric column across all rows using a custom aggregation function.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name containing numeric values.</param>
    /// <param name="aggFunc">The aggregation function that takes an array of doubles and returns a single value.</param>
    /// <returns>The aggregated result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="aggFunc"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no numeric values are found in the column.</exception>
    public static double AggregateParallel(Dataset ds, string column, Func<double[], double> aggFunc)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (aggFunc is null) throw new ArgumentNullException(nameof(aggFunc));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        var values = new ConcurrentBag<double>();

        Parallel.ForEach(
            ds.Rows,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            row =>
            {
                if (row.TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
                {
                    values.Add(Convert.ToDouble(val));
                }
            });

        if (values.Count == 0)
            throw new ArgumentException($"No numeric values found in column '{column}'.");

        return aggFunc(values.ToArray());
    }

    /// <summary>
    /// Computes the sum of a numeric column in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name.</param>
    /// <returns>The sum of all numeric values in the column.</returns>
    public static double SumParallel(Dataset ds, string column)
    {
        return AggregateParallel(ds, column, vals => {
            double sum = 0.0;
            foreach (double v in vals) sum += v;
            return sum;
        });
    }

    /// <summary>
    /// Computes the mean of a numeric column in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name.</param>
    /// <returns>The mean of all numeric values in the column.</returns>
    public static double MeanParallel(Dataset ds, string column)
    {
        return AggregateParallel(ds, column, vals =>
        {
            double sum = 0.0;
            foreach (double v in vals) sum += v;
            return sum / vals.Length;
        });
    }

    /// <summary>
    /// Counts the occurrences of each distinct value in a column in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column name.</param>
    /// <returns>A dictionary mapping distinct values to their counts.</returns>
    public static Dictionary<string, int> ValueCountsParallel(Dataset ds, string column)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

        var counts = new ConcurrentDictionary<string, int>();

        Parallel.ForEach(
            ds.Rows,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            row =>
            {
                string key = row.TryGetValue(column, out object? val) && val is not null
                    ? val.ToString() ?? "null"
                    : "null";
                counts.AddOrUpdate(key, 1, (_, c) => c + 1);
            });

        return new Dictionary<string, int>(counts);
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
