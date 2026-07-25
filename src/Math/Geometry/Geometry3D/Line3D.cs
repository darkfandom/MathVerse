namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D line segment defined by two endpoints.</summary>
public readonly record struct Line3D(Point3D P1, Point3D P2)
{
    /// <summary>The first endpoint.</summary>
    public Point3D P1 { get; } = P1;

    /// <summary>The second endpoint.</summary>
    public Point3D P2 { get; } = P2;

    /// <summary>Gets the normalized direction vector from P1 to P2.</summary>
    public Vector3D Direction => new Vector3D(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z).Normalize();

    /// <summary>Gets the length of the line segment.</summary>
    public double Length => P1.DistanceTo(P2);

    /// <summary>Evaluates the line at parameter t.</summary>
    /// <param name="t">The parameter, where 0 returns P1 and 1 returns P2.</param>
    /// <returns>The point at parameter t.</returns>
    public Point3D PointAt(double t) => new(
        P1.X + (P2.X - P1.X) * t,
        P1.Y + (P2.Y - P1.Y) * t,
        P1.Z + (P2.Z - P1.Z) * t);

    /// <summary>Computes the shortest distance from a point to this line segment.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The distance.</returns>
    public double DistanceTo(Point3D p)
    {
        Vector3D ab = new(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
        Vector3D ap = new(p.X - P1.X, p.Y - P1.Y, p.Z - P1.Z);
        double t = ap.Dot(ab) / ab.LengthSquared;
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        Point3D closest = new(P1.X + ab.X * t, P1.Y + ab.Y * t, P1.Z + ab.Z * t);
        return p.DistanceTo(closest);
    }

    /// <summary>Finds the closest point on this line segment to a given point.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the segment.</returns>
    public Point3D ClosestPoint(Point3D p)
    {
        Vector3D ab = new(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
        Vector3D ap = new(p.X - P1.X, p.Y - P1.Y, p.Z - P1.Z);
        double t = ap.Dot(ab) / ab.LengthSquared;
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        return new Point3D(P1.X + ab.X * t, P1.Y + ab.Y * t, P1.Z + ab.Z * t);
    }

    /// <summary>Computes the intersection of this line segment with a plane.</summary>
    /// <param name="plane">The plane.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point3D point) Intersect(Plane3D plane)
    {
        Vector3D dir = new(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
        Vector3D w = new(P1.X - plane.Point.X, P1.Y - plane.Point.Y, P1.Z - plane.Point.Z);
        double denom = plane.Normal.Dot(dir);
        if (System.Math.Abs(denom) < 1e-15)
            return (false, Point3D.Origin);
        double t = -plane.Normal.Dot(w) / denom;
        if (t < 0.0 || t > 1.0)
            return (false, Point3D.Origin);
        return (true, new Point3D(P1.X + dir.X * t, P1.Y + dir.Y * t, P1.Z + dir.Z * t));
    }

    /// <summary>Computes the closest approach between this line segment and another.</summary>
    /// <param name="other">The other line segment.</param>
    /// <returns>A tuple indicating whether the segments are nearly coincident, the closest point on this segment, and the distance between the closest points.</returns>
    public (bool hit, Point3D point, double distance) Intersect(Line3D other)
    {
        Vector3D d1 = new(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
        Vector3D d2 = new(other.P2.X - other.P1.X, other.P2.Y - other.P1.Y, other.P2.Z - other.P1.Z);
        Vector3D r = new(P1.X - other.P1.X, P1.Y - other.P1.Y, P1.Z - other.P1.Z);

        double a = d1.Dot(d1);
        double e = d2.Dot(d2);
        double f = d2.Dot(r);

        if (a < 1e-15 && e < 1e-15)
            return (false, P1, P1.DistanceTo(other.P1));

        double s;
        double t;

        if (a < 1e-15)
        {
            s = 0.0;
            t = System.Math.Max(0.0, System.Math.Min(1.0, f / e));
        }
        else
        {
            double c = d1.Dot(r);
            if (e < 1e-15)
            {
                t = 0.0;
                s = System.Math.Max(0.0, System.Math.Min(1.0, -c / a));
            }
            else
            {
                double b = d1.Dot(d2);
                double denom = a * e - b * b;
                if (System.Math.Abs(denom) < 1e-15)
                {
                    s = 0.0;
                    t = System.Math.Max(0.0, System.Math.Min(1.0, f / e));
                }
                else
                {
                s = System.Math.Max(0.0, System.Math.Min(1.0, (b * f - c * e) / denom));
                t = (a * f - b * c) / denom;
                t = System.Math.Max(0.0, System.Math.Min(1.0, t));
                double tmp = b * t + f;
                if (tmp < 0.0)
                    s = System.Math.Max(0.0, System.Math.Min(1.0, -c / a));
                else if (tmp > e)
                    s = System.Math.Max(0.0, System.Math.Min(1.0, (b - c) / a));
                }
            }
        }

        Point3D cp1 = new(P1.X + d1.X * s, P1.Y + d1.Y * s, P1.Z + d1.Z * s);
        Point3D cp2 = new(other.P1.X + d2.X * t, other.P1.Y + d2.Y * t, other.P1.Z + d2.Z * t);
        double dist = cp1.DistanceTo(cp2);

        return (dist < 1e-10, cp1, dist);
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this line segment.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox() => new(
        new Point3D(
            System.Math.Min(P1.X, P2.X),
            System.Math.Min(P1.Y, P2.Y),
            System.Math.Min(P1.Z, P2.Z)),
        new Point3D(
            System.Math.Max(P1.X, P2.X),
            System.Math.Max(P1.Y, P2.Y),
            System.Math.Max(P1.Z, P2.Z)));

    /// <inheritdoc/>
    public override string ToString() => $"Line3D(P1={P1}, P2={P2})";
}
