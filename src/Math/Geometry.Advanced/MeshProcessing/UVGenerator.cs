using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Represents a 2D UV texture coordinate.</summary>
/// <param name="U">The U (horizontal) texture coordinate.</param>
/// <param name="V">The V (vertical) texture coordinate.</param>
public readonly record struct UVCoordinate(double U, double V);

/// <summary>Provides methods for generating UV texture coordinates for 3D meshes using various projection techniques.</summary>
public static class UVGenerator
{
    private const double Tolerance = 1e-10;
    private const double Pi = System.Math.PI;

    /// <summary>Generates UV coordinates using planar projection along the dominant axis of the given normal.</summary>
    /// <param name="vertices">The vertex positions to project.</param>
    /// <param name="normal">The projection plane normal direction.</param>
    /// <returns>An immutable array of UV coordinates for each vertex.</returns>
    public static ImmutableArray<UVCoordinate> PlanarProjection(ImmutableArray<Point3D> vertices, Vector3D normal)
    {
        Vector3D n = normal.Normalize();
        double ax = System.Math.Abs(n.X), ay = System.Math.Abs(n.Y), az = System.Math.Abs(n.Z);
        Vector3D u, v;
        if (ax >= ay && ax >= az)
        {
            u = Vector3D.UnitY;
            v = Vector3D.UnitZ;
        }
        else if (ay >= ax && ay >= az)
        {
            u = Vector3D.UnitX;
            v = Vector3D.UnitZ;
        }
        else
        {
            u = Vector3D.UnitX;
            v = Vector3D.UnitY;
        }
        return ProjectUV(vertices, u, v);
    }

    /// <summary>Generates UV coordinates using spherical projection centered at the given point.</summary>
    /// <param name="vertices">The vertex positions to project.</param>
    /// <param name="center">The center point of the sphere.</param>
    /// <returns>An immutable array of UV coordinates for each vertex.</returns>
    public static ImmutableArray<UVCoordinate> SphericalProjection(ImmutableArray<Point3D> vertices, Point3D center)
    {
        var result = ImmutableArray.CreateBuilder<UVCoordinate>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            double dx = vertices[i].X - center.X;
            double dy = vertices[i].Y - center.Y;
            double dz = vertices[i].Z - center.Z;
            double r = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (r < Tolerance)
            {
                result.Add(new UVCoordinate(0.5, 0.5));
                continue;
            }
            double theta = System.Math.Atan2(dz, dx);
            double phi = System.Math.Acos(System.Math.Max(-1.0, System.Math.Min(1.0, dy / r)));
            double u = (theta + Pi) / (2.0 * Pi);
            double v = phi / Pi;
            result.Add(new UVCoordinate(u, v));
        }
        return result.ToImmutable();
    }

    /// <summary>Generates UV coordinates using cylindrical projection along the given axis from the given origin.</summary>
    /// <param name="vertices">The vertex positions to project.</param>
    /// <param name="axis">The cylinder axis direction.</param>
    /// <param name="origin">The origin point on the cylinder axis.</param>
    /// <returns>An immutable array of UV coordinates for each vertex.</returns>
    public static ImmutableArray<UVCoordinate> CylindricalProjection(ImmutableArray<Point3D> vertices, Vector3D axis, Point3D origin)
    {
        Vector3D a = axis.Normalize();
        double ax = System.Math.Abs(a.X), ay = System.Math.Abs(a.Y), az = System.Math.Abs(a.Z);
        Vector3D perpU;
        if (ax <= ay && ax <= az)
            perpU = Vector3D.UnitX;
        else if (ay <= ax && ay <= az)
            perpU = Vector3D.UnitY;
        else
            perpU = Vector3D.UnitZ;
        Vector3D u = a.Cross(perpU).Normalize();
        Vector3D v = a.Cross(u).Normalize();

        var result = ImmutableArray.CreateBuilder<UVCoordinate>(vertices.Length);
        double minY = double.MaxValue, maxY = double.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3D diff = vertices[i].ToVector3D().Subtract(origin.ToVector3D());
            double h = diff.Dot(a);
            if (h < minY) minY = h;
            if (h > maxY) maxY = h;
        }
        double range = maxY - minY;
        if (range < Tolerance) range = 1.0;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3D diff = vertices[i].ToVector3D().Subtract(origin.ToVector3D());
            double h = diff.Dot(a);
            double px = diff.Dot(u);
            double pz = diff.Dot(v);
            double theta = System.Math.Atan2(pz, px);
            double uv = (theta + Pi) / (2.0 * Pi);
            double vv = (h - minY) / range;
            result.Add(new UVCoordinate(uv, vv));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<UVCoordinate> ProjectUV(ImmutableArray<Point3D> vertices, Vector3D u, Vector3D v)
    {
        double minU = double.MaxValue, maxU = double.MinValue;
        double minV = double.MaxValue, maxV = double.MinValue;
        var projected = new (double U, double V)[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            double pu = vertices[i].X * u.X + vertices[i].Y * u.Y + vertices[i].Z * u.Z;
            double pv = vertices[i].X * v.X + vertices[i].Y * v.Y + vertices[i].Z * v.Z;
            projected[i] = (pu, pv);
            if (pu < minU) minU = pu;
            if (pu > maxU) maxU = pu;
            if (pv < minV) minV = pv;
            if (pv > maxV) maxV = pv;
        }
        double rangeU = maxU - minU;
        double rangeV = maxV - minV;
        if (rangeU < Tolerance) rangeU = 1.0;
        if (rangeV < Tolerance) rangeV = 1.0;
        var result = ImmutableArray.CreateBuilder<UVCoordinate>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            result.Add(new UVCoordinate(
                (projected[i].U - minU) / rangeU,
                (projected[i].V - minV) / rangeV));
        }
        return result.ToImmutable();
    }
}
