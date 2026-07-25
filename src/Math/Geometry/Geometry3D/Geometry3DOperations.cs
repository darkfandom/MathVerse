using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Geometry3D;

/// <summary>Provides static utility methods for 3D geometry operations.</summary>
public static class Geometry3DOperations
{
    /// <summary>Computes the Euclidean distance between two points.</summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point3D a, Point3D b) => a.DistanceTo(b);

    /// <summary>Computes the shortest distance from a point to a line segment.</summary>
    /// <param name="line">The line segment.</param>
    /// <param name="p">The query point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Line3D line, Point3D p) => line.DistanceTo(p);

    /// <summary>Computes the distance from a point to a plane.</summary>
    /// <param name="plane">The plane.</param>
    /// <param name="p">The query point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Plane3D plane, Point3D p) => plane.DistanceTo(p);

    /// <summary>Computes the closest approach between two line segments.</summary>
    /// <param name="a">The first line segment.</param>
    /// <param name="b">The second line segment.</param>
    /// <returns>A tuple indicating whether the segments are nearly coincident, the closest point on the first segment, and the distance.</returns>
    public static (bool hit, Point3D point, double distance) Intersect(Line3D a, Line3D b) => a.Intersect(b);

    /// <summary>Computes the intersection of a line segment with a plane.</summary>
    /// <param name="line">The line segment.</param>
    /// <param name="plane">The plane.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public static (bool hit, Point3D point) Intersect(Line3D line, Plane3D plane) => line.Intersect(plane);

    /// <summary>Computes the intersection line of two planes.</summary>
    /// <param name="a">The first plane.</param>
    /// <param name="b">The second plane.</param>
    /// <returns>A tuple indicating whether the planes intersect and the resulting line.</returns>
    public static (bool hit, Line3D line) Intersect(Plane3D a, Plane3D b) => a.Intersect(b);

    /// <summary>Computes the intersection of a line segment with a sphere.</summary>
    /// <param name="line">The line segment.</param>
    /// <param name="sphere">The sphere.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public static (bool hit, ImmutableArray<Point3D> points) Intersect(Line3D line, Sphere3D sphere) => sphere.Intersect(line);

    /// <summary>Tests for intersection between a triangle and a line segment using Möller–Trumbore.</summary>
    /// <param name="tri">The triangle.</param>
    /// <param name="line">The line segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public static (bool hit, Point3D point) Intersect(Triangle3D tri, Line3D line) => tri.Intersect(line);

    /// <summary>Projects a point onto a plane.</summary>
    /// <param name="p">The point to project.</param>
    /// <param name="plane">The target plane.</param>
    /// <returns>The projected point.</returns>
    public static Point3D Project(Point3D p, Plane3D plane) => plane.Project(p);

    /// <summary>Projects a point onto a line segment (finds the closest point on the segment).</summary>
    /// <param name="p">The point to project.</param>
    /// <param name="line">The target line segment.</param>
    /// <returns>The closest point on the segment.</returns>
    public static Point3D Project(Point3D p, Line3D line) => line.ClosestPoint(p);

    /// <summary>Finds the closest point on a triangle to a given point.</summary>
    /// <param name="tri">The triangle.</param>
    /// <param name="p">The query point.</param>
    /// <returns>The closest point on the triangle.</returns>
    public static Point3D ClosestPoint(Triangle3D tri, Point3D p) => tri.ClosestPoint(p);

    /// <summary>Computes the volume of a sphere.</summary>
    /// <param name="sphere">The sphere.</param>
    /// <returns>The volume.</returns>
    public static double Volume(Sphere3D sphere) => sphere.Volume;

    /// <summary>Computes the volume of a cylinder.</summary>
    /// <param name="cylinder">The cylinder.</param>
    /// <returns>The volume.</returns>
    public static double Volume(Cylinder3D cylinder) => cylinder.Volume;

    /// <summary>Computes the volume of a cone.</summary>
    /// <param name="cone">The cone.</param>
    /// <returns>The volume.</returns>
    public static double Volume(Cone3D cone) => cone.Volume;

    /// <summary>Computes the surface area of a sphere.</summary>
    /// <param name="sphere">The sphere.</param>
    /// <returns>The surface area.</returns>
    public static double SurfaceArea(Sphere3D sphere) => sphere.SurfaceArea;

    /// <summary>Computes the surface area of a cylinder.</summary>
    /// <param name="cylinder">The cylinder.</param>
    /// <returns>The surface area.</returns>
    public static double SurfaceArea(Cylinder3D cylinder) => cylinder.SurfaceArea;

    /// <summary>Computes the unit normal of a triangle.</summary>
    /// <param name="tri">The triangle.</param>
    /// <returns>The unit normal vector.</returns>
    public static Vector3D Normal(Triangle3D tri) => tri.Normal;

    /// <summary>Computes an axis-aligned bounding box for a triangle.</summary>
    /// <param name="tri">The triangle.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingBox3D BoundingBox(Triangle3D tri) => tri.ToBoundingBox();

    /// <summary>Computes an axis-aligned bounding box for a sphere.</summary>
    /// <param name="sphere">The sphere.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingBox3D BoundingBox(Sphere3D sphere) => sphere.ToBoundingBox();
}
