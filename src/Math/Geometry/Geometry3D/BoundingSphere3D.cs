namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a bounding sphere defined by a center and radius.</summary>
public readonly record struct BoundingSphere3D(Point3D Center, double Radius)
{
    /// <summary>The center of the bounding sphere.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The radius of the bounding sphere.</summary>
    public double Radius { get; } = Radius;

    /// <summary>Tests whether a point is inside or on the bounding sphere.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is contained within the sphere.</returns>
    public bool Contains(Point3D p) =>
        Center.DistanceSquaredTo(p) <= Radius * Radius;

    /// <summary>Tests whether another bounding sphere is entirely inside this sphere.</summary>
    /// <param name="other">The other bounding sphere.</param>
    /// <returns>True if the other sphere is entirely contained.</returns>
    public bool Contains(BoundingSphere3D other)
    {
        double dist = Center.DistanceTo(other.Center);
        return dist + other.Radius <= Radius;
    }

    /// <summary>Tests whether this sphere intersects another sphere.</summary>
    /// <param name="other">The other bounding sphere.</param>
    /// <returns>True if the spheres overlap.</returns>
    public bool Intersects(BoundingSphere3D other)
    {
        double dist = Center.DistanceTo(other.Center);
        return dist <= Radius + other.Radius;
    }

    /// <summary>Computes the minimum bounding sphere enclosing both this sphere and another.</summary>
    /// <param name="other">The other bounding sphere.</param>
    /// <returns>The enclosing bounding sphere.</returns>
    public BoundingSphere3D Union(BoundingSphere3D other)
    {
        Vector3D d = new(other.Center.X - Center.X, other.Center.Y - Center.Y, other.Center.Z - Center.Z);
        double dist = d.Length;

        if (dist + other.Radius <= Radius)
            return this;

        if (dist + Radius <= other.Radius)
            return other;

        double newRadius = (dist + Radius + other.Radius) * 0.5;
        double t = (newRadius - Radius) / dist;
        return new BoundingSphere3D(
            new Point3D(
                Center.X + d.X * t,
                Center.Y + d.Y * t,
                Center.Z + d.Z * t),
            newRadius);
    }

    /// <summary>Computes a bounding sphere enclosing all given points using Ritter's algorithm.</summary>
    /// <param name="points">The points to enclose.</param>
    /// <returns>The bounding sphere, or a zero-radius sphere at the origin if the collection is empty.</returns>
    public static BoundingSphere3D FromPoints(IEnumerable<Point3D> points)
    {
        using IEnumerator<Point3D> enumerator = points.GetEnumerator();
        if (!enumerator.MoveNext())
            return new BoundingSphere3D(Point3D.Origin, 0.0);

        Point3D center = enumerator.Current;
        double radius = 0.0;

        while (enumerator.MoveNext())
        {
            Point3D p = enumerator.Current;
            double dx = p.X - center.X;
            double dy = p.Y - center.Y;
            double dz = p.Z - center.Z;
            double dist = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (dist > radius)
            {
                double newRadius = (radius + dist) * 0.5;
                double scale = (newRadius - radius) / dist;
                center = new Point3D(
                    center.X + dx * scale,
                    center.Y + dy * scale,
                    center.Z + dz * scale);
                radius = newRadius;
            }
        }

        return new BoundingSphere3D(center, radius);
    }

    /// <inheritdoc/>
    public override string ToString() => $"BoundingSphere3D(Center={Center}, Radius={Radius})";
}
