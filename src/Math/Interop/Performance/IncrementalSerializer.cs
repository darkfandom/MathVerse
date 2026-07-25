namespace MathVerse.Math.Interop.Performance;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Supports incremental serialization, allowing large objects to be serialized in chunks.
/// </summary>
public sealed class IncrementalSerializer
{
    private readonly StreamingSerializer _serializer;

    /// <summary>
    /// Gets or sets the chunk size in bytes for incremental operations.
    /// </summary>
    public int ChunkSize { get; set; } = 65536;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncrementalSerializer"/> class.
    /// </summary>
    /// <param name="serializer">The underlying streaming serializer.</param>
    public IncrementalSerializer(StreamingSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Serializes an object incrementally, flushing chunks to the stream.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The target stream.</param>
    /// <param name="progress">Optional progress callback with bytes written.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total number of bytes written.</returns>
    public async ValueTask<long> SerializeIncrementalAsync(object value, Stream stream, Action<long>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        using var buffer = new ZeroCopyBuffer(ChunkSize);
        long totalBytes = 0;

        await using var ms = new MemoryStream();
        var bytesWritten = await _serializer.SerializeAsync(value, ms, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;

        byte[] chunk = new byte[ChunkSize];
        int bytesRead;
        while ((bytesRead = await ms.ReadAsync(chunk, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await stream.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytes += bytesRead;
            progress?.Invoke(totalBytes);
        }

        return totalBytes;
    }
}
