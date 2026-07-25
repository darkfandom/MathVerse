namespace MathVerse.Math.Interop.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Thread-safe registry for format configurations.
/// </summary>
public sealed class FormatRegistry
{
    private readonly ConcurrentDictionary<string, FormatRegistration> _registrations = new();

    /// <summary>
    /// Gets the number of registered formats.
    /// </summary>
    public int Count => _registrations.Count;

    /// <summary>
    /// Registers a format with its configuration.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <param name="registration">The format registration.</param>
    public void Register(string formatId, FormatRegistration registration)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _ = registration ?? throw new ArgumentNullException(nameof(registration));
        _registrations[formatId] = registration;
    }

    /// <summary>
    /// Gets a format registration by identifier.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>The format registration, or null if not found.</returns>
    public FormatRegistration? GetRegistration(string formatId)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _registrations.TryGetValue(formatId, out var registration);
        return registration;
    }

    /// <summary>
    /// Gets all registered format identifiers.
    /// </summary>
    /// <returns>A collection of format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredFormatIds()
    {
        return _registrations.Keys.ToArray();
    }

    /// <summary>
    /// Unregisters a format.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>True if the format was unregistered.</returns>
    public bool Unregister(string formatId)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        return _registrations.TryRemove(formatId, out _);
    }
}

/// <summary>
/// Represents a format registration entry.
/// </summary>
public sealed class FormatRegistration
{
    /// <summary>
    /// Gets or sets the format name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the supported file extensions.
    /// </summary>
    public IReadOnlyList<string> Extensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the adapter configuration.
    /// </summary>
    public AdapterConfiguration AdapterConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the serialization configuration.
    /// </summary>
    public SerializationConfiguration SerializationConfig { get; set; } = new();
}
