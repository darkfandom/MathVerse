namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a truncated cone (frustum) defined by near/far distances and radii along an axis direction.</summary>
public readonly record struct Frustum3D(double NearDistance, double FarDistance, double NearRadius, double FarRadius, Vector3D Axis)
{
    /// <summary>The distance from the origin to the near face along the axis.</summary>
    public double NearDistance { get; } = NearDistance;

    /// <summary>The distance from the origin to the far face along the axis.</summary>
    public double FarDistance { get; } = FarDistance;

    /// <summary>The radius of the near face.</summary>
    public double NearRadius { get; } = NearRadius;

    /// <summary>The radius of the far face.</summary>
    public double FarRadius { get; } = FarRadius;

    /// <summary>The axis direction of the frustum.</summary>
    public Vector3D Axis { get; } = Axis;

    /// <summary>Gets the height (distance between near and far faces).</summary>
    public double Height => System.Math.Abs(FarDistance - NearDistance);

    /// <summary>Gets the volume of the frustum.</summary>
    public double Volume
    {
        get
        {
            double h = Height;
            double r1 = NearRadius;
            double r2 = FarRadius;
            return System.Math.PI * h * (r1 * r1 + r1 * r2 + r2 * r2) / 3.0;
        }
    }

    /// <summary>Gets the total surface area (lateral + two circular faces).</summary>
    public double SurfaceArea
    {
        get
        {
            double h = Height;
            double r1 = NearRadius;
            double r2 = FarRadius;
            double slantHeight = System.Math.Sqrt(h * h + (r2 - r1) * (r2 - r1));
            return System.Math.PI * (r1 + r2) * slantHeight +
                   System.Math.PI * r1 * r1 +
                   System.Math.PI * r2 * r2;
        }
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this frustum.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox()
    {
        Vector3D axisNorm = Axis.Normalize();
        Point3D nearCenter = new(axisNorm.X * NearDistance, axisNorm.Y * NearDistance, axisNorm.Z * NearDistance);
        Point3D farCenter = new(axisNorm.X * FarDistance, axisNorm.Y * FarDistance, axisNorm.Z * FarDistance);

        double nxExtent = NearRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.X * axisNorm.X));
        double nyExtent = NearRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Y * axisNorm.Y));
        double nzExtent = NearRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Z * axisNorm.Z));

        double fxExtent = FarRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.X * axisNorm.X));
        double fyExtent = FarRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Y * axisNorm.Y));
        double fzExtent = FarRadius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Z * axisNorm.Z));

        return new BoundingBox3D(
            new Point3D(
                System.Math.Min(nearCenter.X - nxExtent, farCenter.X - fxExtent),
                System.Math.Min(nearCenter.Y - nyExtent, farCenter.Y - fyExtent),
                System.Math.Min(nearCenter.Z - nzExtent, farCenter.Z - fzExtent)),
            new Point3D(
                System.Math.Max(nearCenter.X + nxExtent, farCenter.X + fxExtent),
                System.Math.Max(nearCenter.Y + nyExtent, farCenter.Y + fyExtent),
                System.Math.Max(nearCenter.Z + nzExtent, farCenter.Z + fzExtent)));
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Frustum3D(NearDist={NearDistance}, FarDist={FarDistance}, NearR={NearRadius}, FarR={FarRadius}, Axis={Axis})";
}
