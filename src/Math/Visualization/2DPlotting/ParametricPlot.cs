namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates parametric curve plots from X(t) and Y(t) functions.</summary>
public static class ParametricPlot
{
    /// <summary>Creates a parametric curve plot by sampling the given functions.</summary>
    /// <param name="xFunc">Function mapping parameter t to the X coordinate.</param>
    /// <param name="yFunc">Function mapping parameter t to the Y coordinate.</param>
    /// <param name="tMin">Minimum parameter value.</param>
    /// <param name="tMax">Maximum parameter value.</param>
    /// <param name="samples">Number of sample points along the curve.</param>
    /// <returns>A <see cref="Plot2DResult"/> containing the sampled curve and auto-scaled axes.</returns>
    public static Plot2DResult Create(
        Func<double, double> xFunc,
        Func<double, double> yFunc,
        double tMin,
        double tMax,
        int samples = 200)
    {
        if (xFunc is null) throw new System.ArgumentNullException(nameof(xFunc));
        if (yFunc is null) throw new System.ArgumentNullException(nameof(yFunc));
        if (samples < 2)
            throw new System.ArgumentException("samples must be at least 2.");
        if (tMax <= tMin)
            throw new System.ArgumentException("tMax must be greater than tMin.");

        var points = new List<Point2D>(samples);
        double xMin = double.MaxValue, xMax = double.MinValue;
        double yMin = double.MaxValue, yMax = double.MinValue;

        double dt = (tMax - tMin) / (samples - 1);

        for (int i = 0; i < samples; i++)
        {
            double t = tMin + i * dt;
            double x = xFunc(t);
            double y = yFunc(t);

            if (double.IsNaN(x) || double.IsInfinity(x) ||
                double.IsNaN(y) || double.IsInfinity(y))
                continue;

            points.Add(new Point2D(x, y));

            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;
        }

        if (points.Count == 0)
            throw new System.ArgumentException(
                "Functions produced no finite values in the given range.");

        double xPad = (xMax - xMin) * 0.05;
        double yPad = (yMax - yMin) * 0.05;
        if (xPad < 1e-10) xPad = System.Math.Max(1.0, System.Math.Abs(xMin) * 0.05 + 0.5);
        if (yPad < 1e-10) yPad = System.Math.Max(1.0, System.Math.Abs(yMin) * 0.05 + 0.5);

        xMin -= xPad;
        xMax += xPad;
        yMin -= yPad;
        yMax += yPad;

        var xTicks = AxisGenerator.GenerateTicks(xMin, xMax);
        var yTicks = AxisGenerator.GenerateTicks(yMin, yMax);

        var series = new PlotSeries
        {
            Label = "Parametric",
            Points = points,
            Color = "#007ACC",
            LineWidth = 2.0,
            LineStyle = LineStyle.Solid,
            Marker = MarkerStyle.None
        };

        return new Plot2DResult
        {
            Series = [series],
            XAxis = new PlotAxis { Label = "x", Min = xMin, Max = xMax, Ticks = xTicks },
            YAxis = new PlotAxis { Label = "y", Min = yMin, Max = yMax, Ticks = yTicks },
            Title = "",
            ShowGrid = true,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(xMin, yMin, xMax, yMax)
        };
    }
}
