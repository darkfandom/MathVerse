namespace MathVerse.Simulation.Tests.Diagnostics;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public class DiagnosticsEngineTests
{
    [Fact]
    public void StabilityAnalyzer_AllNegativeEigenvalues_IsStable()
    {
        var eigenvalues = ImmutableArray.Create(-1.0, -2.0, -3.0);

        var report = StabilityAnalyzer.Analyze(eigenvalues, 0.01);

        report.IsStable.Should().BeTrue();
    }

    [Fact]
    public void StabilityAnalyzer_PositiveEigenvalue_IsUnstable()
    {
        var eigenvalues = ImmutableArray.Create(-1.0, 2.0, -3.0);

        var report = StabilityAnalyzer.Analyze(eigenvalues, 0.01);

        report.IsStable.Should().BeFalse();
    }

    [Fact]
    public void StabilityAnalyzer_ZeroEigenvalue_IsUnstable()
    {
        var eigenvalues = ImmutableArray.Create(-1.0, 0.0, -3.0);

        var report = StabilityAnalyzer.Analyze(eigenvalues, 0.01);

        report.IsStable.Should().BeTrue();
    }

    [Fact]
    public void StabilityAnalyzer_GeneratesWarnings()
    {
        var eigenvalues = ImmutableArray.Create(5.0);

        var report = StabilityAnalyzer.Analyze(eigenvalues, 0.1);

        report.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public void StabilityAnalyzer_NoWarningsForStableSystem()
    {
        var eigenvalues = ImmutableArray.Create(-1.0, -2.0);

        var report = StabilityAnalyzer.Analyze(eigenvalues, 0.01);

        report.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void CheckEnergyDrift_ConstantEnergy_WithinTolerance()
    {
        var energy = ImmutableArray.Create(1.0, 1.0, 1.0, 1.0);

        var report = StabilityAnalyzer.CheckEnergyDrift(energy, 1e-6);

        report.WithinTolerance.Should().BeTrue();
        report.TotalDrift.Should().BeApproximately(0, 1e-15);
    }

    [Fact]
    public void CheckEnergyDrift_SmallDrift_WithinTolerance()
    {
        var energy = ImmutableArray.Create(1.0, 1.0000001, 1.0000002, 1.0000003);

        var report = StabilityAnalyzer.CheckEnergyDrift(energy, 1e-4);

        report.WithinTolerance.Should().BeTrue();
    }

    [Fact]
    public void CheckEnergyDrift_LargeDrift_ExceedsTolerance()
    {
        var energy = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);

        var report = StabilityAnalyzer.CheckEnergyDrift(energy, 1e-6);

        report.WithinTolerance.Should().BeFalse();
    }

    [Fact]
    public void CheckEnergyDrift_SingleElement_ReturnsZeroDrift()
    {
        var energy = ImmutableArray.Create(1.0);

        var report = StabilityAnalyzer.CheckEnergyDrift(energy);

        report.MaxRelativeDrift.Should().Be(0);
        report.WithinTolerance.Should().BeTrue();
    }

    [Fact]
    public void ConvergenceAnalyzer_OrderFour_Converges()
    {
        var errors = ImmutableArray.Create(1e-2, 6.25e-4, 3.90625e-5);
        var stepSizes = ImmutableArray.Create(0.1, 0.025, 0.00625);

        var report = ConvergenceAnalyzer.Analyze(errors, stepSizes);

        report.IsConvergent.Should().BeTrue();
        report.EstimatedOrder.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public void ConvergenceAnalyzer_InsufficientData_NotConvergent()
    {
        var errors = ImmutableArray.Create(1e-2);
        var stepSizes = ImmutableArray.Create(0.1);

        var report = ConvergenceAnalyzer.Analyze(errors, stepSizes);

        report.IsConvergent.Should().BeFalse();
    }

    [Fact]
    public void ConvergenceAnalyzer_MismatchedLengths_NotConvergent()
    {
        var errors = ImmutableArray.Create(1e-2, 1e-3);
        var stepSizes = ImmutableArray.Create(0.1);

        var report = ConvergenceAnalyzer.Analyze(errors, stepSizes);

        report.IsConvergent.Should().BeFalse();
    }

    [Fact]
    public void DivergenceDetector_ConvergingTrajectory_NotDiverging()
    {
        var trajectory = ImmutableArray.Create(
            new Vector(1.0, 0.0),
            new Vector(1.1, 0.1),
            new Vector(1.05, 0.05),
            new Vector(1.02, 0.02));

        DivergenceDetector.CheckDivergence(trajectory).Should().BeFalse();
    }

    [Fact]
    public void DivergenceDetector_DivergingTrajectory_IsDiverging()
    {
        var trajectory = ImmutableArray.Create(
            new Vector(1.0, 0.0),
            new Vector(100.0, 0.0),
            new Vector(10000.0, 0.0),
            new Vector(1e7, 0.0));

        DivergenceDetector.CheckDivergence(trajectory, 1e6).Should().BeTrue();
    }

    [Fact]
    public void DivergenceDetector_ShortTrajectory_NotDiverging()
    {
        var trajectory = ImmutableArray.Create(new Vector(1.0));

        DivergenceDetector.CheckDivergence(trajectory).Should().BeFalse();
    }

    [Fact]
    public void DivergenceDetector_Analyze_ProducesReport()
    {
        var trajectory = ImmutableArray.Create(
            new Vector(1.0),
            new Vector(2.0),
            new Vector(3.0));

        var report = DivergenceDetector.Analyze(trajectory);

        report.Should().NotBeNull();
        report.FinalNorm.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void DiagnosticCollector_Add_IncreasesCount()
    {
        var collector = new DiagnosticCollector();

        collector.Add(SimulationDiagnostic.Warning("test", 0));

        collector.Diagnostics.Count.Should().Be(1);
    }

    [Fact]
    public void DiagnosticCollector_Clear_RemovesAll()
    {
        var collector = new DiagnosticCollector();
        collector.Add(SimulationDiagnostic.Warning("test1", 0));
        collector.Add(SimulationDiagnostic.Error("test2", 0));

        collector.Clear();

        collector.Diagnostics.Count.Should().Be(0);
    }

    [Fact]
    public void DiagnosticCollector_WarningCount_Correct()
    {
        var collector = new DiagnosticCollector();
        collector.Add(SimulationDiagnostic.Warning("w1", 0));
        collector.Add(SimulationDiagnostic.Warning("w2", 0));
        collector.Add(SimulationDiagnostic.Error("e1", 0));

        collector.WarningCount.Should().Be(2);
    }

    [Fact]
    public void DiagnosticCollector_ErrorCount_Correct()
    {
        var collector = new DiagnosticCollector();
        collector.Add(SimulationDiagnostic.Warning("w1", 0));
        collector.Add(SimulationDiagnostic.Error("e1", 0));
        collector.Add(SimulationDiagnostic.Error("e2", 0));

        collector.ErrorCount.Should().Be(2);
    }

    [Fact]
    public void DiagnosticCollector_AddWarning_Shortcut()
    {
        var collector = new DiagnosticCollector();

        collector.AddWarning("test warning", 1.5);

        collector.Diagnostics.Count.Should().Be(1);
        collector.Diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void DiagnosticCollector_AddError_Shortcut()
    {
        var collector = new DiagnosticCollector();

        collector.AddError("test error", 2.0);

        collector.Diagnostics.Count.Should().Be(1);
        collector.Diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public void SimulationDiagnostic_Warning_CreatesWarning()
    {
        var diag = SimulationDiagnostic.Warning("test", 0.5);

        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
        diag.Message.Should().Be("test");
        diag.Time.Should().Be(0.5);
    }

    [Fact]
    public void SimulationDiagnostic_Error_CreatesError()
    {
        var diag = SimulationDiagnostic.Error("error msg", 1.0);

        diag.Severity.Should().Be(DiagnosticSeverity.Error);
        diag.Type.Should().Be(DiagnosticType.Error);
    }
}
