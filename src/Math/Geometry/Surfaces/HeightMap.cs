namespace MathVerse.Math.Geometry.Surfaces;

using Geometry3D;
using Mesh;

/// <summary>Represents a height map surface defined by a 2D grid of height values.</summary>
public sealed class HeightMap
{
    /// <summary>Initializes a new instance of the <see cref="HeightMap"/> class.</summary>
    /// <param name="heights">The 2D array of height values [x, y].</param>
    /// <param name="xMin">The minimum x bound.</param>
    /// <param name="xMax">The maximum x bound.</param>
    /// <param name="yMin">The minimum y bound.</param>
    /// <param name="yMax">The maximum y bound.</param>
    public HeightMap(double[,] heights, double xMin, double xMax, double yMin, double yMax)
    {
        Heights = heights ?? throw new ArgumentNullException(nameof(heights));
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }

    /// <summary>Gets the 2D array of height values.</summary>
    public double[,] Heights { get; }

    /// <summary>Gets the minimum x bound.</summary>
    public double XMin { get; }

    /// <summary>Gets the maximum x bound.</summary>
    public double XMax { get; }

    /// <summary>Gets the minimum y bound.</summary>
    public double YMin { get; }

    /// <summary>Gets the maximum y bound.</summary>
    public double YMax { get; }

    /// <summary>Gets the number of samples along the x axis.</summary>
    public int Width => Heights.GetLength(0);

    /// <summary>Gets the number of samples along the y axis.</summary>
    public int Height => Heights.GetLength(1);

    /// <summary>Gets the minimum height value in the grid.</summary>
    public double Min
    {
        get
        {
            double min = double.MaxValue;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (Heights[i, j] < min) min = Heights[i, j];
            return min;
        }
    }

    /// <summary>Gets the maximum height value in the grid.</summary>
    public double Max
    {
        get
        {
            double max = double.MinValue;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (Heights[i, j] > max) max = Heights[i, j];
            return max;
        }
    }

    /// <summary>Evaluates the height at continuous coordinates (x, y) using bilinear interpolation.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The interpolated height value.</returns>
    public double Evaluate(double x, double y)
    {
        double fx = (x - XMin) / (XMax - XMin) * (Width - 1);
        double fy = (y - YMin) / (YMax - YMin) * (Height - 1);

        int ix = (int)System.Math.Floor(fx);
        int iy = (int)System.Math.Floor(fy);

        ix = System.Math.Clamp(ix, 0, Width - 2);
        iy = System.Math.Clamp(iy, 0, Height - 2);

        double tx = fx - ix;
        double ty = fy - iy;

        double v00 = Heights[ix, iy];
        double v10 = Heights[ix + 1, iy];
        double v01 = Heights[ix, iy + 1];
        double v11 = Heights[ix + 1, iy + 1];

        double v0 = v00 + (v10 - v00) * tx;
        double v1 = v01 + (v11 - v01) * tx;
        return v0 + (v1 - v0) * ty;
    }

    /// <summary>Computes the unit surface normal at (x, y) via finite differences.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The unit normal vector.</returns>
    public Vector3D Normal(double x, double y)
    {
        double eps = (XMax - XMin) / (Width - 1) * 0.5;
        double epsY = (YMax - YMin) / (Height - 1) * 0.5;

        double dhdx = (Evaluate(x + eps, y) - Evaluate(x - eps, y)) / (2.0 * eps);
        double dhdy = (Evaluate(x, y + epsY) - Evaluate(x, y - epsY)) / (2.0 * epsY);

        Vector3D normal = new Vector3D(-dhdx, -dhdy, 1.0);
        return normal.Normalize();
    }

    /// <summary>Generates a triangle mesh from the height map data.</summary>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A triangle mesh representing the height map surface.</returns>
    public TriangleMesh ToMesh(int resolution)
    {
        var builder = new MeshBuilder();
        int cols = resolution + 1;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double x = XMin + (XMax - XMin) * i / resolution;
                double y = YMin + (YMax - YMin) * j / resolution;
                double z = Evaluate(x, y);
                builder.AddVertex(new Point3D(x, y, z));
            }
        }

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                int topLeft = j * cols + i;
                int topRight = topLeft + 1;
                int bottomLeft = (j + 1) * cols + i;
                int bottomRight = bottomLeft + 1;

                builder.AddTriangle(topLeft, bottomLeft, topRight);
                builder.AddTriangle(topRight, bottomLeft, bottomRight);
            }
        }

        return builder.ToMesh();
    }
}
