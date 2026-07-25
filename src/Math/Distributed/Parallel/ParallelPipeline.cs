namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides pipeline parallelism where data flows through a sequence of stages,
    /// with each stage executing asynchronously.
    /// </summary>
    public sealed class ParallelPipeline
    {
        /// <summary>
        /// Executes a pipeline of asynchronous stages sequentially, passing the output of each
        /// stage as the input to the next. Each stage runs as a <see cref="ValueTask{T}"/>
        /// for efficient async processing.
        /// </summary>
        /// <typeparam name="T">The data type flowing through the pipeline.</typeparam>
        /// <param name="input">The initial input to the pipeline.</param>
        /// <param name="stages">
        /// An ordered array of stage functions. Each receives the output of the previous stage.
        /// </param>
        /// <returns>The final output after all stages have executed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stages"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stages"/> is empty.</exception>
        public static async ValueTask<T> Execute<T>(T input, Func<T, ValueTask<T>>[] stages)
        {
            if (stages is null)
                throw new ArgumentNullException(nameof(stages));
            if (stages.Length == 0)
                throw new ArgumentException("Pipeline must have at least one stage.", nameof(stages));

            T current = input;

            for (int i = 0; i < stages.Length; i++)
            {
                current = await stages[i](current).ConfigureAwait(false);
            }

            return current;
        }

        /// <summary>
        /// Executes multiple independent pipeline instances in parallel, each processing
        /// the same input through the same set of stages.
        /// </summary>
        /// <typeparam name="T">The data type flowing through each pipeline.</typeparam>
        /// <param name="inputs">An array of inputs, one per pipeline instance.</param>
        /// <param name="stages">
        /// An ordered array of stage functions applied to each pipeline instance.
        /// </param>
        /// <returns>An array of outputs, one per pipeline instance, in the same order as the inputs.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="inputs"/> or <paramref name="stages"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stages"/> is empty.</exception>
        public static async ValueTask<T[]> ExecuteParallel<T>(T[] inputs, Func<T, ValueTask<T>>[] stages)
        {
            if (inputs is null)
                throw new ArgumentNullException(nameof(inputs));
            if (stages is null)
                throw new ArgumentNullException(nameof(stages));
            if (stages.Length == 0)
                throw new ArgumentException("Pipeline must have at least one stage.", nameof(stages));

            Task<T>[] tasks = new Task<T>[inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                int index = i;
                tasks[i] = Task.Run(async () =>
                {
                    T current = inputs[index];
                    for (int s = 0; s < stages.Length; s++)
                    {
                        current = await stages[s](current).ConfigureAwait(false);
                    }
                    return current;
                });
            }

            T[] results = new T[inputs.Length];
            T[] completedResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            Array.Copy(completedResults, results, inputs.Length);

            return results;
        }
    }
}
