namespace MathVerse.Math.Geometry.Picking;

using Geometry3D;

/// <summary>Represents a picking ray in 3D space.</summary>
public readonly record struct Ray(Point3D Origin, Vector3D Direction)
{
    /// <summary>Point along the ray at parameter t.</summary>
    public Point3D PointAt(double t) => new(Origin.X + Direction.X * t, Origin.Y + Direction.Y * t, Origin.Z + Direction.Z * t);
    
    /// <summary>Distance from ray origin to the closest point on the ray to a given point.</summary>
    public double ClosestParameter(Point3D point)
    {
        Vector3D toPoint = new(point.X - Origin.X, point.Y - Origin.Y, point.Z - Origin.Z);
        return toPoint.Dot(Direction);
    }
    
    /// <summary>Closest point on the ray to a given point.</summary>
    public Point3D ClosestPoint(Point3D point) => PointAt(ClosestParameter(point));
    
    /// <summary>Distance from this ray to a point.</summary>
    public double DistanceTo(Point3D point)
    {
        double t = ClosestParameter(point);
        if (t < 0) return Origin.DistanceTo(point);
        return PointAt(t).DistanceTo(point);
    }
}
