using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents an immutable triangle mesh composed of vertices and triangular faces.</summary>
public sealed class TriangleMesh
{
    /// <summary>An empty triangle mesh with no vertices or faces.</summary>
    public static readonly TriangleMesh Empty = new(
        ImmutableArray<Vertex>.Empty,
        ImmutableArray<TriangleFace>.Empty);

    /// <summary>Gets the vertex data.</summary>
    public ImmutableArray<Vertex> Vertices { get; }

    /// <summary>Gets the triangle faces.</summary>
    public ImmutableArray<TriangleFace> Faces { get; }

    /// <summary>Initializes a new triangle mesh.</summary>
    /// <param name="vertices">The vertex data.</param>
    /// <param name="faces">The triangle faces.</param>
    public TriangleMesh(ImmutableArray<Vertex> vertices, ImmutableArray<TriangleFace> faces)
    {
        Vertices = vertices;
        Faces = faces;
    }

    /// <summary>Gets the number of vertices in this mesh.</summary>
    public int VertexCount => Vertices.Length;

    /// <summary>Gets the number of triangle faces in this mesh.</summary>
    public int TriangleCount => Faces.Length;

    /// <summary>Gets the number of unique edges in this mesh.</summary>
    public int EdgeCount => GetEdges().Length;

    /// <summary>Returns the vertices as a read-only list.</summary>
    /// <returns>A read-only list of vertices.</returns>
    public IReadOnlyList<Vertex> GetVertices() => Vertices;

    /// <summary>Returns the triangle faces as a read-only list.</summary>
    /// <returns>A read-only list of triangle faces.</returns>
    public IReadOnlyList<TriangleFace> GetTriangles() => Faces;

    /// <summary>Computes all unique edges from the triangle faces.</summary>
    /// <returns>An immutable array of unique edges in canonical form.</returns>
    public ImmutableArray<Edge> GetEdges()
    {
        HashSet<(int, int)> seen = new();
        ImmutableArray<Edge>.Builder builder = ImmutableArray.CreateBuilder<Edge>();

        for (int i = 0; i < Faces.Length; i++)
        {
            TriangleFace face = Faces[i];
            int[] indices = { face.V0, face.V1, face.V2 };
            for (int j = 0; j < 3; j++)
            {
                int a = indices[j];
                int b = indices[(j + 1) % 3];
                (int, int) key = a < b ? (a, b) : (b, a);
                if (seen.Add(key))
                    builder.Add(new Edge(a, b));
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>Computes an axis-aligned bounding box enclosing all vertices.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D BoundingBox()
    {
        if (Vertices.Length == 0)
            return new BoundingBox3D(Point3D.Origin, Point3D.Origin);

        Point3D min = Vertices[0].Position;
        Point3D max = min;

        for (int i = 1; i < Vertices.Length; i++)
        {
            Point3D p = Vertices[i].Position;
            min = new Point3D(
                System.Math.Min(min.X, p.X),
                System.Math.Min(min.Y, p.Y),
                System.Math.Min(min.Z, p.Z));
            max = new Point3D(
                System.Math.Max(max.X, p.X),
                System.Math.Max(max.Y, p.Y),
                System.Math.Max(max.Z, p.Z));
        }

        return new BoundingBox3D(min, max);
    }

    /// <summary>Returns a new mesh with face-area-weighted vertex normals computed.</summary>
    /// <returns>A new mesh with updated vertex normals.</returns>
    public TriangleMesh CalculateNormals()
    {
        ImmutableArray<Vector3D> normals = NormalGenerator.ComputeVertexNormals(this);
        Vertex[] updated = new Vertex[Vertices.Length];

        for (int i = 0; i < Vertices.Length; i++)
        {
            updated[i] = new Vertex(Vertices[i].Position, normals[i], Vertices[i].UV);
        }

        return new TriangleMesh(ImmutableArray.Create(updated), Faces);
    }

    /// <summary>Applies an affine transform to all vertices and normals, returning a new mesh.</summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new transformed mesh.</returns>
    public TriangleMesh Transform(Transform3D transform)
    {
        Transform3D normalTransform = transform.InverseTranspose3x3();
        Vertex[] result = new Vertex[Vertices.Length];

        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertex v = Vertices[i];
            result[i] = new Vertex(
                transform.TransformPoint(v.Position),
                normalTransform.TransformVector(v.Normal).Normalize(),
                v.UV);
        }

        return new TriangleMesh(ImmutableArray.Create(result), Faces);
    }

    /// <summary>Validates the mesh for topological and geometric correctness.</summary>
    /// <returns>A geometry result indicating success or describing validation errors.</returns>
    public GeometryResult Validate()
    {
        if (Vertices.Length == 0 && Faces.Length == 0)
            return GeometryResult.Ok();

        if (Vertices.Length == 0)
            return GeometryResult.Failure("Mesh has faces but no vertices.", GeometryDiagnosticType.EmptyMesh);

        if (Faces.Length == 0)
            return GeometryResult.Failure("Mesh has no faces.", GeometryDiagnosticType.EmptyMesh);

        for (int i = 0; i < Faces.Length; i++)
        {
            TriangleFace f = Faces[i];
            if (f.V0 < 0 || f.V0 >= Vertices.Length ||
                f.V1 < 0 || f.V1 >= Vertices.Length ||
                f.V2 < 0 || f.V2 >= Vertices.Length)
            {
                return GeometryResult.Failure(
                    $"Face {i} references out-of-range vertex index.",
                    GeometryDiagnosticType.InvalidFace);
            }

            if (f.V0 == f.V1 || f.V1 == f.V2 || f.V0 == f.V2)
            {
                return GeometryResult.Failure(
                    $"Face {i} is degenerate (duplicate vertex indices).",
                    GeometryDiagnosticType.DegenerateGeometry);
            }
        }

        return GeometryResult.Ok();
    }
}
