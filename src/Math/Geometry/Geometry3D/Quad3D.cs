namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D quadrilateral defined by four vertices.</summary>
public readonly record struct Quad3D(Point3D A, Point3D B, Point3D C, Point3D D)
{
    /// <summary>The first vertex.</summary>
    public Point3D A { get; } = A;

    /// <summary>The second vertex.</summary>
    public Point3D B { get; } = B;

    /// <summary>The third vertex.</summary>
    public Point3D C { get; } = C;

    /// <summary>The fourth vertex.</summary>
    public Point3D D { get; } = D;

    /// <summary>Splits the quad into two triangles (ABC and ACD).</summary>
    /// <returns>The two triangles.</returns>
    public (Triangle3D tri1, Triangle3D tri2) Triangulate() =>
        (new Triangle3D(A, B, C), new Triangle3D(A, C, D));

    /// <summary>Gets the unit normal of the quad (computed from the first triangle).</summary>
    public Vector3D Normal
    {
        get
        {
            Vector3D ab = new(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3D ac = new(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
            return ab.Cross(ac).Normalize();
        }
    }

    /// <summary>Gets the centroid (average of the four vertices).</summary>
    public Point3D Centroid => new(
        (A.X + B.X + C.X + D.X) * 0.25,
        (A.Y + B.Y + C.Y + D.Y) * 0.25,
        (A.Z + B.Z + C.Z + D.Z) * 0.25);

    /// <summary>Gets the total area of the quad (sum of the two triangles).</summary>
    public double Area
    {
        get
        {
            var (tri1, tri2) = Triangulate();
            return tri1.Area + tri2.Area;
        }
    }

    /// <summary>Tests whether a point lies inside or on the quad.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is inside or on the quad.</returns>
    public bool Contains(Point3D p)
    {
        var (tri1, tri2) = Triangulate();
        return tri1.Contains(p) || tri2.Contains(p);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Quad3D(A={A}, B={B}, C={C}, D={D})";
}
