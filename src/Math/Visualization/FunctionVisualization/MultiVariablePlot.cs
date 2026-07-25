namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates 2D plots of two-variable functions z = f(x, y) as colored grids.</summary>
public sealed class MultiVariablePlot
{
    /// <summary>Creates a colored grid visualization of a two-variable function.</summary>
    /// <param name="func">The function z = f(x, y) to visualize.</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A MultiVariableResult containing the grid values and metadata.</returns>
    public static MultiVariableResult Create(
        Func<double, double, double> func,
        double xMin, double xMax,
        double yMin, double yMax,
        int resolution = 50)
    {
        ArgumentNullException.ThrowIfNull(func);
        if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 2.");

        double[,] values = new double[resolution + 1, resolution + 1];
        double valMin = double.MaxValue;
        double valMax = double.MinValue;

        double xStep = (xMax - xMin) / resolution;
        double yStep = (yMax - yMin) / resolution;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double x = xMin + i * xStep;
                double y = yMin + j * yStep;
                double z = func(x, y);
                if (double.IsNaN(z) || double.IsInfinity(z)) z = 0.0;
                values[j, i] = z;
                if (z < valMin) valMin = z;
                if (z > valMax) valMax = z;
            }
        }

        double valSpan = valMax - valMin;
        if (valSpan < 1e-12) valSpan = 1.0;

        Vector4[,] colors = new Vector4[resolution + 1, resolution + 1];
        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double t = (values[j, i] - valMin) / valSpan;
                colors[j, i] = ViridisColor(t);
            }
        }

        return new MultiVariableResult
        {
            Values = values,
            Colors = colors,
            XMin = xMin,
            XMax = xMax,
            YMin = yMin,
            YMax = yMax,
            ValueMin = valMin,
            ValueMax = valMax,
            Resolution = resolution
        };
    }

    private static Vector4 ViridisColor(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        float r = (float)System.Math.Clamp(0.267 + t * 1.34 - t * t * 1.69 + t * t * t * 0.76, 0.0, 1.0);
        float g = (float)System.Math.Clamp(0.004 + t * 2.30 - t * t * 2.16 + t * t * t * 0.65, 0.0, 1.0);
        float b = (float)System.Math.Clamp(0.329 + t * 1.18 - t * t * 2.34 + t * t * t * 1.36, 0.0, 1.0);
        return new Vector4(r, g, b, 1f);
    }
}

/// <summary>Result of a multi-variable function plot operation.</summary>
public sealed class MultiVariableResult
{
    /// <summary>Gets the 2D array of function values indexed as [row, col].</summary>
    public double[,] Values { get; init; } = new double[0, 0];

    /// <summary>Gets the 2D array of RGBA colors corresponding to each grid cell.</summary>
    public Vector4[,] Colors { get; init; } = new Vector4[0, 0];

    /// <summary>Gets the X range minimum.</summary>
    public double XMin { get; init; }

    /// <summary>Gets the X range maximum.</summary>
    public double XMax { get; init; }

    /// <summary>Gets the Y range minimum.</summary>
    public double YMin { get; init; }

    /// <summary>Gets the Y range maximum.</summary>
    public double YMax { get; init; }

    /// <summary>Gets the minimum function value in the sampled grid.</summary>
    public double ValueMin { get; init; }

    /// <summary>Gets the maximum function value in the sampled grid.</summary>
    public double ValueMax { get; init; }

    /// <summary>Gets the number of subdivisions along each axis.</summary>
    public int Resolution { get; init; }
}
