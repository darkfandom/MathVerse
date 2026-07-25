namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.Configuration;
using MathVerse.Math.Compiler.IR;

public sealed class CompilerContext
{
    [ThreadStatic]
    private static CompilerContext? _threadLocalInstance;

    private readonly Dictionary<string, IRValue> _symbolTable = new();
    private readonly Stack<Dictionary<string, IRValue>> _scopeStack = new();
    private int _tempCounter;

    public string CurrentMethodName { get; set; } = string.Empty;

    public IRType CurrentReturnType { get; set; } = IRType.Void;

    public CompilationTargetType CurrentTarget { get; set; } = CompilationTargetType.Generic;

    public bool IsInLoop { get; set; }

    public int ExpressionDepth { get; set; }

    public static CompilerContext Current
    {
        get
        {
            _threadLocalInstance ??= new CompilerContext();
            return _threadLocalInstance;
        }
    }

    public void DefineSymbol(string name, IRValue value)
    {
        _symbolTable[name] = value;
    }

    public IRValue? LookupSymbol(string name)
    {
        return _symbolTable.TryGetValue(name, out var value) ? value : null;
    }

    public void PushScope()
    {
        _scopeStack.Push(new Dictionary<string, IRValue>(_symbolTable));
    }

    public void PopScope()
    {
        if (_scopeStack.Count == 0)
            throw new InvalidOperationException("No scope to pop.");
        var scope = _scopeStack.Pop();
        _symbolTable.Clear();
        foreach (var kvp in scope)
            _symbolTable[kvp.Key] = kvp.Value;
    }

    public string NextTempName() => $"%t{_tempCounter++}";

    public void Reset()
    {
        _symbolTable.Clear();
        _scopeStack.Clear();
        _tempCounter = 0;
        CurrentMethodName = string.Empty;
        CurrentReturnType = IRType.Void;
        CurrentTarget = CompilationTargetType.Generic;
        IsInLoop = false;
        ExpressionDepth = 0;
    }
}
