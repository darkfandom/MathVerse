namespace MathVerse.Math.Interop.Performance;

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base class for streaming serializers that minimize allocations.
/// </summary>
public abstract class StreamingSerializer
{
    /// <summary>
    /// Gets the format identifier for this serializer.
    /// </summary>
    public abstract string Format { get; }

    /// <summary>
    /// Serializes data to a stream using pooled buffers.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The target stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of bytes written.</returns>
    public abstract ValueTask<long> SerializeAsync(object value, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes data from a stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized object.</returns>
    public abstract ValueTask<object?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a chunk of data to the stream using a pooled buffer.
    /// </summary>
    /// <param name="stream">The target stream.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    protected static async ValueTask WriteChunkAsync(Stream stream, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }
}
