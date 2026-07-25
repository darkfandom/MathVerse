namespace MathVerse.Math.AI.Performance;

/// <summary>Wraps <see cref="System.Buffers.ArrayPool{T}"/> for efficient temporary double array allocation with automatic return.</summary>
public sealed class ArrayPoolBuffer : IDisposable
{
    private double[]? _buffer;
    private int _length;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="ArrayPoolBuffer"/> class.</summary>
    /// <param name="initialSize">The initial buffer capacity.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when initialSize is negative.</exception>
    public ArrayPoolBuffer(int initialSize = 0)
    {
        if (initialSize < 0)
            throw new ArgumentOutOfRangeException(nameof(initialSize));

        if (initialSize > 0)
        {
            _buffer = ArrayPool<double>.Shared.Rent(initialSize);
            _length = 0;
        }
    }

    /// <summary>Gets the current number of elements in the buffer.</summary>
    public int Length
    {
        get
        {
            ThrowIfDisposed();
            return _length;
        }
    }

    /// <summary>Gets the current capacity of the underlying rented array.</summary>
    public int Capacity
    {
        get
        {
            ThrowIfDisposed();
            return _buffer?.Length ?? 0;
        }
    }

    /// <summary>Gets whether the buffer has been disposed.</summary>
    public bool IsDisposed => _disposed;

    /// <summary>Accesses an element by index.</summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The value at the specified index.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public double this[int index]
    {
        get
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _buffer![index];
        }
        set
        {
            ThrowIfDisposed();
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException(nameof(index));
            _buffer![index] = value;
        }
    }

    /// <summary>Rents a buffer of at least the specified size.</summary>
    /// <param name="size">The minimum required capacity.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is negative.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Rent(int size)
    {
        ThrowIfDisposed();

        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        if (_buffer != null && _buffer.Length >= size)
        {
            _length = 0;
            return;
        }

        if (_buffer != null)
        {
            ArrayPool<double>.Shared.Return(_buffer, clearArray: true);
        }

        _buffer = ArrayPool<double>.Shared.Rent(size);
        _length = 0;
    }

    /// <summary>Appends a value to the end of the buffer, growing if necessary.</summary>
    /// <param name="value">The value to append.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Append(double value)
    {
        ThrowIfDisposed();

        if (_buffer == null || _length >= _buffer.Length)
        {
            Grow();
        }

        _buffer![_length++] = value;
    }

    /// <summary>Appends multiple values to the buffer.</summary>
    /// <param name="values">The values to append.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void AppendRange(ReadOnlySpan<double> values)
    {
        ThrowIfDisposed();

        foreach (var value in values)
        {
            Append(value);
        }
    }

    /// <summary>Copies the active portion of the buffer into a new array.</summary>
    /// <returns>A new array containing the buffer's contents.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public double[] ToArray()
    {
        ThrowIfDisposed();

        if (_buffer == null || _length == 0)
        {
            return [];
        }

        var result = new double[_length];
        System.Array.Copy(_buffer, result, _length);
        return result;
    }

    /// <summary>Copies the active portion of the buffer into the destination span.</summary>
    /// <param name="destination">The destination span to copy into.</param>
    /// <exception cref="ArgumentException">Thrown when the destination is too small.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void CopyTo(Span<double> destination)
    {
        ThrowIfDisposed();

        if (destination.Length < _length)
            throw new ArgumentException("Destination span is too small.");

        if (_buffer != null && _length > 0)
        {
            for (int i = 0; i < _length; i++)
            {
                destination[i] = _buffer[i];
            }
        }
    }

    /// <summary>Resizes the buffer to the specified length, growing or shrinking as needed.</summary>
    /// <param name="newLength">The new length.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when newLength is negative.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public void Resize(int newLength)
    {
        ThrowIfDisposed();

        if (newLength < 0)
            throw new ArgumentOutOfRangeException(nameof(newLength));

        if (newLength == 0)
        {
            if (_buffer != null)
            {
                ArrayPool<double>.Shared.Return(_buffer, clearArray: true);
                _buffer = null;
            }
            _length = 0;
            return;
        }

        if (_buffer != null && _buffer.Length >= newLength)
        {
            _length = newLength;
            return;
        }

        double[] newBuffer = ArrayPool<double>.Shared.Rent(newLength);
        if (_buffer != null && _length > 0)
        {
            System.Array.Copy(_buffer, newBuffer, System.Math.Min(_length, newLength));
        }

        if (_buffer != null)
        {
            ArrayPool<double>.Shared.Return(_buffer, clearArray: true);
        }

        _buffer = newBuffer;
        _length = newLength;
    }

    /// <summary>Gets a span view over the active portion of the buffer.</summary>
    /// <returns>A span over the buffer contents.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    public Span<double> AsSpan()
    {
        ThrowIfDisposed();

        if (_buffer == null || _length == 0)
        {
            return [];
        }

        return new Span<double>(_buffer, 0, _length);
    }

    /// <summary>Disposes the buffer, returning the underlying array to the pool.</summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_buffer != null)
        {
            ArrayPool<double>.Shared.Return(_buffer, clearArray: true);
            _buffer = null;
        }

        _length = 0;
        _disposed = true;
    }

    /// <summary>Grows the buffer by doubling its capacity.</summary>
    private void Grow()
    {
        int newCapacity = _buffer == null ? 4 : _buffer.Length * 2;
        Resize(newCapacity);
    }

    /// <summary>Throws if the buffer has been disposed.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed.</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
