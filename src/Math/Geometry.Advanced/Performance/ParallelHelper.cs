using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace MathVerse.Math.Geometry.Advanced.Performance;

/// <summary>
/// Provides parallel execution helpers for geometry processing operations.
/// These utilities wrap System.Threading.Tasks.Parallel with convenient overloads
/// for common parallel iteration patterns, automatic partitioning, and configurable
/// maximum degree of parallelism.
/// </summary>
public static class ParallelHelper
{
    /// <summary>
    /// Executes a parallel for loop over the integer range [inclusiveStart, exclusiveEnd)
    /// with automatic partitioning. The range is divided into chunks that are processed
    /// concurrently across available processor cores.
    /// </summary>
    /// <param name="inclusiveStart">The inclusive lower bound of the iteration range.</param>
    /// <param name="exclusiveEnd">The exclusive upper bound of the iteration range.</param>
    /// <param name="body">The action to execute for each iteration index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when exclusiveEnd is less than inclusiveStart.</exception>
    /// <exception cref="ArgumentNullException">Thrown when body is null.</exception>
    public static void For(int inclusiveStart, int exclusiveEnd, Action<int> body)
    {
        if (exclusiveEnd < inclusiveStart)
            throw new ArgumentOutOfRangeException(nameof(exclusiveEnd));
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        int rangeSize = exclusiveEnd - inclusiveStart;
        if (rangeSize <= 0) return;

        int chunkSize = System.Math.Max(1, rangeSize / (Environment.ProcessorCount * 4));

        Parallel.ForEach(
            Partitioner.Create(inclusiveStart, exclusiveEnd, chunkSize),
            range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    body(i);
            });
    }

    /// <summary>
    /// Executes a parallel for loop over the integer range [inclusiveStart, exclusiveEnd)
    /// with a specified maximum degree of parallelism. This overload is useful for
    /// limiting resource consumption when running alongside other parallel workloads.
    /// </summary>
    /// <param name="inclusiveStart">The inclusive lower bound of the iteration range.</param>
    /// <param name="exclusiveEnd">The exclusive upper bound of the iteration range.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent tasks. Use -1 for unlimited parallelism.</param>
    /// <param name="body">The action to execute for each iteration index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when exclusiveEnd is less than inclusiveStart.</exception>
    /// <exception cref="ArgumentNullException">Thrown when body is null.</exception>
    public static void For(int inclusiveStart, int exclusiveEnd, int maxDegreeOfParallelism, Action<int> body)
    {
        if (exclusiveEnd < inclusiveStart)
            throw new ArgumentOutOfRangeException(nameof(exclusiveEnd));
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        int rangeSize = exclusiveEnd - inclusiveStart;
        if (rangeSize <= 0) return;

        int chunkSize = System.Math.Max(1, rangeSize / (maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism * 4 : Environment.ProcessorCount * 4));

        Parallel.ForEach(
            Partitioner.Create(inclusiveStart, exclusiveEnd, chunkSize),
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    body(i);
            });
    }

    /// <summary>
    /// Executes a parallel foreach loop over an immutable array of items, processing each
    /// element with the specified action. The items are partitioned automatically for
    /// efficient concurrent execution across available cores.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="items">The immutable array of items to process in parallel.</param>
    /// <param name="body">The action to execute for each item.</param>
    /// <exception cref="ArgumentNullException">Thrown when body is null.</exception>
    public static void ForEach<T>(ImmutableArray<T> items, Action<T> body)
    {
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        if (items.Length == 0) return;

        int chunkSize = System.Math.Max(1, items.Length / (Environment.ProcessorCount * 4));

        Parallel.ForEach(
            Partitioner.Create(0, items.Length, chunkSize),
            range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    body(items[i]);
            });
    }

    /// <summary>
    /// Applies a mapping function to each element of the input array in parallel, producing
    /// a new output array with the transformed values. The mapping function is invoked
    /// concurrently for each element, and results are collected into the output array.
    /// </summary>
    /// <typeparam name="TIn">The type of elements in the input array.</typeparam>
    /// <typeparam name="TOut">The type of elements in the output array.</typeparam>
    /// <param name="input">The input array to transform.</param>
    /// <param name="map">The mapping function to apply to each element.</param>
    /// <returns>A new array containing the mapped results in the same order as the input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input or map is null.</exception>
    public static TOut[] Map<TIn, TOut>(TIn[] input, Func<TIn, TOut> map)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (map == null)
            throw new ArgumentNullException(nameof(map));

        if (input.Length == 0)
            return Array.Empty<TOut>();

        TOut[] output = new TOut[input.Length];
        int chunkSize = System.Math.Max(1, input.Length / (Environment.ProcessorCount * 4));

        Parallel.ForEach(
            Partitioner.Create(0, input.Length, chunkSize),
            range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    output[i] = map(input[i]);
            });

        return output;
    }
}
