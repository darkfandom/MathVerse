namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a parallel for-loop implementation that partitions an integer range
    /// across multiple threads for concurrent execution.
    /// </summary>
    public sealed class ParallelFor
    {
        /// <summary>
        /// Executes a parallel for-loop over the integer range [<paramref name="fromInclusive"/>, <paramref name="toExclusive"/>).
        /// The range is partitioned into approximately <paramref name="degreeOfParallelism"/> segments.
        /// </summary>
        /// <param name="fromInclusive">The inclusive lower bound of the loop.</param>
        /// <param name="toExclusive">The exclusive upper bound of the loop.</param>
        /// <param name="body">The action to invoke for each iteration index.</param>
        /// <param name="degreeOfParallelism">
        /// The desired number of concurrent partitions. A value of -1 uses the processor count.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="fromInclusive"/> is greater than <paramref name="toExclusive"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="degreeOfParallelism"/> is not -1 and is less than 1.
        /// </exception>
        public static void Execute(
            int fromInclusive,
            int toExclusive,
            Action<int> body,
            int degreeOfParallelism = -1)
        {
            if (fromInclusive > toExclusive)
                throw new ArgumentOutOfRangeException(nameof(fromInclusive), "fromInclusive must be less than or equal to toExclusive.");
            if (body is null)
                throw new ArgumentNullException(nameof(body));
            if (degreeOfParallelism != -1 && degreeOfParallelism < 1)
                throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism), "Degree of parallelism must be -1 or a positive integer.");

            int totalRange = toExclusive - fromInclusive;
            if (totalRange == 0)
                return;

            int partitions = degreeOfParallelism == -1
                ? System.Environment.ProcessorCount
                : degreeOfParallelism;

            partitions = System.Math.Min(partitions, totalRange);

            List<(int start, int end)> ranges = TaskPartitioner.PartitionForRange(totalRange, partitions);

            Task[] tasks = new Task[ranges.Count];

            for (int i = 0; i < ranges.Count; i++)
            {
                int rangeStart = fromInclusive + ranges[i].start;
                int rangeEnd = fromInclusive + ranges[i].end;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = rangeStart; j < rangeEnd; j++)
                    {
                        body(j);
                    }
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
