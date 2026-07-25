namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;

/// <summary>Represents a ray for hit testing.</summary>
public sealed class Ray
{
    /// <summary>Gets the ray origin.</summary>
    public Vector3 Origin { get; init; }

    /// <summary>Gets the normalized ray direction.</summary>
    public Vector3 Direction { get; init; }

    /// <summary>Creates a ray from origin and direction.</summary>
    /// <param name="origin">The ray origin.</param>
    /// <param name="direction">The ray direction (will be normalized).</param>
    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction.LengthSquared() > 0 ? Vector3.Normalize(direction) : Vector3.UnitZ;
    }
}

/// <summary>Represents the result of a hit test.</summary>
public sealed class HitTestResult
{
    /// <summary>Gets whether the ray hit the object.</summary>
    public bool Hit { get; init; }

    /// <summary>Gets the distance from the ray origin to the hit point.</summary>
    public float Distance { get; init; }

    /// <summary>Gets the hit point in world coordinates.</summary>
    public Vector3 Point { get; init; }

    /// <summary>Gets the ID of the hit object, if available.</summary>
    public string? ObjectId { get; init; }

    /// <summary>Gets the surface normal at the hit point.</summary>
    public Vector3 Normal { get; init; }

    /// <summary>A result indicating no hit.</summary>
    public static HitTestResult Miss => new HitTestResult { Hit = false, Distance = float.MaxValue };
}

/// <summary>Axis-aligned bounding box for 3D hit testing.</summary>
public sealed class BoundingBox
{
    /// <summary>Gets or sets the minimum corner.</summary>
    public Vector3 Min { get; set; }

    /// <summary>Gets or sets the maximum corner.</summary>
    public Vector3 Max { get; set; }

    /// <summary>Gets the center of the bounding box.</summary>
    public Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>Gets the size of the bounding box.</summary>
    public Vector3 Size => Max - Min;
}

/// <summary>Performs ray-based hit testing against various primitives.</summary>
public sealed class HitTester
{
    private const float Epsilon = 1e-6f;

    /// <summary>Tests a ray against a sphere.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="center">The sphere center.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <returns>The hit test result.</returns>
    public HitTestResult RaySphere(Ray ray, Vector3 center, float radius)
    {
        Vector3 oc = ray.Origin - center;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = 2.0f * Vector3.Dot(oc, ray.Direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;

        float discriminant = b * b - 4.0f * a * c;

        if (discriminant < 0)
            return HitTestResult.Miss;

        float sqrtDisc = (float)System.Math.Sqrt(discriminant);
        float t = (-b - sqrtDisc) / (2.0f * a);

        if (t < 0)
        {
            t = (-b + sqrtDisc) / (2.0f * a);
            if (t < 0)
                return HitTestResult.Miss;
        }

        Vector3 point = ray.Origin + ray.Direction * t;
        Vector3 normal = Vector3.Normalize(point - center);

        return new HitTestResult
        {
            Hit = true,
            Distance = t,
            Point = point,
            Normal = normal
        };
    }

    /// <summary>Tests a ray against an axis-aligned bounding box.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="box">The bounding box.</param>
    /// <returns>The hit test result.</returns>
    public HitTestResult RayAABB(Ray ray, BoundingBox box)
    {
        float tmin = float.MinValue;
        float tmax = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            float orig = GetComponent(ray.Origin, i);
            float dir = GetComponent(ray.Direction, i);
            float bmin = GetComponent(box.Min, i);
            float bmax = GetComponent(box.Max, i);

            if (System.Math.Abs(dir) < Epsilon)
            {
                if (orig < bmin || orig > bmax)
                    return HitTestResult.Miss;
            }
            else
            {
                float t1 = (bmin - orig) / dir;
                float t2 = (bmax - orig) / dir;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tmin = System.Math.Max(tmin, t1);
                tmax = System.Math.Min(tmax, t2);

                if (tmin > tmax)
                    return HitTestResult.Miss;
            }
        }

        if (tmax < 0)
            return HitTestResult.Miss;

        float t = tmin >= 0 ? tmin : tmax;
        Vector3 point = ray.Origin + ray.Direction * t;
        Vector3 normal = ComputeAABBNormal(point, box);

        return new HitTestResult
        {
            Hit = true,
            Distance = t,
            Point = point,
            Normal = normal
        };
    }

    /// <summary>Tests a ray against a triangle using Moller-Trumbore algorithm.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="v0">First vertex of the triangle.</param>
    /// <param name="v1">Second vertex of the triangle.</param>
    /// <param name="v2">Third vertex of the triangle.</param>
    /// <returns>The hit test result.</returns>
    public HitTestResult RayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        return MollerTrumboreIntersect(ray, v0, v1, v2);
    }

    /// <summary>Tests a ray against a plane.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="plane">The plane to test against.</param>
    /// <returns>The hit test result.</returns>
    public HitTestResult RayPlane(Ray ray, System.Numerics.Plane plane)
    {
        float denom = Vector3.Dot(plane.Normal, ray.Direction);

        if (System.Math.Abs(denom) < Epsilon)
            return HitTestResult.Miss;

        float t = -(Vector3.Dot(plane.Normal, ray.Origin) + plane.D) / denom;

        if (t < 0)
            return HitTestResult.Miss;

        Vector3 point = ray.Origin + ray.Direction * t;

        return new HitTestResult
        {
            Hit = true,
            Distance = t,
            Point = point,
            Normal = plane.Normal
        };
    }

    /// <summary>Tests a ray against a cylinder.</summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="baseCenter">The cylinder base center.</param>
    /// <param name="axis">The cylinder axis (normalized).</param>
    /// <param name="radius">The cylinder radius.</param>
    /// <param name="height">The cylinder height.</param>
    /// <returns>The hit test result.</returns>
    public HitTestResult RayCylinder(Ray ray, Vector3 baseCenter, Vector3 axis, float radius, float height)
    {
        Vector3 oc = ray.Origin - baseCenter;

        float axisDotDir = Vector3.Dot(axis, ray.Direction);
        float axisDotOC = Vector3.Dot(axis, oc);

        Vector3 projDir = ray.Direction - axis * axisDotDir;
        Vector3 projOC = oc - axis * axisDotOC;

        float a = Vector3.Dot(projDir, projDir);
        float b = 2.0f * Vector3.Dot(projDir, projOC);
        float c = Vector3.Dot(projOC, projOC) - radius * radius;

        float discriminant = b * b - 4.0f * a * c;

        if (discriminant < 0)
            return HitTestResult.Miss;

        float sqrtDisc = (float)System.Math.Sqrt(discriminant);
        float t = (-b - sqrtDisc) / (2.0f * a);

        if (t < 0)
        {
            t = (-b + sqrtDisc) / (2.0f * a);
            if (t < 0)
                return HitTestResult.Miss;
        }

        Vector3 point = ray.Origin + ray.Direction * t;
        float h = Vector3.Dot(point - baseCenter, axis);

        if (h < 0 || h > height)
            return HitTestResult.Miss;

        Vector3 normal = Vector3.Normalize(point - baseCenter - axis * h);

        return new HitTestResult
        {
            Hit = true,
            Distance = t,
            Point = point,
            Normal = normal
        };
    }

    /// <summary>Moller-Trumbore ray-triangle intersection algorithm.</summary>
    /// <param name="ray">The ray.</param>
    /// <param name="v0">First vertex.</param>
    /// <param name="v1">Second vertex.</param>
    /// <param name="v2">Third vertex.</param>
    /// <returns>The hit test result.</returns>
    public static HitTestResult MollerTrumboreIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;

        Vector3 h = Vector3.Cross(ray.Direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (a > -Epsilon && a < Epsilon)
            return HitTestResult.Miss;

        float f = 1.0f / a;
        Vector3 s = ray.Origin - v0;
        float u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return HitTestResult.Miss;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(ray.Direction, q);

        if (v < 0.0f || u + v > 1.0f)
            return HitTestResult.Miss;

        float t = f * Vector3.Dot(edge2, q);

        if (t > Epsilon)
        {
            Vector3 point = ray.Origin + ray.Direction * t;
            Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            if (Vector3.Dot(ray.Direction, normal) > 0)
                normal = -normal;

            return new HitTestResult
            {
                Hit = true,
                Distance = t,
                Point = point,
                Normal = normal
            };
        }

        return HitTestResult.Miss;
    }

    private static float GetComponent(Vector3 v, int index)
    {
        return index switch
        {
            0 => v.X,
            1 => v.Y,
            2 => v.Z,
            _ => 0
        };
    }

    private static Vector3 ComputeAABBNormal(Vector3 point, BoundingBox box)
    {
        Vector3 center = box.Center;
        Vector3 halfSize = box.Size * 0.5f;

        float dx = System.Math.Abs(point.X - center.X) - halfSize.X;
        float dy = System.Math.Abs(point.Y - center.Y) - halfSize.Y;
        float dz = System.Math.Abs(point.Z - center.Z) - halfSize.Z;

        if (dx >= dy && dx >= dz)
            return new Vector3(point.X > center.X ? 1.0f : -1.0f, 0, 0);
        if (dy >= dz)
            return new Vector3(0, point.Y > center.Y ? 1.0f : -1.0f, 0);

        return new Vector3(0, 0, point.Z > center.Z ? 1.0f : -1.0f);
    }
}
