using MathVerse.Desktop.Core;

namespace MathVerse.Desktop.Commands;

public sealed class SelectObjectCommand : ICommand
{
    public string Name => "SelectObject";

    public bool CanExecute(CommandContext context) =>
        context.Parameters?.ContainsKey("ObjectId") == true ||
        context.Parameters?.ContainsKey("UndoData") == true ||
        context.Parameters?.ContainsKey("RedoData") == true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var id = (Guid)context.Parameters!["ObjectId"]!;
        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.SetSelection(id);
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}

public sealed class ToggleSelectObjectCommand : ICommand
{
    public string Name => "ToggleSelectObject";

    public bool CanExecute(CommandContext context) =>
        context.Parameters?.ContainsKey("ObjectId") == true ||
        context.Parameters?.ContainsKey("UndoData") == true ||
        context.Parameters?.ContainsKey("RedoData") == true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var id = (Guid)context.Parameters!["ObjectId"]!;
        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.ToggleSelection(id);
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}

public sealed class DeselectCommand : ICommand
{
    public string Name => "Deselect";

    public bool CanExecute(CommandContext context) =>
        context.Selection.HasSelection ||
        context.Parameters?.ContainsKey("UndoData") == true ||
        context.Parameters?.ContainsKey("RedoData") == true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.DeselectAll();
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}

public sealed class ClearSelectionCommand : ICommand
{
    public string Name => "ClearSelection";

    public bool CanExecute(CommandContext context) => true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.DeselectAll();
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}

public sealed class BoxSelectCommand : ICommand
{
    public string Name => "BoxSelect";

    public bool CanExecute(CommandContext context) =>
        context.Parameters?.ContainsKey("ObjectIds") == true ||
        context.Parameters?.ContainsKey("UndoData") == true ||
        context.Parameters?.ContainsKey("RedoData") == true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var ids = (IEnumerable<Guid>)context.Parameters!["ObjectIds"]!;
        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.SetSelectionRange(ids);
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}

public sealed class SelectAllCommand : ICommand
{
    public string Name => "SelectAll";

    public bool CanExecute(CommandContext context) => true;

    public object? Execute(CommandContext context)
    {
        if (TryUndoRedo(context, out var result)) return result;

        var previous = new HashSet<Guid>(context.Selection.SelectedIds);
        context.Selection.SelectAll();
        return previous;
    }

    public object? GetUndoData(CommandContext context) =>
        new HashSet<Guid>(context.Selection.SelectedIds);

    private static bool TryUndoRedo(CommandContext context, out object? result)
    {
        result = null;
        if (context.Parameters is null) return false;
        if (context.Parameters.TryGetValue("UndoData", out var undo) && undo is HashSet<Guid> snap)
        { RestoreSelection(context, snap); return true; }
        if (context.Parameters.TryGetValue("RedoData", out var redo) && redo is HashSet<Guid> rsnap)
        { RestoreSelection(context, rsnap); return true; }
        return false;
    }

    private static void RestoreSelection(CommandContext context, HashSet<Guid> snapshot)
    {
        context.Selection.DeselectAll();
        foreach (var id in snapshot)
            if (context.Registry.GetById(id) is not null)
                context.Selection.Select(id);
    }
}
