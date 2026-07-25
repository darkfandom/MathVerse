namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates parametric surface plots defined by a vector-valued function of two parameters.</summary>
public sealed class ParametricSurfacePlot
{
    /// <summary>Creates a parametric surface plot from a function (u, v) -> (x, y, z).</summary>
    /// <param name="func">The parametric function mapping (u, v) to (x, y, z).</param>
    /// <param name="uMin">The minimum value of the u parameter.</param>
    /// <param name="uMax">The maximum value of the u parameter.</param>
    /// <param name="vMin">The minimum value of the v parameter.</param>
    /// <param name="vMax">The maximum value of the v parameter.</param>
    /// <param name="resolution">The number of subdivisions along each parameter axis.</param>
    /// <returns>A Plot3DResult containing the parametric surface mesh.</returns>
    public static Plot3DResult Create(
        Func<double, double, (double x, double y, double z)> func,
        double uMin, double uMax,
        double vMin, double vMax,
        int resolution = 50)
    {
        ArgumentNullException.ThrowIfNull(func);
        if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 2.");

        List<Vector3> vertices = [];
        List<int[]> faces = [];
        List<Vector3> normals = [];
        List<Vector4> vertexColors = [];

        int cols = resolution + 1;
        double uRange = uMax - uMin;
        double vRange = vMax - vMin;

        for (int j = 0; j <= resolution; j++)
        {
            double v = vMin + j * vRange / resolution;
            for (int i = 0; i <= resolution; i++)
            {
                double u = uMin + i * uRange / resolution;
                var (x, y, z) = func(u, v);
                if (double.IsNaN(x) || double.IsInfinity(x)) x = 0.0;
                if (double.IsNaN(y) || double.IsInfinity(y)) y = 0.0;
                if (double.IsNaN(z) || double.IsInfinity(z)) z = 0.0;
                vertices.Add(new Vector3((float)x, (float)y, (float)z));
            }
        }

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                int tl = j * cols + i;
                int tr = tl + 1;
                int bl = (j + 1) * cols + i;
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

        for (int i = 0; i < vertices.Count; i++)
        {
            float ny = (vertices[i].Y - bmin.Y) / System.Math.Max(bmax.Y - bmin.Y, 1e-6f);
            float r = (float)System.Math.Clamp(ny, 0.0, 1.0);
            float g = (float)System.Math.Clamp(1.0 - System.Math.Abs(ny - 0.5) * 2.0, 0.0, 1.0);
            float b = (float)System.Math.Clamp(1.0 - ny, 0.0, 1.0);
            vertexColors.Add(new Vector4(r, g, b, 1.0f));
        }

        return new Plot3DResult
        {
            Vertices = vertices,
            Faces = faces,
            Normals = normals,
            VertexColors = vertexColors,
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.ParametricSurface
        };
    }
}
