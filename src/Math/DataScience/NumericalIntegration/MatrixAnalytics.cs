namespace MathVerse.Math.DataScience.NumericalIntegration;

using System;

/// <summary>
/// Provides matrix analytics operations including determinant, inverse, eigenvalues,
/// trace, Frobenius norm, and rank computation.
/// </summary>
public static class MatrixAnalytics
{
    /// <summary>
    /// Computes the determinant of a square matrix using LU decomposition.
    /// </summary>
    /// <param name="matrix">The square matrix.</param>
    /// <returns>The determinant value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the matrix is not square.</exception>
    public static double Determinant(double[,] matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int n = matrix.GetLength(0);
        if (matrix.GetLength(1) != n)
            throw new ArgumentException("Matrix must be square.", nameof(matrix));

        if (n == 1) return matrix[0, 0];
        if (n == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        double[,] lu = (double[,])matrix.Clone();
        double det = 1.0;
        int[] piv = new int[n];
        for (int i = 0; i < n; i++) piv[i] = i;

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(lu[col, col]);
            for (int row = col + 1; row < n; row++)
            {
                if (System.Math.Abs(lu[row, col]) > maxVal)
                {
                    maxVal = System.Math.Abs(lu[row, col]);
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                for (int j = 0; j < n; j++)
                    (lu[col, j], lu[maxRow, j]) = (lu[maxRow, j], lu[col, j]);
                det = -det;
            }

            if (System.Math.Abs(lu[col, col]) < 1e-15)
                return 0.0;

            det *= lu[col, col];

            for (int row = col + 1; row < n; row++)
            {
                lu[row, col] /= lu[col, col];
                for (int j = col + 1; j < n; j++)
                    lu[row, j] -= lu[row, col] * lu[col, j];
            }
        }

        return det;
    }

    /// <summary>
    /// Computes the inverse of a square matrix using Gauss-Jordan elimination.
    /// </summary>
    /// <param name="matrix">The square matrix to invert.</param>
    /// <returns>The inverse matrix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the matrix is not square or singular.</exception>
    public static double[,] Inverse(double[,] matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int n = matrix.GetLength(0);
        if (matrix.GetLength(1) != n)
            throw new ArgumentException("Matrix must be square.", nameof(matrix));

        double[,] augmented = new double[n, 2 * n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = matrix[i, j];
            augmented[i, n + i] = 1.0;
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
                    (augmented[col, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[col, j]);
            }

            double pivot = augmented[col, col];
            if (System.Math.Abs(pivot) < 1e-15)
                throw new ArgumentException("Matrix is singular and cannot be inverted.");

            for (int j = 0; j < 2 * n; j++)
                augmented[col, j] /= pivot;

            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = augmented[row, col];
                for (int j = 0; j < 2 * n; j++)
                    augmented[row, j] -= factor * augmented[col, j];
            }
        }

        double[,] result = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = augmented[i, n + j];

        return result;
    }

    /// <summary>
    /// Computes the dominant eigenvalue and eigenvector using the power iteration method.
    /// </summary>
    /// <param name="matrix">The square matrix.</param>
    /// <param name="maxIterations">Maximum number of iterations.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <returns>A tuple of (eigenvalue, eigenvector).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the matrix is not square or fails to converge.</exception>
    public static (double Eigenvalue, double[] Eigenvector) Eigenvalues(
        double[,] matrix,
        int maxIterations = 1000,
        double tolerance = 1e-10)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int n = matrix.GetLength(0);
        if (matrix.GetLength(1) != n)
            throw new ArgumentException("Matrix must be square.", nameof(matrix));

        double[] v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = 1.0 / System.Math.Sqrt(n);

        double eigenvalue = 0.0;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] w = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    w[i] += matrix[i, j] * v[j];
            }

            double norm = 0.0;
            for (int i = 0; i < n; i++)
                norm += w[i] * w[i];
            norm = System.Math.Sqrt(norm);

            if (norm < 1e-15)
                throw new ArgumentException("Power iteration converged to zero vector.");

            double[] newV = new double[n];
            for (int i = 0; i < n; i++)
                newV[i] = w[i] / norm;

            double newEigenvalue = 0.0;
            for (int i = 0; i < n; i++)
                newEigenvalue += newV[i] * w[i];

            if (System.Math.Abs(newEigenvalue - eigenvalue) < tolerance)
                return (newEigenvalue, newV);

            eigenvalue = newEigenvalue;
            v = newV;
        }

        return (eigenvalue, v);
    }

    /// <summary>
    /// Computes the trace (sum of diagonal elements) of a square matrix.
    /// </summary>
    /// <param name="matrix">The square matrix.</param>
    /// <returns>The trace value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    public static double Trace(double[,] matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int n = System.Math.Min(matrix.GetLength(0), matrix.GetLength(1));
        double trace = 0.0;
        for (int i = 0; i < n; i++)
            trace += matrix[i, i];
        return trace;
    }

    /// <summary>
    /// Computes the Frobenius norm of a matrix: sqrt(sum of squares of all elements).
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The Frobenius norm.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    public static double FrobeniusNorm(double[,] matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double sum = 0.0;

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sum += matrix[i, j] * matrix[i, j];

        return System.Math.Sqrt(sum);
    }

    /// <summary>
    /// Computes the rank of a matrix using Gaussian elimination.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="tolerance">Tolerance for considering a pivot as zero.</param>
    /// <returns>The rank of the matrix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    public static int Rank(double[,] matrix, double tolerance = 1e-10)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double[,] mat = (double[,])matrix.Clone();
        int rank = 0;

        for (int col = 0; col < cols && rank < rows; col++)
        {
            int maxRow = rank;
            for (int row = rank + 1; row < rows; row++)
            {
                if (System.Math.Abs(mat[row, col]) > System.Math.Abs(mat[maxRow, col]))
                    maxRow = row;
            }

            if (System.Math.Abs(mat[maxRow, col]) < tolerance)
                continue;

            if (maxRow != rank)
            {
                for (int j = 0; j < cols; j++)
                    (mat[rank, j], mat[maxRow, j]) = (mat[maxRow, j], mat[rank, j]);
            }

            for (int row = rank + 1; row < rows; row++)
            {
                double factor = mat[row, col] / mat[rank, col];
                for (int j = col; j < cols; j++)
                    mat[row, j] -= factor * mat[rank, j];
            }

            rank++;
        }

        return rank;
    }

    /// <summary>
    /// Computes the transpose of a matrix.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The transposed matrix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is null.</exception>
    public static double[,] Transpose(double[,] matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double[,] result = new double[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];

        return result;
    }
}
