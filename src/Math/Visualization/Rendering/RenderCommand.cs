namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>A single draw command to be submitted to the renderer, containing all state needed for a draw call.</summary>
public sealed class RenderCommand
{
    /// <summary>Gets the identifier of the object this command renders.</summary>
    public string ObjectId { get; init; } = "";

    /// <summary>Gets the world transform matrix for this draw command.</summary>
    public Matrix4x4 Transform { get; init; } = Matrix4x4.Identity;

    /// <summary>Gets the material identifier to use when rendering this command.</summary>
    public string MaterialId { get; init; } = "default";

    /// <summary>Gets the type of primitive to render.</summary>
    public RenderPrimitiveType PrimitiveType { get; init; }

    /// <summary>Gets the number of vertices to draw.</summary>
    public int VertexCount { get; init; }

    /// <summary>Gets the number of indices to draw, or 0 if using non-indexed rendering.</summary>
    public int IndexCount { get; init; }

    /// <summary>Gets the sort key used for ordering commands within the render queue.</summary>
    public int SortKey { get; init; }

    /// <summary>Gets the view-space depth of this command for depth-based sorting.</summary>
    public float Depth { get; init; }
}

/// <summary>Enumerates the types of renderable primitives.</summary>
public enum RenderPrimitiveType
{
    /// <summary>Individual point primitives.</summary>
    Points,

    /// <summary>Independent line segments.</summary>
    Lines,

    /// <summary>A connected sequence of line segments.</summary>
    LineStrip,

    /// <summary>Independent triangles.</summary>
    Triangles,

    /// <summary>A connected strip of triangles sharing adjacent vertices.</summary>
    TriangleStrip,

    /// <summary>A fan of triangles sharing a common first vertex.</summary>
    TriangleFan,

    /// <summary>Quadrilateral primitives (two triangles per quad).</summary>
    Quads
}
