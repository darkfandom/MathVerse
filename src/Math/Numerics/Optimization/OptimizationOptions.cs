namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;

public enum LineSearchMethod
{
    Backtracking,
    Armijo,
    Wolfe,
    StrongWolfe
}

public sealed record OptimizationOptions
{
    public double Tolerance { get; init; } = 1e-10;
    public int MaxIterations { get; init; } = 1000;
    public bool TrackHistory { get; init; } = false;
    public double StepSize { get; init; } = 1e-6;
    public LineSearchMethod LineSearch { get; init; } = LineSearchMethod.Backtracking;
    public double ArmijoC1 { get; init; } = 1e-4;
    public double WolfeC2 { get; init; } = 0.9;

    public static OptimizationOptions Default { get; } = new();
}