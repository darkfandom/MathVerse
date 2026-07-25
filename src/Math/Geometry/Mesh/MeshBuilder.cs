using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Mesh;

/// <summary>
/// Provides a mutable builder for constructing triangle meshes incrementally.
/// </summary>
public class MeshBuilder
{
    private readonly List<Point3D> _vertices = new();
    private readonly List<int> _indices = new();

    /// <summary>Gets the current number of vertices in the builder.</summary>
    public int VertexCount => _vertices.Count;

    /// <summary>Gets the current number of indices in the builder.</summary>
    public int IndexCount => _indices.Count;

    /// <summary>
    /// Adds a vertex to the mesh and returns its index.
    /// </summary>
    /// <param name="vertex">The vertex position.</param>
    /// <returns>The index of the newly added vertex.</returns>
    public int AddVertex(Point3D vertex)
    {
        int index = _vertices.Count;
        _vertices.Add(vertex);
        return index;
    }

    /// <summary>
    /// Adds a triangle face defined by three vertex indices.
    /// </summary>
    /// <param name="i0">The first vertex index.</param>
    /// <param name="i1">The second vertex index.</param>
    /// <param name="i2">The third vertex index.</param>
    public void AddTriangle(int i0, int i1, int i2)
    {
        _indices.Add(i0);
        _indices.Add(i1);
        _indices.Add(i2);
    }

    /// <summary>
    /// Builds a read-only <see cref="TriangleMesh"/> from the current builder state.
    /// </summary>
    /// <returns>A new <see cref="TriangleMesh"/> containing the accumulated geometry.</returns>
    public TriangleMesh ToMesh()
    {
        return new TriangleMesh(
            _vertices.ToImmutableArray(),
            _indices.ToImmutableArray());
    }
}
