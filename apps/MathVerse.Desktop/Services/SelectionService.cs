using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Services;

public sealed class SelectionService
{
    private readonly ObjectRegistry _registry;
    private readonly EventBus _eventBus;
    private readonly HashSet<Guid> _selectedIds = [];
    private readonly Dictionary<Guid, IWorkspaceObject> _selectedObjects = [];
    private IWorkspaceObject? _activeObject;
    private IWorkspaceObject? _hoveredObject;

    public IReadOnlySet<Guid> SelectedIds => _selectedIds;
    public Guid? ActiveObjectId => _activeObject?.Id;
    public Guid? HoveredObjectId => _hoveredObject?.Id;
    public int Count => _selectedIds.Count;
    public bool HasSelection => _selectedIds.Count > 0;
    public IEnumerable<IWorkspaceObject> SelectedObjects => _selectedObjects.Values;
    public IWorkspaceObject? ActiveObject => _activeObject;
    public IWorkspaceObject? HoveredObject => _hoveredObject;

    public SelectionService(ObjectRegistry registry, EventBus eventBus)
    {
        _registry = registry;
        _eventBus = eventBus;
    }

    public void Select(Guid id)
    {
        var obj = _registry.GetById(id);
        if (obj is null) return;

        if (_selectedIds.Contains(id))
        {
            if (_activeObject?.Id != id)
            {
                _activeObject = obj;
                _eventBus.Publish(new EventData(EventType.ActiveObjectChanged, id));
            }
            return;
        }

        _selectedIds.Add(id);
        _selectedObjects[id] = obj;
        _activeObject = obj;
        obj.IsSelected = true;
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged, id));
        _eventBus.Publish(new EventData(EventType.ActiveObjectChanged, id));
    }

    public void Deselect(Guid id)
    {
        if (!_selectedIds.Remove(id)) return;

        _selectedObjects.Remove(id);

        var obj = _registry.GetById(id);
        if (obj is not null) obj.IsSelected = false;

        if (_activeObject?.Id == id)
            _activeObject = _selectedObjects.Count > 0 ? _selectedObjects.Values.Last() : null;

        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged, id));
    }

    public void DeselectAll()
    {
        if (_selectedIds.Count == 0) return;

        foreach (var obj in _selectedObjects.Values.ToList())
            obj.IsSelected = false;

        _selectedIds.Clear();
        _selectedObjects.Clear();
        _activeObject = null;
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged));
    }

    public void ToggleSelection(Guid id)
    {
        if (_selectedIds.Contains(id))
            Deselect(id);
        else
            Select(id);
    }

    public void SetSelection(Guid id)
    {
        DeselectAll();
        Select(id);
    }

    public void SetSelectionRange(IEnumerable<Guid> ids)
    {
        DeselectAll();
        foreach (var id in ids)
        {
            var obj = _registry.GetById(id);
            if (obj is null) continue;
            _selectedIds.Add(id);
            _selectedObjects[id] = obj;
            obj.IsSelected = true;
        }
        _activeObject = _selectedObjects.Count > 0 ? _selectedObjects.Values.Last() : null;
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged));
    }

    public void SelectAll()
    {
        var all = _registry.GetAll().ToList();
        if (all.Count == 0) return;

        foreach (var obj in all)
        {
            _selectedIds.Add(obj.Id);
            _selectedObjects[obj.Id] = obj;
            obj.IsSelected = true;
        }
        _activeObject = _selectedObjects.Values.Last();
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged));
    }

    public bool IsSelected(Guid id) => _selectedIds.Contains(id);

    public void SetActive(Guid id)
    {
        if (_activeObject?.Id == id) return;
        var obj = _registry.GetById(id);
        if (obj is null) return;
        _activeObject = obj;
        _eventBus.Publish(new EventData(EventType.ActiveObjectChanged, id));
    }

    public event Action<Guid?>? HoveredChanged;

    public void SetHovered(Guid id)
    {
        if (_hoveredObject?.Id == id) return;
        _hoveredObject = _registry.GetById(id);
        _eventBus.Publish(new EventData(EventType.HoveredObjectChanged, id));
        HoveredChanged?.Invoke(id);
    }

    public void ClearHovered()
    {
        if (_hoveredObject is null) return;
        _hoveredObject = null;
        _eventBus.Publish(new EventData(EventType.HoveredObjectChanged));
        HoveredChanged?.Invoke(null);
    }

    public void Clear()
    {
        DeselectAll();
        _activeObject = null;
        ClearHovered();
    }

}
