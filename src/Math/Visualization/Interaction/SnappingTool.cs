namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Provides snapping utilities for precise positioning.</summary>
public sealed class SnappingTool
{
    /// <summary>Snap a point to the nearest grid intersection.</summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="gridSize">The grid spacing.</param>
    /// <returns>The snapped point.</returns>
    public static Vector2 SnapToGrid(Vector2 point, double gridSize)
    {
        if (gridSize <= 0)
            return point;

        float snappedX = (float)(System.Math.Round(point.X / gridSize) * gridSize);
        float snappedY = (float)(System.Math.Round(point.Y / gridSize) * gridSize);

        return new Vector2(snappedX, snappedY);
    }

    /// <summary>Snap a point to the nearest vertex from a list.</summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="vertices">The available vertices.</param>
    /// <param name="threshold">The maximum snap distance.</param>
    /// <returns>The snapped point, or the original if no vertex is close enough.</returns>
    public static Vector2 SnapToVertex(Vector2 point, List<Vector2> vertices, double threshold)
    {
        if (vertices == null || vertices.Count == 0)
            return point;

        float closestDist = (float)threshold;
        Vector2 closest = point;
        bool found = false;

        foreach (var vertex in vertices)
        {
            float dx = point.X - vertex.X;
            float dy = point.Y - vertex.Y;
            float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = vertex;
                found = true;
            }
        }

        return found ? closest : point;
    }

    /// <summary>Snap a point to the nearest edge from a list of line segments.</summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="edges">The line segments (start, end pairs).</param>
    /// <param name="threshold">The maximum snap distance.</param>
    /// <returns>The snapped point on the nearest edge.</returns>
    public static Vector2 SnapToEdge(Vector2 point, List<(Vector2 Start, Vector2 End)> edges, double threshold)
    {
        if (edges == null || edges.Count == 0)
            return point;

        float closestDist = (float)threshold;
        Vector2 closest = point;
        bool found = false;

        foreach (var (start, end) in edges)
        {
            var snapPoint = ClosestPointOnSegment(point, start, end);
            float dx = point.X - snapPoint.X;
            float dy = point.Y - snapPoint.Y;
            float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = snapPoint;
                found = true;
            }
        }

        return found ? closest : point;
    }

    /// <summary>Snap an angle to the nearest increment.</summary>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="increment">The angle increment in radians.</param>
    /// <returns>The snapped angle.</returns>
    public static double SnapAngle(double angle, double increment)
    {
        if (increment <= 0)
            return angle;

        return System.Math.Round(angle / increment) * increment;
    }

    /// <summary>Snap a value to the nearest step.</summary>
    /// <param name="value">The value to snap.</param>
    /// <param name="step">The step size.</param>
    /// <returns>The snapped value.</returns>
    public static double SnapToStep(double value, double step)
    {
        if (step <= 0)
            return value;

        return System.Math.Round(value / step) * step;
    }

    /// <summary>Snap a point to the nearest point on a line defined by two points.</summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="lineStart">Start of the line.</param>
    /// <param name="lineEnd">End of the line.</param>
    /// <returns>The closest point on the line.</returns>
    public static Vector2 SnapToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        return ClosestPointOnSegment(point, lineStart, lineEnd);
    }

    /// <summary>Snap a point to the nearest axis-aligned direction.</summary>
    /// <param name="point">The point relative to the origin.</param>
    /// <param name="origin">The snap origin.</param>
    /// <param name="angleThreshold">The angle threshold for axis snapping (degrees).</param>
    /// <returns>The snapped point.</returns>
    public static Vector2 SnapToAxis(Vector2 point, Vector2 origin, double angleThreshold = 15.0)
    {
        Vector2 delta = point - origin;
        float length = delta.Length();

        if (length < 0.0001f)
            return point;

        double angle = System.Math.Atan2(delta.Y, delta.X);
        double angleDeg = angle * 180.0 / System.Math.PI;

        double[] snapAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        double thresholdRad = angleThreshold * System.Math.PI / 180.0;

        foreach (double snapDeg in snapAngles)
        {
            double snapRad = snapDeg * System.Math.PI / 180.0;
            double diff = System.Math.Abs(angle - snapRad);

            if (diff > System.Math.PI)
                diff = 2.0 * System.Math.PI - diff;

            if (diff <= thresholdRad)
            {
                return origin + new Vector2(
                    (float)(length * System.Math.Cos(snapRad)),
                    (float)(length * System.Math.Sin(snapRad)));
            }
        }

        return point;
    }

    /// <summary>Snap a point to maintain distance constraint from an origin.</summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="origin">The constraint origin.</param>
    /// <param name="radius">The desired distance.</param>
    /// <returns>The snapped point at the exact distance from origin.</returns>
    public static Vector2 SnapToRadius(Vector2 point, Vector2 origin, double radius)
    {
        Vector2 delta = point - origin;
        float length = delta.Length();

        if (length < 0.0001f)
            return origin + new Vector2((float)radius, 0);

        return origin + delta / length * (float)radius;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float segLenSq = segment.LengthSquared();

        if (segLenSq < 1e-10f)
            return start;

        float t = Vector2.Dot(point - start, segment) / segLenSq;
        t = System.Math.Max(0.0f, System.Math.Min(1.0f, t));

        return start + segment * t;
    }
}
