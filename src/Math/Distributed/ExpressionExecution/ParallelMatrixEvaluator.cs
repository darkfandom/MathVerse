namespace MathVerse.Math.Distributed.ExpressionExecution
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel matrix operations evaluator.
    /// </summary>
    public sealed class ParallelMatrixEvaluator
    {
        /// <summary>
        /// Multiplies two matrices in parallel using blocked algorithm.
        /// </summary>
        /// <param name="a">First matrix.</param>
        /// <param name="b">Second matrix.</param>
        /// <returns>Result matrix.</returns>
        public double[,] MultiplyParallel(double[,] a, double[,] b)
        {
            int aRows = a.GetLength(0);
            int aCols = a.GetLength(1);
            int bCols = b.GetLength(1);

            if (aCols != b.GetLength(0))
                throw new ArgumentException("Matrix dimensions are incompatible for multiplication.");

            var result = new double[aRows, bCols];
            int blockSize = 64;

            Parallel.For(0, (aRows + blockSize - 1) / blockSize, bi =>
            {
                for (int bj = 0; bj < (aCols + blockSize - 1) / blockSize; bj++)
                {
                    for (int bk = 0; bk < (bCols + blockSize - 1) / blockSize; bk++)
                    {
                        int iEnd = System.Math.Min((bi + 1) * blockSize, aRows);
                        int jEnd = System.Math.Min((bj + 1) * blockSize, aCols);
                        int kEnd = System.Math.Min((bk + 1) * blockSize, bCols);

                        for (int i = bi * blockSize; i < iEnd; i++)
                        {
                            for (int j = bj * blockSize; j < jEnd; j++)
                            {
                                double sum = 0;
                                for (int k = bk * blockSize; k < kEnd; k++)
                                {
                                    sum += a[i, k] * b[k, j];
                                }
                                result[i, j] += sum;
                            }
                        }
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Transposes a matrix in parallel.
        /// </summary>
        /// <param name="matrix">Input matrix.</param>
        /// <returns>Transposed matrix.</returns>
        public double[,] TransposeParallel(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var result = new double[cols, rows];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            });

            return result;
        }

        /// <summary>
        /// Computes the inverse of a matrix using Gauss-Jordan elimination.
        /// </summary>
        /// <param name="matrix">Input matrix.</param>
        /// <returns>Inverse matrix.</returns>
        public double[,] InverseParallel(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            var augmented = new double[n, 2 * n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                }
                augmented[i, n + i] = 1;
            }

            for (int col = 0; col < n; col++)
            {
                int maxRow = col;
                for (int row = col + 1; row < n; row++)
                {
                    if (System.Math.Abs(augmented[row, col]) > System.Math.Abs(augmented[maxRow, col]))
                        maxRow = row;
                }

                if (maxRow != col)
                {
                    for (int j = 0; j < 2 * n; j++)
                    {
                        (augmented[col, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[col, j]);
                    }
                }

                double pivot = augmented[col, col];
                if (System.Math.Abs(pivot) < 1e-12)
                    throw new InvalidOperationException("Matrix is singular.");

                Parallel.For(0, 2 * n, j =>
                {
                    augmented[col, j] /= pivot;
                });

                Parallel.For(0, n, row =>
                {
                    if (row != col)
                    {
                        double factor = augmented[row, col];
                        for (int j = 0; j < 2 * n; j++)
                        {
                            augmented[row, j] -= factor * augmented[col, j];
                        }
                    }
                });
            }

            var inverse = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    inverse[i, j] = augmented[i, n + j];
                }
            }

            return inverse;
        }
    }
}
