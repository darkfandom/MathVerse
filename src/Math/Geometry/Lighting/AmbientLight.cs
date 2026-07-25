namespace MathVerse.Math.Geometry.Lighting;

/// <summary>Ambient light that illuminates everything equally.</summary>
public sealed record AmbientLight : Light
{
    /// <summary>Ambient color as (R, G, B) in [0,1].</summary>
    public (double R, double G, double B) Color { get; init; } = (1.0, 1.0, 1.0);
}
