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
