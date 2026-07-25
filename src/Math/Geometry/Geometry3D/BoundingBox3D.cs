using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Transformations;
using MathVerse.Math.Geometry.Picking;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents an axis-aligned 3D bounding box defined by minimum and maximum corners.</summary>
public readonly record struct BoundingBox3D(Point3D Min, Point3D Max)
{
    /// <summary>The minimum corner.</summary>
    public Point3D Min { get; } = Min;

    /// <summary>The maximum corner.</summary>
    public Point3D Max { get; } = Max;

    /// <summary>Gets the width (extent along X).</summary>
    public double Width => Max.X - Min.X;

    /// <summary>Gets the height (extent along Y).</summary>
    public double Height => Max.Y - Min.Y;

    /// <summary>Gets the depth (extent along Z).</summary>
    public double Depth => Max.Z - Min.Z;

    /// <summary>Gets the center point.</summary>
    public Point3D Center => new(
        (Min.X + Max.X) * 0.5,
        (Min.Y + Max.Y) * 0.5,
        (Min.Z + Max.Z) * 0.5);

    /// <summary>Gets the volume of the box.</summary>
    public double Volume => Width * Height * Depth;

    /// <summary>Gets the surface area of the box.</summary>
    public double SurfaceArea =>
        2.0 * (Width * Height + Height * Depth + Depth * Width);

    /// <summary>Tests whether a point is inside or on the box.</summary>
    /// <param name="p">The query point.</param>
    /// <returns>True if the point is contained within the box.</returns>
    public bool Contains(Point3D p) =>
        p.X >= Min.X && p.X <= Max.X &&
        p.Y >= Min.Y && p.Y <= Max.Y &&
        p.Z >= Min.Z && p.Z <= Max.Z;

    /// <summary>Tests whether another bounding box is entirely inside this box.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns>True if the other box is entirely contained within this box.</returns>
    public bool Contains(BoundingBox3D other) =>
        other.Min.X >= Min.X && other.Max.X <= Max.X &&
        other.Min.Y >= Min.Y && other.Max.Y <= Max.Y &&
        other.Min.Z >= Min.Z && other.Max.Z <= Max.Z;

    /// <summary>Tests whether this box intersects another box.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns>True if the boxes overlap.</returns>
    public bool Intersects(BoundingBox3D other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Computes the union of this box with another, enclosing both.</summary>
    /// <param name="other">The other bounding box.</param>
    /// <returns>The enclosing bounding box.</returns>
    public BoundingBox3D Union(BoundingBox3D other) => new(
        new Point3D(
            System.Math.Min(Min.X, other.Min.X),
            System.Math.Min(Min.Y, other.Min.Y),
            System.Math.Min(Min.Z, other.Min.Z)),
        new Point3D(
            System.Math.Max(Max.X, other.Max.X),
            System.Math.Max(Max.Y, other.Max.Y),
            System.Math.Max(Max.Z, other.Max.Z)));

    /// <summary>Expands the box by a uniform amount on all sides.</summary>
    /// <param name="amount">The amount to inflate each side.</param>
    /// <returns>The inflated bounding box.</returns>
    public BoundingBox3D Inflate(double amount) => new(
        new Point3D(Min.X - amount, Min.Y - amount, Min.Z - amount),
        new Point3D(Max.X + amount, Max.Y + amount, Max.Z + amount));

    /// <summary>Transforms this bounding box and recomputes the axis-aligned bounds.</summary>
    /// <param name="t">The transform to apply.</param>
    /// <returns>The transformed bounding box.</returns>
    public BoundingBox3D Transform(Transform3D t)
    {
        ImmutableArray<Point3D> corners = Corners;
        Point3D transformed = t.TransformPoint(corners[0]);
        double minX = transformed.X;
        double minY = transformed.Y;
        double minZ = transformed.Z;
        double maxX = transformed.X;
        double maxY = transformed.Y;
        double maxZ = transformed.Z;

        for (int i = 1; i < corners.Length; i++)
        {
            transformed = t.TransformPoint(corners[i]);
            if (transformed.X < minX) minX = transformed.X;
            if (transformed.Y < minY) minY = transformed.Y;
            if (transformed.Z < minZ) minZ = transformed.Z;
            if (transformed.X > maxX) maxX = transformed.X;
            if (transformed.Y > maxY) maxY = transformed.Y;
            if (transformed.Z > maxZ) maxZ = transformed.Z;
        }

        return new BoundingBox3D(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
    }

    /// <summary>Gets the eight corners of the bounding box.</summary>
    public ImmutableArray<Point3D> Corners => ImmutableArray.Create(
        new Point3D(Min.X, Min.Y, Min.Z),
        new Point3D(Max.X, Min.Y, Min.Z),
        new Point3D(Max.X, Max.Y, Min.Z),
        new Point3D(Min.X, Max.Y, Min.Z),
        new Point3D(Min.X, Min.Y, Max.Z),
        new Point3D(Max.X, Min.Y, Max.Z),
        new Point3D(Max.X, Max.Y, Max.Z),
        new Point3D(Min.X, Max.Y, Max.Z));

    /// <summary>Creates a bounding box enclosing all given points.</summary>
    /// <param name="points">The points to enclose.</param>
    /// <returns>The bounding box, or a zero-size box at the origin if the collection is empty.</returns>
    public static BoundingBox3D FromPoints(IEnumerable<Point3D> points)
    {
        using IEnumerator<Point3D> enumerator = points.GetEnumerator();
        if (!enumerator.MoveNext())
            return new BoundingBox3D(Point3D.Origin, Point3D.Origin);

        Point3D min = enumerator.Current;
        Point3D max = min;

        while (enumerator.MoveNext())
        {
            Point3D p = enumerator.Current;
            if (p.X < min.X) min = new Point3D(p.X, min.Y, min.Z);
            if (p.Y < min.Y) min = new Point3D(min.X, p.Y, min.Z);
            if (p.Z < min.Z) min = new Point3D(min.X, min.Y, p.Z);
            if (p.X > max.X) max = new Point3D(p.X, max.Y, max.Z);
            if (p.Y > max.Y) max = new Point3D(max.X, p.Y, max.Z);
            if (p.Z > max.Z) max = new Point3D(max.X, max.Y, p.Z);
        }

        return new BoundingBox3D(min, max);
    }

    /// <summary>Gets a corner by index.</summary>
    /// <param name="index">The corner index (0–7).</param>
    /// <returns>The corner point.</returns>
    public Point3D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => new Point3D(Min.X, Min.Y, Min.Z),
            1 => new Point3D(Max.X, Min.Y, Min.Z),
            2 => new Point3D(Max.X, Max.Y, Min.Z),
            3 => new Point3D(Min.X, Max.Y, Min.Z),
            4 => new Point3D(Min.X, Min.Y, Max.Z),
            5 => new Point3D(Max.X, Min.Y, Max.Z),
            6 => new Point3D(Max.X, Max.Y, Max.Z),
            7 => new Point3D(Min.X, Max.Y, Max.Z),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    /// <summary>Tests whether a ray intersects this bounding box.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <returns>True if the ray intersects the box.</returns>
    public bool RayIntersects(Picking.Ray ray)
    {
        double tmin = double.MinValue, tmax = double.MaxValue;
        double[] origin = { ray.Origin.X, ray.Origin.Y, ray.Origin.Z };
        double[] dir = { ray.Direction.X, ray.Direction.Y, ray.Direction.Z };
        double[] bmin = { Min.X, Min.Y, Min.Z };
        double[] bmax = { Max.X, Max.Y, Max.Z };

        for (int i = 0; i < 3; i++)
        {
            if (System.Math.Abs(dir[i]) < 1e-15)
            {
                if (origin[i] < bmin[i] || origin[i] > bmax[i]) return false;
                continue;
            }
            double invD = 1.0 / dir[i];
            double t1 = (bmin[i] - origin[i]) * invD;
            double t2 = (bmax[i] - origin[i]) * invD;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = System.Math.Max(tmin, t1);
            tmax = System.Math.Min(tmax, t2);
            if (tmin > tmax) return false;
        }
        return tmax >= 0;
    }

    /// <inheritdoc/>
    public override string ToString() => $"BoundingBox3D(Min={Min}, Max={Max})";
}
