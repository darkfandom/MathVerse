namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates piecewise function plots with discontinuity handling at breakpoints.</summary>
public sealed class PiecewisePlot
{
    /// <summary>Creates a plot of a piecewise-defined function.</summary>
    /// <param name="segments">The array of segment functions, one per interval.</param>
    /// <param name="breakpoints">The sorted array of breakpoints defining interval boundaries. Must have Length = segments.Length + 1.</param>
    /// <param name="xMin">The overall minimum X value to plot.</param>
    /// <param name="xMax">The overall maximum X value to plot.</param>
    /// <returns>A list of curve segments, where NaN values indicate discontinuities.</returns>
    public static List<Vector2> Create(
        Func<double, double>[] segments,
        double[] breakpoints,
        double xMin, double xMax)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(breakpoints);
        if (segments.Length == 0) throw new ArgumentException("At least one segment is required.", nameof(segments));
        if (breakpoints.Length != segments.Length + 1)
            throw new ArgumentException($"Breakpoints must have {segments.Length + 1} elements (segments.Length + 1).", nameof(breakpoints));

        List<Vector2> points = [];
        const int samplesPerSegment = 100;

        for (int s = 0; s < segments.Length; s++)
        {
            double segMin = System.Math.Max(breakpoints[s], xMin);
            double segMax = System.Math.Min(breakpoints[s + 1], xMax);

            if (segMin >= segMax) continue;

            double step = (segMax - segMin) / samplesPerSegment;
            if (step <= 0) continue;

            for (int i = 0; i <= samplesPerSegment; i++)
            {
                double x = segMin + i * step;
                if (x < xMin || x > xMax) continue;

                double y = segments[s](x);
                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    points.Add(new Vector2(float.NaN, float.NaN));
                }
                else
                {
                    points.Add(new Vector2((float)x, (float)y));
                }
            }

            if (s < segments.Length - 1)
            {
                double bp = breakpoints[s + 1];
                if (bp > xMin && bp < xMax)
                {
                    double yAtBp = segments[s](bp);
                    if (!double.IsNaN(yAtBp) && !double.IsInfinity(yAtBp))
                    {
                        points.Add(new Vector2((float)bp, (float)yAtBp));
                    }

                    points.Add(new Vector2(float.NaN, float.NaN));
                }
            }
        }

        return points;
    }
}
