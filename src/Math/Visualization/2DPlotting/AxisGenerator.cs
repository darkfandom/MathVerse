namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Generates axis tick marks using nice-number rounding for clean labels.</summary>
public static class AxisGenerator
{
    /// <summary>Generates evenly spaced tick marks with human-readable labels.</summary>
    /// <param name="min">Minimum axis value.</param>
    /// <param name="max">Maximum axis value.</param>
    /// <param name="maxTicks">Maximum number of ticks to generate.</param>
    /// <returns>A list of <see cref="TickMark"/> entries spanning the range.</returns>
    public static List<TickMark> GenerateTicks(double min, double max, int maxTicks = 10)
    {
        if (max <= min)
            return [new TickMark(min, FormatTickLabel(min))];

        double range = NiceNumber(max - min, round: false);
        double spacing = NiceNumber(range / System.Math.Max(1, maxTicks - 1), round: true);

        if (spacing <= 0)
            return [new TickMark(min, FormatTickLabel(min))];

        double graphMin = System.Math.Floor(min / spacing) * spacing;
        double graphMax = System.Math.Ceiling(max / spacing) * spacing;

        var ticks = new List<TickMark>();
        for (double value = graphMin; value <= graphMax + 0.5 * spacing; value += spacing)
        {
            ticks.Add(new TickMark(value, FormatTickLabel(value)));
        }

        return ticks;
    }

    private static double NiceNumber(double value, bool round)
    {
        if (value <= 0)
            return 0;

        double exponent = System.Math.Floor(System.Math.Log10(value));
        double fraction = value / System.Math.Pow(10, exponent);

        double nice = round
            ? fraction switch
            {
                < 1.5 => 1.0,
                < 3.0 => 2.0,
                < 7.0 => 5.0,
                _ => 10.0
            }
            : fraction switch
            {
                <= 1.0 => 1.0,
                <= 2.0 => 2.0,
                <= 5.0 => 5.0,
                _ => 10.0
            };

        return nice * System.Math.Pow(10, exponent);
    }

    private static string FormatTickLabel(double value)
    {
        if (System.Math.Abs(value) < 1e-10)
            return "0";

        double abs = System.Math.Abs(value);
        if (abs >= 1e6 || (abs < 1e-3 && abs > 0))
            return value.ToString("G4");

        return value.ToString("G6");
    }
}
