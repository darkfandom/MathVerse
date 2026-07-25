using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents an infinite line defined by two points.</summary>
public readonly record struct Line2D(Point2D P1, Point2D P2)
{
    /// <summary>The first point on the line.</summary>
    public Point2D P1 { get; } = P1;

    /// <summary>The second point on the line.</summary>
    public Point2D P2 { get; } = P2;

    /// <summary>Gets the normalized direction vector of the line.</summary>
    public Vector2D Direction => new Vector2D(P2.X - P1.X, P2.Y - P1.Y).Normalize();

    /// <summary>Gets the length between the two defining points.</summary>
    public double Length => P1.DistanceTo(P2);

    /// <summary>Returns a point along the line at parameter t.</summary>
    /// <param name="t">The parameter (0 = P1, 1 = P2).</param>
    /// <returns>The point at parameter t.</returns>
    public Point2D PointAt(double t) => new(P1.X + (P2.X - P1.X) * t, P1.Y + (P2.Y - P1.Y) * t);

    /// <summary>Computes the distance from the line to a point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The perpendicular distance from the line to the point.</returns>
    public double DistanceTo(Point2D p)
    {
        Vector2D d = Direction;
        Vector2D v = new(p.X - P1.X, p.Y - P1.Y);
        return System.Math.Abs(d.Cross(v));
    }

    /// <summary>Finds the closest point on the line to a given point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The closest point on the line.</returns>
    public Point2D ClosestPoint(Point2D p)
    {
        Vector2D d = new(P2.X - P1.X, P2.Y - P1.Y);
        double lenSq = d.LengthSquared;
        if (lenSq < 1e-30) return P1;
        double t = new Vector2D(p.X - P1.X, p.Y - P1.Y).Dot(d) / lenSq;
        return PointAt(t);
    }

    /// <summary>Computes the intersection of this line with another line.</summary>
    /// <param name="other">The other line.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point2D point) Intersect(Line2D other)
    {
        double d1x = P2.X - P1.X, d1y = P2.Y - P1.Y;
        double d2x = other.P2.X - other.P1.X, d2y = other.P2.Y - other.P1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < 1e-15) return (false, Point2D.Origin);
        double t = ((other.P1.X - P1.X) * d2y - (other.P1.Y - P1.Y) * d2x) / cross;
        return (true, new Point2D(P1.X + t * d1x, P1.Y + t * d1y));
    }

    /// <summary>Determines whether the line contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the line contains the point; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p)
    {
        Vector2D d1 = new(P2.X - P1.X, P2.Y - P1.Y);
        Vector2D d2 = new(p.X - P1.X, p.Y - P1.Y);
        return System.Math.Abs(d1.Cross(d2)) < 1e-10;
    }

    /// <summary>Computes the axis-aligned bounding box of this line.</summary>
    /// <returns>The bounding box enclosing both endpoints.</returns>
    public BoundingBox2D ToBoundingBox() => BoundingBox2D.FromPoints(new[] { P1, P2 });

    /// <summary>Indexer for endpoint access by index (0 = P1, 1 = P2).</summary>
    /// <param name="index">The endpoint index.</param>
    /// <returns>The endpoint.</returns>
    public Point2D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => P1,
            1 => P2,
            _ => throw new System.IndexOutOfRangeException($"Line2D index {index} out of range [0, 1].")
        };
    }

    /// <summary>Returns a string representation of this line.</summary>
    public override string ToString() => $"Line2D({P1}, {P2})";
}
