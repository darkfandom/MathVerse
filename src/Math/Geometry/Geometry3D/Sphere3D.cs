using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D sphere defined by a center and radius.</summary>
public readonly record struct Sphere3D(Point3D Center, double Radius)
{
    /// <summary>The center of the sphere.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The radius of the sphere.</summary>
    public double Radius { get; } = Radius;

    /// <summary>Gets the volume of the sphere.</summary>
    public double Volume => (4.0 / 3.0) * System.Math.PI * Radius * Radius * Radius;

    /// <summary>Gets the surface area of the sphere.</summary>
    public double SurfaceArea => 4.0 * System.Math.PI * Radius * Radius;

    /// <summary>Tests whether a point is inside or on the sphere.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is contained within the sphere.</returns>
    public bool Contains(Point3D p) =>
        Center.DistanceSquaredTo(p) <= Radius * Radius;

    /// <summary>Tests whether a bounding box is entirely inside or on the sphere.</summary>
    /// <param name="box">The bounding box.</param>
    /// <returns>True if the box is entirely contained within the sphere.</returns>
    public bool Contains(BoundingBox3D box)
    {
        ImmutableArray<Point3D> corners = box.Corners;
        for (int i = 0; i < corners.Length; i++)
        {
            if (!Contains(corners[i]))
                return false;
        }
        return true;
    }

    /// <summary>Computes the shortest distance from a point to the sphere surface.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The distance (zero if the point is inside the sphere).</returns>
    public double DistanceTo(Point3D p)
    {
        double dist = Center.DistanceTo(p);
        return System.Math.Max(0.0, dist - Radius);
    }

    /// <summary>Computes the intersection of this sphere with a line segment.</summary>
    /// <param name="line">The line segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points (0, 1, or 2).</returns>
    public (bool hit, ImmutableArray<Point3D> points) Intersect(Line3D line)
    {
        Vector3D d = new(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y, line.P2.Z - line.P1.Z);
        Vector3D f = new(line.P1.X - Center.X, line.P1.Y - Center.Y, line.P1.Z - Center.Z);

        double a = d.Dot(d);
        double b = 2.0 * f.Dot(d);
        double c = f.Dot(f) - Radius * Radius;

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < 0.0)
            return (false, ImmutableArray<Point3D>.Empty);

        double sqrtDisc = System.Math.Sqrt(discriminant);
        double t1 = (-b - sqrtDisc) / (2.0 * a);
        double t2 = (-b + sqrtDisc) / (2.0 * a);

        bool t1Valid = t1 >= 0.0 && t1 <= 1.0;
        bool t2Valid = t2 >= 0.0 && t2 <= 1.0;

        if (!t1Valid && !t2Valid)
            return (false, ImmutableArray<Point3D>.Empty);

        ImmutableArray<Point3D>.Builder builder = ImmutableArray.CreateBuilder<Point3D>(2);

        if (t1Valid)
            builder.Add(new Point3D(line.P1.X + d.X * t1, line.P1.Y + d.Y * t1, line.P1.Z + d.Z * t1));

        if (t2Valid && System.Math.Abs(t2 - t1) > 1e-10)
            builder.Add(new Point3D(line.P1.X + d.X * t2, line.P1.Y + d.Y * t2, line.P1.Z + d.Z * t2));

        return (builder.Count > 0, builder.ToImmutable());
    }

    /// <summary>Computes the intersection circle of this sphere with a plane.</summary>
    /// <param name="plane">The plane.</param>
    /// <returns>A tuple indicating whether an intersection exists and the resulting circle on the plane.</returns>
    public (bool hit, CircleOnPlane circle) Intersect(Plane3D plane)
    {
        double dist = plane.SignedDistanceTo(Center);
        double absDist = System.Math.Abs(dist);

        if (absDist >= Radius)
            return (false, default);

        Point3D circleCenter = plane.Project(Center);
        double circleRadius = System.Math.Sqrt(Radius * Radius - absDist * absDist);

        return (true, new CircleOnPlane(circleCenter, plane.Normal, circleRadius));
    }

    /// <summary>Finds the closest point on the sphere surface to a given point.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the surface.</returns>
    public Point3D ClosestPointOnSurface(Point3D p)
    {
        Vector3D dir = new(p.X - Center.X, p.Y - Center.Y, p.Z - Center.Z);
        double len = dir.Length;
        if (len < 1e-15)
            return new Point3D(Center.X + Radius, Center.Y, Center.Z);
        double invLen = 1.0 / len;
        return new Point3D(
            Center.X + dir.X * invLen * Radius,
            Center.Y + dir.Y * invLen * Radius,
            Center.Z + dir.Z * invLen * Radius);
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this sphere.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox() => new(
        new Point3D(Center.X - Radius, Center.Y - Radius, Center.Z - Radius),
        new Point3D(Center.X + Radius, Center.Y + Radius, Center.Z + Radius));

    /// <inheritdoc/>
    public override string ToString() => $"Sphere3D(Center={Center}, Radius={Radius})";
}
