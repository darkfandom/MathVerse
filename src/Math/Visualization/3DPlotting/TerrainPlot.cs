namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates terrain visualization plots with coloring based on elevation.</summary>
public sealed class TerrainPlot
{
    /// <summary>Creates a terrain visualization with height-based coloring.</summary>
    /// <param name="heights">The 2D height array where values represent elevation.</param>
    /// <param name="colorMap">The name of the color map: "Terrain", "GreenBrown", "Alpine", or "Desert".</param>
    /// <returns>A Plot3DResult containing the terrain mesh with appropriate coloring.</returns>
    public static Plot3DResult Create(double[,] heights, string colorMap = "Terrain")
    {
        ArgumentNullException.ThrowIfNull(heights);

        int rows = heights.GetLength(0);
        int cols = heights.GetLength(1);
        if (rows < 2 || cols < 2) throw new ArgumentException("Height array must be at least 2x2.", nameof(heights));

        List<Vector3> vertices = [];
        List<int[]> faces = [];
        List<Vector3> normals = [];
        List<Vector4> vertexColors = [];

        double hMin = double.MaxValue;
        double hMax = double.MinValue;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double h = heights[r, c];
                if (double.IsNaN(h) || double.IsInfinity(h)) h = 0.0;
                if (h < hMin) hMin = h;
                if (h > hMax) hMax = h;
            }
        }

        double hSpan = hMax - hMin;
        if (hSpan < 1e-12) hSpan = 1.0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double x = (double)c / (cols - 1);
                double y = heights[r, c];
                double z = (double)r / (rows - 1);
                vertices.Add(new Vector3((float)x, (float)y, (float)z));

                double hNorm = (heights[r, c] - hMin) / hSpan;
                Vector4 color = SampleTerrainColorMap(colorMap, hNorm);
                vertexColors.Add(color);
            }
        }

        for (int r = 0; r < rows - 1; r++)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                int tl = r * cols + c;
                int tr = tl + 1;
                int bl = (r + 1) * cols + c;
                int br = bl + 1;

                faces.Add([tl, bl, tr]);
                faces.Add([tr, bl, br]);
            }
        }

        SurfacePlot.ComputeNormalsForFaces(vertices, faces, normals);

        Vector3 bmin = Vector3.One * float.MaxValue;
        Vector3 bmax = Vector3.One * float.MinValue;
        foreach (Vector3 v in vertices)
        {
            bmin = Vector3.Min(bmin, v);
            bmax = Vector3.Max(bmax, v);
        }

        return new Plot3DResult
        {
            Vertices = vertices,
            Faces = faces,
            Normals = normals,
            VertexColors = vertexColors,
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.Surface
        };
    }

    /// <summary>Samples a terrain color map at the given normalized height value.</summary>
    /// <param name="colorMapName">The name of the color map.</param>
    /// <param name="t">The normalized height value in [0, 1].</param>
    /// <returns>The interpolated color as an RGBA vector.</returns>
    private static Vector4 SampleTerrainColorMap(string colorMapName, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        return colorMapName switch
        {
            "GreenBrown" => InterpolateGreenBrown(t),
            "Alpine" => InterpolateAlpine(t),
            "Desert" => InterpolateDesert(t),
            _ => InterpolateTerrain(t),
        };
    }

    /// <summary>Terrain color map: deep blue (water) -> green (lowland) -> brown (highland) -> white (snow).</summary>
    private static Vector4 InterpolateTerrain(double t)
    {
        if (t < 0.2)
        {
            double s = t / 0.2;
            return LerpColor(new Vector4(0.1f, 0.3f, 0.6f, 1f), new Vector4(0.2f, 0.5f, 0.2f, 1f), (float)s);
        }
        if (t < 0.5)
        {
            double s = (t - 0.2) / 0.3;
            return LerpColor(new Vector4(0.2f, 0.5f, 0.2f, 1f), new Vector4(0.6f, 0.5f, 0.3f, 1f), (float)s);
        }
        if (t < 0.8)
        {
            double s = (t - 0.5) / 0.3;
            return LerpColor(new Vector4(0.6f, 0.5f, 0.3f, 1f), new Vector4(0.5f, 0.4f, 0.3f, 1f), (float)s);
        }
        double s2 = (t - 0.8) / 0.2;
        return LerpColor(new Vector4(0.5f, 0.4f, 0.3f, 1f), new Vector4(0.95f, 0.95f, 0.95f, 1f), (float)s2);
    }

    /// <summary>Green-brown color map: green (low) -> brown (high).</summary>
    private static Vector4 InterpolateGreenBrown(double t)
    {
        if (t < 0.5)
        {
            double s = t / 0.5;
            return LerpColor(new Vector4(0.15f, 0.55f, 0.15f, 1f), new Vector4(0.55f, 0.45f, 0.25f, 1f), (float)s);
        }
        double s2 = (t - 0.5) / 0.5;
        return LerpColor(new Vector4(0.55f, 0.45f, 0.25f, 1f), new Vector4(0.75f, 0.7f, 0.6f, 1f), (float)s2);
    }

    /// <summary>Alpine color map: green -> grey rock -> white snow.</summary>
    private static Vector4 InterpolateAlpine(double t)
    {
        if (t < 0.3)
        {
            double s = t / 0.3;
            return LerpColor(new Vector4(0.2f, 0.5f, 0.15f, 1f), new Vector4(0.35f, 0.45f, 0.2f, 1f), (float)s);
        }
        if (t < 0.7)
        {
            double s = (t - 0.3) / 0.4;
            return LerpColor(new Vector4(0.35f, 0.45f, 0.2f, 1f), new Vector4(0.55f, 0.53f, 0.5f, 1f), (float)s);
        }
        double s2 = (t - 0.7) / 0.3;
        return LerpColor(new Vector4(0.55f, 0.53f, 0.5f, 1f), new Vector4(0.98f, 0.98f, 0.98f, 1f), (float)s2);
    }

    /// <summary>Desert color map: dark sand -> light sand -> white salt.</summary>
    private static Vector4 InterpolateDesert(double t)
    {
        if (t < 0.4)
        {
            double s = t / 0.4;
            return LerpColor(new Vector4(0.55f, 0.4f, 0.2f, 1f), new Vector4(0.8f, 0.65f, 0.4f, 1f), (float)s);
        }
        if (t < 0.75)
        {
            double s = (t - 0.4) / 0.35;
            return LerpColor(new Vector4(0.8f, 0.65f, 0.4f, 1f), new Vector4(0.92f, 0.88f, 0.75f, 1f), (float)s);
        }
        double s2 = (t - 0.75) / 0.25;
        return LerpColor(new Vector4(0.92f, 0.88f, 0.75f, 1f), new Vector4(1.0f, 1.0f, 0.95f, 1f), (float)s2);
    }

    /// <summary>Linearly interpolates between two colors.</summary>
    private static Vector4 LerpColor(Vector4 a, Vector4 b, float t)
    {
        return Vector4.Lerp(a, b, t);
    }
}
