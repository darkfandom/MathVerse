namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates contour plots using marching squares for line extraction.</summary>
public static class ContourPlot
{
    private readonly record struct ColorStop(double Pos, byte R, byte G, byte B);

    /// <summary>Creates a contour plot from a 2D data matrix.</summary>
    /// <param name="data">2D array of values.</param>
    /// <param name="levels">Number of contour levels to extract.</param>
    /// <param name="colorMap">Name of the color map for level coloring.</param>
    /// <returns>A <see cref="Plot2DResult"/> with contour line series.</returns>
    public static Plot2DResult Create(double[,] data, int levels = 10, string colorMap = "Viridis")
    {
        if (data is null) throw new System.ArgumentNullException(nameof(data));

        int rows = data.GetLength(0);
        int cols = data.GetLength(1);

        if (rows < 2 || cols < 2)
            throw new System.ArgumentException("Data matrix must be at least 2x2.");

        double min = data[0, 0], max = data[0, 0];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (data[i, j] < min) min = data[i, j];
                if (data[i, j] > max) max = data[i, j];
            }
        }

        if (max - min < 1e-14)
        {
            min -= 0.5;
            max += 0.5;
        }

        var stops = GetColorMapStops(colorMap);
        var seriesList = new List<PlotSeries>();

        for (int lev = 0; lev < levels; lev++)
        {
            double t = (lev + 0.5) / levels;
            double level = min + t * (max - min);
            string color = MapToColor(t, stops);

            var segments = ExtractContourSegments(data, rows, cols, level);
            var chains = ChainSegments(segments);

            foreach (var chain in chains)
            {
                if (chain.Count < 2) continue;

                seriesList.Add(new PlotSeries
                {
                    Label = $"Level {level:G4}",
                    Points = chain,
                    Color = color,
                    LineWidth = 1.5,
                    LineStyle = LineStyle.Solid,
                    Marker = MarkerStyle.None
                });
            }
        }

        var xTicks = AxisGenerator.GenerateTicks(0, cols - 1);
        var yTicks = AxisGenerator.GenerateTicks(0, rows - 1);

        return new Plot2DResult
        {
            Series = seriesList,
            XAxis = new PlotAxis { Label = "", Min = 0, Max = cols - 1, Ticks = xTicks },
            YAxis = new PlotAxis { Label = "", Min = 0, Max = rows - 1, Ticks = yTicks },
            Title = "",
            ShowGrid = false,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(0, 0, cols - 1, rows - 1)
        };
    }

    private static List<(Point2D P1, Point2D P2)> ExtractContourSegments(
        double[,] data, int rows, int cols, double level)
    {
        var segments = new List<(Point2D P1, Point2D P2)>();

        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                double tl = data[i, j];
                double tr = data[i, j + 1];
                double bl = data[i + 1, j];
                double br = data[i + 1, j + 1];

                int caseIndex = 0;
                if (tl >= level) caseIndex |= 8;
                if (tr >= level) caseIndex |= 4;
                if (br >= level) caseIndex |= 2;
                if (bl >= level) caseIndex |= 1;

                if (caseIndex == 0 || caseIndex == 15)
                    continue;

                Point2D top = Interp(tl, tr, level, j, i, j + 1, i);
                Point2D right = Interp(tr, br, level, j + 1, i, j + 1, i + 1);
                Point2D bottom = Interp(bl, br, level, j, i + 1, j + 1, i + 1);
                Point2D left = Interp(tl, bl, level, j, i, j, i + 1);

                switch (caseIndex)
                {
                    case 1: segments.Add((left, bottom)); break;
                    case 2: segments.Add((bottom, right)); break;
                    case 3: segments.Add((left, right)); break;
                    case 4: segments.Add((top, right)); break;
                    case 6: segments.Add((top, bottom)); break;
                    case 7: segments.Add((top, left)); break;
                    case 8: segments.Add((top, left)); break;
                    case 9: segments.Add((top, bottom)); break;
                    case 11: segments.Add((top, right)); break;
                    case 12: segments.Add((left, right)); break;
                    case 13: segments.Add((bottom, right)); break;
                    case 14: segments.Add((left, bottom)); break;
                    case 5:
                    {
                        double center = (tl + tr + bl + br) * 0.25;
                        if (center >= level)
                        {
                            segments.Add((left, top));
                            segments.Add((bottom, right));
                        }
                        else
                        {
                            segments.Add((left, bottom));
                            segments.Add((top, right));
                        }
                        break;
                    }
                    case 10:
                    {
                        double center = (tl + tr + bl + br) * 0.25;
                        if (center >= level)
                        {
                            segments.Add((top, right));
                            segments.Add((left, bottom));
                        }
                        else
                        {
                            segments.Add((top, left));
                            segments.Add((bottom, right));
                        }
                        break;
                    }
                }
            }
        }

        return segments;
    }

    private static Point2D Interp(
        double v1, double v2, double level,
        double x1, double y1, double x2, double y2)
    {
        if (System.Math.Abs(v2 - v1) < 1e-14)
            return new Point2D((x1 + x2) * 0.5, (y1 + y2) * 0.5);

        double t = System.Math.Clamp((level - v1) / (v2 - v1), 0.0, 1.0);
        return new Point2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
    }

    private static List<List<Point2D>> ChainSegments(
        List<(Point2D P1, Point2D P2)> segments)
    {
        var chains = new List<List<Point2D>>();
        var used = new bool[segments.Count];

        for (int s = 0; s < segments.Count; s++)
        {
            if (used[s]) continue;
            used[s] = true;

            var chain = new List<Point2D> { segments[s].P1, segments[s].P2 };

            bool extended;
            do
            {
                extended = false;
                for (int i = 0; i < segments.Count; i++)
                {
                    if (used[i]) continue;

                    var last = chain[^1];
                    var first = chain[0];

                    if (CloseEnough(last, segments[i].P1))
                    {
                        chain.Add(segments[i].P2);
                        used[i] = true;
                        extended = true;
                    }
                    else if (CloseEnough(last, segments[i].P2))
                    {
                        chain.Add(segments[i].P1);
                        used[i] = true;
                        extended = true;
                    }
                    else if (CloseEnough(first, segments[i].P2))
                    {
                        chain.Insert(0, segments[i].P1);
                        used[i] = true;
                        extended = true;
                    }
                    else if (CloseEnough(first, segments[i].P1))
                    {
                        chain.Insert(0, segments[i].P2);
                        used[i] = true;
                        extended = true;
                    }
                }
            } while (extended);

            if (chain.Count >= 2)
                chains.Add(chain);
        }

        return chains;
    }

    private static bool CloseEnough(Point2D a, Point2D b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return dx * dx + dy * dy < 1e-10;
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
