namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Base abstract record for all 2D geometry types.</summary>
public abstract record Geometry2D
{
    /// <summary>Gets the axis-aligned bounding box of this geometry.</summary>
    public abstract BoundingBox2D BoundingBox { get; }

    /// <summary>Gets the area of this geometry.</summary>
    public abstract double Area { get; }

    /// <summary>Determines whether this geometry contains the specified point.</summary>
    /// <param name="point">The point to test.</param>
    /// <returns><c>true</c> if the geometry contains the point; otherwise, <c>false</c>.</returns>
    public abstract bool Contains(Point2D point);
}
