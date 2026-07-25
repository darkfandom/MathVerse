namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates simple line plots connecting data points.</summary>
public static class LinePlot
{
    /// <summary>Creates a line plot from X and Y data arrays.</summary>
    /// <param name="x">X coordinate values.</param>
    /// <param name="y">Y coordinate values.</param>
    /// <param name="color">Stroke color as a hex string.</param>
    /// <param name="lineWidth">Width of the connecting line.</param>
    /// <returns>A <see cref="Plot2DResult"/> with the line series and auto-scaled axes.</returns>
    public static Plot2DResult Create(
        double[] x,
        double[] y,
        string color = "#007ACC",
        double lineWidth = 2.0)
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

        double xPad = (xMax - xMin) * 0.05;
        double yPad = (yMax - yMin) * 0.05;
        if (xPad < 1e-10) xPad = System.Math.Max(1.0, System.Math.Abs(xMin) * 0.05 + 0.5);
        if (yPad < 1e-10) yPad = System.Math.Max(1.0, System.Math.Abs(yMin) * 0.05 + 0.5);

        xMin -= xPad;
        xMax += xPad;
        yMin -= yPad;
        yMax += yPad;

        var points = new List<Point2D>(x.Length);
        for (int i = 0; i < x.Length; i++)
            points.Add(new Point2D(x[i], y[i]));

        var xTicks = AxisGenerator.GenerateTicks(xMin, xMax);
        var yTicks = AxisGenerator.GenerateTicks(yMin, yMax);

        var series = new PlotSeries
        {
            Label = "Line",
            Points = points,
            Color = color,
            LineWidth = lineWidth,
            LineStyle = LineStyle.Solid,
            Marker = MarkerStyle.None
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
