namespace MathVerse.Performance.Tests;

public sealed class MemoryTests
{
    [Fact]
    public void MemoryTracker_RecordAllocation_IncreasesCurrentBytes()
    {
        var tracker = new MemoryTracker();

        tracker.RecordAllocation(100);

        tracker.CurrentBytes.Should().Be(100);
    }

    [Fact]
    public void MemoryTracker_RecordDeallocation_DecreasesCurrentBytes()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);

        tracker.RecordDeallocation(40);

        tracker.CurrentBytes.Should().Be(60);
    }

    [Fact]
    public void MemoryTracker_PeakBytes_TracksMaximum()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);
        tracker.RecordDeallocation(50);
        tracker.RecordAllocation(80);

        tracker.PeakBytes.Should().Be(130);
    }

    [Fact]
    public void MemoryTracker_ZeroAllocation_Ignored()
    {
        var tracker = new MemoryTracker();

        tracker.RecordAllocation(0);

        tracker.CurrentBytes.Should().Be(0);
    }

    [Fact]
    public void MemoryTracker_NegativeAllocation_Ignored()
    {
        var tracker = new MemoryTracker();

        tracker.RecordAllocation(-10);

        tracker.CurrentBytes.Should().Be(0);
    }

    [Fact]
    public void MemoryTracker_ZeroDeallocation_Ignored()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);

        tracker.RecordDeallocation(0);

        tracker.CurrentBytes.Should().Be(100);
    }

    [Fact]
    public void MemoryTracker_NegativeDeallocation_Ignored()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);

        tracker.RecordDeallocation(-10);

        tracker.CurrentBytes.Should().Be(100);
    }

    [Fact]
    public void MemoryTracker_RecordReuse()
    {
        var tracker = new MemoryTracker();

        tracker.RecordReuse();
        tracker.RecordReuse();
        tracker.RecordReuse();

        var stats = tracker.GetStatistics();
        stats.ObjectReuseCount.Should().Be(3);
    }

    [Fact]
    public void MemoryTracker_GetStatistics()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);
        tracker.RecordReuse();

        var stats = tracker.GetStatistics();

        stats.CurrentAllocations.Should().Be(100);
        stats.PeakAllocations.Should().Be(100);
        stats.TotalAllocations.Should().Be(100);
        stats.ObjectReuseCount.Should().Be(1);
    }

    [Fact]
    public void MemoryTracker_Reset()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);
        tracker.RecordReuse();

        tracker.Reset();

        var stats = tracker.GetStatistics();
        stats.CurrentAllocations.Should().Be(0);
        stats.PeakAllocations.Should().Be(0);
        stats.TotalAllocations.Should().Be(0);
        stats.ObjectReuseCount.Should().Be(0);
    }

    [Fact]
    public void MemoryTracker_TotalAllocations_Cumulative()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);
        tracker.RecordDeallocation(100);
        tracker.RecordAllocation(200);

        var stats = tracker.GetStatistics();
        stats.TotalAllocations.Should().Be(300);
        stats.CurrentAllocations.Should().Be(200);
    }

    [Fact]
    public async Task MemoryTracker_ThreadSafety()
    {
        var tracker = new MemoryTracker();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => tracker.RecordAllocation(10)))
            .ToArray();

        await Task.WhenAll(tasks);

        tracker.CurrentBytes.Should().Be(1000);
    }

    [Fact]
    public async Task MemoryTracker_ThreadSafety_ConcurrentAllocDealloc()
    {
        var tracker = new MemoryTracker();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
            tasks.Add(Task.Run(() => tracker.RecordAllocation(10)));
        for (int i = 0; i < 50; i++)
            tasks.Add(Task.Run(() => tracker.RecordDeallocation(10)));

        await Task.WhenAll(tasks);

        tracker.CurrentBytes.Should().Be(0);
    }

    [Fact]
    public void MemoryStatistics_RecordStructEquality()
    {
        var a = new MemoryStatistics { CurrentAllocations = 100, PeakAllocations = 200 };
        var b = new MemoryStatistics { CurrentAllocations = 100, PeakAllocations = 200 };

        a.Should().Be(b);
    }

    [Fact]
    public void MemoryStatistics_ToString()
    {
        var stats = new MemoryStatistics
        {
            CurrentAllocations = 100,
            PeakAllocations = 200,
            TotalAllocations = 300
        };

        var str = stats.ToString();
        str.Should().Contain("CurrentAllocations=100");
        str.Should().Contain("PeakAllocations=200");
    }

    [Fact]
    public void MemoryStatistics_AllProperties()
    {
        var stats = new MemoryStatistics
        {
            CurrentAllocations = 100,
            PeakAllocations = 200,
            TotalAllocations = 300,
            CacheMemoryBytes = 400,
            PoolMemoryBytes = 500,
            BufferMemoryBytes = 600,
            ObjectReuseCount = 7,
            Gen0Collections = 1,
            Gen1Collections = 2,
            Gen2Collections = 3
        };

        stats.CurrentAllocations.Should().Be(100);
        stats.PeakAllocations.Should().Be(200);
        stats.TotalAllocations.Should().Be(300);
        stats.CacheMemoryBytes.Should().Be(400);
        stats.PoolMemoryBytes.Should().Be(500);
        stats.BufferMemoryBytes.Should().Be(600);
        stats.ObjectReuseCount.Should().Be(7);
        stats.Gen0Collections.Should().Be(1);
        stats.Gen1Collections.Should().Be(2);
        stats.Gen2Collections.Should().Be(3);
    }

    [Fact]
    public void MemoryPressureMonitor_InitialPressure_IsZero()
    {
        var monitor = new MemoryPressureMonitor();

        monitor.PressureLevel.Should().BeGreaterThanOrEqualTo(0.0);
        monitor.PressureLevel.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void MemoryPressureMonitor_Update_SetsPressure()
    {
        var monitor = new MemoryPressureMonitor();

        monitor.Update();

        monitor.PressureLevel.Should().BeGreaterThanOrEqualTo(0.0);
        monitor.PressureLevel.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void MemoryPressureMonitor_HighPressure_InitiallyFalse()
    {
        var monitor = new MemoryPressureMonitor();

        monitor.IsHighPressure.Should().BeFalse();
    }

    [Fact]
    public void MemoryPressureMonitor_Configure_ValidThresholds()
    {
        var monitor = new MemoryPressureMonitor();

        Action act = () => monitor.Configure(1024 * 1024, 2048 * 1024);

        act.Should().NotThrow();
    }

    [Fact]
    public void MemoryPressureMonitor_Configure_ZeroWarning_Throws()
    {
        var monitor = new MemoryPressureMonitor();

        Action act = () => monitor.Configure(0, 1024);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MemoryPressureMonitor_Configure_NegativeWarning_Throws()
    {
        var monitor = new MemoryPressureMonitor();

        Action act = () => monitor.Configure(-1, 1024);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MemoryPressureMonitor_Configure_CriticalNotExceedWarning_Throws()
    {
        var monitor = new MemoryPressureMonitor();

        Action act = () => monitor.Configure(1024, 512);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MemoryPressureMonitor_Configure_CriticalEqualWarning_Throws()
    {
        var monitor = new MemoryPressureMonitor();

        Action act = () => monitor.Configure(1024, 1024);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MemoryPressureMonitor_MultipleUpdates()
    {
        var monitor = new MemoryPressureMonitor();

        for (int i = 0; i < 10; i++)
            monitor.Update();

        monitor.PressureLevel.Should().BeGreaterThanOrEqualTo(0.0);
        monitor.PressureLevel.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void AllocationProfiler_RecordSingle()
    {
        var profiler = new AllocationProfiler();

        profiler.Record("cache", 1024);

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(1024);
        profile.TotalCount.Should().Be(1);
    }

    [Fact]
    public void AllocationProfiler_RecordMultiple_SameCategory()
    {
        var profiler = new AllocationProfiler();

        profiler.Record("cache", 1024);
        profiler.Record("cache", 512);

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(1536);
        profile.TotalCount.Should().Be(2);
    }

    [Fact]
    public void AllocationProfiler_RecordDifferentCategories()
    {
        var profiler = new AllocationProfiler();

        profiler.Record("cache", 1024);
        profiler.Record("pool", 512);

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(1536);
        profile.TotalCount.Should().Be(2);
    }

    [Fact]
    public void AllocationProfiler_Reset()
    {
        var profiler = new AllocationProfiler();
        profiler.Record("cache", 1024);

        profiler.Reset();

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(0);
        profile.TotalCount.Should().Be(0);
    }

    [Fact]
    public void AllocationProfiler_NullCategory_Throws()
    {
        var profiler = new AllocationProfiler();
        Action act = () => profiler.Record(null!, 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AllocationProfiler_EmptyCategory_Throws()
    {
        var profiler = new AllocationProfiler();
        Action act = () => profiler.Record("", 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AllocationProfiler_WhitespaceCategory_Throws()
    {
        var profiler = new AllocationProfiler();
        Action act = () => profiler.Record("   ", 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AllocationProfiler_GetByCategory()
    {
        var profiler = new AllocationProfiler();
        profiler.Record("cache", 1024);
        profiler.Record("pool", 512);
        profiler.Record("cache", 256);

        var byCategory = profiler.GetByCategory();

        byCategory.Should().HaveCount(2);
        byCategory["cache"].Bytes.Should().Be(1280);
        byCategory["cache"].Count.Should().Be(2);
        byCategory["pool"].Bytes.Should().Be(512);
        byCategory["pool"].Count.Should().Be(1);
    }

    [Fact]
    public void AllocationProfile_TotalBytes()
    {
        var categories = new Dictionary<string, CategoryStats>
        {
            ["a"] = new(100, 1),
            ["b"] = new(200, 3)
        };
        var profile = new AllocationProfile(categories);

        profile.TotalBytes.Should().Be(300);
    }

    [Fact]
    public void AllocationProfile_TotalCount()
    {
        var categories = new Dictionary<string, CategoryStats>
        {
            ["a"] = new(100, 2),
            ["b"] = new(200, 3)
        };
        var profile = new AllocationProfile(categories);

        profile.TotalCount.Should().Be(5);
    }

    [Fact]
    public void AllocationProfile_Categories()
    {
        var categories = new Dictionary<string, CategoryStats>
        {
            ["a"] = new(100, 1)
        };
        var profile = new AllocationProfile(categories);

        profile.Categories.Should().HaveCount(1);
    }

    [Fact]
    public void AllocationProfile_NullCategories_Throws()
    {
        Action act = () => new AllocationProfile(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AllocationProfile_EmptyCategories()
    {
        var profile = new AllocationProfile(new Dictionary<string, CategoryStats>());

        profile.TotalBytes.Should().Be(0);
        profile.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task AllocationProfiler_ThreadSafety()
    {
        var profiler = new AllocationProfiler();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => profiler.Record("cat", 10)))
            .ToArray();

        await Task.WhenAll(tasks);

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(1000);
        profile.TotalCount.Should().Be(100);
    }

    [Fact]
    public void BufferManager_RentBuffer()
    {
        var manager = new BufferManager();
        var buffer = manager.RentBuffer(128);

        buffer.Should().NotBeNull();
        buffer.Length.Should().BeGreaterThanOrEqualTo(128);
        manager.ActiveBuffers.Should().Be(1);

        manager.ReturnBuffer(buffer);
    }

    [Fact]
    public void BufferManager_ReturnBuffer_DecrementsActive()
    {
        var manager = new BufferManager();
        var buffer = manager.RentBuffer(64);

        manager.ActiveBuffers.Should().Be(1);

        manager.ReturnBuffer(buffer);

        manager.ActiveBuffers.Should().Be(0);
        manager.PooledBuffers.Should().Be(1);
    }

    [Fact]
    public void BufferManager_ZeroSize_Throws()
    {
        var manager = new BufferManager();
        Action act = () => manager.RentBuffer(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BufferManager_NegativeSize_Throws()
    {
        var manager = new BufferManager();
        Action act = () => manager.RentBuffer(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BufferManager_ReturnNull_Throws()
    {
        var manager = new BufferManager();
        Action act = () => manager.ReturnBuffer(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BufferManager_NullPool_Throws()
    {
        Action act = () => new BufferManager(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BufferManager_Clear()
    {
        var manager = new BufferManager();
        var buffer = manager.RentBuffer(64);
        manager.ReturnBuffer(buffer);

        manager.PooledBuffers.Should().Be(1);

        manager.Clear();

        manager.PooledBuffers.Should().Be(0);
    }

    [Fact]
    public void BufferManager_MultipleRents()
    {
        var manager = new BufferManager();
        var buffers = Enumerable.Range(0, 10)
            .Select(_ => manager.RentBuffer(64))
            .ToList();

        manager.ActiveBuffers.Should().Be(10);

        foreach (var buf in buffers)
            manager.ReturnBuffer(buf);

        manager.ActiveBuffers.Should().Be(0);
    }

    [Fact]
    public async Task BufferManager_ThreadSafety()
    {
        var manager = new BufferManager();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() =>
            {
                var buffer = manager.RentBuffer(64);
                buffer[0] = 1;
                manager.ReturnBuffer(buffer);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        manager.ActiveBuffers.Should().Be(0);
    }

    [Fact]
    public void MemoryTracker_LargeAllocations()
    {
        var tracker = new MemoryTracker();

        tracker.RecordAllocation(1024 * 1024 * 100);

        tracker.CurrentBytes.Should().Be(1024 * 1024 * 100);
        tracker.PeakBytes.Should().Be(1024 * 1024 * 100);
    }

    [Fact]
    public void MemoryTracker_OverDealloc_Clamps()
    {
        var tracker = new MemoryTracker();
        tracker.RecordAllocation(100);
        tracker.RecordDeallocation(200);

        tracker.CurrentBytes.Should().Be(-100);
    }

    [Fact]
    public void AllocationProfiler_CategoryStatsRecord()
    {
        var stats = new CategoryStats(100, 5);
        stats.Bytes.Should().Be(100);
        stats.Count.Should().Be(5);
    }

    [Fact]
    public void CategoryStats_Equality()
    {
        var a = new CategoryStats(100, 5);
        var b = new CategoryStats(100, 5);

        a.Should().Be(b);
    }

    [Fact]
    public void CategoryStats_Inequality()
    {
        var a = new CategoryStats(100, 5);
        var b = new CategoryStats(100, 6);

        a.Should().NotBe(b);
    }

    [Fact]
    public void BufferManager_ActiveBuffersInitiallyZero()
    {
        var manager = new BufferManager();

        manager.ActiveBuffers.Should().Be(0);
        manager.PooledBuffers.Should().Be(0);
    }

    [Fact]
    public void MemoryTracker_GetStatistics_GenCollections()
    {
        var tracker = new MemoryTracker();

        var stats = tracker.GetStatistics();

        stats.Gen0Collections.Should().BeGreaterThanOrEqualTo(0);
        stats.Gen1Collections.Should().BeGreaterThanOrEqualTo(0);
        stats.Gen2Collections.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void AllocationProfiler_NegativeBytes_Recorded()
    {
        var profiler = new AllocationProfiler();

        profiler.Record("cat", -100);

        var profile = profiler.GetProfile();
        profile.TotalBytes.Should().Be(-100);
    }

    [Fact]
    public void MemoryPressureMonitor_InitiallyLowPressure()
    {
        var monitor = new MemoryPressureMonitor();

        monitor.Update();

        monitor.IsHighPressure.Should().BeFalse();
    }

    [Fact]
    public void BufferManager_BufferDataCleared()
    {
        var manager = new BufferManager();
        var buffer = manager.RentBuffer(128);

        buffer[0] = 42;
        buffer[100] = 99;

        manager.ReturnBuffer(buffer);

        var buffer2 = manager.RentBuffer(128);
        buffer2[0].Should().Be(0);
        buffer2[100].Should().Be(0);

        manager.ReturnBuffer(buffer2);
    }

    [Fact]
    public void MemoryPressureMonitor_MultipleUpdates_PressureBounded()
    {
        var monitor = new MemoryPressureMonitor();

        for (int i = 0; i < 50; i++)
            monitor.Update();

        monitor.PressureLevel.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void BufferManager_RentMultipleSizes()
    {
        var manager = new BufferManager();
        var small = manager.RentBuffer(16);
        var medium = manager.RentBuffer(512);
        var large = manager.RentBuffer(4096);

        small.Length.Should().BeGreaterThanOrEqualTo(16);
        medium.Length.Should().BeGreaterThanOrEqualTo(512);
        large.Length.Should().BeGreaterThanOrEqualTo(4096);

        manager.ReturnBuffer(small);
        manager.ReturnBuffer(medium);
        manager.ReturnBuffer(large);
    }
}
