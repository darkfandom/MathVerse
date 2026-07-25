namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Manages a single interoperability session with checkpointing and state tracking.
/// </summary>
public sealed class InteropSession
{
    private readonly ConcurrentDictionary<string, object> _checkpoints = new();
    private readonly List<string> _operationLog = new();
    private readonly object _logLock = new();

    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the timestamp when the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the current session state.
    /// </summary>
    public SessionState State { get; private set; }

    /// <summary>
    /// Gets the number of operations performed in this session.
    /// </summary>
    public int OperationCount
    {
        get { lock (_logLock) { return _operationLog.Count; } }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropSession"/> class.
    /// </summary>
    public InteropSession()
    {
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
        State = SessionState.Active;
    }

    /// <summary>
    /// Saves a checkpoint with the specified name.
    /// </summary>
    /// <param name="name">The checkpoint name.</param>
    /// <param name="state">The state to save.</param>
    public void SaveCheckpoint(string name, object state)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        _ = state ?? throw new ArgumentNullException(nameof(state));
        _checkpoints[name] = state;
        LogOperation($"Checkpoint saved: {name}");
    }

    /// <summary>
    /// Restores a checkpoint by name.
    /// </summary>
    /// <param name="name">The checkpoint name.</param>
    /// <returns>The saved state, or null if not found.</returns>
    public object? RestoreCheckpoint(string name)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        if (_checkpoints.TryGetValue(name, out var state))
        {
            LogOperation($"Checkpoint restored: {name}");
            return state;
        }
        return null;
    }

    /// <summary>
    /// Gets all checkpoint names.
    /// </summary>
    /// <returns>A collection of checkpoint names.</returns>
    public IReadOnlyCollection<string> GetCheckpointNames()
    {
        return _checkpoints.Keys.ToArray();
    }

    /// <summary>
    /// Gets the operation log for this session.
    /// </summary>
    /// <returns>A copy of the operation log.</returns>
    public IReadOnlyList<string> GetOperationLog()
    {
        lock (_logLock)
        {
            return _operationLog.ToArray();
        }
    }

    /// <summary>
    /// Closes the session.
    /// </summary>
    public void Close()
    {
        State = SessionState.Closed;
        LogOperation("Session closed");
    }

    private void LogOperation(string operation)
    {
        lock (_logLock)
        {
            _operationLog.Add($"[{DateTimeOffset.UtcNow:O}] {operation}");
        }
    }
}

/// <summary>
/// Represents the state of an interoperability session.
/// </summary>
public enum SessionState
{
    /// <summary>The session is active.</summary>
    Active,

    /// <summary>The session is suspended.</summary>
    Suspended,

    /// <summary>The session is closed.</summary>
    Closed
}
