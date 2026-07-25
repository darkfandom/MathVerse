namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates histograms with automatic binning using Sturges' rule.</summary>
public static class HistogramPlot
{
    /// <summary>Creates a histogram from a data array.</summary>
    /// <param name="data">Data values to bin.</param>
    /// <param name="bins">Number of bins, or 0 for automatic selection via Sturges' rule.</param>
    /// <param name="barColor">Fill color for the histogram bars as a hex string.</param>
    /// <returns>A <see cref="Plot2DResult"/> with bar series representing each bin.</returns>
    public static Plot2DResult Create(
        double[] data,
        int bins = 0,
        string barColor = "#007ACC")
    {
        if (data is null) throw new System.ArgumentNullException(nameof(data));
        if (data.Length == 0)
            throw new System.ArgumentException("Data array must not be empty.");

        double min = data[0], max = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            if (data[i] < min) min = data[i];
            if (data[i] > max) max = data[i];
        }

        double range = max - min;
        if (range < 1e-14)
        {
            min -= 0.5;
            max += 0.5;
            range = 1.0;
        }

        if (bins <= 0)
            bins = (int)System.Math.Ceiling(1.0 + 3.322 * System.Math.Log10(data.Length));

        bins = System.Math.Max(1, bins);

        double binWidth = range / bins;

        int[] counts = new int[bins];
        for (int i = 0; i < data.Length; i++)
        {
            int binIndex = (int)((data[i] - min) / binWidth);
            if (binIndex >= bins) binIndex = bins - 1;
            if (binIndex < 0) binIndex = 0;
            counts[binIndex]++;
        }

        int maxCount = 0;
        for (int i = 0; i < bins; i++)
        {
            if (counts[i] > maxCount) maxCount = counts[i];
        }

        double yMax = maxCount;
        double yPad = yMax * 0.05;
        if (yPad < 1e-10) yPad = 1.0;
        yMax += yPad;

        double plotXMin = min - binWidth * 0.1;
        double plotXMax = max + binWidth * 0.1;

        var seriesList = new List<PlotSeries>(bins);

        for (int i = 0; i < bins; i++)
        {
            double left = min + i * binWidth;
            double right = left + binWidth;

            var barPoints = new List<Point2D>(5)
            {
                new Point2D(left, 0),
                new Point2D(right, 0),
                new Point2D(right, counts[i]),
                new Point2D(left, counts[i]),
                new Point2D(left, 0)
            };

            string label = $"[{FormatBinLabel(left)}, {FormatBinLabel(right)})";

            seriesList.Add(new PlotSeries
            {
                Label = label,
                Points = barPoints,
                Color = barColor,
                LineWidth = 1.0,
                LineStyle = LineStyle.Solid,
                Marker = MarkerStyle.None,
                IsFilled = true,
                FillColor = barColor
            });
        }

        var xTicks = AxisGenerator.GenerateTicks(plotXMin, plotXMax);
        var yTicks = AxisGenerator.GenerateTicks(0, yMax);

        return new Plot2DResult
        {
            Series = seriesList,
            XAxis = new PlotAxis { Label = "", Min = plotXMin, Max = plotXMax, Ticks = xTicks },
            YAxis = new PlotAxis { Label = "Count", Min = 0, Max = yMax, Ticks = yTicks },
            Title = "",
            ShowGrid = true,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(plotXMin, 0, plotXMax, yMax)
        };
    }

    private static string FormatBinLabel(double value)
    {
        if (System.Math.Abs(value) < 1e-10)
            return "0";

        double abs = System.Math.Abs(value);
        if (abs >= 1e5 || (abs < 1e-3 && abs > 0))
            return value.ToString("G4");

        return value.ToString("G4");
    }
}
