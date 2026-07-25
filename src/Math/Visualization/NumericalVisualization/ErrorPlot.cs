namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates error plots comparing computed values against exact values, showing absolute and relative errors.</summary>
public sealed class ErrorPlot
{
    /// <summary>Creates a Plot2DResult showing absolute and relative error series for computed vs exact values.</summary>
    /// <param name="computed">Array of computed numerical values.</param>
    /// <param name="exact">Array of exact reference values (must be same length as computed).</param>
    /// <returns>A <see cref="Plot2DResult"/> with error series.</returns>
    public static Plot2DResult Create(double[] computed, double[] exact)
    {
        var result = new Plot2DResult
        {
            Title = "Numerical Error Analysis",
            XLabel = "Index",
            YLabel = "Error"
        };

        if (computed.Length == 0 || exact.Length == 0) return result;

        int len = System.Math.Min(computed.Length, exact.Length);
        var indices = ImmutableArray.Create(Enumerable.Range(0, len).Select(i => (double)i).ToArray());
        var absErrors = new double[len];
        var relErrors = new double[len];

        double maxAbs = 0;
        double maxRel = 0;

        for (int i = 0; i < len; i++)
        {
            absErrors[i] = System.Math.Abs(computed[i] - exact[i]);
            if (absErrors[i] > maxAbs) maxAbs = absErrors[i];

            if (System.Math.Abs(exact[i]) > 1e-15)
            {
                relErrors[i] = absErrors[i] / System.Math.Abs(exact[i]);
                if (relErrors[i] > maxRel) maxRel = relErrors[i];
            }
            else
            {
                relErrors[i] = 0;
            }
        }

        result.Lines.Add(new Line2DSeries
        {
            Name = "Absolute Error",
            X = indices,
            Y = ImmutableArray.Create(absErrors),
            Color = "#E74C3C",
            LineWidth = 1.5
        });

        if (maxRel > 0)
        {
            result.Lines.Add(new Line2DSeries
            {
                Name = "Relative Error",
                X = indices,
                Y = ImmutableArray.Create(relErrors),
                Color = "#3498DB",
                LineWidth = 1.5,
                Style = LineStyle.Dashed
            });
        }

        double yMax = System.Math.Max(maxAbs, maxRel > 0 ? maxRel : 0) * 1.1;
        if (yMax < 1e-15) yMax = 1.0;

        result.XMin = 0;
        result.XMax = len - 1;
        result.YMin = 0;
        result.YMax = yMax;
        result.LogScaleY = maxAbs > 0 && maxRel > 0 && (maxRel / System.Math.Max(maxAbs, 1e-15)) > 100;

        result.Annotations.Add(new Annotation2D
        {
            X = len * 0.7,
            Y = yMax * 0.9,
            Text = $"Max Abs: {maxAbs:E3}\nMax Rel: {maxRel:E3}",
            Color = "#2C3E50"
        });

        return result;
    }
}
