using System.Collections.Immutable;
using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Represents a 3D torus defined by a center, major radius, minor radius, and axis direction.</summary>
public readonly record struct Torus3D(Point3D Center, Vector3D Axis, double MajorRadius, double MinorRadius)
{
    /// <summary>The center of the torus.</summary>
    public Point3D Center { get; } = Center;

    /// <summary>The axis direction of the torus (normalized).</summary>
    public Vector3D Axis { get; } = Axis.Normalize();

    /// <summary>The major radius (distance from center to the center of the tube).</summary>
    public double MajorRadius { get; } = MajorRadius;

    /// <summary>The minor radius (radius of the tube).</summary>
    public double MinorRadius { get; } = MinorRadius;

    /// <summary>Gets the volume of the torus: V = 2 * PI^2 * R * r^2.</summary>
    public double Volume => 2.0 * System.Math.PI * System.Math.PI * MajorRadius * MinorRadius * MinorRadius;

    /// <summary>Gets the surface area of the torus: A = 4 * PI^2 * R * r.</summary>
    public double SurfaceArea => 4.0 * System.Math.PI * System.Math.PI * MajorRadius * MinorRadius;

    /// <summary>Gets the inner radius (closest approach to the center).</summary>
    public double InnerRadius => MajorRadius - MinorRadius;

    /// <summary>Gets the outer radius (farthest distance from the center).</summary>
    public double OuterRadius => MajorRadius + MinorRadius;

    /// <summary>Tests whether a point is inside or on the torus.</summary>
    public bool Contains(Point3D p)
    {
        Vector3D toP = new(p.X - Center.X, p.Y - Center.Y, p.Z - Center.Z);
        double axProj = toP.Dot(Axis);
        Vector3D planeProj = toP.Subtract(Axis.Scale(axProj));
        double distToCenter = planeProj.Length;
        double distToTube = System.Math.Sqrt((distToCenter - MajorRadius) * (distToCenter - MajorRadius) + axProj * axProj);
        return distToTube <= MinorRadius;
    }

    /// <summary>Computes the distance from a point to the torus surface.</summary>
    public double DistanceTo(Point3D p)
    {
        Vector3D toP = new(p.X - Center.X, p.Y - Center.Y, p.Z - Center.Z);
        double axProj = toP.Dot(Axis);
        Vector3D planeProj = toP.Subtract(Axis.Scale(axProj));
        double distToCenter = planeProj.Length;
        double tubeDist = System.Math.Sqrt((distToCenter - MajorRadius) * (distToCenter - MajorRadius) + axProj * axProj);
        return System.Math.Max(0.0, tubeDist - MinorRadius);
    }

    /// <summary>Computes an axis-aligned bounding box enclosing this torus.</summary>
    public BoundingBox3D ToBoundingBox()
    {
        double r = MajorRadius + MinorRadius;
        double ax = System.Math.Abs(Axis.X), ay = System.Math.Abs(Axis.Y), az = System.Math.Abs(Axis.Z);
        double ex = r * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - ax * ax)) + MinorRadius * ax;
        double ey = r * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - ay * ay)) + MinorRadius * ay;
        double ez = r * System.Math.Sqrt(System.Math.Max(0.0, 1.0 - az * az)) + MinorRadius * az;
        return new BoundingBox3D(
            new Point3D(Center.X - ex, Center.Y - ey, Center.Z - ez),
            new Point3D(Center.X + ex, Center.Y + ey, Center.Z + ez));
    }

    /// <summary>Computes a point on the torus surface at the given angles.</summary>
    /// <param name="majorAngle">Angle around the major circle in radians.</param>
    /// <param name="minorAngle">Angle around the tube in radians.</param>
    public Point3D PointAt(double majorAngle, double minorAngle)
    {
        Vector3D u = Axis;
        Vector3D a;
        if (System.Math.Abs(u.X) <= System.Math.Abs(u.Y))
            a = u.Cross(Vector3D.UnitX).Normalize();
        else
            a = u.Cross(Vector3D.UnitY).Normalize();
        Vector3D b = u.Cross(a);

        double cosMajor = System.Math.Cos(majorAngle);
        double sinMajor = System.Math.Sin(majorAngle);
        double cosMinor = System.Math.Cos(minorAngle);
        double sinMinor = System.Math.Sin(minorAngle);

        Point3D tubeCenter = new(
            Center.X + MajorRadius * (cosMajor * a.X + sinMajor * b.X),
            Center.Y + MajorRadius * (cosMajor * a.Y + sinMajor * b.Y),
            Center.Z + MajorRadius * (cosMajor * a.Z + sinMajor * b.Z));

        Vector3D outward = new(
            cosMajor * a.X + sinMajor * b.X,
            cosMajor * a.Y + sinMajor * b.Y,
            cosMajor * a.Z + sinMajor * b.Z);

        return new Point3D(
            tubeCenter.X + MinorRadius * (cosMinor * outward.X + sinMinor * u.X),
            tubeCenter.Y + MinorRadius * (cosMinor * outward.Y + sinMinor * u.Y),
            tubeCenter.Z + MinorRadius * (cosMinor * outward.Z + sinMinor * u.Z));
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Torus3D(Center={Center}, Major={MajorRadius:F4}, Minor={MinorRadius:F4})";
}
