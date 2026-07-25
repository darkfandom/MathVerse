using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D vector.</summary>
public readonly record struct Vector3D(double X, double Y, double Z)
{
    /// <summary>The zero vector.</summary>
    public static readonly Vector3D Zero = new(0, 0, 0);

    /// <summary>The unit vector along the X axis.</summary>
    public static readonly Vector3D UnitX = new(1, 0, 0);

    /// <summary>The unit vector along the Y axis.</summary>
    public static readonly Vector3D UnitY = new(0, 1, 0);

    /// <summary>The unit vector along the Z axis.</summary>
    public static readonly Vector3D UnitZ = new(0, 0, 1);

    /// <summary>The X component.</summary>
    public double X { get; } = X;

    /// <summary>The Y component.</summary>
    public double Y { get; } = Y;

    /// <summary>The Z component.</summary>
    public double Z { get; } = Z;

    /// <summary>Gets the component at the specified index (0=X, 1=Y, 2=Z).</summary>
    /// <param name="index">The component index.</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new System.IndexOutOfRangeException($"Vector3D index {index} out of range [0, 2].")
        };
    }

    /// <summary>Gets the length of this vector.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Gets the squared length of this vector.</summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    /// <summary>Returns a unit vector in the same direction.</summary>
    /// <returns>The normalized vector, or zero if the length is near zero.</returns>
    public Vector3D Normalize()
    {
        double l = Length;
        return l > 1e-15 ? new Vector3D(X / l, Y / l, Z / l) : Zero;
    }

    /// <summary>Computes the dot product with another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product.</returns>
    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>Computes the cross product with another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product vector.</returns>
    public Vector3D Cross(Vector3D other) =>
        new(Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X);

    /// <summary>Adds another vector to this vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The sum vector.</returns>
    public Vector3D Add(Vector3D other) => new(X + other.X, Y + other.Y, Z + other.Z);

    /// <summary>Subtracts another vector from this vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The difference vector.</returns>
    public Vector3D Subtract(Vector3D other) => new(X - other.X, Y - other.Y, Z - other.Z);

    /// <summary>Scales this vector by a scalar.</summary>
    /// <param name="s">The scale factor.</param>
    /// <returns>The scaled vector.</returns>
    public Vector3D Scale(double s) => new(X * s, Y * s, Z * s);

    /// <summary>Returns the negation of this vector.</summary>
    /// <returns>The negated vector.</returns>
    public Vector3D Negate() => new(-X, -Y, -Z);

    /// <summary>Computes the angle between this vector and another.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The angle in radians.</returns>
    public double AngleTo(Vector3D other) =>
        System.Math.Acos(Dot(other) / (Length * other.Length));

    /// <summary>Operator overload for vector addition.</summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum vector.</returns>
    public static Vector3D operator +(Vector3D a, Vector3D b) => a.Add(b);

    /// <summary>Operator overload for vector subtraction.</summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference vector.</returns>
    public static Vector3D operator -(Vector3D a, Vector3D b) => a.Subtract(b);

    /// <summary>Operator overload for scalar multiplication (vector * scalar).</summary>
    /// <param name="a">The vector.</param>
    /// <param name="s">The scalar.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3D operator *(Vector3D a, double s) => a.Scale(s);

    /// <summary>Operator overload for scalar multiplication (scalar * vector).</summary>
    /// <param name="s">The scalar.</param>
    /// <param name="a">The vector.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3D operator *(double s, Vector3D a) => a.Scale(s);

    /// <summary>Operator overload for vector negation.</summary>
    /// <param name="a">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static Vector3D operator -(Vector3D a) => a.Negate();

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y}, {Z})";
}
