namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Material definition for rendering with color, shader, and texture properties.</summary>
public sealed class Material
{
    /// <summary>Gets the unique identifier for this material.</summary>
    public string MaterialId { get; init; } = "default";

    /// <summary>Gets the human-readable display name of this material.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the base color of the material in RGBA format.</summary>
    public Vector4 BaseColor { get; init; } = new(0.5f, 0.5f, 0.5f, 1.0f);

    /// <summary>Gets the metallic factor, ranging from 0.0 (dielectric) to 1.0 (fully metallic).</summary>
    public float Metallic { get; init; }

    /// <summary>Gets the roughness factor, ranging from 0.0 (smooth) to 1.0 (rough).</summary>
    public float Roughness { get; init; } = 0.5f;

    /// <summary>Gets the opacity of the material, ranging from 0.0 (fully transparent) to 1.0 (fully opaque).</summary>
    public float Opacity { get; init; } = 1.0f;

    /// <summary>Gets a value indicating whether this material is rendered from both sides.</summary>
    public bool DoubleSided { get; init; }

    /// <summary>Gets the name of the shader program used to render this material.</summary>
    public string ShaderName { get; init; } = "standard";

    /// <summary>Gets the optional texture identifier for this material's albedo map.</summary>
    public string? TextureId { get; init; }

    /// <summary>Gets the blending mode used when rendering this material.</summary>
    public MaterialBlendMode BlendMode { get; init; }

    /// <summary>Gets a default gray opaque material.</summary>
    public static Material Default => new();

    /// <summary>Gets a wireframe material with black color and no shading.</summary>
    public static Material Wireframe => new() { ShaderName = "wireframe", BaseColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f) };

    /// <summary>Gets a semi-transparent material using alpha blending.</summary>
    public static Material Transparent => new() { BlendMode = MaterialBlendMode.Alpha, Opacity = 0.5f };

    /// <summary>Creates a new material with the specified identifier and base color.</summary>
    /// <param name="id">The unique material identifier.</param>
    /// <param name="baseColor">The base color in RGBA format.</param>
    /// <returns>A new <see cref="Material"/> instance.</returns>
    public static Material Create(string id, Vector4 baseColor) => new() { MaterialId = id, BaseColor = baseColor };

    /// <summary>Creates a new material with the specified identifier, base color, and opacity.</summary>
    /// <param name="id">The unique material identifier.</param>
    /// <param name="baseColor">The base color in RGBA format.</param>
    /// <param name="opacity">The opacity value from 0.0 to 1.0.</param>
    /// <returns>A new <see cref="Material"/> instance.</returns>
    public static Material Create(string id, Vector4 baseColor, float opacity) => new()
    {
        MaterialId = id,
        BaseColor = baseColor,
        Opacity = opacity,
        BlendMode = opacity < 1.0f ? MaterialBlendMode.Alpha : MaterialBlendMode.Opaque
    };

    /// <summary>Gets a value indicating whether this material requires alpha blending.</summary>
    public bool IsTransparent => BlendMode != MaterialBlendMode.Opaque || Opacity < 1.0f;
}

/// <summary>Enumerates the available blending modes for materials.</summary>
public enum MaterialBlendMode
{
    /// <summary>No blending; the material is fully opaque.</summary>
    Opaque,

    /// <summary>Standard alpha blending based on source alpha.</summary>
    Alpha,

    /// <summary>Additive blending that accumulates color values.</summary>
    Additive,

    /// <summary>Multiplicative blending that darkens the output.</summary>
    Multiply
}
