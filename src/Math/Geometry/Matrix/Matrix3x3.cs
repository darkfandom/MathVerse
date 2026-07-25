using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Matrix;

/// <summary>Represents an immutable 3x3 matrix stored in row-major order.</summary>
public readonly record struct Matrix3x3(double M00, double M01, double M02,
                                        double M10, double M11, double M12,
                                        double M20, double M21, double M22)
{
    /// <summary>The identity matrix.</summary>
    public static readonly Matrix3x3 Identity = new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>The zero matrix.</summary>
    public static readonly Matrix3x3 Zero = new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>Element at row 0, column 0.</summary>
    public double M00 { get; } = M00;
    /// <summary>Element at row 0, column 1.</summary>
    public double M01 { get; } = M01;
    /// <summary>Element at row 0, column 2.</summary>
    public double M02 { get; } = M02;
    /// <summary>Element at row 1, column 0.</summary>
    public double M10 { get; } = M10;
    /// <summary>Element at row 1, column 1.</summary>
    public double M11 { get; } = M11;
    /// <summary>Element at row 1, column 2.</summary>
    public double M12 { get; } = M12;
    /// <summary>Element at row 2, column 0.</summary>
    public double M20 { get; } = M20;
    /// <summary>Element at row 2, column 1.</summary>
    public double M21 { get; } = M21;
    /// <summary>Element at row 2, column 2.</summary>
    public double M22 { get; } = M22;

    /// <summary>Gets the element at the specified row and column.</summary>
    public double this[int row, int col]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (row, col) switch
        {
            (0, 0) => M00, (0, 1) => M01, (0, 2) => M02,
            (1, 0) => M10, (1, 1) => M11, (1, 2) => M12,
            (2, 0) => M20, (2, 1) => M21, (2, 2) => M22,
            _ => throw new System.IndexOutOfRangeException($"Matrix3x3 index [{row},{col}] out of range.")
        };
    }

    /// <summary>Gets the determinant.</summary>
    public double Determinant =>
        M00 * (M11 * M22 - M12 * M21) -
        M01 * (M10 * M22 - M12 * M20) +
        M02 * (M10 * M21 - M11 * M20);

    /// <summary>Gets the trace.</summary>
    public double Trace => M00 + M11 + M22;

    /// <summary>Returns the transpose.</summary>
    public Matrix3x3 Transpose() => new(M00, M10, M20, M01, M11, M21, M02, M12, M22);

    /// <summary>Returns the cofactor matrix.</summary>
    public Matrix3x3 Cofactor() => new(
        M11 * M22 - M12 * M21, M12 * M20 - M10 * M22, M10 * M21 - M11 * M20,
        M02 * M21 - M01 * M22, M00 * M22 - M02 * M20, M01 * M20 - M00 * M21,
        M01 * M12 - M02 * M11, M02 * M10 - M00 * M12, M00 * M11 - M01 * M10);

    /// <summary>Returns the adjugate (adjoint) matrix.</summary>
    public Matrix3x3 Adjugate() => Cofactor().Transpose();

    /// <summary>Returns the inverse, or throws if singular.</summary>
    public Matrix3x3 Inverse()
    {
        double det = Determinant;
        if (System.Math.Abs(det) < 1e-15)
            throw new System.InvalidOperationException("Matrix3x3 is singular.");
        return Adjugate().Scale(1.0 / det);
    }

    /// <summary>Multiplies by a scalar.</summary>
    public Matrix3x3 Scale(double s) => new(
        M00 * s, M01 * s, M02 * s,
        M10 * s, M11 * s, M12 * s,
        M20 * s, M21 * s, M22 * s);

    /// <summary>Multiplies two matrices.</summary>
    public Matrix3x3 Multiply(Matrix3x3 o) => new(
        M00 * o.M00 + M01 * o.M10 + M02 * o.M20,
        M00 * o.M01 + M01 * o.M11 + M02 * o.M21,
        M00 * o.M02 + M01 * o.M12 + M02 * o.M22,
        M10 * o.M00 + M11 * o.M10 + M12 * o.M20,
        M10 * o.M01 + M11 * o.M11 + M12 * o.M21,
        M10 * o.M02 + M11 * o.M12 + M12 * o.M22,
        M20 * o.M00 + M21 * o.M10 + M22 * o.M20,
        M20 * o.M01 + M21 * o.M11 + M22 * o.M21,
        M20 * o.M02 + M21 * o.M12 + M22 * o.M22);

    /// <summary>Transforms a 3D vector.</summary>
    public Vector3D Transform(Vector3D v) => new(
        M00 * v.X + M01 * v.Y + M02 * v.Z,
        M10 * v.X + M11 * v.Y + M12 * v.Z,
        M20 * v.X + M21 * v.Y + M22 * v.Z);

    /// <summary>Transforms a 3D point.</summary>
    public Point3D TransformPoint(Point3D p) => new(
        M00 * p.X + M01 * p.Y + M02 * p.Z,
        M10 * p.X + M11 * p.Y + M12 * p.Z,
        M20 * p.X + M21 * p.Y + M22 * p.Z);

    /// <summary>Extracts the upper-left 3x3 from a Transform3D.</summary>
    public static Matrix3x3 FromTransform(Transformations.Transform3D t) => new(
        t[0, 0], t[0, 1], t[0, 2],
        t[1, 0], t[1, 1], t[1, 2],
        t[2, 0], t[2, 1], t[2, 2]);

    /// <summary>Creates a rotation matrix from axis-angle.</summary>
    public static Matrix3x3 RotationAxis(Vector3D axis, double angle)
    {
        Vector3D u = axis.Normalize();
        double c = System.Math.Cos(angle), s = System.Math.Sin(angle), t = 1.0 - c;
        return new Matrix3x3(
            t * u.X * u.X + c, t * u.X * u.Y - s * u.Z, t * u.X * u.Z + s * u.Y,
            t * u.X * u.Y + s * u.Z, t * u.Y * u.Y + c, t * u.Y * u.Z - s * u.X,
            t * u.X * u.Z - s * u.Y, t * u.Y * u.Z + s * u.X, t * u.Z * u.Z + c);
    }

    /// <summary>Creates a scaling matrix.</summary>
    public static Matrix3x3 Scaling(double sx, double sy, double sz) => new(
        sx, 0, 0, 0, sy, 0, 0, 0, sz);

    /// <summary>Solves the 3x3 linear system Ax = b.</summary>
    public Vector3D Solve(Vector3D b)
    {
        double det = Determinant;
        if (System.Math.Abs(det) < 1e-15)
            throw new System.InvalidOperationException("Matrix3x3 is singular.");
        double invDet = 1.0 / det;
        return new Vector3D(
            (b.X * (M11 * M22 - M12 * M21) - M01 * (b.Y * M22 - M12 * b.Z) + M02 * (b.Y * M21 - M11 * b.Z)) * invDet,
            (M00 * (b.Y * M22 - M12 * b.Z) - b.X * (M10 * M22 - M12 * M20) + M02 * (M10 * b.Z - b.Y * M20)) * invDet,
            (M00 * (M11 * b.Z - b.Y * M21) - M01 * (M10 * b.Z - b.Y * M20) + b.X * (M10 * M21 - M11 * M20)) * invDet);
    }

    /// <summary>Operator overload for matrix multiplication.</summary>
    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b) => a.Multiply(b);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Matrix3x3 operator *(Matrix3x3 a, double s) => a.Scale(s);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Matrix3x3 operator *(double s, Matrix3x3 a) => a.Scale(s);

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{M00:F4}, {M01:F4}, {M02:F4}; {M10:F4}, {M11:F4}, {M12:F4}; {M20:F4}, {M21:F4}, {M22:F4}]";
}
