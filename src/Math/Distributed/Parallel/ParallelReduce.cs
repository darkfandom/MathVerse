namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides parallel reduction operations using a tree reduction pattern
    /// for associative and commutative combining operations.
    /// </summary>
    public sealed class ParallelReduce
    {
        /// <summary>
        /// Reduces an array of elements using a binary reducer function and an identity value.
        /// Uses a tree reduction pattern for parallelism.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source array to reduce.</param>
        /// <param name="reducer">The associative binary combining function.</param>
        /// <param name="identity">The identity element for the reduction.</param>
        /// <returns>The reduced result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="reducer"/> is null.
        /// </exception>
        public static T Reduce<T>(T[] source, Func<T, T, T> reducer, T identity)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));
            if (source.Length == 0)
                return identity;

            if (source.Length == 1)
                return reducer(identity, source[0]);

            int processors = System.Environment.ProcessorCount;
            int chunkSize = (source.Length + processors - 1) / processors;
            int partialCount = (source.Length + chunkSize - 1) / chunkSize;
            T[] partials = new T[partialCount];

            Task[] tasks = new Task[partialCount];

            for (int i = 0; i < partialCount; i++)
            {
                int chunkStart = i * chunkSize;
                int chunkEnd = System.Math.Min(chunkStart + chunkSize, source.Length);
                int idx = i;
                tasks[i] = Task.Run(() =>
                {
                    T acc = identity;
                    for (int j = chunkStart; j < chunkEnd; j++)
                    {
                        acc = reducer(acc, source[j]);
                    }
                    partials[idx] = acc;
                });
            }

            Task.WaitAll(tasks);

            T result = identity;
            for (int i = 0; i < partials.Length; i++)
            {
                result = reducer(result, partials[i]);
            }

            return result;
        }

        /// <summary>
        /// Computes the sum of all elements in a double array in parallel.
        /// </summary>
        /// <param name="values">The array of values to sum.</param>
        /// <returns>The sum of all elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        public static double Sum(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            return Reduce(values, (a, b) => a + b, 0.0);
        }

        /// <summary>
        /// Finds the maximum value in a double array using parallel reduction.
        /// </summary>
        /// <param name="values">The array of values.</param>
        /// <returns>The maximum value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
        public static double Max(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("Array must not be empty.", nameof(values));

            return Reduce(values, (a, b) => a > b ? a : b, double.MinValue);
        }

        /// <summary>
        /// Finds the minimum value in a double array using parallel reduction.
        /// </summary>
        /// <param name="values">The array of values.</param>
        /// <returns>The minimum value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
        public static double Min(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("Array must not be empty.", nameof(values));

            return Reduce(values, (a, b) => a < b ? a : b, double.MaxValue);
        }
    }
}
