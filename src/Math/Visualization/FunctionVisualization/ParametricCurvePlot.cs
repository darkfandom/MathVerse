namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates parametric curve plots defined by x(t) and y(t).</summary>
public sealed class ParametricCurvePlot
{
    /// <summary>Creates a parametric curve from separate x(t) and y(t) functions.</summary>
    /// <param name="xFunc">The function defining the X component: x = x(t).</param>
    /// <param name="yFunc">The function defining the Y component: y = y(t).</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    /// <param name="samples">The number of sample points along the curve.</param>
    /// <returns>A list of 2D points representing the parametric curve.</returns>
    public static List<Vector2> Create(
        Func<double, double> xFunc,
        Func<double, double> yFunc,
        double tMin, double tMax,
        int samples = 200)
    {
        ArgumentNullException.ThrowIfNull(xFunc);
        ArgumentNullException.ThrowIfNull(yFunc);
        if (tMin >= tMax) throw new ArgumentException("tMin must be less than tMax.");
        if (samples < 2) throw new ArgumentOutOfRangeException(nameof(samples), "Samples must be at least 2.");

        List<Vector2> points = [];
        double step = (tMax - tMin) / (samples - 1);

        for (int i = 0; i < samples; i++)
        {
            double t = tMin + i * step;
            double x = xFunc(t);
            double y = yFunc(t);
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
            {
                points.Add(new Vector2(float.NaN, float.NaN));
            }
            else
            {
                points.Add(new Vector2((float)x, (float)y));
            }
        }

        return points;
    }
}
