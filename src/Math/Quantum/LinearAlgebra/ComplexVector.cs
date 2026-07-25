namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Immutable complex-valued vector for quantum computations.
/// </summary>
public sealed class ComplexVector
{
    private readonly Complex[] _data;

    /// <summary>Gets the dimension of the vector.</summary>
    public int Dimension => _data.Length;

    /// <summary>Gets the underlying data as a read-only span.</summary>
    public ReadOnlySpan<Complex> Data => _data;

    /// <summary>Creates a zero vector of the specified dimension.</summary>
    public ComplexVector(int dimension)
    {
        _data = dimension > 0 ? new Complex[dimension] : throw new ArgumentOutOfRangeException(nameof(dimension));
    }

    /// <summary>Creates a vector from an array of complex values.</summary>
    public ComplexVector(Complex[] values)
    {
        _data = values ?? throw new ArgumentNullException(nameof(values));
        if (values.Length == 0) throw new ArgumentException("Vector cannot be empty.", nameof(values));
    }

    /// <summary>Gets the element at the specified index.</summary>
    public Complex this[int index]
    {
        get => _data[index];
    }

    /// <summary>Returns a new vector with the specified element set.</summary>
    public ComplexVector WithElement(int index, Complex value)
    {
        var copy = (Complex[])_data.Clone();
        copy[index] = value;
        return new ComplexVector(copy);
    }

    /// <summary>Computes the L2 norm of this vector.</summary>
    public double Norm()
    {
        double sum = 0.0;
        for (int i = 0; i < _data.Length; i++)
            sum += _data[i].Magnitude * _data[i].Magnitude;
        return System.Math.Sqrt(sum);
    }

    /// <summary>Returns a normalized copy of this vector.</summary>
    public ComplexVector Normalize()
    {
        double norm = Norm();
        if (norm < 1e-15) throw new InvalidOperationException("Cannot normalize a zero vector.");
        var result = new Complex[_data.Length];
        for (int i = 0; i < _data.Length; i++)
            result[i] = _data[i] / norm;
        return new ComplexVector(result);
    }

    /// <summary>Computes the inner product (Hermitian dot product) with another vector.</summary>
    public Complex InnerProduct(ComplexVector other)
    {
        if (other.Dimension != Dimension) throw new ArgumentException("Vector dimensions must match.");
        Complex sum = Complex.Zero;
        for (int i = 0; i < _data.Length; i++)
            sum += Complex.Conjugate(_data[i]) * other._data[i];
        return sum;
    }

    /// <summary>Computes the outer product with another vector.</summary>
    public ComplexMatrix OuterProduct(ComplexVector other)
    {
        var result = new Complex[Dimension, other.Dimension];
        for (int i = 0; i < Dimension; i++)
            for (int j = 0; j < other.Dimension; j++)
                result[i, j] = _data[i] * Complex.Conjugate(other._data[j]);
        return new ComplexMatrix(result);
    }

    /// <summary>Adds two vectors.</summary>
    public ComplexVector Add(ComplexVector other)
    {
        if (other.Dimension != Dimension) throw new ArgumentException("Vector dimensions must match.");
        var result = new Complex[Dimension];
        for (int i = 0; i < Dimension; i++)
            result[i] = _data[i] + other._data[i];
        return new ComplexVector(result);
    }

    /// <summary>Subtracts another vector from this vector.</summary>
    public ComplexVector Subtract(ComplexVector other)
    {
        if (other.Dimension != Dimension) throw new ArgumentException("Vector dimensions must match.");
        var result = new Complex[Dimension];
        for (int i = 0; i < Dimension; i++)
            result[i] = _data[i] - other._data[i];
        return new ComplexVector(result);
    }

    /// <summary>Scales the vector by a complex scalar.</summary>
    public ComplexVector Scale(Complex scalar)
    {
        var result = new Complex[Dimension];
        for (int i = 0; i < Dimension; i++)
            result[i] = _data[i] * scalar;
        return new ComplexVector(result);
    }

    /// <summary>Returns the tensor product with another vector.</summary>
    public ComplexVector TensorProduct(ComplexVector other)
    {
        var result = new Complex[Dimension * other.Dimension];
        for (int i = 0; i < Dimension; i++)
            for (int j = 0; j < other.Dimension; j++)
                result[i * other.Dimension + j] = _data[i] * other._data[j];
        return new ComplexVector(result);
    }

    /// <summary>Creates a computational basis vector.</summary>
    public static ComplexVector BasisVector(int dimension, int index)
    {
        if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
        if (index < 0 || index >= dimension) throw new ArgumentOutOfRangeException(nameof(index));
        var result = new Complex[dimension];
        result[index] = Complex.One;
        return new ComplexVector(result);
    }

    /// <summary>Creates a zero vector.</summary>
    public static ComplexVector Zero(int dimension) => new ComplexVector(dimension);
}
