namespace MathVerse.Math.Geometry.Surfaces;

using Geometry3D;

/// <summary>Represents a tensor-product B-spline surface.</summary>
public sealed class BSplineSurface
{
    /// <summary>Initializes a new instance of the <see cref="BSplineSurface"/> class.</summary>
    /// <param name="knotsU">The knot vector in the u direction.</param>
    /// <param name="knotsV">The knot vector in the v direction.</param>
    /// <param name="controlPoints">The 2D grid of control points [u][v].</param>
    /// <param name="degreeU">The degree in the u direction.</param>
    /// <param name="degreeV">The degree in the v direction.</param>
    public BSplineSurface(
        ImmutableArray<double> knotsU,
        ImmutableArray<double> knotsV,
        ImmutableArray<ImmutableArray<Point3D>> controlPoints,
        int degreeU,
        int degreeV)
    {
        KnotsU = knotsU.IsDefault ? ImmutableArray<double>.Empty : knotsU;
        KnotsV = knotsV.IsDefault ? ImmutableArray<double>.Empty : knotsV;
        ControlPoints = controlPoints.IsDefault
            ? ImmutableArray<ImmutableArray<Point3D>>.Empty
            : controlPoints;
        DegreeU = degreeU;
        DegreeV = degreeV;
    }

    /// <summary>Gets the knot vector in the u direction.</summary>
    public ImmutableArray<double> KnotsU { get; }

    /// <summary>Gets the knot vector in the v direction.</summary>
    public ImmutableArray<double> KnotsV { get; }

    /// <summary>Gets the 2D grid of control points.</summary>
    public ImmutableArray<ImmutableArray<Point3D>> ControlPoints { get; }

    /// <summary>Gets the degree in the u direction.</summary>
    public int DegreeU { get; }

    /// <summary>Gets the degree in the v direction.</summary>
    public int DegreeV { get; }

    /// <summary>Evaluates the surface at parameters (u, v) using Cox-de Boor basis functions.</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The point on the surface.</returns>
    public Point3D PointAt(double u, double v)
    {
        if (ControlPoints.Length == 0) return Point3D.Origin;

        double[] Nu = EvaluateBasis(KnotsU, DegreeU, u);
        double[] Nv = EvaluateBasis(KnotsV, DegreeV, v);

        int countU = ControlPoints.Length;
        int countV = ControlPoints[0].Length;

        double px = 0.0, py = 0.0, pz = 0.0;
        for (int i = 0; i < countU; i++)
        {
            for (int j = 0; j < countV; j++)
            {
                double basis = Nu[i] * Nv[j];
                px += basis * ControlPoints[i][j].X;
                py += basis * ControlPoints[i][j].Y;
                pz += basis * ControlPoints[i][j].Z;
            }
        }

        return new Point3D(px, py, pz);
    }

    /// <summary>Computes the unit surface normal at (u, v) via the cross product of partial derivatives.</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The unit normal vector.</returns>
    public Vector3D Normal(double u, double v)
    {
        double eps = 1e-5;
        Point3D center = PointAt(u, v);
        Point3D du = PointAt(u + eps, v);
        Point3D dv = PointAt(u, v + eps);

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
        double uMin = KnotsU[DegreeU];
        double uMax = KnotsU[KnotsU.Length - DegreeU - 1];
        double vMin = KnotsV[DegreeV];
        double vMax = KnotsV[KnotsV.Length - DegreeV - 1];

        var rows = ImmutableArray.CreateBuilder<ImmutableArray<Point3D>>(uCount);
        for (int i = 0; i < uCount; i++)
        {
            double u = uMin + (uMax - uMin) * i / (uCount - 1);
            var row = ImmutableArray.CreateBuilder<Point3D>(vCount);
            for (int j = 0; j < vCount; j++)
            {
                double v = vMin + (vMax - vMin) * j / (vCount - 1);
                row.Add(PointAt(u, v));
            }
            rows.Add(row.MoveToImmutable());
        }
        return rows.MoveToImmutable();
    }

    private static double[] EvaluateBasis(ImmutableArray<double> knots, int degree, double t)
    {
        int n = knots.Length - degree - 1;
        double[] N = new double[n];

        for (int i = 0; i < n; i++)
        {
            bool inSpan = t >= knots[i] && t < knots[i + degree + 1];
            bool isLast = i == n - 1 && System.Math.Abs(t - knots[knots.Length - 1]) < 1e-12;
            N[i] = (inSpan || isLast) ? 1.0 : 0.0;
        }

        for (int k = 1; k <= degree; k++)
        {
            double[] Nprev = (double[])N.Clone();
            for (int i = 0; i < n; i++)
            {
                double left = 0.0;
                double right = 0.0;

                double denomLeft = knots[i + k] - knots[i];
                if (System.Math.Abs(denomLeft) > 1e-15)
                    left = ((t - knots[i]) / denomLeft) * Nprev[i];

                double denomRight = knots[i + k + 1] - knots[i + 1];
                if (System.Math.Abs(denomRight) > 1e-15)
                    right = ((knots[i + k + 1] - t) / denomRight) * Nprev[i + 1];

                N[i] = left + right;
            }
        }

        return N;
    }
}
