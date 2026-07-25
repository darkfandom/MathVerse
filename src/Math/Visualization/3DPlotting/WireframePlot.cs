namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates wireframe plots of z = f(x, y).</summary>
public sealed class WireframePlot
{
    /// <summary>Creates a wireframe plot from a scalar function of two variables.</summary>
    /// <param name="zFunc">The function z = f(x, y).</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A Plot3DResult containing the wireframe line segments.</returns>
    public static Plot3DResult Create(
        Func<double, double, double> zFunc,
        double xMin, double xMax,
        double yMin, double yMax,
        int resolution = 20)
    {
        ArgumentNullException.ThrowIfNull(zFunc);
        if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 2.");

        List<Vector3> vertices = [];
        List<int[]> faces = [];

        int cols = resolution + 1;
        double xRange = xMax - xMin;
        double yRange = yMax - yMin;

        for (int j = 0; j <= resolution; j++)
        {
            double y = yMin + j * yRange / resolution;
            for (int i = 0; i <= resolution; i++)
            {
                double x = xMin + i * xRange / resolution;
                double z = zFunc(x, y);
                if (double.IsNaN(z) || double.IsInfinity(z)) z = 0.0;
                vertices.Add(new Vector3((float)x, (float)z, (float)y));
            }
        }

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                int curr = j * cols + i;
                int next = curr + 1;
                faces.Add([curr, next]);
            }
        }

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                int curr = j * cols + i;
                int below = (j + 1) * cols + i;
                faces.Add([curr, below]);
            }
        }

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
            Normals = [],
            VertexColors = [],
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.Wireframe
        };
    }
}
