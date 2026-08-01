using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Commands;

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
