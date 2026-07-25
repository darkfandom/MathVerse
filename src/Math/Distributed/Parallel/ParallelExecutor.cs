namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Main parallel execution orchestrator for dispatching independent tasks
    /// and chunked work across available processor cores.
    /// </summary>
    public sealed class ParallelExecutor
    {
        /// <summary>
        /// Executes an array of independent tasks in parallel and returns their results
        /// in the same order as the input array.
        /// </summary>
        /// <typeparam name="T">The return type of each task.</typeparam>
        /// <param name="tasks">An array of task functions to execute.</param>
        /// <returns>An array of results in the same order as the input tasks.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tasks"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tasks"/> is empty.</exception>
        public static T[] ExecuteParallel<T>(Func<T>[] tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0)
                throw new ArgumentException("Task array must not be empty.", nameof(tasks));

            T[] results = new T[tasks.Length];
            Task<T>[] taskArray = new Task<T>[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                int index = i;
                taskArray[i] = Task.Run(() => tasks[index]());
            }

            Task.WaitAll(taskArray);

            for (int i = 0; i < taskArray.Length; i++)
            {
                results[i] = taskArray[i].Result;
            }

            return results;
        }

        /// <summary>
        /// Executes an array of independent void actions in parallel.
        /// </summary>
        /// <param name="tasks">An array of action delegates to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tasks"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tasks"/> is empty.</exception>
        public static void ExecuteParallel(Action[] tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0)
                throw new ArgumentException("Task array must not be empty.", nameof(tasks));

            Task[] taskArray = new Task[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                int index = i;
                taskArray[i] = Task.Run(() => tasks[index]());
            }

            Task.WaitAll(taskArray);
        }

        /// <summary>
        /// Executes a chunked parallel operation where the work is divided into fixed-size
        /// blocks and each chunk is processed by the provided function with its starting index.
        /// </summary>
        /// <typeparam name="T">The return type of each chunk function.</typeparam>
        /// <param name="chunkFunc">A function that processes a chunk starting at the given index.</param>
        /// <param name="totalItems">The total number of work items to process.</param>
        /// <param name="chunkSize">The number of items per chunk.</param>
        /// <returns>An array of results, one per chunk.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="chunkFunc"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="totalItems"/> or <paramref name="chunkSize"/> is non-positive.
        /// </exception>
        public static T[] ExecuteParallelChunked<T>(Func<int, T> chunkFunc, int totalItems, int chunkSize)
        {
            if (chunkFunc is null)
                throw new ArgumentNullException(nameof(chunkFunc));
            if (totalItems <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalItems), "Total items must be positive.");
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

            int chunkCount = (totalItems + chunkSize - 1) / chunkSize;
            Task<T>[] taskArray = new Task<T>[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                int start = i * chunkSize;
                taskArray[i] = Task.Run(() => chunkFunc(start));
            }

            Task.WaitAll(taskArray);

            T[] results = new T[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                results[i] = taskArray[i].Result;
            }

            return results;
        }
    }
}
