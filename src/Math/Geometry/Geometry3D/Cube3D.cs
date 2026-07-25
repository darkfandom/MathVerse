using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents an axis-aligned cube defined by its center and side length.</summary>
public readonly record struct Cube3D(Point3D Center, double SideLength)
{
    /// <summary>The center of the cube.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The side length of the cube.</summary>
    public double SideLength { get; } = SideLength;

    /// <summary>Gets the volume of the cube.</summary>
    public double Volume => SideLength * SideLength * SideLength;

    /// <summary>Gets the surface area of the cube.</summary>
    public double SurfaceArea => 6.0 * SideLength * SideLength;

    /// <summary>Gets the eight vertices of the cube.</summary>
    public ImmutableArray<Point3D> Vertices
    {
        get
        {
            double h = SideLength * 0.5;
            return ImmutableArray.Create(
                new Point3D(Center.X - h, Center.Y - h, Center.Z - h),
                new Point3D(Center.X + h, Center.Y - h, Center.Z - h),
                new Point3D(Center.X + h, Center.Y + h, Center.Z - h),
                new Point3D(Center.X - h, Center.Y + h, Center.Z - h),
                new Point3D(Center.X - h, Center.Y - h, Center.Z + h),
                new Point3D(Center.X + h, Center.Y - h, Center.Z + h),
                new Point3D(Center.X + h, Center.Y + h, Center.Z + h),
                new Point3D(Center.X - h, Center.Y + h, Center.Z + h));
        }
    }

    /// <summary>Gets the six faces of the cube as quads.</summary>
    public ImmutableArray<Quad3D> Faces
    {
        get
        {
            ImmutableArray<Point3D> v = Vertices;
            return ImmutableArray.Create(
                new Quad3D(v[0], v[1], v[2], v[3]),
                new Quad3D(v[5], v[4], v[7], v[6]),
                new Quad3D(v[4], v[0], v[3], v[7]),
                new Quad3D(v[1], v[5], v[6], v[2]),
                new Quad3D(v[4], v[5], v[1], v[0]),
                new Quad3D(v[3], v[2], v[6], v[7]));
        }
    }

    /// <summary>Tests whether a point is inside or on the cube.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is contained within the cube.</returns>
    public bool Contains(Point3D p)
    {
        double h = SideLength * 0.5;
        double dx = System.Math.Abs(p.X - Center.X);
        double dy = System.Math.Abs(p.Y - Center.Y);
        double dz = System.Math.Abs(p.Z - Center.Z);
        return dx <= h && dy <= h && dz <= h;
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this cube.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox()
    {
        double h = SideLength * 0.5;
        return new BoundingBox3D(
            new Point3D(Center.X - h, Center.Y - h, Center.Z - h),
            new Point3D(Center.X + h, Center.Y + h, Center.Z + h));
    }

    /// <inheritdoc/>
    public override string ToString() => $"Cube3D(Center={Center}, SideLength={SideLength})";
}
