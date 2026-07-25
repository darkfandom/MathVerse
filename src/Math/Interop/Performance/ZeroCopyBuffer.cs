namespace MathVerse.Math.Interop.Performance;

using System;
using System.Buffers;

/// <summary>
/// Provides zero-copy buffer management for serialization operations using ArrayPool.
/// </summary>
public sealed class ZeroCopyBuffer : IDisposable
{
    private byte[]? _buffer;
    private int _length;
    private bool _disposed;

    /// <summary>
    /// Gets the current buffer contents as a read-only span.
    /// </summary>
    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _length);

    /// <summary>
    /// Gets the current buffer contents as a read-only memory.
    /// </summary>
    public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _length);

    /// <summary>
    /// Gets the number of bytes written.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the underlying buffer capacity.
    /// </summary>
    public int Capacity => _buffer?.Length ?? 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZeroCopyBuffer"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial buffer capacity.</param>
    public ZeroCopyBuffer(int initialCapacity = 4096)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _length = 0;
    }

    /// <summary>
    /// Appends bytes to the buffer.
    /// </summary>
    /// <param name="data">The data to append.</param>
    public void Write(ReadOnlySpan<byte> data)
    {
        EnsureCapacity(_length + data.Length);
        data.CopyTo(_buffer.AsSpan(_length));
        _length += data.Length;
    }

    /// <summary>
    /// Appends a single byte to the buffer.
    /// </summary>
    /// <param name="value">The byte value.</param>
    public void WriteByte(byte value)
    {
        EnsureCapacity(_length + 1);
        _buffer![_length++] = value;
    }

    /// <summary>
    /// Gets the written data as a byte array (allocates a new array).
    /// </summary>
    /// <returns>A byte array containing the buffer contents.</returns>
    public byte[] ToArray()
    {
        return WrittenSpan.ToArray();
    }

    /// <summary>
    /// Resets the buffer for reuse without deallocating.
    /// </summary>
    public void Reset()
    {
        _length = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed && _buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
            _disposed = true;
        }
    }

    private void EnsureCapacity(int required)
    {
        if (_buffer != null && required <= _buffer.Length)
        {
            return;
        }

        int newCapacity = System.Math.Max(required, _buffer != null ? _buffer.Length * 2 : 4096);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);
        if (_buffer != null && _length > 0)
        {
            _buffer.AsSpan(0, _length).CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(_buffer);
        }
        _buffer = newBuffer;
    }
}
