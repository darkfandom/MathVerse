using System.Collections.Immutable;
using MathVerse.Math.Geometry.Meshes;

namespace MathVerse.Math.Geometry.Topology;

/// <summary>Wraps a <see cref="TriangleMesh"/> and provides topological query operations.</summary>
public sealed class TopologyMesh
{
    private readonly TriangleMesh _mesh;
    private Dictionary<(int, int), List<int>>? _edgeFaceMap;
    private Dictionary<int, List<int>>? _vertexFaceMap;
    private List<ImmutableArray<int>>? _adjacentFacesCache;

    /// <summary>Gets the underlying triangle mesh.</summary>
    public TriangleMesh Mesh => _mesh;

    /// <summary>Initializes a new topology mesh wrapper.</summary>
    /// <param name="mesh">The triangle mesh to wrap.</param>
    public TopologyMesh(TriangleMesh mesh)
    {
        _mesh = mesh;
    }

    /// <summary>Returns the indices of all faces adjacent to the specified face.</summary>
    /// <param name="faceIndex">The face index.</param>
    /// <returns>An immutable array of adjacent face indices.</returns>
    public ImmutableArray<int> AdjacentFaces(int faceIndex)
    {
        BuildAdjacencyCaches();

        if (_adjacentFacesCache == null || faceIndex < 0 || faceIndex >= _mesh.Faces.Length)
            return ImmutableArray<int>.Empty;

        return _adjacentFacesCache[faceIndex];
    }

    /// <summary>Returns the indices of all faces that share the specified vertex.</summary>
    /// <param name="vertexIndex">The vertex index.</param>
    /// <returns>An immutable array of face indices containing this vertex.</returns>
    public ImmutableArray<int> VertexFaces(int vertexIndex)
    {
        BuildVertexFaceCache();

        if (_vertexFaceMap == null || !_vertexFaceMap.TryGetValue(vertexIndex, out List<int>? faces))
            return ImmutableArray<int>.Empty;

        return faces.ToImmutableArray();
    }

    /// <summary>Returns the indices of all faces that share the specified edge.</summary>
    /// <param name="edge">The edge to query.</param>
    /// <returns>An immutable array of face indices containing this edge.</returns>
    public ImmutableArray<int> EdgeFaces(Edge edge)
    {
        BuildEdgeFaceCache();

        if (_edgeFaceMap == null)
            return ImmutableArray<int>.Empty;

        Edge canonical = edge.Canonical();
        (int, int) key = (canonical.V0, canonical.V1);
        if (!_edgeFaceMap.TryGetValue(key, out List<int>? faces))
            return ImmutableArray<int>.Empty;

        return faces.ToImmutableArray();
    }

    /// <summary>Returns all boundary edges (edges shared by exactly one face).</summary>
    /// <returns>An immutable array of boundary edges.</returns>
    public ImmutableArray<Edge> BoundaryEdges()
    {
        BuildEdgeFaceCache();

        if (_edgeFaceMap == null)
            return ImmutableArray<Edge>.Empty;

        ImmutableArray<Edge>.Builder builder = ImmutableArray.CreateBuilder<Edge>();

        foreach (KeyValuePair<(int, int), List<int>> kvp in _edgeFaceMap)
        {
            if (kvp.Value.Count == 1)
                builder.Add(new Edge(kvp.Key.Item1, kvp.Key.Item2));
        }

        return builder.ToImmutable();
    }

    /// <summary>Determines whether every edge is shared by at most two faces.</summary>
    /// <returns>True if the mesh is manifold; otherwise, false.</returns>
    public bool IsManifold()
    {
        BuildEdgeFaceCache();

        if (_edgeFaceMap == null)
            return true;

        foreach (KeyValuePair<(int, int), List<int>> kvp in _edgeFaceMap)
        {
            if (kvp.Value.Count > 2)
                return false;
        }

        return true;
    }

    /// <summary>Determines whether the mesh is watertight (every edge shared by exactly two faces).</summary>
    /// <returns>True if the mesh is watertight; otherwise, false.</returns>
    public bool IsWatertight()
    {
        BuildEdgeFaceCache();

        if (_edgeFaceMap == null)
            return false;

        if (_mesh.Faces.Length == 0)
            return false;

        foreach (KeyValuePair<(int, int), List<int>> kvp in _edgeFaceMap)
        {
            if (kvp.Value.Count != 2)
                return false;
        }

        return true;
    }

    /// <summary>Computes the Euler characteristic of the mesh (V - E + F).</summary>
    /// <returns>The Euler characteristic.</returns>
    public int EulerCharacteristic()
    {
        int v = _mesh.VertexCount;
        int f = _mesh.Faces.Length;

        BuildEdgeFaceCache();
        int e = _edgeFaceMap?.Count ?? 0;

        return v - e + f;
    }

    /// <summary>Computes the connected components of the mesh based on face adjacency.</summary>
    /// <returns>An immutable array of components, each being an array of face indices.</returns>
    public ImmutableArray<ImmutableArray<int>> ConnectedComponents()
    {
        if (_mesh.Faces.Length == 0)
            return ImmutableArray<ImmutableArray<int>>.Empty;

        BuildAdjacencyCaches();

        bool[] visited = new bool[_mesh.Faces.Length];
        List<ImmutableArray<int>> components = new();

        for (int i = 0; i < _mesh.Faces.Length; i++)
        {
            if (visited[i])
                continue;

            List<int> component = new();
            Queue<int> queue = new();
            queue.Enqueue(i);
            visited[i] = true;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                component.Add(current);

                if (_adjacentFacesCache != null)
                {
                    foreach (int adj in _adjacentFacesCache[current])
                    {
                        if (!visited[adj])
                        {
                            visited[adj] = true;
                            queue.Enqueue(adj);
                        }
                    }
                }
            }

            components.Add(component.ToImmutableArray());
        }

        return components.ToImmutableArray();
    }

    /// <summary>Determines whether the mesh forms a closed surface (watertight with no boundary).</summary>
    /// <returns>True if the mesh is closed; otherwise, false.</returns>
    public bool IsClosed()
    {
        return IsWatertight() && BoundaryEdges().Length == 0;
    }

    private void BuildAdjacencyCaches()
    {
        if (_adjacentFacesCache != null)
            return;

        _adjacentFacesCache = new List<ImmutableArray<int>>(_mesh.Faces.Length);
        _edgeFaceMap = new Dictionary<(int, int), List<int>>();

        for (int i = 0; i < _mesh.Faces.Length; i++)
            _adjacentFacesCache.Add(ImmutableArray<int>.Empty);

        Dictionary<int, List<int>> vertexToFaces = new();

        for (int i = 0; i < _mesh.Faces.Length; i++)
        {
            TriangleFace f = _mesh.Faces[i];
            AddToSet(vertexToFaces, f.V0, i);
            AddToSet(vertexToFaces, f.V1, i);
            AddToSet(vertexToFaces, f.V2, i);

            int[] verts = { f.V0, f.V1, f.V2 };
            for (int j = 0; j < 3; j++)
            {
                int a = verts[j];
                int b = verts[(j + 1) % 3];
                (int, int) key = a < b ? (a, b) : (b, a);
                AddToSet(_edgeFaceMap, key, i);
            }
        }

        for (int i = 0; i < _mesh.Faces.Length; i++)
        {
            TriangleFace f = _mesh.Faces[i];
            HashSet<int> neighbors = new();

            CollectNeighbors(vertexToFaces, f.V0, i, neighbors);
            CollectNeighbors(vertexToFaces, f.V1, i, neighbors);
            CollectNeighbors(vertexToFaces, f.V2, i, neighbors);

            _adjacentFacesCache[i] = neighbors.ToImmutableArray();
        }
    }

    private void BuildVertexFaceCache()
    {
        if (_vertexFaceMap != null)
            return;

        _vertexFaceMap = new Dictionary<int, List<int>>();

        for (int i = 0; i < _mesh.Faces.Length; i++)
        {
            TriangleFace f = _mesh.Faces[i];
            AddToSet(_vertexFaceMap, f.V0, i);
            AddToSet(_vertexFaceMap, f.V1, i);
            AddToSet(_vertexFaceMap, f.V2, i);
        }
    }

    private void BuildEdgeFaceCache()
    {
        if (_edgeFaceMap != null)
            return;

        _edgeFaceMap = new Dictionary<(int, int), List<int>>();

        for (int i = 0; i < _mesh.Faces.Length; i++)
        {
            TriangleFace f = _mesh.Faces[i];
            int[] verts = { f.V0, f.V1, f.V2 };

            for (int j = 0; j < 3; j++)
            {
                int a = verts[j];
                int b = verts[(j + 1) % 3];
                (int, int) key = a < b ? (a, b) : (b, a);
                AddToSet(_edgeFaceMap, key, i);
            }
        }
    }

    private static void AddToSet<K>(Dictionary<K, List<int>> map, K key, int value) where K : notnull
    {
        if (!map.TryGetValue(key, out List<int>? list))
        {
            list = new List<int>();
            map[key] = list;
        }

        if (!list.Contains(value))
            list.Add(value);
    }

    private static void CollectNeighbors(Dictionary<int, List<int>> vertexToFaces, int vertex, int excludeFace, HashSet<int> neighbors)
    {
        if (!vertexToFaces.TryGetValue(vertex, out List<int>? faces))
            return;

        foreach (int face in faces)
        {
            if (face != excludeFace)
                neighbors.Add(face);
        }
    }
}
