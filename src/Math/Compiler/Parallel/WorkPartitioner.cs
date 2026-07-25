namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Generic;

/// <summary>
/// Partitions work items across available processor cores.
/// Provides methods to split a total workload into balanced ranges
/// that can be processed in parallel.
/// </summary>
public sealed class WorkPartitioner
{
    private readonly int _processorCount;

    /// <summary>
    /// Initializes the work partitioner.
    /// </summary>
    /// <param name="processorCount">Number of available processors. Default is <see cref="Environment.ProcessorCount"/>.</param>
    public WorkPartitioner(int processorCount = 0)
    {
        _processorCount = processorCount > 0 ? processorCount : Environment.ProcessorCount;
    }

    /// <summary>
    /// Gets the number of processors used for partitioning.
    /// </summary>
    public int ProcessorCount => _processorCount;

    /// <summary>
    /// Partitions total work items into ranges across available cores.
    /// </summary>
    /// <param name="totalWork">Total number of work items to partition.</param>
    /// <param name="maxPartitions">Maximum number of partitions. Use 0 for unlimited.</param>
    /// <returns>An immutable array of ranges representing the partitions.</returns>
    public IReadOnlyList<Range> Partition(int totalWork, int maxPartitions = 0)
    {
        if (totalWork <= 0)
            return Array.Empty<Range>();

        var numPartitions = maxPartitions > 0
            ? Math.Min(_processorCount, maxPartitions)
            : _processorCount;

        numPartitions = Math.Min(numPartitions, totalWork);

        if (numPartitions <= 1)
            return new[] { new Range(0, totalWork) };

        return PartitionEvenly(totalWork, numPartitions);
    }

    /// <summary>
    /// Partitions work with a minimum chunk size, merging small partitions.
    /// </summary>
    /// <param name="totalWork">Total number of work items.</param>
    /// <param name="minChunkSize">Minimum work items per partition.</param>
    /// <param name="maxPartitions">Maximum number of partitions.</param>
    /// <returns>An immutable array of ranges.</returns>
    public IReadOnlyList<Range> PartitionWithMinChunk(
        int totalWork,
        int minChunkSize,
        int maxPartitions = 0)
    {
        if (totalWork <= 0)
            return Array.Empty<Range>();

        if (minChunkSize <= 0)
            return Partition(totalWork, maxPartitions);

        var idealPartitions = (totalWork + minChunkSize - 1) / minChunkSize;
        var actualPartitions = maxPartitions > 0
            ? Math.Min(idealPartitions, maxPartitions)
            : idealPartitions;

        actualPartitions = Math.Max(1, Math.Min(actualPartitions, _processorCount));
        actualPartitions = Math.Min(actualPartitions, totalWork);

        return PartitionEvenly(totalWork, actualPartitions);
    }

    /// <summary>
    /// Partitions work items by weight, distributing heavier work items
    /// more evenly across partitions.
    /// </summary>
    /// <param name="weights">Weight of each work item.</param>
    /// <param name="maxPartitions">Maximum number of partitions.</param>
    /// <returns>An immutable array of index ranges into the weights array.</returns>
    public IReadOnlyList<Range> PartitionByWeight(
        IReadOnlyList<double> weights,
        int maxPartitions = 0)
    {
        if (weights.Count == 0)
            return Array.Empty<Range>();

        var numPartitions = maxPartitions > 0
            ? Math.Min(_processorCount, maxPartitions)
            : _processorCount;

        numPartitions = Math.Min(numPartitions, weights.Count);

        if (numPartitions <= 1)
            return new[] { new Range(0, weights.Count) };

        var totalWeight = 0.0;
        foreach (var w in weights)
            totalWeight += w;

        if (totalWeight <= 0)
            return PartitionEvenly(weights.Count, numPartitions);

        var targetWeightPerPartition = totalWeight / numPartitions;
        var partitions = new List<Range>();

        var currentStart = 0;
        var currentWeight = 0.0;

        for (var i = 0; i < weights.Count && partitions.Count < numPartitions - 1; i++)
        {
            currentWeight += weights[i];

            if (currentWeight >= targetWeightPerPartition && i + 1 > currentStart)
            {
                partitions.Add(new Range(currentStart, i + 1));
                currentStart = i + 1;
                currentWeight = 0;
            }
        }

        if (currentStart < weights.Count)
            partitions.Add(new Range(currentStart, weights.Count));

        return partitions;
    }

    /// <summary>
    /// Partitions a range into chunks of approximately equal size.
    /// </summary>
    /// <param name="start">Inclusive start index.</param>
    /// <param name="end">Exclusive end index.</param>
    /// <param name="chunkSize">Desired chunk size.</param>
    /// <returns>An immutable array of ranges.</returns>
    public IReadOnlyList<Range> Chunk(int start, int end, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

        var total = end - start;
        if (total <= 0)
            return Array.Empty<Range>();

        var chunks = new List<Range>();
        var current = start;

        while (current < end)
        {
            var chunkEnd = Math.Min(current + chunkSize, end);
            chunks.Add(new Range(current, chunkEnd));
            current = chunkEnd;
        }

        return chunks;
    }

    /// <summary>
    /// Estimates the optimal number of partitions based on work item count
    /// and estimated cost per item.
    /// </summary>
    /// <param name="totalWork">Total number of work items.</param>
    /// <param name="estimatedCostPerItem">Estimated computational cost per work item.</param>
    /// <returns>The recommended number of partitions.</returns>
    public int EstimateOptimalPartitions(int totalWork, double estimatedCostPerItem = 1.0)
    {
        var totalCost = totalWork * estimatedCostPerItem;
        var minCostPerCore = 100.0;
        var coresNeeded = (int)Math.Ceiling(totalCost / minCostPerCore);
        return Math.Max(1, Math.Min(coresNeeded, _processorCount));
    }

    private static IReadOnlyList<Range> PartitionEvenly(int totalWork, int numPartitions)
    {
        var partitions = new List<Range>();
        var baseSize = totalWork / numPartitions;
        var remainder = totalWork % numPartitions;

        var current = 0;
        for (var i = 0; i < numPartitions; i++)
        {
            var partitionSize = baseSize + (i < remainder ? 1 : 0);
            partitions.Add(new Range(current, current + partitionSize));
            current += partitionSize;
        }

        return partitions;
    }
}

/// <summary>
/// Represents an inclusive-exclusive range of work items [Start, End).
/// </summary>
public readonly struct Range : IEquatable<Range>
{
    /// <summary>Inclusive start index.</summary>
    public int Start { get; }

    /// <summary>Exclusive end index.</summary>
    public int End { get; }

    /// <summary>Number of work items in this range.</summary>
    public int Length => End - Start;

    /// <summary>
    /// Initializes a range.
    /// </summary>
    public Range(int start, int end)
    {
        Start = start;
        End = end;
    }

    /// <inheritdoc />
    public bool Equals(Range other) => Start == other.Start && End == other.End;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Range other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <inheritdoc />
    public override string ToString() => $"[{Start}..{End})";

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Range left, Range right) => left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Range left, Range right) => !left.Equals(right);
}
