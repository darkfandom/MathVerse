namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Represents a unitary matrix (U†U = I). Unitary matrices preserve inner products
/// and are used to describe quantum gate operations.
/// </summary>
public sealed class UnitaryMatrix
{
    private readonly ComplexMatrix _matrix;

    /// <summary>Gets the underlying matrix.</summary>
    public ComplexMatrix Matrix => _matrix;

    /// <summary>Gets the number of rows.</summary>
    public int Rows => _matrix.Rows;

    /// <summary>Gets the number of columns.</summary>
    public int Cols => _matrix.Cols;

    /// <summary>Creates a unitary matrix from a matrix.</summary>
    public UnitaryMatrix(ComplexMatrix matrix)
    {
        _matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        if (matrix.Rows != matrix.Cols)
            throw new ArgumentException("Unitary matrix must be square.", nameof(matrix));
    }

    /// <summary>Gets the element at the specified row and column.</summary>
    public Complex this[int row, int col] => _matrix[row, col];

    /// <summary>Returns the inverse of this unitary matrix, which is its conjugate transpose.</summary>
    public ComplexMatrix Inverse() => _matrix.ConjugateTranspose();

    /// <summary>Tests whether this matrix is unitary within the given tolerance.</summary>
    public bool IsUnitary(double tolerance = 1e-10)
    {
        ComplexMatrix product = _matrix.ConjugateTranspose().Multiply(_matrix);
        ComplexMatrix identity = ComplexMatrix.Identity(_matrix.Rows);

        for (int i = 0; i < product.Rows; i++)
        {
            for (int j = 0; j < product.Cols; j++)
            {
                double diff = (product[i, j] - identity[i, j]).Magnitude;
                if (diff > tolerance) return false;
            }
        }
        return true;
    }
}
