namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a circle lying on a plane in 3D space.</summary>
public readonly record struct CircleOnPlane(Point3D Center, Vector3D Normal, double Radius)
{
    /// <summary>The center of the circle.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The unit normal of the plane the circle lies on.</summary>
    public Vector3D Normal { get; } = Normal;

    /// <summary>The radius of the circle.</summary>
    public double Radius { get; } = Radius;

    /// <summary>Gets the circumference of the circle.</summary>
    public double Circumference => 2.0 * System.Math.PI * Radius;

    /// <summary>Gets the area of the circle.</summary>
    public double Area => System.Math.PI * Radius * Radius;

    /// <inheritdoc/>
    public override string ToString() => $"CircleOnPlane(Center={Center}, Normal={Normal}, Radius={Radius})";
}
