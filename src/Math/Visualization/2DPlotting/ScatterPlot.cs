namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates scatter plots with optional per-point colors and sizes.</summary>
public static class ScatterPlot
{
    /// <summary>Creates a scatter plot from coordinate arrays with optional color and size data.</summary>
    /// <param name="x">X coordinate values.</param>
    /// <param name="y">Y coordinate values.</param>
    /// <param name="colors">Optional per-point scalar values mapped to colors.</param>
    /// <param name="sizes">Optional per-point scalar values mapped to marker sizes.</param>
    /// <returns>A <see cref="Plot2DResult"/> with the scatter series and auto-scaled axes.</returns>
    public static Plot2DResult Create(
        double[] x,
        double[] y,
        double[]? colors = null,
        double[]? sizes = null)
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

        var seriesList = new List<PlotSeries>();

        bool hasColors = colors is not null && colors.Length == x.Length;
        bool hasSizes = sizes is not null && sizes.Length == x.Length;

        if (!hasColors && !hasSizes)
        {
            var points = new List<Point2D>(x.Length);
            for (int i = 0; i < x.Length; i++)
                points.Add(new Point2D(x[i], y[i]));

            seriesList.Add(new PlotSeries
            {
                Label = "Scatter",
                Points = points,
                Color = "#007ACC",
                LineWidth = 0,
                LineStyle = LineStyle.Solid,
                Marker = MarkerStyle.Circle,
                MarkerSize = 4.0
            });
        }
        else
        {
            double cMin = 0, cMax = 1;
            if (hasColors)
            {
                cMin = colors![0];
                cMax = colors[0];
                for (int i = 1; i < colors.Length; i++)
                {
                    if (colors[i] < cMin) cMin = colors[i];
                    if (colors[i] > cMax) cMax = colors[i];
                }
            }

            double sMin = 2.0, sMax = 8.0;
            if (hasSizes)
            {
                sMin = sizes![0];
                sMax = sizes[0];
                for (int i = 1; i < sizes.Length; i++)
                {
                    if (sizes[i] < sMin) sMin = sizes[i];
                    if (sizes[i] > sMax) sMax = sizes[i];
                }
            }

            var colorGroups = new System.Collections.Generic.Dictionary<int, List<Point2D>>();
            var colorGroupSizes = new System.Collections.Generic.Dictionary<int, double>();
            var colorGroupColors = new System.Collections.Generic.Dictionary<int, string>();

            for (int i = 0; i < x.Length; i++)
            {
                int colorBin = 0;
                if (hasColors)
                {
                    double t = cMax > cMin ? (colors![i] - cMin) / (cMax - cMin) : 0.5;
                    colorBin = (int)(t * 9.0);
                    colorBin = System.Math.Clamp(colorBin, 0, 9);
                }

                if (!colorGroups.ContainsKey(colorBin))
                {
                    colorGroups[colorBin] = [];
                    colorGroupColors[colorBin] = hasColors
                        ? InterpolateColor(colorBin / 9.0)
                        : "#007ACC";
                    colorGroupSizes[colorBin] = hasSizes
                        ? 2.0 + 6.0 * ((sizes![i] - sMin) / (sMax > sMin ? sMax - sMin : 1.0))
                        : 4.0;
                }

                colorGroups[colorBin].Add(new Point2D(x[i], y[i]));
            }

            foreach (var kvp in colorGroups)
            {
                seriesList.Add(new PlotSeries
                {
                    Label = $"Scatter {kvp.Key}",
                    Points = kvp.Value,
                    Color = colorGroupColors[kvp.Key],
                    LineWidth = 0,
                    LineStyle = LineStyle.Solid,
                    Marker = MarkerStyle.Circle,
                    MarkerSize = colorGroupSizes[kvp.Key]
                });
            }
        }

        var xTicks = AxisGenerator.GenerateTicks(xMin, xMax);
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

    private static string InterpolateColor(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        double r, g, b;
        if (t < 0.25)
        {
            double s = t / 0.25;
            r = 68.0 + s * (33.0 - 68.0);
            g = 1.0 + s * (145.0 - 1.0);
            b = 84.0 + s * (140.0 - 84.0);
        }
        else if (t < 0.5)
        {
            double s = (t - 0.25) / 0.25;
            r = 33.0 + s * (94.0 - 33.0);
            g = 145.0 + s * (201.0 - 145.0);
            b = 140.0 + s * (98.0 - 140.0);
        }
        else if (t < 0.75)
        {
            double s = (t - 0.5) / 0.25;
            r = 94.0 + s * (237.0 - 94.0);
            g = 201.0 + s * (222.0 - 201.0);
            b = 98.0 + s * (37.0 - 98.0);
        }
        else
        {
            double s = (t - 0.75) / 0.25;
            r = 237.0 + s * (253.0 - 237.0);
            g = 222.0 + s * (231.0 - 222.0);
            b = 37.0 + s * (37.0 - 37.0);
        }

        int ri = System.Math.Clamp((int)System.Math.Round(r), 0, 255);
        int gi = System.Math.Clamp((int)System.Math.Round(g), 0, 255);
        int bi = System.Math.Clamp((int)System.Math.Round(b), 0, 255);

        return $"#{ri:X2}{gi:X2}{bi:X2}";
    }
}
