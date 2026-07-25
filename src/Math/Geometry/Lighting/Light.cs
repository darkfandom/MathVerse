namespace MathVerse.Math.Geometry.Lighting;

/// <summary>Abstract base class for all light sources.</summary>
public abstract record Light
{
    /// <summary>Light intensity.</summary>
    public double Intensity { get; init; } = 1.0;

    /// <summary>Whether this light is enabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Light name.</summary>
    public string Name { get; init; } = "";
}
