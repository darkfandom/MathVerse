namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Axis-aligned 3D bounding box for rendering purposes, defined by minimum and maximum corners.</summary>
/// <param name="Min">The minimum corner of the bounding box.</param>
/// <param name="Max">The maximum corner of the bounding box.</param>
public readonly record struct BoundingBox(Vector3 Min, Vector3 Max)
{
    /// <summary>The minimum corner of the bounding box.</summary>
    public Vector3 Min { get; } = Min;

    /// <summary>The maximum corner of the bounding box.</summary>
    public Vector3 Max { get; } = Max;

    /// <summary>Gets the center of the bounding box.</summary>
    public Vector3 Center => Vector3.Lerp(Min, Max, 0.5f);

    /// <summary>Gets the extent (half-size) of the bounding box along each axis.</summary>
    public Vector3 Extent => (Max - Min) * 0.5f;

    /// <summary>Gets the size (extents) of the bounding box.</summary>
    public Vector3 Size => Max - Min;

    /// <summary>Gets the width (extent along X).</summary>
    public float Width => Max.X - Min.X;

    /// <summary>Gets the height (extent along Y).</summary>
    public float Height => Max.Y - Min.Y;

    /// <summary>Gets the depth (extent along Z).</summary>
    public float Depth => Max.Z - Min.Z;

    /// <summary>Tests whether a point is contained within this bounding box.</summary>
    /// <param name="point">The point to test.</param>
    /// <returns><c>true</c> if the point is inside the box; otherwise <c>false</c>.</returns>
    public bool Contains(Vector3 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;

    /// <summary>Tests whether this bounding box intersects another bounding box.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns><c>true</c> if the boxes overlap; otherwise <c>false</c>.</returns>
    public bool Intersects(BoundingBox other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Computes the union of this bounding box with another, enclosing both.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns>The enclosing bounding box.</returns>
    public BoundingBox Union(BoundingBox other) => new(
        new Vector3(
            System.Math.Min(Min.X, other.Min.X),
            System.Math.Min(Min.Y, other.Min.Y),
            System.Math.Min(Min.Z, other.Min.Z)),
        new Vector3(
            System.Math.Max(Max.X, other.Max.X),
            System.Math.Max(Max.Y, other.Max.Y),
            System.Math.Max(Max.Z, other.Max.Z)));

    /// <summary>Expands this bounding box by a uniform margin on all sides.</summary>
    /// <param name="margin">The amount to inflate each side.</param>
    /// <returns>The inflated bounding box.</returns>
    public BoundingBox Inflate(float margin) => new(
        Min - new Vector3(margin, margin, margin),
        Max + new Vector3(margin, margin, margin));

    /// <summary>Transforms this bounding box by a matrix and recomputes the axis-aligned bounds.</summary>
    /// <param name="transform">The transformation matrix.</param>
    /// <returns>The transformed axis-aligned bounding box.</returns>
    public BoundingBox Transform(Matrix4x4 transform)
    {
        Vector3[] corners =
        [
            new(Min.X, Min.Y, Min.Z),
            new(Max.X, Min.Y, Min.Z),
            new(Max.X, Max.Y, Min.Z),
            new(Min.X, Max.Y, Min.Z),
            new(Min.X, Min.Y, Max.Z),
            new(Max.X, Min.Y, Max.Z),
            new(Max.X, Max.Y, Max.Z),
            new(Min.X, Max.Y, Max.Z)
        ];

        Vector3 resultMin = Vector3.Transform(corners[0], transform);
        Vector3 resultMax = resultMin;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 transformed = Vector3.Transform(corners[i], transform);
            resultMin = Vector3.Min(resultMin, transformed);
            resultMax = Vector3.Max(resultMax, transformed);
        }

        return new BoundingBox(resultMin, resultMax);
    }

    /// <summary>Creates a bounding box from a set of vertices.</summary>
    /// <param name="vertices">The vertices to enclose.</param>
    /// <returns>The enclosing bounding box, or a zero-size box at the origin if the collection is empty.</returns>
    public static BoundingBox FromVertices(IReadOnlyList<Vector3> vertices)
    {
        if (vertices.Count == 0)
            return new BoundingBox(Vector3.Zero, Vector3.Zero);

        Vector3 min = vertices[0];
        Vector3 max = vertices[0];

        for (int i = 1; i < vertices.Count; i++)
        {
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }

        return new BoundingBox(min, max);
    }

    /// <summary>Creates a bounding box from a center point and half-extents.</summary>
    /// <param name="center">The center of the bounding box.</param>
    /// <param name="halfExtents">The half-size along each axis.</param>
    /// <returns>The resulting bounding box.</returns>
    public static BoundingBox FromCenterExtents(Vector3 center, Vector3 halfExtents) => new(
        center - halfExtents,
        center + halfExtents);
}
