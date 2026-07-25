namespace MathVerse.Math.Visualization.Animation;

/// <summary>Standard animation easing and interpolation curves.</summary>
public sealed class AnimationCurves
{
    /// <summary>
    /// Smoothstep ease-in-out curve. Accelerates and decelerates smoothly.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double EaseInOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return t * t * (3.0 - 2.0 * t);
    }

    /// <summary>
    /// Quadratic ease-in curve. Slow start, fast end.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double EaseIn(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return t * t;
    }

    /// <summary>
    /// Quadratic ease-out curve. Fast start, slow end.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double EaseOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return 1.0 - (1.0 - t) * (1.0 - t);
    }

    /// <summary>
    /// Bounce effect curve that simulates a bouncing ball.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Bounced value (0-1).</returns>
    public static double Bounce(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        if (t < 1.0 / 2.75)
        {
            return 7.5625 * t * t;
        }
        else if (t < 2.0 / 2.75)
        {
            t -= 1.5 / 2.75;
            return 7.5625 * t * t + 0.75;
        }
        else if (t < 2.5 / 2.75)
        {
            t -= 2.25 / 2.75;
            return 7.5625 * t * t + 0.9375;
        }
        else
        {
            t -= 2.625 / 2.75;
            return 7.5625 * t * t + 0.984375;
        }
    }

    /// <summary>
    /// Damped spring oscillation curve.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <param name="frequency">Oscillation frequency.</param>
    /// <param name="damping">Damping factor (0 = no damping, 1 = critical damping).</param>
    /// <returns>Spring value.</returns>
    public static double Spring(double t, double frequency = 5.0, double damping = 0.5)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        double decay = System.Math.Exp(-damping * t * 10.0);
        double oscillation = System.Math.Cos(2.0 * System.Math.PI * frequency * t);
        return 1.0 - decay * oscillation;
    }

    /// <summary>
    /// Linear passthrough (no easing).
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Same value as input.</returns>
    public static double Linear(double t)
    {
        return System.Math.Clamp(t, 0.0, 1.0);
    }

    /// <summary>
    /// Cubic ease-in-out curve. Smoother than quadratic smoothstep.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double CubicEaseInOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        if (t < 0.5)
            return 4.0 * t * t * t;
        else
        {
            double f = 2.0 * t - 2.0;
            return 0.5 * f * f * f + 1.0;
        }
    }

    /// <summary>
    /// Exponential ease-out curve.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double ExpoEaseOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return t < 1.0 ? 1.0 - System.Math.Pow(2.0, -10.0 * t) : 1.0;
    }

    /// <summary>
    /// Back ease-in-out (overshoots slightly).
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value (0-1).</returns>
    public static double BackEaseInOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        double s = 1.70158 * 1.525;
        if (t < 0.5)
            return 0.5 * (System.Math.Pow(2.0 * t, 2.0) * ((s + 1.0) * 2.0 * t - s));
        else
        {
            double u = 2.0 * t - 2.0;
            return 0.5 * (u * u * ((s + 1.0) * u + s) + 2.0);
        }
    }

    /// <summary>
    /// Elastic ease-out curve.
    /// </summary>
    /// <param name="t">Normalized time (0-1).</param>
    /// <returns>Eased value.</returns>
    public static double ElasticOut(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        if (t < 1e-15) return 0.0;
        if (System.Math.Abs(t - 1.0) < 1e-15) return 1.0;
        return System.Math.Pow(2.0, -10.0 * t) * System.Math.Sin((t - 0.075) * (2.0 * System.Math.PI) / 0.3) + 1.0;
    }
}
