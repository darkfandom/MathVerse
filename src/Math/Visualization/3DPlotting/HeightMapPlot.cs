namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates height map plots from 2D height arrays.</summary>
public sealed class HeightMapPlot
{
    /// <summary>Creates a height map plot from a 2D array of heights.</summary>
    /// <param name="heights">The 2D height array where [row, col] corresponds to the height at that grid position.</param>
    /// <param name="xScale">The scale factor along the X axis.</param>
    /// <param name="zScale">The scale factor along the Z axis (depth).</param>
    /// <param name="yScale">The scale factor along the Y axis (height).</param>
    /// <returns>A Plot3DResult containing the height map mesh.</returns>
    public static Plot3DResult Create(double[,] heights, double xScale = 1.0, double zScale = 1.0, double yScale = 1.0)
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

        double normXScale = xScale / (cols - 1);
        double normZScale = zScale / (rows - 1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double x = c * normXScale;
                double y = heights[r, c] * yScale;
                double z = r * normZScale;
                vertices.Add(new Vector3((float)x, (float)y, (float)z));

                double hNorm = (heights[r, c] - hMin) / hSpan;
                float cr = (float)System.Math.Clamp(hNorm, 0.0, 1.0);
                float cg = (float)System.Math.Clamp(1.0 - System.Math.Abs(hNorm - 0.5) * 2.0, 0.0, 1.0);
                float cb = (float)System.Math.Clamp(1.0 - hNorm, 0.0, 1.0);
                vertexColors.Add(new Vector4(cr, cg, cb, 1.0f));
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
            PlotType = Plot3DType.HeightMap
        };
    }
}
