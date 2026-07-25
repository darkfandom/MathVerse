using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a circular arc defined by a center, radius, start angle, and end angle.</summary>
public readonly record struct Arc2D(Point2D Center, double Radius, double StartAngle, double EndAngle)
{
    /// <summary>The center of the arc.</summary>
    public Point2D Center { get; } = Center;

    /// <summary>The radius of the arc.</summary>
    public double Radius { get; } = Radius;

    /// <summary>The start angle in radians.</summary>
    public double StartAngle { get; } = StartAngle;

    /// <summary>The end angle in radians.</summary>
    public double EndAngle { get; } = EndAngle;

    /// <summary>Returns a point along the arc at parameter t.</summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point on the arc at parameter t.</returns>
    public Point2D PointAt(double t)
    {
        double angle = StartAngle + (EndAngle - StartAngle) * t;
        return new Point2D(Center.X + Radius * System.Math.Cos(angle), Center.Y + Radius * System.Math.Sin(angle));
    }

    /// <summary>Gets the arc length.</summary>
    public double Length
    {
        get
        {
            double sweep = EndAngle - StartAngle;
            while (sweep < 0) sweep += 2.0 * System.Math.PI;
            while (sweep > 2.0 * System.Math.PI) sweep -= 2.0 * System.Math.PI;
            return Radius * sweep;
        }
    }

    /// <summary>Computes the axis-aligned bounding box of this arc.</summary>
    /// <returns>The bounding box enclosing the arc.</returns>
    public BoundingBox2D ToBoundingBox()
    {
        double minX = Center.X + Radius * System.Math.Cos(StartAngle);
        double maxX = minX;
        double minY = Center.Y + Radius * System.Math.Sin(StartAngle);
        double maxY = minY;

        double sweep = EndAngle - StartAngle;
        while (sweep < 0) sweep += 2.0 * System.Math.PI;
        while (sweep > 2.0 * System.Math.PI) sweep -= 2.0 * System.Math.PI;

        for (double a = 0; a <= sweep; a += System.Math.PI * 0.5)
        {
            double angle = StartAngle + a;
            double px = Center.X + Radius * System.Math.Cos(angle);
            double py = Center.Y + Radius * System.Math.Sin(angle);
            if (px < minX) minX = px;
            if (px > maxX) maxX = px;
            if (py < minY) minY = py;
            if (py > maxY) maxY = py;
        }

        return new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
    }

    /// <summary>Indexer for component access by index.</summary>
    /// <param name="index">The component index (0 = Center X, 1 = Center Y, 2 = Radius, 3 = StartAngle, 4 = EndAngle).</param>
    /// <returns>The component value.</returns>
    public double this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => index switch
        {
            0 => Center.X,
            1 => Center.Y,
            2 => Radius,
            3 => StartAngle,
            4 => EndAngle,
            _ => throw new System.IndexOutOfRangeException($"Arc2D index {index} out of range [0, 4].")
        };
    }

    /// <summary>Returns a string representation of this arc.</summary>
    public override string ToString() => $"Arc2D({Center}, r={Radius}, {StartAngle}..{EndAngle})";
}
