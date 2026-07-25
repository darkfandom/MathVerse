namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates 3D point cloud scatter plots.</summary>
public sealed class PointCloudPlot
{
    /// <summary>Creates a point cloud plot from an array of 3D points with optional per-point colors.</summary>
    /// <param name="points">The 3D points as arrays of [x, y, z].</param>
    /// <param name="colors">Optional per-point colors as arrays of [r, g, b] or [r, g, b, a] in [0,1]. Null defaults to a single color.</param>
    /// <returns>A Plot3DResult containing the point cloud.</returns>
    public static Plot3DResult Create(double[][] points, double[]? colors = null)
    {
        ArgumentNullException.ThrowIfNull(points);

        List<Vector3> vertices = [];
        List<Vector4> vertexColors = [];
        int colorStride = 3;

        if (colors != null && colors.Length > 0)
        {
            if (colors.Length % 3 == 0) colorStride = 3;
            else if (colors.Length % 4 == 0) colorStride = 4;
            else throw new ArgumentException("Color array length must be divisible by 3 or 4.", nameof(colors));
        }

        for (int i = 0; i < points.Length; i++)
        {
            double[] p = points[i];
            if (p.Length < 3) throw new ArgumentException($"Point at index {i} must have at least 3 components.", nameof(points));
            vertices.Add(new Vector3((float)p[0], (float)p[1], (float)p[2]));
        }

        if (colors != null && colors.Length > 0)
        {
            int colorCount = colors.Length / colorStride;
            int pointCount = System.Math.Min(points.Length, colorCount);
            for (int i = 0; i < pointCount; i++)
            {
                float r = (float)System.Math.Clamp(colors[i * colorStride + 0], 0.0, 1.0);
                float g = (float)System.Math.Clamp(colors[i * colorStride + 1], 0.0, 1.0);
                float b = (float)System.Math.Clamp(colors[i * colorStride + 2], 0.0, 1.0);
                float a = colorStride == 4
                    ? (float)System.Math.Clamp(colors[i * colorStride + 3], 0.0, 1.0)
                    : 1.0f;
                vertexColors.Add(new Vector4(r, g, b, a));
            }

            for (int i = pointCount; i < points.Length; i++)
            {
                vertexColors.Add(Vector4.One);
            }
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
            {
                vertexColors.Add(new Vector4(0.2f, 0.6f, 1.0f, 1.0f));
            }
        }

        Vector3 bmin = vertices.Count > 0 ? vertices[0] : Vector3.Zero;
        Vector3 bmax = bmin;
        foreach (Vector3 v in vertices)
        {
            bmin = Vector3.Min(bmin, v);
            bmax = Vector3.Max(bmax, v);
        }

        return new Plot3DResult
        {
            Vertices = vertices,
            Faces = [],
            Normals = [],
            VertexColors = vertexColors,
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.PointCloud
        };
    }
}
