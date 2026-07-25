namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates bar charts from labeled values.</summary>
public static class BarPlot
{
    private const double DefaultBarWidthFraction = 0.8;

    /// <summary>Creates a bar chart from labels and values.</summary>
    /// <param name="labels">Category labels for each bar.</param>
    /// <param name="values">Numeric values for each bar.</param>
    /// <param name="barColor">Fill color for the bars as a hex string.</param>
    /// <returns>A <see cref="Plot2DResult"/> with rectangular bar series.</returns>
    public static Plot2DResult Create(
        string[] labels,
        double[] values,
        string barColor = "#007ACC")
    {
        if (labels is null) throw new System.ArgumentNullException(nameof(labels));
        if (values is null) throw new System.ArgumentNullException(nameof(values));
        if (labels.Length != values.Length)
            throw new System.ArgumentException("labels and values arrays must have the same length.");
        if (labels.Length == 0)
            throw new System.ArgumentException("Input arrays must not be empty.");

        double yMin = 0.0, yMax = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < yMin) yMin = values[i];
            if (values[i] > yMax) yMax = values[i];
        }

        double yPad = (yMax - yMin) * 0.05;
        if (yPad < 1e-10) yPad = 1.0;
        yMax += yPad;
        if (yMin > 0) yMin = 0;

        double xMin = -0.5;
        double xMax = labels.Length - 0.5;
        double barWidth = DefaultBarWidthFraction;

        var seriesList = new List<PlotSeries>(labels.Length);

        for (int i = 0; i < labels.Length; i++)
        {
            double left = i - barWidth / 2.0;
            double right = i + barWidth / 2.0;
            double barTop = values[i];
            double barBottom = System.Math.Min(0.0, values[i]);

            var barPoints = new List<Point2D>(5)
            {
                new Point2D(left, barBottom),
                new Point2D(right, barBottom),
                new Point2D(right, barTop),
                new Point2D(left, barTop),
                new Point2D(left, barBottom)
            };

            seriesList.Add(new PlotSeries
            {
                Label = labels[i],
                Points = barPoints,
                Color = barColor,
                LineWidth = 1.0,
                LineStyle = LineStyle.Solid,
                Marker = MarkerStyle.None,
                IsFilled = true,
                FillColor = barColor
            });
        }

        var xTicks = new List<TickMark>(labels.Length);
        for (int i = 0; i < labels.Length; i++)
            xTicks.Add(new TickMark(i, labels[i]));

        var yTicks = AxisGenerator.GenerateTicks(yMin, yMax);

        return new Plot2DResult
        {
            Series = seriesList,
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
