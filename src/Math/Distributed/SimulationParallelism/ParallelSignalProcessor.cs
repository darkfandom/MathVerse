namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel signal processor that applies filters to signal data by partitioning
    /// the signal into segments and processing them concurrently.
    /// </summary>
    public sealed class ParallelSignalProcessor
    {
        /// <summary>
        /// Processes a signal in parallel by partitioning it into segments, applying the filter
        /// to each segment concurrently, and combining the results.
        /// </summary>
        /// <param name="signal">Input signal data as an array of sample values.</param>
        /// <param name="filter">
        /// Filter function that transforms a segment of the signal.
        /// Signature: (double[] segment) -> double[] filteredSegment.
        /// The output array must have the same length as the input segment.
        /// </param>
        /// <returns>
        /// A new array containing the filtered signal with all segments combined.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="signal"/> or <paramref name="filter"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="signal"/> is empty.</exception>
        public static double[] ProcessParallel(double[] signal, Func<double[], double[]> filter)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.", nameof(signal));

            int length = signal.Length;
            int processorCount = System.Environment.ProcessorCount;
            int segmentSize = System.Math.Max(1, length / processorCount);
            int segmentCount = (length + segmentSize - 1) / segmentSize;

            double[][] segments = new double[segmentCount][];
            double[][] results = new double[segmentCount][];

            for (int s = 0; s < segmentCount; s++)
            {
                int start = s * segmentSize;
                int end = System.Math.Min(start + segmentSize, length);
                int count = end - start;

                segments[s] = new double[count];
                System.Array.Copy(signal, start, segments[s], 0, count);
            }

            Parallel.For(0, segmentCount, s =>
            {
                results[s] = filter(segments[s]);
            });

            double[] output = new double[length];

            Parallel.For(0, segmentCount, s =>
            {
                int start = s * segmentSize;
                int copyLength = System.Math.Min(results[s].Length, length - start);
                System.Array.Copy(results[s], 0, output, start, copyLength);
            });

            return output;
        }
    }
}
