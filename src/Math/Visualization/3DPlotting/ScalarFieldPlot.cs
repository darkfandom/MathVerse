namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates scalar field visualizations using volume rendering abstractions.</summary>
public sealed class ScalarFieldPlot
{
    /// <summary>Creates an isosurface visualization of a 3D scalar field stored in an array.</summary>
    /// <param name="field">The 3D scalar field array indexed as [z, y, x].</param>
    /// <param name="isoValue">The threshold value for the isosurface extraction.</param>
    /// <param name="colorMap">The color map name: "Viridis", "Plasma", "Magma", "Inferno", "Cividis", or "Turbo".</param>
    /// <returns>A Plot3DResult containing the isosurface mesh with field-based coloring.</returns>
    public static Plot3DResult Create(double[,,] field, double isoValue, string colorMap = "Viridis")
    {
        ArgumentNullException.ThrowIfNull(field);

        int depth = field.GetLength(0);
        int height = field.GetLength(1);
        int width = field.GetLength(2);

        if (depth < 2 || height < 2 || width < 2)
            throw new ArgumentException("Field dimensions must be at least 2x2x2.", nameof(field));

        double fMin = double.MaxValue;
        double fMax = double.MinValue;

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double v = field[z, y, x];
                    if (double.IsNaN(v) || double.IsInfinity(v)) continue;
                    if (v < fMin) fMin = v;
                    if (v > fMax) fMax = v;
                }
            }
        }

        double fSpan = fMax - fMin;
        if (fSpan < 1e-12) fSpan = 1.0;
        int res = System.Math.Max(width, System.Math.Max(height, depth)) - 1;

        Plot3DResult result = IsosurfacePlot.Create(
            (x, y, z) =>
            {
                int ix = System.Math.Clamp((int)System.Math.Round(x * (width - 1)), 0, width - 1);
                int iy = System.Math.Clamp((int)System.Math.Round(y * (height - 1)), 0, height - 1);
                int iz = System.Math.Clamp((int)System.Math.Round(z * (depth - 1)), 0, depth - 1);
                return field[iz, iy, ix];
            },
            isoValue, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, res);

        result.VertexColors.Clear();
        foreach (Vector3 v in result.Vertices)
        {
            int ix = System.Math.Clamp((int)System.Math.Round(v.X * (width - 1)), 0, width - 1);
            int iy = System.Math.Clamp((int)System.Math.Round(v.Y * (height - 1)), 0, height - 1);
            int iz = System.Math.Clamp((int)System.Math.Round(v.Z * (depth - 1)), 0, depth - 1);
            double val = field[iz, iy, ix];
            double t = (val - fMin) / fSpan;
            result.VertexColors.Add(SampleColorMap(colorMap, t));
        }

        return result;
    }

    private static Vector4 SampleColorMap(string name, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return name switch
        {
            "Plasma" => Lerp(new Vector4(0.05f, 0.03f, 0.53f, 1f), new Vector4(0.94f, 0.98f, 0.13f, 1f), (float)t),
            "Magma" => Lerp(new Vector4(0.00f, 0.00f, 0.04f, 1f), new Vector4(0.99f, 0.98f, 0.75f, 1f), (float)t),
            "Inferno" => Lerp(new Vector4(0.00f, 0.00f, 0.02f, 1f), new Vector4(0.99f, 0.99f, 0.65f, 1f), (float)t),
            "Cividis" => Lerp(new Vector4(0.00f, 0.13f, 0.33f, 1f), new Vector4(0.99f, 0.91f, 0.14f, 1f), (float)t),
            "Turbo" => TurboSample(t),
            _ => ViridisSample(t),
        };
    }

    private static Vector4 ViridisSample(double t)
    {
        float r = (float)System.Math.Clamp(0.267 + t * 1.34 - t * t * 1.69 + t * t * t * 0.76, 0.0, 1.0);
        float g = (float)System.Math.Clamp(0.004 + t * 2.30 - t * t * 2.16 + t * t * t * 0.65, 0.0, 1.0);
        float b = (float)System.Math.Clamp(0.329 + t * 1.18 - t * t * 2.34 + t * t * t * 1.36, 0.0, 1.0);
        return new Vector4(r, g, b, 1f);
    }

    private static Vector4 TurboSample(double t)
    {
        float r = (float)System.Math.Clamp(0.14 + t * 2.2 - t * t * 1.5, 0.0, 1.0);
        float g = (float)System.Math.Clamp(0.02 + t * 2.8 - t * t * 2.6, 0.0, 1.0);
        float b = (float)System.Math.Clamp(0.50 + t * 1.0 - t * t * 2.8 + t * t * t * 1.5, 0.0, 1.0);
        return new Vector4(r, g, b, 1f);
    }

    private static Vector4 Lerp(Vector4 a, Vector4 b, float t)
    {
        return Vector4.Lerp(a, b, t);
    }
}
