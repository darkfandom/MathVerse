namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D triangle defined by three vertices.</summary>
public readonly record struct Triangle3D(Point3D A, Point3D B, Point3D C)
{
    /// <summary>The first vertex.</summary>
    public Point3D A { get; } = A;

    /// <summary>The second vertex.</summary>
    public Point3D B { get; } = B;

    /// <summary>The third vertex.</summary>
    public Point3D C { get; } = C;

    /// <summary>Gets the unit normal of the triangle (computed via cross product).</summary>
    public Vector3D Normal
    {
        get
        {
            Vector3D ab = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3D ac = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
            return ab.Cross(ac).Normalize();
        }
    }

    /// <summary>Gets the area of the triangle.</summary>
    public double Area
    {
        get
        {
            Vector3D ab = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3D ac = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
            return ab.Cross(ac).Length * 0.5;
        }
    }

    /// <summary>Gets the perimeter of the triangle.</summary>
    public double Perimeter => A.DistanceTo(B) + B.DistanceTo(C) + C.DistanceTo(A);

    /// <summary>Gets the centroid (average of the three vertices).</summary>
    public Point3D Centroid => new(
        (A.X + B.X + C.X) / 3.0,
        (A.Y + B.Y + C.Y) / 3.0,
        (A.Z + B.Z + C.Z) / 3.0);

    /// <summary>Gets the circumcenter of the triangle.</summary>
    public Point3D Circumcenter
    {
        get
        {
            Vector3D u = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3D v = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);

            double uu = u.Dot(u);
            double vv = v.Dot(v);
            double uv = u.Dot(v);
            double d = uu * vv - uv * uv;

            if (System.Math.Abs(d) < 1e-30)
                return Centroid;

            double alpha = vv * (uu - uv) / (2.0 * d);
            double beta = uu * (vv - uv) / (2.0 * d);

            return new Point3D(
                A.X + alpha * u.X + beta * v.X,
                A.Y + alpha * u.Y + beta * v.Y,
                A.Z + alpha * u.Z + beta * v.Z);
        }
    }

    /// <summary>Computes the barycentric coordinates of a point with respect to this triangle.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The barycentric coordinates (u, v, w) where p = u*A + v*B + w*C.</returns>
    public (double u, double v, double w) BarycentricCoords(Point3D p)
    {
        Vector3D v0 = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
        Vector3D v1 = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
        Vector3D v2 = new(p.X - A.X, p.Y - A.Y, p.Z - A.Z);

        double d00 = v0.Dot(v0);
        double d01 = v0.Dot(v1);
        double d11 = v1.Dot(v1);
        double d20 = v2.Dot(v0);
        double d21 = v2.Dot(v1);

        double denom = d00 * d11 - d01 * d01;

        if (System.Math.Abs(denom) < 1e-30)
            return (0, 0, 0);

        double vCoord = (d11 * d20 - d01 * d21) / denom;
        double wCoord = (d00 * d21 - d01 * d20) / denom;
        double uCoord = 1.0 - vCoord - wCoord;

        return (uCoord, vCoord, wCoord);
    }

    /// <summary>Tests whether a point lies inside or on the triangle using barycentric coordinates.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is inside or on the triangle.</returns>
    public bool Contains(Point3D p)
    {
        var (u, v, w) = BarycentricCoords(p);
        const double eps = -1e-10;
        return u >= eps && v >= eps && w >= eps;
    }

    /// <summary>Finds the closest point on the triangle to a given point.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the triangle.</returns>
    public Point3D ClosestPoint(Point3D p)
    {
        Plane3D plane = Plane;
        Point3D projected = plane.Project(p);
        var (u, v, w) = BarycentricCoords(projected);

        if (u >= 0 && v >= 0 && w >= 0)
            return projected;

        Line3D edgeAB = new(A, B);
        Line3D edgeBC = new(B, C);
        Line3D edgeCA = new(C, A);

        Point3D cpAB = edgeAB.ClosestPoint(projected);
        Point3D cpBC = edgeBC.ClosestPoint(projected);
        Point3D cpCA = edgeCA.ClosestPoint(projected);

        double dAB = projected.DistanceSquaredTo(cpAB);
        double dBC = projected.DistanceSquaredTo(cpBC);
        double dCA = projected.DistanceSquaredTo(cpCA);

        if (dAB <= dBC && dAB <= dCA)
            return cpAB;
        if (dBC <= dCA)
            return cpBC;
        return cpCA;
    }

    /// <summary>Tests for intersection with a line segment using the Möller–Trumbore algorithm.</summary>
    /// <param name="line">The line segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public (bool hit, Point3D point) Intersect(Line3D line)
    {
        const double epsilon = 1e-10;

        Vector3D v0v1 = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
        Vector3D v0v2 = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
        Vector3D dir = new(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y, line.P2.Z - line.P1.Z);
        Vector3D orig = new(line.P1.X, line.P1.Y, line.P1.Z);

        Vector3D pvec = dir.Cross(v0v2);
        double det = v0v1.Dot(pvec);

        if (System.Math.Abs(det) < epsilon)
            return (false, Point3D.Origin);

        double invDet = 1.0 / det;
        Vector3D tvec = new(orig.X - A.X, orig.Y - A.Y, orig.Z - A.Z);
        double u = tvec.Dot(pvec) * invDet;

        if (u < 0.0 || u > 1.0)
            return (false, Point3D.Origin);

        Vector3D qvec = tvec.Cross(v0v1);
        double v = dir.Dot(qvec) * invDet;

        if (v < 0.0 || u + v > 1.0)
            return (false, Point3D.Origin);

        double t = v0v2.Dot(qvec) * invDet;

        if (t < epsilon || t > 1.0 - epsilon)
            return (false, Point3D.Origin);

        return (true, new Point3D(orig.X + dir.X * t, orig.Y + dir.Y * t, orig.Z + dir.Z * t));
    }

    /// <summary>Gets the plane containing this triangle.</summary>
    public Plane3D Plane => Plane3D.FromTriangle(this);

    /// <summary>Tests whether the triangle is degenerate (near-zero area).</summary>
    /// <param name="tol">The tolerance threshold.</param>
    /// <returns>True if the triangle is degenerate.</returns>
    public bool IsDegenerate(double tol = 1e-10) => Area < tol;

    /// <summary>Computes an axis-aligned bounding box enclosing this triangle.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox() => new(
        new Point3D(
            System.Math.Min(A.X, System.Math.Min(B.X, C.X)),
            System.Math.Min(A.Y, System.Math.Min(B.Y, C.Y)),
            System.Math.Min(A.Z, System.Math.Min(B.Z, C.Z))),
        new Point3D(
            System.Math.Max(A.X, System.Math.Max(B.X, C.X)),
            System.Math.Max(A.Y, System.Math.Max(B.Y, C.Y)),
            System.Math.Max(A.Z, System.Math.Max(B.Z, C.Z))));

    /// <inheritdoc/>
    public override string ToString() => $"Triangle3D(A={A}, B={B}, C={C})";
}
