using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Transformations;

/// <summary>Represents an affine 2D transformation stored as a row-major 3x3 matrix.</summary>
/// <remarks>
/// The matrix layout is:
/// <code>
/// | M00  M01  M02 |   | a   b   tx |
/// | M10  M11  M12 | = | c   d   ty |
/// | M20  M21  M22 |   | 0   0   1  |
/// </code>
/// </remarks>
public readonly record struct Transform2D
{
    private readonly double _m00;
    private readonly double _m01;
    private readonly double _m02;
    private readonly double _m10;
    private readonly double _m11;
    private readonly double _m12;
    private readonly double _m20;
    private readonly double _m21;
    private readonly double _m22;

    private Transform2D(
        double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        _m00 = m00;
        _m01 = m01;
        _m02 = m02;
        _m10 = m10;
        _m11 = m11;
        _m12 = m12;
        _m20 = m20;
        _m21 = m21;
        _m22 = m22;
    }

    /// <summary>Gets the identity transformation.</summary>
    public static Transform2D Identity => new(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1);

    /// <summary>Creates a translation transformation.</summary>
    /// <param name="dx">The translation along the X axis.</param>
    /// <param name="dy">The translation along the Y axis.</param>
    /// <returns>The translation transformation.</returns>
    public static Transform2D Translation(double dx, double dy) => new(
        1, 0, dx,
        0, 1, dy,
        0, 0, 1);

    /// <summary>Creates a rotation transformation.</summary>
    /// <param name="angleRadians">The rotation angle in radians (counter-clockwise).</param>
    /// <returns>The rotation transformation.</returns>
    public static Transform2D Rotation(double angleRadians)
    {
        double c = System.Math.Cos(angleRadians);
        double s = System.Math.Sin(angleRadians);
        return new(
            c, -s, 0,
            s,  c, 0,
            0,  0, 1);
    }

    /// <summary>Creates a non-uniform scaling transformation.</summary>
    /// <param name="sx">The scale factor along the X axis.</param>
    /// <param name="sy">The scale factor along the Y axis.</param>
    /// <returns>The scaling transformation.</returns>
    public static Transform2D Scaling(double sx, double sy) => new(
        sx, 0, 0,
        0, sy, 0,
        0,  0, 1);

    /// <summary>Creates a uniform scaling transformation.</summary>
    /// <param name="uniform">The uniform scale factor.</param>
    /// <returns>The scaling transformation.</returns>
    public static Transform2D Scaling(double uniform) => Scaling(uniform, uniform);

    /// <summary>Creates a reflection transformation across the axis defined by the given direction vector.</summary>
    /// <param name="axis">The direction vector defining the reflection axis.</param>
    /// <returns>The reflection transformation.</returns>
    public static Transform2D Reflection(Vector2D axis)
    {
        Vector2D n = axis.Normalize();
        double nx = n.X;
        double ny = n.Y;
        return new(
            2.0 * nx * nx - 1.0, 2.0 * nx * ny,     0,
            2.0 * nx * ny,       2.0 * ny * ny - 1.0, 0,
            0,                   0,                   1);
    }

    /// <summary>Creates a shearing transformation.</summary>
    /// <param name="shx">The shear factor along the X axis (shifts X per unit Y).</param>
    /// <param name="shy">The shear factor along the Y axis (shifts Y per unit X).</param>
    /// <returns>The shearing transformation.</returns>
    public static Transform2D Shearing(double shx, double shy) => new(
        1,  shx, 0,
        shy, 1,  0,
        0,   0,  1);

    /// <summary>Applies this transformation to a point.</summary>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public Point2D TransformPoint(Point2D p)
    {
        double x = _m00 * p.X + _m01 * p.Y + _m02;
        double y = _m10 * p.X + _m11 * p.Y + _m12;
        return new Point2D(x, y);
    }

    /// <summary>Applies this transformation to a vector (ignoring translation).</summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public Vector2D TransformVector(Vector2D v)
    {
        double x = _m00 * v.X + _m01 * v.Y;
        double y = _m10 * v.X + _m11 * v.Y;
        return new Vector2D(x, y);
    }

    /// <summary>Multiplies this transformation by another transformation (this * other).</summary>
    /// <param name="other">The other transformation.</param>
    /// <returns>The composed transformation.</returns>
    public Transform2D Multiply(Transform2D other) => new(
        _m00 * other._m00 + _m01 * other._m10 + _m02 * other._m20,
        _m00 * other._m01 + _m01 * other._m11 + _m02 * other._m21,
        _m00 * other._m02 + _m01 * other._m12 + _m02 * other._m22,
        _m10 * other._m00 + _m11 * other._m10 + _m12 * other._m20,
        _m10 * other._m01 + _m11 * other._m11 + _m12 * other._m21,
        _m10 * other._m02 + _m11 * other._m12 + _m12 * other._m22,
        _m20 * other._m00 + _m21 * other._m10 + _m22 * other._m20,
        _m20 * other._m01 + _m21 * other._m11 + _m22 * other._m21,
        _m20 * other._m02 + _m21 * other._m12 + _m22 * other._m22);

    /// <summary>Computes the inverse of this transformation.</summary>
    /// <returns>The inverse transformation.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the matrix is singular (determinant is zero).</exception>
    public Transform2D Inverse()
    {
        double det = _m00 * _m11 - _m01 * _m10;
        if (System.Math.Abs(det) < 1e-15)
            throw new System.InvalidOperationException("Transform2D matrix is singular and cannot be inverted.");

        double invDet = 1.0 / det;
        return new Transform2D(
             _m11 * invDet, -_m01 * invDet, (_m01 * _m12 - _m02 * _m11) * invDet,
            -_m10 * invDet,  _m00 * invDet, (_m02 * _m10 - _m00 * _m12) * invDet,
             0,              0,              1);
    }

    /// <summary>Computes the determinant of this transformation matrix.</summary>
    /// <returns>The determinant value.</returns>
    public double Determinant() => _m00 * _m11 - _m01 * _m10;

    /// <summary>Composes this transformation with another (this * other).</summary>
    /// <param name="other">The other transformation to compose with.</param>
    /// <returns>The composed transformation.</returns>
    public Transform2D Compose(Transform2D other) => Multiply(other);

    /// <summary>Indexer for matrix element access by row and column.</summary>
    /// <param name="row">The row index (0-2).</param>
    /// <param name="col">The column index (0-2).</param>
    /// <returns>The matrix element at the specified position.</returns>
    public double this[int row, int col]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (row, col) switch
        {
            (0, 0) => _m00, (0, 1) => _m01, (0, 2) => _m02,
            (1, 0) => _m10, (1, 1) => _m11, (1, 2) => _m12,
            (2, 0) => _m20, (2, 1) => _m21, (2, 2) => _m22,
            _ => throw new System.IndexOutOfRangeException(
                $"Transform2D index [{row}, {col}] out of range [0, 2].")
        };
    }

    /// <summary>Multiplies two transformations.</summary>
    /// <param name="a">The left transformation.</param>
    /// <param name="b">The right transformation.</param>
    /// <returns>The product a * b.</returns>
    public static Transform2D operator *(Transform2D a, Transform2D b) => a.Multiply(b);

    /// <summary>Applies a transformation to a point.</summary>
    /// <param name="t">The transformation.</param>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public static Point2D operator *(Transform2D t, Point2D p) => t.TransformPoint(p);

    /// <summary>Returns a string representation of this transformation.</summary>
    public override string ToString()
        => $"Transform2D([{_m00:F4}, {_m01:F4}, {_m02:F4}; {_m10:F4}, {_m11:F4}, {_m12:F4}; {_m20:F4}, {_m21:F4}, {_m22:F4}])";
}
