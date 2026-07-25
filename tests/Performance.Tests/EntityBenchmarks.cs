using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class EntityBenchmarks
{
    public sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    public sealed class TestAggregate : AggregateRoot
    {
        public void DoWork() => RaiseEvent(new TestEvent());
    }

    private sealed class TestEvent : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }

    private readonly Guid _id = Guid.NewGuid();

    [Benchmark(Baseline = true)]
    public TestEntity EntityCreation() => new(_id);

    [Benchmark]
    public bool EntityEquality()
    {
        var e1 = new TestEntity(_id);
        var e2 = new TestEntity(_id);
        return e1 == e2;
    }

    [Benchmark]
    public int EntityGetHashCode() => new TestEntity(_id).GetHashCode();

    [Benchmark]
    public TestAggregate AggregateCreation() => new();

    [Benchmark]
    public int AggregateRaiseEvent()
    {
        var agg = new TestAggregate();
        agg.DoWork();
        return agg.DomainEvents.Count;
    }
}
