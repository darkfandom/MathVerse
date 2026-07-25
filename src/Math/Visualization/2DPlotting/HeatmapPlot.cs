namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates 2D heatmaps by mapping matrix values to colors.</summary>
public static class HeatmapPlot
{
    private readonly record struct ColorStop(double Pos, byte R, byte G, byte B);

    /// <summary>Creates a heatmap from a 2D data matrix.</summary>
    /// <param name="data">2D array of values to visualize.</param>
    /// <param name="colorMap">Name of the color map: "Viridis", "Inferno", "Plasma", "Magma", or "Grayscale".</param>
    /// <returns>A <see cref="Plot2DResult"/> with colored rectangles for each cell.</returns>
    public static Plot2DResult Create(double[,] data, string colorMap = "Viridis")
    {
        if (data is null) throw new System.ArgumentNullException(nameof(data));

        int rows = data.GetLength(0);
        int cols = data.GetLength(1);

        if (rows == 0 || cols == 0)
            throw new System.ArgumentException("Data matrix must not be empty.");

        double min = data[0, 0], max = data[0, 0];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (data[i, j] < min) min = data[i, j];
                if (data[i, j] > max) max = data[i, j];
            }
        }

        var stops = GetColorMapStops(colorMap);
        var seriesList = new List<PlotSeries>(rows * cols);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                double t = max > min ? (data[i, j] - min) / (max - min) : 0.5;
                string color = MapToColor(t, stops);

                double x0 = j;
                double y0 = i;
                double x1 = j + 1.0;
                double y1 = i + 1.0;

                var rect = new List<Point2D>(5)
                {
                    new Point2D(x0, y0),
                    new Point2D(x1, y0),
                    new Point2D(x1, y1),
                    new Point2D(x0, y1),
                    new Point2D(x0, y0)
                };

                seriesList.Add(new PlotSeries
                {
                    Label = $"[{i},{j}]={data[i, j]:G4}",
                    Points = rect,
                    Color = color,
                    LineWidth = 0,
                    LineStyle = LineStyle.Solid,
                    Marker = MarkerStyle.None,
                    IsFilled = true,
                    FillColor = color
                });
            }
        }

        var xTicks = AxisGenerator.GenerateTicks(0, cols, System.Math.Min(cols, 10));
        var yTicks = AxisGenerator.GenerateTicks(0, rows, System.Math.Min(rows, 10));

        return new Plot2DResult
        {
            Series = seriesList,
            XAxis = new PlotAxis { Label = "", Min = 0, Max = cols, Ticks = xTicks },
            YAxis = new PlotAxis { Label = "", Min = 0, Max = rows, Ticks = yTicks },
            Title = "",
            ShowGrid = false,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(0, 0, cols, rows)
        };
    }

    private static string MapToColor(double t, ColorStop[] stops)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        int idx = 0;
        while (idx < stops.Length - 2 && stops[idx + 1].Pos <= t)
            idx++;

        double span = stops[idx + 1].Pos - stops[idx].Pos;
        double localT = span > 1e-14 ? (t - stops[idx].Pos) / span : 0.0;
        localT = System.Math.Clamp(localT, 0.0, 1.0);

        int r = (int)System.Math.Round(stops[idx].R + localT * (stops[idx + 1].R - stops[idx].R));
        int g = (int)System.Math.Round(stops[idx].G + localT * (stops[idx + 1].G - stops[idx].G));
        int b = (int)System.Math.Round(stops[idx].B + localT * (stops[idx + 1].B - stops[idx].B));

        r = System.Math.Clamp(r, 0, 255);
        g = System.Math.Clamp(g, 0, 255);
        b = System.Math.Clamp(b, 0, 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static ColorStop[] GetColorMapStops(string colorMap) => colorMap switch
    {
        "Viridis" =>
        [
            new(0.000, 68, 1, 84),
            new(0.167, 68, 58, 131),
            new(0.333, 49, 104, 142),
            new(0.500, 33, 145, 140),
            new(0.667, 53, 183, 121),
            new(0.833, 143, 215, 68),
            new(1.000, 253, 231, 37)
        ],
        "Inferno" =>
        [
            new(0.000, 0, 0, 4),
            new(0.250, 87, 16, 110),
            new(0.500, 188, 55, 84),
            new(0.750, 249, 142, 9),
            new(1.000, 252, 255, 164)
        ],
        "Plasma" =>
        [
            new(0.000, 13, 8, 135),
            new(0.250, 126, 3, 168),
            new(0.500, 204, 71, 170),
            new(0.750, 248, 150, 114),
            new(1.000, 240, 249, 33)
        ],
        "Magma" =>
        [
            new(0.000, 0, 0, 4),
            new(0.250, 81, 18, 124),
            new(0.500, 183, 55, 121),
            new(0.750, 252, 135, 97),
            new(1.000, 252, 253, 191)
        ],
        _ =>
        [
            new(0.000, 0, 0, 0),
            new(1.000, 255, 255, 255)
        ]
    };
}
