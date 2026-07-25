namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates full Cartesian plots with auto-scaling, grid lines, and tick generation.</summary>
public static class CartesianPlot
{
    /// <summary>Creates a Cartesian line plot from X and Y data arrays.</summary>
    /// <param name="x">X coordinate values.</param>
    /// <param name="y">Y coordinate values.</param>
    /// <param name="options">Optional plot configuration.</param>
    /// <returns>A <see cref="Plot2DResult"/> containing the series, axes, and bounds.</returns>
    public static Plot2DResult Create(double[] x, double[] y, Plot2DOptions? options = null)
    {
        if (x is null) throw new System.ArgumentNullException(nameof(x));
        if (y is null) throw new System.ArgumentNullException(nameof(y));
        if (x.Length != y.Length)
            throw new System.ArgumentException("x and y arrays must have the same length.");
        if (x.Length == 0)
            throw new System.ArgumentException("Input arrays must not be empty.");

        options ??= new Plot2DOptions();

        double xMin = x[0], xMax = x[0];
        double yMin = y[0], yMax = y[0];

        for (int i = 1; i < x.Length; i++)
        {
            if (x[i] < xMin) xMin = x[i];
            if (x[i] > xMax) xMax = x[i];
            if (y[i] < yMin) yMin = y[i];
            if (y[i] > yMax) yMax = y[i];
        }

        ApplyPadding(ref xMin, ref xMax);
        ApplyPadding(ref yMin, ref yMax);

        if (options.XMin.HasValue) xMin = options.XMin.Value;
        if (options.XMax.HasValue) xMax = options.XMax.Value;
        if (options.YMin.HasValue) yMin = options.YMin.Value;
        if (options.YMax.HasValue) yMax = options.YMax.Value;

        var xTicks = AxisGenerator.GenerateTicks(xMin, xMax, options.MaxTicks);
        var yTicks = AxisGenerator.GenerateTicks(yMin, yMax, options.MaxTicks);

        var points = new List<Point2D>(x.Length);
        for (int i = 0; i < x.Length; i++)
            points.Add(new Point2D(x[i], y[i]));

        var series = new PlotSeries
        {
            Label = "Data",
            Points = points,
            Color = "#007ACC",
            LineWidth = 2.0,
            LineStyle = LineStyle.Solid,
            Marker = MarkerStyle.None
        };

        return new Plot2DResult
        {
            Series = [series],
            XAxis = new PlotAxis
            {
                Label = options.XAxisLabel ?? "",
                Min = xMin,
                Max = xMax,
                IsLogarithmic = options.IsXLogarithmic,
                Ticks = xTicks
            },
            YAxis = new PlotAxis
            {
                Label = options.YAxisLabel ?? "",
                Min = yMin,
                Max = yMax,
                IsLogarithmic = options.IsYLogarithmic,
                Ticks = yTicks
            },
            Title = options.Title ?? "",
            ShowGrid = options.ShowGrid,
            ShowLegend = options.ShowLegend,
            BackgroundColor = options.BackgroundColor,
            Bounds = new BoundingBox2D(xMin, yMin, xMax, yMax)
        };
    }

    /// <summary>Applies 5% padding to an axis range, ensuring a minimum span of 1.0.</summary>
    private static void ApplyPadding(ref double min, ref double max)
    {
        double span = max - min;
        double pad = span * 0.05;

        if (pad < 1e-10)
            pad = System.Math.Max(1.0, System.Math.Abs(min) * 0.05 + 0.5);

        min -= pad;
        max += pad;
    }
}
