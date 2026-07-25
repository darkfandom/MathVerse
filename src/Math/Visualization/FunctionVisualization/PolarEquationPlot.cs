namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates polar equation plots r = f(theta) converted to Cartesian coordinates.</summary>
public sealed class PolarEquationPlot
{
    /// <summary>Creates a polar curve plot from the function r = f(theta).</summary>
    /// <param name="rFunc">The radial function r = f(theta) where theta is in radians.</param>
    /// <param name="thetaMin">The minimum angle in radians.</param>
    /// <param name="thetaMax">The maximum angle in radians.</param>
    /// <param name="samples">The number of sample points along the angle.</param>
    /// <returns>A list of 2D Cartesian points representing the polar curve.</returns>
    public static List<Vector2> Create(
        Func<double, double> rFunc,
        double thetaMin, double thetaMax,
        int samples = 360)
    {
        ArgumentNullException.ThrowIfNull(rFunc);
        if (thetaMin >= thetaMax) throw new ArgumentException("thetaMin must be less than thetaMax.");
        if (samples < 2) throw new ArgumentOutOfRangeException(nameof(samples), "Samples must be at least 2.");

        List<Vector2> points = [];
        double step = (thetaMax - thetaMin) / (samples - 1);

        for (int i = 0; i < samples; i++)
        {
            double theta = thetaMin + i * step;
            double r = rFunc(theta);

            if (double.IsNaN(r) || double.IsInfinity(r))
            {
                points.Add(new Vector2(float.NaN, float.NaN));
                continue;
            }

            double x = r * System.Math.Cos(theta);
            double y = r * System.Math.Sin(theta);
            points.Add(new Vector2((float)x, (float)y));
        }

        return points;
    }
}
