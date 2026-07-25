namespace MathVerse.Math.CAS.Evaluation;

using System.Collections.Immutable;

public enum NumericMode
{
    Double,
    Complex,
    Interval,
    ArbitraryPrecision
}

public sealed record EvaluationOptions
{
    public bool AllowComplex { get; init; } = true;
    public bool ThrowOnUndefined { get; init; } = true;
    public double Tolerance { get; init; } = 1e-12;
    public NumericMode Mode { get; init; } = NumericMode.Double;

    public static EvaluationOptions Default { get; } = new();
    public static EvaluationOptions RealOnly { get; } = new() { AllowComplex = false };
    public static EvaluationOptions Strict { get; } = new() { ThrowOnUndefined = true };
}