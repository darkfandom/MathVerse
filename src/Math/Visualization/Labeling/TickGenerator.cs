namespace MathVerse.Math.Visualization.Labeling;

/// <summary>Represents a single tick mark with its value, label, and position.</summary>
public sealed class TickMark
{
    /// <summary>Gets the numeric value of the tick.</summary>
    public double Value { get; init; }

    /// <summary>Gets the formatted label string for the tick.</summary>
    public string Label { get; init; } = "";

    /// <summary>Gets the normalized position (0-1) along the axis.</summary>
    public double Position { get; init; }
}

/// <summary>Generates tick marks for axis rendering.</summary>
public sealed class TickGenerator
{
    /// <summary>Generates linear tick marks between min and max values.</summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="maxTicks">Maximum number of ticks to generate.</param>
    /// <returns>A list of tick marks with values, labels, and positions.</returns>
    public static System.Collections.Generic.List<TickMark> GenerateLinearTicks(double min, double max, int maxTicks = 10)
    {
        var ticks = new System.Collections.Generic.List<TickMark>();

        if (System.Math.Abs(max - min) < 1e-10)
            return ticks;

        double range = max - min;
        double rawStep = range / maxTicks;
        double step = CalculateNiceStep(rawStep);
        double niceMin = System.Math.Floor(min / step) * step;
        double niceMax = System.Math.Ceiling(max / step) * step;

        for (double v = niceMin; v <= niceMax + step * 0.5; v += step)
        {
            if (v < min - step * 0.001 || v > max + step * 0.001)
                continue;

            double normalizedPos = (v - min) / range;
            string label = FormatTickValue(v, step);

            ticks.Add(new TickMark
            {
                Value = v,
                Label = label,
                Position = normalizedPos
            });
        }

        return ticks;
    }

    /// <summary>Generates logarithmic tick marks between min and max values.</summary>
    /// <param name="min">The minimum value (must be positive).</param>
    /// <param name="max">The maximum value (must be positive).</param>
    /// <param name="maxTicks">Maximum number of ticks to generate.</param>
    /// <returns>A list of tick marks with values, labels, and positions.</returns>
    public static System.Collections.Generic.List<TickMark> GenerateLogTicks(double min, double max, int maxTicks = 10)
    {
        var ticks = new System.Collections.Generic.List<TickMark>();

        if (min <= 0 || max <= 0 || min >= max)
            return ticks;

        double logMin = System.Math.Log10(min);
        double logMax = System.Math.Log10(max);
        double logRange = logMax - logMin;

        int decadeStart = (int)System.Math.Floor(logMin);
        int decadeEnd = (int)System.Math.Ceiling(logMax);

        int decades = decadeEnd - decadeStart;
        int majorTicksPerDecade = maxTicks > decades ? System.Math.Min(9, maxTicks / System.Math.Max(1, decades)) : 1;

        for (int decade = decadeStart; decade <= decadeEnd; decade++)
        {
            if (majorTicksPerDecade <= 1)
            {
                double value = System.Math.Pow(10.0, decade);
                if (value >= min * 0.999 && value <= max * 1.001)
                {
                    double logPos = (System.Math.Log10(value) - logMin) / logRange;
                    ticks.Add(new TickMark
                    {
                        Value = value,
                        Label = FormatLogValue(value),
                        Position = logPos
                    });
                }
            }
            else
            {
                for (int m = 1; m <= 9; m++)
                {
                    if (m > majorTicksPerDecade)
                        break;

                    double value = m * System.Math.Pow(10.0, decade);
                    if (value < min * 0.999 || value > max * 1.001)
                        continue;

                    double logPos = (System.Math.Log10(value) - logMin) / logRange;
                    string label = m == 1 ? FormatLogValue(value) : m.ToString();

                    ticks.Add(new TickMark
                    {
                        Value = value,
                        Label = label,
                        Position = logPos
                    });
                }
            }
        }

        return ticks;
    }

    private static double CalculateNiceStep(double rawStep)
    {
        if (rawStep <= 0)
            return 1.0;

        double logStep = System.Math.Log10(rawStep);
        double exponent = System.Math.Floor(logStep);
        double fraction = System.Math.Pow(10.0, logStep - exponent);

        double niceFraction;
        if (fraction <= 1.5)
            niceFraction = 1.0;
        else if (fraction <= 3.0)
            niceFraction = 2.0;
        else if (fraction <= 7.0)
            niceFraction = 5.0;
        else
            niceFraction = 10.0;

        return niceFraction * System.Math.Pow(10.0, exponent);
    }

    private static string FormatTickValue(double value, double step)
    {
        if (System.Math.Abs(value) < step * 1e-6)
            return "0";

        int decimals = 0;
        if (step < 1.0)
        {
            decimals = (int)System.Math.Ceiling(-System.Math.Log10(step));
            decimals = System.Math.Max(0, System.Math.Min(decimals, 10));
        }

        return value.ToString($"F{decimals}");
    }

    private static string FormatLogValue(double value)
    {
        if (value < 1.0)
            return value.ToString("G3");

        if (value >= 1e6)
            return value.ToString("E0");

        return value.ToString("G4");
    }
}
