namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents a mesh edge defined by two vertex indices.</summary>
public readonly record struct Edge(int V0, int V1)
{
    /// <summary>First vertex index.</summary>
    public int V0 { get; } = V0;

    /// <summary>Second vertex index.</summary>
    public int V1 { get; } = V1;

    /// <summary>Returns the edge with reversed vertex order.</summary>
    /// <returns>An edge with the vertex indices swapped.</returns>
    public Edge Reversed() => new(V1, V0);

    /// <summary>Returns a canonical representation where V0 &lt;= V1.</summary>
    /// <returns>The canonical form of this edge.</returns>
    public Edge Canonical() => V0 <= V1 ? this : new Edge(V1, V0);
}
