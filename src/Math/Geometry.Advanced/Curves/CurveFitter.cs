using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Advanced.Surfaces;

namespace MathVerse.Math.Geometry.Advanced.Curves;

/// <summary>
/// Represents a fitted B-spline curve along with the residual fitting error.
/// </summary>
/// <param name="ControlPoints">The computed control points of the fitted B-spline curve.</param>
/// <param name="Error">The root-mean-square fitting error over the input data points.</param>
public readonly record struct FittedCurve(ImmutableArray<Point3D> ControlPoints, double Error);

/// <summary>
/// Provides methods for fitting B-spline curves to point data using least-squares optimization.
/// </summary>
public static class CurveFitter
{
    /// <summary>
    /// Fits a B-spline curve to the given data points using a least-squares approach. The method constructs a uniform
    /// clamped knot vector, builds the basis function matrix, and solves the normal equations to determine optimal control points.
    /// </summary>
    /// <param name="points">The data points to fit.</param>
    /// <param name="controlPointCount">The desired number of control points in the fitted curve. Must be at least degree + 1 and at most points.Count.</param>
    /// <param name="degree">The polynomial degree of the B-spline curve. Must be at least 1.</param>
    /// <returns>A <see cref="FittedCurve"/> containing the computed control points and the RMS fitting error.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are inconsistent or insufficient data is provided.</exception>
    public static FittedCurve FitBSpline(ImmutableArray<Point3D> points, int controlPointCount, int degree)
    {
        if (points.Length < 2)
            throw new ArgumentException("At least 2 data points are required.", nameof(points));
        if (degree < 1)
            throw new ArgumentException("Degree must be at least 1.", nameof(degree));
        if (controlPointCount < degree + 1)
            throw new ArgumentException($"Control point count must be at least {degree + 1} for degree {degree}.", nameof(controlPointCount));
        if (controlPointCount > points.Length)
            throw new ArgumentException("Control point count cannot exceed data point count.", nameof(controlPointCount));

        int n = controlPointCount;
        int m = points.Length;

        ImmutableArray<double> knots = BuildClampedUniformKnots(n, degree, m);

        double[,] basisMatrix = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            double t = (double)i / (m - 1);
            double tScaled = t * (knots[n] - knots[degree]) + knots[degree];

            for (int j = 0; j < n; j++)
            {
                basisMatrix[i, j] = BSplineSurfaceAdvanced.BasisFunction(knots, j, degree, tScaled);
            }
        }

        double[,] basisT = Transpose(basisMatrix, m, n);
        double[,] normalMatrix = Multiply(basisT, basisMatrix, n, m, n);

        double[] rhsX = MultiplyVec(basisT, points, m, n, p => p.X);
        double[] rhsY = MultiplyVec(basisT, points, m, n, p => p.Y);
        double[] rhsZ = MultiplyVec(basisT, points, m, n, p => p.Z);

        double[] cpx = SolveLinearSystem(normalMatrix, rhsX, n);
        double[] cpy = SolveLinearSystem(normalMatrix, rhsY, n);
        double[] cpz = SolveLinearSystem(normalMatrix, rhsZ, n);

        var controlPoints = ImmutableArray.CreateBuilder<Point3D>(n);
        for (int i = 0; i < n; i++)
            controlPoints.Add(new Point3D(cpx[i], cpy[i], cpz[i]));

        ImmutableArray<Point3D> controlPointsImmutable = controlPoints.MoveToImmutable();
        double error = ComputeRMSError(points, controlPointsImmutable, knots, degree);

        return new FittedCurve(controlPointsImmutable, error);
    }

    /// <summary>
    /// Evaluates a B-spline curve at regularly spaced parameter values.
    /// </summary>
    /// <param name="controlPoints">The control points of the B-spline curve.</param>
    /// <param name="knots">The knot vector for the B-spline curve.</param>
    /// <param name="degree">The polynomial degree of the B-spline curve.</param>
    /// <param name="samples">The number of evenly spaced sample points to evaluate. Must be at least 2.</param>
    /// <returns>An immutable array of evaluated points along the curve.</returns>
    public static ImmutableArray<Point3D> EvaluateBSpline(ImmutableArray<Point3D> controlPoints, ImmutableArray<double> knots, int degree, int samples)
    {
        if (samples < 2)
            throw new ArgumentException("Samples must be at least 2.", nameof(samples));

        int n = controlPoints.Length;
        var result = ImmutableArray.CreateBuilder<Point3D>(samples);

        double tMin = knots[degree];
        double tMax = knots[knots.Length - degree - 1];

        for (int s = 0; s < samples; s++)
        {
            double t = tMin + (double)s / (samples - 1) * (tMax - tMin);

            double x = 0.0, y = 0.0, z = 0.0;
            for (int i = 0; i < n; i++)
            {
                double b = BSplineSurfaceAdvanced.BasisFunction(knots, i, degree, t);
                x += b * controlPoints[i].X;
                y += b * controlPoints[i].Y;
                z += b * controlPoints[i].Z;
            }

            result.Add(new Point3D(x, y, z));
        }

        return result.MoveToImmutable();
    }

    /// <summary>
    /// Builds a clamped uniform knot vector for the given number of control points and degree.
    /// </summary>
    private static ImmutableArray<double> BuildClampedUniformKnots(int n, int degree, int dataCount)
    {
        int knotCount = n + degree + 1;
        var knots = ImmutableArray.CreateBuilder<double>(knotCount);

        for (int i = 0; i <= degree; i++)
            knots.Add(0.0);

        int innerCount = n - degree;
        if (innerCount > 0)
        {
            for (int i = 1; i <= innerCount; i++)
            {
                knots.Add((double)i / (innerCount + 1));
            }
        }

        for (int i = 0; i <= degree; i++)
            knots.Add(1.0);

        return knots.MoveToImmutable();
    }

    /// <summary>
    /// Computes the transpose of a matrix.
    /// </summary>
    private static double[,] Transpose(double[,] matrix, int rows, int cols)
    {
        double[,] result = new double[cols, rows];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];
        return result;
    }

    /// <summary>
    /// Multiplies two matrices: result = A^T * B where A is (m×k) and B is (m×n), result is (k×n).
    /// </summary>
    private static double[,] Multiply(double[,] at, double[,] b, int k, int m, int n)
    {
        double[,] result = new double[k, n];
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int p = 0; p < m; p++)
                    sum += at[i, p] * b[p, j];
                result[i, j] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// Multiplies a transposed matrix by a vector extracted from point data: result = A^T * v.
    /// </summary>
    private static double[] MultiplyVec(double[,] at, ImmutableArray<Point3D> points, int m, int k, Func<Point3D, double> selector)
    {
        double[] result = new double[k];
        for (int i = 0; i < k; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < m; j++)
                sum += at[i, j] * selector(points[j]);
            result[i] = sum;
        }
        return result;
    }

    /// <summary>
    /// Solves a symmetric positive-definite linear system Ax = b using Cholesky decomposition.
    /// </summary>
    private static double[] SolveLinearSystem(double[,] A, double[] b, int n)
    {
        double[,] L = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < j; k++)
                    sum += L[i, k] * L[j, k];

                if (i == j)
                {
                    double diag = A[i, i] - sum;
                    if (diag < 1e-15) diag = 1e-15;
                    L[i, j] = System.Math.Sqrt(diag);
                }
                else
                {
                    double denom = L[j, j];
                    L[i, j] = denom > 1e-15 ? (A[i, j] - sum) / denom : 0.0;
                }
            }
        }

        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int k = 0; k < i; k++)
                sum += L[i, k] * y[k];
            y[i] = (b[i] - sum) / L[i, i];
        }

        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0.0;
            for (int k = i + 1; k < n; k++)
                sum += L[k, i] * x[k];
            x[i] = (y[i] - sum) / L[i, i];
        }

        return x;
    }

    /// <summary>
    /// Computes the root-mean-square fitting error between data points and the evaluated B-spline curve.
    /// </summary>
    private static double ComputeRMSError(ImmutableArray<Point3D> dataPoints, ImmutableArray<Point3D> controlPoints, ImmutableArray<double> knots, int degree)
    {
        double sumSq = 0.0;
        int m = dataPoints.Length;

        for (int i = 0; i < m; i++)
        {
            double t = (double)i / (m - 1);
            double tMin = knots[degree];
            double tMax = knots[knots.Length - degree - 1];
            double tScaled = tMin + t * (tMax - tMin);

            double x = 0.0, y = 0.0, z = 0.0;
            for (int j = 0; j < controlPoints.Length; j++)
            {
                double b = BSplineSurfaceAdvanced.BasisFunction(knots, j, degree, tScaled);
                x += b * controlPoints[j].X;
                y += b * controlPoints[j].Y;
                z += b * controlPoints[j].Z;
            }

            double dx = dataPoints[i].X - x;
            double dy = dataPoints[i].Y - y;
            double dz = dataPoints[i].Z - z;
            sumSq += dx * dx + dy * dy + dz * dz;
        }

        return System.Math.Sqrt(sumSq / m);
    }
}
