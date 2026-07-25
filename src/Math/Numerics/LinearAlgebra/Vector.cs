namespace MathVerse.Math.Numerics.LinearAlgebra;

using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;

public readonly record struct Vector
{
    internal readonly ImmutableArray<double> _values;

    public Vector(ImmutableArray<double> values)
    {
        _values = values;
    }

    public Vector(params double[] values)
    {
        _values = values.ToImmutableArray();
    }

    public int Size => _values.Length;

    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values[index];
    }

    public ImmutableArray<double> Values => _values;

    public static Vector Zero => new(ImmutableArray<double>.Empty);

    public static Vector ZeroOf(int size) => new(ImmutableArray<double>.Empty.AddRange(Enumerable.Repeat(0.0, size)));

    public static Vector One(int size) => new(ImmutableArray<double>.Empty.AddRange(Enumerable.Repeat(1.0, size)));

    public static Vector Basis(int size, int index)
    {
        var values = Enumerable.Repeat(0.0, size).ToArray();
        values[index] = 1.0;
        return new Vector(values.ToImmutableArray());
    }

    public double Norm() => System.Math.Sqrt(Dot(this));

    public double Norm1() => _values.Sum(System.Math.Abs);

    public double NormInf() => _values.Max(System.Math.Abs);

    public double Dot(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vectors must have same size");
        double sum = 0;
        for (int i = 0; i < Size; i++) sum += _values[i] * other._values[i];
        return sum;
    }

    public Vector Add(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vectors must have same size");
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = _values[i] + other._values[i];
        return new Vector(result.ToImmutableArray());
    }

    public Vector Subtract(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vectors must have same size");
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = _values[i] - other._values[i];
        return new Vector(result.ToImmutableArray());
    }

    public Vector Scale(double scalar)
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = _values[i] * scalar;
        return new Vector(result.ToImmutableArray());
    }

    public Vector Negate() => Scale(-1.0);

    public Vector ElementWiseMultiply(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vectors must have same size");
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = _values[i] * other._values[i];
        return new Vector(result.ToImmutableArray());
    }

    public Vector ElementWiseDivide(Vector other)
    {
        if (Size != other.Size) throw new ArgumentException("Vectors must have same size");
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = _values[i] / other._values[i];
        return new Vector(result.ToImmutableArray());
    }

    public Vector Abs()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Abs(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public Vector Sqrt()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Sqrt(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public Vector Exp()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Exp(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public Vector Log()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Log(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public Vector Sin()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Sin(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public Vector Cos()
    {
        var result = new double[Size];
        for (int i = 0; i < Size; i++) result[i] = System.Math.Cos(_values[i]);
        return new Vector(result.ToImmutableArray());
    }

    public static Vector operator +(Vector a, Vector b) => a.Add(b);

    public static Vector operator -(Vector a, Vector b) => a.Subtract(b);

    public static Vector operator *(Vector a, double scalar) => a.Scale(scalar);

    public static Vector operator *(double scalar, Vector a) => a.Scale(scalar);

    public static Vector operator -(Vector a) => a.Negate();

    public double[] ToArray() => _values.ToArray();

    public ImmutableArray<double> ToImmutableArray() => _values;

    public override string ToString() => $"Vector[{string.Join(", ", _values)}]";

    public static implicit operator Vector(double[] values) => new(values.ToImmutableArray());

    public static implicit operator Vector(ImmutableArray<double> values) => new(values);
}

public static class VectorOperations
{
    public static double Dot(Vector a, Vector b) => a.Dot(b);

    public static double Norm(Vector v) => v.Norm();

    public static double Norm1(Vector v) => v.Norm1();

    public static double NormInf(Vector v) => v.NormInf();

    public static double Distance(Vector a, Vector b) => (a - b).Norm();

    public static double Distance1(Vector a, Vector b) => (a - b).Norm1();

    public static double DistanceInf(Vector a, Vector b) => (a - b).NormInf();

    public static Vector Normalize(Vector v)
    {
        double norm = v.Norm();
        return norm > 0 ? v.Scale(1.0 / norm) : v;
    }

    public static Vector Normalize1(Vector v)
    {
        double norm = v.Norm1();
        return norm > 0 ? v.Scale(1.0 / norm) : v;
    }

    public static double CosineSimilarity(Vector a, Vector b) => a.Dot(b) / (a.Norm() * b.Norm());

    public static double Angle(Vector a, Vector b) => System.Math.Acos(System.Math.Clamp(CosineSimilarity(a, b), -1.0, 1.0));

    public static Vector Lerp(Vector a, Vector b, double t) => a.Add(b.Subtract(a).Scale(t));

    public static Vector Max(Vector a, Vector b)
    {
        int n = System.Math.Min(a.Size, b.Size);
        var result = new double[n];
        for (int i = 0; i < n; i++) result[i] = System.Math.Max(a[i], b[i]);
        return new Vector(result.ToImmutableArray());
    }

    public static Vector Min(Vector a, Vector b)
    {
        int n = System.Math.Min(a.Size, b.Size);
        var result = new double[n];
        for (int i = 0; i < n; i++) result[i] = System.Math.Min(a[i], b[i]);
        return new Vector(result.ToImmutableArray());
    }

    public static Vector Clamp(Vector v, double min, double max)
    {
        var result = new double[v.Size];
        for (int i = 0; i < v.Size; i++) result[i] = System.Math.Clamp(v[i], min, max);
        return new Vector(result.ToImmutableArray());
    }

    public static double Sum(Vector v) => v._values.Sum();

    public static double Mean(Vector v) => v.Size > 0 ? v._values.Average() : 0;

    public static double Variance(Vector v)
    {
        if (v.Size <= 1) return 0;
        double mean = Mean(v);
        double sum = 0;
        for (int i = 0; i < v.Size; i++)
        {
            double diff = v[i] - mean;
            sum += diff * diff;
        }
        return sum / (v.Size - 1);
    }

    public static double StdDev(Vector v) => System.Math.Sqrt(Variance(v));

    public static (double min, double max) MinMax(Vector v)
    {
        if (v.Size == 0) return (0, 0);
        double min = v[0], max = v[0];
        for (int i = 1; i < v.Size; i++)
        {
            if (v[i] < min) min = v[i];
            if (v[i] > max) max = v[i];
        }
        return (min, max);
    }
}