using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Represents a vertex in the half-edge mesh structure with its position and an incident edge index.</summary>
/// <param name="Position">The 3D position of the vertex.</param>
/// <param name="EdgeIndex">The index of an outgoing half-edge from this vertex.</param>
public readonly record struct HEVertex(Point3D Position, int EdgeIndex);

/// <summary>Represents a half-edge in the half-edge mesh structure with connectivity information.</summary>
/// <param name="Next">The index of the next half-edge in the same face.</param>
/// <param name="Prev">The index of the previous half-edge in the same face.</param>
/// <param name="Twin">The index of the opposite half-edge.</param>
/// <param name="Face">The index of the face this half-edge belongs to (-1 for boundary).</param>
/// <param name="Vertex">The index of the vertex this half-edge originates from.</param>
public readonly record struct HEEdge(int Next, int Prev, int Twin, int Face, int Vertex);

/// <summary>Represents a face in the half-edge mesh structure.</summary>
/// <param name="EdgeIndex">The index of one half-edge belonging to this face.</param>
public readonly record struct HEFace(int EdgeIndex);

/// <summary>An advanced half-edge mesh data structure providing efficient topological queries for triangle meshes.</summary>
public class HalfEdgeMeshAdvanced
{
    private const double Tolerance = 1e-10;

    /// <summary>Gets the vertices of the half-edge mesh.</summary>
    public ImmutableArray<HEVertex> Vertices { get; }

    /// <summary>Gets the half-edges of the mesh.</summary>
    public ImmutableArray<HEEdge> Edges { get; }

    /// <summary>Gets the faces of the mesh.</summary>
    public ImmutableArray<HEFace> Faces { get; }

    /// <summary>Gets the total number of half-edges.</summary>
    public int EdgeCount => Edges.Length;

    /// <summary>Gets the total number of faces.</summary>
    public int FaceCount => Faces.Length;

    private HalfEdgeMeshAdvanced(ImmutableArray<HEVertex> vertices, ImmutableArray<HEEdge> edges, ImmutableArray<HEFace> faces)
    {
        Vertices = vertices;
        Edges = edges;
        Faces = faces;
    }

    /// <summary>Builds a half-edge mesh from an indexed triangle mesh.</summary>
    /// <param name="vertices">The vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>A new <see cref="HalfEdgeMeshAdvanced"/> with full connectivity.</returns>
    public static HalfEdgeMeshAdvanced Build(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var halfEdges = new List<HEEdge>();
        var faces = new List<HEFace>();
        var vertEdges = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) vertEdges[i] = -1;

        var edgeMap = new Dictionary<(int, int), int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3], i1 = indices[t * 3 + 1], i2 = indices[t * 3 + 2];
            int[] tri = { i0, i1, i2 };
            int[] edgeIndices = new int[3];
            for (int e = 0; e < 3; e++)
            {
                int v0 = tri[e], v1 = tri[(e + 1) % 3];
                int edgeIdx = halfEdges.Count;
                edgeIndices[e] = edgeIdx;
                halfEdges.Add(new HEEdge(-1, -1, -1, t, v0));
                vertEdges[v0] = edgeIdx;
                (int, int) key = (v0, v1);
                (int, int) revKey = (v1, v0);
                if (edgeMap.TryGetValue(revKey, out int twinIdx))
                {
                    halfEdges[edgeIdx] = new HEEdge(-1, -1, twinIdx, t, v0);
                    HEEdge twin = halfEdges[twinIdx];
                    halfEdges[twinIdx] = new HEEdge(twin.Next, twin.Prev, edgeIdx, twin.Face, twin.Vertex);
                }
                else
                    edgeMap[key] = edgeIdx;
            }
            for (int e = 0; e < 3; e++)
            {
                int next = edgeIndices[(e + 1) % 3];
                int prev = edgeIndices[(e + 2) % 3];
                HEEdge he = halfEdges[edgeIndices[e]];
                halfEdges[edgeIndices[e]] = new HEEdge(next, prev, he.Twin, he.Face, he.Vertex);
            }
            faces.Add(new HEFace(edgeIndices[0]));
        }

        var verticesOut = ImmutableArray.CreateBuilder<HEVertex>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
            verticesOut.Add(new HEVertex(vertices[i], vertEdges[i]));

        return new HalfEdgeMeshAdvanced(verticesOut.ToImmutable(), ImmutableArray.Create(halfEdges.ToArray()), ImmutableArray.Create(faces.ToArray()));
    }

    /// <summary>Converts the half-edge mesh back to an indexed triangle mesh.</summary>
    /// <returns>A tuple containing the vertex positions and triangle index buffer.</returns>
    public (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) ToIndexedMesh()
    {
        var verts = ImmutableArray.CreateBuilder<Point3D>(Vertices.Length);
        for (int i = 0; i < Vertices.Length; i++)
            verts.Add(Vertices[i].Position);

        var idx = ImmutableArray.CreateBuilder<int>(Faces.Length * 3);
        for (int f = 0; f < Faces.Length; f++)
        {
            int e0 = Faces[f].EdgeIndex;
            int e1 = Edges[e0].Next;
            int e2 = Edges[e1].Next;
            idx.Add(Edges[e0].Vertex);
            idx.Add(Edges[e1].Vertex);
            idx.Add(Edges[e2].Vertex);
        }

        return (verts.ToImmutable(), idx.ToImmutable());
    }

    /// <summary>Gets the vertex indices of the specified face.</summary>
    /// <param name="faceIndex">The zero-based index of the face.</param>
    /// <returns>An immutable array of vertex indices belonging to the face.</returns>
    public ImmutableArray<int> GetFaceVertices(int faceIndex)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        if (faceIndex < 0 || faceIndex >= Faces.Length) return result.ToImmutable();
        int e0 = Faces[faceIndex].EdgeIndex;
        int e = e0;
        do
        {
            result.Add(Edges[e].Vertex);
            e = Edges[e].Next;
        } while (e != e0 && result.Count < 100);
        return result.ToImmutable();
    }

    /// <summary>Gets the face indices of all faces adjacent to the specified vertex.</summary>
    /// <param name="vertexIndex">The zero-based index of the vertex.</param>
    /// <returns>An immutable array of face indices touching the vertex.</returns>
    public ImmutableArray<int> GetVertexFaces(int vertexIndex)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        if (vertexIndex < 0 || vertexIndex >= Vertices.Length) return result.ToImmutable();
        var visited = new HashSet<int>();
        int startEdge = Vertices[vertexIndex].EdgeIndex;
        if (startEdge < 0 || startEdge >= Edges.Length) return result.ToImmutable();
        int e = startEdge;
        do
        {
            int face = Edges[e].Face;
            if (face >= 0 && visited.Add(face))
                result.Add(face);
            int twin = Edges[e].Twin;
            if (twin < 0) break;
            e = Edges[twin].Next;
        } while (e != startEdge && visited.Count < 1000);
        return result.ToImmutable();
    }
}
