using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;
using Transform3D = MathVerse.Math.Geometry.Transformations.Transform3D;

namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Provides static convenience methods for coordinate system conversions and local/world transforms.</summary>
public static class CoordinateTransform
{
    /// <summary>Converts a Cartesian coordinate to spherical coordinates and encodes the result as a <see cref="Point3D"/> (R, Theta, Phi).</summary>
    /// <param name="p">The Cartesian coordinate to convert.</param>
    /// <returns>A <see cref="Point3D"/> whose X=R, Y=Theta, Z=Phi.</returns>
    public static Point3D CartesianToSpherical(CartesianCoordinate p)
    {
        SphericalCoordinate s = p.ToSpherical();
        return new Point3D(s.R, s.Theta, s.Phi);
    }

    /// <summary>Converts spherical coordinates to a Cartesian coordinate.</summary>
    /// <param name="r">The radial distance.</param>
    /// <param name="theta">The azimuthal angle in radians.</param>
    /// <param name="phi">The polar angle in radians from the Z axis.</param>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public static CartesianCoordinate SphericalToCartesian(double r, double theta, double phi)
        => CartesianCoordinate.FromSpherical(new SphericalCoordinate(r, theta, phi));

    /// <summary>Converts cylindrical coordinates to a Cartesian coordinate.</summary>
    /// <param name="r">The radial distance from the Z axis.</param>
    /// <param name="theta">The azimuthal angle in radians.</param>
    /// <param name="z">The height along the Z axis.</param>
    /// <returns>The equivalent Cartesian coordinate.</returns>
    public static CartesianCoordinate CylindricalToCartesian(double r, double theta, double z)
        => CartesianCoordinate.FromCylindrical(new CylindricalCoordinate(r, theta, z));

    /// <summary>Creates a local-to-world transformation from an origin, forward direction, and up direction.</summary>
    /// <param name="origin">The local origin position in world space.</param>
    /// <param name="forward">The forward direction of the local frame.</param>
    /// <param name="up">The up direction of the local frame.</param>
    /// <returns>The local-to-world transformation.</returns>
    public static Transform3D LocalToWorld(Point3D origin, Vector3D forward, Vector3D up)
    {
        Vector3D f = forward.Normalize();
        Vector3D r = f.Cross(up).Normalize();
        Vector3D u = r.Cross(f);

        return new Transform3D(ImmutableArray.Create(
            ImmutableArray.Create(r.X, r.Y, r.Z, origin.X),
            ImmutableArray.Create(u.X, u.Y, u.Z, origin.Y),
            ImmutableArray.Create(-f.X, -f.Y, -f.Z, origin.Z),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a world-to-local transformation from an origin, forward direction, and up direction.</summary>
    /// <param name="origin">The local origin position in world space.</param>
    /// <param name="forward">The forward direction of the local frame.</param>
    /// <param name="up">The up direction of the local frame.</param>
    /// <returns>The world-to-local transformation (the inverse of local-to-world).</returns>
    public static Transform3D WorldToLocal(Point3D origin, Vector3D forward, Vector3D up)
        => LocalToWorld(origin, forward, up).Inverse();
}
