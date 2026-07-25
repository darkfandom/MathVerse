using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Provides methods for generating surfaces of revolution by rotating profile curves around an arbitrary axis.
/// </summary>
public static class RevolveSurface
{
    /// <summary>
    /// Generates a surface of revolution by rotating a profile curve around an axis through the specified origin.
    /// The profile is rotated through a full 360 degrees (2π radians).
    /// </summary>
    /// <param name="profile">The profile curve to revolve, defined as an array of 3D points in world coordinates.</param>
    /// <param name="axis">The direction vector of the revolution axis. Will be normalized internally.</param>
    /// <param name="origin">A point on the revolution axis.</param>
    /// <param name="angularSegments">The number of angular subdivisions around the revolution. Must be at least 3.</param>
    /// <returns>An immutable array of <see cref="SurfacePoint"/> representing the revolved surface in row-major order (profile point varies fastest, then angular).</returns>
    /// <exception cref="ArgumentException">Thrown when the profile has fewer than 2 points, angularSegments is less than 3, or the axis is zero.</exception>
    public static ImmutableArray<SurfacePoint> Generate(ImmutableArray<Point3D> profile, Vector3D axis, Point3D origin, int angularSegments)
    {
        if (profile.Length < 2)
            throw new ArgumentException("Profile must have at least 2 points.", nameof(profile));
        if (angularSegments < 3)
            throw new ArgumentException("Angular segments must be at least 3.", nameof(angularSegments));

        double axLen = System.Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);
        if (axLen < 1e-15)
            throw new ArgumentException("Axis vector must not be zero.", nameof(axis));

        Vector3D a = new Vector3D(axis.X / axLen, axis.Y / axLen, axis.Z / axLen);
        ComputeInitialFrame(profile, a, out Vector3D n, out Vector3D b);

        var builder = ImmutableArray.CreateBuilder<SurfacePoint>(profile.Length * (angularSegments + 1));

        for (int j = 0; j <= angularSegments; j++)
        {
            double angle = 2.0 * System.Math.PI * j / angularSegments;
            double cosA = System.Math.Cos(angle);
            double sinA = System.Math.Sin(angle);

            for (int i = 0; i < profile.Length; i++)
            {
                Vector3D rel = new Vector3D(
                    profile[i].X - origin.X,
                    profile[i].Y - origin.Y,
                    profile[i].Z - origin.Z);

                Point3D worldPos = RotateVector(rel, a, cosA, sinA);
                worldPos = new Point3D(worldPos.X + origin.X, worldPos.Y + origin.Y, worldPos.Z + origin.Z);

                double rn = rel.X * n.X + rel.Y * n.Y + rel.Z * n.Z;
                double rb = rel.X * b.X + rel.Y * b.Y + rel.Z * b.Z;
                Vector3D normal = ComputeRadialNormal(n, b, rn, rb, cosA, sinA);

                builder.Add(new SurfacePoint(worldPos, normal));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Rotates a vector around an axis using Rodrigues' rotation formula: v' = v*cosθ + (a×v)*sinθ + a*(a·v)*(1-cosθ).
    /// </summary>
    private static Point3D RotateVector(Vector3D v, Vector3D a, double cosA, double sinA)
    {
        double dot = v.X * a.X + v.Y * a.Y + v.Z * a.Z;

        Vector3D cross = new Vector3D(
            a.Y * v.Z - a.Z * v.Y,
            a.Z * v.X - a.X * v.Z,
            a.X * v.Y - a.Y * v.X);

        double f = 1.0 - cosA;

        return new Point3D(
            cosA * v.X + sinA * cross.X + f * dot * a.X,
            cosA * v.Y + sinA * cross.Y + f * dot * a.Y,
            cosA * v.Z + sinA * cross.Z + f * dot * a.Z);
    }

    /// <summary>
    /// Computes the surface normal at a revolution angle. The normal is the radial direction (perpendicular to axis) rotated by the same angle.
    /// </summary>
    private static Vector3D ComputeRadialNormal(Vector3D n, Vector3D b, double rn, double rb, double cosA, double sinA)
    {
        double mag = System.Math.Sqrt(rn * rn + rb * rb);
        if (mag < 1e-15)
            return new Vector3D(n.X, n.Y, n.Z);

        double radialN = rn / mag;
        double radialB = rb / mag;

        double normalN = cosA * radialN - sinA * radialB;
        double normalB = sinA * radialN + cosA * radialB;

        Vector3D normal = new Vector3D(
            normalN * n.X + normalB * b.X,
            normalN * n.Y + normalB * b.Y,
            normalN * n.Z + normalB * b.Z);

        double len = System.Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        if (len < 1e-15)
            return new Vector3D(n.X, n.Y, n.Z);

        return new Vector3D(normal.X / len, normal.Y / len, normal.Z / len);
    }

    /// <summary>
    /// Computes an initial orthonormal frame {a, n, b} perpendicular to the revolution axis from the first profile segment.
    /// </summary>
    private static void ComputeInitialFrame(ImmutableArray<Point3D> profile, Vector3D a, out Vector3D n, out Vector3D b)
    {
        Vector3D dir = new Vector3D(
            profile[1].X - profile[0].X,
            profile[1].Y - profile[0].Y,
            profile[1].Z - profile[0].Z);

        Vector3D perp = Cross(dir, a);
        double len = System.Math.Sqrt(perp.X * perp.X + perp.Y * perp.Y + perp.Z * perp.Z);

        if (len < 1e-15)
        {
            Vector3D arb = System.Math.Abs(a.X) < 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);
            perp = Cross(arb, a);
            len = System.Math.Sqrt(perp.X * perp.X + perp.Y * perp.Y + perp.Z * perp.Z);
        }

        if (len < 1e-15)
        {
            n = new Vector3D(1, 0, 0);
            b = Cross(a, n);
            Normalize(ref b);
            n = Cross(b, a);
            Normalize(ref n);
            return;
        }

        n = new Vector3D(perp.X / len, perp.Y / len, perp.Z / len);
        b = Cross(a, n);
        Normalize(ref b);
    }

    /// <summary>
    /// Normalizes a vector in place.
    /// </summary>
    private static void Normalize(ref Vector3D v)
    {
        double len = System.Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        if (len > 1e-15)
            v = new Vector3D(v.X / len, v.Y / len, v.Z / len);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    private static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }
}
