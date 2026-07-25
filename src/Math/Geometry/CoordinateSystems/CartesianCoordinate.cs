using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a point in Cartesian coordinates (X, Y, Z).</summary>
public readonly record struct CartesianCoordinate(double X, double Y, double Z)
{
    /// <summary>Gets the X coordinate.</summary>
    public double X { get; } = X;

    /// <summary>Gets the Y coordinate.</summary>
    public double Y { get; } = Y;

    /// <summary>Gets the Z coordinate.</summary>
    public double Z { get; } = Z;

    /// <summary>Converts this Cartesian coordinate to polar coordinates (projected onto the XY plane).</summary>
    /// <returns>The equivalent polar coordinate.</returns>
    public PolarCoordinate ToPolar()
    {
        double r = System.Math.Sqrt(X * X + Y * Y);
        double theta = System.Math.Atan2(Y, X);
        return new PolarCoordinate(r, theta);
    }

    /// <summary>Converts this Cartesian coordinate to spherical coordinates.</summary>
    /// <returns>The equivalent spherical coordinate (R, Theta, Phi) where Theta is the azimuthal angle and Phi is the polar angle from the Z axis.</returns>
    public SphericalCoordinate ToSpherical()
    {
        double r = System.Math.Sqrt(X * X + Y * Y + Z * Z);
        if (r < 1e-15) return new SphericalCoordinate(0, 0, 0);
        double theta = System.Math.Atan2(Y, X);
        double phi = System.Math.Acos(System.Math.Clamp(Z / r, -1.0, 1.0));
        return new SphericalCoordinate(r, theta, phi);
    }

    /// <summary>Converts this Cartesian coordinate to cylindrical coordinates.</summary>
    /// <returns>The equivalent cylindrical coordinate.</returns>
    public CylindricalCoordinate ToCylindrical()
    {
        double r = System.Math.Sqrt(X * X + Y * Y);
        double theta = System.Math.Atan2(Y, X);
        return new CylindricalCoordinate(r, theta, Z);
    }

    /// <summary>Converts this Cartesian coordinate to a homogeneous coordinate with W=1.</summary>
    /// <returns>The equivalent homogeneous coordinate.</returns>
    public HomogeneousCoordinate ToHomogeneous() => new(X, Y, Z, 1.0);

    /// <summary>Converts this Cartesian coordinate to a <see cref="Point3D"/>.</summary>
    /// <returns>The equivalent 3D point.</returns>
    public Point3D ToPoint3D() => new(X, Y, Z);

    /// <summary>Creates a Cartesian coordinate from a spherical coordinate.</summary>
    /// <param name="s">The spherical coordinate.</param>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public static CartesianCoordinate FromSpherical(SphericalCoordinate s)
    {
        double sinPhi = System.Math.Sin(s.Phi);
        return new CartesianCoordinate(
            s.R * sinPhi * System.Math.Cos(s.Theta),
            s.R * sinPhi * System.Math.Sin(s.Theta),
            s.R * System.Math.Cos(s.Phi));
    }

    /// <summary>Creates a Cartesian coordinate from a cylindrical coordinate.</summary>
    /// <param name="c">The cylindrical coordinate.</param>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public static CartesianCoordinate FromCylindrical(CylindricalCoordinate c)
        => new(c.R * System.Math.Cos(c.Theta), c.R * System.Math.Sin(c.Theta), c.Z);

    /// <summary>Returns a string representation of this coordinate.</summary>
    public override string ToString() => $"({X:F6}, {Y:F6}, {Z:F6})";
}
