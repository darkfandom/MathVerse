namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents an axis-aligned cylinder defined by a center, radius, and height along the Y axis.</summary>
public readonly record struct Cylinder3D(Point3D Center, double Radius, double Height)
{
    /// <summary>The center of the cylinder.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The radius of the cylinder.</summary>
    public double Radius { get; } = Radius;

    /// <summary>The height of the cylinder.</summary>
    public double Height { get; } = Height;

    /// <summary>Gets the volume of the cylinder.</summary>
    public double Volume => System.Math.PI * Radius * Radius * Height;

    /// <summary>Gets the total surface area (lateral + two caps).</summary>
    public double SurfaceArea =>
        2.0 * System.Math.PI * Radius * Height +
        2.0 * System.Math.PI * Radius * Radius;

    /// <summary>Returns a point on the cylinder surface at the given normalized height and angle.</summary>
    /// <param name="t">Normalized height parameter in [0, 1], where 0 is the bottom and 1 is the top.</param>
    /// <param name="angle">The angle in radians around the cylinder axis.</param>
    /// <returns>A point on the cylinder surface.</returns>
    public Point3D PointAt(double t, double angle)
    {
        double halfH = Height * 0.5;
        double y = Center.Y - halfH + Height * t;
        return new Point3D(
            Center.X + Radius * System.Math.Cos(angle),
            y,
            Center.Z + Radius * System.Math.Sin(angle));
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this cylinder.</summary>
    /// <returns>The bounding box.</returns>
    public BoundingBox3D ToBoundingBox()
    {
        double halfH = Height * 0.5;
        return new BoundingBox3D(
            new Point3D(Center.X - Radius, Center.Y - halfH, Center.Z - Radius),
            new Point3D(Center.X + Radius, Center.Y + halfH, Center.Z + Radius));
    }

    /// <inheritdoc/>
    public override string ToString() => $"Cylinder3D(Center={Center}, Radius={Radius}, Height={Height})";
}
