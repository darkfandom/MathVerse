using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MathVerse.Desktop.Models;

public abstract class WorkspaceObject : IWorkspaceObject, INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _icon = string.Empty;
    private bool _isVisible = true;
    private bool _isLocked;
    private bool _isPinned;
    private bool _isSelected;
    private bool _isExpanded = true;
    private string _category = string.Empty;
    private Guid? _parentId;
    private Matrix4x4 _transform = Matrix4x4.Identity;
    private BoundingBox? _boundingBox;
    private int _layer;
    private DateTime _modifiedAt;

    public Guid Id { get; } = Guid.NewGuid();
    public string TypeTag { get; }
    public List<string> Tags { get; } = [];
    public List<Guid> Children { get; } = [];
    public Dictionary<string, object> Metadata { get; } = [];
    public Guid? Owner { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public bool IsPinned
    {
        get => _isPinned;
        set => SetProperty(ref _isPinned, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public Guid? ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }

    public Matrix4x4 Transform
    {
        get => _transform;
        set => SetProperty(ref _transform, value);
    }

    public BoundingBox? BoundingBox
    {
        get => _boundingBox;
        set => SetProperty(ref _boundingBox, value);
    }

    public int Layer
    {
        get => _layer;
        set => SetProperty(ref _layer, value);
    }

    public DateTime ModifiedAt
    {
        get => _modifiedAt;
        set => SetProperty(ref _modifiedAt, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected WorkspaceObject(string typeTag, string name)
    {
        TypeTag = typeTag;
        _name = name;
        _modifiedAt = CreatedAt;
    }

    public virtual IWorkspaceObject Clone()
    {
        var clone = (WorkspaceObject)MemberwiseClone();
        clone.Id.GetType().GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(clone, Guid.NewGuid());
        clone._name = _name;
        clone._modifiedAt = DateTime.UtcNow;
        clone.Tags.Clear();
        clone.Tags.AddRange(Tags);
        clone.Metadata.Clear();
        foreach (var kvp in Metadata)
            clone.Metadata[kvp.Key] = kvp.Value;
        clone.Children.Clear();
        return clone;
    }

    public virtual byte[] Serialize() =>
        JsonSerializer.SerializeToUtf8Bytes(this);

    public virtual void Destroy()
    {
        PropertyChanged = null;
    }

    public virtual IWorkspaceObject Duplicate()
    {
        var dup = Clone();
        dup.Name = $"{Name} (Copy)";
        return dup;
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        ModifiedAt = DateTime.UtcNow;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
