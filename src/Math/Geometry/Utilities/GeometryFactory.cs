using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Meshes;
using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Math.Geometry.Utilities;

/// <summary>Factory for creating common geometric primitives.</summary>
public static class GeometryFactory
{
    /// <summary>Creates a regular polygon centered at the origin.</summary>
    public static Polygon2D RegularPolygon(int sides, double radius)
    {
        if (sides < 3) throw new System.ArgumentOutOfRangeException(nameof(sides));
        var verts = ImmutableArray.CreateBuilder<Point2D>(sides);
        for (int i = 0; i < sides; i++)
        {
            double angle = 2.0 * System.Math.PI * i / sides - System.Math.PI * 0.5;
            verts.Add(new Point2D(radius * System.Math.Cos(angle), radius * System.Math.Sin(angle)));
        }
        return new Polygon2D(verts.ToImmutable());
    }

    /// <summary>Creates a grid of 2D points.</summary>
    public static ImmutableArray<Point2D> Grid2D(double xMin, double xMax, int xCount, double yMin, double yMax, int yCount)
    {
        var pts = ImmutableArray.CreateBuilder<Point2D>(xCount * yCount);
        for (int iy = 0; iy < yCount; iy++)
        {
            double y = yCount > 1 ? yMin + (yMax - yMin) * iy / (yCount - 1) : (yMin + yMax) * 0.5;
            for (int ix = 0; ix < xCount; ix++)
            {
                double x = xCount > 1 ? xMin + (xMax - xMin) * ix / (xCount - 1) : (xMin + xMax) * 0.5;
                pts.Add(new Point2D(x, y));
            }
        }
        return pts.ToImmutable();
    }

    /// <summary>Creates a grid of 3D points on the XZ plane.</summary>
    public static ImmutableArray<Point3D> Grid3D(double xMin, double xMax, int xCount, double zMin, double zMax, int zCount, double y = 0)
    {
        var pts = ImmutableArray.CreateBuilder<Point3D>(xCount * zCount);
        for (int iz = 0; iz < zCount; iz++)
        {
            double z = zCount > 1 ? zMin + (zMax - zMin) * iz / (zCount - 1) : (zMin + zMax) * 0.5;
            for (int ix = 0; ix < xCount; ix++)
            {
                double x = xCount > 1 ? xMin + (xMax - xMin) * ix / (xCount - 1) : (xMin + xMax) * 0.5;
                pts.Add(new Point3D(x, y, z));
            }
        }
        return pts.ToImmutable();
    }

    /// <summary>Creates a unit sphere mesh with the specified subdivisions.</summary>
    public static TriangleMesh UnitSphere(int subdivisions)
    {
        MeshBuilder mb = new();
        double phi = System.Math.PI * (3.0 - System.Math.Sqrt(5.0));
        int n = subdivisions * subdivisions;
        for (int i = 0; i < n; i++)
        {
            double y = 1.0 - (2.0 * i) / (n - 1);
            double radius = System.Math.Sqrt(1.0 - y * y);
            double theta = phi * i;
            mb.AddVertex(new Vertex(
                new Point3D(System.Math.Cos(theta) * radius, y, System.Math.Sin(theta) * radius),
                new Vector3D(System.Math.Cos(theta) * radius, y, System.Math.Sin(theta) * radius).Normalize(),
                ((double)(System.Math.Atan2(System.Math.Sin(theta) * radius, System.Math.Cos(theta) * radius) / (2 * System.Math.PI) + 0.5),
                 (double)(System.Math.Acos(y) / System.Math.PI))));
        }
        for (int i = 0; i < n - 1; i++)
        {
            mb.AddTriangle(i, (i + 1) % n, (i + 2) % n);
        }
        return mb.Build();
    }

    /// <summary>Creates a unit cube mesh.</summary>
    public static TriangleMesh UnitCube()
    {
        MeshBuilder mb = new();
        int v0 = mb.AddVertex(new Vertex(new Point3D(-0.5, -0.5, -0.5), new Vector3D(0, 0, -1), (0, 0)));
        int v1 = mb.AddVertex(new Vertex(new Point3D(0.5, -0.5, -0.5), new Vector3D(0, 0, -1), (1, 0)));
        int v2 = mb.AddVertex(new Vertex(new Point3D(0.5, 0.5, -0.5), new Vector3D(0, 0, -1), (1, 1)));
        int v3 = mb.AddVertex(new Vertex(new Point3D(-0.5, 0.5, -0.5), new Vector3D(0, 0, -1), (0, 1)));
        int v4 = mb.AddVertex(new Vertex(new Point3D(-0.5, -0.5, 0.5), new Vector3D(0, 0, 1), (0, 0)));
        int v5 = mb.AddVertex(new Vertex(new Point3D(0.5, -0.5, 0.5), new Vector3D(0, 0, 1), (1, 0)));
        int v6 = mb.AddVertex(new Vertex(new Point3D(0.5, 0.5, 0.5), new Vector3D(0, 0, 1), (1, 1)));
        int v7 = mb.AddVertex(new Vertex(new Point3D(-0.5, 0.5, 0.5), new Vector3D(0, 0, 1), (0, 1)));

        mb.AddTriangle(v0, v2, v1); mb.AddTriangle(v0, v3, v2);
        mb.AddTriangle(v4, v5, v6); mb.AddTriangle(v4, v6, v7);
        mb.AddTriangle(v0, v1, v5); mb.AddTriangle(v0, v5, v4);
        mb.AddTriangle(v2, v3, v7); mb.AddTriangle(v2, v7, v6);
        mb.AddTriangle(v0, v4, v7); mb.AddTriangle(v0, v7, v3);
        mb.AddTriangle(v1, v2, v6); mb.AddTriangle(v1, v6, v5);

        return mb.Build();
    }

    /// <summary>Creates a line between two 3D points.</summary>
    public static Line3D Line(Point3D a, Point3D b) => new(a, b);

    /// <summary>Creates a segment between two 2D points.</summary>
    public static Segment2D Segment(Point2D a, Point2D b) => new(a, b);

    /// <summary>Creates a plane from a point and normal.</summary>
    public static Plane3D Plane(Point3D point, Vector3D normal) => new(point, normal);

    /// <summary>Creates an axis-aligned bounding box from min/max corners.</summary>
    public static BoundingBox3D AABB(Point3D min, Point3D max) => new(min, max);

    /// <summary>Creates a bounding box from a set of points.</summary>
    public static BoundingBox3D AABBFromPoints(IReadOnlyList<Point3D> points) => BoundingBox3D.FromPoints(points);
}
