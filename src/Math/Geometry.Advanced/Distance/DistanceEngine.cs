using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Distance;

/// <summary>
/// Represents the result of a distance computation between two geometric objects.
/// </summary>
/// <param name="Distance">The minimum distance between the two objects.</param>
/// <param name="ClosestOnA">The closest point on the first object.</param>
/// <param name="ClosestOnB">The closest point on the second object.</param>
public readonly record struct DistanceResult(double Distance, Point3D ClosestOnA, Point3D ClosestOnB);

/// <summary>
/// Provides static methods for computing distances between various geometric primitives.
/// </summary>
public static class DistanceEngine
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the Euclidean distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    public static double PointToPoint(Point3D a, Point3D b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double dz = b.Z - a.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Computes the distance from a point to an infinite line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line (normalized).</param>
    /// <returns>The distance from the point to the line.</returns>
    public static double PointToLine(Point3D point, Point3D lineOrigin, Vector3D lineDir)
    {
        Vector3D toPoint = new Vector3D(
            point.X - lineOrigin.X,
            point.Y - lineOrigin.Y,
            point.Z - lineOrigin.Z
        );

        double t = toPoint.X * lineDir.X + toPoint.Y * lineDir.Y + toPoint.Z * lineDir.Z;

        Vector3D closest = new Vector3D(
            lineOrigin.X + t * lineDir.X,
            lineOrigin.Y + t * lineDir.Y,
            lineOrigin.Z + t * lineDir.Z
        );

        double dx = point.X - closest.X;
        double dy = point.Y - closest.Y;
        double dz = point.Z - closest.Z;

        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Computes the distance from a point to a finite line segment.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="a">The start of the segment.</param>
    /// <param name="b">The end of the segment.</param>
    /// <returns>The distance from the point to the segment.</returns>
    public static double PointToSegment(Point3D point, Point3D a, Point3D b)
    {
        Vector3D ab = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D ap = new Vector3D(point.X - a.X, point.Y - a.Y, point.Z - a.Z);

        double abLenSq = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;

        if (abLenSq < Tolerance)
        {
            return PointToPoint(point, a);
        }

        double t = (ap.X * ab.X + ap.Y * ab.Y + ap.Z * ab.Z) / abLenSq;

        if (t < 0) t = 0;
        if (t > 1) t = 1;

        Point3D closest = new Point3D(
            a.X + t * ab.X,
            a.Y + t * ab.Y,
            a.Z + t * ab.Z
        );

        return PointToPoint(point, closest);
    }

    /// <summary>
    /// Computes the distance from a point to a plane.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="plane">The plane.</param>
    /// <returns>The signed distance from the point to the plane.</returns>
    public static double PointToPlane(Point3D point, Plane3D plane)
    {
        return System.Math.Abs(
            plane.Normal.X * point.X +
            plane.Normal.Y * point.Y +
            plane.Normal.Z * point.Z +
            -(plane.Normal.X * plane.Point.X + plane.Normal.Y * plane.Point.Y + plane.Normal.Z * plane.Point.Z)
        );
    }

    /// <summary>
    /// Computes the distance from a point to a sphere.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="sphere">The sphere.</param>
    /// <returns>The distance from the point to the sphere surface (negative if inside).</returns>
    public static double PointToSphere(Point3D point, Sphere3D sphere)
    {
        double dx = point.X - sphere.Center.X;
        double dy = point.Y - sphere.Center.Y;
        double dz = point.Z - sphere.Center.Z;

        double dist = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

        return dist - sphere.Radius;
    }

    /// <summary>
    /// Computes the minimum distance between two line segments.
    /// </summary>
    /// <param name="a1">The start of the first segment.</param>
    /// <param name="a2">The end of the first segment.</param>
    /// <param name="b1">The start of the second segment.</param>
    /// <param name="b2">The end of the second segment.</param>
    /// <returns>A <see cref="DistanceResult"/> with the distance and closest points.</returns>
    public static DistanceResult SegmentToSegment(Point3D a1, Point3D a2, Point3D b1, Point3D b2)
    {
        Vector3D d1 = new Vector3D(a2.X - a1.X, a2.Y - a1.Y, a2.Z - a1.Z);
        Vector3D d2 = new Vector3D(b2.X - b1.X, b2.Y - b1.Y, b2.Z - b1.Z);
        Vector3D r = new Vector3D(a1.X - b1.X, a1.Y - b1.Y, a1.Z - b1.Z);

        double a = d1.X * d1.X + d1.Y * d1.Y + d1.Z * d1.Z;
        double e = d2.X * d2.X + d2.Y * d2.Y + d2.Z * d2.Z;
        double f = d2.X * r.X + d2.Y * r.Y + d2.Z * r.Z;

        if (a < Tolerance && e < Tolerance)
        {
            return new DistanceResult(PointToPoint(a1, b1), a1, b1);
        }

        double s, t;

        if (a < Tolerance)
        {
            s = 0;
            t = System.Math.Max(0, System.Math.Min(1, f / e));
        }
        else
        {
            double c = d1.X * r.X + d1.Y * r.Y + d1.Z * r.Z;

            if (e < Tolerance)
            {
                t = 0;
                s = System.Math.Max(0, System.Math.Min(1, -c / a));
            }
            else
            {
                double b = d1.X * d2.X + d1.Y * d2.Y + d1.Z * d2.Z;
                double denom = a * e - b * b;

                if (System.Math.Abs(denom) > Tolerance)
                {
                    s = System.Math.Max(0, System.Math.Min(1, (b * f - c * e) / denom));
                }
                else
                {
                    s = 0;
                }

                t = (b * s + f) / e;

                if (t < 0)
                {
                    t = 0;
                    s = System.Math.Max(0, System.Math.Min(1, -c / a));
                }
                else if (t > 1)
                {
                    t = 1;
                    s = System.Math.Max(0, System.Math.Min(1, (b - c) / a));
                }
            }
        }

        Point3D closestA = new Point3D(a1.X + s * d1.X, a1.Y + s * d1.Y, a1.Z + s * d1.Z);
        Point3D closestB = new Point3D(b1.X + t * d2.X, b1.Y + t * d2.Y, b1.Z + t * d2.Z);

        return new DistanceResult(PointToPoint(closestA, closestB), closestA, closestB);
    }

    /// <summary>
    /// Computes the minimum distance between two triangles using iterative closest point (ICP).
    /// </summary>
    /// <param name="a">The first triangle.</param>
    /// <param name="b">The second triangle.</param>
    /// <returns>A <see cref="DistanceResult"/> with the distance and closest points.</returns>
    public static DistanceResult TriangleToTriangle(Triangle3D a, Triangle3D b)
    {
        Point3D bestA = a.A;
        Point3D bestB = b.A;
        double bestDist = double.MaxValue;

        Point3D[] vertsA = { a.A, a.B, a.C };
        Point3D[] vertsB = { b.A, b.B, b.C };

        for (int i = 0; i < vertsA.Length; i++)
        {
            for (int j = 0; j < vertsB.Length; j++)
            {
                double dist = PointToPoint(vertsA[i], vertsB[j]);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestA = vertsA[i];
                    bestB = vertsB[j];
                }
            }
        }

        for (int i = 0; i < vertsA.Length; i++)
        {
            Point3D pt = vertsA[i];
            Point3D closest = ClosestPointOnTriangle(pt, b);
            double dist = PointToPoint(pt, closest);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestA = pt;
                bestB = closest;
            }
        }

        for (int j = 0; j < vertsB.Length; j++)
        {
            Point3D pt = vertsB[j];
            Point3D closest = ClosestPointOnTriangle(pt, a);
            double dist = PointToPoint(pt, closest);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestA = closest;
                bestB = pt;
            }
        }

        return new DistanceResult(bestDist, bestA, bestB);
    }

    /// <summary>
    /// Computes the distance from a point to a triangle mesh.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle indices (3 per triangle).</param>
    /// <returns>The minimum distance from the point to any triangle in the mesh.</returns>
    public static double PointToMesh(Point3D point, ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        double minDist = double.MaxValue;

        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            Triangle3D tri = new Triangle3D(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            );

            Point3D closest = ClosestPointOnTriangle(point, tri);
            double dist = PointToPoint(point, closest);

            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        return minDist;
    }

    private static Point3D ClosestPointOnTriangle(Point3D point, Triangle3D tri)
    {
        Vector3D ab = new Vector3D(tri.B.X - tri.A.X, tri.B.Y - tri.A.Y, tri.B.Z - tri.A.Z);
        Vector3D ac = new Vector3D(tri.C.X - tri.A.X, tri.C.Y - tri.A.Y, tri.C.Z - tri.A.Z);
        Vector3D ap = new Vector3D(point.X - tri.A.X, point.Y - tri.A.Y, point.Z - tri.A.Z);

        double d1 = ap.X * ab.X + ap.Y * ab.Y + ap.Z * ab.Z;
        double d2 = ap.X * ac.X + ap.Y * ac.Y + ap.Z * ac.Z;

        if (d1 <= 0 && d2 <= 0)
        {
            return tri.A;
        }

        Vector3D bp = new Vector3D(point.X - tri.B.X, point.Y - tri.B.Y, point.Z - tri.B.Z);
        double d3 = bp.X * ab.X + bp.Y * ab.Y + bp.Z * ab.Z;
        double d4 = bp.X * ac.X + bp.Y * ac.Y + bp.Z * ac.Z;

        if (d3 >= 0 && d4 <= d3)
        {
            return tri.B;
        }

        double vc = d1 * d4 - d3 * d2;
        double v = vc / (ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z - 2.0 * (ab.X * ac.X + ab.Y * ac.Y + ab.Z * ac.Z) + ac.X * ac.X + ac.Y * ac.Y + ac.Z * ac.Z);

        if (v < 0 && d1 < 0 && d3 < 0)
        {
            double w1 = d1 / (d1 - d3);
            return new Point3D(
                tri.A.X + w1 * ab.X,
                tri.A.Y + w1 * ab.Y,
                tri.A.Z + w1 * ab.Z
            );
        }

        Vector3D cp = new Vector3D(point.X - tri.C.X, point.Y - tri.C.Y, point.Z - tri.C.Z);
        double d5 = cp.X * ab.X + cp.Y * ab.Y + cp.Z * ab.Z;
        double d6 = cp.X * ac.X + cp.Y * ac.Y + cp.Z * ac.Z;

        if (d6 >= 0 && d5 <= d6)
        {
            return tri.C;
        }

        double vb = d5 * d2 - d1 * d6;
        double w = vb / (ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z - 2.0 * (ab.X * ac.X + ab.Y * ac.Y + ab.Z * ac.Z) + ac.X * ac.X + ac.Y * ac.Y + ac.Z * ac.Z);

        if (w < 0 && d2 < 0 && d6 < 0)
        {
            double w2 = d2 / (d2 - d6);
            return new Point3D(
                tri.A.X + w2 * ac.X,
                tri.A.Y + w2 * ac.Y,
                tri.A.Z + w2 * ac.Z
            );
        }

        double va = d3 * d6 - d5 * d4;
        double denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z + 2.0 * (ab.X * ac.X + ab.Y * ac.Y + ab.Z * ac.Z) + ac.X * ac.X + ac.Y * ac.Y + ac.Z * ac.Z;

        if (va < 0 && (d4 - d3) < 0 && (d5 - d6) < 0)
        {
            double w2 = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            Vector3D bc = new Vector3D(tri.C.X - tri.B.X, tri.C.Y - tri.B.Y, tri.C.Z - tri.B.Z);
            return new Point3D(
                tri.B.X + w2 * bc.X,
                tri.B.Y + w2 * bc.Y,
                tri.B.Z + w2 * bc.Z
            );
        }

        double denom2 = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z + 2.0 * (ab.X * ac.X + ab.Y * ac.Y + ab.Z * ac.Z) + ac.X * ac.X + ac.Y * ac.Y + ac.Z * ac.Z;

        if (System.Math.Abs(denom2) < Tolerance)
        {
            return tri.A;
        }

        double s = (va / denom2);
        double t = (vb / denom2);
        double u = 1.0 - s - t;

        return new Point3D(
            tri.A.X * u + tri.B.X * s + tri.C.X * t,
            tri.A.Y * u + tri.B.Y * s + tri.C.Y * t,
            tri.A.Z * u + tri.B.Z * s + tri.C.Z * t
        );
    }
}