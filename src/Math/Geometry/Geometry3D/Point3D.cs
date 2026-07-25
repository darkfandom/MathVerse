using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D point.</summary>
public readonly record struct Point3D(double X, double Y, double Z)
{
    /// <summary>The origin point (0, 0, 0).</summary>
    public static readonly Point3D Origin = new(0, 0, 0);

    /// <summary>The X coordinate.</summary>
    public double X { get; } = X;

    /// <summary>The Y coordinate.</summary>
    public double Y { get; } = Y;

    /// <summary>The Z coordinate.</summary>
    public double Z { get; } = Z;

    /// <summary>Gets the coordinate at the specified index (0=X, 1=Y, 2=Z).</summary>
    /// <param name="index">The coordinate index.</param>
    /// <returns>The coordinate value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new System.IndexOutOfRangeException($"Point3D index {index} out of range [0, 2].")
        };
    }

    /// <summary>Computes the Euclidean distance to another point.</summary>
    /// <param name="other">The target point.</param>
    /// <returns>The distance.</returns>
    public double DistanceTo(Point3D other) =>
        System.Math.Sqrt((X - other.X) * (X - other.X) +
                         (Y - other.Y) * (Y - other.Y) +
                         (Z - other.Z) * (Z - other.Z));

    /// <summary>Computes the squared distance to another point.</summary>
    /// <param name="other">The target point.</param>
    /// <returns>The squared distance.</returns>
    public double DistanceSquaredTo(Point3D other) =>
        (X - other.X) * (X - other.X) +
        (Y - other.Y) * (Y - other.Y) +
        (Z - other.Z) * (Z - other.Z);

    /// <summary>Linearly interpolates between this point and another.</summary>
    /// <param name="other">The target point.</param>
    /// <param name="t">Interpolation parameter in [0, 1].</param>
    /// <returns>The interpolated point.</returns>
    public Point3D Lerp(Point3D other, double t) =>
        new(X + (other.X - X) * t, Y + (other.Y - Y) * t, Z + (other.Z - Z) * t);

    /// <summary>Converts this point to a vector from the origin.</summary>
    /// <returns>The corresponding vector.</returns>
    public Vector3D ToVector3D() => new(X, Y, Z);

    /// <summary>Translates this point by a vector.</summary>
    /// <param name="v">The translation vector.</param>
    /// <returns>The translated point.</returns>
    public Point3D Translate(Vector3D v) => new(X + v.X, Y + v.Y, Z + v.Z);

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y}, {Z})";
}
