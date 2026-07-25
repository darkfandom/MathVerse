namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents a triangular face with three vertex indices.</summary>
public readonly record struct TriangleFace(int V0, int V1, int V2)
{
    /// <summary>First vertex index.</summary>
    public int V0 { get; } = V0;

    /// <summary>Second vertex index.</summary>
    public int V1 { get; } = V1;

    /// <summary>Third vertex index.</summary>
    public int V2 { get; } = V2;

    /// <summary>Returns the three edges of this triangle.</summary>
    public (Edge E0, Edge E1, Edge E2) Edges => (new Edge(V0, V1), new Edge(V1, V2), new Edge(V2, V0));

    /// <summary>Returns the vertex indices as an array.</summary>
    public int[] Indices => [V0, V1, V2];
}
