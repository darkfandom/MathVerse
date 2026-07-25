namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates convergence plots showing error reduction over iterations, typically on log scale.</summary>
public sealed class ConvergencePlot
{
    /// <summary>Creates a Plot2DResult showing convergence curves with optional log-scale Y axis.</summary>
    /// <param name="iterations">Array of iteration numbers or step indices.</param>
    /// <param name="errors">Array of error values at each iteration (must be same length as iterations).</param>
    /// <returns>A <see cref="Plot2DResult"/> with the convergence curve.</returns>
    public static Plot2DResult Create(double[] iterations, double[] errors)
    {
        var result = new Plot2DResult
        {
            Title = "Convergence Plot",
            XLabel = "Iteration",
            YLabel = "Error",
            LogScaleY = true
        };

        if (iterations.Length == 0 || errors.Length == 0) return result;

        int len = System.Math.Min(iterations.Length, errors.Length);
        var itArr = ImmutableArray.Create(iterations.Take(len).ToArray());
        var errArr = new double[len];

        for (int i = 0; i < len; i++)
            errArr[i] = System.Math.Max(errors[i], 1e-300);

        double minErr = errArr.Min();
        double maxErr = errArr.Max();

        result.Lines.Add(new Line2DSeries
        {
            Name = "Error",
            X = itArr,
            Y = ImmutableArray.Create(errArr),
            Color = "#E74C3C",
            LineWidth = 2.0
        });

        // Compute convergence rate if we have enough points
        if (len > 2)
        {
            var rates = new double[len - 1];
            for (int i = 1; i < len; i++)
            {
                if (errArr[i] > 1e-300 && errArr[i - 1] > 1e-300)
                {
                    double ratio = errArr[i - 1] / errArr[i];
                    rates[i - 1] = ratio > 1.0 ? System.Math.Log(ratio) / System.Math.Log(2.0) : 0;
                }
            }

            double avgRate = 0;
            int validRates = 0;
            for (int i = 0; i < rates.Length; i++)
            {
                if (rates[i] > 0)
                {
                    avgRate += rates[i];
                    validRates++;
                }
            }
            if (validRates > 0) avgRate /= validRates;

            if (validRates > 0 && avgRate > 0)
            {
                result.Annotations.Add(new Annotation2D
                {
                    X = iterations[len - 1] * 0.6,
                    Y = minErr * 10,
                    Text = $"Avg Rate: {avgRate:F2} bits/iter",
                    Color = "#8E44AD"
                });
            }
        }

        result.XMin = iterations.Take(len).Min();
        result.XMax = iterations.Take(len).Max();
        result.YMin = minErr * 0.5;
        result.YMax = maxErr * 2.0;

        return result;
    }
}
