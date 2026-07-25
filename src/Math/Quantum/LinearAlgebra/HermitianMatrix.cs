namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Represents a Hermitian matrix (equal to its own conjugate transpose).
/// All eigenvalues of a Hermitian matrix are guaranteed to be real.
/// </summary>
public sealed class HermitianMatrix
{
    private readonly ComplexMatrix _matrix;

    /// <summary>Gets the underlying matrix.</summary>
    public ComplexMatrix Matrix => _matrix;

    /// <summary>Gets the number of rows.</summary>
    public int Rows => _matrix.Rows;

    /// <summary>Gets the number of columns.</summary>
    public int Cols => _matrix.Cols;

    /// <summary>Creates a Hermitian matrix from a matrix, validating hermiticity.</summary>
    public HermitianMatrix(ComplexMatrix matrix)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        if (matrix.Rows != matrix.Cols)
            throw new ArgumentException("Hermitian matrix must be square.", nameof(matrix));

        ComplexMatrix conjTrans = matrix.ConjugateTranspose();
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Cols; j++)
            {
                if ((matrix[i, j] - conjTrans[i, j]).Magnitude > 1e-10)
                    throw new ArgumentException(
                        $"Matrix is not hermitian at [{i},{j}]: {matrix[i,j]} != {conjTrans[i,j]}");
            }
        }
        _matrix = matrix;
    }

    /// <summary>Gets the element at the specified row and column.</summary>
    public Complex this[int row, int col] => _matrix[row, col];

    /// <summary>Computes all eigenvalues of this Hermitian matrix using the Jacobi algorithm.</summary>
    public double[] Eigenvalues()
    {
        return EigenSolver.Eigenvalues(_matrix);
    }

    /// <summary>Computes the eigenvectors of this Hermitian matrix using the Jacobi algorithm.</summary>
    public ComplexMatrix Eigenvectors()
    {
        return EigenSolver.Eigenvectors(_matrix);
    }
}
