using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents a half-edge data structure for mesh connectivity queries.</summary>
public sealed class HalfEdgeMesh
{
    private readonly List<HalfEdge> _halfEdges = new();
    private readonly List<Vertex> _vertices = new();
    private readonly List<Face> _faces = new();

    /// <summary>Gets the half-edges.</summary>
    public IReadOnlyList<HalfEdge> HalfEdges => _halfEdges;

    /// <summary>Gets the vertices.</summary>
    public IReadOnlyList<Vertex> Vertices => _vertices;

    /// <summary>Gets the faces.</summary>
    public IReadOnlyList<Face> Faces => _faces;

    /// <summary>Gets the vertex count.</summary>
    public int VertexCount => _vertices.Count;

    /// <summary>Gets the half-edge count.</summary>
    public int HalfEdgeCount => _halfEdges.Count;

    /// <summary>Gets the face count.</summary>
    public int FaceCount => _faces.Count;

    /// <summary>Gets the edge count (half-edges / 2).</summary>
    public int EdgeCount => _halfEdges.Count / 2;

    /// <summary>Builds a half-edge mesh from a triangle mesh.</summary>
    public static HalfEdgeMesh FromTriangleMesh(TriangleMesh mesh)
    {
        var hem = new HalfEdgeMesh();
        var vertexMap = new Dictionary<Point3D, int>();

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            hem._vertices.Add(mesh.Vertices[i]);
        }

        Dictionary<(int, int), int> edgeMap = new();

        for (int f = 0; f < mesh.TriangleCount; f++)
        {
            TriangleFace face = mesh.Faces[f];
            int[] verts = { face.V0, face.V1, face.V2 };
            int[] heIndices = new int[3];

            for (int e = 0; e < 3; e++)
            {
                int from = verts[e];
                int to = verts[(e + 1) % 3];
                (int, int) edgeKey = (from, to);
                (int, int) oppKey = (to, from);

                int faceIdx = hem._faces.Count;
                int heIdx = hem._halfEdges.Count;

                if (edgeMap.TryGetValue(oppKey, out int oppIdx))
                {
                    hem._halfEdges.Add(new HalfEdge { Next = heIdx + 1, Opposite = oppIdx, Vertex = to, Face = faceIdx });
                    hem._halfEdges[oppIdx] = new HalfEdge
                    {
                        Next = hem._halfEdges[oppIdx].Next,
                        Opposite = heIdx,
                        Vertex = hem._halfEdges[oppIdx].Vertex,
                        Face = hem._halfEdges[oppIdx].Face
                    };
                }
                else
                {
                    hem._halfEdges.Add(new HalfEdge { Next = heIdx + 1, Opposite = -1, Vertex = to, Face = faceIdx });
                }

                edgeMap[edgeKey] = heIdx;
                heIndices[e] = heIdx;
            }

            hem._halfEdges[heIndices[2]] = new HalfEdge
            {
                Next = heIndices[0],
                Opposite = hem._halfEdges[heIndices[2]].Opposite,
                Vertex = hem._halfEdges[heIndices[2]].Vertex,
                Face = hem._halfEdges[heIndices[2]].Face
            };

            hem._faces.Add(new Face { HalfEdge = heIndices[0] });
        }

        return hem;
    }

    /// <summary>Gets the outgoing half-edges from a vertex.</summary>
    public IEnumerable<HalfEdge> GetOutgoingEdges(int vertexIndex)
    {
        for (int i = 0; i < _halfEdges.Count; i++)
        {
            if (_halfEdges[i].Vertex == vertexIndex)
            {
                int start = i;
                do
                {
                    yield return _halfEdges[i];
                    if (_halfEdges[i].Opposite < 0) break;
                    i = _halfEdges[_halfEdges[i].Opposite].Next;
                    if (i == start) break;
                } while (true);
                yield break;
            }
        }
    }

    /// <summary>Gets the vertex ring (one-ring neighborhood) of a vertex.</summary>
    public ImmutableArray<int> GetVertexRing(int vertexIndex)
    {
        var ring = ImmutableArray.CreateBuilder<int>();
        for (int i = 0; i < _halfEdges.Count; i++)
        {
            if (_halfEdges[i].Vertex == vertexIndex && _halfEdges[i].Opposite >= 0)
            {
                int oppIdx = _halfEdges[i].Opposite;
                ring.Add(_halfEdges[oppIdx].Vertex);
            }
        }
        return ring.ToImmutable();
    }

    /// <summary>Gets the adjacent faces of a face.</summary>
    public ImmutableArray<int> GetAdjacentFaces(int faceIndex)
    {
        var adj = ImmutableArray.CreateBuilder<int>();
        if (faceIndex < 0 || faceIndex >= _faces.Count) return adj.ToImmutable();

        int startHe = _faces[faceIndex].HalfEdge;
        int he = startHe;
        do
        {
            if (_halfEdges[he].Opposite >= 0)
            {
                int oppFace = _halfEdges[_halfEdges[he].Opposite].Face;
                if (oppFace >= 0) adj.Add(oppFace);
            }
            he = _halfEdges[he].Next;
        } while (he != startHe);

        return adj.ToImmutable();
    }

    /// <summary>Validates the half-edge mesh connectivity.</summary>
    public bool Validate()
    {
        for (int i = 0; i < _halfEdges.Count; i++)
        {
            HalfEdge he = _halfEdges[i];
            if (he.Next < 0 || he.Next >= _halfEdges.Count) return false;
            if (he.Opposite >= 0 && (_halfEdges[he.Opposite].Opposite != i)) return false;
            if (he.Vertex < 0 || he.Vertex >= _vertices.Count) return false;
            if (he.Face < 0 || he.Face >= _faces.Count) return false;
        }
        return true;
    }

    /// <summary>A half-edge record.</summary>
    public sealed class HalfEdge
    {
        /// <summary>Index of the next half-edge in the face.</summary>
        public int Next { get; init; }

        /// <summary>Index of the opposite (twin) half-edge, or -1 for boundary.</summary>
        public int Opposite { get; init; }

        /// <summary>Index of the vertex this half-edge points to.</summary>
        public int Vertex { get; init; }

        /// <summary>Index of the face this half-edge belongs to.</summary>
        public int Face { get; init; }
    }

    /// <summary>A face record.</summary>
    public sealed class Face
    {
        /// <summary>Index of one half-edge belonging to this face.</summary>
        public int HalfEdge { get; init; }
    }
}
