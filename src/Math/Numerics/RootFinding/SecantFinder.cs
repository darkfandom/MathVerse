namespace MathVerse.Math.Numerics.RootFinding;

using System;
using System.Collections.Immutable;
using static System.Math;

public sealed class SecantFinder : IRootFinder
{
    public RootResult FindRoot(Func<double, double> f, double initialGuess, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
        var history = options.TrackHistory ? ImmutableArray.CreateBuilder<double>() : null;

        double x0 = initialGuess;
        double x1 = initialGuess * 1.001 + 1e-6;
        double fx0 = f(x0);
        double fx1 = f(x1);
        history?.Add(x0);
        history?.Add(x1);

        if (Abs(fx0) < Abs(fx1))
        {
            double temp = x0; x0 = x1; x1 = temp;
            temp = fx0; fx0 = fx1; fx1 = temp;
        }

        if (Abs(fx1) < options.Tolerance)
        {
            return new RootResult(x1, true, 1, Abs(fx1), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
        }

        for (int i = 0; i < options.MaxIterations - 1; i++)
        {
            double denom = fx1 - fx0;
            if (Abs(denom) < 1e-14)
            {
                return new RootResult(x1, false, i + 1, Abs(fx1), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Singular);
            }

            double x2 = x1 - fx1 * (x1 - x0) / denom;
            history?.Add(x2);

            double fx2 = f(x2);

            if (Abs(fx2) < options.Tolerance || Abs(x2 - x1) < options.Tolerance)
            {
                return new RootResult(x2, true, i + 2, Abs(fx2), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
            }

            if (Abs(x2) > 1e100 || Double.IsNaN(x2) || Double.IsInfinity(x2))
            {
                return new RootResult(x2, false, i + 2, Abs(fx2), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Diverged);
            }

            x0 = x1;
            x1 = x2;
            fx0 = fx1;
            fx1 = fx2;
        }

        return new RootResult(x1, false, options.MaxIterations, Abs(fx1), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.MaxIterations);
    }

    public RootResult FindRootBracketed(Func<double, double> f, double a, double b, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
        var history = options.TrackHistory ? ImmutableArray.CreateBuilder<double>() : null;

        double fa = f(a);
        double fb = f(b);

        if (fa * fb >= 0)
        {
            return new RootResult(
                (a + b) * 0.5,
                false,
                0,
                Min(Abs(fa), Abs(fb)),
                history?.ToImmutable() ?? ImmutableArray<double>.Empty,
                RootStatus.NoBracket);
        }

        return FindRoot(f, a, options);
    }
}