namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum OptimizationStatus
{
    Converged,
    MaxIterations,
    LineSearchFailed,
    Diverged,
    NaNDetected
}

public sealed record OptimizationResult
{
    public Vector Optimum { get; init; }
    public double OptimalValue { get; init; }
    public bool Converged { get; init; }
    public int Iterations { get; init; }
    public int FunctionEvaluations { get; init; }
    public int GradientEvaluations { get; init; }
    public ImmutableArray<double> History { get; init; }
    public OptimizationStatus Status { get; init; }

    public OptimizationResult(Vector optimum, double optimalValue, bool converged, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history, OptimizationStatus status)
    {
        Optimum = optimum;
        OptimalValue = optimalValue;
        Converged = converged;
        Iterations = iterations;
        FunctionEvaluations = functionEvaluations;
        GradientEvaluations = gradientEvaluations;
        History = history;
        Status = status;
    }

    public static OptimizationResult ConvergedResult(Vector optimum, double optimalValue, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history)
        => new(optimum, optimalValue, true, iterations, functionEvaluations, gradientEvaluations, history, OptimizationStatus.Converged);

    public static OptimizationResult MaxIterationsResult(Vector optimum, double optimalValue, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history)
        => new(optimum, optimalValue, false, iterations, functionEvaluations, gradientEvaluations, history, OptimizationStatus.MaxIterations);

    public static OptimizationResult LineSearchFailedResult(Vector optimum, double optimalValue, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history)
        => new(optimum, optimalValue, false, iterations, functionEvaluations, gradientEvaluations, history, OptimizationStatus.LineSearchFailed);

    public static OptimizationResult DivergedResult(Vector optimum, double optimalValue, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history)
        => new(optimum, optimalValue, false, iterations, functionEvaluations, gradientEvaluations, history, OptimizationStatus.Diverged);

    public static OptimizationResult NaNDetectedResult(Vector optimum, double optimalValue, int iterations, int functionEvaluations, int gradientEvaluations, ImmutableArray<double> history)
        => new(optimum, optimalValue, false, iterations, functionEvaluations, gradientEvaluations, history, OptimizationStatus.NaNDetected);
}