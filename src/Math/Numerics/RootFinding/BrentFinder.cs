namespace MathVerse.Math.Numerics.RootFinding;

using System;
using System.Collections.Immutable;
using static System.Math;

public sealed class BrentFinder : IRootFinder
{
    public RootResult FindRoot(Func<double, double> f, double initialGuess, RootOptions? options = null)
    {
        options ??= RootOptions.Default;
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
                Min(Abs(fa), Abs(fb)),
                history?.ToImmutable() ?? ImmutableArray<double>.Empty,
                RootStatus.NoBracket);
        }

        if (Abs(fa) < Abs(fb))
        {
            Swap(ref a, ref b);
            Swap(ref fa, ref fb);
        }

        double c = a;
        double fc = fa;
        double d = 0;
        bool mflag = true;

        for (int iter = 0; iter < options.MaxIterations; iter++)
        {
            history?.Add(b);

            if (Abs(fb) < options.Tolerance || Abs(b - a) < options.Tolerance)
            {
                return new RootResult(b, true, iter + 1, Abs(fb), history?.ToImmutable() ?? ImmutableArray<double>.Empty, RootStatus.Converged);
            }

            double s;
            if (fa != fc && fb != fc)
            {
                s = a * fb * fc / ((fa - fb) * (fa - fc))
                  + b * fa * fc / ((fb - fa) * (fb - fc))
                  + c * fa * fb / ((fc - fa) * (fc - fb));
            }
            else
            {
                s = b - fb * (b - a) / (fb - fa);
            }

            double tol = 2 * Double.Epsilon * Abs(b) + options.Tolerance;
            double m = 0.5 * (a + b);

            bool useBisection = false;
            if (s < (3 * a + b) * 0.25 || s > b)
            {
                useBisection = true;
            }
            else if (mflag && Abs(s - b) >= 0.5 * Abs(b - c))
            {
                useBisection = true;
            }
            else if (!mflag && Abs(s - b) >= 0.5 * Abs(c - d))
            {
                useBisection = true;
            }
            else if (mflag && Abs(b - c) < tol)
            {
                useBisection = true;
            }
            else if (!mflag && Abs(c - d) < tol)
            {
                useBisection = true;
            }

            if (useBisection)
            {
                s = m;
                mflag = true;
            }
            else
            {
                mflag = false;
            }

            double fs = f(s);
            d = c;
            c = b;
            fc = fb;

            if (fa * fs < 0)
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }

            if (Abs(fa) < Abs(fb))
            {
                Swap(ref a, ref b);
                Swap(ref fa, ref fb);
            }
        }

        return new RootResult(
            b,
            false,
            options.MaxIterations,
            Abs(fb),
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

    private static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
}