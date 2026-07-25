using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Represents the result of a narrow-phase collision test.
/// </summary>
/// <param name="Colliding">Whether the objects are colliding.</param>
/// <param name="ContactPoint">The point of contact.</param>
/// <param name="ContactNormal">The collision normal direction.</param>
/// <param name="PenetrationDepth">The penetration depth.</param>
public readonly record struct NarrowPhaseResult(bool Colliding, Point3D ContactPoint, Vector3D ContactNormal, double PenetrationDepth);

/// <summary>
/// Provides narrow-phase collision detection methods for precise intersection tests.
/// </summary>
public static class NarrowPhase
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Tests two spheres for collision.
    /// </summary>
    /// <param name="a">The first sphere.</param>
    /// <param name="b">The second sphere.</param>
    /// <returns>A <see cref="NarrowPhaseResult"/> if colliding; otherwise, null.</returns>
    public static NarrowPhaseResult? SphereVsSphere(Sphere3D a, Sphere3D b)
    {
        Vector3D d = new Vector3D(
            b.Center.X - a.Center.X,
            b.Center.Y - a.Center.Y,
            b.Center.Z - a.Center.Z
        );

        double distSq = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
        double radiiSum = a.Radius + b.Radius;

        if (distSq >= radiiSum * radiiSum)
        {
            return null;
        }

        double dist = System.Math.Sqrt(distSq);

        if (dist < Tolerance)
        {
            return new NarrowPhaseResult(
                true,
                a.Center,
                new Vector3D(1, 0, 0),
                radiiSum
            );
        }

        Vector3D normal = new Vector3D(d.X / dist, d.Y / dist, d.Z / dist);
        double penetration = radiiSum - dist;

        Point3D contact = new Point3D(
            a.Center.X + normal.X * (a.Radius - penetration * 0.5),
            a.Center.Y + normal.Y * (a.Radius - penetration * 0.5),
            a.Center.Z + normal.Z * (a.Radius - penetration * 0.5)
        );

        return new NarrowPhaseResult(true, contact, normal, penetration);
    }

    /// <summary>
    /// Tests two axis-aligned bounding boxes for collision.
    /// </summary>
    /// <param name="a">The first bounding box.</param>
    /// <param name="b">The second bounding box.</param>
    /// <returns>A <see cref="NarrowPhaseResult"/> if colliding; otherwise, null.</returns>
    public static NarrowPhaseResult? AABBBoundingBox(BoundingBox3D a, BoundingBox3D b)
    {
        if (a.Min.X > b.Max.X || a.Max.X < b.Min.X ||
            a.Min.Y > b.Max.Y || a.Max.Y < b.Min.Y ||
            a.Min.Z > b.Max.Z || a.Max.Z < b.Min.Z)
        {
            return null;
        }

        Vector3D aCenter = new Vector3D(
            (a.Min.X + a.Max.X) * 0.5,
            (a.Min.Y + a.Max.Y) * 0.5,
            (a.Min.Z + a.Max.Z) * 0.5
        );

        Vector3D bCenter = new Vector3D(
            (b.Min.X + b.Max.X) * 0.5,
            (b.Min.Y + b.Max.Y) * 0.5,
            (b.Min.Z + b.Max.Z) * 0.5
        );

        Vector3D d = new Vector3D(
            bCenter.X - aCenter.X,
            bCenter.Y - aCenter.Y,
            bCenter.Z - aCenter.Z
        );

        Vector3D aHalf = new Vector3D(
            (a.Max.X - a.Min.X) * 0.5,
            (a.Max.Y - a.Min.Y) * 0.5,
            (a.Max.Z - a.Min.Z) * 0.5
        );

        Vector3D bHalf = new Vector3D(
            (b.Max.X - b.Min.X) * 0.5,
            (b.Max.Y - b.Min.Y) * 0.5,
            (b.Max.Z - b.Min.Z) * 0.5
        );

        double overlapX = aHalf.X + bHalf.X - System.Math.Abs(d.X);
        double overlapY = aHalf.Y + bHalf.Y - System.Math.Abs(d.Y);
        double overlapZ = aHalf.Z + bHalf.Z - System.Math.Abs(d.Z);

        Vector3D normal;
        double depth;

        if (overlapX <= overlapY && overlapX <= overlapZ)
        {
            normal = new Vector3D(d.X < 0 ? -1 : 1, 0, 0);
            depth = overlapX;
        }
        else if (overlapY <= overlapZ)
        {
            normal = new Vector3D(0, d.Y < 0 ? -1 : 1, 0);
            depth = overlapY;
        }
        else
        {
            normal = new Vector3D(0, 0, d.Z < 0 ? -1 : 1);
            depth = overlapZ;
        }

        Point3D contact = new Point3D(
            aCenter.X + normal.X * (aHalf.X - depth * 0.5),
            aCenter.Y + normal.Y * (aHalf.Y - depth * 0.5),
            aCenter.Z + normal.Z * (aHalf.Z - depth * 0.5)
        );

        return new NarrowPhaseResult(true, contact, normal, depth);
    }

    /// <summary>
    /// Tests two triangles for collision.
    /// </summary>
    /// <param name="a">The first triangle.</param>
    /// <param name="b">The second triangle.</param>
    /// <returns>A <see cref="NarrowPhaseResult"/> if colliding; otherwise, null.</returns>
    public static NarrowPhaseResult? TriangleVsTriangle(Triangle3D a, Triangle3D b)
    {
        Vector3D n1 = ComputeTriangleNormal(a);
        Vector3D n2 = ComputeTriangleNormal(b);

        Vector3D[] axes = new Vector3D[]
        {
            n1,
            n2,
            ComputeEdgeAxis(a.A, a.B, b.A, b.B),
            ComputeEdgeAxis(a.A, a.B, b.B, b.C),
            ComputeEdgeAxis(a.A, a.B, b.C, b.A),
            ComputeEdgeAxis(a.B, a.C, b.A, b.B),
            ComputeEdgeAxis(a.B, a.C, b.B, b.C),
            ComputeEdgeAxis(a.B, a.C, b.C, b.A),
            ComputeEdgeAxis(a.C, a.A, b.A, b.B),
            ComputeEdgeAxis(a.C, a.A, b.B, b.C),
            ComputeEdgeAxis(a.C, a.A, b.C, b.A)
        };

        double minOverlap = double.MaxValue;
        Vector3D minAxis = default;

        Point3D[] vertsA = { a.A, a.B, a.C };
        Point3D[] vertsB = { b.A, b.B, b.C };

        for (int i = 0; i < axes.Length; i++)
        {
            Vector3D axis = axes[i];
            double axisLen = System.Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);

            if (axisLen < Tolerance)
            {
                continue;
            }

            axis = new Vector3D(axis.X / axisLen, axis.Y / axisLen, axis.Z / axisLen);

            double minA = vertsA[0].X * axis.X + vertsA[0].Y * axis.Y + vertsA[0].Z * axis.Z;
            double maxA = minA;

            for (int j = 1; j < 3; j++)
            {
                double proj = vertsA[j].X * axis.X + vertsA[j].Y * axis.Y + vertsA[j].Z * axis.Z;
                if (proj < minA) minA = proj;
                if (proj > maxA) maxA = proj;
            }

            double minB = vertsB[0].X * axis.X + vertsB[0].Y * axis.Y + vertsB[0].Z * axis.Z;
            double maxB = minB;

            for (int j = 1; j < 3; j++)
            {
                double proj = vertsB[j].X * axis.X + vertsB[j].Y * axis.Y + vertsB[j].Z * axis.Z;
                if (proj < minB) minB = proj;
                if (proj > maxB) maxB = proj;
            }

            double overlap = System.Math.Min(maxA, maxB) - System.Math.Max(minA, minB);

            if (overlap < Tolerance)
            {
                return null;
            }

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                minAxis = axis;
            }
        }

        Vector3D d = new Vector3D(
            b.A.X - a.A.X,
            b.A.Y - a.A.Y,
            b.A.Z - a.A.Z
        );

        if (d.X * minAxis.X + d.Y * minAxis.Y + d.Z * minAxis.Z < 0)
        {
            minAxis = new Vector3D(-minAxis.X, -minAxis.Y, -minAxis.Z);
        }

        Point3D contact = new Point3D(
            (a.A.X + a.B.X + a.C.X) * 0.3333333333333333 +
            (b.A.X + b.B.X + b.C.X) * 0.1666666666666667,
            (a.A.Y + a.B.Y + a.C.Y) * 0.3333333333333333 +
            (b.A.Y + b.B.Y + b.C.Y) * 0.1666666666666667,
            (a.A.Z + a.B.Z + a.C.Z) * 0.3333333333333333 +
            (b.A.Z + b.B.Z + b.C.Z) * 0.1666666666666667
        );

        return new NarrowPhaseResult(true, contact, minAxis, minOverlap);
    }

    /// <summary>
    /// Tests a triangle mesh against a sphere for collision.
    /// </summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle indices (3 per triangle).</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>A <see cref="NarrowPhaseResult"/> if colliding; otherwise, null.</returns>
    public static NarrowPhaseResult? MeshVsSphere(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, Sphere3D sphere)
    {
        double minDist = double.MaxValue;
        Point3D closestPoint = default;
        Vector3D closestNormal = default;
        int closestTriIdx = -1;

        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            Triangle3D tri = new Triangle3D(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            );

            Point3D cp = ClosestPointOnTriangle(sphere.Center, tri);
            double dist = MathVerse.Math.Geometry.Advanced.Distance.DistanceEngine.PointToPoint(sphere.Center, cp);

            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = cp;
                closestTriIdx = i;
            }
        }

        if (minDist > sphere.Radius)
        {
            return null;
        }

        Vector3D toCenter = new Vector3D(
            sphere.Center.X - closestPoint.X,
            sphere.Center.Y - closestPoint.Y,
            sphere.Center.Z - closestPoint.Z
        );

        double toCenterLen = System.Math.Sqrt(toCenter.X * toCenter.X + toCenter.Y * toCenter.Y + toCenter.Z * toCenter.Z);

        if (toCenterLen < Tolerance)
        {
            Triangle3D tri = new Triangle3D(
                vertices[indices[closestTriIdx]],
                vertices[indices[closestTriIdx + 1]],
                vertices[indices[closestTriIdx + 2]]
            );
            closestNormal = ComputeTriangleNormal(tri);
        }
        else
        {
            closestNormal = new Vector3D(toCenter.X / toCenterLen, toCenter.Y / toCenterLen, toCenter.Z / toCenterLen);
        }

        double penetration = sphere.Radius - minDist;

        Point3D contact = new Point3D(
            closestPoint.X + closestNormal.X * minDist * 0.5,
            closestPoint.Y + closestNormal.Y * minDist * 0.5,
            closestPoint.Z + closestNormal.Z * minDist * 0.5
        );

        return new NarrowPhaseResult(true, contact, closestNormal, penetration);
    }

    private static Vector3D ComputeTriangleNormal(Triangle3D tri)
    {
        Vector3D v1 = new Vector3D(tri.B.X - tri.A.X, tri.B.Y - tri.A.Y, tri.B.Z - tri.A.Z);
        Vector3D v2 = new Vector3D(tri.C.X - tri.A.X, tri.C.Y - tri.A.Y, tri.C.Z - tri.A.Z);

        Vector3D n = new Vector3D(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X
        );

        double len = System.Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);

        if (len < Tolerance)
        {
            return default;
        }

        return new Vector3D(n.X / len, n.Y / len, n.Z / len);
    }

    private static Vector3D ComputeEdgeAxis(Point3D a1, Point3D a2, Point3D b1, Point3D b2)
    {
        Vector3D e1 = new Vector3D(a2.X - a1.X, a2.Y - a1.Y, a2.Z - a1.Z);
        Vector3D e2 = new Vector3D(b2.X - b1.X, b2.Y - b1.Y, b2.Z - b1.Z);

        return new Vector3D(
            e1.Y * e2.Z - e1.Z * e2.Y,
            e1.Z * e2.X - e1.X * e2.Z,
            e1.X * e2.Y - e1.Y * e2.X
        );
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

        if (System.Math.Abs(denom) < Tolerance)
        {
            return tri.A;
        }

        double s = va / denom;
        double t = vb / denom;
        double u = 1.0 - s - t;

        return new Point3D(
            tri.A.X * u + tri.B.X * s + tri.C.X * t,
            tri.A.Y * u + tri.B.Y * s + tri.C.Y * t,
            tri.A.Z * u + tri.B.Z * s + tri.C.Z * t
        );
    }
}