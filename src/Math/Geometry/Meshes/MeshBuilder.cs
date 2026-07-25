using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Provides a mutable builder for constructing triangle and quad meshes incrementally.</summary>
public sealed class MeshBuilder
{
    private readonly List<Vertex> _vertices = new();
    private readonly List<TriangleFace> _triangles = new();
    private readonly List<QuadFace> _quads = new();
    private readonly List<Edge> _edges = new();

    /// <summary>Gets the current number of vertices.</summary>
    public int VertexCount => _vertices.Count;

    /// <summary>Gets the current number of triangle faces.</summary>
    public int TriangleCount => _triangles.Count;

    /// <summary>Gets the current number of quad faces.</summary>
    public int QuadCount => _quads.Count;

    /// <summary>Adds a vertex and returns its index.</summary>
    /// <param name="v">The vertex to add.</param>
    /// <returns>The index of the newly added vertex.</returns>
    public int AddVertex(Vertex v)
    {
        int index = _vertices.Count;
        _vertices.Add(v);
        return index;
    }

    /// <summary>Adds a vertex with the specified position and returns its index.</summary>
    /// <param name="position">The vertex position.</param>
    /// <returns>The index of the newly added vertex.</returns>
    public int AddVertex(Point3D position)
    {
        int index = _vertices.Count;
        _vertices.Add(new Vertex(position, Vector3D.Zero, (0.0, 0.0)));
        return index;
    }

    /// <summary>Adds a triangle face and returns its face index.</summary>
    /// <param name="v0">First vertex index.</param>
    /// <param name="v1">Second vertex index.</param>
    /// <param name="v2">Third vertex index.</param>
    /// <returns>The index of the newly added triangle face.</returns>
    public int AddTriangle(int v0, int v1, int v2)
    {
        int index = _triangles.Count;
        _triangles.Add(new TriangleFace(v0, v1, v2));
        return index;
    }

    /// <summary>Adds a quad face and returns its face index.</summary>
    /// <param name="v0">First vertex index.</param>
    /// <param name="v1">Second vertex index.</param>
    /// <param name="v2">Third vertex index.</param>
    /// <param name="v3">Fourth vertex index.</param>
    /// <returns>The index of the newly added quad face.</returns>
    public int AddQuad(int v0, int v1, int v2, int v3)
    {
        int index = _quads.Count;
        _quads.Add(new QuadFace(v0, v1, v2, v3));
        return index;
    }

    /// <summary>Adds an edge to the mesh.</summary>
    /// <param name="v0">First vertex index.</param>
    /// <param name="v1">Second vertex index.</param>
    public void AddEdge(int v0, int v1)
    {
        _edges.Add(new Edge(v0, v1));
    }

    /// <summary>Gets the vertex at the specified index.</summary>
    /// <param name="index">The vertex index.</param>
    /// <returns>The vertex at the given index.</returns>
    public Vertex GetVertex(int index) => _vertices[index];

    /// <summary>Sets the normal of the vertex at the specified index.</summary>
    /// <param name="index">The vertex index.</param>
    /// <param name="normal">The new normal vector.</param>
    public void SetVertexNormal(int index, Vector3D normal)
    {
        Vertex v = _vertices[index];
        _vertices[index] = new Vertex(v.Position, normal, v.UV);
    }

    /// <summary>Sets the UV coordinates of the vertex at the specified index.</summary>
    /// <param name="index">The vertex index.</param>
    /// <param name="u">The U texture coordinate.</param>
    /// <param name="v">The V texture coordinate.</param>
    public void SetVertexUV(int index, double u, double v)
    {
        Vertex vertex = _vertices[index];
        _vertices[index] = new Vertex(vertex.Position, vertex.Normal, (u, v));
    }

    /// <summary>Builds an immutable <see cref="TriangleMesh"/> from the current state.</summary>
    /// <returns>A new triangle mesh.</returns>
    public TriangleMesh Build()
    {
        return new TriangleMesh(
            _vertices.ToImmutableArray(),
            _triangles.ToImmutableArray());
    }

    /// <summary>Builds an immutable <see cref="QuadMesh"/> from the current state.</summary>
    /// <returns>A new quad mesh.</returns>
    public QuadMesh BuildQuadMesh()
    {
        return new QuadMesh(
            _vertices.ToImmutableArray(),
            _quads.ToImmutableArray());
    }

    /// <summary>Clears all vertices, faces, and edges from the builder.</summary>
    public void Clear()
    {
        _vertices.Clear();
        _triangles.Clear();
        _quads.Clear();
        _edges.Clear();
    }
}
