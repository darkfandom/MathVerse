namespace MathVerse.Math.Simulation.Events;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Simulation.Core;

public sealed record SimulationEvent
{
    public double Time { get; init; }
    public string Name { get; init; } = string.Empty;
    public EventType Type { get; init; }
    public ImmutableDictionary<string, object> Data { get; init; } = ImmutableDictionary<string, object>.Empty;
    public EventPriority Priority { get; init; }
    public Action<SimulationState>? Handler { get; init; }
    public bool IsRecurring { get; init; }
    public double Interval { get; init; }
    public int MaxOccurrences { get; init; }
    public int OccurrenceCount { get; init; }
    public bool IsEnabled { get; init; } = true;

    public static SimulationEvent Create(double time, string name, EventType type, Action<SimulationState>? handler = null, ImmutableDictionary<string, object>? data = null, EventPriority priority = EventPriority.Normal) =>
        new()
        {
            Time = time,
            Name = name,
            Type = type,
            Data = data ?? ImmutableDictionary<string, object>.Empty,
            Priority = priority,
            Handler = handler
        };

    public static SimulationEvent Recurring(double startTime, double interval, string name, Action<SimulationState> handler, int maxOccurrences = int.MaxValue, ImmutableDictionary<string, object>? data = null) =>
        new()
        {
            Time = 0,
            Interval = interval,
            Name = name,
            Type = EventType.Recurring,
            Handler = handler,
            IsRecurring = true,
            IsEnabled = true,
            MaxOccurrences = maxOccurrences,
            Data = data ?? ImmutableDictionary<string, object>.Empty
        };
}

public enum EventType
{
    StateChange,
    ThresholdCrossing,
    TimePoint,
    Recurring,
    ConditionMet,
    ExternalTrigger,
    Collision,
    BoundaryHit,
    Custom
}

public enum EventPriority
{
    Low = 0,
    Normal = 100,
    High = 200,
    Critical = 300
}

public sealed class EventQueue
{
    private readonly PriorityQueue<SimulationEvent, (int, double)> _queue = new();
    private int _sequence = 0;

    public int Count => _queue.Count;

    public void Enqueue(SimulationEvent evt)
    {
        int priority = (int)evt.Priority * 1000000 + evt.OccurrenceCount;
        _queue.Enqueue(evt, (priority, evt.Time));
        _sequence++;
    }

    public bool TryDequeue(out SimulationEvent? evt)
    {
        if (_queue.TryDequeue(out evt, out _))
        {
            if (evt.IsRecurring && evt.IsEnabled && evt.OccurrenceCount < evt.MaxOccurrences)
            {
                var nextEvent = evt with { Time = evt.Time + evt.Interval, OccurrenceCount = evt.OccurrenceCount + 1 };
                Enqueue(nextEvent);
            }
            return true;
        }
        evt = null;
        return false;
    }

    public bool TryPeek(out SimulationEvent? evt)
    {
        return _queue.TryPeek(out evt, out _);
    }

    public void Clear() => _queue.Clear();
}

public sealed class EventDispatcher
{
    private readonly EventQueue _queue = new();
    private readonly Dictionary<string, List<Action<SimulationEvent>>> _handlers = new();

    public void Subscribe(string eventName, Action<SimulationEvent> handler)
    {
        if (!_handlers.ContainsKey(eventName))
            _handlers[eventName] = new List<Action<SimulationEvent>>();
        _handlers[eventName].Add(handler);
    }

    public void Unsubscribe(string eventName, Action<SimulationEvent> handler)
    {
        if (_handlers.TryGetValue(eventName, out var list))
            list.Remove(handler);
    }

    public void Publish(SimulationEvent evt)
    {
        if (_handlers.TryGetValue(evt.Name, out var handlers))
        {
            foreach (var handler in handlers)
                handler(evt);
        }
    }

    public void Schedule(SimulationEvent evt) => _queue.Enqueue(evt);

    public bool ProcessNext(out SimulationEvent? outEvt)
    {
        if (_queue.TryDequeue(out var dequeuedEvt))
        {
            if (dequeuedEvt != null)
                Dispatch(dequeuedEvt);
            outEvt = dequeuedEvt;
            return true;
        }
        outEvt = null;
        return false;
    }

    private void Dispatch(SimulationEvent evt)
    {
        evt.Handler?.Invoke(null!);
    }
}

public sealed record StateChangeEvent
{
    public string VariableName { get; init; } = string.Empty;
    public double OldValue { get; init; }
    public double NewValue { get; init; }
    public double Time { get; init; }
}