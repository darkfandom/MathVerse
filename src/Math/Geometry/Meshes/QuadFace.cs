namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Represents a quad face with four vertex indices.</summary>
public readonly record struct QuadFace(int V0, int V1, int V2, int V3)
{
    /// <summary>First vertex index.</summary>
    public int V0 { get; } = V0;

    /// <summary>Second vertex index.</summary>
    public int V1 { get; } = V1;

    /// <summary>Third vertex index.</summary>
    public int V2 { get; } = V2;

    /// <summary>Fourth vertex index.</summary>
    public int V3 { get; } = V3;

    /// <summary>Splits the quad into two triangles.</summary>
    /// <returns>A pair of triangular faces covering the quad.</returns>
    public (TriangleFace T0, TriangleFace T1) Triangulate() => (new(V0, V1, V2), new(V0, V2, V3));
}
