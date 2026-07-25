using BenchmarkDotNet.Attributes;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Diagnostics;
using MathVerse.Math.Simulation.Events;
using MathVerse.Math.Simulation.Physics;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class DiagnosticsBenchmarks
{
    private DiagnosticCollector _collector = null!;
    private double[] _energyHistory = null!;
    private double[] _convergenceErrors = null!;
    private double[] _stepSizes = null!;
    private MVVector[] _trajectory = null!;
    private SimulationState _state = null!;

    private ImmutableArray<double> _eigenvalues10;
    private ImmutableArray<double> _eigenvalues100;
    private ImmutableArray<double> _energyHistoryImmutable;
    private ImmutableArray<double> _convergenceErrorsImmutable;
    private ImmutableArray<double> _stepSizesImmutable;
    private ImmutableArray<MVVector> _trajectoryImmutable;
    private ImmutableArray<Constraint> _constraints;

    [GlobalSetup]
    public void Setup()
    {
        _collector = new DiagnosticCollector();
        _energyHistory = new double[100];
        _convergenceErrors = new double[100];
        _stepSizes = new double[100];
        _trajectory = new MVVector[100];

        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            _energyHistory[i] = 100.0 + rng.NextDouble() * 0.001;
            _convergenceErrors[i] = System.Math.Pow(0.5, i);
            _stepSizes[i] = 0.1 / (i + 1);
            _trajectory[i] = new MVVector(new double[] { rng.NextDouble(), rng.NextDouble(), rng.NextDouble() });
        }

        _eigenvalues10 = Enumerable.Range(0, 10).Select(i => -1.0 + i * 0.05).ToImmutableArray();
        _eigenvalues100 = Enumerable.Range(0, 100).Select(i => -1.0 + i * 0.02).ToImmutableArray();
        _energyHistoryImmutable = _energyHistory.ToImmutableArray();
        _convergenceErrorsImmutable = _convergenceErrors.ToImmutableArray();
        _stepSizesImmutable = _stepSizes.ToImmutableArray();
        _trajectoryImmutable = _trajectory.ToImmutableArray();
        _constraints = ImmutableArray.Create(
            Constraint.Distance("c1", "p1", "p2", 1.0),
            Constraint.Distance("c2", "p2", "p3", 2.0)
        );

        _state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty
            .Add("x", 1.0).Add("y", 2.0).Add("z", 3.0));
    }

    [Benchmark]
    public void DiagnosticCollector_AddWarning()
    {
        _collector.AddWarning("Test warning", 1.0);
    }

    [Benchmark]
    public void DiagnosticCollector_AddError()
    {
        _collector.AddError("Test error", 1.0);
    }

    [Benchmark]
    public void DiagnosticCollector_Add_100Diagnostics()
    {
        _collector.Clear();
        for (int i = 0; i < 100; i++)
        {
            if (i % 2 == 0)
                _collector.AddWarning($"Warning {i}", i * 0.1);
            else
                _collector.AddError($"Error {i}", i * 0.1);
        }
    }

    [Benchmark]
    public void DiagnosticCollector_Clear()
    {
        _collector.AddWarning("temp", 0.0);
        _collector.Clear();
    }

    [Benchmark]
    public int DiagnosticCollector_WarningCount()
    {
        return _collector.WarningCount;
    }

    [Benchmark]
    public int DiagnosticCollector_ErrorCount()
    {
        return _collector.ErrorCount;
    }

    [Benchmark]
    public IReadOnlyList<SimulationDiagnostic> DiagnosticCollector_DiagnosticsAccess()
    {
        return _collector.Diagnostics;
    }

    [Benchmark]
    public SimulationDiagnostic SimulationDiagnostic_WarningFactory()
    {
        return SimulationDiagnostic.Warning("Test warning", 1.5);
    }

    [Benchmark]
    public SimulationDiagnostic SimulationDiagnostic_ErrorFactory()
    {
        return SimulationDiagnostic.Error("Test error", 2.5);
    }

    [Benchmark]
    public EnergyDriftReport StabilityAnalyzer_CheckEnergyDrift_NoDrift()
    {
        var stableHistory = Enumerable.Repeat(1.0, 100).ToImmutableArray();
        return StabilityAnalyzer.CheckEnergyDrift(stableHistory, 1e-6);
    }

    [Benchmark]
    public EnergyDriftReport StabilityAnalyzer_CheckEnergyDrift_WithDrift()
    {
        return StabilityAnalyzer.CheckEnergyDrift(_energyHistoryImmutable, 1e-10);
    }

    [Benchmark]
    public EnergyDriftReport StabilityAnalyzer_CheckEnergyDrift_LargeHistory()
    {
        var largeHistory = Enumerable.Range(0, 10000).Select(i => 100.0 + i * 1e-7).ToImmutableArray();
        return StabilityAnalyzer.CheckEnergyDrift(largeHistory, 1e-6);
    }

    [Benchmark]
    public ConvergenceReport ConvergenceAnalyzer_Analyze_Convergent()
    {
        return ConvergenceAnalyzer.Analyze(_convergenceErrorsImmutable, _stepSizesImmutable);
    }

    [Benchmark]
    public ConvergenceReport ConvergenceAnalyzer_Analyze_Divergent()
    {
        var divergentErrors = Enumerable.Range(0, 100).Select(i => System.Math.Pow(2.0, i)).ToImmutableArray();
        var divergentSteps = Enumerable.Range(0, 100).Select(i => 0.1 / (i + 1)).ToImmutableArray();
        return ConvergenceAnalyzer.Analyze(divergentErrors, divergentSteps);
    }

    [Benchmark]
    public ConvergenceReport ConvergenceAnalyzer_Analyze_Empty()
    {
        return ConvergenceAnalyzer.Analyze(
            ImmutableArray<double>.Empty,
            ImmutableArray<double>.Empty);
    }

    [Benchmark]
    public bool DivergenceDetector_CheckDivergence_NoDivergence()
    {
        var stableTrajectory = Enumerable.Range(0, 50)
            .Select(i => new MVVector(new double[] { System.Math.Sin(i * 0.1), System.Math.Cos(i * 0.1), 0.0 }))
            .ToImmutableArray();
        return DivergenceDetector.CheckDivergence(stableTrajectory, 1e6);
    }

    [Benchmark]
    public bool DivergenceDetector_CheckDivergence_WithDivergence()
    {
        var divergent = Enumerable.Range(0, 20)
            .Select(i => new MVVector(new double[] { System.Math.Pow(10, i), 0.0, 0.0 }))
            .ToImmutableArray();
        return DivergenceDetector.CheckDivergence(divergent, 1e3);
    }

    [Benchmark]
    public DivergenceReport DivergenceDetector_Analyze()
    {
        return DivergenceDetector.Analyze(_trajectoryImmutable);
    }

    [Benchmark]
    public ConstraintReport ConstraintMonitor_CheckConstraints_NoViolations()
    {
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        return ConstraintMonitor.CheckConstraints(state, ImmutableArray<Constraint>.Empty, 1e-6);
    }

    [Benchmark]
    public ConstraintReport ConstraintMonitor_CheckConstraints_WithViolations()
    {
        return ConstraintMonitor.CheckConstraints(_state, _constraints, 1e-6);
    }

    [Benchmark]
    public StabilityReport StabilityAnalyzer_Analyze_10Eigenvalues()
    {
        return StabilityAnalyzer.Analyze(_eigenvalues10, 0.01);
    }

    [Benchmark]
    public StabilityReport StabilityAnalyzer_Analyze_100Eigenvalues()
    {
        return StabilityAnalyzer.Analyze(_eigenvalues100, 0.01);
    }

    [Benchmark]
    public bool DivergenceDetector_CheckDivergence_Threshold()
    {
        var barelyDivergent = Enumerable.Range(0, 30)
            .Select(i => new MVVector(new double[] { 1.0 + i * 0.5, 0.0, 0.0 }))
            .ToImmutableArray();
        return DivergenceDetector.CheckDivergence(barelyDivergent, 100.0);
    }

    [Benchmark]
    public DiagnosticType DiagnosticType_EnumValues()
    {
        var values = Enum.GetValues<DiagnosticType>();
        return values[values.Length - 1];
    }

    [Benchmark]
    public DiagnosticSeverity DiagnosticSeverity_EnumValues()
    {
        var values = Enum.GetValues<DiagnosticSeverity>();
        return values[values.Length - 1];
    }
}
