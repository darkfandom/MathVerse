using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Core;

public sealed class SelectionManager
{
    private readonly ObjectRegistry _registry;
    private readonly EventBus _eventBus;
    private readonly List<Guid> _selected = [];
    private Guid? _primarySelection;

    public IReadOnlyList<Guid> SelectedIds => _selected.AsReadOnly();
    public Guid? PrimarySelection => _primarySelection;

    public IEnumerable<IWorkspaceObject> SelectedObjects =>
        _selected
            .Select(id => _registry.GetById(id))
            .Where(obj => obj is not null)
            .Cast<IWorkspaceObject>();

    public int Count => _selected.Count;

    public SelectionManager(ObjectRegistry registry, EventBus eventBus)
    {
        _registry = registry;
        _eventBus = eventBus;
    }

    public void Select(Guid id)
    {
        var obj = _registry.GetById(id);
        if (obj is null)
            return;

        if (_selected.Contains(id))
        {
            if (_primarySelection != id)
            {
                _primarySelection = id;
                _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged, id));
            }
            return;
        }

        _selected.Add(id);
        _primarySelection = id;
        obj.IsSelected = true;
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged, id));
    }

    public void Deselect(Guid id)
    {
        if (!_selected.Remove(id))
            return;

        var obj = _registry.GetById(id);
        if (obj is not null)
            obj.IsSelected = false;

        if (_primarySelection == id)
            _primarySelection = _selected.Count > 0 ? _selected[^1] : null;

        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged, id));
    }

    public void DeselectAll()
    {
        foreach (var id in _selected.ToList())
        {
            var obj = _registry.GetById(id);
            if (obj is not null)
                obj.IsSelected = false;
        }

        _selected.Clear();
        _primarySelection = null;
        _eventBus.Publish(new EventData(EventType.ObjectSelectionChanged));
    }

    public void ToggleSelection(Guid id)
    {
        if (_selected.Contains(id))
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
            Select(id);
    }

    public bool IsSelected(Guid id) => _selected.Contains(id);

    public void Clear()
    {
        DeselectAll();
    }
}
