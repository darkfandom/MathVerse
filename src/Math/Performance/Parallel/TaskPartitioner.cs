namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Partitions work items into groups for parallel execution.
/// </summary>
public static class TaskPartitioner
{
    /// <summary>
    /// Partitions the items into the specified number of roughly equal partitions.
    /// </summary>
    /// <typeparam name="T">The type of items to partition.</typeparam>
    /// <param name="items">The items to partition.</param>
    /// <param name="partitionCount">The desired number of partitions.</param>
    /// <returns>A list of partitions, each containing a subset of the original items.</returns>
    public static IReadOnlyList<IReadOnlyList<T>> Partition<T>(IReadOnlyList<T> items, int partitionCount)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (partitionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(partitionCount), partitionCount, "Must be greater than zero.");

        var itemCount = items.Count;
        if (itemCount == 0)
            return [];

        var effectiveCount = System.Math.Min(partitionCount, itemCount);
        var partitions = new List<IReadOnlyList<T>>(effectiveCount);
        var baseSize = itemCount / effectiveCount;
        var remainder = itemCount % effectiveCount;
        var offset = 0;

        for (var i = 0; i < effectiveCount; i++)
        {
            var size = baseSize + (i < remainder ? 1 : 0);
            var segment = new T[size];
            for (var j = 0; j < size; j++)
            {
                segment[j] = items[offset + j];
            }
            partitions.Add(segment);
            offset += size;
        }

        return partitions;
    }

    /// <summary>
    /// Partitions the items so that no partition exceeds the specified maximum size.
    /// </summary>
    /// <typeparam name="T">The type of items to partition.</typeparam>
    /// <param name="items">The items to partition.</param>
    /// <param name="maxPartitionSize">The maximum number of items per partition.</param>
    /// <returns>A list of partitions, each containing at most <paramref name="maxPartitionSize"/> items.</returns>
    public static IReadOnlyList<IReadOnlyList<T>> PartitionBySize<T>(IReadOnlyList<T> items, int maxPartitionSize)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (maxPartitionSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPartitionSize), maxPartitionSize, "Must be greater than zero.");

        var itemCount = items.Count;
        if (itemCount == 0)
            return [];

        var partitionCount = (itemCount + maxPartitionSize - 1) / maxPartitionSize;
        var partitions = new List<IReadOnlyList<T>>(partitionCount);
        var offset = 0;

        while (offset < itemCount)
        {
            var size = System.Math.Min(maxPartitionSize, itemCount - offset);
            var segment = new T[size];
            for (var j = 0; j < size; j++)
            {
                segment[j] = items[offset + j];
            }
            partitions.Add(segment);
            offset += size;
        }

        return partitions;
    }
}
