using MathVerse.Desktop.Models;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop.Core;

public interface ICommand
{
    string Name { get; }
    bool CanExecute(CommandContext context);
    object? Execute(CommandContext context);
    object? GetUndoData(CommandContext context);
}

public readonly record struct CommandContext(
    Workspace Workspace,
    ObjectRegistry Registry,
    EventBus EventBus,
    SelectionService Selection,
    IReadOnlyList<Guid> SelectedIds,
    Guid? ActiveToolId,
    Dictionary<string, object>? Parameters = null);

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = [];

    public void Register(ICommand command)
    {
        _commands[command.Name] = command;
    }

    public ICommand? Get(string name) =>
        _commands.TryGetValue(name, out var cmd) ? cmd : null;

    public IEnumerable<ICommand> All => _commands.Values;

    public bool Contains(string name) => _commands.ContainsKey(name);
}

public sealed class CommandManager
{
    private readonly CommandRegistry _registry;
    private readonly Workspace _workspace;
    private readonly ObjectRegistry _objectRegistry;
    private readonly EventBus _eventBus;
    private readonly SelectionService _selection;
    private readonly List<CommandRecord> _history = [];

    public IReadOnlyList<CommandRecord> History => _history.AsReadOnly();
    public int ExecutedCommandCount => _history.Count;

    public CommandManager(
        CommandRegistry registry,
        Workspace workspace,
        ObjectRegistry objectRegistry,
        EventBus eventBus,
        SelectionService selection)
    {
        _registry = registry;
        _workspace = workspace;
        _objectRegistry = objectRegistry;
        _eventBus = eventBus;
        _selection = selection;
    }

    public bool Execute(string commandName, Dictionary<string, object>? parameters = null)
    {
        var cmd = _registry.Get(commandName);
        if (cmd is null)
            return false;

        var context = BuildContext(parameters);
        if (!cmd.CanExecute(context))
            return false;

        var result = cmd.Execute(context);
        var undoData = cmd.GetUndoData(context);

        _history.Add(new CommandRecord(commandName, parameters, undoData, DateTime.UtcNow));
        _eventBus.Publish(new EventData(EventType.CommandExecuted, null, commandName, null, result));
        return true;
    }

    private CommandContext BuildContext(Dictionary<string, object>? parameters)
    {
        return new CommandContext(
            _workspace,
            _objectRegistry,
            _eventBus,
            _selection,
            _selection.SelectedIds.ToList(),
            null,
            parameters);
    }
}

public readonly record struct CommandRecord(
    string CommandName,
    Dictionary<string, object>? Parameters,
    object? UndoData,
    DateTime Timestamp);
