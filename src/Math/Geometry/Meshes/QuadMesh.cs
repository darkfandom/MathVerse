using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents an immutable quad mesh composed of vertices and quad faces.</summary>
public sealed class QuadMesh
{
    /// <summary>An empty quad mesh with no vertices or faces.</summary>
    public static readonly QuadMesh Empty = new(
        ImmutableArray<Vertex>.Empty,
        ImmutableArray<QuadFace>.Empty);

    /// <summary>Gets the vertex data.</summary>
    public ImmutableArray<Vertex> Vertices { get; }

    /// <summary>Gets the quad faces.</summary>
    public ImmutableArray<QuadFace> Quads { get; }

    /// <summary>Initializes a new quad mesh.</summary>
    /// <param name="vertices">The vertex data.</param>
    /// <param name="quads">The quad faces.</param>
    public QuadMesh(ImmutableArray<Vertex> vertices, ImmutableArray<QuadFace> quads)
    {
        Vertices = vertices;
        Quads = quads;
    }

    /// <summary>Gets the number of vertices in this mesh.</summary>
    public int VertexCount => Vertices.Length;

    /// <summary>Gets the number of quad faces in this mesh.</summary>
    public int QuadCount => Quads.Length;

    /// <summary>Returns the vertices as a read-only list.</summary>
    /// <returns>A read-only list of vertices.</returns>
    public IReadOnlyList<Vertex> GetVertices() => Vertices;

    /// <summary>Returns the quad faces as a read-only list.</summary>
    /// <returns>A read-only list of quad faces.</returns>
    public IReadOnlyList<QuadFace> GetQuads() => Quads;

    /// <summary>Converts this quad mesh into a triangle mesh by triangulating each quad.</summary>
    /// <returns>A new <see cref="TriangleMesh"/> with two triangles per quad.</returns>
    public TriangleMesh Triangulate()
    {
        Vertex[] vertices = new Vertex[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
            vertices[i] = Vertices[i];

        TriangleFace[] faces = new TriangleFace[Quads.Length * 2];

        for (int i = 0; i < Quads.Length; i++)
        {
            (TriangleFace t0, TriangleFace t1) = Quads[i].Triangulate();
            faces[i * 2] = t0;
            faces[i * 2 + 1] = t1;
        }

        return new TriangleMesh(
            ImmutableArray.Create(vertices),
            ImmutableArray.Create(faces));
    }

    /// <summary>Validates the mesh for topological and geometric correctness.</summary>
    /// <returns>A geometry result indicating success or describing validation errors.</returns>
    public GeometryResult Validate()
    {
        if (Vertices.Length == 0 && Quads.Length == 0)
            return GeometryResult.Ok();

        if (Vertices.Length == 0)
            return GeometryResult.Failure("Mesh has faces but no vertices.", GeometryDiagnosticType.EmptyMesh);

        if (Quads.Length == 0)
            return GeometryResult.Failure("Mesh has no faces.", GeometryDiagnosticType.EmptyMesh);

        for (int i = 0; i < Quads.Length; i++)
        {
            QuadFace f = Quads[i];
            if (f.V0 < 0 || f.V0 >= Vertices.Length ||
                f.V1 < 0 || f.V1 >= Vertices.Length ||
                f.V2 < 0 || f.V2 >= Vertices.Length ||
                f.V3 < 0 || f.V3 >= Vertices.Length)
            {
                return GeometryResult.Failure(
                    $"Face {i} references out-of-range vertex index.",
                    GeometryDiagnosticType.InvalidFace);
            }

            if (f.V0 == f.V1 || f.V1 == f.V2 || f.V2 == f.V3 || f.V3 == f.V0)
            {
                return GeometryResult.Failure(
                    $"Face {i} is degenerate (adjacent duplicate vertex indices).",
                    GeometryDiagnosticType.DegenerateGeometry);
            }
        }

        return GeometryResult.Ok();
    }
}
