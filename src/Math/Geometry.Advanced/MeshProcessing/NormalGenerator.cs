using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Represents a vertex with its computed normal direction.</summary>
/// <param name="Position">The position of the vertex.</param>
/// <param name="Normal">The computed normal vector at this vertex.</param>
public readonly record struct VertexNormal(Point3D Position, Vector3D Normal);

/// <summary>Provides methods for generating vertex normals for triangle meshes.</summary>
public static class NormalGenerator
{
    private const double Tolerance = 1e-10;

    /// <summary>Computes smooth (area-weighted) vertex normals by averaging the face normals of all adjacent triangles.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An immutable array of normal vectors, one per vertex.</returns>
    public static ImmutableArray<Vector3D> GenerateSmoothNormals(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var normals = new Vector3D[vertices.Length];
        int triCount = indices.Length / 3;
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];
            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;
            Point3D p0 = vertices[i0], p1 = vertices[i1], p2 = vertices[i2];
            double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
            double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;
            double nx = ay * bz - az * by;
            double ny = az * bx - ax * bz;
            double nz = ax * by - ay * bx;
            double len = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < Tolerance) continue;
            nx /= len; ny /= len; nz /= len;
            normals[i0] = new Vector3D(normals[i0].X + nx, normals[i0].Y + ny, normals[i0].Z + nz);
            normals[i1] = new Vector3D(normals[i1].X + nx, normals[i1].Y + ny, normals[i1].Z + nz);
            normals[i2] = new Vector3D(normals[i2].X + nx, normals[i2].Y + ny, normals[i2].Z + nz);
        }
        var result = ImmutableArray.CreateBuilder<Vector3D>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            double l = normals[i].Length;
            result.Add(l > Tolerance ? new Vector3D(normals[i].X / l, normals[i].Y / l, normals[i].Z / l) : Vector3D.UnitZ);
        }
        return result.ToImmutable();
    }

    /// <summary>
    /// Computes flat normals by setting each vertex's normal to the face normal of its most recently processed triangle.
    /// Since a vertex can belong to multiple faces with different normals, flat shading requires per-face-vertex
    /// normals (i.e., vertices are conceptually duplicated per face). This method writes to a single normal per
    /// vertex, so the last face that references a given vertex determines its normal. For true flat shading,
    /// the caller should duplicate vertices per-face before calling this method, or use an index buffer that
    /// maps each face to its own set of vertex indices.
    /// </summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An array of normal vectors, one per vertex (last face wins for shared vertices).</returns>
    public static ImmutableArray<Vector3D> GenerateFlatNormals(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var normals = new Vector3D[vertices.Length];
        int triCount = indices.Length / 3;
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];
            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;
            Point3D p0 = vertices[i0], p1 = vertices[i1], p2 = vertices[i2];
            double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
            double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;
            double nx = ay * bz - az * by;
            double ny = az * bx - ax * bz;
            double nz = ax * by - ay * bx;
            double len = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < Tolerance) continue;
            nx /= len; ny /= len; nz /= len;
            Vector3D fn = new Vector3D(nx, ny, nz);
            normals[i0] = fn;
            normals[i1] = fn;
            normals[i2] = fn;
        }
        return ImmutableArray.Create(normals);
    }
}
