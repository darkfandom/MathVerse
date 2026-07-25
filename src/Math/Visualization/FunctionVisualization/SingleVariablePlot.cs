namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates 2D plots of single-variable functions y = f(x) with adaptive sampling.</summary>
public sealed class SingleVariablePlot
{
    /// <summary>Creates a plot of a single-variable function with adaptive sampling based on curvature.</summary>
    /// <param name="func">The function y = f(x) to plot.</param>
    /// <param name="xMin">The minimum X value of the plotting range.</param>
    /// <param name="xMax">The maximum X value of the plotting range.</param>
    /// <param name="samples">The initial number of sample points before adaptive refinement.</param>
    /// <returns>A list of 2D points representing the sampled curve.</returns>
    public static List<Vector2> Create(Func<double, double> func, double xMin, double xMax, int samples = 500)
    {
        ArgumentNullException.ThrowIfNull(func);
        if (xMin >= xMax) throw new ArgumentException("xMin must be less than xMax.");
        if (samples < 2) throw new ArgumentOutOfRangeException(nameof(samples), "Samples must be at least 2.");

        List<Vector2> points = [];
        double step = (xMax - xMin) / (samples - 1);

        for (int i = 0; i < samples; i++)
        {
            double x = xMin + i * step;
            double y = func(x);
            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                points.Add(new Vector2((float)x, float.NaN));
            }
            else
            {
                points.Add(new Vector2((float)x, (float)y));
            }
        }

        List<Vector2> refined = [];
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (float.IsNaN(points[i].Y) || float.IsNaN(points[i + 1].Y))
            {
                if (!float.IsNaN(points[i].Y)) refined.Add(points[i]);
                continue;
            }

            refined.Add(points[i]);
            AdaptiveSubdivide(func, refined, points[i].X, points[i + 1].X, points[i].Y, points[i + 1].Y, 0, 8);
        }

        if (points.Count > 0 && !float.IsNaN(points[^1].Y))
        {
            refined.Add(points[^1]);
        }

        return MergeDuplicates(refined);
    }

    /// <summary>Recursively subdivides a line segment where curvature exceeds the threshold.</summary>
    private static void AdaptiveSubdivide(
        Func<double, double> func,
        List<Vector2> output,
        double x0, double x1,
        float y0, float y1,
        int depth, int maxDepth)
    {
        if (depth >= maxDepth) return;

        double xm = (x0 + x1) * 0.5;
        double ym = func(xm);
        if (double.IsNaN(ym) || double.IsInfinity(ym)) return;

        float ymf = (float)ym;
        float dx = (float)(x1 - x0);

        if (System.Math.Abs(dx) < 1e-10f) return;

        float slope0 = (ymf - y0) / (dx * 0.5f);
        float slope1 = (y1 - ymf) / (dx * 0.5f);
        float slopeDiff = System.Math.Abs(slope1 - slope0);

        if (slopeDiff > 0.15f)
        {
            AdaptiveSubdivide(func, output, x0, xm, y0, ymf, depth + 1, maxDepth);
            output.Add(new Vector2((float)xm, ymf));
            AdaptiveSubdivide(func, output, xm, x1, ymf, y1, depth + 1, maxDepth);
        }
    }

    /// <summary>Merges duplicate or very close points in the output list.</summary>
    private static List<Vector2> MergeDuplicates(List<Vector2> points)
    {
        if (points.Count <= 1) return points;

        List<Vector2> result = [points[0]];
        for (int i = 1; i < points.Count; i++)
        {
            Vector2 last = result[^1];
            if (System.Math.Abs(points[i].X - last.X) > 1e-7f)
            {
                result.Add(points[i]);
            }
            else if (!float.IsNaN(points[i].Y))
            {
                result[^1] = points[i];
            }
        }
        return result;
    }
}
