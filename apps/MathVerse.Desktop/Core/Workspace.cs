using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Core;

public sealed class Workspace
{
    private readonly List<Document> _documents = [];
    private readonly Dictionary<Guid, IWorkspaceObject> _objects = [];
    private readonly Dictionary<Guid, List<Guid>> _parentToChildren = [];

    public Document ActiveDocument { get; private set; }
    public IReadOnlyList<Document> Documents => _documents.AsReadOnly();
    public IReadOnlyDictionary<Guid, IWorkspaceObject> Objects => _objects;

    public event Action<WorkspaceObjectEventArgs>? ObjectAdded;
    public event Action<WorkspaceObjectEventArgs>? ObjectRemoved;

    public Workspace()
    {
        ActiveDocument = new Document { Name = "Untitled" };
        _documents.Add(ActiveDocument);
    }

    public void AddObject(IWorkspaceObject obj)
    {
        _objects[obj.Id] = obj;
        ActiveDocument.Scene.Objects.Add(obj.Id);

        if (obj.ParentId is { } parentId)
        {
            if (!_parentToChildren.ContainsKey(parentId))
                _parentToChildren[parentId] = [];
            _parentToChildren[parentId].Add(obj.Id);
        }

        ObjectAdded?.Invoke(new WorkspaceObjectEventArgs(obj.Id, obj));
    }

    public bool RemoveObject(Guid id)
    {
        if (!_objects.Remove(id, out var obj))
            return false;

        ActiveDocument.Scene.Objects.Remove(id);

        if (obj.ParentId is { } parentId &&
            _parentToChildren.TryGetValue(parentId, out var siblings))
        {
            siblings.Remove(id);
        }

        _parentToChildren.Remove(id);
        ObjectRemoved?.Invoke(new WorkspaceObjectEventArgs(id, obj));
        return true;
    }

    public IWorkspaceObject? GetObject(Guid id) =>
        _objects.TryGetValue(id, out var obj) ? obj : null;

    public IEnumerable<IWorkspaceObject> GetObjectsByType(string typeTag) =>
        _objects.Values.Where(o => o.TypeTag == typeTag);

    public IEnumerable<IWorkspaceObject> GetObjectsByTag(string tag) =>
        _objects.Values.Where(o => o.Tags.Contains(tag));

    public IEnumerable<IWorkspaceObject> GetVisibleObjects() =>
        _objects.Values.Where(o => o.IsVisible);

    public IEnumerable<IWorkspaceObject> GetSelectedObjects() =>
        _objects.Values.Where(o => o.IsSelected);

    public IEnumerable<IWorkspaceObject> GetChildren(Guid parentId)
    {
        if (!_parentToChildren.TryGetValue(parentId, out var children))
            return [];
        return children
            .Select(id => _objects.TryGetValue(id, out var obj) ? obj : null)
            .Where(obj => obj is not null)
            .Cast<IWorkspaceObject>();
    }

    public int ObjectCount => _objects.Count;

    public int ObjectCountByType(string typeTag) =>
        _objects.Values.Count(o => o.TypeTag == typeTag);

    public void Clear()
    {
        var ids = _objects.Keys.ToList();
        foreach (var id in ids)
            RemoveObject(id);
    }
}

public readonly record struct WorkspaceObjectEventArgs(Guid ObjectId, IWorkspaceObject Object);
