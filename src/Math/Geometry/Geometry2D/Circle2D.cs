using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a circle defined by a center and a radius.</summary>
public readonly record struct Circle2D(Point2D Center, double Radius)
{
    /// <summary>The center of the circle.</summary>
    public Point2D Center { get; } = Center;

    /// <summary>The radius of the circle.</summary>
    public double Radius { get; } = Radius;

    /// <summary>Gets the circumference of the circle.</summary>
    public double Circumference => 2.0 * System.Math.PI * Radius;

    /// <summary>Gets the area of the circle.</summary>
    public double Area => System.Math.PI * Radius * Radius;

    /// <summary>Returns a point on the circle at the given angle.</summary>
    /// <param name="angle">The angle in radians.</param>
    /// <returns>The point on the circle at the given angle.</returns>
    public Point2D PointAt(double angle) => new(Center.X + Radius * System.Math.Cos(angle), Center.Y + Radius * System.Math.Sin(angle));

    /// <summary>Determines whether the circle contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the point is inside or on the circle; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p) => Center.DistanceSquaredTo(p) <= Radius * Radius + 1e-10;

    /// <summary>Computes the distance from the circle boundary to a point.</summary>
    /// <param name="p">The point.</param>
    /// <returns>The distance from the circle boundary to the point (negative if inside).</returns>
    public double DistanceTo(Point2D p) => Center.DistanceTo(p) - Radius;

    /// <summary>Computes the intersection of this circle with another circle.</summary>
    /// <param name="other">The other circle.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public (bool hit, ImmutableArray<Point2D> points) Intersect(Circle2D other)
    {
        double dx = other.Center.X - Center.X;
        double dy = other.Center.Y - Center.Y;
        double d = System.Math.Sqrt(dx * dx + dy * dy);

        if (d > Radius + other.Radius + 1e-10 || d < System.Math.Abs(Radius - other.Radius) - 1e-10)
            return (false, ImmutableArray<Point2D>.Empty);

        if (d < 1e-10 && System.Math.Abs(Radius - other.Radius) < 1e-10)
            return (false, ImmutableArray<Point2D>.Empty);

        double a = (Radius * Radius - other.Radius * other.Radius + d * d) / (2.0 * d);
        double h = System.Math.Sqrt(System.Math.Max(0, Radius * Radius - a * a));

        double px = Center.X + a * dx / d;
        double py = Center.Y + a * dy / d;

        if (h < 1e-10)
            return (true, ImmutableArray.Create(new Point2D(px, py)));

        return (true, ImmutableArray.Create(
            new Point2D(px + h * dy / d, py - h * dx / d),
            new Point2D(px - h * dy / d, py + h * dx / d)));
    }

    /// <summary>Computes the intersection of this circle with a line.</summary>
    /// <param name="line">The line to intersect with.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public (bool hit, ImmutableArray<Point2D> points) Intersect(Line2D line)
    {
        Vector2D d = new(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y);
        Vector2D f = new(line.P1.X - Center.X, line.P1.Y - Center.Y);

        double a = d.Dot(d);
        double b = 2.0 * f.Dot(d);
        double c = f.Dot(f) - Radius * Radius;
        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -1e-10) return (false, ImmutableArray<Point2D>.Empty);

        if (discriminant < 1e-10)
        {
            double t = -b / (2.0 * a);
            return (true, ImmutableArray.Create(new Point2D(line.P1.X + t * d.X, line.P1.Y + t * d.Y)));
        }

        double sqrtD = System.Math.Sqrt(discriminant);
        double t1 = (-b - sqrtD) / (2.0 * a);
        double t2 = (-b + sqrtD) / (2.0 * a);

        return (true, ImmutableArray.Create(
            new Point2D(line.P1.X + t1 * d.X, line.P1.Y + t1 * d.Y),
            new Point2D(line.P1.X + t2 * d.X, line.P1.Y + t2 * d.Y)));
    }

    /// <summary>Returns the tangent vector at the given angle.</summary>
    /// <param name="angle">The angle in radians.</param>
    /// <returns>The tangent vector (normalized).</returns>
    public Vector2D TangentAt(double angle) => new(-System.Math.Sin(angle), System.Math.Cos(angle));

    /// <summary>Returns the outward normal vector at the given angle.</summary>
    /// <param name="angle">The angle in radians.</param>
    /// <returns>The normal vector (normalized).</returns>
    public Vector2D NormalAt(double angle) => new(System.Math.Cos(angle), System.Math.Sin(angle));

    /// <summary>Computes the axis-aligned bounding box of this circle.</summary>
    /// <returns>The bounding box enclosing the circle.</returns>
    public BoundingBox2D ToBoundingBox() => new(
        new Point2D(Center.X - Radius, Center.Y - Radius),
        new Point2D(Center.X + Radius, Center.Y + Radius));

    /// <summary>Indexer for component access by index (0 = Center X, 1 = Center Y, 2 = Radius).</summary>
    /// <param name="index">The component index.</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => Center.X,
            1 => Center.Y,
            2 => Radius,
            _ => throw new System.IndexOutOfRangeException($"Circle2D index {index} out of range [0, 2].")
        };
    }

    /// <summary>Returns a string representation of this circle.</summary>
    public override string ToString() => $"Circle2D({Center}, r={Radius})";
}
