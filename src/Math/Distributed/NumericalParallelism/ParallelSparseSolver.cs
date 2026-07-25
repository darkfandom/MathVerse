namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel sparse matrix solver using Jacobi iterative method.
    /// </summary>
    public sealed class ParallelSparseSolver
    {
        /// <summary>
        /// Solves Ax = b using parallel Jacobi iteration.
        /// </summary>
        /// <param name="A">Coefficient matrix.</param>
        /// <param name="b">Right-hand side vector.</param>
        /// <param name="maxIterations">Maximum number of iterations.</param>
        /// <param name="tolerance">Convergence tolerance.</param>
        /// <returns>Solution vector x.</returns>
        public double[] Solve(double[,] A, double[] b, int maxIterations = 1000, double tolerance = 1e-10)
        {
            int n = b.Length;
            if (n != A.GetLength(0) || n != A.GetLength(1))
                throw new ArgumentException("Matrix dimensions must match vector length.");

            var x = new double[n];
            var xNew = new double[n];

            for (int iter = 0; iter < maxIterations; iter++)
            {
                Parallel.For(0, n, i =>
                {
                    double sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            sum += A[i, j] * x[j];
                    }

                    if (System.Math.Abs(A[i, i]) < 1e-15)
                        throw new InvalidOperationException($"Zero diagonal element at row {i}.");

                    xNew[i] = (b[i] - sum) / A[i, i];
                });

                double error = 0;
                for (int i = 0; i < n; i++)
                {
                    double diff = xNew[i] - x[i];
                    error += diff * diff;
                }
                error = System.Math.Sqrt(error);

                Array.Copy(xNew, x, n);

                if (error < tolerance)
                    break;
            }

            return x;
        }

        /// <summary>
        /// Solves Ax = b using parallel Gauss-Seidel iteration.
        /// </summary>
        /// <param name="A">Coefficient matrix.</param>
        /// <param name="b">Right-hand side vector.</param>
        /// <param name="maxIterations">Maximum number of iterations.</param>
        /// <param name="tolerance">Convergence tolerance.</param>
        /// <returns>Solution vector x.</returns>
        public double[] SolveGaussSeidel(double[,] A, double[] b, int maxIterations = 1000, double tolerance = 1e-10)
        {
            int n = b.Length;
            var x = new double[n];

            for (int iter = 0; iter < maxIterations; iter++)
            {
                double maxDiff = 0;

                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            sum += A[i, j] * x[j];
                    }

                    double newVal = (b[i] - sum) / A[i, i];
                    double diff = System.Math.Abs(newVal - x[i]);
                    if (diff > maxDiff) maxDiff = diff;
                    x[i] = newVal;
                }

                if (maxDiff < tolerance)
                    break;
            }

            return x;
        }
    }
}
