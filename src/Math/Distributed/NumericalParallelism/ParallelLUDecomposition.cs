namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel LU decomposition with partial pivoting.
    /// </summary>
    public sealed class ParallelLUDecomposition
    {
        /// <summary>
        /// Decomposes a matrix into L and U factors with partial pivoting.
        /// </summary>
        /// <param name="matrix">Input matrix to decompose.</param>
        /// <returns>Tuple of (L, U, pivot) where pivot indicates row swaps.</returns>
        public (double[,] L, double[,] U, int[] pivot) Decompose(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            var LU = (double[,])matrix.Clone();
            var pivot = new int[n];
            for (int i = 0; i < n; i++) pivot[i] = i;

            for (int col = 0; col < n; col++)
            {
                int maxRow = col;
                double maxVal = System.Math.Abs(LU[col, col]);

                for (int row = col + 1; row < n; row++)
                {
                    double val = System.Math.Abs(LU[row, col]);
                    if (val > maxVal)
                    {
                        maxVal = val;
                        maxRow = row;
                    }
                }

                if (maxRow != col)
                {
                    for (int j = 0; j < n; j++)
                    {
                        (LU[col, j], LU[maxRow, j]) = (LU[maxRow, j], LU[col, j]);
                    }
                    (pivot[col], pivot[maxRow]) = (pivot[maxRow], pivot[col]);
                }

                double diag = LU[col, col];
                if (System.Math.Abs(diag) < 1e-12)
                    throw new InvalidOperationException("Matrix is singular or nearly singular.");

                Parallel.For(col + 1, n, row =>
                {
                    LU[row, col] /= diag;
                    for (int j = col + 1; j < n; j++)
                    {
                        LU[row, j] -= LU[row, col] * LU[col, j];
                    }
                });
            }

            var L = new double[n, n];
            var U = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                L[i, i] = 1;
                for (int j = 0; j < i; j++)
                    L[i, j] = LU[i, j];
                for (int j = i; j < n; j++)
                    U[i, j] = LU[i, j];
            }

            return (L, U, pivot);
        }

        /// <summary>
        /// Solves Ax = b using LU decomposition.
        /// </summary>
        /// <param name="matrix">Coefficient matrix.</param>
        /// <param name="b">Right-hand side vector.</param>
        /// <returns>Solution vector x.</returns>
        public double[] Solve(double[,] matrix, double[] b)
        {
            var (L, U, pivot) = Decompose(matrix);
            int n = matrix.GetLength(0);

            var pb = new double[n];
            for (int i = 0; i < n; i++)
                pb[i] = b[pivot[i]];

            var y = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < i; j++)
                    sum += L[i, j] * y[j];
                y[i] = pb[i] - sum;
            }

            var x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                    sum += U[i, j] * x[j];
                x[i] = (y[i] - sum) / U[i, i];
            }

            return x;
        }
    }
}
