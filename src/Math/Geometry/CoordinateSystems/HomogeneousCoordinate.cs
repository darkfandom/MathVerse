using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a point in homogeneous coordinates (X, Y, Z, W).</summary>
/// <remarks>
/// When W=1 the coordinate represents a point; when W=0 it represents a direction.
/// Cartesian coordinates are obtained by dividing X, Y, Z by W.
/// </remarks>
public readonly record struct HomogeneousCoordinate(double X, double Y, double Z, double W)
{
    /// <summary>Gets the X component.</summary>
    public double X { get; } = X;

    /// <summary>Gets the Y component.</summary>
    public double Y { get; } = Y;

    /// <summary>Gets the Z component.</summary>
    public double Z { get; } = Z;

    /// <summary>Gets the W component (homogeneous denominator).</summary>
    public double W { get; } = W;

    /// <summary>Converts this homogeneous coordinate to a <see cref="CartesianCoordinate"/> by dividing by W.</summary>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    /// <exception cref="System.DivideByZeroException">Thrown when W is zero.</exception>
    public CartesianCoordinate ToCartesian()
    {
        if (System.Math.Abs(W) < 1e-15)
            throw new System.DivideByZeroException("Cannot convert a homogeneous coordinate with W=0 to Cartesian.");
        double inv = 1.0 / W;
        return new CartesianCoordinate(X * inv, Y * inv, Z * inv);
    }

    /// <summary>Creates a homogeneous coordinate from a <see cref="Point3D"/> with W=1.</summary>
    /// <param name="p">The 3D point.</param>
    /// <returns>The homogeneous coordinate representing the point.</returns>
    public static HomogeneousCoordinate FromPoint(Point3D p) => new(p.X, p.Y, p.Z, 1.0);

    /// <summary>Creates a homogeneous coordinate from a <see cref="Vector3D"/> with W=0 (direction).</summary>
    /// <param name="v">The direction vector.</param>
    /// <returns>The homogeneous coordinate representing the direction.</returns>
    public static HomogeneousCoordinate FromDirection(Vector3D v) => new(v.X, v.Y, v.Z, 0.0);

    /// <summary>Returns a string representation of this coordinate.</summary>
    public override string ToString() => $"({X:F6}, {Y:F6}, {Z:F6}, {W:F6})";
}
