namespace MathVerse.Math.Visualization.Core;
using System.Collections.Concurrent;

/// <summary>Registry of visualization renderers for different object types.</summary>
public sealed class VisualizationRegistry
{
    private readonly ConcurrentDictionary<string, Func<VisualizationObject, IReadOnlyDictionary<string, object>>> _renderers = new();

    public void RegisterRenderer(string objectType, Func<VisualizationObject, IReadOnlyDictionary<string, object>> renderer) => _renderers[objectType] = renderer;
    public IReadOnlyDictionary<string, object>? RenderObject(string objectType, VisualizationObject obj) => _renderers.TryGetValue(objectType, out var r) ? r(obj) : null;
    public bool HasRenderer(string objectType) => _renderers.ContainsKey(objectType);
    public static VisualizationRegistry CreateDefault() => new();
}
