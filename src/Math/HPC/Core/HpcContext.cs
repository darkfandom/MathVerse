namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using MathVerse.Math.HPC.Diagnostics;

/// <summary>
/// Thread-local context for HPC operations.
/// </summary>
public sealed class HpcContext
{
    [ThreadStatic]
    private static HpcContext? _current;

    private readonly ConcurrentDictionary<string, object> _symbolTable;
    private readonly ConcurrentDictionary<string, object> _profilingData;
    private readonly List<DiagnosticMessage> _diagnostics;

    /// <summary>
    /// Gets the current thread-local context.
    /// </summary>
    public static HpcContext? Current => _current;

    /// <summary>
    /// Sets the current thread-local context.
    /// </summary>
    /// <param name="context">The context to set as current.</param>
    /// <returns>The previous context.</returns>
    public static HpcContext? SetCurrent(HpcContext? context)
    {
        var previous = _current;
        _current = context;
        return previous;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HpcContext"/> class.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="options">The HPC options.</param>
    public HpcContext(Guid sessionId, HpcOptions options)
    {
        SessionId = sessionId;
        Options = options;
        _symbolTable = new ConcurrentDictionary<string, object>();
        _profilingData = new ConcurrentDictionary<string, object>();
        _diagnostics = new List<DiagnosticMessage>();
        StartTime = DateTime.UtcNow;
        Stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Gets the HPC options.
    /// </summary>
    public HpcOptions Options { get; }

    /// <summary>
    /// Gets the symbol table.
    /// </summary>
    public IReadOnlyDictionary<string, object> SymbolTable => _symbolTable;

    /// <summary>
    /// Gets the profiling data.
    /// </summary>
    public IReadOnlyDictionary<string, object> ProfilingData => _profilingData;

    /// <summary>
    /// Gets the diagnostics collected during the operation.
    /// </summary>
    public IReadOnlyList<DiagnosticMessage> Diagnostics => _diagnostics;

    /// <summary>
    /// Gets the start time.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets the stopwatch.
    /// </summary>
    public Stopwatch Stopwatch { get; }

    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    public TimeSpan Elapsed => Stopwatch.Elapsed;

    /// <summary>
    /// Adds a symbol to the symbol table.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    /// <param name="value">The symbol value.</param>
    /// <returns>The previous value if the key existed; otherwise, null.</returns>
    public object? AddSymbol(string name, object value) => _symbolTable.AddOrUpdate(name, value, (_, _) => value);

    /// <summary>
    /// Tries to get a symbol from the symbol table.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    /// <param name="value">The symbol value.</param>
    /// <returns>True if the symbol was found; otherwise, false.</returns>
    public bool TryGetSymbol(string name, out object? value) => _symbolTable.TryGetValue(name, out value);

    /// <summary>
    /// Removes a symbol from the symbol table.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    /// <returns>The removed value if found; otherwise, null.</returns>
    public object? RemoveSymbol(string name) => _symbolTable.TryRemove(name, out var value) ? value : null;

    /// <summary>
    /// Adds profiling data.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>The previous value if the key existed; otherwise, null.</returns>
    public object? AddProfilingData(string key, object value) => _profilingData.AddOrUpdate(key, value, (_, _) => value);

    /// <summary>
    /// Tries to get profiling data.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>True if the data was found; otherwise, false.</returns>
    public bool TryGetProfilingData(string key, out object? value) => _profilingData.TryGetValue(key, out value);

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to add.</param>
    public void AddDiagnostic(DiagnosticMessage diagnostic) => _diagnostics.Add(diagnostic);

    /// <summary>
    /// Adds multiple diagnostic messages.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to add.</param>
    public void AddDiagnostics(IEnumerable<DiagnosticMessage> diagnostics) => _diagnostics.AddRange(diagnostics);

    /// <summary>
    /// Clears all diagnostics.
    /// </summary>
    public void ClearDiagnostics() => _diagnostics.Clear();

    /// <summary>
    /// Creates a child context with inherited state.
    /// </summary>
    /// <returns>A new child context.</returns>
    public HpcContext CreateChild()
    {
        var child = new HpcContext(Guid.NewGuid(), Options);
        foreach (var kvp in _symbolTable)
        {
            child._symbolTable.TryAdd(kvp.Key, kvp.Value);
        }
        foreach (var kvp in _profilingData)
        {
            child._profilingData.TryAdd(kvp.Key, kvp.Value);
        }
        return child;
    }

    /// <summary>
    /// Stops the stopwatch.
    /// </summary>
    public void Stop() => Stopwatch.Stop();
}
