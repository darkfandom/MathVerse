namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates residual plots for analyzing the quality of numerical solutions.</summary>
public sealed class ResidualPlot
{
    /// <summary>Creates a Plot2DResult showing residual values over iterations.</summary>
    /// <param name="iterations">Array of iteration numbers.</param>
    /// <param name="residuals">Array of residual values at each iteration (must be same length as iterations).</param>
    /// <returns>A <see cref="Plot2DResult"/> with the residual curve.</returns>
    public static Plot2DResult Create(double[] iterations, double[] residuals)
    {
        var result = new Plot2DResult
        {
            Title = "Residual Plot",
            XLabel = "Iteration",
            YLabel = "Residual"
        };

        if (iterations.Length == 0 || residuals.Length == 0) return result;

        int len = System.Math.Min(iterations.Length, residuals.Length);
        var itArr = ImmutableArray.Create(iterations.Take(len).ToArray());
        var resArr = ImmutableArray.Create(residuals.Take(len).ToArray());

        double minRes = residuals.Take(len).Min();
        double maxRes = residuals.Take(len).Max();
        double absMax = System.Math.Max(System.Math.Abs(minRes), System.Math.Abs(maxRes));

        result.Lines.Add(new Line2DSeries
        {
            Name = "Residual",
            X = itArr,
            Y = resArr,
            Color = "#E67E22",
            LineWidth = 1.5
        });

        // Zero reference line
        result.Lines.Add(new Line2DSeries
        {
            Name = "Zero",
            X = ImmutableArray.Create(iterations.Take(len).First(), iterations.Take(len).Last()),
            Y = ImmutableArray.Create(0.0, 0.0),
            Color = "#95A5A6",
            LineWidth = 1.0,
            Style = LineStyle.Dashed
        });

        // Convergence band
        if (len > 2)
        {
            var absResiduals = new double[len];
            for (int i = 0; i < len; i++)
                absResiduals[i] = System.Math.Abs(residuals[i]);

            result.Lines.Add(new Line2DSeries
            {
                Name = "|Residual|",
                X = itArr,
                Y = ImmutableArray.Create(absResiduals),
                Color = "#3498DB",
                LineWidth = 1.0,
                Style = LineStyle.Dotted
            });
        }

        result.XMin = iterations.Take(len).Min();
        result.XMax = iterations.Take(len).Max();
        result.YMin = -absMax * 1.1;
        result.YMax = absMax * 1.1;

        // Annotate with final residual
        if (len > 0)
        {
            double finalRes = residuals[len - 1];
            result.Annotations.Add(new Annotation2D
            {
                X = iterations[len - 1] * 0.7,
                Y = absMax * 0.8,
                Text = $"Final: {finalRes:E3}",
                Color = "#2C3E50"
            });
        }

        return result;
    }
}
