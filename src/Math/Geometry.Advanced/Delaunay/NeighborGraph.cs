using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Delaunay;

/// <summary>
/// Computes the edge set and vertex adjacency graph from a Delaunay triangulation.
/// </summary>
public static class NeighborGraph
{
    /// <summary>
    /// Extracts the unique edges from a list of Delaunay triangles.
    /// Each edge appears exactly once regardless of how many triangles share it.
    /// </summary>
    /// <param name="triangles">The list of Delaunay triangles.</param>
    /// <returns>An immutable array of unique edges.</returns>
    public static ImmutableArray<DelaunayEdge> ComputeEdges(ImmutableArray<DelaunayTriangle> triangles)
    {
        var edgeSet = new HashSet<(int, int)>();
        var result = ImmutableArray.CreateBuilder<DelaunayEdge>();

        for (int i = 0; i < triangles.Length; i++)
        {
            DelaunayTriangle tri = triangles[i];
            AddEdge(result, edgeSet, tri.V0, tri.V1);
            AddEdge(result, edgeSet, tri.V1, tri.V2);
            AddEdge(result, edgeSet, tri.V2, tri.V0);
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Builds a vertex-to-neighbors adjacency map from a list of Delaunay triangles.
    /// Each vertex maps to an immutable array of its neighboring vertex indices.
    /// </summary>
    /// <param name="triangles">The list of Delaunay triangles.</param>
    /// <returns>A dictionary mapping each vertex index to its sorted set of neighbor indices.</returns>
    public static Dictionary<int, ImmutableArray<int>> ComputeAdjacency(ImmutableArray<DelaunayTriangle> triangles)
    {
        var adj = new Dictionary<int, HashSet<int>>();

        for (int i = 0; i < triangles.Length; i++)
        {
            DelaunayTriangle tri = triangles[i];
            AddNeighbor(adj, tri.V0, tri.V1);
            AddNeighbor(adj, tri.V0, tri.V2);
            AddNeighbor(adj, tri.V1, tri.V0);
            AddNeighbor(adj, tri.V1, tri.V2);
            AddNeighbor(adj, tri.V2, tri.V0);
            AddNeighbor(adj, tri.V2, tri.V1);
        }

        var result = new Dictionary<int, ImmutableArray<int>>();
        foreach (var kvp in adj)
        {
            int[] sorted = kvp.Value.ToArray();
            System.Array.Sort(sorted);
            result[kvp.Key] = ImmutableArray.Create(sorted);
        }

        return result;
    }

    private static void AddEdge(ImmutableArray<DelaunayEdge>.Builder builder, HashSet<(int, int)> edgeSet, int v0, int v1)
    {
        int min = System.Math.Min(v0, v1);
        int max = System.Math.Max(v0, v1);
        if (edgeSet.Add((min, max)))
            builder.Add(new DelaunayEdge(min, max));
    }

    private static void AddNeighbor(Dictionary<int, HashSet<int>> adj, int vertex, int neighbor)
    {
        if (!adj.ContainsKey(vertex))
            adj[vertex] = new HashSet<int>();
        adj[vertex].Add(neighbor);
    }
}
