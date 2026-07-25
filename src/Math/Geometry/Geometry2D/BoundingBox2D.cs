using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents an axis-aligned bounding box in 2D.</summary>
public readonly record struct BoundingBox2D(Point2D Min, Point2D Max)
{
    /// <summary>The minimum corner.</summary>
    public Point2D Min { get; } = Min;

    /// <summary>The maximum corner.</summary>
    public Point2D Max { get; } = Max;

    /// <summary>Gets the width of the bounding box.</summary>
    public double Width => Max.X - Min.X;

    /// <summary>Gets the height of the bounding box.</summary>
    public double Height => Max.Y - Min.Y;

    /// <summary>Gets the center of the bounding box.</summary>
    public Point2D Center => new((Min.X + Max.X) * 0.5, (Min.Y + Max.Y) * 0.5);

    /// <summary>Gets the area of the bounding box.</summary>
    public double Area => Width * Height;

    /// <summary>Determines whether the bounding box contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the point is inside the bounding box; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p) => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;

    /// <summary>Determines whether this bounding box fully contains another bounding box.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns><c>true</c> if this bounding box contains the other; otherwise, <c>false</c>.</returns>
    public bool Contains(BoundingBox2D other) => other.Min.X >= Min.X && other.Max.X <= Max.X && other.Min.Y >= Min.Y && other.Max.Y <= Max.Y;

    /// <summary>Determines whether this bounding box intersects another bounding box.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns><c>true</c> if the bounding boxes intersect; otherwise, <c>false</c>.</returns>
    public bool Intersects(BoundingBox2D other) => Min.X <= other.Max.X && Max.X >= other.Min.X && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;

    /// <summary>Computes the union of this bounding box with another.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns>The smallest bounding box containing both.</returns>
    public BoundingBox2D Union(BoundingBox2D other) => new(
        new Point2D(System.Math.Min(Min.X, other.Min.X), System.Math.Min(Min.Y, other.Min.Y)),
        new Point2D(System.Math.Max(Max.X, other.Max.X), System.Math.Max(Max.Y, other.Max.Y)));

    /// <summary>Returns an inflated copy of the bounding box.</summary>
    /// <param name="amount">The amount to inflate on each side.</param>
    /// <returns>The inflated bounding box.</returns>
    public BoundingBox2D Inflate(double amount) => new(
        new Point2D(Min.X - amount, Min.Y - amount),
        new Point2D(Max.X + amount, Max.Y + amount));

    /// <summary>Creates a bounding box from a collection of points.</summary>
    /// <param name="points">The points to enclose.</param>
    /// <returns>The smallest bounding box containing all points.</returns>
    public static BoundingBox2D FromPoints(IEnumerable<Point2D> points)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        bool hasPoints = false;

        foreach (Point2D p in points)
        {
            hasPoints = true;
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        if (!hasPoints) return new BoundingBox2D(Point2D.Origin, Point2D.Origin);
        return new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
    }

    /// <summary>Indexer for corner access by index (0-3).</summary>
    /// <param name="index">The corner index (0 = Min, 1 = (Max.X, Min.Y), 2 = Max, 3 = (Min.X, Max.Y)).</param>
    /// <returns>The corner point.</returns>
    public Point2D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => Min,
            1 => new Point2D(Max.X, Min.Y),
            2 => Max,
            3 => new Point2D(Min.X, Max.Y),
            _ => throw new System.IndexOutOfRangeException($"BoundingBox2D index {index} out of range [0, 3].")
        };
    }

    /// <summary>Returns a string representation of this bounding box.</summary>
    public override string ToString() => $"BoundingBox2D({Min}, {Max})";
}
