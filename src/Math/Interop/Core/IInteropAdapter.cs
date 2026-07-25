namespace MathVerse.Math.Interop.Core;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the interface for all interoperability adapters.
/// Adapters handle conversion between MathVerse types and external formats.
/// </summary>
public interface IInteropAdapter
{
    /// <summary>
    /// Gets the unique identifier for this adapter.
    /// </summary>
    string AdapterId { get; }

    /// <summary>
    /// Gets the display name of this adapter.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the supported format identifiers.
    /// </summary>
    IReadOnlyList<string> SupportedFormats { get; }

    /// <summary>
    /// Gets the version of this adapter.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Serializes an object to the specified stream.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The target stream.</param>
    /// <param name="options">Serialization options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    ValueTask<InteropResult> SerializeAsync(object value, Stream stream, InteropOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes an object from the specified stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="targetType">The target type identifier.</param>
    /// <param name="options">Deserialization options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the deserialized object.</returns>
    ValueTask<InteropResult<object>> DeserializeAsync(Stream stream, string? targetType = null, InteropOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether this adapter can handle the specified format.
    /// </summary>
    /// <param name="format">The format identifier.</param>
    /// <returns>True if this adapter can handle the format.</returns>
    bool CanHandle(string format);
}
