namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a parallel merge sort implementation that recursively splits the array,
    /// sorts halves concurrently, and merges the results.
    /// </summary>
    public sealed class ParallelSort
    {
        /// <summary>
        /// The recursion depth at which to switch from parallel to sequential sort.
        /// </summary>
        private const int ParallelThreshold = 1024;

        /// <summary>
        /// Sorts an array in parallel using a merge sort algorithm.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The array to sort in place.</param>
        /// <param name="comparer">
        /// The comparer to use for ordering. If null, <see cref="Comparer{T}.Default"/> is used.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null.</exception>
        public static void Sort<T>(T[] array, IComparer<T>? comparer = null)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length <= 1)
                return;

            IComparer<T> cmp = comparer ?? Comparer<T>.Default;
            T[] auxiliary = new T[array.Length];
            Array.Copy(array, auxiliary, array.Length);
            ParallelSortInternal(array, auxiliary, 0, array.Length - 1, cmp, 0);
            Array.Copy(auxiliary, array, array.Length);
        }

        /// <summary>
        /// Internal recursive parallel merge sort.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source data.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="left">The left index (inclusive).</param>
        /// <param name="right">The right index (inclusive).</param>
        /// <param name="comparer">The element comparer.</param>
        /// <param name="depth">The current recursion depth.</param>
        private static void ParallelSortInternal<T>(
            T[] source,
            T[] destination,
            int left,
            int right,
            IComparer<T> comparer,
            int depth)
        {
            if (left >= right)
                return;

            int mid = left + (right - left) / 2;
            int rangeSize = right - left + 1;

            if (rangeSize <= ParallelThreshold || depth >= 4)
            {
                SequentialInsertionSort(destination, left, right, comparer);
                return;
            }

            Task leftTask = Task.Run(() =>
            {
                Array.Copy(source, left, destination, left, mid - left + 1);
                ParallelSortInternal(destination, source, left, mid, comparer, depth + 1);
            });

            Task rightTask = Task.Run(() =>
            {
                Array.Copy(source, mid + 1, destination, mid + 1, right - mid);
                ParallelSortInternal(destination, source, mid + 1, right, comparer, depth + 1);
            });

            Task.WaitAll(leftTask, rightTask);

            ParallelMerge.Merge(
                source,
                destination,
                left,
                mid,
                mid + 1,
                right,
                comparer);
        }

        /// <summary>
        /// Sorts a sub-array using insertion sort.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The array to sort.</param>
        /// <param name="left">The left index (inclusive).</param>
        /// <param name="right">The right index (inclusive).</param>
        /// <param name="comparer">The element comparer.</param>
        private static void SequentialInsertionSort<T>(T[] array, int left, int right, IComparer<T> comparer)
        {
            for (int i = left + 1; i <= right; i++)
            {
                T key = array[i];
                int j = i - 1;

                while (j >= left && comparer.Compare(array[j], key) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = key;
            }
        }
    }
}
