using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Commands;

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
