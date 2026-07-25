namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a capsule shape defined by two sphere centers and a shared radius.</summary>
public readonly record struct Capsule3D(Point3D A, Point3D B, double Radius)
{
    /// <summary>The first sphere center.</summary>
    public Point3D A { get; } = A;

    /// <summary>The second sphere center.</summary>
    public Point3D B { get; } = B;

    /// <summary>The radius of the capsule.</summary>
    public double Radius { get; } = Radius;

    /// <summary>Gets the distance between the two sphere centers.</summary>
    public double Length => A.DistanceTo(B);

    /// <summary>Gets the volume of the capsule.</summary>
    public double Volume
    {
        get
        {
            double d = Length;
            double cylLen = System.Math.Max(0.0, d - 2.0 * Radius);
            return System.Math.PI * Radius * Radius * cylLen + (4.0 / 3.0) * System.Math.PI * Radius * Radius * Radius;
        }
    }

    /// <summary>Gets the total surface area (lateral cylinder + two hemispheres).</summary>
    public double SurfaceArea
    {
        get
        {
            double d = Length;
            double cylLen = System.Math.Max(0.0, d - 2.0 * Radius);
            return 2.0 * System.Math.PI * Radius * cylLen + 4.0 * System.Math.PI * Radius * Radius;
        }
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this capsule.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox() => new(
        new Point3D(
            System.Math.Min(A.X, B.X) - Radius,
            System.Math.Min(A.Y, B.Y) - Radius,
            System.Math.Min(A.Z, B.Z) - Radius),
        new Point3D(
            System.Math.Max(A.X, B.X) + Radius,
            System.Math.Max(A.Y, B.Y) + Radius,
            System.Math.Max(A.Z, B.Z) + Radius));

    /// <summary>Tests whether a point is inside or on the capsule.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is contained within the capsule.</returns>
    public bool Contains(Point3D p)
    {
        Point3D closest = ClosestPointOnAxis(p);
        double dx = p.X - closest.X;
        double dy = p.Y - closest.Y;
        double dz = p.Z - closest.Z;
        double distSq = dx * dx + dy * dy + dz * dz;
        return distSq <= Radius * Radius;
    }

    /// <summary>Finds the closest point on the capsule surface to a given point.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the surface.</returns>
    public Point3D ClosestPoint(Point3D p)
    {
        Point3D closestOnAxis = ClosestPointOnAxis(p);
        Vector3D dir = new(p.X - closestOnAxis.X, p.Y - closestOnAxis.Y, p.Z - closestOnAxis.Z);
        double len = dir.Length;

        if (len < 1e-15)
        {
            Vector3D ab = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3D perp;
            if (System.Math.Abs(ab.X) <= System.Math.Abs(ab.Y) && System.Math.Abs(ab.X) <= System.Math.Abs(ab.Z))
                perp = ab.Cross(Vector3D.UnitX).Normalize();
            else if (System.Math.Abs(ab.Y) <= System.Math.Abs(ab.Z))
                perp = ab.Cross(Vector3D.UnitY).Normalize();
            else
                perp = ab.Cross(Vector3D.UnitZ).Normalize();

            return new Point3D(
                closestOnAxis.X + perp.X * Radius,
                closestOnAxis.Y + perp.Y * Radius,
                closestOnAxis.Z + perp.Z * Radius);
        }

        double invLen = 1.0 / len;
        return new Point3D(
            closestOnAxis.X + dir.X * invLen * Radius,
            closestOnAxis.Y + dir.Y * invLen * Radius,
            closestOnAxis.Z + dir.Z * invLen * Radius);
    }

    /// <summary>Finds the closest point on the central axis segment to a given point.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the axis segment.</returns>
    private Point3D ClosestPointOnAxis(Point3D p)
    {
        Vector3D ab = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
        Vector3D ap = new(p.X - A.X, p.Y - A.Y, p.Z - A.Z);
        double t = ap.Dot(ab) / ab.LengthSquared;
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        return new Point3D(A.X + ab.X * t, A.Y + ab.Y * t, A.Z + ab.Z * t);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Capsule3D(A={A}, B={B}, Radius={Radius})";
}
