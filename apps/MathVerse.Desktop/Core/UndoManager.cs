namespace MathVerse.Desktop.Core;

public sealed class UndoTransaction
{
    public string Name { get; }
    public List<UndoOperation> Operations { get; } = [];
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public UndoTransaction(string name)
    {
        Name = name;
    }

    public void AddOperation(string commandName, object? undoData)
    {
        Operations.Add(new UndoOperation(commandName, undoData));
    }
}

public readonly record struct UndoOperation(string CommandName, object? UndoData);

public sealed class UndoManager
{
    private readonly EventBus _eventBus;
    private readonly CommandManager _commandManager;
    private readonly LinkedList<UndoTransaction> _undoStack = new();
    private readonly LinkedList<UndoTransaction> _redoStack = new();
    private UndoTransaction? _currentTransaction;
    private int _gestureDepth;

    public const int MaxDepth = 500;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    public UndoManager(EventBus eventBus, CommandManager commandManager)
    {
        _eventBus = eventBus;
        _commandManager = commandManager;
    }

    public IDisposable BeginTransaction(string name)
    {
        _gestureDepth++;
        if (_currentTransaction is null)
            _currentTransaction = new UndoTransaction(name);
        return new TransactionScope(this);
    }

    public void AddOperation(string commandName, object? undoData)
    {
        _currentTransaction?.AddOperation(commandName, undoData);
    }

    public bool Undo()
    {
        if (_undoStack.Count == 0)
            return false;

        var transaction = _undoStack.Last!.Value;
        _undoStack.RemoveLast();

        foreach (var op in Enumerable.Reverse(transaction.Operations))
        {
            _commandManager.Execute(op.CommandName, new Dictionary<string, object>
            {
                ["UndoData"] = op.UndoData ?? new object()
            });
        }

        _redoStack.AddLast(transaction);
        PruneStacks();
        _eventBus.Publish(new EventData(EventType.UndoPerformed));
        return true;
    }

    public bool Redo()
    {
        if (_redoStack.Count == 0)
            return false;

        var transaction = _redoStack.Last!.Value;
        _redoStack.RemoveLast();

        foreach (var op in transaction.Operations)
        {
            _commandManager.Execute(op.CommandName, new Dictionary<string, object>
            {
                ["RedoData"] = op.UndoData ?? new object()
            });
        }

        _undoStack.AddLast(transaction);
        PruneStacks();
        _eventBus.Publish(new EventData(EventType.RedoPerformed));
        return true;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _currentTransaction = null;
        _gestureDepth = 0;
    }

    private void EndTransaction()
    {
        if (_currentTransaction is null)
            return;

        _redoStack.Clear();
        _undoStack.AddLast(_currentTransaction);
        _currentTransaction = null;
        PruneStacks();
    }

    private void PruneStacks()
    {
        while (_undoStack.Count > MaxDepth)
            _undoStack.RemoveFirst();
    }

    private sealed class TransactionScope : IDisposable
    {
        private readonly UndoManager _manager;
        public TransactionScope(UndoManager manager) => _manager = manager;
        public void Dispose()
        {
            _manager._gestureDepth--;
            if (_manager._gestureDepth == 0)
                _manager.EndTransaction();
        }
    }
}
