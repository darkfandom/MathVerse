using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Provides methods for generating loft surfaces by interpolating between profile curves using cubic Hermite interpolation.
/// </summary>
public static class LoftSurface
{
    /// <summary>
    /// Generates a loft surface by interpolating between profile curves using cubic Hermite blending.
    /// Each profile curve is a polyline defined by control points. The loft is created by linearly interpolating
    /// corresponding points across profiles, with tangent continuity ensured by computing tangents from adjacent profiles.
    /// </summary>
    /// <param name="profiles">An immutable array of profile curves. Each profile is an immutable array of <see cref="Point3D"/> defining a polyline. All profiles must have the same number of points.</param>
    /// <param name="vSegments">The number of interpolation steps between consecutive profiles. Must be at least 1.</param>
    /// <returns>An immutable array of <see cref="SurfacePoint"/> representing the lofted surface in row-major order (u varies fastest).</returns>
    /// <exception cref="ArgumentException">Thrown when fewer than 2 profiles are provided, profiles have inconsistent point counts, or vSegments is less than 1.</exception>
    public static ImmutableArray<SurfacePoint> Generate(ImmutableArray<ImmutableArray<Point3D>> profiles, int vSegments)
    {
        if (profiles.Length < 2)
            throw new ArgumentException("At least 2 profiles are required for lofting.", nameof(profiles));
        if (vSegments < 1)
            throw new ArgumentException("vSegments must be at least 1.", nameof(vSegments));

        int profileCount = profiles.Length;
        int uPointCount = profiles[0].Length;

        for (int p = 1; p < profileCount; p++)
        {
            if (profiles[p].Length != uPointCount)
                throw new ArgumentException($"Profile {p} has {profiles[p].Length} points, expected {uPointCount}.", nameof(profiles));
        }

        double[] profileParams = new double[profileCount];
        for (int i = 0; i < profileCount; i++)
            profileParams[i] = (double)i / (profileCount - 1);

        var builder = ImmutableArray.CreateBuilder<SurfacePoint>();

        int totalV = (profileCount - 1) * vSegments;
        for (int j = 0; j <= totalV; j++)
        {
            double vParam = (double)j / totalV;
            int segment = System.Math.Min((int)(vParam * (profileCount - 1)), profileCount - 2);
            if (segment >= profileCount - 1) segment = profileCount - 2;

            double t = vParam * (profileCount - 1) - segment;
            t = Clamp01(t);

            for (int i = 0; i < uPointCount; i++)
            {
                Point3D p0 = profiles[segment][i];
                Point3D p1 = profiles[segment + 1][i];

                Vector3D m0 = ComputeTangent(profiles, segment, i, profileParams);
                Vector3D m1 = ComputeTangent(profiles, segment + 1, i, profileParams);

                Point3D position = HermiteInterpolate(p0, p1, m0, m1, t);
                Vector3D normal = ComputeNormal(profiles, segment, i, t, profileParams, uPointCount);

                builder.Add(new SurfacePoint(position, normal));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Performs cubic Hermite interpolation between two points given tangent vectors at each endpoint.
    /// </summary>
    private static Point3D HermiteInterpolate(Point3D p0, Point3D p1, Vector3D m0, Vector3D m1, double t)
    {
        double t2 = t * t;
        double t3 = t2 * t;

        double h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
        double h10 = t3 - 2.0 * t2 + t;
        double h01 = -2.0 * t3 + 3.0 * t2;
        double h11 = t3 - t2;

        return new Point3D(
            h00 * p0.X + h10 * m0.X + h01 * p1.X + h11 * m1.X,
            h00 * p0.Y + h10 * m0.Y + h01 * p1.Y + h11 * m1.Y,
            h00 * p0.Z + h10 * m0.Z + h01 * p1.Z + h11 * m1.Z);
    }

    /// <summary>
    /// Computes a tangent vector along the v-direction at a given profile and point index using finite differences.
    /// </summary>
    private static Vector3D ComputeTangent(ImmutableArray<ImmutableArray<Point3D>> profiles, int profileIndex, int pointIndex, double[] profileParams)
    {
        int count = profiles.Length;

        if (count == 2)
        {
            Point3D a = profiles[0][pointIndex];
            Point3D b = profiles[1][pointIndex];
            return new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        }

        if (profileIndex == 0)
        {
            Point3D a = profiles[0][pointIndex];
            Point3D b = profiles[1][pointIndex];
            double dt = profileParams[1] - profileParams[0];
            if (System.Math.Abs(dt) < 1e-15) dt = 1.0;
            return new Vector3D((b.X - a.X) / dt, (b.Y - a.Y) / dt, (b.Z - a.Z) / dt);
        }

        if (profileIndex == count - 1)
        {
            Point3D a = profiles[count - 2][pointIndex];
            Point3D b = profiles[count - 1][pointIndex];
            double dt = profileParams[count - 1] - profileParams[count - 2];
            if (System.Math.Abs(dt) < 1e-15) dt = 1.0;
            return new Vector3D((b.X - a.X) / dt, (b.Y - a.Y) / dt, (b.Z - a.Z) / dt);
        }

        Point3D prev = profiles[profileIndex - 1][pointIndex];
        Point3D next = profiles[profileIndex + 1][pointIndex];
        double dtp = profileParams[profileIndex + 1] - profileParams[profileIndex - 1];
        if (System.Math.Abs(dtp) < 1e-15) dtp = 1.0;
        return new Vector3D(
            (next.X - prev.X) / dtp,
            (next.Y - prev.Y) / dtp,
            (next.Z - prev.Z) / dtp);
    }

    /// <summary>
    /// Computes an approximate surface normal at a given parametric location on the loft.
    /// </summary>
    private static Vector3D ComputeNormal(
        ImmutableArray<ImmutableArray<Point3D>> profiles, int segment, int pointIndex,
        double t, double[] profileParams, int uPointCount)
    {
        const double Epsilon = 1e-10;

        double vEps = 0.001;
        double v0 = System.Math.Max(0.0, t - vEps);
        double v1 = System.Math.Min(1.0, t + vEps);
        double hv = v1 - v0;
        if (hv < Epsilon) hv = Epsilon;

        Point3D p0 = HermiteInterpolate(
            profiles[segment][pointIndex],
            profiles[segment + 1][pointIndex],
            ComputeTangent(profiles, segment, pointIndex, profileParams),
            ComputeTangent(profiles, segment + 1, pointIndex, profileParams),
            v0);
        Point3D p1 = HermiteInterpolate(
            profiles[segment][pointIndex],
            profiles[segment + 1][pointIndex],
            ComputeTangent(profiles, segment, pointIndex, profileParams),
            ComputeTangent(profiles, segment + 1, pointIndex, profileParams),
            v1);

        Vector3D dV = new Vector3D((p1.X - p0.X) / hv, (p1.Y - p0.Y) / hv, (p1.Z - p0.Z) / hv);

        Vector3D dU;
        if (pointIndex > 0 && pointIndex < uPointCount - 1)
        {
            Point3D a = HermiteInterpolate(
                profiles[segment][pointIndex - 1], profiles[segment + 1][pointIndex - 1],
                ComputeTangent(profiles, segment, pointIndex - 1, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex - 1, profileParams), t);
            Point3D b = HermiteInterpolate(
                profiles[segment][pointIndex + 1], profiles[segment + 1][pointIndex + 1],
                ComputeTangent(profiles, segment, pointIndex + 1, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex + 1, profileParams), t);
            dU = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        }
        else if (pointIndex == 0 && uPointCount > 1)
        {
            Point3D a = HermiteInterpolate(
                profiles[segment][pointIndex], profiles[segment + 1][pointIndex],
                ComputeTangent(profiles, segment, pointIndex, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex, profileParams), t);
            Point3D b = HermiteInterpolate(
                profiles[segment][pointIndex + 1], profiles[segment + 1][pointIndex + 1],
                ComputeTangent(profiles, segment, pointIndex + 1, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex + 1, profileParams), t);
            dU = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        }
        else
        {
            Point3D a = HermiteInterpolate(
                profiles[segment][pointIndex - 1], profiles[segment + 1][pointIndex - 1],
                ComputeTangent(profiles, segment, pointIndex - 1, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex - 1, profileParams), t);
            Point3D b = HermiteInterpolate(
                profiles[segment][pointIndex], profiles[segment + 1][pointIndex],
                ComputeTangent(profiles, segment, pointIndex, profileParams),
                ComputeTangent(profiles, segment + 1, pointIndex, profileParams), t);
            dU = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        }

        Vector3D cross = new Vector3D(
            dU.Y * dV.Z - dU.Z * dV.Y,
            dU.Z * dV.X - dU.X * dV.Z,
            dU.X * dV.Y - dU.Y * dV.X);

        double length = System.Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);
        if (length < Epsilon)
            return new Vector3D(0, 0, 1);

        return new Vector3D(cross.X / length, cross.Y / length, cross.Z / length);
    }

    /// <summary>
    /// Clamps a value to the range [0, 1].
    /// </summary>
    private static double Clamp01(double value)
    {
        if (value < 0.0) return 0.0;
        if (value > 1.0) return 1.0;
        return value;
    }
}
