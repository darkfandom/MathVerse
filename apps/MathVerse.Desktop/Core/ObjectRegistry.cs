using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Core;

public sealed class ObjectRegistry
{
    private readonly Dictionary<Guid, IWorkspaceObject> _objects = [];
    private readonly Dictionary<string, List<Guid>> _byType = [];
    private readonly Dictionary<Guid, List<Guid>> _parentToChildren = [];

    public event Action<ObjectRegistryEventArgs>? ObjectAdded;
    public event Action<ObjectRegistryEventArgs>? ObjectRemoved;

    public void Add(IWorkspaceObject obj)
    {
        _objects[obj.Id] = obj;

        if (!_byType.ContainsKey(obj.TypeTag))
            _byType[obj.TypeTag] = [];
        _byType[obj.TypeTag].Add(obj.Id);

        if (obj.ParentId is { } parentId)
        {
            if (!_parentToChildren.ContainsKey(parentId))
                _parentToChildren[parentId] = [];
            _parentToChildren[parentId].Add(obj.Id);
        }

        ObjectAdded?.Invoke(new ObjectRegistryEventArgs(obj.Id, obj));
    }

    public bool Remove(Guid id)
    {
        if (!_objects.Remove(id, out var obj))
            return false;

        if (_byType.TryGetValue(obj.TypeTag, out var typeList))
            typeList.Remove(id);

        if (obj.ParentId is { } parentId &&
            _parentToChildren.TryGetValue(parentId, out var siblings))
        {
            siblings.Remove(id);
        }

        _parentToChildren.Remove(id);
        ObjectRemoved?.Invoke(new ObjectRegistryEventArgs(id, obj));
        return true;
    }

    public IWorkspaceObject? GetById(Guid id) =>
        _objects.TryGetValue(id, out var obj) ? obj : null;

    public IEnumerable<IWorkspaceObject> GetByType(string typeTag)
    {
        if (!_byType.TryGetValue(typeTag, out var ids))
            return [];
        return ids
            .Select(id => _objects.TryGetValue(id, out var obj) ? obj : null)
            .Where(obj => obj is not null)
            .Cast<IWorkspaceObject>();
    }

    public IEnumerable<IWorkspaceObject> GetByTag(string tag) =>
        _objects.Values.Where(o => o.Tags.Contains(tag));

    public IEnumerable<IWorkspaceObject> GetAll() => _objects.Values;

    public IEnumerable<IWorkspaceObject> GetVisible() =>
        _objects.Values.Where(o => o.IsVisible);

    public IEnumerable<IWorkspaceObject> GetSelected() =>
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

    public bool Contains(Guid id) => _objects.ContainsKey(id);

    public int Count => _objects.Count;

    public int CountByType(string typeTag) =>
        _byType.TryGetValue(typeTag, out var ids) ? ids.Count : 0;

    public void Clear()
    {
        var ids = _objects.Keys.ToList();
        foreach (var id in ids)
            Remove(id);
    }

    public IReadOnlyCollection<string> TypeTags => _byType.Keys.ToList().AsReadOnly();
}

public readonly record struct ObjectRegistryEventArgs(Guid ObjectId, IWorkspaceObject Object);
