using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Buffers;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MemoryBenchmarks
{
    private MemoryTracker _memoryTracker = null!;
    private AllocationProfiler _allocationProfiler = null!;
    private BufferManager _bufferManager = null!;
    private MemoryPressureMonitor _pressureMonitor = null!;
    private PerformanceCounter _counter = null!;
    private OperationTimer _timer = null!;
    private Expression[] _expressions = null!;

    [Params(100, 1000)]
    public int IterationCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _memoryTracker = new MemoryTracker();
        _allocationProfiler = new AllocationProfiler();
        _bufferManager = new BufferManager();
        _pressureMonitor = new MemoryPressureMonitor();
        _counter = new PerformanceCounter("test_counter");

        _expressions = new Expression[IterationCount];
        for (var i = 0; i < IterationCount; i++)
            _expressions[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Tracking")]
    public MemoryStatistics MemoryTracker_RecordAllocations()
    {
        _memoryTracker.Reset();
        for (var i = 0; i < IterationCount; i++)
            _memoryTracker.RecordAllocation(128);
        for (var i = 0; i < IterationCount / 2; i++)
            _memoryTracker.RecordDeallocation(128);
        return _memoryTracker.GetStatistics();
    }

    [Benchmark]
    [BenchmarkCategory("Tracking")]
    public MemoryStatistics MemoryTracker_RecordReuses()
    {
        _memoryTracker.Reset();
        for (var i = 0; i < IterationCount; i++)
        {
            _memoryTracker.RecordAllocation(256);
            _memoryTracker.RecordReuse();
        }
        return _memoryTracker.GetStatistics();
    }

    [Benchmark]
    [BenchmarkCategory("Tracking")]
    public void MemoryTracker_Reset()
    {
        for (var cycle = 0; cycle < 10; cycle++)
        {
            for (var i = 0; i < IterationCount; i++)
                _memoryTracker.RecordAllocation(64);
            _memoryTracker.Reset();
        }
    }

    [Benchmark]
    [BenchmarkCategory("Allocation")]
    public AllocationProfile AllocationProfiler_MultipleCategories()
    {
        _allocationProfiler.Reset();
        for (var i = 0; i < IterationCount; i++)
        {
            _allocationProfiler.Record("expressions", 64);
            _allocationProfiler.Record("caches", 128);
            _allocationProfiler.Record("pools", 32);
        }
        return _allocationProfiler.GetProfile();
    }

    [Benchmark]
    [BenchmarkCategory("Allocation")]
    public IReadOnlyDictionary<string, CategoryStats> AllocationProfiler_ByCategory()
    {
        _allocationProfiler.Reset();
        for (var i = 0; i < IterationCount; i++)
        {
            _allocationProfiler.Record("expressions", 64);
            _allocationProfiler.Record("caches", 128);
        }
        return _allocationProfiler.GetByCategory();
    }

    [Benchmark]
    [BenchmarkCategory("Buffer")]
    public byte[] BufferManager_RentAndWrite()
    {
        var buffer = _bufferManager.RentBuffer(4096);
        for (var i = 0; i < Math.Min(buffer.Length, 1024); i++)
            buffer[i] = (byte)(i & 0xFF);
        _bufferManager.ReturnBuffer(buffer);
        return buffer;
    }

    [Benchmark]
    [BenchmarkCategory("Buffer")]
    public byte[] BufferManager_MultipleRents()
    {
        var buffers = new byte[10][];
        for (var i = 0; i < 10; i++)
            buffers[i] = _bufferManager.RentBuffer(256);
        for (var i = 0; i < 10; i++)
            _bufferManager.ReturnBuffer(buffers[i]);
        return buffers[0];
    }

    [Benchmark]
    [BenchmarkCategory("Buffer")]
    public byte[] FreshArray_RentAndWrite()
    {
        var buffer = new byte[4096];
        for (var i = 0; i < 1024; i++)
            buffer[i] = (byte)(i & 0xFF);
        return buffer;
    }

    [Benchmark]
    [BenchmarkCategory("Pressure")]
    public double PressureMonitor_Update()
    {
        _pressureMonitor.Update();
        return _pressureMonitor.PressureLevel;
    }

    [Benchmark]
    [BenchmarkCategory("Pressure")]
    public bool PressureMonitor_IsHighPressure()
    {
        _pressureMonitor.Update();
        return _pressureMonitor.IsHighPressure;
    }

    [Benchmark]
    [BenchmarkCategory("Counter")]
    public long PerformanceCounter_IncrementAndRead()
    {
        _counter.Reset();
        for (var i = 0; i < IterationCount; i++)
            _counter.Increment();
        return _counter.Value;
    }

    [Benchmark]
    [BenchmarkCategory("Counter")]
    public double PerformanceCounter_RecordAndAverage()
    {
        _counter.Reset();
        for (var i = 0; i < IterationCount; i++)
            _counter.Record(i * 0.1);
        return _counter.Average;
    }

    [Benchmark]
    [BenchmarkCategory("Snapshot")]
    public MemoryStatistics FullMemorySnapshot()
    {
        _memoryTracker.Reset();
        for (var i = 0; i < IterationCount; i++)
        {
            _memoryTracker.RecordAllocation(128);
            if (i % 2 == 0)
                _memoryTracker.RecordDeallocation(64);
        }
        return _memoryTracker.GetStatistics();
    }
}
