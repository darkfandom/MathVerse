using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents an axis-aligned rectangle defined by minimum and maximum corners.</summary>
public readonly record struct Rectangle2D(Point2D Min, Point2D Max)
{
    /// <summary>The minimum corner.</summary>
    public Point2D Min { get; } = Min;

    /// <summary>The maximum corner.</summary>
    public Point2D Max { get; } = Max;

    /// <summary>Gets the width of the rectangle.</summary>
    public double Width => Max.X - Min.X;

    /// <summary>Gets the height of the rectangle.</summary>
    public double Height => Max.Y - Min.Y;

    /// <summary>Gets the center of the rectangle.</summary>
    public Point2D Center => new((Min.X + Max.X) * 0.5, (Min.Y + Max.Y) * 0.5);

    /// <summary>Gets the area of the rectangle.</summary>
    public double Area => Width * Height;

    /// <summary>Gets the perimeter of the rectangle.</summary>
    public double Perimeter => 2.0 * (Width + Height);

    /// <summary>Determines whether the rectangle contains the specified point.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the rectangle contains the point; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p) => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;

    /// <summary>Determines whether this rectangle fully contains another rectangle.</summary>
    /// <param name="other">The other rectangle.</param>
    /// <returns><c>true</c> if this rectangle contains the other; otherwise, <c>false</c>.</returns>
    public bool Contains(Rectangle2D other) => other.Min.X >= Min.X && other.Max.X <= Max.X && other.Min.Y >= Min.Y && other.Max.Y <= Max.Y;

    /// <summary>Determines whether this rectangle intersects another rectangle.</summary>
    /// <param name="other">The other rectangle.</param>
    /// <returns><c>true</c> if the rectangles intersect; otherwise, <c>false</c>.</returns>
    public bool Intersects(Rectangle2D other) => Min.X <= other.Max.X && Max.X >= other.Min.X && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;

    /// <summary>Computes the intersection of this rectangle with another rectangle.</summary>
    /// <param name="other">The other rectangle.</param>
    /// <returns>The intersection rectangle, or <c>null</c> if there is no intersection.</returns>
    public Rectangle2D? Intersect(Rectangle2D other)
    {
        double ixMin = System.Math.Max(Min.X, other.Min.X);
        double iyMin = System.Math.Max(Min.Y, other.Min.Y);
        double ixMax = System.Math.Min(Max.X, other.Max.X);
        double iyMax = System.Math.Min(Max.Y, other.Max.Y);
        if (ixMin > ixMax || iyMin > iyMax) return null;
        return new Rectangle2D(new Point2D(ixMin, iyMin), new Point2D(ixMax, iyMax));
    }

    /// <summary>Returns an inflated copy of the rectangle.</summary>
    /// <param name="amount">The amount to inflate on each side.</param>
    /// <returns>The inflated rectangle.</returns>
    public Rectangle2D Inflate(double amount) => new(
        new Point2D(Min.X - amount, Min.Y - amount),
        new Point2D(Max.X + amount, Max.Y + amount));

    /// <summary>Returns a translated copy of the rectangle.</summary>
    /// <param name="offset">The translation offset.</param>
    /// <returns>The translated rectangle.</returns>
    public Rectangle2D Translate(Vector2D offset) => new(
        new Point2D(Min.X + offset.X, Min.Y + offset.Y),
        new Point2D(Max.X + offset.X, Max.Y + offset.Y));

    /// <summary>Gets the four corner points of the rectangle.</summary>
    public ImmutableArray<Point2D> Points => ImmutableArray.Create(
        new Point2D(Min.X, Min.Y),
        new Point2D(Max.X, Min.Y),
        new Point2D(Max.X, Max.Y),
        new Point2D(Min.X, Max.Y));

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
            _ => throw new System.IndexOutOfRangeException($"Rectangle2D index {index} out of range [0, 3].")
        };
    }

    /// <summary>Returns a string representation of this rectangle.</summary>
    public override string ToString() => $"Rectangle2D({Min}, {Max})";
}
