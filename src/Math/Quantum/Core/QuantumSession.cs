using System.Collections.Concurrent;

namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Manages a quantum computation session, including context, options, and checkpoints.
/// </summary>
public sealed class QuantumSession
{
    private readonly ConcurrentDictionary<string, object> _checkpoints;

    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the timestamp when this session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the session context.
    /// </summary>
    public QuantumContext Context { get; }

    /// <summary>
    /// Gets the session options.
    /// </summary>
    public QuantumOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumSession"/> class with default options.
    /// </summary>
    public QuantumSession()
    {
        _checkpoints = new ConcurrentDictionary<string, object>();
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
        Context = new QuantumContext();
        Options = new QuantumOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumSession"/> class with specified options.
    /// </summary>
    /// <param name="options">The quantum options for this session.</param>
    public QuantumSession(QuantumOptions options)
    {
        _checkpoints = new ConcurrentDictionary<string, object>();
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
        Context = new QuantumContext();
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Saves a checkpoint with the specified name and state.
    /// </summary>
    /// <param name="checkpointName">The name of the checkpoint.</param>
    /// <param name="state">The state to save.</param>
    public void SaveCheckpoint(string checkpointName, object state)
    {
        _checkpoints[checkpointName ?? throw new ArgumentNullException(nameof(checkpointName))] =
            state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// Restores a checkpoint by name.
    /// </summary>
    /// <param name="checkpointName">The name of the checkpoint to restore.</param>
    /// <returns>The saved state, or <c>null</c> if the checkpoint does not exist.</returns>
    public object? RestoreCheckpoint(string checkpointName)
    {
        if (_checkpoints.TryGetValue(checkpointName ?? throw new ArgumentNullException(nameof(checkpointName)), out object? state))
        {
            return state;
        }
        return null;
    }
}
