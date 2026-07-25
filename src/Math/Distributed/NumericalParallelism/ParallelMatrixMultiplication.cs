namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel matrix multiplication using blocked algorithm.
    /// </summary>
    public sealed class ParallelMatrixMultiplication
    {
        /// <summary>
        /// Multiplies two matrices using blocked parallel algorithm.
        /// </summary>
        /// <param name="A">First matrix.</param>
        /// <param name="B">Second matrix.</param>
        /// <param name="blockSize">Block size for cache-efficient access.</param>
        /// <returns>Result matrix C = A * B.</returns>
        public double[,] Multiply(double[,] A, double[,] B, int blockSize = 64)
        {
            int aRows = A.GetLength(0);
            int aCols = A.GetLength(1);
            int bCols = B.GetLength(1);

            if (aCols != B.GetLength(0))
                throw new ArgumentException("Matrix dimensions are incompatible for multiplication.");

            var C = new double[aRows, bCols];

            int blocksI = (aRows + blockSize - 1) / blockSize;
            int blocksJ = (bCols + blockSize - 1) / blockSize;
            int blocksK = (aCols + blockSize - 1) / blockSize;

            Parallel.For(0, blocksI, bi =>
            {
                for (int bj = 0; bj < blocksJ; bj++)
                {
                    for (int bk = 0; bk < blocksK; bk++)
                    {
                        int iEnd = System.Math.Min((bi + 1) * blockSize, aRows);
                        int jEnd = System.Math.Min((bj + 1) * blockSize, bCols);
                        int kEnd = System.Math.Min((bk + 1) * blockSize, aCols);

                        for (int i = bi * blockSize; i < iEnd; i++)
                        {
                            for (int k = bk * blockSize; k < kEnd; k++)
                            {
                                double aik = A[i, k];
                                for (int j = bj * blockSize; j < jEnd; j++)
                                {
                                    C[i, j] += aik * B[k, j];
                                }
                            }
                        }
                    }
                }
            });

            return C;
        }

        /// <summary>
        /// Computes C += A * B^T (transpose multiply) in parallel.
        /// </summary>
        /// <param name="A">First matrix.</param>
        /// <param name="B">Second matrix (transposed).</param>
        /// <returns>Result matrix.</returns>
        public double[,] MultiplyTranspose(double[,] A, double[,] B)
        {
            int aRows = A.GetLength(0);
            int bRows = B.GetLength(0);
            int bCols = B.GetLength(1);

            var C = new double[aRows, bRows];

            Parallel.For(0, aRows, i =>
            {
                for (int j = 0; j < bRows; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < bCols; k++)
                    {
                        sum += A[i, k] * B[j, k];
                    }
                    C[i, j] = sum;
                }
            });

            return C;
        }
    }
}
