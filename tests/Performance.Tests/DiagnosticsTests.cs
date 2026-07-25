using MathVerse.Math.Performance.Diagnostics;

namespace MathVerse.Performance.Tests;

public sealed class DiagnosticsTests
{
    [Fact]
    public void DiagnosticReporter_Report_Null_Throws()
    {
        var reporter = new DiagnosticReporter();
        Action act = () => reporter.Report(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DiagnosticReporter_ReportEvent_Null_Throws()
    {
        var reporter = new DiagnosticReporter();
        Action act = () => reporter.ReportEvent(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DiagnosticReporter_Report_StoresDiagnostic()
    {
        var reporter = new DiagnosticReporter();
        var diag = new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "slow", DateTime.UtcNow, null);
        reporter.Report(diag);
        reporter.GetDiagnostics().Should().ContainSingle().Which.Should().BeSameAs(diag);
    }

    [Fact]
    public void DiagnosticReporter_ReportEvent_StoresEvent()
    {
        var reporter = new DiagnosticReporter();
        var evt = new PerformanceEvent("op", 100, 0, true, null);
        reporter.ReportEvent(evt);
        reporter.GetEvents().Should().ContainSingle().Which.Should().BeSameAs(evt);
    }

    [Fact]
    public void DiagnosticReporter_MinimumSeverity_FiltersDiagnostics()
    {
        var reporter = new DiagnosticReporter();
        reporter.MinimumSeverity = PerformanceWarning.CacheMiss;
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "low", DateTime.UtcNow, null));
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "matched", DateTime.UtcNow, null));
        reporter.GetDiagnostics().Should().HaveCount(1);
        reporter.GetDiagnostics()[0].Message.Should().Be("matched");
    }

    [Fact]
    public void DiagnosticReporter_MinimumSeverity_NoneAcceptsAll()
    {
        var reporter = new DiagnosticReporter();
        reporter.MinimumSeverity = PerformanceWarning.None;
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "a", DateTime.UtcNow, null));
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "b", DateTime.UtcNow, null));
        reporter.GetDiagnostics().Should().HaveCount(2);
    }

    [Fact]
    public void DiagnosticReporter_Clear_RemovesAll()
    {
        var reporter = new DiagnosticReporter();
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "msg", DateTime.UtcNow, null));
        reporter.ReportEvent(new PerformanceEvent("op", 100, 0, true, null));
        reporter.Clear();
        reporter.GetDiagnostics().Should().BeEmpty();
        reporter.GetEvents().Should().BeEmpty();
    }

    [Fact]
    public void DiagnosticReporter_GetDiagnostics_ReturnsSnapshot()
    {
        var reporter = new DiagnosticReporter();
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "msg", DateTime.UtcNow, null));
        var snapshot1 = reporter.GetDiagnostics();
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg2", DateTime.UtcNow, null));
        var snapshot2 = reporter.GetDiagnostics();
        snapshot1.Should().HaveCount(1);
        snapshot2.Should().HaveCount(2);
    }

    [Fact]
    public void DiagnosticReporter_Summary_ReturnsReport()
    {
        var reporter = new DiagnosticReporter();
        reporter.ReportEvent(new PerformanceEvent("op1", 1000, 512, true, null));
        var report = reporter.Summary();
        report.Should().NotBeNull();
        report.Snapshot.TotalOperations.Should().Be(1);
    }

    [Fact]
    public void DiagnosticReporter_ConcurrentReport_ThreadSafe()
    {
        var reporter = new DiagnosticReporter();
        Parallel.For(0, 500, i =>
        {
            reporter.Report(new PerformanceDiagnostic(
                PerformanceWarning.SlowEvaluation, $"msg-{i}", DateTime.UtcNow, null));
        });
        reporter.GetDiagnostics().Should().HaveCount(500);
    }

    [Fact]
    public void PerformanceLogger_Constructor_NullReporter_Throws()
    {
        Action act = () => new PerformanceLogger(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceLogger_Log_Action_RecordsEvent()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.Log("test-op", () => { });
        reporter.GetEvents().Should().ContainSingle();
        reporter.GetEvents()[0].Operation.Should().Be("test-op");
        reporter.GetEvents()[0].Success.Should().BeTrue();
    }

    [Fact]
    public void PerformanceLogger_Log_Func_ReturnsResult()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        var result = logger.Log("calc", () => 42);
        result.Should().Be(42);
        reporter.GetEvents().Should().ContainSingle();
        reporter.GetEvents()[0].Operation.Should().Be("calc");
    }

    [Fact]
    public void PerformanceLogger_Log_Action_Exception_RecordsFailedEvent()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Action act = () => logger.Log("fail-op", () => throw new InvalidOperationException("boom"));
        act.Should().Throw<InvalidOperationException>();
        reporter.GetEvents().Should().ContainSingle();
        reporter.GetEvents()[0].Success.Should().BeFalse();
    }

    [Fact]
    public void PerformanceLogger_Log_Func_Exception_RecordsFailedEvent()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Func<int> func = () => throw new InvalidOperationException("boom");
        Action act = () => logger.Log("fail-func", func);
        act.Should().Throw<InvalidOperationException>();
        reporter.GetEvents().Should().ContainSingle();
        reporter.GetEvents()[0].Success.Should().BeFalse();
    }

    [Fact]
    public void PerformanceLogger_Log_NullOperation_Throws()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Action act = () => logger.Log(null!, () => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PerformanceLogger_Log_EmptyOperation_Throws()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Action act = () => logger.Log("  ", () => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PerformanceLogger_Log_NullAction_Throws()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Action act = () => logger.Log("op", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceLogger_AverageOperationMs_NoEvents_ReturnsZero()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.AverageOperationMs("nonexistent").Should().Be(0.0);
    }

    [Fact]
    public void PerformanceLogger_AverageOperationMs_WithEvents()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.Log("op", () => Thread.Sleep(10));
        logger.Log("op", () => Thread.Sleep(10));
        var avg = logger.AverageOperationMs("op");
        avg.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PerformanceLogger_Clear_RemovesEvents()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.Log("op", () => { });
        logger.GetEvents().Should().NotBeEmpty();
        logger.Clear();
        logger.GetEvents().Should().BeEmpty();
    }

    [Fact]
    public void PerformanceLogger_GetEvents_ReturnsEvents()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.Log("a", () => { });
        logger.Log("b", () => { });
        logger.GetEvents().Should().HaveCount(2);
    }

    [Fact]
    public void PerformanceDiagnostic_Create_SetsUtcNow()
    {
        var before = DateTime.UtcNow;
        var diag = PerformanceDiagnostic.Create(PerformanceWarning.CacheMiss, "miss", "Cat");
        var after = DateTime.UtcNow;
        diag.Warning.Should().Be(PerformanceWarning.CacheMiss);
        diag.Message.Should().Be("miss");
        diag.Category.Should().Be("Cat");
        diag.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void PerformanceDiagnostic_Create_NullCategory()
    {
        var diag = PerformanceDiagnostic.Create(PerformanceWarning.SlowEvaluation, "msg");
        diag.Category.Should().BeNull();
    }

    [Fact]
    public void PerformanceDiagnostic_RecordEquality()
    {
        var ts = DateTime.UtcNow;
        var a = new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg", ts, "cat");
        var b = new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg", ts, "cat");
        a.Should().Be(b);
    }

    [Fact]
    public void PerformanceEvent_DurationMs_Computed()
    {
        var ticks = Stopwatch.Frequency;
        var evt = new PerformanceEvent("op", ticks, 0, true, null);
        evt.DurationMs.Should().BeApproximately(1000.0, 10.0);
    }

    [Fact]
    public void PerformanceEvent_ToString_ContainsOperation()
    {
        var evt = new PerformanceEvent("myOp", 100, 256, true, null);
        evt.ToString().Should().Contain("myOp");
    }

    [Fact]
    public void PerformanceEvent_Properties()
    {
        var evt = new PerformanceEvent("test", 500, 1024, false, "detail");
        evt.Operation.Should().Be("test");
        evt.DurationTicks.Should().Be(500);
        evt.AllocatedBytes.Should().Be(1024);
        evt.Success.Should().BeFalse();
        evt.Details.Should().Be("detail");
    }

    [Fact]
    public void OptimizationDiagnostic_Properties()
    {
        var diag = new OptimizationDiagnostic("PassA", 5, 3, TimeSpan.FromMilliseconds(2.5), true);
        diag.PassName.Should().Be("PassA");
        diag.NodesRemoved.Should().Be(5);
        diag.NodesSimplified.Should().Be(3);
        diag.Duration.Should().Be(TimeSpan.FromMilliseconds(2.5));
        diag.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void OptimizationDiagnostic_ToString_ContainsPassName()
    {
        var diag = new OptimizationDiagnostic("MyPass", 0, 0, TimeSpan.Zero, false);
        diag.ToString().Should().Contain("MyPass");
    }

    [Fact]
    public void PerformanceWarning_FlagValues()
    {
        ((int)PerformanceWarning.None).Should().Be(0);
        ((int)PerformanceWarning.SlowEvaluation).Should().Be(1);
        ((int)PerformanceWarning.CacheMiss).Should().Be(2);
        ((int)PerformanceWarning.LargeAllocation).Should().Be(4);
        ((int)PerformanceWarning.ExcessiveRecursion).Should().Be(8);
        ((int)PerformanceWarning.DeepExpressionTree).Should().Be(16);
        ((int)PerformanceWarning.DuplicateExpressions).Should().Be(32);
        ((int)PerformanceWarning.MemoryPressure).Should().Be(64);
        ((int)PerformanceWarning.ThreadContention).Should().Be(128);
    }

    [Fact]
    public void DiagnosticReporter_MultipleEvents_ReportsAll()
    {
        var reporter = new DiagnosticReporter();
        for (var i = 0; i < 10; i++)
            reporter.ReportEvent(new PerformanceEvent($"op-{i}", i * 100, i * 10, true, null));
        reporter.GetEvents().Should().HaveCount(10);
    }

    [Fact]
    public void PerformanceLogger_Log_MeasuresAllocations()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        logger.Log("alloc-test", () => { _ = new byte[1024]; });
        reporter.GetEvents()[0].AllocatedBytes.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void DiagnosticReporter_MinimumSeverity_CombinedFlags()
    {
        var reporter = new DiagnosticReporter();
        reporter.MinimumSeverity = PerformanceWarning.SlowEvaluation | PerformanceWarning.CacheMiss;
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "a", DateTime.UtcNow, null));
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "b", DateTime.UtcNow, null));
        reporter.Report(new PerformanceDiagnostic(PerformanceWarning.LargeAllocation, "c", DateTime.UtcNow, null));
        reporter.GetDiagnostics().Should().HaveCount(2);
    }

    [Fact]
    public void DiagnosticReporter_GetEvents_ReturnsSnapshot()
    {
        var reporter = new DiagnosticReporter();
        reporter.ReportEvent(new PerformanceEvent("op1", 100, 0, true, null));
        var snap1 = reporter.GetEvents();
        reporter.ReportEvent(new PerformanceEvent("op2", 200, 0, true, null));
        var snap2 = reporter.GetEvents();
        snap1.Should().HaveCount(1);
        snap2.Should().HaveCount(2);
    }

    [Fact]
    public void PerformanceLogger_Log_Func_NullOperation_Throws()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Func<int> func = () => 42;
        Action act = () => logger.Log(null!, func);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PerformanceLogger_Log_Func_NullFunc_Throws()
    {
        var reporter = new DiagnosticReporter();
        var logger = new PerformanceLogger(reporter);
        Action act = () => logger.Log<int>("op", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceDiagnostic_RecordEquality_DifferentWarning()
    {
        var ts = DateTime.UtcNow;
        var a = new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg", ts, null);
        var b = new PerformanceDiagnostic(PerformanceWarning.SlowEvaluation, "msg", ts, null);
        a.Should().NotBe(b);
    }

    [Fact]
    public void PerformanceDiagnostic_RecordEquality_DifferentMessage()
    {
        var ts = DateTime.UtcNow;
        var a = new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg1", ts, null);
        var b = new PerformanceDiagnostic(PerformanceWarning.CacheMiss, "msg2", ts, null);
        a.Should().NotBe(b);
    }

    [Fact]
    public void OptimizationDiagnostic_RecordEquality()
    {
        var a = new OptimizationDiagnostic("P", 1, 2, TimeSpan.FromMilliseconds(1), true);
        var b = new OptimizationDiagnostic("P", 1, 2, TimeSpan.FromMilliseconds(1), true);
        a.Should().Be(b);
    }

    [Fact]
    public void OptimizationDiagnostic_RecordInequality_DifferentDuration()
    {
        var a = new OptimizationDiagnostic("P", 1, 2, TimeSpan.FromMilliseconds(1), true);
        var b = new OptimizationDiagnostic("P", 1, 2, TimeSpan.FromMilliseconds(2), true);
        a.Should().NotBe(b);
    }

    [Fact]
    public void PerformanceEvent_RecordEquality()
    {
        var a = new PerformanceEvent("op", 100, 200, true, "d");
        var b = new PerformanceEvent("op", 100, 200, true, "d");
        a.Should().Be(b);
    }

    [Fact]
    public void PerformanceEvent_RecordInequality_DifferentOp()
    {
        var a = new PerformanceEvent("op1", 100, 200, true, null);
        var b = new PerformanceEvent("op2", 100, 200, true, null);
        a.Should().NotBe(b);
    }
}
