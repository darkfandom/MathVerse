namespace MathVerse.Math.DataScience.NumericalIntegration;

using System;

/// <summary>
/// Solves least squares problems using the normal equations method: A^T A x = A^T b.
/// </summary>
public static class LeastSquares
{
    /// <summary>
    /// Solves the least squares problem min ||Ax - b||^2 using normal equations.
    /// </summary>
    /// <param name="A">The coefficient matrix (m x n, m >= n).</param>
    /// <param name="b">The right-hand side vector of length m.</param>
    /// <returns>The least squares solution vector of length n.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="A"/> or <paramref name="b"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when dimensions are incompatible.</exception>
    public static double[] Solve(double[,] A, double[] b)
    {
        if (A is null) throw new ArgumentNullException(nameof(A));
        if (b is null) throw new ArgumentNullException(nameof(b));

        int m = A.GetLength(0);
        int n = A.GetLength(1);

        if (b.Length != m)
            throw new ArgumentException($"Vector length ({b.Length}) must match matrix row count ({m}).");

        if (m < n)
            throw new ArgumentException($"Overdetermined system requires m >= n (got m={m}, n={n}).");

        double[,] AtA = MultiplyTranspose(A, A);
        double[] Atb = MultiplyTransposeVector(A, b);

        return SolveLinearSystem(AtA, Atb);
    }

    /// <summary>
    /// Solves the least squares problem with Tikhonov (ridge) regularization.
    /// min ||Ax - b||^2 + lambda * ||x||^2.
    /// </summary>
    /// <param name="A">The coefficient matrix.</param>
    /// <param name="b">The right-hand side vector.</param>
    /// <param name="lambda">The regularization parameter.</param>
    /// <returns>The regularized solution vector.</returns>
    public static double[] SolveRegularized(double[,] A, double[] b, double lambda)
    {
        if (A is null) throw new ArgumentNullException(nameof(A));
        if (b is null) throw new ArgumentNullException(nameof(b));

        int m = A.GetLength(0);
        int n = A.GetLength(1);

        if (b.Length != m)
            throw new ArgumentException($"Vector length ({b.Length}) must match matrix row count ({m}).");

        double[,] AtA = MultiplyTranspose(A, A);

        for (int i = 0; i < n; i++)
            AtA[i, i] += lambda;

        double[] Atb = MultiplyTransposeVector(A, b);
        return SolveLinearSystem(AtA, Atb);
    }

    /// <summary>
    /// Computes the residual vector b - Ax for a given solution.
    /// </summary>
    /// <param name="A">The coefficient matrix.</param>
    /// <param name="x">The solution vector.</param>
    /// <param name="b">The right-hand side vector.</param>
    /// <returns>The residual vector.</returns>
    public static double[] ComputeResidual(double[,] A, double[] x, double[] b)
    {
        if (A is null) throw new ArgumentNullException(nameof(A));
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (b is null) throw new ArgumentNullException(nameof(b));

        int m = A.GetLength(0);
        int n = A.GetLength(1);
        double[] residual = new double[m];

        for (int i = 0; i < m; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
                sum += A[i, j] * x[j];
            residual[i] = b[i] - sum;
        }

        return residual;
    }

    /// <summary>
    /// Computes the sum of squared residuals.
    /// </summary>
    /// <param name="A">The coefficient matrix.</param>
    /// <param name="x">The solution vector.</param>
    /// <param name="b">The right-hand side vector.</param>
    /// <returns>The sum of squared residuals.</returns>
    public static double ComputeResidualSumOfSquares(double[,] A, double[] x, double[] b)
    {
        double[] residual = ComputeResidual(A, x, b);
        double sum = 0.0;
        for (int i = 0; i < residual.Length; i++)
            sum += residual[i] * residual[i];
        return sum;
    }

    private static double[,] MultiplyTranspose(double[,] A, double[,] B)
    {
        int mA = A.GetLength(0);
        int nA = A.GetLength(1);
        int nB = B.GetLength(1);

        double[,] result = new double[nA, nB];

        for (int i = 0; i < nA; i++)
        {
            for (int j = 0; j < nB; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < mA; k++)
                    sum += A[k, i] * B[k, j];
                result[i, j] = sum;
            }
        }

        return result;
    }

    private static double[] MultiplyTransposeVector(double[,] A, double[] b)
    {
        int m = A.GetLength(0);
        int n = A.GetLength(1);
        double[] result = new double[n];

        for (int j = 0; j < n; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < m; i++)
                sum += A[i, j] * b[i];
            result[j] = sum;
        }

        return result;
    }

    private static double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        double[,] augmented = new double[n, n + 1];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = A[i, j];
            augmented[i, n] = b[i];
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
                for (int j = 0; j <= n; j++)
                    (augmented[col, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[col, j]);
            }

            if (System.Math.Abs(augmented[col, col]) < 1e-15)
                throw new InvalidOperationException("Singular matrix encountered in least squares solve.");

            for (int row = col + 1; row < n; row++)
            {
                double factor = augmented[row, col] / augmented[col, col];
                for (int j = col; j <= n; j++)
                    augmented[row, j] -= factor * augmented[col, j];
            }
        }

        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = augmented[i, n];
            for (int j = i + 1; j < n; j++)
                sum -= augmented[i, j] * x[j];
            x[i] = sum / augmented[i, i];
        }

        return x;
    }
}
