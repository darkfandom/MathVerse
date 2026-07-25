using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents an oriented 3D bounding box defined by a center, three orthonormal axes, and three half-extents.</summary>
public readonly record struct OBB3D(Point3D Center, Vector3D AxisX, Vector3D AxisY, Vector3D AxisZ, double ExtentX, double ExtentY, double ExtentZ)
{
    /// <summary>The center of the OBB.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The first orthonormal axis.</summary>
    public Vector3D AxisX { get; } = AxisX.Normalize();

    /// <summary>The second orthonormal axis.</summary>
    public Vector3D AxisY { get; } = AxisY.Normalize();

    /// <summary>The third orthonormal axis.</summary>
    public Vector3D AxisZ { get; } = AxisZ.Normalize();

    /// <summary>Half-extent along the X axis.</summary>
    public double ExtentX { get; } = ExtentX;

    /// <summary>Half-extent along the Y axis.</summary>
    public double ExtentY { get; } = ExtentY;

    /// <summary>Half-extent along the Z axis.</summary>
    public double ExtentZ { get; } = ExtentZ;

    /// <summary>Gets the volume of the OBB.</summary>
    public double Volume => 8.0 * ExtentX * ExtentY * ExtentZ;

    /// <summary>Gets the surface area of the OBB.</summary>
    public double SurfaceArea =>
        8.0 * (ExtentX * ExtentY + ExtentY * ExtentZ + ExtentZ * ExtentX);

    /// <summary>Creates an OBB from an AABB.</summary>
    public static OBB3D FromAABB(BoundingBox3D aabb) => new(
        aabb.Center,
        Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ,
        aabb.Width * 0.5, aabb.Height * 0.5, aabb.Depth * 0.5);

    /// <summary>Creates an OBB from a set of points using PCA approximation.</summary>
    public static OBB3D FromPoints(IReadOnlyList<Point3D> points)
    {
        if (points.Count == 0) return new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 0, 0, 0);

        Point3D centroid = BoundingBox3D.FromPoints(points).Center;

        double sumXX = 0, sumXY = 0, sumXZ = 0;
        double sumYY = 0, sumYZ = 0, sumZZ = 0;

        for (int i = 0; i < points.Count; i++)
        {
            double dx = points[i].X - centroid.X;
            double dy = points[i].Y - centroid.Y;
            double dz = points[i].Z - centroid.Z;
            sumXX += dx * dx; sumXY += dx * dy; sumXZ += dx * dz;
            sumYY += dy * dy; sumYZ += dy * dz; sumZZ += dz * dz;
        }

        double n = points.Count;
        Vector3D ax = new Vector3D(sumXX / n, sumXY / n, sumXZ / n).Normalize();
        Vector3D ay = new Vector3D(sumXY / n, sumYY / n, sumYZ / n).Normalize();
        Vector3D az = ax.Cross(ay).Normalize();
        ay = az.Cross(ax).Normalize();

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        double minZ = double.MaxValue, maxZ = double.MinValue;

        for (int i = 0; i < points.Count; i++)
        {
            double dx = points[i].X - centroid.X;
            double dy = points[i].Y - centroid.Y;
            double dz = points[i].Z - centroid.Z;
            double px = dx * ax.X + dy * ax.Y + dz * ax.Z;
            double py = dx * ay.X + dy * ay.Y + dz * ay.Z;
            double pz = dx * az.X + dy * az.Y + dz * az.Z;
            if (px < minX) minX = px; if (px > maxX) maxX = px;
            if (py < minY) minY = py; if (py > maxY) maxY = py;
            if (pz < minZ) minZ = pz; if (pz > maxZ) maxZ = pz;
        }

        return new OBB3D(centroid, ax, ay, az,
            (maxX - minX) * 0.5, (maxY - minY) * 0.5, (maxZ - minZ) * 0.5);
    }

    /// <summary>Gets the 8 corners of the OBB.</summary>
    public ImmutableArray<Point3D> Corners
    {
        get
        {
            double cx = Center.X, cy = Center.Y, cz = Center.Z;
            var corners = ImmutableArray.CreateBuilder<Point3D>(8);
            for (int i = 0; i < 8; i++)
            {
                double sx = (i & 1) == 0 ? -ExtentX : ExtentX;
                double sy = (i & 2) == 0 ? -ExtentY : ExtentY;
                double sz = (i & 4) == 0 ? -ExtentZ : ExtentZ;
                corners.Add(new Point3D(
                    cx + sx * AxisX.X + sy * AxisY.X + sz * AxisZ.X,
                    cy + sx * AxisX.Y + sy * AxisY.Y + sz * AxisZ.Y,
                    cz + sx * AxisX.Z + sy * AxisY.Z + sz * AxisZ.Z));
            }
            return corners.ToImmutable();
        }
    }

    /// <summary>Tests whether a point is inside the OBB.</summary>
    public bool Contains(Point3D p)
    {
        Vector3D d = new(p.X - Center.X, p.Y - Center.Y, p.Z - Center.Z);
        return System.Math.Abs(d.Dot(AxisX)) <= ExtentX &&
               System.Math.Abs(d.Dot(AxisY)) <= ExtentY &&
               System.Math.Abs(d.Dot(AxisZ)) <= ExtentZ;
    }

    /// <summary>Tests whether this OBB intersects another OBB using the separating axis theorem.</summary>
    public bool Intersects(OBB3D other)
    {
        Vector3D[] axes = new Vector3D[15];
        axes[0] = AxisX; axes[1] = AxisY; axes[2] = AxisZ;
        axes[3] = other.AxisX; axes[4] = other.AxisY; axes[5] = other.AxisZ;

        Vector3D d = new(other.Center.X - Center.X, other.Center.Y - Center.Y, other.Center.Z - Center.Z);

        int idx = 6;
        for (int i = 0; i < 3; i++)
        {
            Vector3D a = i == 0 ? AxisX : i == 1 ? AxisY : AxisZ;
            for (int j = 0; j < 3; j++)
            {
                Vector3D b = j == 0 ? other.AxisX : j == 1 ? other.AxisY : other.AxisZ;
                axes[idx++] = a.Cross(b);
            }
        }

        for (int i = 0; i < 15; i++)
        {
            Vector3D axis = axes[i];
            double dProj = System.Math.Abs(d.Dot(axis));

            double rA = ExtentX * System.Math.Abs(axis.Dot(AxisX)) +
                        ExtentY * System.Math.Abs(axis.Dot(AxisY)) +
                        ExtentZ * System.Math.Abs(axis.Dot(AxisZ));
            double rB = other.ExtentX * System.Math.Abs(axis.Dot(other.AxisX)) +
                        other.ExtentY * System.Math.Abs(axis.Dot(other.AxisY)) +
                        other.ExtentZ * System.Math.Abs(axis.Dot(other.AxisZ));

            if (dProj > rA + rB) return false;
        }
        return true;
    }

    /// <summary>Converts this OBB to an AABB.</summary>
    public BoundingBox3D ToAABB()
    {
        ImmutableArray<Point3D> corners = Corners;
        Point3D min = corners[0], max = corners[0];
        for (int i = 1; i < corners.Length; i++)
        {
            min = new Point3D(
                System.Math.Min(min.X, corners[i].X),
                System.Math.Min(min.Y, corners[i].Y),
                System.Math.Min(min.Z, corners[i].Z));
            max = new Point3D(
                System.Math.Max(max.X, corners[i].X),
                System.Math.Max(max.Y, corners[i].Y),
                System.Math.Max(max.Z, corners[i].Z));
        }
        return new BoundingBox3D(min, max);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"OBB3D(Center={Center}, Extents=({ExtentX:F4}, {ExtentY:F4}, {ExtentZ:F4}))";
}
