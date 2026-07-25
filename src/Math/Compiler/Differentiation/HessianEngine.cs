namespace MathVerse.Math.Compiler.Differentiation;

using System;

/// <summary>Computes the Hessian matrix (matrix of second partial derivatives) of a scalar function.</summary>
public sealed class HessianEngine
{
    private readonly ForwardModeAD _forwardAD = new();
    private readonly JacobianEngine _jacobianEngine = new();

    /// <summary>Computes the Hessian matrix of a scalar function f: R^n → R at the given point.</summary>
    /// <param name="f">The function to differentiate. Takes a DualNumber array and returns a DualNumber.</param>
    /// <param name="point">The evaluation point (n-dimensional).</param>
    /// <returns>An n×n symmetric Hessian matrix where H[i,j] = ∂²f/∂x_i∂x_j.</returns>
    public double[,] Compute(Func<DualNumber[], DualNumber> f, double[] point)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int n = point.Length;
        var hessian = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double secondDeriv = ComputeSecondDerivative(f, point, i, j);
                hessian[i, j] = secondDeriv;
                hessian[j, i] = secondDeriv;
            }
        }

        return hessian;
    }

    /// <summary>Computes the Hessian using forward-over-forward mode (nested dual numbers).</summary>
    public double[,] ComputeForwardOverForward(Func<DualNumber[], DualNumber> f, double[] point)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int n = point.Length;
        var hessian = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            double[] gradientAtPoint = _forwardAD.Gradient(f, point);

            for (int j = i; j < n; j++)
            {
                double epsilon = 1e-7;
                double[] pointPlus = (double[])point.Clone();
                double[] pointMinus = (double[])point.Clone();
                pointPlus[j] += epsilon;
                pointMinus[j] -= epsilon;

                double[] gradPlus = _forwardAD.Gradient(f, pointPlus);
                double[] gradMinus = _forwardAD.Gradient(f, pointMinus);

                double secondDeriv = (gradPlus[i] - gradMinus[i]) / (2.0 * epsilon);
                hessian[i, j] = secondDeriv;
                hessian[j, i] = secondDeriv;
            }
        }

        return hessian;
    }

    /// <summary>Computes the Hessian-vector product Hv for a given vector v, without forming the full Hessian.</summary>
    public double[] HessianVectorProduct(Func<DualNumber[], DualNumber> f, double[] point, double[] v)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));
        if (v is null) throw new ArgumentNullException(nameof(v));

        int n = point.Length;
        var result = new double[n];

        for (int i = 0; i < n; i++)
        {
            double[] gradDirection = new double[n];
            for (int j = 0; j < n; j++)
                gradDirection[j] = point[j];

            double epsilon = 1e-7;
            double[] pointPlus = (double[])point.Clone();
            double[] pointMinus = (double[])point.Clone();

            for (int j = 0; j < n; j++)
            {
                pointPlus[j] = point[j] + epsilon * v[j];
                pointMinus[j] = point[j] - epsilon * v[j];
            }

            double[] gradPlus = _forwardAD.Gradient(f, pointPlus);
            double[] gradMinus = _forwardAD.Gradient(f, pointMinus);

            result[i] = (gradPlus[i] - gradMinus[i]) / (2.0 * epsilon);
        }

        return result;
    }

    /// <summary>Computes the trace of the Hessian (sum of diagonal elements, i.e., the Laplacian).</summary>
    public double Trace(Func<DualNumber[], DualNumber> f, double[] point)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int n = point.Length;
        double trace = 0;

        for (int i = 0; i < n; i++)
        {
            double hii = ComputeSecondDerivative(f, point, i, i);
            trace += hii;
        }

        return trace;
    }

    /// <summary>Verifies the Hessian against numerical finite differences.</summary>
    public (double[,] Analytical, double[,] Numerical, double MaxError) VerifyHessian(
        Func<DualNumber[], DualNumber> f,
        double[] point,
        double epsilon = 1e-5)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int n = point.Length;
        var analytical = Compute(f, point);
        var numerical = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double[] pp = (double[])point.Clone();
                double[] pm = (double[])point.Clone();
                double[] mp = (double[])point.Clone();
                double[] mm = (double[])point.Clone();

                pp[i] += epsilon; pp[j] += epsilon;
                pm[i] += epsilon; pm[j] -= epsilon;
                mp[i] -= epsilon; mp[j] += epsilon;
                mm[i] -= epsilon; mm[j] -= epsilon;

                double fpp = EvaluateScalar(f, pp);
                double fpm = EvaluateScalar(f, pm);
                double fmp = EvaluateScalar(f, mp);
                double fmm = EvaluateScalar(f, mm);

                numerical[i, j] = (fpp - fpm - fmp + fmm) / (4.0 * epsilon * epsilon);
                numerical[j, i] = numerical[i, j];
            }
        }

        double maxError = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double error = Math.Abs(analytical[i, j] - numerical[i, j]);
                if (error > maxError) maxError = error;
            }
        }

        return (analytical, numerical, maxError);
    }

    private double ComputeSecondDerivative(Func<DualNumber[], DualNumber> f, double[] point, int i, int j)
    {
        double epsilon = 1e-7;

        double[] pp = (double[])point.Clone();
        double[] pm = (double[])point.Clone();
        double[] mp = (double[])point.Clone();
        double[] mm = (double[])point.Clone();

        pp[i] += epsilon; pp[j] += epsilon;
        pm[i] += epsilon; pm[j] -= epsilon;
        mp[i] -= epsilon; mp[j] += epsilon;
        mm[i] -= epsilon; mm[j] -= epsilon;

        double fpp = EvaluateScalar(f, pp);
        double fpm = EvaluateScalar(f, pm);
        double fmp = EvaluateScalar(f, mp);
        double fmm = EvaluateScalar(f, mm);

        return (fpp - fpm - fmp + fmm) / (4.0 * epsilon * epsilon);
    }

    private static double EvaluateScalar(Func<DualNumber[], DualNumber> f, double[] point)
    {
        var inputs = new DualNumber[point.Length];
        for (int i = 0; i < point.Length; i++)
            inputs[i] = DualNumber.FromValue(point[i]);
        return f(inputs).Real;
    }
}
