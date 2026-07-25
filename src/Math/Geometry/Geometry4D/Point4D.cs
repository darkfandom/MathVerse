using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry4D;

/// <summary>Represents a 4D point with homogeneous coordinates.</summary>
public readonly record struct Point4D(double X, double Y, double Z, double W)
{
    /// <summary>The origin point (0, 0, 0, 1).</summary>
    public static readonly Point4D Origin = new(0, 0, 0, 1);

    /// <summary>The X coordinate.</summary>
    public double X { get; } = X;

    /// <summary>The Y coordinate.</summary>
    public double Y { get; } = Y;

    /// <summary>The Z coordinate.</summary>
    public double Z { get; } = Z;

    /// <summary>The W (homogeneous) coordinate.</summary>
    public double W { get; } = W;

    /// <summary>Indexer for coordinate access by index (0=X, 1=Y, 2=Z, 3=W).</summary>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => X, 1 => Y, 2 => Z, 3 => W,
            _ => throw new System.IndexOutOfRangeException($"Point4D index {index} out of range [0, 3].")
        };
    }

    /// <summary>Gets the Euclidean length from the origin in 4D.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

    /// <summary>Computes the Euclidean distance to another point.</summary>
    public double DistanceTo(Point4D other) =>
        System.Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y) +
                         (Z - other.Z) * (Z - other.Z) + (W - other.W) * (W - other.W));

    /// <summary>Computes the squared distance to another point.</summary>
    public double DistanceSquaredTo(Point4D other) =>
        (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y) +
        (Z - other.Z) * (Z - other.Z) + (W - other.W) * (W - other.W);

    /// <summary>Linearly interpolates between this point and another.</summary>
    public Point4D Lerp(Point4D other, double t) =>
        new(X + (other.X - X) * t, Y + (other.Y - Y) * t,
            Z + (other.Z - Z) * t, W + (other.W - W) * t);

    /// <summary>Projects to 3D by dividing by W.</summary>
    public Geometry3D.Point3D ToPoint3D() =>
        new(X / W, Y / W, Z / W);

    /// <summary>Converts to a Vector4D.</summary>
    public Vector4D ToVector4D() => new(X, Y, Z, W);

    /// <summary>Returns a string representation.</summary>
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
