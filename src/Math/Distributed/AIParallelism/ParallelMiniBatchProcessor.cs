namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel mini-batch processor that partitions data into batches and processes
    /// them concurrently for efficient data-parallel computation.
    /// </summary>
    public sealed class ParallelMiniBatchProcessor
    {
        /// <summary>
        /// Processes data in parallel mini-batches. The input data is partitioned into
        /// batches of the specified size, and each batch is processed concurrently.
        /// Results from all batches are concatenated in order.
        /// </summary>
        /// <param name="data">Input data array where each element is a data sample.</param>
        /// <param name="processor">
        /// Processing function applied to each mini-batch.
        /// Signature: (double[][] batch) -> double[][] processedBatch.
        /// </param>
        /// <param name="batchSize">Number of samples per mini-batch (default: 32).</param>
        /// <returns>
        /// All processed samples concatenated in the original order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> or <paramref name="processor"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="data"/> is empty or <paramref name="batchSize"/> is not positive.
        /// </exception>
        public static double[][] ProcessBatch(
            double[][] data,
            Func<double[][], double[][]> processor,
            int batchSize = 32)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (processor == null) throw new ArgumentNullException(nameof(processor));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.", nameof(data));
            if (batchSize <= 0) throw new ArgumentException("Batch size must be positive.", nameof(batchSize));

            int totalSamples = data.Length;
            int batchCount = (totalSamples + batchSize - 1) / batchSize;

            double[][][] batchResults = new double[batchCount][][];

            Parallel.For(0, batchCount, b =>
            {
                int start = b * batchSize;
                int end = System.Math.Min(start + batchSize, totalSamples);
                int count = end - start;

                double[][] batch = new double[count][];
                for (int i = 0; i < count; i++)
                {
                    batch[i] = data[start + i];
                }

                batchResults[b] = processor(batch);
            });

            // Concatenate results in order
            int totalOutput = 0;
            for (int b = 0; b < batchCount; b++)
            {
                totalOutput += batchResults[b].Length;
            }

            double[][] result = new double[totalOutput][];
            int offset = 0;

            for (int b = 0; b < batchCount; b++)
            {
                System.Array.Copy(batchResults[b], 0, result, offset, batchResults[b].Length);
                offset += batchResults[b].Length;
            }

            return result;
        }
    }
}
