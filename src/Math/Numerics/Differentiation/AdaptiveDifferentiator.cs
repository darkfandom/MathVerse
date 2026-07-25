namespace MathVerse.Math.Numerics.Differentiation;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public static class AdaptiveDifferentiator
{
    public static DerivativeResult Derivative(Func<double, double> f, double x, DerivativeOptions? options = null)
    {
        return Differentiator.Derivative(f, x, options);
    }

    public static DerivativeResult DerivativeRichardson(Func<double, double> f, double x, double h, int maxOrder = 8)
    {
        var table = new double[maxOrder + 1, maxOrder + 1];
        double hCurrent = h;

        for (int i = 0; i <= maxOrder; i++)
        {
            table[i, 0] = (f(x + hCurrent) - f(x - hCurrent)) / (2 * hCurrent);
            hCurrent /= 2;
        }

        for (int j = 1; j <= maxOrder; j++)
        {
            for (int i = 0; i <= maxOrder - j; i++)
            {
                double factor = System.Math.Pow(4, j);
                table[i, j] = (factor * table[i + 1, j - 1] - table[i, j - 1]) / (factor - 1);
            }
        }

        double bestValue = table[0, maxOrder];
        double error = System.Math.Abs(table[0, maxOrder] - table[1, maxOrder - 1]);

        return new DerivativeResult(table[0, maxOrder], error, maxOrder + 1, true, ImmutableArray<double>.Empty);
    }

    public static Vector Gradient(Func<Vector, double> f, Vector x, DerivativeOptions? options = null)
    {
        return Differentiator.Gradient(f, x, options);
    }

    public static Matrix Jacobian(Func<Vector, Vector> f, Vector x, DerivativeOptions? options = null)
    {
        return Differentiator.Jacobian(f, x, options);
    }

    public static Matrix Hessian(Func<Vector, double> f, Vector x, DerivativeOptions? options = null)
    {
        return Differentiator.Hessian(f, x, options);
    }
}