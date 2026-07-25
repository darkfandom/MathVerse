namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides parallel merge operations for combining two sorted sub-arrays into a single
    /// sorted result.
    /// </summary>
    public sealed class ParallelMerge
    {
        /// <summary>
        /// Merges two sorted sub-arrays from the source into a sorted destination array.
        /// The left sub-range is [leftStart, leftEnd] and the right sub-range is [rightStart, rightEnd].
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source array containing both sorted sub-ranges.</param>
        /// <param name="destination">The destination array to write the merged result.</param>
        /// <param name="leftStart">The start index of the left sub-range (inclusive).</param>
        /// <param name="leftEnd">The end index of the left sub-range (inclusive).</param>
        /// <param name="rightStart">The start index of the right sub-range (inclusive).</param>
        /// <param name="rightEnd">The end index of the right sub-range (inclusive).</param>
        /// <param name="comparer">
        /// The comparer to use for ordering. If null, <see cref="Comparer{T}.Default"/> is used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="destination"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the sub-ranges are not valid or not sorted.</exception>
        public static void Merge<T>(
            T[] source,
            T[] destination,
            int leftStart,
            int leftEnd,
            int rightStart,
            int rightEnd,
            IComparer<T>? comparer = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (leftStart > leftEnd)
                throw new ArgumentException("Left range is invalid.");
            if (rightStart > rightEnd)
                throw new ArgumentException("Right range is invalid.");
            if (rightStart != leftEnd + 1)
                throw new ArgumentException("Right range must immediately follow the left range.");

            IComparer<T> cmp = comparer ?? Comparer<T>.Default;
            int leftCount = leftEnd - leftStart + 1;
            int rightCount = rightEnd - rightStart + 1;
            int totalCount = leftCount + rightCount;

            if (totalCount <= 0)
                return;

            int threshold = 256;

            if (totalCount <= threshold || (leftCount <= 64 && rightCount <= 64))
            {
                SequentialMerge(source, destination, leftStart, leftEnd, rightStart, rightEnd, cmp);
                return;
            }

            if (leftCount > rightCount)
            {
                int leftMid = leftStart + leftCount / 2;
                T pivot = source[leftMid];
                int rightPartition = BinarySearchLower(source, rightStart, rightEnd, pivot, cmp);

                destination[leftMid + (rightPartition - rightStart)] = source[leftMid];

                Task leftTask = System.Threading.Tasks.Task.Run(() =>
                    Merge(source, destination, leftStart, leftMid - 1, rightStart, rightPartition - 1, cmp));

                Task rightTask = System.Threading.Tasks.Task.Run(() =>
                    Merge(source, destination, leftMid + 1, leftEnd, rightPartition, rightEnd, cmp));

                System.Threading.Tasks.Task.WaitAll(leftTask, rightTask);
            }
            else
            {
                int rightMid = rightStart + rightCount / 2;
                T pivot = source[rightMid];
                int leftPartition = BinarySearchUpper(source, leftStart, leftEnd, pivot, cmp);

                destination[leftPartition + (rightMid - rightStart)] = source[rightMid];

                Task leftTask = System.Threading.Tasks.Task.Run(() =>
                    Merge(source, destination, leftStart, leftPartition - 1, rightStart, rightMid - 1, cmp));

                Task rightTask = System.Threading.Tasks.Task.Run(() =>
                    Merge(source, destination, leftPartition, leftEnd, rightMid + 1, rightEnd, cmp));

                System.Threading.Tasks.Task.WaitAll(leftTask, rightTask);
            }
        }

        /// <summary>
        /// Merges two sorted arrays into a new sorted array.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="left">The first sorted array.</param>
        /// <param name="right">The second sorted array.</param>
        /// <param name="comparer">
        /// The comparer to use for ordering. If null, <see cref="Comparer{T}.Default"/> is used.
        /// </param>
        /// <returns>A new sorted array containing all elements from both input arrays.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is null.
        /// </exception>
        public static T[] Merge<T>(T[] left, T[] right, IComparer<T>? comparer = null)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));
            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Length == 0)
                return (T[])right.Clone();
            if (right.Length == 0)
                return (T[])left.Clone();

            T[] combined = new T[left.Length + right.Length];
            T[] source = new T[left.Length + right.Length];
            Array.Copy(left, 0, source, 0, left.Length);
            Array.Copy(right, 0, source, left.Length, right.Length);

            Merge(source, combined, 0, left.Length - 1, left.Length, source.Length - 1, comparer);
            return combined;
        }

        /// <summary>
        /// Performs a sequential merge of two sorted sub-ranges.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        private static void SequentialMerge<T>(
            T[] source,
            T[] destination,
            int leftStart,
            int leftEnd,
            int rightStart,
            int rightEnd,
            IComparer<T> comparer)
        {
            int i = leftStart;
            int j = rightStart;
            int k = leftStart;

            while (i <= leftEnd && j <= rightEnd)
            {
                if (comparer.Compare(source[i], source[j]) <= 0)
                {
                    destination[k++] = source[i++];
                }
                else
                {
                    destination[k++] = source[j++];
                }
            }

            while (i <= leftEnd)
            {
                destination[k++] = source[i++];
            }

            while (j <= rightEnd)
            {
                destination[k++] = source[j++];
            }
        }

        /// <summary>
        /// Binary search for the first element in the range greater than or equal to the pivot.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        private static int BinarySearchLower<T>(T[] array, int start, int end, T pivot, IComparer<T> comparer)
        {
            int lo = start;
            int hi = end + 1;

            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (comparer.Compare(array[mid], pivot) < 0)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lo;
        }

        /// <summary>
        /// Binary search for the first element in the range greater than the pivot.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        private static int BinarySearchUpper<T>(T[] array, int start, int end, T pivot, IComparer<T> comparer)
        {
            int lo = start;
            int hi = end + 1;

            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (comparer.Compare(array[mid], pivot) <= 0)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lo;
        }
    }
}
