namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Registry of all registered interop adapters and format handlers.
/// </summary>
public sealed class InteropRegistry
{
    private readonly ConcurrentDictionary<string, Func<IInteropAdapter>> _adapterFactories = new();
    private readonly ConcurrentDictionary<string, FormatDescriptor> _formats = new();

    /// <summary>
    /// Gets the number of registered formats.
    /// </summary>
    public int Count => _formats.Count;

    /// <summary>
    /// Registers a format with its adapter factory.
    /// </summary>
    /// <param name="formatId">The unique format identifier.</param>
    /// <param name="descriptor">The format descriptor.</param>
    /// <param name="factory">The factory function to create adapters.</param>
    public void Register(string formatId, FormatDescriptor descriptor, Func<IInteropAdapter> factory)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _ = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        _formats[formatId] = descriptor;
        _adapterFactories[formatId] = factory;
    }

    /// <summary>
    /// Creates an adapter for the specified format.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>The adapter instance, or null if the format is not registered.</returns>
    public IInteropAdapter? CreateAdapter(string formatId)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        if (_adapterFactories.TryGetValue(formatId, out var factory))
        {
            return factory();
        }
        return null;
    }

    /// <summary>
    /// Gets the descriptor for a registered format.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>The format descriptor, or null if not registered.</returns>
    public FormatDescriptor? GetDescriptor(string formatId)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _formats.TryGetValue(formatId, out var descriptor);
        return descriptor;
    }

    /// <summary>
    /// Gets all registered format identifiers.
    /// </summary>
    /// <returns>A collection of format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredFormats()
    {
        return _formats.Keys.ToArray();
    }

    /// <summary>
    /// Checks whether a format is registered.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>True if the format is registered.</returns>
    public bool IsRegistered(string formatId)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        return _formats.ContainsKey(formatId);
    }
}

/// <summary>
/// Describes a registered format.
/// </summary>
public sealed class FormatDescriptor
{
    /// <summary>
    /// Gets or sets the format name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extensions associated with this format.
    /// </summary>
    public IReadOnlyList<string> Extensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format version.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets a description of the format.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the capabilities of this format.
    /// </summary>
    public FormatCapabilities Capabilities { get; set; } = new();
}

/// <summary>
/// Describes the capabilities of a format.
/// </summary>
public sealed class FormatCapabilities
{
    /// <summary>
    /// Gets or sets a value indicating whether this format supports streaming.
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this format supports compression.
    /// </summary>
    public bool SupportsCompression { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this format supports async operations.
    /// </summary>
    public bool SupportsAsync { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this format supports bidirectional conversion.
    /// </summary>
    public bool Bidirectional { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum recommended payload size in bytes.
    /// </summary>
    public long MaxPayloadSize { get; set; } = 1024 * 1024 * 1024;
}
