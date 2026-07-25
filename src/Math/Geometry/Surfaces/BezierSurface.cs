namespace MathVerse.Math.Geometry.Surfaces;

using Geometry3D;

/// <summary>Represents a tensor-product Bezier surface defined by a 2D grid of control points.</summary>
public sealed class BezierSurface
{
    /// <summary>Initializes a new instance of the <see cref="BezierSurface"/> class.</summary>
    /// <param name="controlPoints">The 2D array of control points.</param>
    public BezierSurface(ImmutableArray<ImmutableArray<Point3D>> controlPoints)
    {
        ControlPoints = controlPoints.IsDefault
            ? ImmutableArray<ImmutableArray<Point3D>>.Empty
            : controlPoints;
    }

    /// <summary>Gets the 2D array of control points.</summary>
    public ImmutableArray<ImmutableArray<Point3D>> ControlPoints { get; }

    /// <summary>Gets the degree in the u direction.</summary>
    public int DegreeU => ControlPoints.Length > 0 ? ControlPoints[0].Length - 1 : 0;

    /// <summary>Gets the degree in the v direction.</summary>
    public int DegreeV => ControlPoints.Length - 1;

    /// <summary>Evaluates the surface at parameters (u, v) using De Casteljau's algorithm in both directions.</summary>
    /// <param name="u">The u parameter value in [0, 1].</param>
    /// <param name="v">The v parameter value in [0, 1].</param>
    /// <returns>The point on the surface.</returns>
    public Point3D PointAt(double u, double v)
    {
        if (ControlPoints.Length == 0) return Point3D.Origin;

        int rows = ControlPoints.Length;
        int cols = ControlPoints[0].Length;

        var temp = new Point3D[rows];
        for (int i = 0; i < rows; i++)
        {
            temp[i] = DeCasteljau1D(ControlPoints[i], v);
        }

        return DeCasteljau1DArray(temp, u);
    }

    /// <summary>Computes the unit surface normal at (u, v) via the cross product of partial derivatives.</summary>
    /// <param name="u">The u parameter value in [0, 1].</param>
    /// <param name="v">The v parameter value in [0, 1].</param>
    /// <returns>The unit normal vector.</returns>
    public Vector3D Normal(double u, double v)
    {
        double eps = 1e-5;
        Point3D center = PointAt(u, v);
        Point3D du = PointAt(System.Math.Min(u + eps, 1.0), v);
        Point3D dv = PointAt(u, System.Math.Min(v + eps, 1.0));

        Vector3D tangentU = new Vector3D(du.X - center.X, du.Y - center.Y, du.Z - center.Z);
        Vector3D tangentV = new Vector3D(dv.X - center.X, dv.Y - center.Y, dv.Z - center.Z);

        return tangentU.Cross(tangentV).Normalize();
    }

    /// <summary>Samples the surface on a regular grid.</summary>
    /// <param name="uCount">The number of samples along the u axis.</param>
    /// <param name="vCount">The number of samples along the v axis.</param>
    /// <returns>A 2D immutable array of sampled points.</returns>
    public ImmutableArray<ImmutableArray<Point3D>> Sample(int uCount, int vCount)
    {
        var rows = ImmutableArray.CreateBuilder<ImmutableArray<Point3D>>(uCount);
        for (int i = 0; i < uCount; i++)
        {
            double u = (double)i / (uCount - 1);
            var row = ImmutableArray.CreateBuilder<Point3D>(vCount);
            for (int j = 0; j < vCount; j++)
            {
                double v = (double)j / (vCount - 1);
                row.Add(PointAt(u, v));
            }
            rows.Add(row.MoveToImmutable());
        }
        return rows.MoveToImmutable();
    }

    private static Point3D DeCasteljau1D(ImmutableArray<Point3D> points, double t)
    {
        int n = points.Length;
        if (n == 0) return Point3D.Origin;
        if (n == 1) return points[0];

        var temp = new Point3D[n];
        points.CopyTo(temp, 0);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        return temp[0];
    }

    private static Point3D DeCasteljau1DArray(Point3D[] points, double t)
    {
        int n = points.Length;
        if (n == 0) return Point3D.Origin;
        if (n == 1) return points[0];

        var temp = new Point3D[n];
        Array.Copy(points, temp, n);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        return temp[0];
    }
}
