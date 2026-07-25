using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents an immutable unit direction vector in 3D space, guaranteed to be normalized.</summary>
public readonly record struct Direction3D(double X, double Y, double Z)
{
    /// <summary>The forward direction (0, 0, 1).</summary>
    public static readonly Direction3D Forward = new(0, 0, 1);

    /// <summary>The backward direction (0, 0, -1).</summary>
    public static readonly Direction3D Backward = new(0, 0, -1);

    /// <summary>The up direction (0, 1, 0).</summary>
    public static readonly Direction3D Up = new(0, 1, 0);

    /// <summary>The down direction (0, -1, 0).</summary>
    public static readonly Direction3D Down = new(0, -1, 0);

    /// <summary>The right direction (1, 0, 0).</summary>
    public static readonly Direction3D Right = new(1, 0, 0);

    /// <summary>The left direction (-1, 0, 0).</summary>
    public static readonly Direction3D Left = new(-1, 0, 0);

    /// <summary>The X component.</summary>
    public double X { get; } = X;

    /// <summary>The Y component.</summary>
    public double Y { get; } = Y;

    /// <summary>The Z component.</summary>
    public double Z { get; } = Z;

    /// <summary>Indexer for component access (0=X, 1=Y, 2=Z).</summary>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X, 1 => Y, 2 => Z,
            _ => throw new System.IndexOutOfRangeException($"Direction3D index {index} out of range [0, 2].")
        };
    }

    /// <summary>Creates a Direction3D from a vector, normalizing it.</summary>
    public static Direction3D FromVector(Vector3D v)
    {
        Vector3D n = v.Normalize();
        return new Direction3D(n.X, n.Y, n.Z);
    }

    /// <summary>Creates a Direction3D from two points (direction from A to B).</summary>
    public static Direction3D FromPoints(Point3D a, Point3D b)
    {
        Vector3D v = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D n = v.Normalize();
        return new Direction3D(n.X, n.Y, n.Z);
    }

    /// <summary>Converts to a Vector3D.</summary>
    public Vector3D ToVector3D() => new(X, Y, Z);

    /// <summary>Computes the angle between two directions in radians.</summary>
    public double AngleTo(Direction3D other) =>
        System.Math.Acos(System.Math.Max(-1.0, System.Math.Min(1.0, Dot(other))));

    /// <summary>Computes the dot product with another direction.</summary>
    public double Dot(Direction3D other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>Computes the cross product with another direction (result may not be unit-length).</summary>
    public Vector3D Cross(Direction3D other) =>
        new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

    /// <summary>Returns the negated direction.</summary>
    public Direction3D Negate() => new(-X, -Y, -Z);

    /// <summary>Linearly interpolates between two directions.</summary>
    public Direction3D Lerp(Direction3D other, double t) =>
        FromVector(new Vector3D(
            X + (other.X - X) * t,
            Y + (other.Y - Y) * t,
            Z + (other.Z - Z) * t));

    /// <inheritdoc/>
    public override string ToString() => $"({X:F6}, {Y:F6}, {Z:F6})";
}
