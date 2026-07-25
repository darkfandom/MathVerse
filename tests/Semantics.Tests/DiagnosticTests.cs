using FluentAssertions;
using MathVerse.Math.Semantics.Diagnostics;

namespace MathVerse.Semantics.Tests;

public class DiagnosticTests
{
    [Fact]
    public void DiagnosticBag_InitiallyEmpty()
    {
        var bag = new SemanticDiagnosticBag();
        bag.Count.Should().Be(0);
        bag.HasErrors.Should().BeFalse();
        bag.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void DiagnosticBag_ReportError()
    {
        var bag = new SemanticDiagnosticBag();
        bag.ReportError(SemanticDiagnosticCode.UndefinedVariable, "x not found");
        bag.HasErrors.Should().BeTrue();
        bag.Count.Should().Be(1);
    }

    [Fact]
    public void DiagnosticBag_ReportWarning()
    {
        var bag = new SemanticDiagnosticBag();
        bag.ReportWarning(SemanticDiagnosticCode.DivisionByZero, "div by 0");
        bag.HasWarnings.Should().BeTrue();
        bag.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void DiagnosticBag_ReportInfo()
    {
        var bag = new SemanticDiagnosticBag();
        bag.ReportInfo(SemanticDiagnosticCode.NotImplemented, "not done");
        bag.Count.Should().Be(1);
        bag.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void DiagnosticBag_GetBySeverity()
    {
        var bag = new SemanticDiagnosticBag();
        bag.ReportError(SemanticDiagnosticCode.UndefinedVariable, "err");
        bag.ReportWarning(SemanticDiagnosticCode.DivisionByZero, "warn");
        bag.GetBySeverity(SemanticSeverity.Error).Should().HaveCount(1);
        bag.GetBySeverity(SemanticSeverity.Warning).Should().HaveCount(1);
        bag.GetBySeverity(SemanticSeverity.Info).Should().HaveCount(0);
    }

    [Fact]
    public void DiagnosticBag_Merge()
    {
        var bag1 = new SemanticDiagnosticBag();
        bag1.ReportError(SemanticDiagnosticCode.InternalError, "e1");
        var bag2 = new SemanticDiagnosticBag();
        bag2.ReportError(SemanticDiagnosticCode.InternalError, "e2");
        bag1.Merge(bag2);
        bag1.Count.Should().Be(2);
    }

    [Fact]
    public void DiagnosticBag_Clear()
    {
        var bag = new SemanticDiagnosticBag();
        bag.ReportError(SemanticDiagnosticCode.InternalError, "err");
        bag.Clear();
        bag.Count.Should().Be(0);
    }

    [Fact]
    public async Task DiagnosticBag_ThreadSafe()
    {
        var bag = new SemanticDiagnosticBag();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() =>
                bag.ReportInfo(SemanticDiagnosticCode.NotImplemented, "test")))
            .ToArray();
        await Task.WhenAll(tasks);
        bag.Count.Should().Be(100);
    }

    [Fact]
    public void Diagnostic_ToString()
    {
        var d = new SemanticDiagnostic(
            SemanticDiagnosticCode.UndefinedVariable, "x not found",
            SemanticSeverity.Error, "line 1");
        d.ToString().Should().Contain("Error");
        d.ToString().Should().Contain("x not found");
        d.ToString().Should().Contain("line 1");
    }

    [Fact]
    public void DiagnosticBag_Report_WithLocation()
    {
        var bag = new SemanticDiagnosticBag();
        bag.Report(SemanticDiagnosticCode.InvalidLiteral, "bad",
            SemanticSeverity.Error, "pos:5");
        bag.All[0].Location.Should().Be("pos:5");
    }
}
