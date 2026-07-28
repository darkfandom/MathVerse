namespace MathVerse.Desktop.Models;

public sealed class GenericWorkspaceObject : WorkspaceObject
{
    public GenericWorkspaceObject(string typeTag, string name) : base(typeTag, name)
    {
    }

    public override IWorkspaceObject Clone()
    {
        var clone = new GenericWorkspaceObject(TypeTag, Name);
        clone.IsVisible = IsVisible;
        clone.IsLocked = IsLocked;
        clone.IsPinned = IsPinned;
        clone.IsExpanded = IsExpanded;
        clone.Category = Category;
        clone.ParentId = ParentId;
        clone.Transform = Transform;
        clone.Layer = Layer;
        clone.Owner = Owner;
        foreach (var tag in Tags)
            clone.Tags.Add(tag);
        foreach (var kvp in Metadata)
            clone.Metadata[kvp.Key] = kvp.Value;
        return clone;
    }
}
