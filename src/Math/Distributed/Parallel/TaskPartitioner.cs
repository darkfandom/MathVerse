namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods for partitioning work items into chunks for parallel processing.
    /// </summary>
    public sealed class TaskPartitioner
    {
        /// <summary>
        /// Partitions a total number of items into fixed-size chunks.
        /// The last chunk may be smaller if the total is not evenly divisible.
        /// </summary>
        /// <param name="totalItems">The total number of work items.</param>
        /// <param name="chunkSize">The maximum number of items per chunk.</param>
        /// <returns>A list of (start, end) tuples representing each chunk's range [start, end).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="totalItems"/> or <paramref name="chunkSize"/> is non-positive.
        /// </exception>
        public static List<(int start, int end)> Partition(int totalItems, int chunkSize)
        {
            if (totalItems < 0)
                throw new ArgumentOutOfRangeException(nameof(totalItems), "Total items must be non-negative.");
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

            List<(int start, int end)> partitions = new List<(int, int)>();

            if (totalItems == 0)
                return partitions;

            for (int i = 0; i < totalItems; i += chunkSize)
            {
                int end = System.Math.Min(i + chunkSize, totalItems);
                partitions.Add((i, end));
            }

            return partitions;
        }

        /// <summary>
        /// Partitions a total number of items into at most the specified number of ranges,
        /// distributing items as evenly as possible across partitions.
        /// </summary>
        /// <param name="totalItems">The total number of work items.</param>
        /// <param name="maxPartitions">The maximum number of partitions.</param>
        /// <returns>A list of (start, end) tuples representing each partition's range [start, end).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="totalItems"/> is negative or <paramref name="maxPartitions"/> is non-positive.
        /// </exception>
        public static List<(int start, int end)> PartitionForRange(int totalItems, int maxPartitions)
        {
            if (totalItems < 0)
                throw new ArgumentOutOfRangeException(nameof(totalItems), "Total items must be non-negative.");
            if (maxPartitions <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPartitions), "Max partitions must be positive.");

            List<(int start, int end)> partitions = new List<(int, int)>();

            if (totalItems == 0)
                return partitions;

            int actualPartitions = System.Math.Min(maxPartitions, totalItems);
            int baseSize = totalItems / actualPartitions;
            int remainder = totalItems % actualPartitions;

            int current = 0;

            for (int i = 0; i < actualPartitions; i++)
            {
                int size = baseSize + (i < remainder ? 1 : 0);
                partitions.Add((current, current + size));
                current += size;
            }

            return partitions;
        }
    }
}
