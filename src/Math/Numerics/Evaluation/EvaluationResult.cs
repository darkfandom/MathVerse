namespace MathVerse.Math.Numerics.Evaluation;

using System.Collections.Immutable;
using System.Numerics;
using MathVerse.Math.Expressions;

public sealed record EvaluationResult
{
    public double Value { get; init; }
    public Complex ComplexValue { get; init; }
    public bool IsComplex { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int Iterations { get; init; }
    public ImmutableDictionary<string, double> VariableValues { get; init; } = ImmutableDictionary<string, double>.Empty;
    public ImmutableDictionary<string, Complex> ComplexVariableValues { get; init; } = ImmutableDictionary<string, Complex>.Empty;
    public TimeSpan EvaluationTime { get; init; }
    public int IterationCount { get; init; }
    public bool Converged { get; init; }
    public double ErrorEstimate { get; init; }

    public static EvaluationResult Success(double value, int iterations = 0, TimeSpan? time = null, double errorEstimate = 0, bool converged = true)
        => new() { Value = value, IsComplex = false, IsSuccess = true, Iterations = iterations, EvaluationTime = time ?? TimeSpan.Zero, IterationCount = iterations, Converged = converged, ErrorEstimate = errorEstimate };

    public static EvaluationResult Success(Complex value, int iterations = 0, TimeSpan? time = null, double errorEstimate = 0, bool converged = true)
        => new() { ComplexValue = value, IsComplex = true, IsSuccess = true, Iterations = iterations, EvaluationTime = time ?? TimeSpan.Zero, IterationCount = iterations, Converged = converged, ErrorEstimate = errorEstimate };

    public static EvaluationResult Failure(string errorMessage, TimeSpan? time = null)
        => new() { IsSuccess = false, ErrorMessage = errorMessage, EvaluationTime = time ?? TimeSpan.Zero };

    public static EvaluationResult Failure(string errorMessage, TimeSpan time, int iterations)
        => new() { IsSuccess = false, ErrorMessage = errorMessage, EvaluationTime = time, Iterations = iterations };

    public double GetRealValue() => IsComplex ? ComplexValue.Real : Value;

    public Complex GetComplexValue() => IsComplex ? ComplexValue : new Complex(Value, 0);

    public bool IsReal => !IsComplex;

    public bool IsZero(double tolerance = 1e-12) => IsComplex ? ComplexValue.Magnitude < tolerance : System.Math.Abs(Value) < tolerance;

    public override string ToString() => IsSuccess
        ? (IsComplex ? $"{ComplexValue.Real:G15}{(ComplexValue.Imaginary >= 0 ? "+" : "")}{ComplexValue.Imaginary:G15}i" : $"{Value:G15}")
        : $"Error: {ErrorMessage}";
}

public static class EvaluationResultExtensions
{
    public static EvaluationResult WithVariable(this EvaluationResult result, string name, double value)
        => result with { VariableValues = result.VariableValues.SetItem(name, value) };

    public static EvaluationResult WithComplexVariable(this EvaluationResult result, string name, Complex value)
        => result with { ComplexVariableValues = result.ComplexVariableValues.SetItem(name, value) };

    public static EvaluationResult WithVariables(this EvaluationResult result, IEnumerable<KeyValuePair<string, double>> variables)
        => result with { VariableValues = result.VariableValues.AddRange(variables) };

    public static EvaluationResult WithComplexVariables(this EvaluationResult result, IEnumerable<KeyValuePair<string, Complex>> variables)
        => result with { ComplexVariableValues = result.ComplexVariableValues.AddRange(variables) };

    public static EvaluationResult WithError(this EvaluationResult result, double error)
        => result with { ErrorEstimate = error };

    public static EvaluationResult WithIterations(this EvaluationResult result, int iterations)
        => result with { Iterations = iterations, IterationCount = iterations };

    public static EvaluationResult WithTime(this EvaluationResult result, TimeSpan time)
        => result with { EvaluationTime = time };

    public static bool IsSuccess(this EvaluationResult? result) => result?.IsSuccess ?? false;

    public static bool IsFailure(this EvaluationResult? result) => result?.IsSuccess != true;
}