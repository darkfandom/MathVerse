using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a finite line segment defined by two endpoints.</summary>
public readonly record struct Segment2D(Point2D P1, Point2D P2)
{
    /// <summary>The first endpoint.</summary>
    public Point2D P1 { get; } = P1;

    /// <summary>The second endpoint.</summary>
    public Point2D P2 { get; } = P2;

    /// <summary>Gets the length of the segment.</summary>
    public double Length => P1.DistanceTo(P2);

    /// <summary>Gets the midpoint of the segment.</summary>
    public Point2D Midpoint => new((P1.X + P2.X) * 0.5, (P1.Y + P2.Y) * 0.5);

    /// <summary>Gets the normalized direction vector from P1 to P2.</summary>
    public Vector2D Direction => new Vector2D(P2.X - P1.X, P2.Y - P1.Y).Normalize();

    /// <summary>Returns a point along the segment at parameter t.</summary>
    /// <param name="t">The parameter (0 = P1, 1 = P2).</param>
    /// <returns>The point at parameter t.</returns>
    public Point2D PointAt(double t) => new(P1.X + (P2.X - P1.X) * t, P1.Y + (P2.Y - P1.Y) * t);

    /// <summary>Computes the intersection of this segment with another segment.</summary>
    /// <param name="other">The other segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point2D point) Intersect(Segment2D other)
    {
        double d1x = P2.X - P1.X, d1y = P2.Y - P1.Y;
        double d2x = other.P2.X - other.P1.X, d2y = other.P2.Y - other.P1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < 1e-15) return (false, Point2D.Origin);

        double t = ((other.P1.X - P1.X) * d2y - (other.P1.Y - P1.Y) * d2x) / cross;
        double u = ((other.P1.X - P1.X) * d1y - (other.P1.Y - P1.Y) * d1x) / cross;

        if (t >= -1e-10 && t <= 1.0 + 1e-10 && u >= -1e-10 && u <= 1.0 + 1e-10)
            return (true, new Point2D(P1.X + t * d1x, P1.Y + t * d1y));

        return (false, Point2D.Origin);
    }

    /// <summary>Computes the distance from the segment to a point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The minimum distance from the segment to the point.</returns>
    public double DistanceTo(Point2D p) => ClosestPoint(p).DistanceTo(p);

    /// <summary>Finds the closest point on the segment to a given point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The closest point on the segment.</returns>
    public Point2D ClosestPoint(Point2D p)
    {
        Vector2D d = new(P2.X - P1.X, P2.Y - P1.Y);
        double lenSq = d.LengthSquared;
        if (lenSq < 1e-30) return P1;
        double t = System.Math.Clamp(new Vector2D(p.X - P1.X, p.Y - P1.Y).Dot(d) / lenSq, 0.0, 1.0);
        return PointAt(t);
    }

    /// <summary>Computes the intersection of this segment with an infinite line.</summary>
    /// <param name="line">The line to intersect with.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point2D point) IntersectLine(Line2D line)
    {
        double d1x = P2.X - P1.X, d1y = P2.Y - P1.Y;
        double d2x = line.P2.X - line.P1.X, d2y = line.P2.Y - line.P1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < 1e-15) return (false, Point2D.Origin);

        double t = ((line.P1.X - P1.X) * d2y - (line.P1.Y - P1.Y) * d2x) / cross;
        if (t >= -1e-10 && t <= 1.0 + 1e-10)
            return (true, new Point2D(P1.X + t * d1x, P1.Y + t * d1y));

        return (false, Point2D.Origin);
    }

    /// <summary>Computes the axis-aligned bounding box of this segment.</summary>
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
            _ => throw new System.IndexOutOfRangeException($"Segment2D index {index} out of range [0, 1].")
        };
    }

    /// <summary>Returns a string representation of this segment.</summary>
    public override string ToString() => $"Segment2D({P1}, {P2})";
}
