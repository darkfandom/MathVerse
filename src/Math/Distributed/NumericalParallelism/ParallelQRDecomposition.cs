namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel QR decomposition using modified Gram-Schmidt process.
    /// </summary>
    public sealed class ParallelQRDecomposition
    {
        /// <summary>
        /// Decomposes a matrix into Q (orthogonal) and R (upper triangular) factors.
        /// </summary>
        /// <param name="matrix">Input matrix.</param>
        /// <returns>Tuple of (Q, R).</returns>
        public (double[,] Q, double[,] R) Decompose(double[,] matrix)
        {
            int m = matrix.GetLength(0);
            int n = matrix.GetLength(1);

            var Q = new double[m, n];
            var R = new double[n, n];

            var columns = new double[n][];
            for (int j = 0; j < n; j++)
            {
                columns[j] = new double[m];
                for (int i = 0; i < m; i++)
                    columns[j][i] = matrix[i, j];
            }

            for (int j = 0; j < n; j++)
            {
                double[] u = new double[m];
                Array.Copy(columns[j], u, m);

                for (int i = 0; i < j; i++)
                {
                    double dot = 0;
                    for (int k = 0; k < m; k++)
                        dot += Q[k, i] * columns[j][k];
                    R[i, j] = dot;

                    for (int k = 0; k < m; k++)
                        u[k] -= dot * Q[k, i];
                }

                double norm = 0;
                for (int k = 0; k < m; k++)
                    norm += u[k] * u[k];
                norm = System.Math.Sqrt(norm);

                if (norm < 1e-12)
                    throw new InvalidOperationException("Matrix columns are linearly dependent.");

                R[j, j] = norm;

                for (int k = 0; k < m; k++)
                    Q[k, j] = u[k] / norm;
            }

            return (Q, R);
        }

        /// <summary>
        /// Solves the least squares problem min||Ax - b|| using QR decomposition.
        /// </summary>
        /// <param name="A">Coefficient matrix.</param>
        /// <param name="b">Right-hand side vector.</param>
        /// <returns>Least squares solution.</returns>
        public double[] SolveLeastSquares(double[,] A, double[] b)
        {
            var (Q, R) = Decompose(A);
            int m = A.GetLength(0);
            int n = A.GetLength(1);

            var Qtb = new double[n];
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int i = 0; i < m; i++)
                    sum += Q[i, j] * b[i];
                Qtb[j] = sum;
            }

            var x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                    sum += R[i, j] * x[j];
                x[i] = (Qtb[i] - sum) / R[i, i];
            }

            return x;
        }
    }
}
