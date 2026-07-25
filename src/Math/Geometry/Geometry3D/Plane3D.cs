using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D plane defined by a point and a unit normal vector.</summary>
public readonly record struct Plane3D(Point3D Point, Vector3D Normal)
{
    /// <summary>A point on the plane.</summary>
    public Point3D Point { get; } = Point;

    /// <summary>The unit normal vector of the plane.</summary>
    public Vector3D Normal { get; } = Normal;

    /// <summary>Computes the signed distance from a point to this plane.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The signed distance (positive on the normal side).</returns>
    public double SignedDistanceTo(Point3D p) =>
        Normal.Dot(new Vector3D(p.X - Point.X, p.Y - Point.Y, p.Z - Point.Z));

    /// <summary>Computes the absolute distance from a point to this plane.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The distance.</returns>
    public double DistanceTo(Point3D p) => System.Math.Abs(SignedDistanceTo(p));

    /// <summary>Tests whether a point lies on this plane within tolerance.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is on the plane.</returns>
    public bool Contains(Point3D p) => System.Math.Abs(SignedDistanceTo(p)) < 1e-10;

    /// <summary>Projects a point onto this plane.</summary>
    /// <param name="p">The point to project.</param>
    /// <returns>The projected point.</returns>
    public Point3D Project(Point3D p)
    {
        double d = SignedDistanceTo(p);
        return new Point3D(p.X - Normal.X * d, p.Y - Normal.Y * d, p.Z - Normal.Z * d);
    }

    /// <summary>Computes the intersection of this plane with a line segment.</summary>
    /// <param name="line">The line segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point3D point) Intersect(Line3D line)
    {
        Vector3D dir = new(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y, line.P2.Z - line.P1.Z);
        Vector3D w = new(line.P1.X - Point.X, line.P1.Y - Point.Y, line.P1.Z - Point.Z);
        double denom = Normal.Dot(dir);
        if (System.Math.Abs(denom) < 1e-15)
            return (false, Point3D.Origin);
        double t = -Normal.Dot(w) / denom;
        if (t < 0.0 || t > 1.0)
            return (false, Point3D.Origin);
        return (true, new Point3D(line.P1.X + dir.X * t, line.P1.Y + dir.Y * t, line.P1.Z + dir.Z * t));
    }

    /// <summary>Computes the intersection line of this plane with another plane.</summary>
    /// <param name="other">The other plane.</param>
    /// <returns>A tuple indicating whether the planes intersect and the resulting line of intersection.</returns>
    public (bool hit, Line3D line) Intersect(Plane3D other)
    {
        Vector3D dir = Normal.Cross(other.Normal);
        double dirLenSq = dir.LengthSquared;

        if (dirLenSq < 1e-30)
            return (false, default);

        double d1 = Normal.Dot(new Vector3D(Point.X, Point.Y, Point.Z));
        double d2 = other.Normal.Dot(new Vector3D(other.Point.X, other.Point.Y, other.Point.Z));

        Vector3D n2CrossDir = other.Normal.Cross(dir);
        Vector3D dirCrossN1 = dir.Cross(Normal);

        Point3D pointOnLine = new(
            (d1 * n2CrossDir.X + d2 * dirCrossN1.X) / dirLenSq,
            (d1 * n2CrossDir.Y + d2 * dirCrossN1.Y) / dirLenSq,
            (d1 * n2CrossDir.Z + d2 * dirCrossN1.Z) / dirLenSq);

        Point3D secondPoint = new(pointOnLine.X + dir.X, pointOnLine.Y + dir.Y, pointOnLine.Z + dir.Z);

        return (true, new Line3D(pointOnLine, secondPoint));
    }

    /// <summary>Transforms this plane by an affine transform.</summary>
    /// <param name="t">The transform to apply.</param>
    /// <returns>The transformed plane.</returns>
    public Plane3D Transform(Transform3D t)
    {
        Point3D newPoint = t.TransformPoint(Point);
        Transform3D invTrans = t.InverseTranspose3x3();
        Vector3D newNormal = invTrans.TransformVector(Normal).Normalize();
        return new Plane3D(newPoint, newNormal);
    }

    /// <summary>Creates a plane from a triangle.</summary>
    /// <param name="tri">The triangle.</param>
    /// <returns>The plane containing the triangle.</returns>
    public static Plane3D FromTriangle(Triangle3D tri)
    {
        Vector3D ab = new(tri.B.X - tri.A.X, tri.B.Y - tri.A.Y, tri.B.Z - tri.A.Z);
        Vector3D ac = new(tri.C.X - tri.A.X, tri.C.Y - tri.A.Y, tri.C.Z - tri.A.Z);
        Vector3D normal = ab.Cross(ac).Normalize();
        return new Plane3D(tri.A, normal);
    }

    /// <summary>Creates a plane from three non-collinear points.</summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns>The plane passing through all three points.</returns>
    public static Plane3D FromPoints(Point3D a, Point3D b, Point3D c)
    {
        Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
        Vector3D normal = ab.Cross(ac).Normalize();
        return new Plane3D(a, normal);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Plane3D(Point={Point}, Normal={Normal})";
}
