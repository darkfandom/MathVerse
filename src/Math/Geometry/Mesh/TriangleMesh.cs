using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Mesh;

/// <summary>
/// Represents an immutable triangle mesh composed of vertices and triangle index triples.
/// </summary>
public class TriangleMesh
{
    /// <summary>An empty triangle mesh with no vertices or faces.</summary>
    public static readonly TriangleMesh Empty = new(ImmutableArray<Point3D>.Empty, ImmutableArray<int>.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMesh"/> class.
    /// </summary>
    /// <param name="vertices">The vertex positions.</param>
    /// <param name="indices">The triangle index triples (each consecutive triple defines one face).</param>
    public TriangleMesh(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        Vertices = vertices;
        Indices = indices;
    }

    /// <summary>Gets the vertex positions.</summary>
    public ImmutableArray<Point3D> Vertices { get; }

    /// <summary>Gets the triangle index triples.</summary>
    public ImmutableArray<int> Indices { get; }

    /// <summary>Gets the number of vertices in this mesh.</summary>
    public int VertexCount => Vertices.Length;

    /// <summary>Gets the number of triangle faces in this mesh.</summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>Gets a value indicating whether this mesh has no faces.</summary>
    public bool IsEmpty => Indices.Length == 0;
}
