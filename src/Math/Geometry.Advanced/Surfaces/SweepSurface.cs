using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Provides methods for generating sweep surfaces by transforming a profile curve along a 3D path using rotation-minimizing frames.
/// </summary>
public static class SweepSurface
{
    /// <summary>
    /// Sweeps a profile curve along a path curve, generating a surface mesh. At each path point, a local coordinate frame
    /// is constructed using the path tangent and an up vector, and the profile is transformed into that frame. Rotation-minimizing
    /// frames are used to avoid artificial twisting when the path curves.
    /// </summary>
    /// <param name="profile">The profile curve to sweep, defined as an array of 3D points in the local xy-plane (z = 0 for best results).</param>
    /// <param name="path">The path curve along which to sweep the profile, defined as an array of 3D points.</param>
    /// <param name="up">The approximate up vector used to initialize the first frame. Should not be parallel to the first path tangent.</param>
    /// <returns>An immutable array of <see cref="SurfacePoint"/> representing the swept surface in row-major order (profile varies fastest).</returns>
    /// <exception cref="ArgumentException">Thrown when the profile or path has fewer than 2 points, or when the up vector is parallel to the path tangent.</exception>
    public static ImmutableArray<SurfacePoint> Generate(ImmutableArray<Point3D> profile, ImmutableArray<Point3D> path, Vector3D up)
    {
        if (profile.Length < 2)
            throw new ArgumentException("Profile must have at least 2 points.", nameof(profile));
        if (path.Length < 2)
            throw new ArgumentException("Path must have at least 2 points.", nameof(path));

        int profileCount = profile.Length;
        int pathCount = path.Length;

        Vector3D[] tangents = ComputePathTangents(path);
        Vector3D[] normals = ComputeRotationMinimizingFrames(tangents, up);

        var builder = ImmutableArray.CreateBuilder<SurfacePoint>(profileCount * pathCount);

        for (int j = 0; j < pathCount; j++)
        {
            Point3D origin = path[j];
            Vector3D T = tangents[j];
            Vector3D N = normals[j];
            Vector3D B = Cross(T, N);
            double bLen = System.Math.Sqrt(B.X * B.X + B.Y * B.Y + B.Z * B.Z);
            if (bLen > 1e-15)
            {
                B = new Vector3D(B.X / bLen, B.Y / bLen, B.Z / bLen);
            }

            for (int i = 0; i < profileCount; i++)
            {
                double px = profile[i].X;
                double py = profile[i].Y;
                double pz = profile[i].Z;

                double worldX = origin.X + px * N.X + py * B.X + pz * T.X;
                double worldY = origin.Y + px * N.Y + py * B.Y + pz * T.Y;
                double worldZ = origin.Z + px * N.Z + py * B.Z + pz * T.Z;

                Vector3D normal = ComputeSurfaceNormal(profile, path, tangents, normals, i, j, profileCount, pathCount);
                builder.Add(new SurfacePoint(new Point3D(worldX, worldY, worldZ), normal));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Computes tangent vectors at each path point using central finite differences.
    /// </summary>
    private static Vector3D[] ComputePathTangents(ImmutableArray<Point3D> path)
    {
        int count = path.Length;
        var tangents = new Vector3D[count];

        for (int i = 0; i < count; i++)
        {
            Vector3D tangent;
            if (i == 0)
            {
                tangent = new Vector3D(
                    path[1].X - path[0].X,
                    path[1].Y - path[0].Y,
                    path[1].Z - path[0].Z);
            }
            else if (i == count - 1)
            {
                tangent = new Vector3D(
                    path[count - 1].X - path[count - 2].X,
                    path[count - 1].Y - path[count - 2].Y,
                    path[count - 1].Z - path[count - 2].Z);
            }
            else
            {
                tangent = new Vector3D(
                    path[i + 1].X - path[i - 1].X,
                    path[i + 1].Y - path[i - 1].Y,
                    path[i + 1].Z - path[i - 1].Z);
            }

            double len = System.Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y + tangent.Z * tangent.Z);
            if (len > 1e-15)
                tangent = new Vector3D(tangent.X / len, tangent.Y / len, tangent.Z / len);

            tangents[i] = tangent;
        }

        return tangents;
    }

    /// <summary>
    /// Computes rotation-minimizing frames along the path to avoid artificial twisting.
    /// Uses the double-reflection method for rotation minimization.
    /// </summary>
    private static Vector3D[] ComputeRotationMinimizingFrames(Vector3D[] tangents, Vector3D up)
    {
        int count = tangents.Length;
        var normals = new Vector3D[count];

        Vector3D T0 = tangents[0];
        Vector3D N0 = Cross(up, T0);
        double n0Len = System.Math.Sqrt(N0.X * N0.X + N0.Y * N0.Y + N0.Z * N0.Z);

        if (n0Len < 1e-15)
        {
            Vector3D arbitrary = System.Math.Abs(T0.X) < 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);
            N0 = Cross(arbitrary, T0);
            n0Len = System.Math.Sqrt(N0.X * N0.X + N0.Y * N0.Y + N0.Z * N0.Z);
        }

        N0 = new Vector3D(N0.X / n0Len, N0.Y / n0Len, N0.Z / n0Len);
        normals[0] = N0;

        for (int i = 1; i < count; i++)
        {
            Vector3D Tprev = tangents[i - 1];
            Vector3D Tcurr = tangents[i];
            Vector3D Nprev = normals[i - 1];

            double dotT = Tprev.X * Tcurr.X + Tprev.Y * Tcurr.Y + Tprev.Z * Tcurr.Z;
            double alpha = 1.0 / (1.0 + System.Math.Max(dotT, -1.0 + 1e-15));

            Vector3D r1 = new Vector3D(
                Tcurr.X - Tprev.X,
                Tcurr.Y - Tprev.Y,
                Tcurr.Z - Tprev.Z);

            Vector3D Nrefl = new Vector3D(
                Nprev.X - alpha * (r1.X * Nprev.X + r1.Y * Nprev.Y + r1.Z * Nprev.Z) * r1.X,
                Nprev.Y - alpha * (r1.X * Nprev.X + r1.Y * Nprev.Y + r1.Z * Nprev.Z) * r1.Y,
                Nprev.Z - alpha * (r1.X * Nprev.X + r1.Y * Nprev.Y + r1.Z * Nprev.Z) * r1.Z);

            Vector3D r2 = new Vector3D(
                Tcurr.X - Tprev.X,
                Tcurr.Y - Tprev.Y,
                Tcurr.Z - Tprev.Z);

            Vector3D Nfinal = new Vector3D(
                Nrefl.X - alpha * (r2.X * Nrefl.X + r2.Y * Nrefl.Y + r2.Z * Nrefl.Z) * r2.X,
                Nrefl.Y - alpha * (r2.X * Nrefl.X + r2.Y * Nrefl.Y + r2.Z * Nrefl.Z) * r2.Y,
                Nrefl.Z - alpha * (r2.X * Nrefl.X + r2.Y * Nrefl.Y + r2.Z * Nrefl.Z) * r2.Z);

            double nLen = System.Math.Sqrt(Nfinal.X * Nfinal.X + Nfinal.Y * Nfinal.Y + Nfinal.Z * Nfinal.Z);
            if (nLen < 1e-15)
            {
                normals[i] = normals[i - 1];
            }
            else
            {
                normals[i] = new Vector3D(Nfinal.X / nLen, Nfinal.Y / nLen, Nfinal.Z / nLen);
            }
        }

        return normals;
    }

    /// <summary>
    /// Computes the surface normal at a profile/path intersection using finite differences in both parametric directions.
    /// </summary>
    private static Vector3D ComputeSurfaceNormal(
        ImmutableArray<Point3D> profile, ImmutableArray<Point3D> path, Vector3D[] tangents, Vector3D[] normals,
        int profileIndex, int pathIndex, int profileCount, int pathCount)
    {
        const double Eps = 1e-10;

        Vector3D dU = ComputeDerivativeU(profile, tangents, normals, profileIndex, pathIndex);
        Vector3D dV = ComputeDerivativeV(profile, path, tangents, normals, profileIndex, pathIndex, profileCount, pathCount);

        Vector3D cross = new Vector3D(
            dU.Y * dV.Z - dU.Z * dV.Y,
            dU.Z * dV.X - dU.X * dV.Z,
            dU.X * dV.Y - dU.Y * dV.X);

        double len = System.Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);
        if (len < Eps)
            return new Vector3D(0, 0, 1);

        return new Vector3D(cross.X / len, cross.Y / len, cross.Z / len);
    }

    /// <summary>
    /// Computes the derivative of the swept surface with respect to the profile parameter.
    /// </summary>
    private static Vector3D ComputeDerivativeU(
        ImmutableArray<Point3D> profile, Vector3D[] tangents, Vector3D[] normals,
        int profileIndex, int pathIndex)
    {
        Vector3D T = tangents[pathIndex];
        Vector3D N = normals[pathIndex];
        Vector3D B = Cross(T, N);

        if (profileIndex < profile.Length - 1)
        {
            double dx = profile[profileIndex + 1].X - profile[profileIndex].X;
            double dy = profile[profileIndex + 1].Y - profile[profileIndex].Y;
            return new Vector3D(dx * N.X + dy * B.X, dx * N.Y + dy * B.Y, dx * N.Z + dy * B.Z);
        }
        else
        {
            double dx = profile[profileIndex].X - profile[profileIndex - 1].X;
            double dy = profile[profileIndex].Y - profile[profileIndex - 1].Y;
            return new Vector3D(dx * N.X + dy * B.X, dx * N.Y + dy * B.Y, dx * N.Z + dy * B.Z);
        }
    }

    /// <summary>
    /// Computes the derivative of the swept surface with respect to the path parameter.
    /// </summary>
    private static Vector3D ComputeDerivativeV(
        ImmutableArray<Point3D> profile, ImmutableArray<Point3D> path, Vector3D[] tangents, Vector3D[] normals,
        int profileIndex, int pathIndex, int profileCount, int pathCount)
    {
        double px = profile[profileIndex].X;
        double py = profile[profileIndex].Y;

        int j0, j1;
        double sign;
        if (pathIndex < pathCount - 1)
        {
            j0 = pathIndex;
            j1 = pathIndex + 1;
            sign = 1.0;
        }
        else
        {
            j0 = pathIndex - 1;
            j1 = pathIndex;
            sign = 1.0;
        }

        Vector3D N0 = normals[j0];
        Vector3D N1 = normals[j1];
        Vector3D T0 = tangents[j0];
        Vector3D T1 = tangents[j1];
        Vector3D B0 = Cross(T0, N0);
        Vector3D B1 = Cross(T1, N1);

        double x0 = px * N0.X + py * B0.X;
        double y0 = px * N0.Y + py * B0.Y;
        double z0 = px * N0.Z + py * B0.Z;

        double x1 = px * N1.X + py * B1.X;
        double y1 = px * N1.Y + py * B1.Y;
        double z1 = px * N1.Z + py * B1.Z;

        double len01 = System.Math.Sqrt(
            (path[j1].X - path[j0].X) * (path[j1].X - path[j0].X) +
            (path[j1].Y - path[j0].Y) * (path[j1].Y - path[j0].Y) +
            (path[j1].Z - path[j0].Z) * (path[j1].Z - path[j0].Z));

        if (len01 < 1e-15) len01 = 1.0;

        double dpx = (x1 - x0) / len01;
        double dpy = (y1 - y0) / len01;
        double dpz = (z1 - z0) / len01;

        return new Vector3D(dpx * sign, dpy * sign, dpz * sign);
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
