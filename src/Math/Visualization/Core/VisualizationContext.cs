namespace MathVerse.Math.Visualization.Core;
using System.Collections.Concurrent;

/// <summary>Execution context for visualization operations.</summary>
public sealed class VisualizationContext
{
    private readonly ConcurrentDictionary<string, object> _state = new();

    public string SessionId { get; }
    public VisualizationConfiguration Configuration { get; }
    public VisualizationScene Scene { get; }
    public DateTime CreatedAt { get; }

    public VisualizationContext(VisualizationConfiguration? config = null)
    {
        SessionId = Guid.NewGuid().ToString("N");
        Configuration = config ?? VisualizationConfiguration.Default;
        Scene = new VisualizationScene();
        CreatedAt = DateTime.UtcNow;
    }

    public void SetState(string key, object value) => _state[key] = value;
    public bool TryGetState<T>(string key, out T? value) where T : class
    {
        if (_state.TryGetValue(key, out var obj) && obj is T typed) { value = typed; return true; }
        value = null;
        return false;
    }

    public void ClearState() => _state.Clear();
}
