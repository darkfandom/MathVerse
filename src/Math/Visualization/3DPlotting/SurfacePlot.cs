namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates surface plots of z = f(x, y).</summary>
public sealed class SurfacePlot
{
    /// <summary>Creates a surface plot from a scalar function of two variables.</summary>
    /// <param name="zFunc">The function z = f(x, y) mapping coordinates to height.</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A Plot3DResult containing the surface mesh.</returns>
    public static Plot3DResult Create(
        Func<double, double, double> zFunc,
        double xMin, double xMax,
        double yMin, double yMax,
        int resolution = 50)
    {
        ArgumentNullException.ThrowIfNull(zFunc);
        if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 2.");

        List<Vector3> vertices = [];
        List<int[]> faces = [];
        List<Vector3> normals = [];
        List<Vector4> vertexColors = [];

        int cols = resolution + 1;
        double xRange = xMax - xMin;
        double yRange = yMax - yMin;

        double zMinVal = double.MaxValue;
        double zMaxVal = double.MinValue;

        for (int j = 0; j <= resolution; j++)
        {
            double y = yMin + j * yRange / resolution;
            for (int i = 0; i <= resolution; i++)
            {
                double x = xMin + i * xRange / resolution;
                double z = zFunc(x, y);
                if (double.IsNaN(z) || double.IsInfinity(z)) z = 0.0;
                vertices.Add(new Vector3((float)x, (float)z, (float)y));
                if (z < zMinVal) zMinVal = z;
                if (z > zMaxVal) zMaxVal = z;
            }
        }

        double zSpan = zMaxVal - zMinVal;
        if (zSpan < 1e-12) zSpan = 1.0;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double zNorm = (vertices[j * cols + i].Y - (float)zMinVal) / zSpan;
                float r = (float)System.Math.Clamp(zNorm, 0.0, 1.0);
                float g = (float)System.Math.Clamp(1.0 - System.Math.Abs(zNorm - 0.5) * 2.0, 0.0, 1.0);
                float b = (float)System.Math.Clamp(1.0 - zNorm, 0.0, 1.0);
                vertexColors.Add(new Vector4(r, g, b, 1.0f));
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

        ComputeNormalsForFaces(vertices, faces, normals);

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

    /// <summary>Computes smooth per-vertex normals by averaging face normals for shared vertices.</summary>
    /// <param name="vertices">The vertex list.</param>
    /// <param name="faces">The face index arrays.</param>
    /// <param name="normals">The output normal list to populate.</param>
    internal static void ComputeNormalsForFaces(
        List<Vector3> vertices,
        List<int[]> faces,
        List<Vector3> normals)
    {
        normals.Clear();
        for (int i = 0; i < vertices.Count; i++)
        {
            normals.Add(Vector3.Zero);
        }

        foreach (int[] face in faces)
        {
            if (face.Length < 3) continue;
            Vector3 v0 = vertices[face[0]];
            Vector3 v1 = vertices[face[1]];
            Vector3 v2 = vertices[face[2]];
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 faceNormal = Vector3.Cross(edge1, edge2);

            for (int i = 0; i < face.Length; i++)
            {
                normals[face[i]] += faceNormal;
            }
        }

        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = Vector3.Normalize(normals[i]);
            if (float.IsNaN(normals[i].X) || float.IsInfinity(normals[i].X))
            {
                normals[i] = Vector3.UnitY;
            }
        }
    }
}
