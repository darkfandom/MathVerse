using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Represents a standard indexed triangle mesh with vertex positions and an index buffer.</summary>
/// <param name="Vertices">The vertex positions of the mesh.</param>
/// <param name="Indices">The triangle index buffer (groups of 3).</param>
public readonly record struct IndexedMesh(ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices);

/// <summary>Provides factory methods for creating common mesh primitives.</summary>
public static class MeshFactory
{
    private const double Tolerance = 1e-10;
    private const double Pi = System.Math.PI;

    /// <summary>Creates a quad mesh centered at the given position with the specified normal direction and size.</summary>
    /// <param name="center">The center point of the quad.</param>
    /// <param name="normal">The normal direction of the quad plane.</param>
    /// <param name="size">The side length of the quad.</param>
    /// <returns>An <see cref="IndexedMesh"/> representing the quad.</returns>
    public static IndexedMesh CreateQuad(Point3D center, Vector3D normal, double size)
    {
        Vector3D n = normal.Normalize();
        double ax = System.Math.Abs(n.X), ay = System.Math.Abs(n.Y), az = System.Math.Abs(n.Z);
        Vector3D u;
        if (ax >= ay && ax >= az)
            u = Vector3D.UnitY;
        else if (ay >= ax && ay >= az)
            u = Vector3D.UnitX;
        else
            u = Vector3D.UnitX;
        u = n.Cross(u).Normalize();
        Vector3D v = n.Cross(u).Normalize();
        double half = size * 0.5;

        var vertices = ImmutableArray.Create(
            new Point3D(center.X - u.X * half - v.X * half, center.Y - u.Y * half - v.Y * half, center.Z - u.Z * half - v.Z * half),
            new Point3D(center.X + u.X * half - v.X * half, center.Y + u.Y * half - v.Y * half, center.Z + u.Z * half - v.Z * half),
            new Point3D(center.X + u.X * half + v.X * half, center.Y + u.Y * half + v.Y * half, center.Z + u.Z * half + v.Z * half),
            new Point3D(center.X - u.X * half + v.X * half, center.Y - u.Y * half + v.Y * half, center.Z - u.Z * half + v.Z * half));

        var indices = ImmutableArray.Create(0, 1, 2, 0, 2, 3);
        return new IndexedMesh(vertices, indices);
    }

    /// <summary>Creates a subdivided plane mesh in the XY plane centered at the origin.</summary>
    /// <param name="width">The total width of the plane (X extent).</param>
    /// <param name="height">The total height of the plane (Y extent).</param>
    /// <param name="subdivisionsX">The number of subdivisions along the X axis.</param>
    /// <param name="subdivisionsY">The number of subdivisions along the Y axis.</param>
    /// <returns>An <see cref="IndexedMesh"/> representing the subdivided plane.</returns>
    public static IndexedMesh CreatePlane(double width, double height, int subdivisionsX, int subdivisionsY)
    {
        if (subdivisionsX < 1) subdivisionsX = 1;
        if (subdivisionsY < 1) subdivisionsY = 1;
        int vertCountX = subdivisionsX + 1;
        int vertCountY = subdivisionsY + 1;
        var vertices = ImmutableArray.CreateBuilder<Point3D>(vertCountX * vertCountY);
        double halfW = width * 0.5;
        double halfH = height * 0.5;

        for (int y = 0; y < vertCountY; y++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                double px = -halfW + (double)x / subdivisionsX * width;
                double py = -halfH + (double)y / subdivisionsY * height;
                vertices.Add(new Point3D(px, py, 0));
            }
        }

        var indices = ImmutableArray.CreateBuilder<int>(subdivisionsX * subdivisionsY * 6);
        for (int y = 0; y < subdivisionsY; y++)
        {
            for (int x = 0; x < subdivisionsX; x++)
            {
                int i0 = y * vertCountX + x;
                int i1 = i0 + 1;
                int i2 = i0 + vertCountX;
                int i3 = i2 + 1;
                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i2);
            }
        }

        return new IndexedMesh(vertices.ToImmutable(), indices.ToImmutable());
    }

    /// <summary>Creates a UV sphere mesh centered at the given position with the specified radius.</summary>
    /// <param name="center">The center point of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="slices">The number of longitudinal slices (around the Y axis).</param>
    /// <param name="stacks">The number of latitudinal stacks (from pole to pole).</param>
    /// <returns>An <see cref="IndexedMesh"/> representing the UV sphere.</returns>
    public static IndexedMesh CreateSphere(Point3D center, double radius, int slices, int stacks)
    {
        if (slices < 3) slices = 3;
        if (stacks < 2) stacks = 2;
        int vertCount = (slices + 1) * (stacks + 1);
        var vertices = ImmutableArray.CreateBuilder<Point3D>(vertCount);

        for (int stack = 0; stack <= stacks; stack++)
        {
            double phi = Pi * stack / stacks;
            double sinPhi = System.Math.Sin(phi);
            double cosPhi = System.Math.Cos(phi);
            for (int slice = 0; slice <= slices; slice++)
            {
                double theta = 2.0 * Pi * slice / slices;
                double x = sinPhi * System.Math.Cos(theta);
                double y = cosPhi;
                double z = sinPhi * System.Math.Sin(theta);
                vertices.Add(new Point3D(
                    center.X + radius * x,
                    center.Y + radius * y,
                    center.Z + radius * z));
            }
        }

        var indices = ImmutableArray.CreateBuilder<int>(stacks * slices * 6);
        for (int stack = 0; stack < stacks; stack++)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                int i0 = stack * (slices + 1) + slice;
                int i1 = i0 + 1;
                int i2 = i0 + slices + 1;
                int i3 = i2 + 1;
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        return new IndexedMesh(vertices.ToImmutable(), indices.ToImmutable());
    }
}
