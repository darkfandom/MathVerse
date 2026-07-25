using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Represents a winged-edge data structure entry encoding edge connectivity.</summary>
/// <param name="V0">The index of the first vertex of the edge.</param>
/// <param name="V1">The index of the second vertex of the edge.</param>
/// <param name="Face">The index of the face on the left side of the edge.</param>
/// <param name="OppositeFace">The index of the face on the right side of the edge (-1 for boundary).</param>
/// <param name="NextCW">The index of the next edge clockwise around the face.</param>
/// <param name="NextCCW">The index of the next edge counter-clockwise around the face.</param>
/// <param name="PrevCW">The index of the previous edge clockwise around the face.</param>
/// <param name="PrevCCW">The index of the previous edge counter-clockwise around the face.</param>
public readonly record struct WEdge(int V0, int V1, int Face, int OppositeFace, int NextCW, int NextCCW, int PrevCW, int PrevCCW);

/// <summary>A winged-edge mesh data structure providing efficient edge-based traversal of triangle meshes.</summary>
public class WingedEdgeMesh
{
    private const double Tolerance = 1e-10;

    /// <summary>Gets the vertices of the mesh.</summary>
    public ImmutableArray<Point3D> Vertices { get; }

    /// <summary>Gets the winged edges of the mesh.</summary>
    public ImmutableArray<WEdge> Edges { get; }

    /// <summary>Gets the faces of the mesh, each face being a list of vertex indices.</summary>
    public ImmutableArray<ImmutableArray<int>> Faces { get; }

    private WingedEdgeMesh(ImmutableArray<Point3D> vertices, ImmutableArray<WEdge> edges, ImmutableArray<ImmutableArray<int>> faces)
    {
        Vertices = vertices;
        Edges = edges;
        Faces = faces;
    }

    /// <summary>Builds a winged-edge mesh from an indexed triangle mesh.</summary>
    /// <param name="vertices">The vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>A new <see cref="WingedEdgeMesh"/> with full edge connectivity.</returns>
    public static WingedEdgeMesh FromTriangleMesh(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var edgeMap = new Dictionary<(int, int), int>();
        var edges = new List<WEdge>();
        var faceEdges = new List<ImmutableArray<int>>();
        var vertexEdgeMap = new List<HashSet<int>>();
        for (int i = 0; i < vertices.Length; i++)
            vertexEdgeMap.Add(new HashSet<int>());

        int[] faceEdgeCounts = new int[triCount];
        for (int f = 0; f < triCount; f++)
        {
            int i0 = indices[f * 3];
            int i1 = indices[f * 3 + 1];
            int i2 = indices[f * 3 + 2];
            int[] tri = { i0, i1, i2 };
            var fe = ImmutableArray.CreateBuilder<int>();
            for (int e = 0; e < 3; e++)
            {
                int v0 = tri[e];
                int v1 = tri[(e + 1) % 3];
                (int, int) key = v0 < v1 ? (v0, v1) : (v1, v0);
                int edgeIdx;
                if (edgeMap.TryGetValue(key, out int existing))
                {
                    edges[existing] = new WEdge(
                        edges[existing].V0, edges[existing].V1,
                        edges[existing].Face, f,
                        edges[existing].NextCW, edges[existing].NextCCW,
                        edges[existing].PrevCW, edges[existing].PrevCCW);
                    edgeIdx = existing;
                }
                else
                {
                    edgeIdx = edges.Count;
                    edgeMap[key] = edgeIdx;
                    edges.Add(new WEdge(v0, v1, f, -1, -1, -1, -1, -1));
                    vertexEdgeMap[v0].Add(edgeIdx);
                    vertexEdgeMap[v1].Add(edgeIdx);
                }
                fe.Add(edgeIdx);
            }
            faceEdges.Add(fe.ToImmutable());
        }

        for (int f = 0; f < triCount; f++)
        {
            ImmutableArray<int> fe = faceEdges[f];
            for (int e = 0; e < 3; e++)
            {
                int curr = fe[e];
                int next = fe[(e + 1) % 3];
                int prev = fe[(e + 2) % 3];
                WEdge w = edges[curr];
                edges[curr] = new WEdge(w.V0, w.V1, w.Face, w.OppositeFace, next, prev, prev, next);
            }
        }

        var faces = ImmutableArray.CreateBuilder<ImmutableArray<int>>(triCount);
        for (int f = 0; f < triCount; f++)
        {
            faces.Add(ImmutableArray.Create(indices[f * 3], indices[f * 3 + 1], indices[f * 3 + 2]));
        }

        return new WingedEdgeMesh(vertices, ImmutableArray.Create(edges.ToArray()), faces.ToImmutable());
    }

    /// <summary>Gets the indices of all edges belonging to the specified face.</summary>
    /// <param name="faceIndex">The zero-based index of the face.</param>
    /// <returns>An immutable array of edge indices for the face.</returns>
    public ImmutableArray<int> GetFaceEdges(int faceIndex)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        if (faceIndex < 0 || faceIndex >= Faces.Length)
            return result.ToImmutable();

        for (int i = 0; i < Edges.Length; i++)
        {
            if (Edges[i].Face == faceIndex || Edges[i].OppositeFace == faceIndex)
                result.Add(i);
        }
        return result.ToImmutable();
    }

    /// <summary>Gets the indices of all edges incident to the specified vertex.</summary>
    /// <param name="vertexIndex">The zero-based index of the vertex.</param>
    /// <returns>An immutable array of edge indices touching the vertex.</returns>
    public ImmutableArray<int> GetVertexEdges(int vertexIndex)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        for (int i = 0; i < Edges.Length; i++)
        {
            if (Edges[i].V0 == vertexIndex || Edges[i].V1 == vertexIndex)
                result.Add(i);
        }
        return result.ToImmutable();
    }

    /// <summary>Gets the indices of all faces adjacent to the specified face.</summary>
    /// <param name="faceIndex">The zero-based index of the face.</param>
        /// <returns>An immutable array of adjacent face indices.</returns>
    public ImmutableArray<int> GetAdjacentFaces(int faceIndex)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        var adjacent = new HashSet<int>();
        for (int i = 0; i < Edges.Length; i++)
        {
            WEdge e = Edges[i];
            if (e.Face == faceIndex && e.OppositeFace >= 0 && e.OppositeFace != faceIndex)
                adjacent.Add(e.OppositeFace);
            else if (e.OppositeFace == faceIndex && e.Face >= 0 && e.Face != faceIndex)
                adjacent.Add(e.Face);
        }
        foreach (int adj in adjacent)
            result.Add(adj);
        return result.ToImmutable();
    }
}
