namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class RuntimeContext
{
    private readonly ConcurrentDictionary<string, object> _variables = new();
    private readonly Stack<Dictionary<string, object>> _scopeStack = new();

    public T GetVariable<T>(string name)
    {
        if (_variables.TryGetValue(name, out var value) && value is T typed)
            return typed;
        throw new KeyNotFoundException($"Variable '{name}' not found or type mismatch.");
    }

    public void SetVariable<T>(string name, T value)
    {
        _variables[name] = value!;
    }

    public bool TryGetVariable<T>(string name, out T? value)
    {
        if (_variables.TryGetValue(name, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    public void PushScope()
    {
        _scopeStack.Push(new Dictionary<string, object>(_variables));
    }

    public void PopScope()
    {
        if (_scopeStack.Count == 0)
            throw new InvalidOperationException("No scope to pop.");

        var scope = _scopeStack.Pop();
        _variables.Clear();
        foreach (var kvp in scope)
            _variables[kvp.Key] = kvp.Value;
    }

    public void Clear() => _variables.Clear();
}
