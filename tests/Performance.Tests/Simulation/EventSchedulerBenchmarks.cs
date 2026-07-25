using BenchmarkDotNet.Attributes;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Events;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class EventSchedulerBenchmarks
{
    private EventQueue _queue = null!;
    private EventDispatcher _dispatcher = null!;
    private SimulationEvent _event = null!;
    private SimulationEvent _recurringEvent = null!;
    private SimulationState _state = null!;
    private Action<SimulationState> _handler = null!;
    private SimulationEvent _lowPriorityEvent = null!;
    private SimulationEvent _highPriorityEvent = null!;

    [GlobalSetup]
    public void Setup()
    {
        _queue = new EventQueue();
        _dispatcher = new EventDispatcher();
        _state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty
            .Add("x", 1.0).Add("y", 2.0));

        _handler = _ => { };
        _event = SimulationEvent.Create(1.0, "TestEvent", EventType.TimePoint, _handler);
        _recurringEvent = SimulationEvent.Recurring(0.0, 0.5, "RecurringEvent", _ => { }, 100);
        _lowPriorityEvent = SimulationEvent.Create(1.0, "LowEvent", EventType.TimePoint, priority: EventPriority.Low);
        _highPriorityEvent = SimulationEvent.Create(1.0, "HighEvent", EventType.TimePoint, priority: EventPriority.High);
    }

    [Benchmark]
    public void EventQueue_Enqueue()
    {
        _queue.Clear();
        _queue.Enqueue(_event);
    }

    [Benchmark]
    public bool EventQueue_TryDequeue()
    {
        _queue.Clear();
        _queue.Enqueue(_event);
        return _queue.TryDequeue(out _);
    }

    [Benchmark]
    public bool EventQueue_TryPeek()
    {
        _queue.Clear();
        _queue.Enqueue(_event);
        return _queue.TryPeek(out _);
    }

    [Benchmark]
    public int EventQueue_EnqueueDequeue_100()
    {
        _queue.Clear();
        for (int i = 0; i < 100; i++)
            _queue.Enqueue(SimulationEvent.Create(i * 0.1, $"Event{i}", EventType.TimePoint));

        int count = 0;
        while (_queue.TryDequeue(out _))
            count++;
        return count;
    }

    [Benchmark]
    public void EventQueue_Enqueue_1000()
    {
        _queue.Clear();
        for (int i = 0; i < 1000; i++)
            _queue.Enqueue(SimulationEvent.Create(i * 0.01, $"Event{i}", EventType.TimePoint));
    }

    [Benchmark]
    public int EventQueue_DequeueAll_1000()
    {
        _queue.Clear();
        for (int i = 0; i < 1000; i++)
            _queue.Enqueue(SimulationEvent.Create(i * 0.01, $"Event{i}", EventType.TimePoint));

        int count = 0;
        while (_queue.TryDequeue(out _))
            count++;
        return count;
    }

    [Benchmark]
    public void EventQueue_Clear()
    {
        for (int i = 0; i < 10; i++)
            _queue.Enqueue(SimulationEvent.Create(i, $"Event{i}", EventType.TimePoint));
        _queue.Clear();
    }

    [Benchmark]
    public int EventQueue_Count()
    {
        _queue.Clear();
        for (int i = 0; i < 50; i++)
            _queue.Enqueue(SimulationEvent.Create(i, $"Event{i}", EventType.TimePoint));
        return _queue.Count;
    }

    [Benchmark]
    public int EventQueue_RecurringEvent()
    {
        _queue.Clear();
        _queue.Enqueue(_recurringEvent);
        int count = 0;
        while (count < 10 && _queue.TryDequeue(out _))
            count++;
        return count;
    }

    [Benchmark]
    public int EventQueue_MixedPriorities()
    {
        _queue.Clear();
        var priorities = new[] { EventPriority.Low, EventPriority.Normal, EventPriority.High, EventPriority.Critical };
        for (int i = 0; i < 20; i++)
        {
            var priority = priorities[i % 4];
            _queue.Enqueue(SimulationEvent.Create(i * 0.1, $"Event{i}", EventType.TimePoint, priority: priority));
        }

        int count = 0;
        while (_queue.TryDequeue(out _))
            count++;
        return count;
    }

    [Benchmark]
    public void EventDispatcher_Subscribe()
    {
        _dispatcher.Subscribe("TestEvent", static _ => { });
    }

    [Benchmark]
    public void EventDispatcher_Unsubscribe()
    {
        Action<SimulationEvent> handler = static _ => { };
        _dispatcher.Subscribe("TestEvent", handler);
        _dispatcher.Unsubscribe("TestEvent", handler);
    }

    [Benchmark]
    public void EventDispatcher_Publish()
    {
        Action<SimulationEvent> handler = static _ => { };
        _dispatcher.Subscribe("TestEvent", handler);
        _dispatcher.Publish(_event);
    }

    [Benchmark]
    public void EventDispatcher_Publish_10Subscribers()
    {
        var handlers = new Action<SimulationEvent>[10];
        for (int i = 0; i < 10; i++)
        {
            handlers[i] = static _ => { };
            _dispatcher.Subscribe("TestEvent", handlers[i]);
        }
        _dispatcher.Publish(_event);
        for (int i = 0; i < 10; i++)
            _dispatcher.Unsubscribe("TestEvent", handlers[i]);
    }

    [Benchmark]
    public void EventDispatcher_Schedule()
    {
        _dispatcher.Schedule(_event);
    }

    [Benchmark]
    public bool EventDispatcher_ProcessNext()
    {
        _dispatcher.Schedule(_event);
        return _dispatcher.ProcessNext(out _);
    }

    [Benchmark]
    public int EventDispatcher_ScheduleAndProcess_100()
    {
        for (int i = 0; i < 100; i++)
            _dispatcher.Schedule(SimulationEvent.Create(i * 0.1, $"Event{i}", EventType.TimePoint));

        int count = 0;
        while (_dispatcher.ProcessNext(out _))
            count++;
        return count;
    }

    [Benchmark]
    public void EventDispatcher_SubscribeUnsubscribe()
    {
        Action<SimulationEvent> handler = static _ => { };
        _dispatcher.Subscribe("TestEvent", handler);
        _dispatcher.Subscribe("TestEvent", handler);
        _dispatcher.Unsubscribe("TestEvent", handler);
    }

    [Benchmark]
    public SimulationEvent SimulationEvent_Create()
    {
        return SimulationEvent.Create(1.5, "CreatedEvent", EventType.StateChange, _handler);
    }

    [Benchmark]
    public SimulationEvent SimulationEvent_Recurring()
    {
        return SimulationEvent.Recurring(0.0, 0.25, "Recurring", _ => { }, 50);
    }

    [Benchmark]
    public SimulationEvent SimulationEvent_WithPriority()
    {
        return SimulationEvent.Create(2.0, "PriorityEvent", EventType.ThresholdCrossing, priority: EventPriority.Critical);
    }

    [Benchmark]
    public SimulationEvent SimulationEvent_WithHandler()
    {
        return SimulationEvent.Create(3.0, "HandledEvent", EventType.ConditionMet, state =>
        {
            var _ = state.CurrentTime;
        });
    }

    [Benchmark]
    public int EventQueue_SortedDequeue()
    {
        _queue.Clear();
        var rng = new Random(42);
        for (int i = 0; i < 50; i++)
            _queue.Enqueue(SimulationEvent.Create(rng.NextDouble() * 100, $"Event{i}", EventType.TimePoint));

        int count = 0;
        while (_queue.TryDequeue(out _))
            count++;
        return count;
    }

    [Benchmark]
    public EventType EventType_EnumValues()
    {
        var values = Enum.GetValues<EventType>();
        return values[values.Length - 1];
    }

    [Benchmark]
    public EventPriority EventPriority_EnumValues()
    {
        var values = Enum.GetValues<EventPriority>();
        return values[values.Length - 1];
    }
}
