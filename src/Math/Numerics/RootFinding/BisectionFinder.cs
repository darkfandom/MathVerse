namespace MathVerse.Math.Numerics.RootFinding;

using System;
using System.Collections.Immutable;
using static System.Math;

public sealed class BisectionFinder : IRootFinder
{
    public RootResult FindRoot(Func<double, double> f, double initialGuess, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
        var history = options.TrackHistory ? ImmutableArray.CreateBuilder<double>() : null;
        var (a, b) = BracketRoot(f, initialGuess, options);
        return FindRootBracketed(f, a, b, options);
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
                Abs(fa) < Abs(fb) ? Abs(fa) : Abs(fb),
                history?.ToImmutable() ?? ImmutableArray<double>.Empty,
                RootStatus.NoBracket);
        }

        if (fa == 0) return new RootResult(a, true, 0, 0, history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
        if (fb == 0) return new RootResult(b, true, 0, 0, history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);

        double c = 0;
        double fc = 0;
        int iterations = 0;

        for (int i = 0; i < options.MaxIterations; i++)
        {
            c = (a + b) * 0.5;
            fc = f(c);
            history?.Add(c);
            iterations++;

            if (Abs(fc) < options.Tolerance || (b - a) * 0.5 < options.Tolerance)
            {
                return new RootResult(c, true, iterations, Abs(fc), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
            }

            if (fa * fc <= 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        }

        return new RootResult(
            c,
            false,
            iterations,
            Abs(fc),
            history?.ToImmutable() ?? ImmutableArray<double>.Empty,
            RootStatus.MaxIterations);
    }

    private static (double a, double b) BracketRoot(Func<double, double> f, double initialGuess, RootOptions options)
    {
        if (options.RequireBracket)
        {
            throw new ArgumentException("Bracket required but not provided");
        }

        double a = initialGuess - 1;
        double b = initialGuess + 1;
        double fa = f(a);
        double fb = f(b);

        for (int i = 0; i < options.MaxIterations; i++)
        {
            if (fa * fb <= 0) return (a, b);

            if (Abs(fa) < Abs(fb))
            {
                b = a + 2 * (b - a);
                fb = f(b);
            }
            else
            {
                a = b - 2 * (b - a);
                fa = f(a);
            }
        }

        throw new ArgumentException("Could not find bracket containing root");
    }
}