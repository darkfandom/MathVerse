using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class AggregateRootTests
{
    private sealed class TestDomainEvent : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
        public string Payload { get; init; } = string.Empty;
    }

    private sealed class TestAggregate : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;

        public TestAggregate() : base() { }

        public TestAggregate(Guid id) : base(id) { }

        public void SetName(string name)
        {
            Name = name;
            RaiseEvent(new TestDomainEvent { Payload = $"Name set to {name}" });
        }
    }

    [Fact]
    public void Aggregate_HasId()
    {
        var aggregate = new TestAggregate();

        aggregate.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Aggregate_WithExplicitId()
    {
        var id = Guid.NewGuid();
        var aggregate = new TestAggregate(id);

        aggregate.Id.Should().Be(id);
    }

    [Fact]
    public void Aggregate_RaiseEvent_AddsToDomainEvents()
    {
        var aggregate = new TestAggregate();

        aggregate.SetName("test");

        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents[0].Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void Aggregate_ClearEvents_RemovesEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.SetName("test");

        aggregate.ClearEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_IncrementsVersion_OnEvent()
    {
        var aggregate = new TestAggregate();

        aggregate.Version.Should().Be(0);

        aggregate.SetName("test");

        aggregate.Version.Should().Be(1);
    }

    [Fact]
    public void Aggregate_MultipleEvents_Accumulate()
    {
        var aggregate = new TestAggregate();

        aggregate.SetName("first");
        aggregate.SetName("second");

        aggregate.DomainEvents.Should().HaveCount(2);
    }
}
