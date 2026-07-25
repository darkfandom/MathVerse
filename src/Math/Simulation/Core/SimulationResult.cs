namespace MathVerse.Math.Simulation.Core;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;

public sealed record SimulationResult
{
    public SimulationStatus Status { get; init; }
    public SimulationState FinalState { get; init; } = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
    public int TotalSteps { get; init; }
    public double TotalTime { get; init; }
    public int EventCount { get; init; }
    public int FunctionEvaluations { get; init; }
    public ImmutableDictionary<string, object> Statistics { get; init; } = ImmutableDictionary<string, object>.Empty;
    public string? ErrorMessage { get; init; }
    public TimeSpan ExecutionTime { get; init; }

    public static SimulationResult Success(SimulationState state, int steps, double time, int events, int evals, TimeSpan duration) =>
        new()
        {
            Status = SimulationStatus.Completed,
            FinalState = state,
            TotalSteps = steps,
            TotalTime = time,
            EventCount = events,
            FunctionEvaluations = evals,
            ExecutionTime = duration
        };

    public static SimulationResult Failure(string error, TimeSpan duration) =>
        new()
        {
            Status = SimulationStatus.Failed,
            ErrorMessage = error,
            ExecutionTime = duration
        };
}