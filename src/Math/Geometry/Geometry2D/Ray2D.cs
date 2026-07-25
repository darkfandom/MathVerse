using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a ray defined by an origin and a direction.</summary>
public readonly record struct Ray2D(Point2D Origin, Vector2D Direction)
{
    /// <summary>The origin of the ray.</summary>
    public Point2D Origin { get; } = Origin;

    /// <summary>The direction of the ray (should be normalized).</summary>
    public Vector2D Direction { get; } = Direction;

    /// <summary>Returns a point along the ray at parameter t.</summary>
    /// <param name="t">The parameter (t >= 0 for points on the ray).</param>
    /// <returns>The point at parameter t.</returns>
    public Point2D PointAt(double t) => new(Origin.X + Direction.X * t, Origin.Y + Direction.Y * t);

    /// <summary>Computes the intersection of this ray with a line.</summary>
    /// <param name="line">The line to intersect with.</param>
    /// <returns>A tuple indicating whether a hit occurred and the parameter t along the ray.</returns>
    public (bool hit, double t) Intersect(Line2D line)
    {
        double d1x = Direction.X, d1y = Direction.Y;
        double d2x = line.P2.X - line.P1.X, d2y = line.P2.Y - line.P1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < 1e-15) return (false, 0);
        double t = ((line.P1.X - Origin.X) * d2y - (line.P1.Y - Origin.Y) * d2x) / cross;
        return (t >= 0, t);
    }

    /// <summary>Computes the intersection of this ray with a circle.</summary>
    /// <param name="circle">The circle to intersect with.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public (bool hit, ImmutableArray<Point2D> points) Intersect(Circle2D circle)
    {
        Vector2D oc = new(Origin.X - circle.Center.X, Origin.Y - circle.Center.Y);
        double a = Direction.Dot(Direction);
        double b = 2.0 * oc.Dot(Direction);
        double c = oc.Dot(oc) - circle.Radius * circle.Radius;
        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -1e-15) return (false, ImmutableArray<Point2D>.Empty);

        var builder = ImmutableArray.CreateBuilder<Point2D>();

        if (discriminant < 1e-15)
        {
            double t = -b / (2.0 * a);
            if (t >= 0) builder.Add(PointAt(t));
        }
        else
        {
            double sqrtD = System.Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2.0 * a);
            double t2 = (-b + sqrtD) / (2.0 * a);
            if (t1 >= 0) builder.Add(PointAt(t1));
            if (t2 >= 0) builder.Add(PointAt(t2));
        }

        return (builder.Count > 0, builder.ToImmutable());
    }

    /// <summary>Computes the distance from the ray to a point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The minimum distance from the ray to the point.</returns>
    public double DistanceTo(Point2D p)
    {
        Vector2D v = new(p.X - Origin.X, p.Y - Origin.Y);
        double t = v.Dot(Direction);
        if (t < 0) return Origin.DistanceTo(p);
        Point2D closest = PointAt(t);
        return closest.DistanceTo(p);
    }

    /// <summary>Indexer for component access by index (0 = Origin X, 1 = Origin Y, 2 = Direction X, 3 = Direction Y).</summary>
    /// <param name="index">The component index.</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => Origin.X,
            1 => Origin.Y,
            2 => Direction.X,
            3 => Direction.Y,
            _ => throw new System.IndexOutOfRangeException($"Ray2D index {index} out of range [0, 3].")
        };
    }

    /// <summary>Returns a string representation of this ray.</summary>
    public override string ToString() => $"Ray2D({Origin}, {Direction})";
}
