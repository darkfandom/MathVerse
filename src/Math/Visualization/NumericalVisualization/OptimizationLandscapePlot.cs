namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates 2D optimization landscape visualizations as heatmaps with contour overlay.</summary>
public sealed class OptimizationLandscapePlot
{
    /// <summary>Creates a Plot2DResult showing a heatmap of objective function values over a 2D domain.</summary>
    /// <param name="objective">The objective function f(x, y) to visualize.</param>
    /// <param name="xMin">Minimum X value of the domain.</param>
    /// <param name="xMax">Maximum X value of the domain.</param>
    /// <param name="yMin">Minimum Y value of the domain.</param>
    /// <param name="yMax">Maximum Y value of the domain.</param>
    /// <param name="resolution">Number of grid points in each direction (default 50).</param>
    /// <returns>A <see cref="Plot2DResult"/> containing the heatmap and contour data.</returns>
    public static Plot2DResult Create(
        Func<double, double, double> objective,
        double xMin, double xMax, double yMin, double yMax,
        int resolution = 50)
    {
        var result = new Plot2DResult
        {
            Title = "Optimization Landscape",
            XLabel = "x",
            YLabel = "y"
        };

        double dx = (xMax - xMin) / resolution;
        double dy = (yMax - yMin) / resolution;

        // Evaluate function on grid
        var values = new double[resolution + 1, resolution + 1];
        double fMin = double.MaxValue;
        double fMax = double.MinValue;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double x = xMin + i * dx;
                double y = yMin + j * dy;
                values[i, j] = objective(x, y);

                if (values[i, j] < fMin) fMin = values[i, j];
                if (values[i, j] > fMax) fMax = values[i, j];
            }
        }

        double fRange = fMax - fMin;
        if (fRange < 1e-15) fRange = 1.0;

        // Create contour lines using marching squares
        int contourLevels = 10;
        var levels = new double[contourLevels];
        for (int l = 0; l < contourLevels; l++)
            levels[l] = fMin + fRange * (l + 1) / (contourLevels + 1);

        foreach (double level in levels)
        {
            var contourSegments = MarchingSquares(values, resolution, resolution, level, xMin, yMin, dx, dy);

            for (int s = 0; s < contourSegments.Count; s += 2)
            {
                if (s + 1 >= contourSegments.Count) break;

                result.Lines.Add(new Line2DSeries
                {
                    Name = $"Contour {level:F2}",
                    X = ImmutableArray.Create(contourSegments[s][0], contourSegments[s + 1][0]),
                    Y = ImmutableArray.Create(contourSegments[s][1], contourSegments[s + 1][1]),
                    Color = GetHeatmapColor((level - fMin) / fRange),
                    LineWidth = 0.8
                });
            }
        }

        // Find and mark the minimum
        double minX = xMin, minY = yMin, minVal = fMin;
        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                if (values[i, j] < minVal)
                {
                    minVal = values[i, j];
                    minX = xMin + i * dx;
                    minY = yMin + j * dy;
                }
            }
        }

        result.Points.Add(new Point2DSeries
        {
            Name = "Minimum",
            X = ImmutableArray.Create(minX),
            Y = ImmutableArray.Create(minY),
            Color = "#E74C3C",
            PointSize = 8.0,
            Marker = "star"
        });

        result.Annotations.Add(new Annotation2D
        {
            X = minX,
            Y = minY,
            Text = $"min: ({minX:F2}, {minY:F2})\nf={minVal:F4}",
            Color = "#E74C3C"
        });

        result.XMin = xMin;
        result.XMax = xMax;
        result.YMin = yMin;
        result.YMax = yMax;

        return result;
    }

    private static List<double[]> MarchingSquares(double[,] values, int width, int height,
        double level, double offsetX, double offsetY, double dx, double dy)
    {
        var segments = new List<double[]>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                double v00 = values[i, j];
                double v10 = values[i + 1, j];
                double v11 = values[i + 1, j + 1];
                double v01 = values[i, j + 1];

                int code = 0;
                if (v00 >= level) code |= 1;
                if (v10 >= level) code |= 2;
                if (v11 >= level) code |= 4;
                if (v01 >= level) code |= 8;

                if (code == 0 || code == 15) continue;

                double x0 = offsetX + i * dx;
                double y0 = offsetY + j * dy;
                double x1 = offsetX + (i + 1) * dx;
                double y1 = offsetY + (j + 1) * dy;

                double mx = (x0 + x1) * 0.5;
                double my = (y0 + y1) * 0.5;

                double top = Lerp(x0, x1, (level - v00) / (v10 - v00));
                double right = Lerp(y0, y1, (level - v10) / (v11 - v10));
                double bottom = Lerp(x0, x1, (level - v01) / (v11 - v01));
                double left = Lerp(y0, y1, (level - v00) / (v01 - v00));

                switch (code)
                {
                    case 1: case 14: segments.Add([top, y0]); segments.Add([x0, left]); break;
                    case 2: case 13: segments.Add([top, y0]); segments.Add([x1, right]); break;
                    case 3: case 12: segments.Add([x0, left]); segments.Add([x1, right]); break;
                    case 4: case 11: segments.Add([x1, right]); segments.Add([bottom, y1]); break;
                    case 5:
                        segments.Add([top, y0]); segments.Add([x1, right]);
                        segments.Add([x0, left]); segments.Add([bottom, y1]);
                        break;
                    case 6: case 9: segments.Add([top, y0]); segments.Add([bottom, y1]); break;
                    case 7: case 8: segments.Add([x0, left]); segments.Add([bottom, y1]); break;
                    case 10:
                        segments.Add([top, y0]); segments.Add([x0, left]);
                        segments.Add([x1, right]); segments.Add([bottom, y1]);
                        break;
                }
            }
        }

        return segments;
    }

    private static double Lerp(double a, double b, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return a + t * (b - a);
    }

    private static string GetHeatmapColor(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        // Viridis-like color map
        double r, g, b;

        if (t < 0.25)
        {
            double s = t / 0.25;
            r = System.Math.Round(68 + s * (1 - 68));
            g = System.Math.Round(1 + s * (120 - 1));
            b = System.Math.Round(84 + s * (140 - 84));
        }
        else if (t < 0.5)
        {
            double s = (t - 0.25) / 0.25;
            r = System.Math.Round(1 + s * (53 - 1));
            g = System.Math.Round(120 + s * (165 - 120));
            b = System.Math.Round(140 + s * (112 - 140));
        }
        else if (t < 0.75)
        {
            double s = (t - 0.5) / 0.25;
            r = System.Math.Round(53 + s * (180 - 53));
            g = System.Math.Round(165 + s * (200 - 165));
            b = System.Math.Round(112 + s * (33 - 112));
        }
        else
        {
            double s = (t - 0.75) / 0.25;
            r = System.Math.Round(180 + s * (253 - 180));
            g = System.Math.Round(200 + s * (231 - 200));
            b = System.Math.Round(33 + s * (37 - 33));
        }

        return $"#{(int)r:X2}{(int)g:X2}{(int)b:X2}";
    }
}
