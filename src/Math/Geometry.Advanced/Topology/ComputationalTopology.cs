using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Topology;

/// <summary>
/// Describes the topological properties of a triangle mesh.
/// </summary>
/// <param name="EulerCharacteristic">The Euler characteristic (V - E + F).</param>
/// <param name="Genus">The genus (number of handles) of the surface.</param>
/// <param name="ConnectedComponents">The number of connected components.</param>
/// <param name="BoundaryLoops">The number of boundary edge loops.</param>
/// <param name="IsManifold">Whether the mesh is a 2-manifold.</param>
public readonly record struct TopologyInfo(
    int EulerCharacteristic,
    int Genus,
    int ConnectedComponents,
    int BoundaryLoops,
    bool IsManifold);

/// <summary>
/// Provides computational topology analysis for triangle meshes.
/// Computes Euler characteristic, genus, connected components, boundary loops, and manifold detection.
/// </summary>
public static class ComputationalTopology
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the full topological properties of a triangle mesh.
    /// Calculates Euler characteristic, genus, connected components, boundary loops,
    /// and manifold status.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>A <see cref="TopologyInfo"/> containing all computed topological properties.</returns>
    public static TopologyInfo Analyze(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int euler = ComputeEulerCharacteristic(vertices, indices);
        bool manifold = IsManifold(vertices, indices);
        var components = FindConnectedComponents(indices);
        var boundaryEdges = FindBoundaryEdges(indices);

        int boundaryLoops = CountBoundaryLoops(boundaryEdges, indices);
        int genus = ComputeGenus(euler, components.Length, boundaryLoops);

        return new TopologyInfo(euler, genus, components.Length, boundaryLoops, manifold);
    }

    /// <summary>
    /// Computes the Euler characteristic of the mesh using V - E + F.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>The Euler characteristic value.</returns>
    public static int ComputeEulerCharacteristic(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int V = vertices.Length;
        var edgeSet = new HashSet<(int, int)>();
        int triCount = indices.Length / 3;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= V || i1 >= V || i2 >= V)
                continue;

            AddOrientedEdge(edgeSet, i0, i1);
            AddOrientedEdge(edgeSet, i1, i2);
            AddOrientedEdge(edgeSet, i2, i0);
        }

        int E = edgeSet.Count;
        int F = triCount;

        return V - E + F;
    }

    /// <summary>
    /// Checks whether the mesh is a 2-manifold. A mesh is manifold if every edge
    /// is shared by at most two faces and every vertex has a disk-like neighborhood.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns><c>true</c> if the mesh is manifold; otherwise, <c>false</c>.</returns>
    public static bool IsManifold(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var edgeCount = new Dictionary<(int, int), int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            if (i0 == i1 || i1 == i2 || i0 == i2)
                return false;

            IncrementEdge(edgeCount, i0, i1);
            IncrementEdge(edgeCount, i1, i2);
            IncrementEdge(edgeCount, i2, i0);
        }

        foreach (var kvp in edgeCount)
        {
            if (kvp.Value > 2)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Finds connected components of the mesh using breadth-first search on face adjacency.
    /// Each component is a set of face indices that are reachable from each other via shared edges.
    /// </summary>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An array of connected components, each containing the face indices of that component.</returns>
    public static ImmutableArray<ImmutableArray<int>> FindConnectedComponents(ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        if (triCount == 0)
            return ImmutableArray<ImmutableArray<int>>.Empty;

        var adjacency = BuildEdgeFaceMap(indices, triCount);
        var visited = new bool[triCount];
        var components = ImmutableArray.CreateBuilder<ImmutableArray<int>>();

        for (int t = 0; t < triCount; t++)
        {
            if (visited[t]) continue;

            var component = ImmutableArray.CreateBuilder<int>();
            var queue = new Queue<int>();
            queue.Enqueue(t);
            visited[t] = true;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                component.Add(current);

                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (int f in neighbors)
                    {
                        if (!visited[f])
                        {
                            visited[f] = true;
                            queue.Enqueue(f);
                        }
                    }
                }
            }

            components.Add(component.ToImmutable());
        }

        return components.ToImmutable();
    }

    /// <summary>
    /// Finds boundary edges in the mesh. A boundary edge is an edge that belongs
    /// to exactly one triangle (appears only once in the edge-face incidence).
    /// </summary>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An array of boundary edge indices (pairs of vertex indices).</returns>
    public static ImmutableArray<int> FindBoundaryEdges(ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var edgeCount = new Dictionary<(int, int), int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            IncrementEdge(edgeCount, i0, i1);
            IncrementEdge(edgeCount, i1, i2);
            IncrementEdge(edgeCount, i2, i0);
        }

        var result = ImmutableArray.CreateBuilder<int>();
        foreach (var kvp in edgeCount)
        {
            if (kvp.Value == 1)
            {
                result.Add(kvp.Key.Item1);
                result.Add(kvp.Key.Item2);
            }
        }

        return result.ToImmutable();
    }

    private static void AddOrientedEdge(HashSet<(int, int)> edgeSet, int v0, int v1)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        edgeSet.Add((lo, hi));
    }

    private static int ComputeGenus(int euler, int connectedComponents, int boundaryLoops)
    {
        int genus = (2 * connectedComponents - euler - boundaryLoops) / 2;
        return System.Math.Max(0, genus);
    }

    private static int CountBoundaryLoops(ImmutableArray<int> boundaryEdges, ImmutableArray<int> indices)
    {
        if (boundaryEdges.Length == 0)
            return 0;

        int edgePairCount = boundaryEdges.Length / 2;
        var adjacency = new Dictionary<int, List<int>>();

        for (int i = 0; i < edgePairCount; i++)
        {
            int v0 = boundaryEdges[i * 2];
            int v1 = boundaryEdges[i * 2 + 1];

            if (!adjacency.ContainsKey(v0))
                adjacency[v0] = new List<int>();
            if (!adjacency.ContainsKey(v1))
                adjacency[v1] = new List<int>();

            adjacency[v0].Add(v1);
            adjacency[v1].Add(v0);
        }

        var visited = new HashSet<int>();
        int loops = 0;

        foreach (var start in adjacency.Keys)
        {
            if (!visited.Add(start)) continue;

            int current = start;
            int prev = -1;
            bool complete = false;

            while (!complete)
            {
                var neighbors = adjacency[current];
                bool foundNext = false;

                foreach (int next in neighbors)
                {
                    if (next != prev && !visited.Contains(next))
                    {
                        prev = current;
                        current = next;
                        visited.Add(current);
                        foundNext = true;
                        break;
                    }
                }

                if (!foundNext)
                {
                    if (current == start)
                        loops++;
                    complete = true;
                }
            }
        }

        return loops;
    }

    private static void IncrementEdge(Dictionary<(int, int), int> edgeCount, int v0, int v1)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        var key = (lo, hi);
        if (edgeCount.ContainsKey(key))
            edgeCount[key]++;
        else
            edgeCount[key] = 1;
    }

    private static Dictionary<int, List<int>> BuildEdgeFaceMap(ImmutableArray<int> indices, int triCount)
    {
        var map = new Dictionary<int, List<int>>();
        var edgeFace = new Dictionary<(int, int), List<int>>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            AddFaceToEdge(edgeFace, i0, i1, t);
            AddFaceToEdge(edgeFace, i1, i2, t);
            AddFaceToEdge(edgeFace, i2, i0, t);
        }

        for (int t = 0; t < triCount; t++)
        {
            map[t] = new List<int>();
        }

        foreach (var kvp in edgeFace)
        {
            if (kvp.Value.Count == 2)
            {
                map[kvp.Value[0]].Add(kvp.Value[1]);
                map[kvp.Value[1]].Add(kvp.Value[0]);
            }
        }

        return map;
    }

    private static void AddFaceToEdge(Dictionary<(int, int), List<int>> edgeFace, int v0, int v1, int face)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        var key = (lo, hi);
        if (!edgeFace.ContainsKey(key))
            edgeFace[key] = new List<int>();
        if (edgeFace[key].Count < 2)
            edgeFace[key].Add(face);
    }
}
