using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a 2D point.</summary>
public readonly record struct Point2D(double X, double Y)
{
    /// <summary>The origin point (0, 0).</summary>
    public static readonly Point2D Origin = new(0, 0);

    /// <summary>Gets the X coordinate.</summary>
    public double X { get; } = X;

    /// <summary>Gets the Y coordinate.</summary>
    public double Y { get; } = Y;

    /// <summary>Indexer for coordinate access by index (0 = X, 1 = Y).</summary>
    /// <param name="index">The coordinate index.</param>
    /// <returns>The coordinate value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new System.IndexOutOfRangeException($"Point2D index {index} out of range [0, 1].")
        };
    }

    /// <summary>Computes the Euclidean distance to another point.</summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance between the two points.</returns>
    public double DistanceTo(Point2D other) => System.Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

    /// <summary>Computes the squared Euclidean distance to another point.</summary>
    /// <param name="other">The other point.</param>
    /// <returns>The squared distance between the two points.</returns>
    public double DistanceSquaredTo(Point2D other) => (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);

    /// <summary>Linearly interpolates between this point and another.</summary>
    /// <param name="other">The target point.</param>
    /// <param name="t">The interpolation parameter (0 = this, 1 = other).</param>
    /// <returns>The interpolated point.</returns>
    public Point2D Lerp(Point2D other, double t) => new(X + (other.X - X) * t, Y + (other.Y - Y) * t);

    /// <summary>Converts this point to a <see cref="Vector2D"/>.</summary>
    /// <returns>A vector from the origin to this point.</returns>
    public Vector2D ToVector2D() => new(X, Y);

    /// <summary>Translates this point by a vector.</summary>
    /// <param name="v">The translation vector.</param>
    /// <returns>The translated point.</returns>
    public Point2D Translate(Vector2D v) => new(X + v.X, Y + v.Y);

    /// <summary>Returns a string representation of this point.</summary>
    public override string ToString() => $"({X}, {Y})";
}
