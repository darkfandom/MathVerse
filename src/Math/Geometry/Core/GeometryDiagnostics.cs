using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

using Circle2D = MathVerse.Math.Geometry.Geometry2D.Circle2D;
using WindingOrderValue = MathVerse.Math.Geometry.Geometry2D.WindingOrder;

namespace MathVerse.Math.Geometry;

/// <summary>
/// Provides static methods for validating geometry primitives and computing diagnostic metrics.
/// </summary>
public static class GeometryDiagnostics
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Determines whether the specified 2D point contains valid finite numerical values.
    /// </summary>
    /// <param name="p">The point to validate.</param>
    /// <returns><c>true</c> if both coordinates are finite and not NaN; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Point2D p)
        => double.IsFinite(p.X) && double.IsFinite(p.Y);

    /// <summary>
    /// Determines whether the specified 3D point contains valid finite numerical values.
    /// </summary>
    /// <param name="p">The point to validate.</param>
    /// <returns><c>true</c> if all coordinates are finite and not NaN; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Point3D p)
        => double.IsFinite(p.X) && double.IsFinite(p.Y) && double.IsFinite(p.Z);

    /// <summary>
    /// Determines whether the specified 2D vector contains valid finite numerical values.
    /// </summary>
    /// <param name="v">The vector to validate.</param>
    /// <returns><c>true</c> if both components are finite and not NaN; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Vector2D v)
        => double.IsFinite(v.X) && double.IsFinite(v.Y);

    /// <summary>
    /// Determines whether the specified 3D vector contains valid finite numerical values.
    /// </summary>
    /// <param name="v">The vector to validate.</param>
    /// <returns><c>true</c> if all components are finite and not NaN; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Vector3D v)
        => double.IsFinite(v.X) && double.IsFinite(v.Y) && double.IsFinite(v.Z);

    /// <summary>
    /// Determines whether the specified 2D triangle is valid (non-degenerate and finite).
    /// </summary>
    /// <param name="t">The triangle to validate.</param>
    /// <returns><c>true</c> if the triangle has positive area and finite vertices; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Triangle2D t)
        => DegeneracyScore(t) > Tolerance && IsValid(t.A) && IsValid(t.B) && IsValid(t.C);

    /// <summary>
    /// Determines whether the specified 3D triangle is valid (non-degenerate and finite).
    /// </summary>
    /// <param name="t">The triangle to validate.</param>
    /// <returns><c>true</c> if the triangle has positive area and finite vertices; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Triangle3D t)
        => DegeneracyScore(t) > Tolerance && IsValid(t.A) && IsValid(t.B) && IsValid(t.C);

    /// <summary>
    /// Determines whether the specified 2D circle is valid (positive radius, finite values).
    /// </summary>
    /// <param name="c">The circle to validate.</param>
    /// <returns><c>true</c> if the circle has a positive radius and finite center; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Circle2D c)
        => c.Radius > Tolerance && double.IsFinite(c.Radius) && IsValid(c.Center);

    /// <summary>
    /// Determines whether the specified 3D sphere is valid (positive radius, finite values).
    /// </summary>
    /// <param name="s">The sphere to validate.</param>
    /// <returns><c>true</c> if the sphere has a positive radius and finite center; otherwise, <c>false</c>.</returns>
    public static bool IsValid(Sphere3D s)
        => s.Radius > Tolerance && double.IsFinite(s.Radius) && IsValid(s.Center);

    /// <summary>
    /// Computes a degeneracy score for a 2D triangle.
    /// Returns 0 for a degenerate (zero-area) triangle and 1 for a perfect equilateral triangle.
    /// </summary>
    /// <param name="t">The triangle to evaluate.</param>
    /// <returns>A value between 0 and 1 indicating how close the triangle is to equilateral.</returns>
    public static double DegeneracyScore(Triangle2D t)
    {
        double ax = t.B.X - t.A.X;
        double ay = t.B.Y - t.A.Y;
        double bx = t.C.X - t.A.X;
        double by = t.C.Y - t.A.Y;
        double area = System.Math.Abs(ax * by - ay * bx) / 2.0;

        double a = System.Math.Sqrt(ax * ax + ay * ay);
        double b = System.Math.Sqrt(bx * bx + by * by);
        double dcx = t.C.X - t.B.X;
        double dcy = t.C.Y - t.B.Y;
        double c = System.Math.Sqrt(dcx * dcx + dcy * dcy);

        double sumSq = a * a + b * b + c * c;
        if (sumSq < Tolerance)
            return 0.0;

        return System.Math.Min(4.0 * System.Math.Sqrt(3.0) * area / sumSq, 1.0);
    }

    /// <summary>
    /// Computes a degeneracy score for a 3D triangle.
    /// Returns 0 for a degenerate (zero-area) triangle and 1 for a perfect equilateral triangle.
    /// </summary>
    /// <param name="t">The triangle to evaluate.</param>
    /// <returns>A value between 0 and 1 indicating how close the triangle is to equilateral.</returns>
    public static double DegeneracyScore(Triangle3D t)
    {
        double ax = t.B.X - t.A.X;
        double ay = t.B.Y - t.A.Y;
        double az = t.B.Z - t.A.Z;
        double bx = t.C.X - t.A.X;
        double by = t.C.Y - t.A.Y;
        double bz = t.C.Z - t.A.Z;

        double crossX = ay * bz - az * by;
        double crossY = az * bx - ax * bz;
        double crossZ = ax * by - ay * bx;
        double area = System.Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ) / 2.0;

        double a = System.Math.Sqrt(ax * ax + ay * ay + az * az);
        double b = System.Math.Sqrt(bx * bx + by * by + bz * bz);
        double dcx = t.C.X - t.B.X;
        double dcy = t.C.Y - t.B.Y;
        double dcz = t.C.Z - t.B.Z;
        double c = System.Math.Sqrt(dcx * dcx + dcy * dcy + dcz * dcz);

        double sumSq = a * a + b * b + c * c;
        if (sumSq < Tolerance)
            return 0.0;

        return System.Math.Min(4.0 * System.Math.Sqrt(3.0) * area / sumSq, 1.0);
    }

    /// <summary>
    /// Determines whether the specified polygon has only convex interior angles.
    /// </summary>
    /// <param name="polygon">The ordered list of polygon vertices.</param>
    /// <returns><c>true</c> if the polygon is convex; otherwise, <c>false</c>.</returns>
    public static bool IsConvex(IReadOnlyList<Point2D> polygon)
    {
        if (polygon.Count < 3)
            return false;

        bool hasPositive = false;
        bool hasNegative = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            Point2D a = polygon[i];
            Point2D b = polygon[(i + 1) % polygon.Count];
            Point2D c = polygon[(i + 2) % polygon.Count];

            double cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);

            if (cross > Tolerance)
                hasPositive = true;
            if (cross < -Tolerance)
                hasNegative = true;

            if (hasPositive && hasNegative)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Computes the winding order of the specified polygon vertices.
    /// </summary>
    /// <param name="polygon">The ordered list of polygon vertices.</param>
    /// <returns><see cref="WindingOrderValue.Clockwise"/> for clockwise or <see cref="WindingOrderValue.CounterClockwise"/> for counter-clockwise.</returns>
    public static WindingOrderValue ComputeWindingOrder(IReadOnlyList<Point2D> polygon)
    {
        double signedArea = 0.0;

        for (int i = 0; i < polygon.Count; i++)
        {
            Point2D current = polygon[i];
            Point2D next = polygon[(i + 1) % polygon.Count];
            signedArea += (next.X - current.X) * (next.Y + current.Y);
        }

        return signedArea > 0 ? WindingOrderValue.Clockwise : WindingOrderValue.CounterClockwise;
    }
}
