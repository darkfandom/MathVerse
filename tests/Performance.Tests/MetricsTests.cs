using MathVerse.Math.Performance.Metrics;
using MathVerse.Math.Performance.Diagnostics;

namespace MathVerse.Performance.Tests;

public sealed class MetricsTests
{
    [Fact]
    public void PerformanceCounter_Constructor_NullName_Throws()
    {
        Action act = () => new PerformanceCounter(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PerformanceCounter_Constructor_EmptyName_Throws()
    {
        Action act = () => new PerformanceCounter("  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PerformanceCounter_Name_ReturnsName()
    {
        var counter = new PerformanceCounter("requests");
        counter.Name.Should().Be("requests");
    }

    [Fact]
    public void PerformanceCounter_Value_InitiallyZero()
    {
        var counter = new PerformanceCounter("test");
        counter.Value.Should().Be(0);
    }

    [Fact]
    public void PerformanceCounter_Increment_IncreasesValue()
    {
        var counter = new PerformanceCounter("test");
        counter.Increment();
        counter.Value.Should().Be(1);
        counter.Increment();
        counter.Value.Should().Be(2);
    }

    [Fact]
    public void PerformanceCounter_Increment_ByValue()
    {
        var counter = new PerformanceCounter("test");
        counter.Increment(5);
        counter.Value.Should().Be(5);
        counter.Increment(3);
        counter.Value.Should().Be(8);
    }

    [Fact]
    public void PerformanceCounter_Decrement_DecreasesValue()
    {
        var counter = new PerformanceCounter("test");
        counter.Increment(5);
        counter.Decrement();
        counter.Value.Should().Be(4);
        counter.Decrement();
        counter.Value.Should().Be(3);
    }

    [Fact]
    public void PerformanceCounter_Record_AffectsAverage()
    {
        var counter = new PerformanceCounter("test");
        counter.Record(10);
        counter.Average.Should().Be(10.0);
        counter.Record(20);
        counter.Average.Should().Be(15.0);
    }

    [Fact]
    public void PerformanceCounter_Record_NoRecords_AverageZero()
    {
        var counter = new PerformanceCounter("test");
        counter.Average.Should().Be(0.0);
    }

    [Fact]
    public void PerformanceCounter_Reset_ClearsAll()
    {
        var counter = new PerformanceCounter("test");
        counter.Increment(10);
        counter.Record(50);
        counter.Reset();
        counter.Value.Should().Be(0);
        counter.Average.Should().Be(0.0);
    }

    [Fact]
    public void PerformanceCounter_ConcurrentIncrement_ThreadSafe()
    {
        var counter = new PerformanceCounter("test");
        Parallel.For(0, 1000, _ => counter.Increment());
        counter.Value.Should().Be(1000);
    }

    [Fact]
    public void PerformanceCounter_ConcurrentDecrement_ThreadSafe()
    {
        var counter = new PerformanceCounter("test");
        counter.Increment(1000);
        Parallel.For(0, 1000, _ => counter.Decrement());
        counter.Value.Should().Be(0);
    }

    [Fact]
    public void PerformanceSnapshot_Constructor_AllProperties()
    {
        var ts = DateTime.UtcNow;
        var snapshot = new PerformanceSnapshot(
            ts, 100, 50.0, 2048, 3, 1, 0, 0.85, 1500.0);
        snapshot.Timestamp.Should().Be(ts);
        snapshot.TotalOperations.Should().Be(100);
        snapshot.ElapsedMs.Should().Be(50.0);
        snapshot.AllocatedBytes.Should().Be(2048);
        snapshot.Gen0Collections.Should().Be(3);
        snapshot.Gen1Collections.Should().Be(1);
        snapshot.Gen2Collections.Should().Be(0);
        snapshot.CacheHitRatio.Should().Be(0.85);
        snapshot.OperationsPerSecond.Should().Be(1500.0);
    }

    [Fact]
    public void PerformanceSnapshot_RecordEquality()
    {
        var ts = DateTime.UtcNow;
        var a = new PerformanceSnapshot(ts, 10, 1.0, 100, 0, 0, 0, 0.5, 10.0);
        var b = new PerformanceSnapshot(ts, 10, 1.0, 100, 0, 0, 0, 0.5, 10.0);
        a.Should().Be(b);
    }

    [Fact]
    public void PerformanceSnapshot_ToString_ContainsValues()
    {
        var snapshot = new PerformanceSnapshot(
            DateTime.UtcNow, 42, 99.9, 512, 1, 2, 3, 0.75, 2000.0);
        var str = snapshot.ToString();
        str.Should().Contain("Operations=42");
        str.Should().Contain("Allocated=512B");
    }

    [Fact]
    public void PerformanceSnapshot_WithExpression_ModifiesValues()
    {
        var snapshot = new PerformanceSnapshot(
            DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var modified = snapshot with { TotalOperations = 99, CacheHitRatio = 0.5 };
        modified.TotalOperations.Should().Be(99);
        modified.CacheHitRatio.Should().Be(0.5);
    }

    [Fact]
    public void OperationTimer_StartNew_ReturnsRunningTimer()
    {
        var timer = OperationTimer.StartNew("test-op");
        timer.OperationName.Should().Be("test-op");
        timer.IsRunning.Should().BeTrue();
        timer.Stop();
    }

    [Fact]
    public void OperationTimer_Stop_StopsTimer()
    {
        var timer = OperationTimer.StartNew("test");
        timer.Stop();
        timer.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void OperationTimer_Stop_RecordsEvent()
    {
        var before = OperationTimer.GetAllEvents().Count;
        using var timer = OperationTimer.StartNew("timed-op");
        timer.Stop();
        OperationTimer.GetAllEvents().Count.Should().BeGreaterThan(before);
        OperationTimer.GetAllEvents().Should().Contain(e => e.Operation == "timed-op");
    }

    [Fact]
    public void OperationTimer_Dispose_StopsAndRecords()
    {
        var before = OperationTimer.GetAllEvents().Count;
        var timer = OperationTimer.StartNew("disposed-op");
        timer.Dispose();
        OperationTimer.GetAllEvents().Count.Should().BeGreaterThan(before);
    }

    [Fact]
    public void OperationTimer_DoubleStop_NoDoubleRecord()
    {
        var before = OperationTimer.GetAllEvents().Count;
        var timer = OperationTimer.StartNew("single-stop");
        timer.Stop();
        timer.Stop();
        OperationTimer.GetAllEvents().Count.Should().Be(before + 1);
    }

    [Fact]
    public void OperationTimer_Elapsed_AfterStop()
    {
        var timer = OperationTimer.StartNew("elapsed");
        Thread.Sleep(10);
        timer.Stop();
        timer.Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void BenchmarkRecorder_Record_NullName_Throws()
    {
        var recorder = new BenchmarkRecorder();
        Action act = () => recorder.Record(null!, 100, 0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BenchmarkRecorder_Record_ZeroIterations_Throws()
    {
        var recorder = new BenchmarkRecorder();
        Action act = () => recorder.Record("bench", 100, 0, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BenchmarkRecorder_Record_StoresResult()
    {
        var recorder = new BenchmarkRecorder();
        recorder.Record("bench1", Stopwatch.Frequency, 512, 10);
        var results = recorder.GetAll();
        results.Should().ContainSingle();
        results[0].Name.Should().Be("bench1");
        results[0].Iterations.Should().Be(10);
    }

    [Fact]
    public void BenchmarkRecorder_GetBest_ReturnsLowestAverageMs()
    {
        var recorder = new BenchmarkRecorder();
        recorder.Record("bench", Stopwatch.Frequency * 2, 0, 1);
        recorder.Record("bench", Stopwatch.Frequency, 0, 1);
        var best = recorder.GetBest("bench");
        best.Should().NotBeNull();
        best!.AverageMs.Should().BeLessThan(2000);
    }

    [Fact]
    public void BenchmarkRecorder_GetBest_NonExistentBenchmark_ReturnsNull()
    {
        var recorder = new BenchmarkRecorder();
        recorder.GetBest("nonexistent").Should().BeNull();
    }

    [Fact]
    public void BenchmarkRecorder_Clear_RemovesAll()
    {
        var recorder = new BenchmarkRecorder();
        recorder.Record("b1", 100, 0, 1);
        recorder.Record("b2", 200, 0, 1);
        recorder.Clear();
        recorder.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void BenchmarkRecorder_MultipleIterations_AverageComputed()
    {
        var recorder = new BenchmarkRecorder();
        var ticks = Stopwatch.Frequency;
        recorder.Record("bench", ticks, 0, 100);
        var best = recorder.GetBest("bench")!;
        best.AverageMs.Should().BeApproximately(10.0, 1.0);
    }

    [Fact]
    public void BenchmarkRecorder_ConcurrentRecord_ThreadSafe()
    {
        var recorder = new BenchmarkRecorder();
        Parallel.For(0, 100, i =>
        {
            recorder.Record($"bench-{i % 5}", 1000, 0, 1);
        });
        recorder.GetAll().Should().HaveCount(100);
    }

    [Fact]
    public void PerformanceReport_Empty_HasDefaults()
    {
        var report = PerformanceReport.Empty;
        report.Snapshot.Should().NotBeNull();
        report.SlowestOperations.Should().BeEmpty();
        report.Benchmarks.Should().BeEmpty();
        report.OptimizationResults.Should().BeEmpty();
    }

    [Fact]
    public void PerformanceReport_Constructor_AllProperties()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 10, 100, 0, 0, 0, 0, 0.9, 50);
        var events = new List<PerformanceEvent>
        {
            new("op1", 1000, 0, true, null),
            new("op2", 500, 0, true, null)
        };
        var benchmarks = new List<BenchmarkResult>
        {
            new("b1", 1.5, 256, 100)
        };
        var optDiags = new List<OptimizationDiagnostic>
        {
            new("PassA", 3, 1, TimeSpan.FromMilliseconds(1), true)
        };
        var report = new PerformanceReport(snapshot, events, benchmarks, optDiags);
        report.Snapshot.Should().BeSameAs(snapshot);
        report.SlowestOperations.Should().HaveCount(2);
        report.Benchmarks.Should().HaveCount(1);
        report.OptimizationResults.Should().HaveCount(1);
    }

    [Fact]
    public void PerformanceReport_Constructor_NullSnapshot_Throws()
    {
        Action act = () => new PerformanceReport(null!, [], [], []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceReport_ToString_ContainsSnapshotInfo()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 50, 200, 1024, 1, 0, 0, 0.8, 250);
        var report = new PerformanceReport(snapshot, [], [], []);
        var str = report.ToString();
        str.Should().Contain("50");
        str.Should().Contain("200");
    }

    [Fact]
    public void PerformanceReport_ToString_WithOperations()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 1, 10, 0, 0, 0, 0, 0, 0);
        var events = new List<PerformanceEvent> { new("slow-op", 5000, 0, true, null) };
        var report = new PerformanceReport(snapshot, events, [], []);
        var str = report.ToString();
        str.Should().Contain("slow-op");
    }

    [Fact]
    public void BenchmarkResult_Properties()
    {
        var result = new BenchmarkResult("test-bench", 2.5, 1024, 50);
        result.Name.Should().Be("test-bench");
        result.AverageMs.Should().Be(2.5);
        result.AllocatedBytes.Should().Be(1024);
        result.Iterations.Should().Be(50);
    }

    [Fact]
    public void BenchmarkResult_RecordEquality()
    {
        var a = new BenchmarkResult("b", 1.0, 100, 10);
        var b = new BenchmarkResult("b", 1.0, 100, 10);
        a.Should().Be(b);
    }

    [Fact]
    public void PerformanceCounter_Increment_LargeValue()
    {
        var counter = new PerformanceCounter("big");
        counter.Increment(long.MaxValue - 10);
        counter.Value.Should().Be(long.MaxValue - 10);
    }

    [Fact]
    public void PerformanceCounter_Record_MultipleValues_Average()
    {
        var counter = new PerformanceCounter("avg");
        counter.Record(100);
        counter.Record(200);
        counter.Record(300);
        counter.Average.Should().Be(200.0);
    }

    [Fact]
    public void PerformanceCounter_Record_NegativeValues()
    {
        var counter = new PerformanceCounter("neg");
        counter.Record(-10);
        counter.Record(-20);
        counter.Average.Should().Be(-15.0);
    }

    [Fact]
    public void OperationTimer_Elapsed_AfterDispose()
    {
        using var timer = OperationTimer.StartNew("dispose-test");
        Thread.Sleep(10);
        var elapsed = timer.Elapsed;
        elapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void OperationTimer_IsRunning_BeforeStop()
    {
        var timer = OperationTimer.StartNew("running");
        timer.IsRunning.Should().BeTrue();
        timer.Stop();
        timer.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void BenchmarkRecorder_GetBest_MultipleBenchmarks()
    {
        var recorder = new BenchmarkRecorder();
        recorder.Record("fast", Stopwatch.Frequency, 0, 1);
        recorder.Record("slow", Stopwatch.Frequency * 10, 0, 1);
        recorder.GetBest("fast")!.AverageMs.Should().BeLessThan(
            recorder.GetBest("slow")!.AverageMs);
    }

    [Fact]
    public void BenchmarkRecorder_GetAll_MultipleBenchmarks()
    {
        var recorder = new BenchmarkRecorder();
        recorder.Record("a", 100, 0, 1);
        recorder.Record("b", 200, 0, 1);
        recorder.Record("c", 300, 0, 1);
        recorder.GetAll().Should().HaveCount(3);
    }

    [Fact]
    public void PerformanceSnapshot_DefaultValues()
    {
        var snapshot = new PerformanceSnapshot(
            DateTime.MinValue, 0, 0, 0, 0, 0, 0, 0, 0);
        snapshot.Timestamp.Should().Be(DateTime.MinValue);
        snapshot.TotalOperations.Should().Be(0);
        snapshot.AllocatedBytes.Should().Be(0);
    }

    [Fact]
    public void PerformanceReport_NullSlowestOperations_Defaults()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var report = new PerformanceReport(snapshot, null!, [], []);
        report.SlowestOperations.Should().BeEmpty();
    }

    [Fact]
    public void PerformanceReport_NullBenchmarks_Defaults()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var report = new PerformanceReport(snapshot, [], null!, []);
        report.Benchmarks.Should().BeEmpty();
    }

    [Fact]
    public void PerformanceReport_NullOptimizationResults_Defaults()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var report = new PerformanceReport(snapshot, [], [], null!);
        report.OptimizationResults.Should().BeEmpty();
    }

    [Fact]
    public void PerformanceReport_ToString_WithBenchmarks()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var benchmarks = new List<BenchmarkResult> { new("my-bench", 1.5, 256, 50) };
        var report = new PerformanceReport(snapshot, [], benchmarks, []);
        var str = report.ToString();
        str.Should().Contain("my-bench");
    }

    [Fact]
    public void PerformanceReport_ToString_WithOptimizationResults()
    {
        var snapshot = new PerformanceSnapshot(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0);
        var opts = new List<OptimizationDiagnostic> { new("FoldPass", 5, 2, TimeSpan.FromMilliseconds(1), true) };
        var report = new PerformanceReport(snapshot, [], [], opts);
        var str = report.ToString();
        str.Should().Contain("FoldPass");
    }
}
