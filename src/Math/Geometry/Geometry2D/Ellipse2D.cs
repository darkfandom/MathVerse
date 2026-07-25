using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents an ellipse defined by a center, semi-major axis, semi-minor axis, and rotation angle.</summary>
public readonly record struct Ellipse2D(Point2D Center, double SemiMajor, double SemiMinor, double RotationAngle)
{
    /// <summary>The center of the ellipse.</summary>
    public Point2D Center { get; } = Center;

    /// <summary>The semi-major axis length.</summary>
    public double SemiMajor { get; } = SemiMajor;

    /// <summary>The semi-minor axis length.</summary>
    public double SemiMinor { get; } = SemiMinor;

    /// <summary>The rotation angle in radians.</summary>
    public double RotationAngle { get; } = RotationAngle;

    /// <summary>Returns a point on the ellipse at the given parameter.</summary>
    /// <param name="t">The parameter in radians (0 to 2*PI).</param>
    /// <returns>The point on the ellipse at parameter t.</returns>
    public Point2D PointAt(double t)
    {
        double cosA = System.Math.Cos(RotationAngle);
        double sinA = System.Math.Sin(RotationAngle);
        double cosT = System.Math.Cos(t);
        double sinT = System.Math.Sin(t);
        double x = SemiMajor * cosT;
        double y = SemiMinor * sinT;
        return new Point2D(
            Center.X + x * cosA - y * sinA,
            Center.Y + x * sinA + y * cosA);
    }

    /// <summary>Returns the tangent vector at the given parameter.</summary>
    /// <param name="t">The parameter in radians.</param>
    /// <returns>The tangent vector (normalized).</returns>
    public Vector2D TangentAt(double t)
    {
        double cosA = System.Math.Cos(RotationAngle);
        double sinA = System.Math.Sin(RotationAngle);
        double cosT = System.Math.Cos(t);
        double sinT = System.Math.Sin(t);
        double dx = -SemiMajor * sinT;
        double dy = SemiMinor * cosT;
        return new Vector2D(dx * cosA - dy * sinA, dx * sinA + dy * cosA).Normalize();
    }

    /// <summary>Gets the area of the ellipse.</summary>
    public double Area => System.Math.PI * SemiMajor * SemiMinor;

    /// <summary>Computes the perimeter using Ramanujan's approximation.</summary>
    /// <returns>An approximation of the perimeter.</returns>
    public double Perimeter()
    {
        double a = SemiMajor;
        double b = SemiMinor;
        double h = ((a - b) * (a - b)) / ((a + b) * (a + b));
        return System.Math.PI * (a + b) * (1.0 + (3.0 * h) / (10.0 + System.Math.Sqrt(4.0 - 3.0 * h)));
    }

    /// <summary>Determines whether the ellipse contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the point is inside or on the ellipse; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p)
    {
        double cosA = System.Math.Cos(-RotationAngle);
        double sinA = System.Math.Sin(-RotationAngle);
        double dx = p.X - Center.X;
        double dy = p.Y - Center.Y;
        double lx = dx * cosA - dy * sinA;
        double ly = dx * sinA + dy * cosA;
        double norm = (lx * lx) / (SemiMajor * SemiMajor) + (ly * ly) / (SemiMinor * SemiMinor);
        return norm <= 1.0 + 1e-10;
    }

    /// <summary>Computes the axis-aligned bounding box of this ellipse.</summary>
    /// <returns>The bounding box enclosing the ellipse.</returns>
    public BoundingBox2D ToBoundingBox()
    {
        double cosA = System.Math.Cos(RotationAngle);
        double sinA = System.Math.Sin(RotationAngle);
        double a = SemiMajor, b = SemiMinor;
        double dx = System.Math.Sqrt(a * a * cosA * cosA + b * b * sinA * sinA);
        double dy = System.Math.Sqrt(a * a * sinA * sinA + b * b * cosA * cosA);
        return new BoundingBox2D(
            new Point2D(Center.X - dx, Center.Y - dy),
            new Point2D(Center.X + dx, Center.Y + dy));
    }

    /// <summary>Indexer for component access by index.</summary>
    /// <param name="index">The component index (0 = Center X, 1 = Center Y, 2 = SemiMajor, 3 = SemiMinor, 4 = RotationAngle).</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => Center.X,
            1 => Center.Y,
            2 => SemiMajor,
            3 => SemiMinor,
            4 => RotationAngle,
            _ => throw new System.IndexOutOfRangeException($"Ellipse2D index {index} out of range [0, 4].")
        };
    }

    /// <summary>Returns a string representation of this ellipse.</summary>
    public override string ToString() => $"Ellipse2D({Center}, a={SemiMajor}, b={SemiMinor}, rot={RotationAngle})";
}
