namespace MathVerse.Math.Geometry.Lighting;

using Geometry3D;

/// <summary>Directional light that illuminates from a direction.</summary>
public sealed record DirectionalLight : Light
{
    /// <summary>Light direction (will be normalized).</summary>
    public Vector3D Direction { get; init; } = new(0, -1, 0);

    /// <summary>Light color as (R, G, B) in [0,1].</summary>
    public (double R, double G, double B) Color { get; init; } = (1.0, 1.0, 1.0);
}
