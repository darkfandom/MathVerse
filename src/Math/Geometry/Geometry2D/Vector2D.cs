using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a 2D vector.</summary>
public readonly record struct Vector2D(double X, double Y)
{
    /// <summary>The zero vector.</summary>
    public static readonly Vector2D Zero = new(0, 0);

    /// <summary>The unit vector along the X axis.</summary>
    public static readonly Vector2D UnitX = new(1, 0);

    /// <summary>The unit vector along the Y axis.</summary>
    public static readonly Vector2D UnitY = new(0, 1);

    /// <summary>Gets the X component.</summary>
    public double X { get; } = X;

    /// <summary>Gets the Y component.</summary>
    public double Y { get; } = Y;

    /// <summary>Gets the length of this vector.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y);

    /// <summary>Gets the squared length of this vector.</summary>
    public double LengthSquared => X * X + Y * Y;

    /// <summary>Returns a normalized copy of this vector.</summary>
    /// <returns>A unit vector in the same direction, or <see cref="Zero"/> if the length is near zero.</returns>
    public Vector2D Normalize()
    {
        double l = Length;
        return l > 1e-15 ? new Vector2D(X / l, Y / l) : Zero;
    }

    /// <summary>Computes the dot product with another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product.</returns>
    public double Dot(Vector2D other) => X * other.X + Y * other.Y;

    /// <summary>Computes the 2D cross product (scalar) with another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product scalar (Z component of the 3D cross product).</returns>
    public double Cross(Vector2D other) => X * other.Y - Y * other.X;

    /// <summary>Adds another vector to this vector.</summary>
    /// <param name="other">The vector to add.</param>
    /// <returns>The sum of the two vectors.</returns>
    public Vector2D Add(Vector2D other) => new(X + other.X, Y + other.Y);

    /// <summary>Subtracts another vector from this vector.</summary>
    /// <param name="other">The vector to subtract.</param>
    /// <returns>The difference of the two vectors.</returns>
    public Vector2D Subtract(Vector2D other) => new(X - other.X, Y - other.Y);

    /// <summary>Scales this vector by a scalar value.</summary>
    /// <param name="s">The scale factor.</param>
    /// <returns>The scaled vector.</returns>
    public Vector2D Scale(double s) => new(X * s, Y * s);

    /// <summary>Returns the negated vector.</summary>
    /// <returns>The vector with both components negated.</returns>
    public Vector2D Negate() => new(-X, -Y);

    /// <summary>Returns a perpendicular vector (rotated 90 degrees counter-clockwise).</summary>
    /// <returns>The perpendicular vector.</returns>
    public Vector2D Perpendicular() => new(-Y, X);

    /// <summary>Computes the signed angle from this vector to another vector.</summary>
    /// <param name="other">The target vector.</param>
    /// <returns>The signed angle in radians.</returns>
    public double AngleTo(Vector2D other) => System.Math.Atan2(Cross(other), Dot(other));

    /// <summary>Gets the angle of this vector from the positive X axis.</summary>
    public double Angle => System.Math.Atan2(Y, X);

    /// <summary>Indexer for component access by index (0 = X, 1 = Y).</summary>
    /// <param name="index">The component index.</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new System.IndexOutOfRangeException($"Vector2D index {index} out of range [0, 1].")
        };
    }

    /// <summary>Adds two vectors.</summary>
    public static Vector2D operator +(Vector2D a, Vector2D b) => a.Add(b);

    /// <summary>Subtracts two vectors.</summary>
    public static Vector2D operator -(Vector2D a, Vector2D b) => a.Subtract(b);

    /// <summary>Scales a vector by a scalar.</summary>
    public static Vector2D operator *(Vector2D a, double s) => a.Scale(s);

    /// <summary>Scales a vector by a scalar.</summary>
    public static Vector2D operator *(double s, Vector2D a) => a.Scale(s);

    /// <summary>Negates a vector.</summary>
    public static Vector2D operator -(Vector2D a) => a.Negate();

    /// <summary>Returns a string representation of this vector.</summary>
    public override string ToString() => $"({X}, {Y})";
}
