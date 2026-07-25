namespace MathVerse.Math.Visualization.Core;
using System.Collections.Concurrent;

/// <summary>Scene containing all visualization objects.</summary>
public sealed class VisualizationScene
{
    private readonly ConcurrentDictionary<string, VisualizationObject> _objects = new();
    private readonly ConcurrentBag<string> _dirtyObjects = [];

    public string SceneId { get; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; }
    public int ObjectCount => _objects.Count;
    public List<VisualizationObject> Objects
    {
        get => _objects.Values.ToList();
        set
        {
            _objects.Clear();
            foreach (var obj in value)
                _objects[obj.Id] = obj;
        }
    }

    public VisualizationScene() { SceneId = Guid.NewGuid().ToString("N"); CreatedAt = DateTime.UtcNow; }

    public string AddObject(VisualizationObject obj) { _objects[obj.Id] = obj; return obj.Id; }
    public bool RemoveObject(string id) => _objects.TryRemove(id, out _);
    public VisualizationObject? GetObject(string id) => _objects.TryGetValue(id, out var obj) ? obj : null;
    public IReadOnlyCollection<VisualizationObject> GetAllObjects() => _objects.Values.ToArray();
    public void MarkDirty(string objectId) { _dirtyObjects.Add(objectId); }
    public List<string> GetDirtyObjects() => _dirtyObjects.ToArray().Distinct().ToList();
    public void ClearDirty() { while (_dirtyObjects.TryTake(out _)) { } }
    public void Clear() { _objects.Clear(); ClearDirty(); }
}
