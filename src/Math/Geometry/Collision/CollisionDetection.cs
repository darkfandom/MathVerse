using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Collision;

/// <summary>Provides static collision detection methods for various geometry types.</summary>
public static class CollisionDetection
{
    /// <summary>Casts a ray against a triangle and returns the closest hit.</summary>
    public static (bool hit, Point3D point, double distance) RayTriangle(Picking.Ray ray, Triangle3D tri)
    {
        var (hit, point) = tri.Intersect(new Line3D(ray.Origin, ray.PointAt(1000)));
        double dist = hit ? ray.Origin.DistanceTo(point) : double.MaxValue;
        return (hit, point, dist);
    }

    /// <summary>Casts a ray against a sphere.</summary>
    public static (bool hit, Point3D point, double distance) RaySphere(Picking.Ray ray, Sphere3D sphere)
    {
        Vector3D oc = new(ray.Origin.X - sphere.Center.X, ray.Origin.Y - sphere.Center.Y, ray.Origin.Z - sphere.Center.Z);
        double a = ray.Direction.Dot(ray.Direction);
        double b = 2.0 * oc.Dot(ray.Direction);
        double c = oc.Dot(oc) - sphere.Radius * sphere.Radius;
        double disc = b * b - 4.0 * a * c;
        if (disc < 0) return (false, Point3D.Origin, double.MaxValue);

        double sqrtDisc = System.Math.Sqrt(disc);
        double t = (-b - sqrtDisc) / (2.0 * a);
        if (t < 0) t = (-b + sqrtDisc) / (2.0 * a);
        if (t < 0) return (false, Point3D.Origin, double.MaxValue);

        Point3D point = ray.PointAt(t);
        return (true, point, t);
    }

    /// <summary>Casts a ray against an AABB.</summary>
    public static (bool hit, Point3D point, double distance) RayAABB(Picking.Ray ray, BoundingBox3D box)
    {
        double tmin = double.MinValue, tmax = double.MaxValue;
        Point3D invDir = new(1.0 / ray.Direction.X, 1.0 / ray.Direction.Y, 1.0 / ray.Direction.Z);

        for (int i = 0; i < 3; i++)
        {
            double origin = i == 0 ? ray.Origin.X : i == 1 ? ray.Origin.Y : ray.Origin.Z;
            double dir = i == 0 ? ray.Direction.X : i == 1 ? ray.Direction.Y : ray.Direction.Z;
            double bmin = i == 0 ? box.Min.X : i == 1 ? box.Min.Y : box.Min.Z;
            double bmax = i == 0 ? box.Max.X : i == 1 ? box.Max.Y : box.Max.Z;

            double invD = i == 0 ? invDir.X : i == 1 ? invDir.Y : invDir.Z;
            double t1 = (bmin - origin) * invD;
            double t2 = (bmax - origin) * invD;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = System.Math.Max(tmin, t1);
            tmax = System.Math.Min(tmax, t2);
            if (tmin > tmax) return (false, Point3D.Origin, double.MaxValue);
        }

        if (tmax < 0) return (false, Point3D.Origin, double.MaxValue);
        double t = tmin >= 0 ? tmin : tmax;
        return (true, ray.PointAt(t), t);
    }

    /// <summary>Casts a ray against a plane.</summary>
    public static (bool hit, Point3D point, double distance) RayPlane(Picking.Ray ray, Plane3D plane)
    {
        double denom = plane.Normal.Dot(ray.Direction);
        if (System.Math.Abs(denom) < 1e-15) return (false, Point3D.Origin, double.MaxValue);

        Vector3D toOrigin = new(plane.Point.X - ray.Origin.X, plane.Point.Y - ray.Origin.Y, plane.Point.Z - ray.Origin.Z);
        double t = toOrigin.Dot(plane.Normal) / denom;
        if (t < 0) return (false, Point3D.Origin, double.MaxValue);
        return (true, ray.PointAt(t), t);
    }

    /// <summary>Tests whether two spheres intersect.</summary>
    public static bool SphereSphere(Sphere3D a, Sphere3D b)
    {
        double distSq = a.Center.DistanceSquaredTo(b.Center);
        double rSum = a.Radius + b.Radius;
        return distSq <= rSum * rSum;
    }

    /// <summary>Tests whether two AABBs intersect.</summary>
    public static bool AABBAABB(BoundingBox3D a, BoundingBox3D b) => a.Intersects(b);

    /// <summary>Tests whether an AABB intersects a sphere.</summary>
    public static bool AABBSphere(BoundingBox3D box, Sphere3D sphere)
    {
        Point3D closest = new(
            System.Math.Max(box.Min.X, System.Math.Min(sphere.Center.X, box.Max.X)),
            System.Math.Max(box.Min.Y, System.Math.Min(sphere.Center.Y, box.Max.Y)),
            System.Math.Max(box.Min.Z, System.Math.Min(sphere.Center.Z, box.Max.Z)));
        return closest.DistanceSquaredTo(sphere.Center) <= sphere.Radius * sphere.Radius;
    }

    /// <summary>Tests whether a capsule intersects a sphere.</summary>
    public static bool CapsuleSphere(Capsule3D capsule, Sphere3D sphere)
    {
        // Find closest point on capsule axis segment to sphere center
        Point3D closestOnAxis = ClosestPointOnSegment(capsule.A, capsule.B, sphere.Center);
        Vector3D diff = new(closestOnAxis.X - sphere.Center.X, closestOnAxis.Y - sphere.Center.Y, closestOnAxis.Z - sphere.Center.Z);
        double distToAxis = diff.Length;
        // Check if distance to axis is within capsule radius + sphere radius
        return distToAxis <= capsule.Radius + sphere.Radius;
    }

    /// <summary>Tests whether two capsules intersect.</summary>
    public static bool CapsuleCapsule(Capsule3D a, Capsule3D b)
    {
        Point3D cpA = ClosestPointOnSegment(a.A, a.B, b.A);
        Point3D cpB = ClosestPointOnSegment(b.A, b.B, cpA);
        cpA = ClosestPointOnSegment(a.A, a.B, cpB);
        double dist = cpA.DistanceTo(cpB);
        return dist <= a.Radius + b.Radius;
    }

    /// <summary>Tests whether an OBB intersects a sphere.</summary>
    public static bool OBBSphere(OBB3D obb, Sphere3D sphere)
    {
        Vector3D d = new(sphere.Center.X - obb.Center.X, sphere.Center.Y - obb.Center.Y, sphere.Center.Z - obb.Center.Z);
        double dx = System.Math.Abs(d.Dot(obb.AxisX));
        double dy = System.Math.Abs(d.Dot(obb.AxisY));
        double dz = System.Math.Abs(d.Dot(obb.AxisZ));

        double closestX = System.Math.Max(-obb.ExtentX, System.Math.Min(dx, obb.ExtentX));
        double closestY = System.Math.Max(-obb.ExtentY, System.Math.Min(dy, obb.ExtentY));
        double closestZ = System.Math.Max(-obb.ExtentZ, System.Math.Min(dz, obb.ExtentZ));

        double distSq = (dx - closestX) * (dx - closestX) + (dy - closestY) * (dy - closestY) + (dz - closestZ) * (dz - closestZ);
        return distSq <= sphere.Radius * sphere.Radius;
    }

    /// <summary>Continuous collision detection between two spheres along velocity vectors.</summary>
    public static (bool willCollide, double timeOfImpact) ContinuousSphereSphere(
        Sphere3D a, Vector3D velA, Sphere3D b, Vector3D velB, double maxTime)
    {
        Vector3D relVel = new(velA.X - velB.X, velA.Y - velB.Y, velA.Z - velB.Z);
        Vector3D delta = new(a.Center.X - b.Center.X, a.Center.Y - b.Center.Y, a.Center.Z - b.Center.Z);
        double rSum = a.Radius + b.Radius;

        double aa = relVel.Dot(relVel);
        double bb = 2.0 * delta.Dot(relVel);
        double cc = delta.Dot(delta) - rSum * rSum;

        if (aa < 1e-30)
        {
            return (cc <= 0, 0);
        }

        double disc = bb * bb - 4.0 * aa * cc;
        if (disc < 0) return (false, double.MaxValue);

        double sqrtDisc = System.Math.Sqrt(disc);
        double t = (-bb - sqrtDisc) / (2.0 * aa);

        if (t < 0) t = (-bb + sqrtDisc) / (2.0 * aa);
        if (t < 0 || t > maxTime) return (false, double.MaxValue);

        return (true, t);
    }

    private static Point3D ClosestPointOnSegment(Point3D a, Point3D b, Point3D p)
    {
        Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D ap = new(p.X - a.X, p.Y - a.Y, p.Z - a.Z);
        double t = ap.Dot(ab) / ab.LengthSquared;
        t = System.Math.Max(0, System.Math.Min(1, t));
        return new Point3D(a.X + ab.X * t, a.Y + ab.Y * t, a.Z + ab.Z * t);
    }
}
