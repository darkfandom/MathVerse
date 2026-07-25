using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Matrix;

/// <summary>Represents an immutable 2x2 matrix stored in row-major order.</summary>
public readonly record struct Matrix2x2(double M00, double M01, double M10, double M11)
{
    /// <summary>The identity matrix.</summary>
    public static readonly Matrix2x2 Identity = new(1, 0, 0, 1);

    /// <summary>The zero matrix.</summary>
    public static readonly Matrix2x2 Zero = new(0, 0, 0, 0);

    /// <summary>Element at row 0, column 0.</summary>
    public double M00 { get; } = M00;

    /// <summary>Element at row 0, column 1.</summary>
    public double M01 { get; } = M01;

    /// <summary>Element at row 1, column 0.</summary>
    public double M10 { get; } = M10;

    /// <summary>Element at row 1, column 1.</summary>
    public double M11 { get; } = M11;

    /// <summary>Gets the element at the specified row and column.</summary>
    public double this[int row, int col]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (row, col) switch
        {
            (0, 0) => M00, (0, 1) => M01,
            (1, 0) => M10, (1, 1) => M11,
            _ => throw new System.IndexOutOfRangeException($"Matrix2x2 index [{row},{col}] out of range.")
        };
    }

    /// <summary>Gets the determinant.</summary>
    public double Determinant => M00 * M11 - M01 * M10;

    /// <summary>Gets the trace (sum of diagonal elements).</summary>
    public double Trace => M00 + M11;

    /// <summary>Returns the transpose.</summary>
    public Matrix2x2 Transpose() => new(M00, M10, M01, M11);

    /// <summary>Returns the inverse, or throws if singular.</summary>
    public Matrix2x2 Inverse()
    {
        double det = Determinant;
        if (System.Math.Abs(det) < 1e-15)
            throw new System.InvalidOperationException("Matrix2x2 is singular.");
        double invDet = 1.0 / det;
        return new Matrix2x2(M11 * invDet, -M01 * invDet, -M10 * invDet, M00 * invDet);
    }

    /// <summary>Computes the adjugate (adjoint) matrix.</summary>
    public Matrix2x2 Adjugate() => new(M11, -M01, -M10, M00);

    /// <summary>Returns the cofactor matrix.</summary>
    public Matrix2x2 Cofactor() => new(M11, -M10, -M01, M00);

    /// <summary>Multiplies by a scalar.</summary>
    public Matrix2x2 Scale(double s) => new(M00 * s, M01 * s, M10 * s, M11 * s);

    /// <summary>Multiplies two matrices.</summary>
    public Matrix2x2 Multiply(Matrix2x2 other) => new(
        M00 * other.M00 + M01 * other.M10,
        M00 * other.M01 + M01 * other.M11,
        M10 * other.M00 + M11 * other.M10,
        M10 * other.M01 + M11 * other.M11);

    /// <summary>Transforms a 2D vector.</summary>
    public Geometry2D.Vector2D Transform(Geometry2D.Vector2D v) =>
        new(M00 * v.X + M01 * v.Y, M10 * v.X + M11 * v.Y);

    /// <summary>Solves the 2x2 linear system Ax = b.</summary>
    public (double X, double Y) Solve(double bx, double by)
    {
        double det = Determinant;
        if (System.Math.Abs(det) < 1e-15)
            throw new System.InvalidOperationException("Matrix2x2 is singular.");
        double invDet = 1.0 / det;
        return ((M11 * bx - M01 * by) * invDet, (-M10 * bx + M00 * by) * invDet);
    }

    /// <summary>Operator overload for matrix multiplication.</summary>
    public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b) => a.Multiply(b);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Matrix2x2 operator *(Matrix2x2 a, double s) => a.Scale(s);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Matrix2x2 operator *(double s, Matrix2x2 a) => a.Scale(s);

    /// <inheritdoc/>
    public override string ToString() => $"[{M00:F4}, {M01:F4}; {M10:F4}, {M11:F4}]";
}
