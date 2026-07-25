using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a triangle defined by three vertices.</summary>
public readonly record struct Triangle2D(Point2D A, Point2D B, Point2D C)
{
    /// <summary>The first vertex.</summary>
    public Point2D A { get; } = A;

    /// <summary>The second vertex.</summary>
    public Point2D B { get; } = B;

    /// <summary>The third vertex.</summary>
    public Point2D C { get; } = C;

    /// <summary>Gets the area of the triangle using the cross product.</summary>
    public double Area => System.Math.Abs((B.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (B.Y - A.Y)) * 0.5;

    /// <summary>Gets the perimeter of the triangle.</summary>
    public double Perimeter => A.DistanceTo(B) + B.DistanceTo(C) + C.DistanceTo(A);

    /// <summary>Gets the centroid of the triangle.</summary>
    public Point2D Centroid => new((A.X + B.X + C.X) / 3.0, (A.Y + B.Y + C.Y) / 3.0);

    /// <summary>Gets the incenter of the triangle.</summary>
    public Point2D Incenter
    {
        get
        {
            double a = B.DistanceTo(C);
            double b = A.DistanceTo(C);
            double c = A.DistanceTo(B);
            double p = a + b + c;
            return new Point2D((a * A.X + b * B.X + c * C.X) / p, (a * A.Y + b * B.Y + c * C.Y) / p);
        }
    }

    /// <summary>Gets the circumcenter of the triangle.</summary>
    public Point2D Circumcenter
    {
        get
        {
            double ax = A.X, ay = A.Y;
            double bx = B.X, by = B.Y;
            double cx = C.X, cy = C.Y;
            double d = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (System.Math.Abs(d) < 1e-15) return Centroid;
            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
            return new Point2D(ux, uy);
        }
    }

    /// <summary>Gets the circumradius of the triangle.</summary>
    public double Circumradius => A.DistanceTo(Circumcenter);

    /// <summary>Gets the inradius of the triangle.</summary>
    public double Inradius
    {
        get
        {
            double area = Area;
            double s = Perimeter * 0.5;
            return s > 1e-15 ? area / s : 0;
        }
    }

    /// <summary>Computes the barycentric coordinates of a point with respect to this triangle.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The barycentric coordinates (u, v, w) where p = u*A + v*B + w*C.</returns>
    public (double u, double v, double w) BarycentricCoords(Point2D p)
    {
        double v0x = B.X - A.X, v0y = B.Y - A.Y;
        double v1x = C.X - A.X, v1y = C.Y - A.Y;
        double v2x = p.X - A.X, v2y = p.Y - A.Y;
        double d00 = v0x * v0x + v0y * v0y;
        double d01 = v0x * v1x + v0y * v1y;
        double d11 = v1x * v1x + v1y * v1y;
        double d20 = v2x * v0x + v2y * v0y;
        double d21 = v2x * v1x + v2y * v1y;
        double denom = d00 * d11 - d01 * d01;
        if (System.Math.Abs(denom) < 1e-15) return (1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0);
        double v = (d11 * d20 - d01 * d21) / denom;
        double w = (d00 * d21 - d01 * d20) / denom;
        double u = 1.0 - v - w;
        return (u, v, w);
    }

    /// <summary>Determines whether the triangle contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the triangle contains the point; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p)
    {
        var (u, v, w) = BarycentricCoords(p);
        return u >= -1e-10 && v >= -1e-10 && w >= -1e-10;
    }

    /// <summary>Computes the axis-aligned bounding box of this triangle.</summary>
    /// <returns>The bounding box enclosing the triangle.</returns>
    public BoundingBox2D ToBoundingBox() => BoundingBox2D.FromPoints(new[] { A, B, C });

    /// <summary>Determines whether the triangle is degenerate (near-zero area).</summary>
    /// <param name="tol">The tolerance for zero area.</param>
    /// <returns><c>true</c> if the triangle is degenerate; otherwise, <c>false</c>.</returns>
    public bool IsDegenerate(double tol = 1e-10) => Area < tol;

    /// <summary>Indexer for vertex access by index (0 = A, 1 = B, 2 = C).</summary>
    /// <param name="index">The vertex index.</param>
    /// <returns>The vertex.</returns>
    public Point2D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => A,
            1 => B,
            2 => C,
            _ => throw new System.IndexOutOfRangeException($"Triangle2D index {index} out of range [0, 2].")
        };
    }

    /// <summary>Returns a string representation of this triangle.</summary>
    public override string ToString() => $"Triangle2D({A}, {B}, {C})";
}
