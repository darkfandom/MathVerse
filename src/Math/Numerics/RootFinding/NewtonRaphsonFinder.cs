namespace MathVerse.Math.Numerics.RootFinding;

using System;
using System.Collections.Immutable;
using static System.Math;

public sealed class NewtonRaphsonFinder : IRootFinder
{
    public RootResult FindRoot(Func<double, double> f, double initialGuess, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
        var history = options.TrackHistory ? ImmutableArray.CreateBuilder<double>() : null;

        double x = initialGuess;
        double fx = f(x);
        history?.Add(x);

        if (Abs(fx) < options.Tolerance)
        {
            return new RootResult(x, true, 0, Abs(fx), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
        }

        for (int i = 0; i < options.MaxIterations; i++)
        {
            double h = Max(Abs(x) * 1e-8, 1e-12);
            double fxh = f(x + h);
            double df = (fxh - fx) / h;

            if (Abs(df) < 1e-14)
            {
                return new RootResult(x, false, i, Abs(fx), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Singular);
            }

            double xNew = x - fx / df;
            history?.Add(xNew);

            double fxNew = f(xNew);

            if (Abs(fxNew) < options.Tolerance || Abs(xNew - x) < options.Tolerance)
            {
                return new RootResult(xNew, true, i + 1, Abs(fxNew), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
            }

            if (Abs(xNew) > 1e100 || Double.IsNaN(xNew) || Double.IsInfinity(xNew))
            {
                return new RootResult(xNew, false, i + 1, Abs(fxNew), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Diverged);
            }

            x = xNew;
            fx = fxNew;
        }

        return new RootResult(x, false, options.MaxIterations, Abs(fx), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.MaxIterations);
    }

    public RootResult FindRootBracketed(Func<double, double> f, double a, double b, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
        return FindRoot(f, (a + b) * 0.5, options);
    }
}