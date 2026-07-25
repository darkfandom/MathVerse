namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Light source for the visualization scene, supporting directional, point, spot, and ambient types.</summary>
public sealed class Light
{
    /// <summary>Gets the unique identifier for this light.</summary>
    public string LightId { get; init; } = "";

    /// <summary>Gets the type of this light source.</summary>
    public LightType Type { get; init; }

    /// <summary>Gets the position of the light in world space.</summary>
    public Vector3 Position { get; init; }

    /// <summary>Gets the direction vector of the light (normalized).</summary>
    public Vector3 Direction { get; init; } = -Vector3.UnitY;

    /// <summary>Gets the color and intensity of the light in RGBA format.</summary>
    public Vector4 Color { get; init; } = Vector4.One;

    /// <summary>Gets the brightness multiplier of the light.</summary>
    public float Intensity { get; init; } = 1.0f;

    /// <summary>Gets the maximum effective range of the light in world units.</summary>
    public float Range { get; init; } = 50.0f;

    /// <summary>Gets the full cone angle of a spot light in degrees.</summary>
    public float SpotAngle { get; init; } = 45.0f;

    /// <summary>Gets the falloff exponent for spot light edge softening.</summary>
    public float SpotExponent { get; init; } = 1.0f;

    /// <summary>Creates a directional light with the specified direction and color.</summary>
    /// <param name="direction">The light direction vector (will be normalized).</param>
    /// <param name="color">The light color in RGBA format.</param>
    /// <param name="intensity">The brightness multiplier. Defaults to 1.0.</param>
    /// <returns>A new <see cref="Light"/> configured as a directional light.</returns>
    public static Light Directional(Vector3 direction, Vector4 color, float intensity = 1.0f) => new()
    {
        Type = LightType.Directional,
        Direction = Vector3.Normalize(direction),
        Color = color,
        Intensity = intensity
    };

    /// <summary>Creates a point light at the specified position with the given color.</summary>
    /// <param name="position">The world-space position of the light.</param>
    /// <param name="color">The light color in RGBA format.</param>
    /// <param name="intensity">The brightness multiplier. Defaults to 1.0.</param>
    /// <returns>A new <see cref="Light"/> configured as a point light.</returns>
    public static Light Point(Vector3 position, Vector4 color, float intensity = 1.0f) => new()
    {
        Type = LightType.Point,
        Position = position,
        Color = color,
        Intensity = intensity
    };

    /// <summary>Creates a spot light at the specified position pointing in the given direction.</summary>
    /// <param name="position">The world-space position of the light.</param>
    /// <param name="direction">The spotlight direction vector (will be normalized).</param>
    /// <param name="color">The light color in RGBA format.</param>
    /// <param name="angle">The full cone angle in degrees. Defaults to 45.</param>
    /// <returns>A new <see cref="Light"/> configured as a spot light.</returns>
    public static Light Spot(Vector3 position, Vector3 direction, Vector4 color, float angle = 45.0f) => new()
    {
        Type = LightType.Spot,
        Position = position,
        Direction = Vector3.Normalize(direction),
        Color = color,
        SpotAngle = angle
    };

    /// <summary>Creates an ambient light with the specified color and intensity.</summary>
    /// <param name="color">The ambient color in RGBA format.</param>
    /// <param name="intensity">The brightness multiplier. Defaults to 1.0.</param>
    /// <returns>A new <see cref="Light"/> configured as an ambient light.</returns>
    public static Light Ambient(Vector4 color, float intensity = 1.0f) => new()
    {
        Type = LightType.Ambient,
        Color = color,
        Intensity = intensity
    };

    /// <summary>Computes the attenuation factor for a point or spot light at the given distance.</summary>
    /// <param name="distance">The distance from the light to the surface point.</param>
    /// <returns>The attenuation multiplier in the range [0, 1].</returns>
    public float ComputeAttenuation(float distance)
    {
        if (Type == LightType.Directional || Type == LightType.Ambient)
            return 1.0f;

        if (distance >= Range)
            return 0.0f;

        float normalizedDist = distance / Range;
        float falloff = 1.0f - (normalizedDist * normalizedDist);
        return System.Math.Clamp(falloff * falloff, 0.0f, 1.0f);
    }

    /// <summary>Computes the spot light falloff for a point based on the angle from the light's direction.</summary>
    /// <param name="surfaceToLight">The normalized vector from the surface point to the light position.</param>
    /// <returns>The spot falloff multiplier in the range [0, 1].</returns>
    public float ComputeSpotFalloff(Vector3 surfaceToLight)
    {
        if (Type != LightType.Spot)
            return 1.0f;

        float cosAngle = Vector3.Dot(-surfaceToLight, Vector3.Normalize(Direction));
        float cosOuter = System.MathF.Cos(SpotAngle * 0.5f * System.MathF.PI / 180.0f);
        float t = System.Math.Clamp((cosAngle - cosOuter) / (1.0f - cosOuter), 0.0f, 1.0f);
        return System.MathF.Pow(t, SpotExponent);
    }
}

/// <summary>Enumerates the types of light sources.</summary>
public enum LightType
{
    /// <summary>A directional light that illuminates the entire scene from a single direction.</summary>
    Directional,

    /// <summary>A point light that emits in all directions from a single position.</summary>
    Point,

    /// <summary>A spot light that emits in a cone from a single position.</summary>
    Spot,

    /// <summary>An ambient light that uniformly illuminates all surfaces.</summary>
    Ambient
}
