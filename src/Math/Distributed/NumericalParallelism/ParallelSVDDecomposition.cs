namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Singular Value Decomposition (SVD) using one-sided Jacobi method.
    /// </summary>
    public sealed class ParallelSVDDecomposition
    {
        /// <summary>
        /// Decomposes a matrix into U, S, V where A = U * diag(S) * V^T.
        /// </summary>
        /// <param name="matrix">Input matrix.</param>
        /// <returns>Tuple of (U, S, V).</returns>
        public (double[,] U, double[] S, double[,] V) Decompose(double[,] matrix)
        {
            int m = matrix.GetLength(0);
            int n = matrix.GetLength(1);
            int minMN = System.Math.Min(m, n);

            var A = (double[,])matrix.Clone();
            var V = new double[n, n];
            for (int i = 0; i < n; i++)
                V[i, i] = 1;

            const int maxSweeps = 100;
            const double tolerance = 1e-12;

            for (int sweep = 0; sweep < maxSweeps; sweep++)
            {
                double offDiag = ComputeOffDiagonal(A, m, n);
                if (offDiag < tolerance)
                    break;

                for (int p = 0; p < n; p++)
                {
                    for (int q = p + 1; q < n; q++)
                    {
                        double[] colP = GetColumn(A, p, m);
                        double[] colQ = GetColumn(A, q, m);

                        double alpha = 0, beta = 0, gamma = 0;
                        for (int i = 0; i < m; i++)
                        {
                            alpha += colP[i] * colP[i];
                            beta += colQ[i] * colQ[i];
                            gamma += colP[i] * colQ[i];
                        }

                        if (System.Math.Abs(gamma) < tolerance * System.Math.Sqrt(alpha * beta))
                            continue;

                        double zeta = (beta - alpha) / (2 * gamma);
                        double t = System.Math.Sign(zeta) / (System.Math.Abs(zeta) + System.Math.Sqrt(1 + zeta * zeta));
                        double c = 1 / System.Math.Sqrt(1 + t * t);
                        double s = t * c;

                        Parallel.For(0, m, i =>
                        {
                            double newP = c * colP[i] - s * colQ[i];
                            double newQ = s * colP[i] + c * colQ[i];
                            A[i, p] = newP;
                            A[i, q] = newQ;
                        });

                        for (int i = 0; i < n; i++)
                        {
                            double newVp = c * V[i, p] - s * V[i, q];
                            double newVq = s * V[i, p] + c * V[i, q];
                            V[i, p] = newVp;
                            V[i, q] = newVq;
                        }
                    }
                }
            }

            var S = new double[minMN];
            var U = new double[m, minMN];

            for (int j = 0; j < minMN; j++)
            {
                double norm = 0;
                for (int i = 0; i < m; i++)
                    norm += A[i, j] * A[i, j];
                S[j] = System.Math.Sqrt(norm);

                if (S[j] > 1e-12)
                {
                    for (int i = 0; i < m; i++)
                        U[i, j] = A[i, j] / S[j];
                }
            }

            return (U, S, V);
        }

        private static double ComputeOffDiagonal(double[,] A, int m, int n)
        {
            double sum = 0;
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < m; i++)
                {
                    if (i != j)
                        sum += A[i, j] * A[i, j];
                }
            }
            return System.Math.Sqrt(sum);
        }

        private static double[] GetColumn(double[,] A, int col, int rows)
        {
            var result = new double[rows];
            for (int i = 0; i < rows; i++)
                result[i] = A[i, col];
            return result;
        }
    }
}
