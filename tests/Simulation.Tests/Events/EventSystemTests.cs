namespace MathVerse.Simulation.Tests.Events;

using System.Collections.Immutable;

public sealed class EventSystemTests
{
    [Fact]
    public void SimulationEvent_Create_SetsProperties()
    {
        var evt = SimulationEvent.Create(1.0, "collision", EventType.Collision);
        evt.Time.Should().Be(1.0);
        evt.Name.Should().Be("collision");
        evt.Type.Should().Be(EventType.Collision);
    }

    [Fact]
    public void SimulationEvent_Create_DefaultPriorityIsNormal()
    {
        var evt = SimulationEvent.Create(0, "test", EventType.StateChange);
        evt.Priority.Should().Be(EventPriority.Normal);
    }

    [Fact]
    public void SimulationEvent_Create_DefaultDataIsEmpty()
    {
        var evt = SimulationEvent.Create(0, "test", EventType.StateChange);
        evt.Data.Should().BeEmpty();
    }

    [Fact]
    public void SimulationEvent_Create_DefaultIsEnabled()
    {
        var evt = SimulationEvent.Create(0, "test", EventType.StateChange);
        evt.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void SimulationEvent_Recurring_SetsProperties()
    {
        var handler = (SimulationState s) => { };
        var evt = SimulationEvent.Recurring(0, 1.0, "tick", handler);
        evt.IsRecurring.Should().BeTrue();
        evt.Interval.Should().Be(1.0);
        evt.Name.Should().Be("tick");
        evt.Type.Should().Be(EventType.Recurring);
    }

    [Fact]
    public void SimulationEvent_Recurring_DefaultMaxOccurrences()
    {
        var handler = (SimulationState s) => { };
        var evt = SimulationEvent.Recurring(0, 1.0, "tick", handler);
        evt.MaxOccurrences.Should().Be(int.MaxValue);
    }

    [Fact]
    public void SimulationEvent_Recurring_OccurrenceCountZero()
    {
        var handler = (SimulationState s) => { };
        var evt = SimulationEvent.Recurring(0, 1.0, "tick", handler);
        evt.OccurrenceCount.Should().Be(0);
    }

    [Fact]
    public void EventType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<EventType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void EventPriority_LowIsZero()
    {
        ((int)EventPriority.Low).Should().Be(0);
    }

    [Fact]
    public void EventPriority_NormalIsOneHundred()
    {
        ((int)EventPriority.Normal).Should().Be(100);
    }

    [Fact]
    public void EventPriority_HighIsTwoHundred()
    {
        ((int)EventPriority.High).Should().Be(200);
    }

    [Fact]
    public void EventPriority_CriticalIsThreeHundred()
    {
        ((int)EventPriority.Critical).Should().Be(300);
    }

    [Fact]
    public void EventQueue_EmptyQueue_CountIsZero()
    {
        var queue = new EventQueue();
        queue.Count.Should().Be(0);
    }

    [Fact]
    public void EventQueue_Enqueue_IncrementsCount()
    {
        var queue = new EventQueue();
        queue.Enqueue(SimulationEvent.Create(1, "test", EventType.TimePoint));
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void EventQueue_TryDequeue_EmptyQueue_ReturnsFalse()
    {
        var queue = new EventQueue();
        queue.TryDequeue(out var evt).Should().BeFalse();
        evt.Should().BeNull();
    }

    [Fact]
    public void EventQueue_TryDequeue_NonEmptyQueue_ReturnsTrue()
    {
        var queue = new EventQueue();
        queue.Enqueue(SimulationEvent.Create(1, "test", EventType.TimePoint));
        queue.TryDequeue(out var evt).Should().BeTrue();
        evt.Should().NotBeNull();
        evt!.Name.Should().Be("test");
    }

    [Fact]
    public void EventQueue_DequeueReducesCount()
    {
        var queue = new EventQueue();
        queue.Enqueue(SimulationEvent.Create(1, "test", EventType.TimePoint));
        queue.TryDequeue(out _);
        queue.Count.Should().Be(0);
    }

    [Fact]
    public void EventQueue_TryPeek_DoesNotDequeue()
    {
        var queue = new EventQueue();
        queue.Enqueue(SimulationEvent.Create(1, "test", EventType.TimePoint));
        queue.TryPeek(out _);
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void EventQueue_Clear_RemovesAll()
    {
        var queue = new EventQueue();
        queue.Enqueue(SimulationEvent.Create(1, "a", EventType.TimePoint));
        queue.Enqueue(SimulationEvent.Create(2, "b", EventType.TimePoint));
        queue.Clear();
        queue.Count.Should().Be(0);
    }

    [Fact]
    public void EventDispatcher_SubscribeAndPublish_InvokesHandler()
    {
        var dispatcher = new EventDispatcher();
        bool invoked = false;
        dispatcher.Subscribe("test", evt => invoked = true);
        dispatcher.Publish(SimulationEvent.Create(0, "test", EventType.StateChange));
        invoked.Should().BeTrue();
    }

    [Fact]
    public void EventDispatcher_Publish_NoSubscribers_DoesNotThrow()
    {
        var dispatcher = new EventDispatcher();
        Action act = () => dispatcher.Publish(SimulationEvent.Create(0, "test", EventType.StateChange));
        act.Should().NotThrow();
    }

    [Fact]
    public void EventDispatcher_Unsubscribe_StopsPublishing()
    {
        var dispatcher = new EventDispatcher();
        int count = 0;
        Action<SimulationEvent> handler = evt => count++;
        dispatcher.Subscribe("test", handler);
        dispatcher.Publish(SimulationEvent.Create(0, "test", EventType.StateChange));
        dispatcher.Unsubscribe("test", handler);
        dispatcher.Publish(SimulationEvent.Create(0, "test", EventType.StateChange));
        count.Should().Be(1);
    }

    [Fact]
    public void StateChangeEvent_DefaultValues()
    {
        var e = new StateChangeEvent();
        e.VariableName.Should().Be(string.Empty);
        e.OldValue.Should().Be(0);
        e.NewValue.Should().Be(0);
        e.Time.Should().Be(0);
    }
}
