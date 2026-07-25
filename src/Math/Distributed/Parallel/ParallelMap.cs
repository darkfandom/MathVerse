namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a parallel map operation that applies a transformation function
    /// to each element of an input array concurrently.
    /// </summary>
    public sealed class ParallelMap
    {
        /// <summary>
        /// Applies a mapping function to each element of the source array in parallel,
        /// returning a new array with the transformed values.
        /// </summary>
        /// <typeparam name="TIn">The input element type.</typeparam>
        /// <typeparam name="TOut">The output element type.</typeparam>
        /// <param name="source">The source array to map over.</param>
        /// <param name="mapper">The transformation function to apply to each element.</param>
        /// <returns>A new array containing the mapped values.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="mapper"/> is null.
        /// </exception>
        public static TOut[] Map<TIn, TOut>(TIn[] source, Func<TIn, TOut> mapper)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (source.Length == 0)
                return Array.Empty<TOut>();

            int processors = System.Environment.ProcessorCount;
            int chunkSize = (source.Length + processors - 1) / processors;
            int chunkCount = (source.Length + chunkSize - 1) / chunkSize;

            TOut[] result = new TOut[source.Length];
            Task[] tasks = new Task[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                int start = i * chunkSize;
                int end = System.Math.Min(start + chunkSize, source.Length);
                tasks[i] = Task.Run(() =>
                {
                    for (int j = start; j < end; j++)
                    {
                        result[j] = mapper(source[j]);
                    }
                });
            }

            Task.WaitAll(tasks);

            return result;
        }
    }
}
