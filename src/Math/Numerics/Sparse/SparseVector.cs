namespace MathVerse.Math.Numerics.Sparse;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed record SparseVector
{
    public int Size { get; }
    public ImmutableArray<int> Indices { get; }
    public ImmutableArray<double> Values { get; }
    public int NonZeros => Indices.Length;

    public SparseVector(int size, ImmutableArray<int> indices, ImmutableArray<double> values)
    {
        if (indices.Length != values.Length)
            throw new ArgumentException("Indices and values must have same length");
        Size = size;
        Indices = indices;
        Values = values;
    }

    public static SparseVector FromDense(Vector dense)
    {
        var indices = new List<int>();
        var values = new List<double>();
        for (int i = 0; i < dense.Size; i++)
        {
            double val = dense[i];
            if (System.Math.Abs(val) > 1e-15)
            {
                indices.Add(i);
                values.Add(val);
            }
        }
        return new SparseVector(dense.Size, indices.ToImmutableArray(), values.ToImmutableArray());
    }

    public Vector ToDense()
    {
        var data = new double[Size];
        for (int k = 0; k < NonZeros; k++)
            data[Indices[k]] = Values[k];
        return new Vector(data.ToImmutableArray());
    }

    public SparseVector Add(SparseVector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vector sizes must match");

        var resultIndices = new List<int>();
        var resultValues = new List<double>();

        int i = 0, j = 0;
        while (i < NonZeros && j < other.NonZeros)
        {
            int ii = Indices[i], ij = other.Indices[j];
            if (ii < ij)
            {
                resultIndices.Add(ii);
                resultValues.Add(Values[i]);
                i++;
            }
            else if (ii > ij)
            {
                resultIndices.Add(ij);
                resultValues.Add(other.Values[j]);
                j++;
            }
            else
            {
                double sum = Values[i] + other.Values[j];
                if (System.Math.Abs(sum) > 1e-15)
                {
                    resultIndices.Add(ii);
                    resultValues.Add(sum);
                }
                i++; j++;
            }
        }
        while (i < NonZeros) { resultIndices.Add(Indices[i]); resultValues.Add(Values[i]); i++; }
        while (j < other.NonZeros) { resultIndices.Add(other.Indices[j]); resultValues.Add(other.Values[j]); j++; }

        return new SparseVector(Size, resultIndices.ToImmutableArray(), resultValues.ToImmutableArray());
    }

    public SparseVector Scale(double scalar)
    {
        if (scalar == 1.0) return this;
        if (scalar == 0.0) return new SparseVector(Size, ImmutableArray<int>.Empty, ImmutableArray<double>.Empty);
        var scaledValues = Values.Select(v => v * scalar).ToImmutableArray();
        return new SparseVector(Size, Indices, scaledValues);
    }

    public double Dot(SparseVector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vector sizes must match");
        double sum = 0;
        int i = 0, j = 0;
        while (i < NonZeros && j < other.NonZeros)
        {
            int ii = Indices[i], ij = other.Indices[j];
            if (ii < ij) i++;
            else if (ii > ij) j++;
            else { sum += Values[i] * other.Values[j]; i++; j++; }
        }
        return sum;
    }

    public double Dot(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vector sizes must match");
        double sum = 0;
        for (int k = 0; k < NonZeros; k++)
            sum += Values[k] * other[Indices[k]];
        return sum;
    }
}