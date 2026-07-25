namespace MathVerse.Math.Geometry.Lighting;

using Geometry3D;

/// <summary>Point light that emanates from a position.</summary>
public sealed record PointLight : Light
{
    /// <summary>Light position.</summary>
    public Point3D Position { get; init; } = Point3D.Origin;

    /// <summary>Light color as (R, G, B) in [0,1].</summary>
    public (double R, double G, double B) Color { get; init; } = (1.0, 1.0, 1.0);

    /// <summary>Attenuation constant.</summary>
    public double ConstantAttenuation { get; init; } = 1.0;

    /// <summary>Linear attenuation factor.</summary>
    public double LinearAttenuation { get; init; } = 0.0;

    /// <summary>Quadratic attenuation factor.</summary>
    public double QuadraticAttenuation { get; init; } = 0.0;

    /// <summary>Maximum effective range.</summary>
    public double Range { get; init; } = double.MaxValue;
}
