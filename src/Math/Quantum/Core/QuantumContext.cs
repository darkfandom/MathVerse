using System.Collections.Concurrent;

namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Provides a session context for quantum operations, storing arbitrary properties in a thread-safe manner.
/// </summary>
public sealed class QuantumContext
{
    private readonly ConcurrentDictionary<string, object> _properties;

    /// <summary>
    /// Gets the unique identifier for this context session.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the collection of properties stored in this context.
    /// </summary>
    public ConcurrentDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumContext"/> class.
    /// </summary>
    public QuantumContext()
    {
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
        _properties = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumContext"/> class with a specified session identifier.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    public QuantumContext(string sessionId)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        CreatedAt = DateTimeOffset.UtcNow;
        _properties = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Sets a property value in the context.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    public void SetProperty(string key, object value)
    {
        _properties[key ?? throw new ArgumentNullException(nameof(key))] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets a property value from the context.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
    public object GetProperty(string key)
    {
        if (_properties.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out object? value))
        {
            return value;
        }
        throw new KeyNotFoundException($"Property '{key}' not found in context.");
    }

    /// <summary>
    /// Attempts to get a property value from the context.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">When this method returns, contains the property value if found; otherwise, the default value.</param>
    /// <returns><c>true</c> if the property was found; otherwise, <c>false</c>.</returns>
    public bool TryGetProperty(string key, out object? value)
    {
        return _properties.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out value);
    }
}
