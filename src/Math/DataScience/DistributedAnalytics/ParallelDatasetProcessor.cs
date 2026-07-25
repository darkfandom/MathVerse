namespace MathVerse.Math.DataScience.DistributedAnalytics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Provides parallel processing operations for datasets including map and filter.
/// </summary>
public static class ParallelDatasetProcessor
{
    /// <summary>
    /// Applies a transformation function to each row of the dataset in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="func">The transformation function applied to each row.</param>
    /// <returns>A new dataset with transformed rows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="func"/> is null.</exception>
    public static Dataset MapParallel(Dataset ds, Func<Dictionary<string, object?>, Dictionary<string, object?>> func)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (func is null) throw new ArgumentNullException(nameof(func));

        var result = new Dataset
        {
            Name = ds.Name,
            Metadata = ds.Metadata,
            Schema = ds.Schema
        };

        var bag = new ConcurrentBag<Dictionary<string, object?>>();
        int batchSize = System.Math.Max(1, ds.Rows.Count / Environment.ProcessorCount);

        Parallel.ForEach(
            ds.Rows,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            () => new List<Dictionary<string, object?>>(),
            (row, state, localList) =>
            {
                Dictionary<string, object?> transformed = func(row);
                localList.Add(transformed);
                return localList;
            },
            localList =>
            {
                foreach (var item in localList)
                    bag.Add(item);
            });

        result.Rows.AddRange(bag);
        return result;
    }

    /// <summary>
    /// Filters rows in parallel using a predicate function.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="predicate">The filter predicate applied to each row.</param>
    /// <returns>A new dataset containing only the matching rows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="predicate"/> is null.</exception>
    public static Dataset FilterParallel(Dataset ds, Func<Dictionary<string, object?>, bool> predicate)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        var result = new Dataset
        {
            Name = ds.Name,
            Metadata = ds.Metadata,
            Schema = ds.Schema
        };

        var bag = new ConcurrentBag<Dictionary<string, object?>>();

        Parallel.ForEach(
            ds.Rows,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            row =>
            {
                if (predicate(row))
                    bag.Add(row);
            });

        result.Rows.AddRange(bag);
        return result;
    }

    /// <summary>
    /// Partitions a dataset into chunks for parallel processing.
    /// </summary>
    /// <param name="ds">The dataset to partition.</param>
    /// <param name="maxPartitions">The maximum number of partitions. If zero, uses the processor count.</param>
    /// <returns>A list of dataset partitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    public static List<Dataset> Partition(Dataset ds, int maxPartitions = 0)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        int partitions = maxPartitions > 0 ? maxPartitions : Environment.ProcessorCount;
        int chunkSize = System.Math.Max(1, ds.Rows.Count / partitions);
        var result = new List<Dataset>();

        for (int i = 0; i < ds.Rows.Count; i += chunkSize)
        {
            var chunk = new Dataset
            {
                Name = $"{ds.Name}_part{result.Count}",
                Metadata = ds.Metadata,
                Schema = ds.Schema
            };

            int end = System.Math.Min(i + chunkSize, ds.Rows.Count);
            for (int j = i; j < end; j++)
                chunk.Rows.Add(ds.Rows[j]);

            result.Add(chunk);
        }

        return result;
    }

    /// <summary>
    /// Applies a map operation to each row in parallel, returning a flat list of results.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="selector">The selector function for each row.</param>
    /// <returns>An array of selected values.</returns>
    /// <typeparam name="T">The type of the selected values.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="selector"/> is null.</exception>
    public static T[] SelectParallel<T>(Dataset ds, Func<Dictionary<string, object?>, T> selector)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (selector is null) throw new ArgumentNullException(nameof(selector));

        T[] results = new T[ds.Rows.Count];

        Parallel.For(0, ds.Rows.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            i =>
            {
                results[i] = selector(ds.Rows[i]);
            });

        return results;
    }
}
