namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a point in 3D spherical coordinates (R, Theta, Phi).</summary>
/// <remarks>
/// Theta is the azimuthal angle in radians from the positive X axis (in the XY plane).
/// Phi is the polar angle in radians from the positive Z axis.
/// </remarks>
public readonly record struct SphericalCoordinate(double R, double Theta, double Phi)
{
    /// <summary>Gets the radial distance from the origin.</summary>
    public double R { get; } = R;

    /// <summary>Gets the azimuthal angle in radians from the positive X axis.</summary>
    public double Theta { get; } = Theta;

    /// <summary>Gets the polar angle in radians from the positive Z axis.</summary>
    public double Phi { get; } = Phi;

    /// <summary>Converts this spherical coordinate to a <see cref="CartesianCoordinate"/>.</summary>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public CartesianCoordinate ToCartesian()
    {
        double sinPhi = System.Math.Sin(Phi);
        return new CartesianCoordinate(
            R * sinPhi * System.Math.Cos(Theta),
            R * sinPhi * System.Math.Sin(Theta),
            R * System.Math.Cos(Phi));
    }

    /// <summary>Returns a string representation of this coordinate.</summary>
    public override string ToString() => $"(R={R:F6}, Theta={Theta:F6}, Phi={Phi:F6})";
}
