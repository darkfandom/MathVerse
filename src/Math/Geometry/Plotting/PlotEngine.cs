using System.Collections.Immutable;
using MathVerse.Math.Geometry.Colors;
using MathVerse.Math.Geometry.Mesh;

namespace MathVerse.Math.Geometry.Plotting;

/// <summary>Provides methods for generating various types of mathematical plots.</summary>
public sealed class PlotEngine
{
    /// <summary>Plots a mathematical function f(x) over the specified range.</summary>
    /// <param name="f">The function to plot.</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotFunction(Func<double, double> f, double xMin, double xMax, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            var points = ImmutableArray<(double X, double Y)>.Empty;
            int steps = 100;
            double step = (xMax - xMin) / steps;

            for (int i = 0; i <= steps; i++)
            {
                double x = xMin + i * step;
                double y = f(x);
                points = points.Add((x, y));
            }

            var line = new LinePlotData("f(x)", Color.Blue, points, 1.5, PlotLineStyle.Solid);
            return new PlotResult(
                cfg,
                new List<LinePlotData> { line },
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotFunction failed: {ex.Message}");
        }
    }

    /// <summary>Plots a parametric curve (x(t), y(t)) over the specified parameter range.</summary>
    /// <param name="f">The parametric function returning (x, y) for parameter t.</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotParametric(Func<double, (double, double)> f, double tMin, double tMax, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            var points = ImmutableArray<(double X, double Y)>.Empty;
            int steps = 200;
            double step = (tMax - tMin) / steps;

            for (int i = 0; i <= steps; i++)
            {
                double t = tMin + i * step;
                (double x, double y) = f(t);
                points = points.Add((x, y));
            }

            var line = new LinePlotData("parametric", Color.Green, points, 1.5, PlotLineStyle.Solid);
            return new PlotResult(
                cfg,
                new List<LinePlotData> { line },
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotParametric failed: {ex.Message}");
        }
    }

    /// <summary>Plots a polar function r(theta) over the specified angle range.</summary>
    /// <param name="f">The polar function returning r for angle theta.</param>
    /// <param name="thetaMin">The minimum angle in radians.</param>
    /// <param name="thetaMax">The maximum angle in radians.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotPolar(Func<double, double> f, double thetaMin, double thetaMax, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            var points = ImmutableArray<(double X, double Y)>.Empty;
            int steps = 360;
            double step = (thetaMax - thetaMin) / steps;

            for (int i = 0; i <= steps; i++)
            {
                double theta = thetaMin + i * step;
                double r = f(theta);
                double x = r * System.Math.Cos(theta);
                double y = r * System.Math.Sin(theta);
                points = points.Add((x, y));
            }

            var line = new LinePlotData("polar", Color.Red, points, 1.5, PlotLineStyle.Solid);
            return new PlotResult(
                cfg,
                new List<LinePlotData> { line },
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotPolar failed: {ex.Message}");
        }
    }

    /// <summary>Plots a scatter diagram from the given data points.</summary>
    /// <param name="points">The data points to plot.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotScatter(IReadOnlyList<(double X, double Y)> points, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            var immutablePoints = ImmutableArray.CreateRange(points);
            var scatter = new ScatterPlotData("scatter", Color.Blue, immutablePoints, 6.0, ScatterMarkerType.Circle);
            return new PlotResult(
                cfg,
                ImmutableArray<LinePlotData>.Empty,
                new List<ScatterPlotData> { scatter },
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotScatter failed: {ex.Message}");
        }
    }

    /// <summary>Plots a histogram from the given values.</summary>
    /// <param name="values">The values to bin and plot.</param>
    /// <param name="bins">The number of bins.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotHistogram(IReadOnlyList<double> values, int bins, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;

            if (values.Count == 0)
            {
                return PlotResult.Failed("No values provided for histogram.");
            }

            double min = values[0];
            double max = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] < min) min = values[i];
                if (values[i] > max) max = values[i];
            }

            if (System.Math.Abs(max - min) < 1e-15)
            {
                max = min + 1.0;
            }

            double binWidth = (max - min) / bins;
            var counts = new int[bins];
            var bars = ImmutableArray<(double X, double Y)>.Empty;

            for (int i = 0; i < values.Count; i++)
            {
                int binIndex = (int)((values[i] - min) / binWidth);
                if (binIndex >= bins) binIndex = bins - 1;
                if (binIndex < 0) binIndex = 0;
                counts[binIndex]++;
            }

            for (int i = 0; i < bins; i++)
            {
                double x = min + (i + 0.5) * binWidth;
                bars = bars.Add((x, counts[i]));
            }

            var barData = new BarPlotData("histogram", Color.Cyan, bars);
            return new PlotResult(
                cfg,
                ImmutableArray<LinePlotData>.Empty,
                ImmutableArray<ScatterPlotData>.Empty,
                new List<BarPlotData> { barData },
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotHistogram failed: {ex.Message}");
        }
    }

    /// <summary>Plots a contour map of the given function.</summary>
    /// <param name="f">The function to contour.</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="levels">The number of contour levels.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotContour(Func<double, double, double> f, double xMin, double xMax, double yMin, double yMax, int levels, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            int gridRes = 50;
            double xStep = (xMax - xMin) / gridRes;
            double yStep = (yMax - yMin) / gridRes;

            double fMin = f(xMin, yMin);
            double fMax = f(xMin, yMin);

            double[,] values = new double[gridRes + 1, gridRes + 1];
            for (int i = 0; i <= gridRes; i++)
            {
                for (int j = 0; j <= gridRes; j++)
                {
                    double x = xMin + i * xStep;
                    double y = yMin + j * yStep;
                    values[i, j] = f(x, y);
                    if (values[i, j] < fMin) fMin = values[i, j];
                    if (values[i, j] > fMax) fMax = values[i, j];
                }
            }

            var contourLines = new List<LinePlotData>();
            double levelStep = (fMax - fMin) / (levels + 1);

            for (int l = 1; l <= levels; l++)
            {
                double levelValue = fMin + l * levelStep;
                var points = ImmutableArray<(double X, double Y)>.Empty;

                for (int i = 0; i < gridRes; i++)
                {
                    for (int j = 0; j < gridRes; j++)
                    {
                        double v00 = values[i, j];
                        double v10 = values[i + 1, j];
                        double v01 = values[i, j + 1];
                        double v11 = values[i + 1, j + 1];

                        if ((v00 <= levelValue && v10 > levelValue) || (v00 > levelValue && v10 <= levelValue))
                        {
                            double t = (levelValue - v00) / (v10 - v00);
                            double x = xMin + (i + t) * xStep;
                            double y = yMin + j * yStep;
                            points = points.Add((x, y));
                        }

                        if ((v01 <= levelValue && v11 > levelValue) || (v01 > levelValue && v11 <= levelValue))
                        {
                            double t = (levelValue - v01) / (v11 - v01);
                            double x = xMin + (i + t) * xStep;
                            double y = yMin + (j + 1) * yStep;
                            points = points.Add((x, y));
                        }
                    }
                }

                double t_norm = (double)l / (levels + 1);
                Color color = ColorMap.Evaluate(t_norm, ColorMapType.Viridis);
                contourLines.Add(new LinePlotData($"level {levelValue:F3}", color, points, 1.0, PlotLineStyle.Solid));
            }

            return new PlotResult(
                cfg,
                contourLines,
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotContour failed: {ex.Message}");
        }
    }

    /// <summary>Plots a 2D vector field.</summary>
    /// <param name="field">The vector field function returning (vx, vy).</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="resolution">The grid resolution.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotVectorField(Func<double, double, (double, double)> field, double xMin, double xMax, double yMin, double yMax, int resolution, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            double xStep = (xMax - xMin) / resolution;
            double yStep = (yMax - yMin) / resolution;
            var allLines = new List<LinePlotData>();

            for (int i = 0; i <= resolution; i++)
            {
                for (int j = 0; j <= resolution; j++)
                {
                    double x = xMin + i * xStep;
                    double y = yMin + j * yStep;
                    (double vx, double vy) = field(x, y);

                    double mag = System.Math.Sqrt(vx * vx + vy * vy);
                    if (mag < 1e-10) continue;

                    double scale = System.Math.Min(xStep, yStep) * 0.4;
                    double nvx = vx / mag * scale;
                    double nvy = vy / mag * scale;

                    var arrowPoints = ImmutableArray.Create(
                        (x - nvx * 0.5, y - nvy * 0.5),
                        (x + nvx * 0.5, y + nvy * 0.5));

                    double t = System.Math.Clamp(mag / 10.0, 0.0, 1.0);
                    Color color = ColorMap.Evaluate(t, ColorMapType.Jet);
                    allLines.Add(new LinePlotData("vector", color, arrowPoints, 1.0, PlotLineStyle.Solid));
                }
            }

            return new PlotResult(
                cfg,
                allLines,
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotVectorField failed: {ex.Message}");
        }
    }

    /// <summary>Generates a 3D surface mesh from the given function.</summary>
    /// <param name="f">The surface function z = f(x, y).</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="resolution">The grid resolution.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <returns>A <see cref="PlotResult"/> containing the surface mesh.</returns>
    public PlotResult PlotSurface(Func<double, double, double> f, double xMin, double xMax, double yMin, double yMax, int resolution, PlotConfiguration? config = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            double xStep = (xMax - xMin) / resolution;
            double yStep = (yMax - yMin) / resolution;

            var vertices = ImmutableArray<Geometry3D.Point3D>.Empty;
            var indices = ImmutableArray<int>.Empty;

            for (int i = 0; i <= resolution; i++)
            {
                for (int j = 0; j <= resolution; j++)
                {
                    double x = xMin + i * xStep;
                    double y = yMin + j * yStep;
                    double z = f(x, y);
                    vertices = vertices.Add(new Geometry3D.Point3D(x, y, z));
                }
            }

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    int topLeft = i * (resolution + 1) + j;
                    int topRight = i * (resolution + 1) + j + 1;
                    int bottomLeft = (i + 1) * (resolution + 1) + j;
                    int bottomRight = (i + 1) * (resolution + 1) + j + 1;

                    indices = indices.AddRange(topLeft, bottomLeft, topRight);
                    indices = indices.AddRange(topRight, bottomLeft, bottomRight);
                }
            }

            var mesh = new TriangleMesh(vertices, indices);
            return new PlotResult(
                cfg,
                ImmutableArray<LinePlotData>.Empty,
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                mesh,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotSurface failed: {ex.Message}");
        }
    }

    /// <summary>Plots a line through the given data points.</summary>
    /// <param name="points">The data points.</param>
    /// <param name="config">Optional plot configuration.</param>
    /// <param name="label">Optional label for the line.</param>
    /// <returns>A <see cref="PlotResult"/> containing the plot data.</returns>
    public PlotResult PlotLine(IReadOnlyList<(double X, double Y)> points, PlotConfiguration? config = null, string? label = null)
    {
        try
        {
            PlotConfiguration cfg = config ?? PlotConfiguration.Default;
            var immutablePoints = ImmutableArray.CreateRange(points);
            var line = new LinePlotData(label ?? "line", Color.Blue, immutablePoints, 1.5, PlotLineStyle.Solid);
            return new PlotResult(
                cfg,
                new List<LinePlotData> { line },
                ImmutableArray<ScatterPlotData>.Empty,
                ImmutableArray<BarPlotData>.Empty,
                null,
                true,
                null);
        }
        catch (Exception ex)
        {
            return PlotResult.Failed($"PlotLine failed: {ex.Message}");
        }
    }
}
