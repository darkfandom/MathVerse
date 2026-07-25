namespace MathVerse.Math.Geometry.Lighting;

using Geometry3D;

/// <summary>Spot light that illuminates in a cone.</summary>
public sealed record SpotLight : Light
{
    /// <summary>Light position.</summary>
    public Point3D Position { get; init; } = Point3D.Origin;

    /// <summary>Spot direction (will be normalized).</summary>
    public Vector3D Direction { get; init; } = new(0, -1, 0);

    /// <summary>Light color as (R, G, B) in [0,1].</summary>
    public (double R, double G, double B) Color { get; init; } = (1.0, 1.0, 1.0);

    /// <summary>Inner cone angle in degrees.</summary>
    public double InnerAngle { get; init; } = 30.0;

    /// <summary>Outer cone angle in degrees.</summary>
    public double OuterAngle { get; init; } = 45.0;

    /// <summary>Falloff exponent.</summary>
    public double Falloff { get; init; } = 1.0;
}
