using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry4D;

/// <summary>Represents a 4D vector.</summary>
public readonly record struct Vector4D(double X, double Y, double Z, double W)
{
    /// <summary>The zero vector.</summary>
    public static readonly Vector4D Zero = new(0, 0, 0, 0);

    /// <summary>The unit vector along the X axis.</summary>
    public static readonly Vector4D UnitX = new(1, 0, 0, 0);

    /// <summary>The unit vector along the Y axis.</summary>
    public static readonly Vector4D UnitY = new(0, 1, 0, 0);

    /// <summary>The unit vector along the Z axis.</summary>
    public static readonly Vector4D UnitZ = new(0, 0, 1, 0);

    /// <summary>The unit vector along the W axis.</summary>
    public static readonly Vector4D UnitW = new(0, 0, 0, 1);

    /// <summary>The X component.</summary>
    public double X { get; } = X;

    /// <summary>The Y component.</summary>
    public double Y { get; } = Y;

    /// <summary>The Z component.</summary>
    public double Z { get; } = Z;

    /// <summary>The W component.</summary>
    public double W { get; } = W;

    /// <summary>Indexer for component access by index (0=X, 1=Y, 2=Z, 3=W).</summary>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X, 1 => Y, 2 => Z, 3 => W,
            _ => throw new System.IndexOutOfRangeException($"Vector4D index {index} out of range [0, 3].")
        };
    }

    /// <summary>Gets the length of this vector.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

    /// <summary>Gets the squared length.</summary>
    public double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    /// <summary>Returns a unit vector in the same direction.</summary>
    public Vector4D Normalize()
    {
        double l = Length;
        return l > 1e-15 ? new Vector4D(X / l, Y / l, Z / l, W / l) : Zero;
    }

    /// <summary>Computes the dot product with another vector.</summary>
    public double Dot(Vector4D other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

    /// <summary>Returns the negation of this vector.</summary>
    public Vector4D Negate() => new(-X, -Y, -Z, -W);

    /// <summary>Adds another vector.</summary>
    public Vector4D Add(Vector4D other) => new(X + other.X, Y + other.Y, Z + other.Z, W + other.W);

    /// <summary>Subtracts another vector.</summary>
    public Vector4D Subtract(Vector4D other) => new(X - other.X, Y - other.Y, Z - other.Z, W - other.W);

    /// <summary>Scales this vector by a scalar.</summary>
    public Vector4D Scale(double s) => new(X * s, Y * s, Z * s, W * s);

    /// <summary>Projects to 3D by dividing by W.</summary>
    public Geometry3D.Vector3D ToVector3D() => new(X / W, Y / W, Z / W);

    /// <summary>Operator overload for vector addition.</summary>
    public static Vector4D operator +(Vector4D a, Vector4D b) => a.Add(b);

    /// <summary>Operator overload for vector subtraction.</summary>
    public static Vector4D operator -(Vector4D a, Vector4D b) => a.Subtract(b);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Vector4D operator *(Vector4D a, double s) => a.Scale(s);

    /// <summary>Operator overload for scalar multiplication.</summary>
    public static Vector4D operator *(double s, Vector4D a) => a.Scale(s);

    /// <summary>Operator overload for vector negation.</summary>
    public static Vector4D operator -(Vector4D a) => a.Negate();

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
