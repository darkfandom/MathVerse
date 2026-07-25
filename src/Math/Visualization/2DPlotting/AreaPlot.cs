namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates filled area plots under a curve.</summary>
public static class AreaPlot
{
    /// <summary>Creates a filled area plot from X and Y data arrays.</summary>
    /// <param name="x">X coordinate values.</param>
    /// <param name="y">Y coordinate values.</param>
    /// <param name="fillColor">Fill color as a hex string with optional alpha.</param>
    /// <returns>A <see cref="Plot2DResult"/> with a filled series and auto-scaled axes.</returns>
    public static Plot2DResult Create(
        double[] x,
        double[] y,
        string fillColor = "#007ACC33")
    {
        if (x is null) throw new System.ArgumentNullException(nameof(x));
        if (y is null) throw new System.ArgumentNullException(nameof(y));
        if (x.Length != y.Length)
            throw new System.ArgumentException("x and y arrays must have the same length.");
        if (x.Length == 0)
            throw new System.ArgumentException("Input arrays must not be empty.");

        double xMin = x[0], xMax = x[0];
        double yMin = y[0], yMax = y[0];

        for (int i = 1; i < x.Length; i++)
        {
            if (x[i] < xMin) xMin = x[i];
            if (x[i] > xMax) xMax = x[i];
            if (y[i] < yMin) yMin = y[i];
            if (y[i] > yMax) yMax = y[i];
        }

        double baseline = System.Math.Min(0.0, yMin);

        double xPad = (xMax - xMin) * 0.05;
        double yPad = (yMax - baseline) * 0.05;
        if (xPad < 1e-10) xPad = System.Math.Max(1.0, System.Math.Abs(xMin) * 0.05 + 0.5);
        if (yPad < 1e-10) yPad = System.Math.Max(1.0, System.Math.Abs(baseline) * 0.05 + 0.5);

        xMin -= xPad;
        xMax += xPad;
        yMin = System.Math.Min(baseline - yPad, yMin - yPad);
        yMax += yPad;

        var points = new List<Point2D>(x.Length + 2);
        points.Add(new Point2D(x[0], baseline));
        for (int i = 0; i < x.Length; i++)
            points.Add(new Point2D(x[i], y[i]));
        points.Add(new Point2D(x[x.Length - 1], baseline));

        var xTicks = AxisGenerator.GenerateTicks(xMin, xMax);
        var yTicks = AxisGenerator.GenerateTicks(yMin, yMax);

        var series = new PlotSeries
        {
            Label = "Area",
            Points = points,
            Color = "#007ACC",
            LineWidth = 1.0,
            LineStyle = LineStyle.Solid,
            Marker = MarkerStyle.None,
            IsFilled = true,
            FillColor = fillColor
        };

        return new Plot2DResult
        {
            Series = [series],
            XAxis = new PlotAxis { Label = "", Min = xMin, Max = xMax, Ticks = xTicks },
            YAxis = new PlotAxis { Label = "", Min = yMin, Max = yMax, Ticks = yTicks },
            Title = "",
            ShowGrid = true,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(xMin, yMin, xMax, yMax)
        };
    }
}
