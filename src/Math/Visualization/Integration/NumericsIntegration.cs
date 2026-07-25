namespace MathVerse.Math.Visualization.Integration;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Represents a data series for visualization.</summary>
public sealed class DataSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the data points.</summary>
    public List<Vector2> Points { get; init; } = new();

    /// <summary>Gets the series color.</summary>
    public string Color { get; init; } = "#0000FF";

    /// <summary>Gets the line width.</summary>
    public double LineWidth { get; init; } = 2.0;
}

/// <summary>Integrates with Numerics for data visualization.</summary>
public sealed class NumericsIntegration
{
    /// <summary>Creates a line plot from raw numeric data arrays.</summary>
    /// <param name="xData">The X data array.</param>
    /// <param name="yData">The Y data array.</param>
    /// <param name="color">The line color.</param>
    /// <returns>A line plot visualization object.</returns>
    public static Core.LinePlot CreateLinePlot(double[] xData, double[] yData, string color = "#0000FF")
    {
        var linePlot = new Core.LinePlot
        {
            Id = "numeric-line-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            LineWidth = 2.0,
            Points = new List<Vector2>()
        };

        int count = System.Math.Min(xData.Length, yData.Length);
        for (int i = 0; i < count; i++)
        {
            linePlot.Points.Add(new Vector2((float)xData[i], (float)yData[i]));
        }

        return linePlot;
    }

    /// <summary>Creates a scatter plot from raw numeric data arrays.</summary>
    /// <param name="xData">The X data array.</param>
    /// <param name="yData">The Y data array.</param>
    /// <param name="color">The point color.</param>
    /// <param name="pointSize">The point size.</param>
    /// <returns>A point cloud visualization object.</returns>
    public static Core.PointCloud CreateScatterPlot(double[] xData, double[] yData, string color = "#FF0000", double pointSize = 4.0)
    {
        var pointCloud = new Core.PointCloud
        {
            Id = "numeric-scatter-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            PointSize = pointSize,
            Points = new List<Vector3>()
        };

        int count = System.Math.Min(xData.Length, yData.Length);
        for (int i = 0; i < count; i++)
        {
            pointCloud.Points.Add(new Vector3((float)xData[i], (float)yData[i], 0));
        }

        return pointCloud;
    }

    /// <summary>Creates a 3D surface plot from a 2D data grid.</summary>
    /// <param name="zData">The Z values in row-major order.</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="fillColor">The surface fill color.</param>
    /// <returns>A mesh object for the surface.</returns>
    public static Core.MeshObject CreateSurfacePlot(double[,] zData, double xMin, double xMax, double yMin, double yMax, string fillColor = "#4488CC")
    {
        int rows = zData.GetLength(0);
        int cols = zData.GetLength(1);

        var meshObj = new Core.MeshObject
        {
            Id = "numeric-surface-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            FillColor = fillColor,
            WireframeColor = "#333333",
            Vertices = new List<Vector3>(),
            Faces = new List<int[]>()
        };

        double xStep = (xMax - xMin) / System.Math.Max(1, cols - 1);
        double yStep = (yMax - yMin) / System.Math.Max(1, rows - 1);

        for (int j = 0; j < rows; j++)
        {
            for (int i = 0; i < cols; i++)
            {
                double x = xMin + i * xStep;
                double y = yMin + j * yStep;
                double z = zData[j, i];

                meshObj.Vertices.Add(new Vector3((float)x, (float)z, (float)y));
            }
        }

        for (int j = 0; j < rows - 1; j++)
        {
            for (int i = 0; i < cols - 1; i++)
            {
                int v00 = j * cols + i;
                int v10 = j * cols + i + 1;
                int v01 = (j + 1) * cols + i;
                int v11 = (j + 1) * cols + i + 1;

                meshObj.Faces.Add(new int[] { v00, v10, v01 });
                meshObj.Faces.Add(new int[] { v10, v11, v01 });
            }
        }

        return meshObj;
    }

    /// <summary>Creates a bar chart from data values.</summary>
    /// <param name="values">The bar heights.</param>
    /// <param name="barWidth">The width of each bar.</param>
    /// <param name="color">The bar color.</param>
    /// <returns>Visualization objects for the bars.</returns>
    public static List<Core.VisualizationObject> CreateBarChart(double[] values, double barWidth = 0.8, string color = "#4488CC")
    {
        var bars = new List<Core.VisualizationObject>();

        for (int i = 0; i < values.Length; i++)
        {
            double x = i;
            double height = values[i];

            var bar = new Core.VisualizationObject
            {
                Id = $"bar-{i}",
                Color = color,
                Position = new Vector3((float)(x + barWidth / 2.0), (float)(height / 2.0), 0)
            };

            bars.Add(bar);
        }

        return bars;
    }

    /// <summary>Creates a histogram from data values.</summary>
    /// <param name="data">The data values.</param>
    /// <param name="binCount">The number of bins.</param>
    /// <param name="color">The bin color.</param>
    /// <returns>Histogram visualization data.</returns>
    public static (double[] BinEdges, double[] Counts) CreateHistogram(double[] data, int binCount = 10, string color = "#4488CC")
    {
        if (data == null || data.Length == 0)
            return (new double[0], new double[0]);

        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (double val in data)
        {
            min = System.Math.Min(min, val);
            max = System.Math.Max(max, val);
        }

        if (System.Math.Abs(max - min) < 1e-10)
        {
            return (new double[] { min - 0.5, max + 0.5 }, new double[] { data.Length });
        }

        double binWidth = (max - min) / binCount;
        var counts = new double[binCount];
        var edges = new double[binCount + 1];

        for (int i = 0; i <= binCount; i++)
        {
            edges[i] = min + i * binWidth;
        }

        foreach (double val in data)
        {
            int bin = (int)((val - min) / binWidth);
            bin = System.Math.Max(0, System.Math.Min(binCount - 1, bin));
            counts[bin]++;
        }

        return (edges, counts);
    }

    /// <summary>Computes basic statistics for a data array.</summary>
    /// <param name="data">The data array.</param>
    /// <returns>Mean, standard deviation, min, and max.</returns>
    public static (double Mean, double StdDev, double Min, double Max) ComputeStatistics(double[] data)
    {
        if (data == null || data.Length == 0)
            return (0, 0, 0, 0);

        double sum = 0;
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (double val in data)
        {
            sum += val;
            min = System.Math.Min(min, val);
            max = System.Math.Max(max, val);
        }

        double mean = sum / data.Length;

        double sumSqDiff = 0;
        foreach (double val in data)
        {
            double diff = val - mean;
            sumSqDiff += diff * diff;
        }

        double stdDev = System.Math.Sqrt(sumSqDiff / data.Length);

        return (mean, stdDev, min, max);
    }

    /// <summary>Normalizes data to a specified range.</summary>
    /// <param name="data">The input data.</param>
    /// <param name="targetMin">The target minimum.</param>
    /// <param name="targetMax">The target maximum.</param>
    /// <returns>The normalized data array.</returns>
    public static double[] NormalizeData(double[] data, double targetMin = 0.0, double targetMax = 1.0)
    {
        if (data == null || data.Length == 0)
            return new double[0];

        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (double val in data)
        {
            min = System.Math.Min(min, val);
            max = System.Math.Max(max, val);
        }

        double range = max - min;
        if (System.Math.Abs(range) < 1e-10)
        {
            double[] result = new double[data.Length];
            double mid = (targetMin + targetMax) / 2.0;
            for (int i = 0; i < data.Length; i++)
                result[i] = mid;
            return result;
        }

        double[] normalized = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            normalized[i] = targetMin + (data[i] - min) / range * (targetMax - targetMin);
        }

        return normalized;
    }

    /// <summary>Computes a moving average of the data.</summary>
    /// <param name="data">The input data.</param>
    /// <param name="windowSize">The moving average window size.</param>
    /// <returns>The smoothed data array.</returns>
    public static double[] MovingAverage(double[] data, int windowSize)
    {
        if (data == null || data.Length == 0 || windowSize <= 0)
            return new double[0];

        windowSize = System.Math.Min(windowSize, data.Length);
        double[] result = new double[data.Length];

        double windowSum = 0;
        for (int i = 0; i < windowSize; i++)
        {
            windowSum += data[i];
            result[i] = windowSum / (i + 1);
        }

        for (int i = windowSize; i < data.Length; i++)
        {
            windowSum += data[i] - data[i - windowSize];
            result[i] = windowSum / windowSize;
        }

        return result;
    }

    /// <summary>Computes the discrete Fourier transform of the data.</summary>
    /// <param name="real">The real part of the input signal.</param>
    /// <param name="imaginary">The imaginary part of the input signal.</param>
    /// <returns>The transformed real and imaginary parts.</returns>
    public static (double[] Real, double[] Imaginary) DiscreteFourierTransform(double[] real, double[] imaginary)
    {
        int n = real.Length;
        double[] outReal = new double[n];
        double[] outImaginary = new double[n];

        for (int k = 0; k < n; k++)
        {
            double sumReal = 0;
            double sumImag = 0;

            for (int t = 0; t < n; t++)
            {
                double angle = -2.0 * System.Math.PI * k * t / n;
                double cosVal = System.Math.Cos(angle);
                double sinVal = System.Math.Sin(angle);

                sumReal += real[t] * cosVal - imaginary[t] * sinVal;
                sumImag += real[t] * sinVal + imaginary[t] * cosVal;
            }

            outReal[k] = sumReal;
            outImaginary[k] = sumImag;
        }

        return (outReal, outImaginary);
    }
}
