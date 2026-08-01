using MathVerse.Desktop.Core;

namespace MathVerse.Desktop.Commands;

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
