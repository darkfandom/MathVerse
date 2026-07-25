namespace MathVerse.Math.Numerics.RootFinding;

using System;

public interface IRootFinder
{
    RootResult FindRoot(Func<double, double> f, double initialGuess, RootOptions? options = null);
    RootResult FindRootBracketed(Func<double, double> f, double a, double b, RootOptions? options = null);
}