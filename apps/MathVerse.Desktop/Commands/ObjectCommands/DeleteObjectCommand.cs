using MathVerse.Desktop.Core;

namespace MathVerse.Desktop.Commands;

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
