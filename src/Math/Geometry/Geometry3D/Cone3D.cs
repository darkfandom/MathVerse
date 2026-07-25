namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D cone defined by an apex, axis direction, base radius, and height.</summary>
public readonly record struct Cone3D(Point3D Apex, Vector3D Axis, double Radius, double Height)
{
    /// <summary>The apex (tip) of the cone.</summary>
    public Point3D Apex { get; } = Apex;

    /// <summary>The axis direction from apex toward the base center.</summary>
    public Vector3D Axis { get; } = Axis;

    /// <summary>The radius of the base circle.</summary>
    public double Radius { get; } = Radius;

    /// <summary>The height from apex to base center along the axis.</summary>
    public double Height { get; } = Height;

    /// <summary>Gets the slant height of the cone.</summary>
    public double SlantHeight => System.Math.Sqrt(Radius * Radius + Height * Height);

    /// <summary>Gets the volume of the cone.</summary>
    public double Volume => (1.0 / 3.0) * System.Math.PI * Radius * Radius * Height;

    /// <summary>Gets the total surface area (lateral surface + base).</summary>
    public double SurfaceArea =>
        System.Math.PI * Radius * (Radius + SlantHeight);

    /// <summary>Computes an axis-aligned bounding box enclosing this cone.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox()
    {
        Vector3D axisNorm = Axis.Normalize();
        Point3D baseCenter = new(
            Apex.X + Height * axisNorm.X,
            Apex.Y + Height * axisNorm.Y,
            Apex.Z + Height * axisNorm.Z);

        double ex = Radius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.X * axisNorm.X));
        double ey = Radius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Y * axisNorm.Y));
        double ez = Radius * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - axisNorm.Z * axisNorm.Z));

        Point3D baseMin = new(baseCenter.X - ex, baseCenter.Y - ey, baseCenter.Z - ez);
        Point3D baseMax = new(baseCenter.X + ex, baseCenter.Y + ey, baseCenter.Z + ez);

        return new BoundingBox3D(
            new Point3D(
                System.Math.Min(Apex.X, baseMin.X),
                System.Math.Min(Apex.Y, baseMin.Y),
                System.Math.Min(Apex.Z, baseMin.Z)),
            new Point3D(
                System.Math.Max(Apex.X, baseMax.X),
                System.Math.Max(Apex.Y, baseMax.Y),
                System.Math.Max(Apex.Z, baseMax.Z)));
    }

    /// <inheritdoc/>
    public override string ToString() => $"Cone3D(Apex={Apex}, Axis={Axis}, Radius={Radius}, Height={Height})";
}
