using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Commands;

public sealed class CreateObjectCommand : ICommand
{
    public string Name => "CreateObject";

    public bool CanExecute(CommandContext context) => true;

    public object? Execute(CommandContext context)
    {
        var typeTag = context.Parameters?.TryGetValue("TypeTag", out var t) == true ? t as string : null;
        var name = context.Parameters?.TryGetValue("Name", out var n) == true ? n as string : null;

        if (typeTag is null)
            return null;

        var obj = new GenericWorkspaceObject(typeTag, name ?? typeTag);
        context.Registry.Add(obj);
        context.Workspace.AddObject(obj);
        return obj.Id;
    }

    public object? GetUndoData(CommandContext context) => null;
}

public sealed class DeleteObjectCommand : ICommand
{
    public string Name => "DeleteObject";

    public bool CanExecute(CommandContext context) => context.SelectedIds.Count > 0;

    public object? Execute(CommandContext context)
    {
        var ids = context.SelectedIds.ToList();
        foreach (var id in ids)
        {
            context.Registry.Remove(id);
            context.Workspace.RemoveObject(id);
        }
        context.Selection.DeselectAll();
        return ids;
    }

    public object? GetUndoData(CommandContext context) => null;
}

public sealed class SetObjectPropertyCommand : ICommand
{
    public string Name => "SetObjectProperty";

    public bool CanExecute(CommandContext context) =>
        context.SelectedIds.Count > 0 &&
        context.Parameters?.ContainsKey("PropertyName") == true &&
        context.Parameters?.ContainsKey("Value") == true;

    public object? Execute(CommandContext context)
    {
        var id = context.SelectedIds[0];
        var obj = context.Registry.GetById(id);
        if (obj is null) return null;

        var propName = context.Parameters!["PropertyName"] as string;
        var value = context.Parameters["Value"];
        var oldValue = GetProperty(obj, propName!);

        SetProperty(obj, propName!, value);
        context.EventBus.Publish(new EventData(
            EventType.ObjectPropertyChanged, id, propName, oldValue, value));
        return oldValue;
    }

    public object? GetUndoData(CommandContext context)
    {
        var id = context.SelectedIds.Count > 0 ? context.SelectedIds[0] : (Guid?)null;
        if (id is null) return null;
        var obj = context.Registry.GetById(id.Value);
        if (obj is null) return null;

        var propName = context.Parameters?["PropertyName"] as string;
        return GetProperty(obj, propName!);
    }

    private static object? GetProperty(IWorkspaceObject obj, string name) => name switch
    {
        "Name" => obj.Name,
        "IsVisible" => obj.IsVisible,
        "IsLocked" => obj.IsLocked,
        "IsPinned" => obj.IsPinned,
        "Category" => obj.Category,
        "Layer" => obj.Layer,
        _ => null
    };

    private static void SetProperty(IWorkspaceObject obj, string name, object? value)
    {
        switch (name)
        {
            case "Name" when value is string s:
                obj.Name = s;
                break;
            case "IsVisible" when value is bool b:
                obj.IsVisible = b;
                break;
            case "IsLocked" when value is bool b:
                obj.IsLocked = b;
                break;
            case "IsPinned" when value is bool b:
                obj.IsPinned = b;
                break;
            case "Category" when value is string s:
                obj.Category = s;
                break;
            case "Layer" when value is int i:
                obj.Layer = i;
                break;
        }
    }
}

public sealed class SetObjectVisibilityCommand : ICommand
{
    public string Name => "SetObjectVisibility";

    public bool CanExecute(CommandContext context) =>
        context.SelectedIds.Count > 0 &&
        context.Parameters?.ContainsKey("Visible") == true;

    public object? Execute(CommandContext context)
    {
        var visible = (bool)context.Parameters!["Visible"]!;
        var changed = new List<Guid>();

        foreach (var id in context.SelectedIds)
        {
            var obj = context.Registry.GetById(id);
            if (obj is null) continue;
            if (obj.IsVisible != visible)
            {
                obj.IsVisible = visible;
                changed.Add(id);
                context.EventBus.Publish(new EventData(
                    EventType.ObjectPropertyChanged, id, "IsVisible", !visible, visible));
            }
        }

        return changed;
    }

    public object? GetUndoData(CommandContext context) =>
        !context.Parameters?.ContainsKey("Visible") == true ? null : !(bool)context.Parameters!["Visible"]!;
}

public sealed class RenameObjectCommand : ICommand
{
    public string Name => "RenameObject";

    public bool CanExecute(CommandContext context) =>
        context.SelectedIds.Count > 0 &&
        context.Parameters?.ContainsKey("Name") == true;

    public object? Execute(CommandContext context)
    {
        var id = context.SelectedIds[0];
        var obj = context.Registry.GetById(id);
        if (obj is null) return null;

        var oldName = obj.Name;
        var newName = (string)context.Parameters!["Name"]!;
        obj.Name = newName;

        context.EventBus.Publish(new EventData(
            EventType.ObjectPropertyChanged, id, "Name", oldName, newName));
        return oldName;
    }

    public object? GetUndoData(CommandContext context)
    {
        var id = context.SelectedIds.Count > 0 ? context.SelectedIds[0] : (Guid?)null;
        if (id is null) return null;
        var obj = context.Registry.GetById(id.Value);
        return obj?.Name;
    }
}
