namespace MathVerse.Math.Geometry.Lighting;

/// <summary>Defines material properties for rendering.</summary>
public sealed record Material
{
    /// <summary>Material name.</summary>
    public string Name { get; init; } = "default";

    /// <summary>Ambient color (R, G, B).</summary>
    public (double R, double G, double B) AmbientColor { get; init; } = (0.2, 0.2, 0.2);

    /// <summary>Diffuse color (R, G, B).</summary>
    public (double R, double G, double B) DiffuseColor { get; init; } = (0.8, 0.8, 0.8);

    /// <summary>Specular color (R, G, B).</summary>
    public (double R, double G, double B) SpecularColor { get; init; } = (1.0, 1.0, 1.0);

    /// <summary>Specular exponent (shininess).</summary>
    public double Shininess { get; init; } = 32.0;

    /// <summary>Opacity (alpha) in [0,1].</summary>
    public double Opacity { get; init; } = 1.0;

    /// <summary>Emissive color (R, G, B).</summary>
    public (double R, double G, double B) EmissiveColor { get; init; } = (0.0, 0.0, 0.0);
}
