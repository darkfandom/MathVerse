namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Immutable complex-valued matrix for quantum computations.
/// </summary>
public sealed class ComplexMatrix
{
    private readonly Complex[,] _data;

    /// <summary>Gets the number of rows.</summary>
    public int Rows { get; }

    /// <summary>Gets the number of columns.</summary>
    public int Cols { get; }

    /// <summary>Gets the element at the specified row and column.</summary>
    public Complex this[int row, int col]
    {
        get => _data[row, col];
    }

    /// <summary>Creates a matrix from a 2D array of complex values.</summary>
    public ComplexMatrix(Complex[,] values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        Rows = values.GetLength(0);
        Cols = values.GetLength(1);
        if (Rows == 0 || Cols == 0) throw new ArgumentException("Matrix dimensions must be positive.", nameof(values));
        _data = (Complex[,])values.Clone();
    }

    /// <summary>Creates a matrix of the specified dimensions filled with zeros.</summary>
    public ComplexMatrix(int rows, int cols)
    {
        if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
        if (cols <= 0) throw new ArgumentOutOfRangeException(nameof(cols));
        Rows = rows;
        Cols = cols;
        _data = new Complex[rows, cols];
    }

    /// <summary>Multiplies this matrix by another matrix (O(n³) algorithm).</summary>
    public ComplexMatrix Multiply(ComplexMatrix other)
    {
        if (Cols != other.Rows)
            throw new ArgumentException($"Inner dimensions must match: ({Rows}×{Cols}) and ({other.Rows}×{other.Cols}).");

        var result = new Complex[Rows, other.Cols];
        for (int i = 0; i < Rows; i++)
        {
            for (int k = 0; k < Cols; k++)
            {
                Complex aik = _data[i, k];
                if (aik == Complex.Zero) continue;
                for (int j = 0; j < other.Cols; j++)
                {
                    result[i, j] += aik * other._data[k, j];
                }
            }
        }
        return new ComplexMatrix(result);
    }

    /// <summary>Multiplies this matrix by a vector.</summary>
    public ComplexVector Multiply(ComplexVector v)
    {
        if (Cols != v.Dimension)
            throw new ArgumentException($"Matrix columns ({Cols}) must equal vector dimension ({v.Dimension}).");

        var result = new Complex[Rows];
        for (int i = 0; i < Rows; i++)
        {
            Complex sum = Complex.Zero;
            for (int j = 0; j < Cols; j++)
                sum += _data[i, j] * v[j];
            result[i] = sum;
        }
        return new ComplexVector(result);
    }

    /// <summary>Adds two matrices.</summary>
    public ComplexMatrix Add(ComplexMatrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match.");

        var result = new Complex[Rows, Cols];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                result[i, j] = _data[i, j] + other._data[i, j];
        return new ComplexMatrix(result);
    }

    /// <summary>Scales the matrix by a complex scalar.</summary>
    public ComplexMatrix Scale(Complex scalar)
    {
        var result = new Complex[Rows, Cols];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                result[i, j] = _data[i, j] * scalar;
        return new ComplexMatrix(result);
    }

    /// <summary>Returns the transpose of this matrix.</summary>
    public ComplexMatrix Transpose()
    {
        var result = new Complex[Cols, Rows];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                result[j, i] = _data[i, j];
        return new ComplexMatrix(result);
    }

    /// <summary>Returns the conjugate (Hermitian) transpose of this matrix.</summary>
    public ComplexMatrix ConjugateTranspose()
    {
        var result = new Complex[Cols, Rows];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                result[j, i] = Complex.Conjugate(_data[i, j]);
        return new ComplexMatrix(result);
    }

    /// <summary>Returns the conjugate transpose (alias for ConjugateTranspose).</summary>
    public ComplexMatrix Dagger() => ConjugateTranspose();

    /// <summary>Computes the trace of a square matrix.</summary>
    public Complex Trace()
    {
        if (Rows != Cols)
            throw new InvalidOperationException("Trace is only defined for square matrices.");

        Complex sum = Complex.Zero;
        for (int i = 0; i < Rows; i++)
            sum += _data[i, i];
        return sum;
    }

    /// <summary>Computes the Frobenius norm of this matrix.</summary>
    public double FrobeniusNorm()
    {
        double sum = 0.0;
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
            {
                double mag = _data[i, j].Magnitude;
                sum += mag * mag;
            }
        return System.Math.Sqrt(sum);
    }

    /// <summary>Computes the Kronecker (tensor) product with another matrix.</summary>
    public ComplexMatrix TensorProduct(ComplexMatrix other)
    {
        var result = new Complex[Rows * other.Rows, Cols * other.Cols];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                for (int k = 0; k < other.Rows; k++)
                    for (int l = 0; l < other.Cols; l++)
                        result[i * other.Rows + k, j * other.Cols + l] = _data[i, j] * other._data[k, l];
        return new ComplexMatrix(result);
    }

    /// <summary>Returns the identity matrix of the specified size.</summary>
    public static ComplexMatrix Identity(int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        var result = new Complex[size, size];
        for (int i = 0; i < size; i++)
            result[i, i] = Complex.One;
        return new ComplexMatrix(result);
    }

    /// <summary>Returns a zero matrix of the specified dimensions.</summary>
    public static ComplexMatrix Zero(int rows, int cols) => new ComplexMatrix(rows, cols);
}
