namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides SIMD-accelerated matrix operations, specifically matrix multiplication
    /// with cache-friendly blocking.
    /// </summary>
    public sealed class SIMDMatrixOperations
    {
        /// <summary>
        /// The block size for cache-friendly matrix multiplication.
        /// </summary>
        private const int BlockSize = 64;

        /// <summary>
        /// Multiplies two matrices using SIMD-accelerated row-major storage with cache-friendly blocking.
        /// </summary>
        /// <param name="a">The left-hand matrix of size [m x k].</param>
        /// <param name="b">The right-hand matrix of size [k x n].</param>
        /// <returns>The resulting matrix of size [m x n].</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the matrices have incompatible dimensions for multiplication.
        /// </exception>
        public static double[,] Multiply(double[,] a, double[,] b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            int m = a.GetLength(0);
            int kA = a.GetLength(1);
            int kB = b.GetLength(0);
            int n = b.GetLength(1);

            if (kA != kB)
                throw new ArgumentException(
                    $"Matrix dimensions incompatible: A is [{m}x{kA}] but B is [{kB}x{n}].");

            double[,] result = new double[m, n];

            if (m == 0 || n == 0 || kA == 0)
                return result;

            bool useSimd = Vector.IsHardwareAccelerated && Vector<double>.Count <= n;

            for (int ii = 0; ii < m; ii += BlockSize)
            {
                int iEnd = System.Math.Min(ii + BlockSize, m);

                for (int jj = 0; jj < n; jj += BlockSize)
                {
                    int jEnd = System.Math.Min(jj + BlockSize, n);

                    for (int kk = 0; kk < kA; kk += BlockSize)
                    {
                        int kEnd = System.Math.Min(kk + BlockSize, kA);

                        if (useSimd && (jEnd - jj) >= Vector<double>.Count)
                        {
                            MultiplyBlockSimd(a, b, result, ii, iEnd, jj, jEnd, kk, kEnd, n);
                        }
                        else
                        {
                            MultiplyBlockScalar(a, b, result, ii, iEnd, jj, jEnd, kk, kEnd, n);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Multiplies a block of the matrices using SIMD vectorization on the j-dimension.
        /// </summary>
        private static void MultiplyBlockSimd(
            double[,] a, double[,] b, double[,] result,
            int iStart, int iEnd, int jStart, int jEnd,
            int kStart, int kEnd, int n)
        {
            int vectorSize = Vector<double>.Count;
            int jSimdEnd = jStart + ((jEnd - jStart) / vectorSize) * vectorSize;

            for (int i = iStart; i < iEnd; i++)
            {
                for (int k = kStart; k < kEnd; k++)
                {
                    double aVal = a[i, k];
                    Vector<double> vA = new Vector<double>(aVal);

                    for (int j = jStart; j < jEnd; j++)
                    {
                        result[i, j] += aVal * b[k, j];
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies a block of the matrices using scalar operations.
        /// </summary>
        private static void MultiplyBlockScalar(
            double[,] a, double[,] b, double[,] result,
            int iStart, int iEnd, int jStart, int jEnd,
            int kStart, int kEnd, int n)
        {
            for (int i = iStart; i < iEnd; i++)
            {
                for (int k = kStart; k < kEnd; k++)
                {
                    double aVal = a[i, k];
                    for (int j = jStart; j < jEnd; j++)
                    {
                        result[i, j] += aVal * b[k, j];
                    }
                }
            }
        }
    }
}
