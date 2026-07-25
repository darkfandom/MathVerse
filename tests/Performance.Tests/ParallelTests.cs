using MathVerse.Math.Performance.Parallel;

namespace MathVerse.Performance.Tests;

public sealed class ParallelTests
{
    [Fact]
    public void TaskPartitioner_Partition_NullItems_Throws()
    {
        Action act = () => TaskPartitioner.Partition<int>(null!, 4);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskPartitioner_Partition_ZeroPartitions_Throws()
    {
        Action act = () => TaskPartitioner.Partition([1, 2, 3], 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TaskPartitioner_Partition_NegativePartitions_Throws()
    {
        Action act = () => TaskPartitioner.Partition([1, 2, 3], -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TaskPartitioner_Partition_EmptyList_ReturnsEmpty()
    {
        var result = TaskPartitioner.Partition<int>([], 4);
        result.Should().BeEmpty();
    }

    [Fact]
    public void TaskPartitioner_Partition_SingleItem_ReturnsOnePartition()
    {
        var result = TaskPartitioner.Partition([42], 4);
        result.Should().HaveCount(1);
        result[0].Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void TaskPartitioner_Partition_PartitionCountExceedsItems_ReturnsItemCountPartitions()
    {
        var items = new[] { 1, 2, 3 };
        var result = TaskPartitioner.Partition(items, 10);
        result.Should().HaveCount(3);
    }

    [Fact]
    public void TaskPartitioner_Partition_AllItemsPreserved()
    {
        var items = Enumerable.Range(0, 20).ToList();
        var result = TaskPartitioner.Partition(items, 4);
        var flat = result.SelectMany(p => p).ToList();
        flat.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void TaskPartitioner_Partition_PreservesOrder()
    {
        var items = Enumerable.Range(0, 10).ToList();
        var result = TaskPartitioner.Partition(items, 3);
        var flat = result.SelectMany(p => p).ToList();
        flat.Should().Equal(items);
    }

    [Fact]
    public void TaskPartitioner_Partition_ExactDivision()
    {
        var items = Enumerable.Range(0, 12).ToList();
        var result = TaskPartitioner.Partition(items, 4);
        result.Should().HaveCount(4);
        foreach (var partition in result)
            partition.Should().HaveCount(3);
    }

    [Fact]
    public void TaskPartitioner_Partition_UnevenDivision()
    {
        var items = Enumerable.Range(0, 10).ToList();
        var result = TaskPartitioner.Partition(items, 3);
        result.Should().HaveCount(3);
        result[0].Count.Should().Be(4);
        result[1].Count.Should().Be(3);
        result[2].Count.Should().Be(3);
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_NullItems_Throws()
    {
        Action act = () => TaskPartitioner.PartitionBySize<int>(null!, 5);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_ZeroSize_Throws()
    {
        Action act = () => TaskPartitioner.PartitionBySize([1, 2, 3], 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_EmptyItems_ReturnsEmpty()
    {
        var result = TaskPartitioner.PartitionBySize<int>([], 5);
        result.Should().BeEmpty();
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_RespectsMaxSize()
    {
        var items = Enumerable.Range(0, 10).ToList();
        var result = TaskPartitioner.PartitionBySize(items, 3);
        foreach (var partition in result)
            partition.Count.Should().BeLessOrEqualTo(3);
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_AllItemsPreserved()
    {
        var items = Enumerable.Range(0, 7).ToList();
        var result = TaskPartitioner.PartitionBySize(items, 3);
        var flat = result.SelectMany(p => p).ToList();
        flat.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_SingleItem()
    {
        var result = TaskPartitioner.PartitionBySize([99], 5);
        result.Should().HaveCount(1);
        result[0].Should().ContainSingle().Which.Should().Be(99);
    }

    [Fact]
    public void TaskPartitioner_PartitionBySize_ExactFit()
    {
        var items = Enumerable.Range(0, 6).ToList();
        var result = TaskPartitioner.PartitionBySize(items, 3);
        result.Should().HaveCount(2);
        result[0].Should().HaveCount(3);
        result[1].Should().HaveCount(3);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_NullInputs_Throws()
    {
        var scheduler = new EvaluationScheduler();
        Action act = () => scheduler.EvaluateAll<int, int>(null!, x => x);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_NullEvaluator_Throws()
    {
        var scheduler = new EvaluationScheduler();
        Action act = () => scheduler.EvaluateAll<int, int>([1, 2, 3], null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_EmptyInputs_ReturnsEmpty()
    {
        var scheduler = new EvaluationScheduler();
        var result = scheduler.EvaluateAll<int, int>([], x => x * 2);
        result.Should().BeEmpty();
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_SingleInput_ReturnsSingleResult()
    {
        var scheduler = new EvaluationScheduler();
        var result = scheduler.EvaluateAll([21], x => x * 2);
        result.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_SerialMode_PreservesOrder()
    {
        var scheduler = new EvaluationScheduler();
        var options = new ParallelEvaluationOptions(1, false, CancellationToken.None, null);
        var items = Enumerable.Range(0, 100).ToList();
        var result = scheduler.EvaluateAll(items, x => x * 2, options);
        var expected = items.Select(x => x * 2).ToList();
        result.Should().Equal(expected);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_Deterministic_PreservesOrder()
    {
        var scheduler = new EvaluationScheduler();
        var options = new ParallelEvaluationOptions(4, true, CancellationToken.None, null);
        var items = Enumerable.Range(0, 50).ToList();
        var result = scheduler.EvaluateAll(items, x => x + 1, options);
        var expected = items.Select(x => x + 1).ToList();
        result.Should().Equal(expected);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_Parallel_Correctness()
    {
        var scheduler = new EvaluationScheduler();
        var options = new ParallelEvaluationOptions(4, false, CancellationToken.None, null);
        var items = Enumerable.Range(0, 100).ToList();
        var result = scheduler.EvaluateAll(items, x => x * x, options);
        var expected = items.Select(x => x * x).ToList();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_Cancellation_Throws()
    {
        var scheduler = new EvaluationScheduler();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var options = new ParallelEvaluationOptions(1, false, cts.Token, null);
        Action act = () => scheduler.EvaluateAll([1, 2, 3], x => x, options);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void EvaluationScheduler_EvaluateSingle_ReturnsCorrectResult()
    {
        var scheduler = new EvaluationScheduler();
        var result = scheduler.EvaluateSingle(21, x => x * 2);
        result.Should().Be(42);
    }

    [Fact]
    public void EvaluationScheduler_EvaluateSingle_NullEvaluator_Throws()
    {
        var scheduler = new EvaluationScheduler();
        Action act = () => scheduler.EvaluateSingle<int, int>(1, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WorkScheduler_Enqueue_NullWork_Throws()
    {
        var scheduler = new WorkScheduler();
        Action act = () => scheduler.Enqueue(0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WorkScheduler_Enqueue_IncrementsCount()
    {
        var scheduler = new WorkScheduler();
        scheduler.Enqueue(0, () => { });
        scheduler.Count.Should().Be(1);
        scheduler.Enqueue(1, () => { });
        scheduler.Count.Should().Be(2);
    }

    [Fact]
    public void WorkScheduler_Dequeue_Empty_ReturnsNull()
    {
        var scheduler = new WorkScheduler();
        scheduler.Dequeue().Should().BeNull();
    }

    [Fact]
    public void WorkScheduler_Dequeue_HighestPriorityFirst()
    {
        var scheduler = new WorkScheduler();
        var order = new List<int>();
        scheduler.Enqueue(5, () => order.Add(5));
        scheduler.Enqueue(0, () => order.Add(0));
        scheduler.Enqueue(3, () => order.Add(3));
        scheduler.Dequeue()!.Invoke();
        scheduler.Dequeue()!.Invoke();
        scheduler.Dequeue()!.Invoke();
        order.Should().Equal(0, 3, 5);
    }

    [Fact]
    public void WorkScheduler_Dequeue_DecrementsCount()
    {
        var scheduler = new WorkScheduler();
        scheduler.Enqueue(0, () => { });
        scheduler.Enqueue(0, () => { });
        scheduler.Count.Should().Be(2);
        scheduler.Dequeue();
        scheduler.Count.Should().Be(1);
    }

    [Fact]
    public void WorkScheduler_Clear_ResetsCount()
    {
        var scheduler = new WorkScheduler();
        for (var i = 0; i < 10; i++)
            scheduler.Enqueue(i % 16, () => { });
        scheduler.Count.Should().Be(10);
        scheduler.Clear();
        scheduler.Count.Should().Be(0);
        scheduler.Dequeue().Should().BeNull();
    }

    [Fact]
    public void WorkScheduler_Enqueue_ClampsPriority()
    {
        var scheduler = new WorkScheduler();
        scheduler.Enqueue(-5, () => { });
        scheduler.Enqueue(999, () => { });
        scheduler.Count.Should().Be(2);
        scheduler.Dequeue().Should().NotBeNull();
        scheduler.Dequeue().Should().NotBeNull();
    }

    [Fact]
    public void WorkScheduler_ConcurrentEnqueueDequeue_ThreadSafe()
    {
        var scheduler = new WorkScheduler();
        var counter = 0;
        Parallel.For(0, 1000, _ => scheduler.Enqueue(0, () => Interlocked.Increment(ref counter)));
        scheduler.Count.Should().Be(1000);
        var threads = 4;
        var perThread = 1000 / threads;
        Parallel.For(0, threads, _ =>
        {
            for (var i = 0; i < perThread; i++)
                scheduler.Dequeue()?.Invoke();
        });
        scheduler.Count.Should().Be(0);
        counter.Should().Be(1000);
    }

    [Fact]
    public void ParallelEvaluationOptions_Default_Values()
    {
        var options = ParallelEvaluationOptions.Default;
        options.MaxDegreeOfParallelism.Should().BeGreaterThan(0);
        options.Deterministic.Should().BeFalse();
        options.CancellationToken.Should().Be(CancellationToken.None);
        options.Timeout.Should().BeNull();
    }

    [Fact]
    public void ParallelEvaluationOptions_InvalidDegree_Throws()
    {
        Action act = () => new ParallelEvaluationOptions(0, false, CancellationToken.None, null);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ParallelEvaluationOptions_RecordEquality()
    {
        var a = new ParallelEvaluationOptions(2, true, CancellationToken.None, TimeSpan.FromSeconds(1));
        var b = new ParallelEvaluationOptions(2, true, CancellationToken.None, TimeSpan.FromSeconds(1));
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ParallelExecutionStatistics_RecordStruct_Values()
    {
        var stats = new ParallelExecutionStatistics
        {
            TotalTasks = 100,
            CompletedTasks = 95,
            FailedTasks = 5,
            AverageTaskTimeMs = 1.5,
            TotalTimeMs = 150,
            PeakConcurrency = 4
        };
        stats.TotalTasks.Should().Be(100);
        stats.CompletedTasks.Should().Be(95);
        stats.FailedTasks.Should().Be(5);
        stats.AverageTaskTimeMs.Should().Be(1.5);
        stats.TotalTimeMs.Should().Be(150);
        stats.PeakConcurrency.Should().Be(4);
    }

    [Fact]
    public void ParallelExecutionStatistics_ToString_ContainsValues()
    {
        var stats = new ParallelExecutionStatistics
        {
            TotalTasks = 10,
            CompletedTasks = 8,
            FailedTasks = 2,
            AverageTaskTimeMs = 3.14,
            TotalTimeMs = 31,
            PeakConcurrency = 2
        };
        var str = stats.ToString();
        str.Should().Contain("TotalTasks=10");
        str.Should().Contain("CompletedTasks=8");
        str.Should().Contain("FailedTasks=2");
    }

    [Fact]
    public void ParallelExecutionContext_Create_SetsToken()
    {
        using var cts = new CancellationTokenSource();
        var ctx = ParallelExecutionContext.Create(cts.Token);
        ctx.Token.Should().Be(cts.Token);
    }

    [Fact]
    public void ParallelExecutionContext_AddResult_StoresResult()
    {
        var ctx = ParallelExecutionContext.Create(CancellationToken.None);
        ctx.AddResult("hello");
        ctx.AddResult(42);
        ctx.Results.Should().HaveCount(2);
        ctx.Results.Should().Contain("hello");
        ctx.Results.Should().Contain(42);
    }

    [Fact]
    public void ParallelExecutionContext_InitialStates()
    {
        var ctx = ParallelExecutionContext.Create(CancellationToken.None);
        ctx.IsCompleted.Should().BeFalse();
        ctx.Error.Should().BeNull();
        ctx.TaskId.Should().Be(0);
        ctx.Results.Should().BeEmpty();
    }

    [Fact]
    public void ParallelExecutionContext_AddResult_WhenCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ctx = ParallelExecutionContext.Create(cts.Token);
        Action act = () => ctx.AddResult("x");
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void EvaluationScheduler_EvaluateAll_LargeInput_ThreadSafety()
    {
        var scheduler = new EvaluationScheduler();
        var items = Enumerable.Range(0, 500).ToList();
        var seen = new System.Collections.Concurrent.ConcurrentBag<int>();
        var options = new ParallelEvaluationOptions(8, false, CancellationToken.None, null);
        scheduler.EvaluateAll(items, x =>
        {
            seen.Add(x);
            return x;
        }, options);
        seen.Should().HaveCount(500);
        seen.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void TaskPartitioner_Partition_SinglePartition()
    {
        var items = Enumerable.Range(0, 5).ToList();
        var result = TaskPartitioner.Partition(items, 1);
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(items);
    }
}
