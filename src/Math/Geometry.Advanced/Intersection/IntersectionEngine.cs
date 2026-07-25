using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Intersection;

/// <summary>
/// Represents the result of an intersection test between geometric primitives.
/// </summary>
/// <param name="Hit">Whether the intersection occurred.</param>
/// <param name="Point">The point of intersection in world space.</param>
/// <param name="Normal">The surface normal at the intersection point.</param>
/// <param name="Distance">The distance from the ray origin to the intersection point.</param>
public readonly record struct HitResult(bool Hit, Point3D Point, Vector3D Normal, double Distance);

/// <summary>
/// Provides static methods for computing intersections between various geometric primitives.
/// </summary>
public static class IntersectionEngine
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the intersection of a line with a plane.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <param name="plane">The plane to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectLinePlane(Point3D lineOrigin, Vector3D lineDir, Plane3D plane)
    {
        double denom = plane.Normal.X * lineDir.X + plane.Normal.Y * lineDir.Y + plane.Normal.Z * lineDir.Z;

        if (System.Math.Abs(denom) < Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        double t = -(
            plane.Normal.X * lineOrigin.X +
            plane.Normal.Y * lineOrigin.Y +
            plane.Normal.Z * lineOrigin.Z +
            -(plane.Normal.X * plane.Point.X + plane.Normal.Y * plane.Point.Y + plane.Normal.Z * plane.Point.Z)
        ) / denom;

        if (t < 0)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            lineOrigin.X + t * lineDir.X,
            lineOrigin.Y + t * lineDir.Y,
            lineOrigin.Z + t * lineDir.Z
        );

        return new HitResult(true, hitPoint, plane.Normal, t);
    }

    /// <summary>
    /// Computes the intersection of a line with a sphere.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <param name="sphere">The sphere to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectLineSphere(Point3D lineOrigin, Vector3D lineDir, Sphere3D sphere)
    {
        Vector3D oc = new Vector3D(
            lineOrigin.X - sphere.Center.X,
            lineOrigin.Y - sphere.Center.Y,
            lineOrigin.Z - sphere.Center.Z
        );

        double a = lineDir.X * lineDir.X + lineDir.Y * lineDir.Y + lineDir.Z * lineDir.Z;
        double b = 2.0 * (oc.X * lineDir.X + oc.Y * lineDir.Y + oc.Z * lineDir.Z);
        double c = oc.X * oc.X + oc.Y * oc.Y + oc.Z * oc.Z - sphere.Radius * sphere.Radius;

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        if (discriminant < 0)
        {
            discriminant = 0;
        }

        double sqrtDisc = System.Math.Sqrt(discriminant);
        double t = (-b - sqrtDisc) / (2.0 * a);

        if (t < 0)
        {
            t = (-b + sqrtDisc) / (2.0 * a);
        }

        if (t < 0)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            lineOrigin.X + t * lineDir.X,
            lineOrigin.Y + t * lineDir.Y,
            lineOrigin.Z + t * lineDir.Z
        );

        Vector3D normal = new Vector3D(
            (hitPoint.X - sphere.Center.X) / sphere.Radius,
            (hitPoint.Y - sphere.Center.Y) / sphere.Radius,
            (hitPoint.Z - sphere.Center.Z) / sphere.Radius
        );

        return new HitResult(true, hitPoint, normal, t);
    }

    /// <summary>
    /// Computes the intersection of a line with an infinite cylinder.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <param name="cylinder">The cylinder to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectLineCylinder(Point3D lineOrigin, Vector3D lineDir, Cylinder3D cylinder)
    {
        Vector3D oc = new Vector3D(
            lineOrigin.X - cylinder.Center.X,
            lineOrigin.Y - cylinder.Center.Y,
            lineOrigin.Z - cylinder.Center.Z
        );

        double radiusSq = cylinder.Radius * cylinder.Radius;
        Point3D cylBottom = cylinder.PointAt(0.0, 0.0);
        Point3D cylTop = cylinder.PointAt(1.0, 0.0);
        Vector3D cylinderAxis = new Vector3D(
            cylTop.X - cylBottom.X,
            cylTop.Y - cylBottom.Y,
            cylTop.Z - cylBottom.Z);
        double axisLen = System.Math.Sqrt(cylinderAxis.X * cylinderAxis.X + cylinderAxis.Y * cylinderAxis.Y + cylinderAxis.Z * cylinderAxis.Z);
        if (axisLen > Tolerance)
        {
            cylinderAxis = new Vector3D(cylinderAxis.X / axisLen, cylinderAxis.Y / axisLen, cylinderAxis.Z / axisLen);
        }
        double dirDotAxis = lineDir.X * cylinderAxis.X + lineDir.Y * cylinderAxis.Y + lineDir.Z * cylinderAxis.Z;
        double ocDotAxis = oc.X * cylinderAxis.X + oc.Y * cylinderAxis.Y + oc.Z * cylinderAxis.Z;

        Vector3D projDir = new Vector3D(
            lineDir.X - dirDotAxis * cylinderAxis.X,
            lineDir.Y - dirDotAxis * cylinderAxis.Y,
            lineDir.Z - dirDotAxis * cylinderAxis.Z
        );

        Vector3D projOc = new Vector3D(
            oc.X - ocDotAxis * cylinderAxis.X,
            oc.Y - ocDotAxis * cylinderAxis.Y,
            oc.Z - ocDotAxis * cylinderAxis.Z
        );

        double a = projDir.X * projDir.X + projDir.Y * projDir.Y + projDir.Z * projDir.Z;
        double b = 2.0 * (projOc.X * projDir.X + projOc.Y * projDir.Y + projOc.Z * projDir.Z);
        double c = projOc.X * projOc.X + projOc.Y * projOc.Y + projOc.Z * projOc.Z - radiusSq;

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        if (discriminant < 0)
        {
            discriminant = 0;
        }

        double sqrtDisc = System.Math.Sqrt(discriminant);
        double t = (-b - sqrtDisc) / (2.0 * a);

        if (t < 0)
        {
            t = (-b + sqrtDisc) / (2.0 * a);
        }

        if (t < 0)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            lineOrigin.X + t * lineDir.X,
            lineOrigin.Y + t * lineDir.Y,
            lineOrigin.Z + t * lineDir.Z
        );

        Vector3D toHit = new Vector3D(
            hitPoint.X - cylinder.Center.X,
            hitPoint.Y - cylinder.Center.Y,
            hitPoint.Z - cylinder.Center.Z
        );

        double axialComponent = toHit.X * cylinderAxis.X + toHit.Y * cylinderAxis.Y + toHit.Z * cylinderAxis.Z;

        Vector3D radial = new Vector3D(
            toHit.X - axialComponent * cylinderAxis.X,
            toHit.Y - axialComponent * cylinderAxis.Y,
            toHit.Z - axialComponent * cylinderAxis.Z
        );

        double radialLen = System.Math.Sqrt(radial.X * radial.X + radial.Y * radial.Y + radial.Z * radial.Z);

        if (radialLen < Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        Vector3D normal = new Vector3D(radial.X / radialLen, radial.Y / radialLen, radial.Z / radialLen);

        return new HitResult(true, hitPoint, normal, t);
    }

    /// <summary>
    /// Computes the intersection of a line with a cone.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <param name="cone">The cone to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectLineCone(Point3D lineOrigin, Vector3D lineDir, Cone3D cone)
    {
        Vector3D oc = new Vector3D(
            lineOrigin.X - cone.Apex.X,
            lineOrigin.Y - cone.Apex.Y,
            lineOrigin.Z - cone.Apex.Z
        );

        double coneHalfAngle = System.Math.Atan2(cone.Radius, cone.Height);
        double cosAngle = System.Math.Cos(coneHalfAngle);
        double cosAngleSq = cosAngle * cosAngle;
        double sinAngle = System.Math.Sin(coneHalfAngle);
        double sinAngleSq = sinAngle * sinAngle;

        double dirDotAxis = lineDir.X * cone.Axis.X + lineDir.Y * cone.Axis.Y + lineDir.Z * cone.Axis.Z;
        double ocDotAxis = oc.X * cone.Axis.X + oc.Y * cone.Axis.Y + oc.Z * cone.Axis.Z;

        double a = dirDotAxis * dirDotAxis - cosAngleSq * (lineDir.X * lineDir.X + lineDir.Y * lineDir.Y + lineDir.Z * lineDir.Z);
        double b = 2.0 * (dirDotAxis * ocDotAxis - cosAngleSq * (lineDir.X * oc.X + lineDir.Y * oc.Y + lineDir.Z * oc.Z));
        double c = ocDotAxis * ocDotAxis - cosAngleSq * (oc.X * oc.X + oc.Y * oc.Y + oc.Z * oc.Z);

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        if (discriminant < 0)
        {
            discriminant = 0;
        }

        double sqrtDisc = System.Math.Sqrt(discriminant);
        double t = (-b - sqrtDisc) / (2.0 * a);

        if (t < 0)
        {
            t = (-b + sqrtDisc) / (2.0 * a);
        }

        if (t < 0)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            lineOrigin.X + t * lineDir.X,
            lineOrigin.Y + t * lineDir.Y,
            lineOrigin.Z + t * lineDir.Z
        );

        Vector3D toHit = new Vector3D(
            hitPoint.X - cone.Apex.X,
            hitPoint.Y - cone.Apex.Y,
            hitPoint.Z - cone.Apex.Z
        );

        double axialComponent = toHit.X * cone.Axis.X + toHit.Y * cone.Axis.Y + toHit.Z * cone.Axis.Z;

        if (axialComponent < Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        double hitRadius = System.Math.Sqrt(toHit.X * toHit.X + toHit.Y * toHit.Y + toHit.Z * toHit.Z - axialComponent * axialComponent);

        Vector3D radial = new Vector3D(
            toHit.X - axialComponent * cone.Axis.X,
            toHit.Y - axialComponent * cone.Axis.Y,
            toHit.Z - axialComponent * cone.Axis.Z
        );

        Vector3D normal = new Vector3D(
            radial.X * cosAngle / hitRadius + cone.Axis.X * sinAngle,
            radial.Y * cosAngle / hitRadius + cone.Axis.Y * sinAngle,
            radial.Z * cosAngle / hitRadius + cone.Axis.Z * sinAngle
        );

        double normalLen = System.Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        normal = new Vector3D(normal.X / normalLen, normal.Y / normalLen, normal.Z / normalLen);

        return new HitResult(true, hitPoint, normal, t);
    }

    /// <summary>
    /// Computes the intersection of a line with a torus using Newton iteration on the quartic equation.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <param name="torus">The torus to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectLineTorus(Point3D lineOrigin, Vector3D lineDir, Torus3D torus)
    {
        Vector3D oc = new Vector3D(
            lineOrigin.X - torus.Center.X,
            lineOrigin.Y - torus.Center.Y,
            lineOrigin.Z - torus.Center.Z
        );

        double majorR = torus.MajorRadius;
        double minorR = torus.MinorRadius;
        double majorR2 = majorR * majorR;
        double minorR2 = minorR * minorR;

        double dirDotAxis = lineDir.X * torus.Axis.X + lineDir.Y * torus.Axis.Y + lineDir.Z * torus.Axis.Z;
        double ocDotAxis = oc.X * torus.Axis.X + oc.Y * torus.Axis.Y + oc.Z * torus.Axis.Z;

        Vector3D projDir = new Vector3D(
            lineDir.X - dirDotAxis * torus.Axis.X,
            lineDir.Y - dirDotAxis * torus.Axis.Y,
            lineDir.Z - dirDotAxis * torus.Axis.Z
        );

        Vector3D projOc = new Vector3D(
            oc.X - ocDotAxis * torus.Axis.X,
            oc.Y - ocDotAxis * torus.Axis.Y,
            oc.Z - ocDotAxis * torus.Axis.Z
        );

        double a = projDir.X * projDir.X + projDir.Y * projDir.Y + projDir.Z * projDir.Z;
        double b = 2.0 * (projOc.X * projDir.X + projOc.Y * projDir.Y + projOc.Z * projDir.Z);
        double c = projOc.X * projOc.X + projOc.Y * projOc.Y + projOc.Z * projOc.Z - majorR2 - minorR2 + dirDotAxis * dirDotAxis;
        double d = 2.0 * (majorR * (projOc.X * projDir.X + projOc.Y * projDir.Y + projOc.Z * projDir.Z) - ocDotAxis * dirDotAxis);
        double e = ocDotAxis * ocDotAxis + projOc.X * projOc.X + projOc.Y * projOc.Y + projOc.Z * projOc.Z - minorR2 + majorR2 - 2.0 * majorR * (projOc.X * torus.Axis.X + projOc.Y * torus.Axis.Y + projOc.Z * torus.Axis.Z);

        double bestT = -1;
        double bestDist = double.MaxValue;

        double[] candidates = { a > Tolerance ? (-b / (2.0 * a)) : 0 };

        for (int i = 0; i < 32; i++)
        {
            for (int ci = 0; ci < candidates.Length; ci++)
            {
                double t = candidates[ci];
                double t2 = t * t;
                double t3 = t2 * t;
                double t4 = t3 * t;

                double f = a * t4 + b * t3 + c * t2 + d * t + e;
                double fp = 4.0 * a * t3 + 3.0 * b * t2 + 2.0 * c * t + d;

                if (System.Math.Abs(fp) < Tolerance)
                {
                    continue;
                }

                double step = f / fp;
                t -= step;

                candidates[ci] = t;
            }
        }

        for (int ci = 0; ci < candidates.Length; ci++)
        {
            double t = candidates[ci];
            if (t < 0) continue;

            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;

            double f = a * t4 + b * t3 + c * t2 + d * t + e;

            if (System.Math.Abs(f) < Tolerance && t < bestT || bestT < 0)
            {
                bestT = t;
                bestDist = System.Math.Abs(f);
            }
        }

        if (bestT < 0)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            lineOrigin.X + bestT * lineDir.X,
            lineOrigin.Y + bestT * lineDir.Y,
            lineOrigin.Z + bestT * lineDir.Z
        );

        Vector3D toHit = new Vector3D(
            hitPoint.X - torus.Center.X,
            hitPoint.Y - torus.Center.Y,
            hitPoint.Z - torus.Center.Z
        );

        double axialComponent = toHit.X * torus.Axis.X + toHit.Y * torus.Axis.Y + toHit.Z * torus.Axis.Z;

        Vector3D projToHit = new Vector3D(
            toHit.X - axialComponent * torus.Axis.X,
            toHit.Y - axialComponent * torus.Axis.Y,
            toHit.Z - axialComponent * torus.Axis.Z
        );

        double projLen = System.Math.Sqrt(projToHit.X * projToHit.X + projToHit.Y * projToHit.Y + projToHit.Z * projToHit.Z);

        Vector3D normal;

        if (projLen < Tolerance)
        {
            normal = torus.Axis;
        }
        else
        {
            Vector3D ringDir = new Vector3D(projToHit.X / projLen, projToHit.Y / projLen, projToHit.Z / projLen);
            Vector3D ringCenter = new Vector3D(
                torus.Center.X + majorR * ringDir.X,
                torus.Center.Y + majorR * ringDir.Y,
                torus.Center.Z + majorR * ringDir.Z
            );

            Vector3D fromRing = new Vector3D(hitPoint.X - ringCenter.X, hitPoint.Y - ringCenter.Y, hitPoint.Z - ringCenter.Z);
            double fromRingLen = System.Math.Sqrt(fromRing.X * fromRing.X + fromRing.Y * fromRing.Y + fromRing.Z * fromRing.Z);

            if (fromRingLen < Tolerance)
            {
                normal = torus.Axis;
            }
            else
            {
                normal = new Vector3D(fromRing.X / fromRingLen, fromRing.Y / fromRingLen, fromRing.Z / fromRingLen);
            }
        }

        return new HitResult(true, hitPoint, normal, bestT);
    }

    /// <summary>
    /// Computes the intersection of a ray with a triangle using the Möller–Trumbore algorithm.
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="dir">The direction of the ray.</param>
    /// <param name="triangle">The triangle to intersect with.</param>
    /// <returns>A <see cref="HitResult"/> containing the intersection information.</returns>
    public static HitResult IntersectRayTriangle(Point3D origin, Vector3D dir, Triangle3D triangle)
    {
        Vector3D v0v1 = new Vector3D(
            triangle.B.X - triangle.A.X,
            triangle.B.Y - triangle.A.Y,
            triangle.B.Z - triangle.A.Z
        );

        Vector3D v0v2 = new Vector3D(
            triangle.C.X - triangle.A.X,
            triangle.C.Y - triangle.A.Y,
            triangle.C.Z - triangle.A.Z
        );

        Vector3D pvec = new Vector3D(
            dir.Y * v0v2.Z - dir.Z * v0v2.Y,
            dir.Z * v0v2.X - dir.X * v0v2.Z,
            dir.X * v0v2.Y - dir.Y * v0v2.X
        );

        double det = v0v1.X * pvec.X + v0v1.Y * pvec.Y + v0v1.Z * pvec.Z;

        if (System.Math.Abs(det) < Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        double invDet = 1.0 / det;

        Vector3D tvec = new Vector3D(
            origin.X - triangle.A.X,
            origin.Y - triangle.A.Y,
            origin.Z - triangle.A.Z
        );

        double u = (tvec.X * pvec.X + tvec.Y * pvec.Y + tvec.Z * pvec.Z) * invDet;

        if (u < -Tolerance || u > 1.0 + Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        Vector3D qvec = new Vector3D(
            tvec.Y * v0v1.Z - tvec.Z * v0v1.Y,
            tvec.Z * v0v1.X - tvec.X * v0v1.Z,
            tvec.X * v0v1.Y - tvec.Y * v0v1.X
        );

        double v = (dir.X * qvec.X + dir.Y * qvec.Y + dir.Z * qvec.Z) * invDet;

        if (v < -Tolerance || u + v > 1.0 + Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        double t = (v0v2.X * qvec.X + v0v2.Y * qvec.Y + v0v2.Z * qvec.Z) * invDet;

        if (t < Tolerance)
        {
            return new HitResult(false, default, default, 0);
        }

        Point3D hitPoint = new Point3D(
            origin.X + t * dir.X,
            origin.Y + t * dir.Y,
            origin.Z + t * dir.Z
        );

        Vector3D normal = new Vector3D(
            v0v1.Y * v0v2.Z - v0v1.Z * v0v2.Y,
            v0v1.Z * v0v2.X - v0v1.X * v0v2.Z,
            v0v1.X * v0v2.Y - v0v1.Y * v0v2.X
        );

        double normalLen = System.Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        normal = new Vector3D(normal.X / normalLen, normal.Y / normalLen, normal.Z / normalLen);

        return new HitResult(true, hitPoint, normal, t);
    }

    /// <summary>
    /// Computes the intersection of a ray with a polygon defined by vertices.
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="dir">The direction of the ray.</param>
    /// <param name="polygonVertices">The vertices of the polygon in order.</param>
    /// <returns>An immutable array of <see cref="HitResult"/> for each triangle intersection.</returns>
    public static ImmutableArray<HitResult> IntersectLinePolygon(Point3D origin, Vector3D dir, ImmutableArray<Point3D> polygonVertices)
    {
        var builder = ImmutableArray.CreateBuilder<HitResult>();

        if (polygonVertices.Length < 3)
        {
            return builder.ToImmutable();
        }

        for (int i = 1; i < polygonVertices.Length - 1; i++)
        {
            Triangle3D tri = new Triangle3D(polygonVertices[0], polygonVertices[i], polygonVertices[i + 1]);
            HitResult result = IntersectRayTriangle(origin, dir, tri);

            if (result.Hit)
            {
                builder.Add(result);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Performs brute-force ray-mesh intersection testing.
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="dir">The direction of the ray.</param>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle indices (3 per triangle).</param>
    /// <returns>An immutable array of <see cref="HitResult"/> for each triangle intersection.</returns>
    public static ImmutableArray<HitResult> IntersectRayMesh(Point3D origin, Vector3D dir, ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var builder = ImmutableArray.CreateBuilder<HitResult>();

        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            Triangle3D tri = new Triangle3D(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            );

            HitResult result = IntersectRayTriangle(origin, dir, tri);

            if (result.Hit)
            {
                builder.Add(result);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Computes the line of intersection between two planes.
    /// </summary>
    /// <param name="a">The first plane.</param>
    /// <param name="b">The second plane.</param>
    /// <param name="point">A point on the intersection line.</param>
    /// <param name="direction">The direction of the intersection line.</param>
    /// <returns>True if the planes are not parallel and intersect; otherwise, false.</returns>
    public static bool PlanePlane(Plane3D a, Plane3D b, out Point3D point, out Vector3D direction)
    {
        direction = new Vector3D(
            a.Normal.Y * b.Normal.Z - a.Normal.Z * b.Normal.Y,
            a.Normal.Z * b.Normal.X - a.Normal.X * b.Normal.Z,
            a.Normal.X * b.Normal.Y - a.Normal.Y * b.Normal.X
        );

        double dirLen = System.Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

        if (dirLen < Tolerance)
        {
            point = default;
            direction = default;
            return false;
        }

        direction = new Vector3D(direction.X / dirLen, direction.Y / dirLen, direction.Z / dirLen);

        Vector3D n1CrossN2 = new Vector3D(
            a.Normal.Y * b.Normal.Z - a.Normal.Z * b.Normal.Y,
            a.Normal.Z * b.Normal.X - a.Normal.X * b.Normal.Z,
            a.Normal.X * b.Normal.Y - a.Normal.Y * b.Normal.X
        );

        double n1CrossN2LenSq = n1CrossN2.X * n1CrossN2.X + n1CrossN2.Y * n1CrossN2.Y + n1CrossN2.Z * n1CrossN2.Z;

        if (n1CrossN2LenSq < Tolerance)
        {
            point = default;
            return false;
        }

        double d1 = -(a.Normal.X * a.Point.X + a.Normal.Y * a.Point.Y + a.Normal.Z * a.Point.Z);
        double d2 = -(b.Normal.X * b.Point.X + b.Normal.Y * b.Point.Y + b.Normal.Z * b.Point.Z);

        double n1DotN1 = a.Normal.X * a.Normal.X + a.Normal.Y * a.Normal.Y + a.Normal.Z * a.Normal.Z;
        double n2DotN2 = b.Normal.X * b.Normal.X + b.Normal.Y * b.Normal.Y + b.Normal.Z * b.Normal.Z;
        double n1DotN2 = a.Normal.X * b.Normal.X + a.Normal.Y * b.Normal.Y + a.Normal.Z * b.Normal.Z;

        double denom = n1DotN1 * n2DotN2 - n1DotN2 * n1DotN2;

        if (System.Math.Abs(denom) < Tolerance)
        {
            point = default;
            return false;
        }

        double c1 = (d1 * n2DotN2 - d2 * n1DotN2) / denom;
        double c2 = (d2 * n1DotN1 - d1 * n1DotN2) / denom;

        point = new Point3D(
            c1 * a.Normal.X + c2 * b.Normal.X,
            c1 * a.Normal.Y + c2 * b.Normal.Y,
            c1 * a.Normal.Z + c2 * b.Normal.Z
        );

        return true;
    }
}