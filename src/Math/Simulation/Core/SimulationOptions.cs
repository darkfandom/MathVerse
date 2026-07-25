namespace MathVerse.Math.Simulation.Core;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;

public sealed record SimulationOptions
{
    public double StartTime { get; init; } = 0.0;
    public double EndTime { get; init; } = 10.0;
    public double MaxTimeStep { get; init; } = 0.1;
    public double MinTimeStep { get; init; } = 1e-6;
    public double Tolerance { get; init; } = 1e-6;
    public SimulationMode Mode { get; init; } = SimulationMode.FixedTimeStep;
    public bool EnableEventDetection { get; init; } = true;
    public bool EnableStateRecording { get; init; } = false;
    public int RecordingInterval { get; init; } = 1;
    public bool EnableParallelExecution { get; init; } = false;
    public int MaxSteps { get; init; } = 1000000;
    public bool EnableCheckpointing { get; init; } = false;
    public int CheckpointInterval { get; init; } = 1000;

    public static SimulationOptions Default { get; } = new();
}

public sealed record SimulationState
{
    public double CurrentTime { get; init; }
    public int StepCount { get; init; }
    public ImmutableDictionary<string, double> Variables { get; init; } = ImmutableDictionary<string, double>.Empty;
    public ImmutableDictionary<string, ImmutableArray<double>> History { get; init; } = ImmutableDictionary<string, ImmutableArray<double>>.Empty;
    public double LastTimeStep { get; init; }
    public SimulationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public ImmutableDictionary<string, object> CustomData { get; init; } = ImmutableDictionary<string, object>.Empty;

    public static SimulationState Create(double time, ImmutableDictionary<string, double> variables) =>
        new()
        {
            CurrentTime = time,
            StepCount = 0,
            Variables = variables,
            History = ImmutableDictionary<string, ImmutableArray<double>>.Empty,
            LastTimeStep = 0,
            Status = SimulationStatus.NotStarted
        };

    public double GetVariable(string name) => Variables.TryGetValue(name, out var v) ? v : double.NaN;

    public bool IsComplete => Status is SimulationStatus.Completed or SimulationStatus.Failed;
}

public enum SimulationStatus
{
    NotStarted,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public enum SimulationMode
{
    FixedTimeStep,
    VariableTimeStep,
    AdaptiveTimeStep,
    EventDriven
}
