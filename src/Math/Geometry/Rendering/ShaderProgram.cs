namespace MathVerse.Math.Geometry.Rendering;

/// <summary>Backend-independent shader program abstraction.</summary>
public sealed record ShaderProgram
{
    /// <summary>Shader name.</summary>
    public string Name { get; init; } = "default";
    
    /// <summary>Vertex shader source.</summary>
    public string VertexSource { get; init; } = "";
    
    /// <summary>Fragment shader source.</summary>
    public string FragmentSource { get; init; } = "";
    
    /// <summary>Whether this shader is compiled and ready.</summary>
    public bool IsValid => !string.IsNullOrEmpty(VertexSource) && !string.IsNullOrEmpty(FragmentSource);
}
