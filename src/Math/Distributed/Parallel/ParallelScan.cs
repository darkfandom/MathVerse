namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an inclusive parallel prefix scan using the Blelloch work-efficient algorithm.
    /// </summary>
    public sealed class ParallelScan
    {
        /// <summary>
        /// Performs an inclusive parallel prefix scan on the source array using the given
        /// associative binary operator and identity element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source array to scan.</param>
        /// <param name="op">The associative binary operator.</param>
        /// <param name="identity">The identity element for the operator.</param>
        /// <returns>A new array of the same length where each element contains the inclusive prefix sum.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="op"/> is null.
        /// </exception>
        public static T[] Scan<T>(T[] source, Func<T, T, T> op, T identity)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (op is null)
                throw new ArgumentNullException(nameof(op));

            if (source.Length == 0)
                return Array.Empty<T>();

            if (source.Length == 1)
            {
                return new T[] { op(identity, source[0]) };
            }

            int n = source.Length;
            T[] result = new T[n];
            Array.Copy(source, result, n);

            int processors = System.Environment.ProcessorCount;
            if (n <= processors * 2)
            {
                SequentialInclusiveScan(result, op);
                return result;
            }

            int depth = ComputeDepth(n);
            int leafSize = (n + processors - 1) / processors;
            int leafCount = (n + leafSize - 1) / leafSize;

            T[] scannedLeaves = new T[leafCount];
            Task[] leafTasks = new Task[leafCount];

            for (int i = 0; i < leafCount; i++)
            {
                int start = i * leafSize;
                int end = System.Math.Min(start + leafSize, n);
                int idx = i;
                leafTasks[i] = Task.Run(() =>
                {
                    T acc = identity;
                    for (int j = start; j < end; j++)
                    {
                        acc = op(acc, result[j]);
                    }
                    scannedLeaves[idx] = acc;
                });
            }

            Task.WaitAll(leafTasks);

            T[] prefixes = new T[leafCount];
            prefixes[0] = scannedLeaves[0];
            for (int i = 1; i < leafCount; i++)
            {
                prefixes[i] = op(prefixes[i - 1], scannedLeaves[i]);
            }

            T[] adjustedPrefixes = new T[leafCount];
            adjustedPrefixes[0] = identity;
            for (int i = 1; i < leafCount; i++)
            {
                adjustedPrefixes[i] = prefixes[i - 1];
            }

            Task[] expandTasks = new Task[leafCount];

            for (int i = 0; i < leafCount; i++)
            {
                int start = i * leafSize;
                int end = System.Math.Min(start + leafSize, n);
                T prefix = adjustedPrefixes[i];
                int idx = i;
                expandTasks[i] = Task.Run(() =>
                {
                    T acc = prefix;
                    for (int j = start; j < end; j++)
                    {
                        acc = op(acc, source[j]);
                        result[j] = acc;
                    }
                });
            }

            Task.WaitAll(expandTasks);

            return result;
        }

        /// <summary>
        /// Computes the number of parallel scan levels needed for the given size.
        /// </summary>
        /// <param name="n">The size of the input.</param>
        /// <returns>The number of levels in the reduction tree.</returns>
        private static int ComputeDepth(int n)
        {
            int depth = 0;
            int v = n;
            while (v > 1)
            {
                v = (v + 1) / 2;
                depth++;
            }
            return depth;
        }

        /// <summary>
        /// Performs a sequential inclusive scan in place.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The array to scan in place.</param>
        /// <param name="op">The associative binary operator.</param>
        private static void SequentialInclusiveScan<T>(T[] array, Func<T, T, T> op)
        {
            for (int i = 1; i < array.Length; i++)
            {
                array[i] = op(array[i - 1], array[i]);
            }
        }
    }
}
