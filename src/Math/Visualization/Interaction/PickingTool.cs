namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Provides point picking on geometry surfaces.</summary>
public sealed class PickingTool
{
    private const float Epsilon = 1e-6f;

    /// <summary>Picks the nearest point on a mesh surface from a ray.</summary>
    /// <param name="vertices">The mesh vertices.</param>
    /// <param name="faces">The mesh face indices (triangles).</param>
    /// <param name="ray">The picking ray.</param>
    /// <returns>The nearest intersection point, or null if no hit.</returns>
    public static Vector3? PickOnSurface(double[][] vertices, int[][] faces, Ray ray)
    {
        if (vertices == null || faces == null)
            return null;

        Vector3? closestPoint = null;
        float closestDistance = float.MaxValue;

        foreach (var face in faces)
        {
            if (face == null || face.Length < 3)
                continue;

            for (int i = 0; i < face.Length - 2; i++)
            {
                int i0 = face[0];
                int i1 = face[i + 1];
                int i2 = face[i + 2];

                if (i0 < 0 || i0 >= vertices.Length ||
                    i1 < 0 || i1 >= vertices.Length ||
                    i2 < 0 || i2 >= vertices.Length)
                    continue;

                Vector3 v0 = ToVector3(vertices[i0]);
                Vector3 v1 = ToVector3(vertices[i1]);
                Vector3 v2 = ToVector3(vertices[i2]);

                var result = HitTester.MollerTrumboreIntersect(ray, v0, v1, v2);

                if (result.Hit && result.Distance < closestDistance)
                {
                    closestDistance = result.Distance;
                    closestPoint = result.Point;
                }
            }
        }

        return closestPoint;
    }

    /// <summary>Picks the nearest point on a line strip from a ray.</summary>
    /// <param name="points">The line points.</param>
    /// <param name="ray">The picking ray.</param>
    /// <param name="threshold">The selection threshold.</param>
    /// <returns>The nearest point on the line, or null if too far.</returns>
    public static Vector3? PickOnLine(List<Vector3> points, Ray ray, float threshold = 0.1f)
    {
        if (points == null || points.Count < 2)
            return null;

        Vector3? closestPoint = null;
        float closestDistance = threshold;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var result = RayLineSegmentIntersect(ray, points[i], points[i + 1]);

            if (result.Distance < closestDistance)
            {
                closestDistance = result.Distance;
                closestPoint = result.Point;
            }
        }

        return closestPoint;
    }

    /// <summary>Picks the nearest point on a point cloud from a ray.</summary>
    /// <param name="points">The point cloud positions.</param>
    /// <param name="ray">The picking ray.</param>
    /// <param name="threshold">The selection threshold in world units.</param>
    /// <returns>The nearest point, or null if too far.</returns>
    public static Vector3? PickOnPointCloud(List<Vector3> points, Ray ray, float threshold = 0.5f)
    {
        if (points == null || points.Count == 0)
            return null;

        Vector3? closestPoint = null;
        float closestDistance = threshold;

        foreach (var pt in points)
        {
            float dist = DistancePointToRay(pt, ray);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = pt;
            }
        }

        return closestPoint;
    }

    /// <summary>Computes the closest point on a ray to another ray.</summary>
    /// <param name="ray1">The first ray.</param>
    /// <param name="ray2">The second ray.</param>
    /// <returns>The closest point on ray1 to ray2.</returns>
    public static Vector3 ClosestPointOnRayToRay(Ray ray1, Ray ray2)
    {
        Vector3 r = ray1.Origin - ray2.Origin;
        float a = Vector3.Dot(ray1.Direction, ray1.Direction);
        float b = Vector3.Dot(ray1.Direction, ray2.Direction);
        float c = Vector3.Dot(ray2.Direction, ray2.Direction);
        float d = Vector3.Dot(ray1.Direction, r);
        float e = Vector3.Dot(ray2.Direction, r);

        float denom = a * c - b * b;

        float s, t;
        if (System.Math.Abs(denom) < Epsilon)
        {
            s = 0.0f;
            t = e / c;
        }
        else
        {
            s = (b * e - c * d) / denom;
            t = (a * e - b * d) / denom;
        }

        s = System.Math.Max(0.0f, s);
        t = System.Math.Max(0.0f, t);

        return ray1.Origin + ray1.Direction * s;
    }

    /// <summary>Computes the distance from a point to a ray.</summary>
    /// <param name="point">The point.</param>
    /// <param name="ray">The ray.</param>
    /// <returns>The distance.</returns>
    public static float DistancePointToRay(Vector3 point, Ray ray)
    {
        Vector3 toPoint = point - ray.Origin;
        float t = Vector3.Dot(toPoint, ray.Direction);
        t = System.Math.Max(0.0f, t);

        Vector3 closest = ray.Origin + ray.Direction * t;
        return Vector3.Distance(point, closest);
    }

    private static (Vector3 Point, float Distance) RayLineSegmentIntersect(Ray ray, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLen = lineDir.Length();

        if (lineLen < Epsilon)
        {
            float dist = DistancePointToRay(lineStart, ray);
            return (lineStart, dist);
        }

        lineDir /= lineLen;

        Vector3 w = ray.Origin - lineStart;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = Vector3.Dot(ray.Direction, lineDir);
        float c = Vector3.Dot(lineDir, lineDir);
        float d = Vector3.Dot(ray.Direction, w);
        float e = Vector3.Dot(lineDir, w);

        float denom = a * c - b * b;

        float s, t;
        if (System.Math.Abs(denom) < Epsilon)
        {
            s = 0.0f;
            t = System.Math.Max(0.0f, e / c);
        }
        else
        {
            s = (b * e - c * d) / denom;
            t = (a * e - b * d) / denom;
        }

        s = System.Math.Max(0.0f, s);
        t = System.Math.Max(0.0f, System.Math.Min(lineLen, t));

        Vector3 pointOnRay = ray.Origin + ray.Direction * s;
        Vector3 pointOnLine = lineStart + lineDir * t;

        float distance = Vector3.Distance(pointOnRay, pointOnLine);

        return (pointOnLine, distance);
    }

    private static Vector3 ToVector3(double[] v)
    {
        if (v == null || v.Length < 3)
            return Vector3.Zero;

        return new Vector3((float)v[0], (float)v[1], (float)v[2]);
    }
}
