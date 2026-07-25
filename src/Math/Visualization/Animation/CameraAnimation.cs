namespace MathVerse.Math.Visualization.Animation;

using System.Collections.Generic;

/// <summary>Represents a point along a camera path.</summary>
public sealed record CameraPathPoint
{
    /// <summary>X position.</summary>
    public required double X { get; init; }

    /// <summary>Y position.</summary>
    public required double Y { get; init; }

    /// <summary>Z position.</summary>
    public required double Z { get; init; }

    /// <summary>Normalized time along the path (0-1).</summary>
    public required double Time { get; init; }

    /// <summary>Forward direction vector at this point.</summary>
    public (double X, double Y, double Z) Forward { get; init; }
}

/// <summary>Complete data for camera path animation.</summary>
public sealed record CameraPathData
{
    /// <summary>Sampled points along the camera path.</summary>
    public required IReadOnlyList<CameraPathPoint> Points { get; init; }

    /// <summary>Total path length (approximate).</summary>
    public required double TotalLength { get; init; }
}

/// <summary>Camera animation system supporting paths and orbits.</summary>
public sealed class CameraAnimation
{
    /// <summary>
    /// Creates a camera path by sampling a Catmull-Rom spline through the given control points.
    /// </summary>
    /// <param name="controlPoints">Control points defining the path.</param>
    /// <param name="samples">Number of samples along the path.</param>
    /// <returns>Sampled camera path data.</returns>
    public CameraPathData CreatePath(List<Vector3> controlPoints, int samples = 100)
    {
        if (controlPoints == null || controlPoints.Count < 2 || samples < 2)
        {
            return new CameraPathData
            {
                Points = [],
                TotalLength = 0.0
            };
        }

        var points = new List<CameraPathPoint>();
        int n = controlPoints.Count;

        for (int s = 0; s <= samples; s++)
        {
            double t = (double)s / (double)samples;
            double segmentT = t * (double)(n - 1);
            int segment = (int)segmentT;
            if (segment >= n - 1)
            {
                segment = n - 2;
                segmentT = (double)(n - 1);
            }

            double localT = segmentT - (double)segment;

            Vector3 p0 = controlPoints[System.Math.Max(0, segment - 1)];
            Vector3 p1 = controlPoints[segment];
            Vector3 p2 = controlPoints[System.Math.Min(n - 1, segment + 1)];
            Vector3 p3 = controlPoints[System.Math.Min(n - 1, segment + 2)];

            Vector3 pos = Interpolation.CatmullRom(p0, p1, p2, p3, localT);

            double epsilon = 0.001;
            Vector3 posNext = Interpolation.CatmullRom(p0, p1, p2, p3, System.Math.Min(1.0, localT + epsilon));
            Vector3 forward = (posNext - pos).Normalized;

            points.Add(new CameraPathPoint
            {
                X = pos.X,
                Y = pos.Y,
                Z = pos.Z,
                Time = t,
                Forward = (forward.X, forward.Y, forward.Z)
            });
        }

        double totalLength = 0.0;
        for (int i = 1; i < points.Count; i++)
        {
            double dx = points[i].X - points[i - 1].X;
            double dy = points[i].Y - points[i - 1].Y;
            double dz = points[i].Z - points[i - 1].Z;
            totalLength += System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        return new CameraPathData
        {
            Points = points,
            TotalLength = totalLength
        };
    }

    /// <summary>
    /// Creates a circular orbit camera path around a center point.
    /// </summary>
    /// <param name="center">Center of the orbit.</param>
    /// <param name="radius">Orbit radius.</param>
    /// <param name="startAngle">Start angle in radians.</param>
    /// <param name="endAngle">End angle in radians.</param>
    /// <param name="samples">Number of samples along the orbit.</param>
    /// <returns>Orbit camera path data.</returns>
    public CameraPathData CreateOrbit(
        Vector3 center,
        double radius,
        double startAngle,
        double endAngle,
        int samples = 100)
    {
        if (samples < 2 || radius <= 0.0)
        {
            return new CameraPathData
            {
                Points = [],
                TotalLength = 0.0
            };
        }

        var points = new List<CameraPathPoint>();
        double angleSpan = endAngle - startAngle;

        for (int s = 0; s <= samples; s++)
        {
            double t = (double)s / (double)samples;
            double angle = startAngle + angleSpan * t;

            double x = center.X + radius * System.Math.Cos(angle);
            double y = center.Y;
            double z = center.Z + radius * System.Math.Sin(angle);

            double fx = center.X - x;
            double fy = center.Y - y;
            double fz = center.Z - z;
            double fMag = System.Math.Sqrt(fx * fx + fy * fy + fz * fz);
            if (fMag > 1e-15)
            {
                fx /= fMag;
                fy /= fMag;
                fz /= fMag;
            }

            points.Add(new CameraPathPoint
            {
                X = x,
                Y = y,
                Z = z,
                Time = t,
                Forward = (fx, fy, fz)
            });
        }

        double totalLength = angleSpan * radius;

        return new CameraPathData
        {
            Points = points,
            TotalLength = System.Math.Abs(totalLength)
        };
    }
}
