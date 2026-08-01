using System.Collections.Concurrent;

namespace MathVerse.Desktop.Core;

public enum EventType
{
    ObjectCreated,
    ObjectDeleted,
    ObjectSelectionChanged,
    SelectionBegin,
    SelectionChanging,
    SelectionCommitted,
    SelectionCancelled,
    ObjectPropertyChanged,
    ViewportCameraChanged,
    CommandExecuted,
    UndoPerformed,
    RedoPerformed,
    WorkspaceModeChanged,
    ToolActivated,
    ToolDeactivated,
    HoveredObjectChanged,
    ActiveObjectChanged,
}

public readonly record struct EventData(
    EventType Type,
    Guid? SourceId = null,
    string? PropertyName = null,
    object? OldValue = null,
    object? NewValue = null,
    Dictionary<string, object>? Extra = null);

public sealed class EventBus
{
    private readonly ConcurrentDictionary<EventType, List<Action<EventData>>> _subscribers = new();

    public void Subscribe(EventType type, Action<EventData> handler)
    {
        _subscribers.AddOrUpdate(
            type,
            _ => [handler],
            (_, list) => { list.Add(handler); return list; });
    }

    public void Unsubscribe(EventType type, Action<EventData> handler)
    {
        if (_subscribers.TryGetValue(type, out var list))
        {
            lock (list)
            {
                list.Remove(handler);
            }
        }
    }

    public void Publish(EventData data)
    {
        if (!_subscribers.TryGetValue(data.Type, out var list))
            return;

        Action<EventData>[] handlers;
        lock (list)
        {
            handlers = list.ToArray();
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler(data);
            }
            catch
            {
            }
        }
    }

    public void Clear()
    {
        _subscribers.Clear();
    }
}
