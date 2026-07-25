namespace MathVerse.Math.DataScience.StreamingAnalytics;

using System;

/// <summary>
/// A fixed-size sliding window that maintains the most recent values added.
/// </summary>
public sealed class SlidingWindow
{
    private readonly double[] _buffer;
    private readonly int _capacity;
    private int _head;
    private int _count;
    private double _sum;
    private double _sumSquares;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindow"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of values the window can hold.</param>
    public SlidingWindow(int capacity)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");
        _capacity = capacity;
        _buffer = new double[capacity];
    }

    /// <summary>
    /// Gets the number of values currently in the window.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets the current values in the window in insertion order.
    /// </summary>
    public ReadOnlySpan<double> Values
    {
        get
        {
            if (_count == 0) return ReadOnlySpan<double>.Empty;

            double[] result = new double[_count];
            if (_count < _capacity)
            {
                System.Array.Copy(_buffer, 0, result, 0, _count);
            }
            else
            {
                int start = (_head - _count + _capacity) % _capacity;
                for (int i = 0; i < _count; i++)
                {
                    result[i] = _buffer[(start + i) % _capacity];
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Gets the arithmetic mean of values currently in the window.
    /// Returns 0 if the window is empty.
    /// </summary>
    public double Mean => _count > 0 ? _sum / _count : 0.0;

    /// <summary>
    /// Gets the standard deviation of values currently in the window.
    /// Uses the sample standard deviation formula.
    /// </summary>
    public double StdDev
    {
        get
        {
            if (_count < 2) return 0.0;
            double variance = (_sumSquares - (_sum * _sum) / _count) / (_count - 1);
            return variance > 0.0 ? System.Math.Sqrt(variance) : 0.0;
        }
    }

    /// <summary>
    /// Gets the minimum value currently in the window.
    /// Returns <see cref="double.PositiveInfinity"/> if the window is empty.
    /// </summary>
    public double Min
    {
        get
        {
            if (_count == 0) return double.PositiveInfinity;

            double min = double.MaxValue;
            for (int i = 0; i < _count; i++)
            {
                int idx;
                if (_count < _capacity)
                    idx = i;
                else
                    idx = ((_head - _count + _capacity) % _capacity + i) % _capacity;

                if (_buffer[idx] < min) min = _buffer[idx];
            }
            return min;
        }
    }

    /// <summary>
    /// Gets the maximum value currently in the window.
    /// Returns <see cref="double.NegativeInfinity"/> if the window is empty.
    /// </summary>
    public double Max
    {
        get
        {
            if (_count == 0) return double.NegativeInfinity;

            double max = double.MinValue;
            for (int i = 0; i < _count; i++)
            {
                int idx;
                if (_count < _capacity)
                    idx = i;
                else
                    idx = ((_head - _count + _capacity) % _capacity + i) % _capacity;

                if (_buffer[idx] > max) max = _buffer[idx];
            }
            return max;
        }
    }

    /// <summary>
    /// Adds a value to the sliding window. If the window is at capacity, the oldest value is overwritten.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public void Add(double value)
    {
        if (_count >= _capacity)
        {
            double removed = _buffer[_head];
            _sum -= removed;
            _sumSquares -= removed * removed;
        }
        else
        {
            _count++;
        }

        _buffer[_head] = value;
        _sum += value;
        _sumSquares += value * value;
        _head = (_head + 1) % _capacity;
    }
}
