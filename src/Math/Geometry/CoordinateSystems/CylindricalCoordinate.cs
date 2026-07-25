namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a point in 3D cylindrical coordinates (R, Theta, Z).</summary>
/// <remarks>Theta is the azimuthal angle in radians measured from the positive X axis.</remarks>
public readonly record struct CylindricalCoordinate(double R, double Theta, double Z)
{
    /// <summary>Gets the radial distance from the Z axis.</summary>
    public double R { get; } = R;

    /// <summary>Gets the azimuthal angle in radians from the positive X axis.</summary>
    public double Theta { get; } = Theta;

    /// <summary>Gets the height along the Z axis.</summary>
    public double Z { get; } = Z;

    /// <summary>Converts this cylindrical coordinate to a <see cref="CartesianCoordinate"/>.</summary>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public CartesianCoordinate ToCartesian()
        => new(R * System.Math.Cos(Theta), R * System.Math.Sin(Theta), Z);

    /// <summary>Returns a string representation of this coordinate.</summary>
    public override string ToString() => $"(R={R:F6}, Theta={Theta:F6}, Z={Z:F6})";
}
