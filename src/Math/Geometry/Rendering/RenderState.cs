namespace MathVerse.Math.Geometry.Rendering;

using Colors;

/// <summary>Represents the current rendering state.</summary>
public sealed record RenderState
{
    /// <summary>Background color.</summary>
    public Color BackgroundColor { get; init; } = Color.Black;
    
    /// <summary>Wireframe mode.</summary>
    public bool Wireframe { get; init; }
    
    /// <summary>Backface culling enabled.</summary>
    public bool BackfaceCulling { get; init; } = true;
    
    /// <summary>Depth test enabled.</summary>
    public bool DepthTest { get; init; } = true;
    
    /// <summary>Alpha blending enabled.</summary>
    public bool AlphaBlending { get; init; }
    
    /// <summary>Line width.</summary>
    public double LineWidth { get; init; } = 1.0;
}
