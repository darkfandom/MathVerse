namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates root-finding method visualization showing bracket/iteration steps graphically.</summary>
public sealed class RootFindingAnimation
{
    private const double FunctionYScale = 1.0;

    /// <summary>Represents a single step of a root-finding iteration.</summary>
    public sealed class RootFindingStep
    {
        /// <summary>Gets the iteration number.</summary>
        public int Iteration { get; init; }

        /// <summary>Gets the current bracket/approximation interval X values.</summary>
        public ImmutableArray<double> BracketX { get; init; }

        /// <summary>Gets the current bracket/approximation interval Y values.</summary>
        public ImmutableArray<double> BracketY { get; init; }

        /// <summary>Gets the current midpoint or approximation.</summary>
        public double Approximation { get; init; }

        /// <summary>Gets the function value at the approximation.</summary>
        public double ApproximationY { get; init; }

        /// <summary>Gets the error estimate.</summary>
        public double Error { get; init; }
    }

    /// <summary>Creates a list of visualization steps for a root-finding method.</summary>
    /// <param name="func">The function to find roots of.</param>
    /// <param name="xMin">Left boundary of the search interval.</param>
    /// <param name="xMax">Right boundary of the search interval.</param>
    /// <param name="method">The root-finding method: "Bisection", "FalsePosition", "Secant".</param>
    /// <param name="maxSteps">Maximum number of iterations (default 20).</param>
    /// <returns>A list of <see cref="RootFindingStep"/> and a <see cref="Plot2DResult"/> for rendering.</returns>
    public static (List<RootFindingStep> Steps, Plot2DResult Plot) CreateSteps(
        Func<double, double> func, double xMin, double xMax,
        string method = "Bisection", int maxSteps = 20)
    {
        var steps = new List<RootFindingStep>();
        var plot = CreateFunctionPlot(func, xMin, xMax);

        double a = xMin;
        double b = xMax;
        double fa = func(a);
        double fb = func(b);

        switch (method.ToUpperInvariant())
        {
            case "BISECTION":
                ComputeBisectionSteps(func, a, b, fa, fb, maxSteps, steps, plot);
                break;
            case "FALSEPOSITION":
                ComputeFalsePositionSteps(func, a, b, fa, fb, maxSteps, steps, plot);
                break;
            case "SECANT":
                ComputeSecantSteps(func, a, b, fa, fb, maxSteps, steps, plot);
                break;
            default:
                ComputeBisectionSteps(func, a, b, fa, fb, maxSteps, steps, plot);
                break;
        }

        return (steps, plot);
    }

    private static void ComputeBisectionSteps(Func<double, double> func,
        double a, double b, double fa, double fb, int maxSteps,
        List<RootFindingStep> steps, Plot2DResult plot)
    {
        for (int i = 0; i < maxSteps; i++)
        {
            double mid = (a + b) * 0.5;
            double fmid = func(mid);

            steps.Add(new RootFindingStep
            {
                Iteration = i,
                BracketX = ImmutableArray.Create(a, mid, b),
                BracketY = ImmutableArray.Create(fa, fmid, fb),
                Approximation = mid,
                ApproximationY = fmid,
                Error = (b - a) * 0.5
            });

            // Add bracket visualization
            plot.Lines.Add(new Line2DSeries
            {
                Name = $"Bracket {i}",
                X = ImmutableArray.Create(a, a, b, b),
                Y = ImmutableArray.Create(fa, 0, fb, 0),
                Color = GetStepColor(i),
                LineWidth = 1.0,
                Style = LineStyle.Dashed
            });

            // Add midpoint marker
            plot.Points.Add(new Point2DSeries
            {
                Name = $"Mid {i}",
                X = ImmutableArray.Create(mid),
                Y = ImmutableArray.Create(fmid),
                Color = GetStepColor(i),
                PointSize = 6.0,
                Marker = "diamond"
            });

            if (System.Math.Abs(fmid) < 1e-15 || (b - a) * 0.5 < 1e-15) break;

            if (fa * fmid < 0)
            {
                b = mid;
                fb = fmid;
            }
            else
            {
                a = mid;
                fa = fmid;
            }
        }
    }

    private static void ComputeFalsePositionSteps(Func<double, double> func,
        double a, double b, double fa, double fb, int maxSteps,
        List<RootFindingStep> steps, Plot2DResult plot)
    {
        for (int i = 0; i < maxSteps; i++)
        {
            double c = (a * fb - b * fa) / (fb - fa);
            double fc = func(c);

            steps.Add(new RootFindingStep
            {
                Iteration = i,
                BracketX = ImmutableArray.Create(a, c, b),
                BracketY = ImmutableArray.Create(fa, fc, fb),
                Approximation = c,
                ApproximationY = fc,
                Error = System.Math.Abs(fc)
            });

            plot.Points.Add(new Point2DSeries
            {
                Name = $"FP {i}",
                X = ImmutableArray.Create(c),
                Y = ImmutableArray.Create(fc),
                Color = GetStepColor(i),
                PointSize = 6.0,
                Marker = "triangle"
            });

            if (System.Math.Abs(fc) < 1e-15) break;

            if (fa * fc < 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        }
    }

    private static void ComputeSecantSteps(Func<double, double> func,
        double a, double b, double fa, double fb, int maxSteps,
        List<RootFindingStep> steps, Plot2DResult plot)
    {
        for (int i = 0; i < maxSteps; i++)
        {
            if (System.Math.Abs(fb - fa) < 1e-15) break;

            double c = b - fb * (b - a) / (fb - fa);
            double fc = func(c);

            steps.Add(new RootFindingStep
            {
                Iteration = i,
                BracketX = ImmutableArray.Create(a, c, b),
                BracketY = ImmutableArray.Create(fa, fc, fb),
                Approximation = c,
                ApproximationY = fc,
                Error = System.Math.Abs(fc)
            });

            plot.Points.Add(new Point2DSeries
            {
                Name = $"Sec {i}",
                X = ImmutableArray.Create(c),
                Y = ImmutableArray.Create(fc),
                Color = GetStepColor(i),
                PointSize = 6.0,
                Marker = "square"
            });

            a = b;
            fa = fb;
            b = c;
            fb = fc;

            if (System.Math.Abs(fb) < 1e-15) break;
        }
    }

    private static Plot2DResult CreateFunctionPlot(Func<double, double> func, double xMin, double xMax)
    {
        var plot = new Plot2DResult
        {
            Title = "Root Finding Visualization",
            XLabel = "x",
            YLabel = "f(x)"
        };

        int samples = 200;
        var xVals = new double[samples];
        var yVals = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            xVals[i] = xMin + (xMax - xMin) * i / (samples - 1);
            yVals[i] = func(xVals[i]);
        }

        plot.Lines.Add(new Line2DSeries
        {
            Name = "f(x)",
            X = ImmutableArray.Create(xVals),
            Y = ImmutableArray.Create(yVals),
            Color = "#3498DB",
            LineWidth = 2.0
        });

        // Zero line
        plot.Lines.Add(new Line2DSeries
        {
            Name = "y = 0",
            X = ImmutableArray.Create(xMin, xMax),
            Y = ImmutableArray.Create(0.0, 0.0),
            Color = "#95A5A6",
            LineWidth = 1.0,
            Style = LineStyle.Dashed
        });

        double yMin = yVals.Min();
        double yMax = yVals.Max();
        double yAbsMax = System.Math.Max(System.Math.Abs(yMin), System.Math.Abs(yMax));

        plot.XMin = xMin;
        plot.XMax = xMax;
        plot.YMin = -yAbsMax * 1.2;
        plot.YMax = yAbsMax * 1.2;

        return plot;
    }

    private static string GetStepColor(int step)
    {
        string[] palette = ["#E74C3C", "#E67E22", "#F1C40F", "#2ECC71", "#1ABC9C", "#3498DB", "#9B59B6", "#34495E"];
        return palette[step % palette.Length];
    }
}
