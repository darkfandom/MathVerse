namespace MathVerse.Math.Geometry.Rendering;

using Meshes;
using Transformations;

/// <summary>Represents a single render command.</summary>
public sealed record RenderCommand
{
    /// <summary>The mesh to render.</summary>
    public TriangleMesh Mesh { get; init; } = null!;
    
    /// <summary>World transform.</summary>
    public Transform3D Transform { get; init; } = Transform3D.Identity;
    
    /// <summary>Material name.</summary>
    public string MaterialName { get; init; } = "default";
    
    /// <summary>Render priority (lower = rendered first).</summary>
    public int Priority { get; init; }
}
