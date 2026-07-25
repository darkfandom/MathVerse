namespace MathVerse.Math.Numerics.Optimization;

using MathVerse.Math.Numerics.LinearAlgebra;

public static class LineSearch
{
    public static double Backtracking(Func<Vector, double> f, Vector x, Vector d, double f0, Vector g0, double c1 = 1e-4, double rho = 0.5, double alphaInit = 1.0, int maxIter = 50)
    {
        double alpha = 1.0;
        double fNew;
        Vector xNew;

        for (int i = 0; i < 50; i++)
        {
            xNew = x.Add(d.Scale(alpha));
            fNew = f(xNew);

            if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                return double.NaN;

            if (fNew <= f0 + c1 * alpha * g0.Dot(d))
                return alpha;

            alpha *= rho;
        }

        return double.NaN;
    }

    public static double Armijo(Func<Vector, double> f, Vector x, Vector d, double f0, Vector g0, double c1 = 1e-4, double rho = 0.5, double alphaInit = 1.0, int maxIter = 50)
        => Backtracking(f, x, d, f0, g0, c1, rho, alphaInit, maxIter);

    public static double Wolfe(Func<Vector, double> f, Func<Vector, Vector> grad, Vector x, Vector d, double f0, Vector g0, double c1 = 1e-4, double c2 = 0.9, double alphaInit = 1.0, int maxIter = 50)
    {
        double alpha = alphaInit;
        double alphaMin = 0.0;
        double alphaMax = double.PositiveInfinity;
        Vector xNew;
        double fNew;
        Vector gNew;

        for (int i = 0; i < maxIter; i++)
        {
            xNew = x.Add(d.Scale(alpha));
            fNew = f(xNew);

            if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                return double.NaN;

            if (fNew > f0 + c1 * alpha * g0.Dot(d))
            {
                alphaMax = alpha;
                alpha = (alphaMin + alphaMax) / 2.0;
                continue;
            }

            gNew = grad(xNew);
            double dgNew = gNew.Dot(d);

            if (dgNew < c2 * g0.Dot(d))
            {
                alphaMin = alpha;
                alpha = double.IsPositiveInfinity(alphaMax) ? alpha * 2.0 : (alphaMin + alphaMax) / 2.0;
                continue;
            }

            return alpha;
        }

        return double.NaN;
    }

    public static double StrongWolfe(Func<Vector, double> f, Func<Vector, Vector> grad, Vector x, Vector d, double f0, Vector g0, double c1 = 1e-4, double c2 = 0.9, double alphaInit = 1.0, int maxIter = 50)
    {
        double alpha = alphaInit;
        double alphaMin = 0.0;
        double alphaMax = double.PositiveInfinity;
        Vector xNew;
        double fNew;
        Vector gNew;

        for (int i = 0; i < maxIter; i++)
        {
            xNew = x.Add(d.Scale(alpha));
            fNew = f(xNew);

            if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                return double.NaN;

            if (fNew > f0 + c1 * alpha * g0.Dot(d))
            {
                alphaMax = alpha;
                alpha = (alphaMin + alphaMax) / 2.0;
                continue;
            }

            gNew = grad(xNew);
            double dgNew = gNew.Dot(d);

            if (System.Math.Abs(dgNew) <= -c2 * g0.Dot(d))
                return alpha;

            if (dgNew > 0.0)
            {
                alphaMax = alpha;
                alpha = (alphaMin + alphaMax) / 2.0;
            }
            else
            {
                alphaMin = alpha;
                alpha = double.IsPositiveInfinity(alphaMax) ? alpha * 2.0 : (alphaMin + alphaMax) / 2.0;
            }
        }

        return double.NaN;
    }

    public static Vector FiniteDifferenceGradient(Func<Vector, double> f, Vector x, double h = 1e-8)
    {
        int n = x.Size;
        var grad = new double[n];
        double f0 = f(x);

        for (int i = 0; i < n; i++)
        {
            var xPlus = x.ToArray();
            xPlus[i] += h;
            double fPlus = f(new Vector(xPlus));
            grad[i] = (fPlus - f0) / h;
        }

        return new Vector(grad);
    }
    
    public static double PerformLineSearch(OptimizationOptions options, Func<Vector, double> f, Func<Vector, Vector> grad, Vector x, Vector d, double f0, Vector g0, double c1, double c2, double stepSize)
    {
        return options.LineSearch switch
        {
            LineSearchMethod.Backtracking => Backtracking(f, x, d, f0, g0, c1, 0.5, stepSize),
            LineSearchMethod.Armijo => Armijo(f, x, d, f0, g0, c1, 0.5, stepSize),
            LineSearchMethod.Wolfe => Wolfe(f, grad, x, d, f0, g0, c1, c2, stepSize),
            LineSearchMethod.StrongWolfe => StrongWolfe(f, grad, x, d, f0, g0, c1, c2, stepSize),
            _ => Backtracking(f, x, d, f0, g0, c1, 0.5, stepSize)
        };
    }
}