namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel tensor operations providing element-wise addition, multiplication,
    /// and matrix multiplication with work distributed across multiple threads.
    /// </summary>
    public sealed class ParallelTensorOps
    {
        /// <summary>
        /// Adds two vectors in parallel. Each element of the result is the sum
        /// of the corresponding elements from <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">First input vector.</param>
        /// <param name="b">Second input vector (must have the same length as <paramref name="a"/>).</param>
        /// <returns>A new vector containing the element-wise sum.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when vectors have different lengths or are empty.
        /// </exception>
        public static double[] AddParallel(double[] a, double[] b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (a.Length == 0) throw new ArgumentException("Vectors must not be empty.", nameof(a));
            if (a.Length != b.Length) throw new ArgumentException("Vectors must have the same length.");

            double[] result = new double[a.Length];

            Parallel.For(0, a.Length, i =>
            {
                result[i] = a[i] + b[i];
            });

            return result;
        }

        /// <summary>
        /// Multiplies two vectors element-wise in parallel. Each element of the result
        /// is the product of the corresponding elements from <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">First input vector.</param>
        /// <param name="b">Second input vector (must have the same length as <paramref name="a"/>).</param>
        /// <returns>A new vector containing the element-wise product.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when vectors have different lengths or are empty.
        /// </exception>
        public static double[] MulParallel(double[] a, double[] b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (a.Length == 0) throw new ArgumentException("Vectors must not be empty.", nameof(a));
            if (a.Length != b.Length) throw new ArgumentException("Vectors must have the same length.");

            double[] result = new double[a.Length];

            Parallel.For(0, a.Length, i =>
            {
                result[i] = a[i] * b[i];
            });

            return result;
        }

        /// <summary>
        /// Multiplies two matrices in parallel using a row-partitioned strategy.
        /// Each output row is computed independently across threads.
        /// </summary>
        /// <param name="a">First matrix of shape [rowsA, colsA].</param>
        /// <param name="b">Second matrix of shape [colsA, colsB].</param>
        /// <returns>A new matrix of shape [rowsA, colsB] containing the product.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when matrices are empty or inner dimensions are incompatible.
        /// </exception>
        public static double[,] MatMulParallel(double[,] a, double[,] b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));

            int rowsA = a.GetLength(0);
            int colsA = a.GetLength(1);
            int rowsB = b.GetLength(0);
            int colsB = b.GetLength(1);

            if (rowsA == 0 || colsA == 0) throw new ArgumentException("Matrix a must not be empty.", nameof(a));
            if (rowsB == 0 || colsB == 0) throw new ArgumentException("Matrix b must not be empty.", nameof(b));
            if (colsA != rowsB)
                throw new ArgumentException(
                    $"Incompatible dimensions: a is {rowsA}x{colsA}, b is {rowsB}x{colsB}. " +
                    $"Columns of a must equal rows of b.");

            double[,] result = new double[rowsA, colsB];

            Parallel.For(0, rowsA, i =>
            {
                for (int j = 0; j < colsB; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < colsA; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            });

            return result;
        }
    }
}
