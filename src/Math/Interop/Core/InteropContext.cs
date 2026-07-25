namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Provides context for interoperability operations, including registered adapters and session state.
/// </summary>
public sealed class InteropContext
{
    private readonly ConcurrentDictionary<string, object> _properties = new();
    private readonly ConcurrentDictionary<string, IInteropAdapter> _adapters = new();

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the properties dictionary for this context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropContext"/> class.
    /// </summary>
    public InteropContext()
    {
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Registers an adapter for a specific format or protocol.
    /// </summary>
    /// <param name="format">The format identifier.</param>
    /// <param name="adapter">The adapter instance.</param>
    public void RegisterAdapter(string format, IInteropAdapter adapter)
    {
        _ = format ?? throw new ArgumentNullException(nameof(format));
        _ = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _adapters[format] = adapter;
    }

    /// <summary>
    /// Gets a registered adapter by format identifier.
    /// </summary>
    /// <param name="format">The format identifier.</param>
    /// <returns>The adapter, or null if not found.</returns>
    public IInteropAdapter? GetAdapter(string format)
    {
        _ = format ?? throw new ArgumentNullException(nameof(format));
        _adapters.TryGetValue(format, out var adapter);
        return adapter;
    }

    /// <summary>
    /// Gets all registered adapter format identifiers.
    /// </summary>
    /// <returns>A collection of registered format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredFormats()
    {
        return _adapters.Keys.ToArray();
    }

    /// <summary>
    /// Sets a context property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    public void SetProperty(string key, object value)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));
        _ = value ?? throw new ArgumentNullException(nameof(value));
        _properties[key] = value;
    }

    /// <summary>
    /// Gets a context property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The property value, or null if not found.</returns>
    public object? GetProperty(string key)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));
        _properties.TryGetValue(key, out var value);
        return value;
    }
}
