namespace MathVerse.Math.Simulation.Diagnostics;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Physics;

public sealed record SimulationDiagnostic
{
    public DiagnosticType Type { get; init; }
    public DiagnosticSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public double Time { get; init; }
    public ImmutableDictionary<string, object> Context { get; init; } = ImmutableDictionary<string, object>.Empty;

    public static SimulationDiagnostic Warning(string message, double time, ImmutableDictionary<string, object>? context = null)
        => new() { Type = DiagnosticType.NumericalWarning, Severity = DiagnosticSeverity.Warning, Message = message, Time = time, Context = context ?? ImmutableDictionary<string, object>.Empty };

    public static SimulationDiagnostic Error(string message, double time, ImmutableDictionary<string, object>? context = null)
        => new() { Type = DiagnosticType.Error, Severity = DiagnosticSeverity.Error, Message = message, Time = time, Context = context ?? ImmutableDictionary<string, object>.Empty };
}

public enum DiagnosticType
{
    Stability,
    Convergence,
    EnergyDrift,
    ConstraintViolation,
    Divergence,
    NumericalWarning,
    StepSizeWarning,
    EventMissed,
    Error,
    Overflow,
    Underflow,
    NaNDetected
}

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public sealed class DiagnosticCollector
{
    private readonly object _lock = new();
    private readonly List<SimulationDiagnostic> _diagnostics = new();

    public IReadOnlyList<SimulationDiagnostic> Diagnostics { get { lock (_lock) return _diagnostics.ToImmutableArray(); } }

    public void Add(SimulationDiagnostic diagnostic)
    {
        lock (_lock) _diagnostics.Add(diagnostic);
    }

    public void AddWarning(string message, double time, ImmutableDictionary<string, object>? context = null)
    {
        Add(SimulationDiagnostic.Warning(message, time, context));
    }

    public void AddError(string message, double time, ImmutableDictionary<string, object>? context = null)
    {
        Add(SimulationDiagnostic.Error(message, time, context));
    }

    public void Clear()
    {
        lock (_lock) _diagnostics.Clear();
    }

    public int WarningCount { get { lock (_lock) return _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning); } }
    public int ErrorCount { get { lock (_lock) return _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error); } }
}

public sealed class StabilityAnalyzer
{
    public static StabilityReport Analyze(ImmutableArray<double> eigenvalues, double timeStep)
    {
        bool stable = true;
        var warnings = ImmutableArray.CreateBuilder<string>();

        foreach (var ev in eigenvalues)
        {
            if (ev > 0) stable = false;
            if (ev * timeStep > 0)
                warnings.Add($"Unstable mode detected: eigenvalue {ev} with dt={timeStep}");
        }

        return new StabilityReport(stable, warnings.ToImmutable());
    }

    public static EnergyDriftReport CheckEnergyDrift(ImmutableArray<double> energyHistory, double tolerance = 1e-6)
    {
        if (energyHistory.Length < 2) return new EnergyDriftReport(0, 0, true);

        double initial = energyHistory[0];
        double maxDrift = 0;
        foreach (var e in energyHistory)
        {
            double drift = System.Math.Abs((e - initial) / initial);
            if (drift > maxDrift) maxDrift = drift;
        }

        return new EnergyDriftReport(maxDrift, energyHistory[^1] - energyHistory[0], maxDrift < tolerance);
    }
}

public sealed record StabilityReport
{
    public bool IsStable { get; init; }
    public ImmutableArray<string> Warnings { get; init; }

    public StabilityReport(bool stable, ImmutableArray<string> warnings)
    {
        IsStable = stable;
        Warnings = warnings;
    }
}

public sealed record EnergyDriftReport
{
    public double MaxRelativeDrift { get; init; }
    public double TotalDrift { get; init; }
    public bool WithinTolerance { get; init; }

    public EnergyDriftReport(double maxDrift, double totalDrift, bool within)
    {
        MaxRelativeDrift = maxDrift;
        TotalDrift = totalDrift;
        WithinTolerance = within;
    }
}

public sealed class ConvergenceAnalyzer
{
    public static ConvergenceReport Analyze(ImmutableArray<double> errors, ImmutableArray<double> stepSizes)
    {
        if (errors.Length != stepSizes.Length || errors.Length < 2)
            return new ConvergenceReport(0, 0, false, ImmutableArray<string>.Empty);

        double order = 0;

        for (int i = 1; i < errors.Length; i++)
        {
            if (errors[i] > 0 && errors[i - 1] > 0 && stepSizes[i] > 0 && stepSizes[i - 1] > 0)
            {
                double localOrder = System.Math.Log(errors[i] / errors[i - 1]) / System.Math.Log(stepSizes[i] / stepSizes[i - 1]);
                order += localOrder;
            }
        }
        order /= System.Math.Max(1, errors.Length - 1);

        return new ConvergenceReport(order, 0, order > 0.5, ImmutableArray<string>.Empty);
    }
}

public sealed record ConvergenceReport
{
    public double EstimatedOrder { get; init; }
    public double AsymptoticConstant { get; init; }
    public bool IsConvergent { get; init; }
    public ImmutableArray<string> Warnings { get; init; }

    public ConvergenceReport(double order, double constant, bool convergent, ImmutableArray<string> warnings)
    {
        EstimatedOrder = order;
        AsymptoticConstant = constant;
        IsConvergent = convergent;
        Warnings = warnings;
    }
}

public sealed class ConstraintMonitor
{
    public static ConstraintReport CheckConstraints(SimulationState state, ImmutableArray<Physics.Constraint> constraints, double tolerance = 1e-6)
    {
        var violations = ImmutableList.CreateBuilder<ConstraintViolation>();
        var violationsArray = violations.ToImmutable();
        var violationsImmutableArray = ImmutableArray.CreateRange(violationsArray);
        return new ConstraintReport { Violations = violationsImmutableArray, AllSatisfied = true };
    }
}

public sealed record ConstraintViolation
{
    public string ConstraintId { get; init; } = string.Empty;
    public string ConstraintType { get; init; } = string.Empty;
    public double Violation { get; init; }
    public double Tolerance { get; init; }
}

public sealed record ConstraintReport
{
    public ImmutableArray<ConstraintViolation> Violations { get; init; }
    public bool AllSatisfied { get; init; }
}

public sealed class DivergenceDetector
{
    public static bool CheckDivergence(ImmutableArray<MVVector> trajectory, double threshold = 1e6)
    {
        if (trajectory.Length < 2) return false;

        double prevNorm = trajectory[0].Norm();
        for (int i = 1; i < trajectory.Length; i++)
        {
            double norm = trajectory[i].Norm();
            if (norm > threshold && norm > 10 * prevNorm)
                return true;
            prevNorm = norm;
        }
        return false;
    }

    public static DivergenceReport Analyze(ImmutableArray<MVVector> trajectory)
    {
        bool diverging = CheckDivergence(trajectory);
        return new DivergenceReport(diverging, trajectory.Length > 0 ? trajectory[^1].Norm() : 0);
    }
}

public sealed record DivergenceReport
{
    public bool IsDiverging { get; init; }
    public double FinalNorm { get; init; }

    public DivergenceReport(bool diverging, double finalNorm)
    {
        IsDiverging = diverging;
        FinalNorm = finalNorm;
    }
}
