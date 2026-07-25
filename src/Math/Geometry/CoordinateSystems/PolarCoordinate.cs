using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a point in 2D polar coordinates (R, Theta).</summary>
/// <remarks>Theta is the azimuthal angle in radians measured from the positive X axis.</remarks>
public readonly record struct PolarCoordinate(double R, double Theta)
{
    /// <summary>Gets the radial distance from the origin.</summary>
    public double R { get; } = R;

    /// <summary>Gets the azimuthal angle in radians from the positive X axis.</summary>
    public double Theta { get; } = Theta;

    /// <summary>Converts this polar coordinate to a <see cref="CartesianCoordinate"/> with Z=0.</summary>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public CartesianCoordinate ToCartesian()
        => new(R * System.Math.Cos(Theta), R * System.Math.Sin(Theta), 0.0);

    /// <summary>Converts this polar coordinate to a <see cref="Point2D"/>.</summary>
    /// <returns>The equivalent 2D point.</returns>
    public Point2D ToCartesian2D()
        => new(R * System.Math.Cos(Theta), R * System.Math.Sin(Theta));

    /// <summary>Returns a string representation of this coordinate.</summary>
    public override string ToString() => $"(R={R:F6}, Theta={Theta:F6})";
}
