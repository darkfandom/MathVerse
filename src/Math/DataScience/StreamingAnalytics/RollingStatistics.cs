namespace MathVerse.Math.DataScience.StreamingAnalytics;

using System;

/// <summary>
/// Maintains rolling statistics over a stream of values using Welford's online algorithm.
/// </summary>
public sealed class RollingStatistics
{
    private readonly int _windowSize;
    private readonly double[] _buffer;
    private int _head;
    private int _count;
    private double _sum;
    private double _min = double.MaxValue;
    private double _max = double.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingStatistics"/> class.
    /// </summary>
    /// <param name="windowSize">The rolling window size.</param>
    public RollingStatistics(int windowSize)
    {
        if (windowSize < 1) throw new ArgumentOutOfRangeException(nameof(windowSize), windowSize, "Window size must be at least 1.");
        _windowSize = windowSize;
        _buffer = new double[windowSize];
    }

    /// <summary>
    /// Gets the number of values currently in the rolling window.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets the arithmetic mean of values in the current rolling window.
    /// </summary>
    public double Mean => _count > 0 ? _sum / _count : 0.0;

    /// <summary>
    /// Gets the population variance of values in the current rolling window.
    /// </summary>
    public double Variance
    {
        get
        {
            if (_count < 2) return 0.0;
            double mean = _sum / _count;
            double m2 = 0.0;
            for (int i = 0; i < _count; i++)
            {
                int idx = GetIndex(i);
                double diff = _buffer[idx] - mean;
                m2 += diff * diff;
            }
            return m2 / _count;
        }
    }

    /// <summary>
    /// Gets the minimum value in the current rolling window.
    /// </summary>
    public double Min => _count > 0 ? _min : double.PositiveInfinity;

    /// <summary>
    /// Gets the maximum value in the current rolling window.
    /// </summary>
    public double Max => _count > 0 ? _max : double.NegativeInfinity;

    /// <summary>
    /// Adds a value to the rolling window and updates all statistics.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public void Add(double value)
    {
        if (_count >= _windowSize)
        {
            double removed = _buffer[_head];
            _sum -= removed;
            _count--;
        }

        _buffer[_head] = value;
        _sum += value;
        _count++;
        _head = (_head + 1) % _windowSize;

        RecomputeMinMax();
    }

    /// <summary>
    /// Gets the value at the specified position in the window (0 = oldest).
    /// </summary>
    /// <param name="relativeIndex">The zero-based relative index within the window.</param>
    /// <returns>The value at the specified position.</returns>
    public double GetAt(int relativeIndex)
    {
        if (relativeIndex < 0 || relativeIndex >= _count)
            throw new ArgumentOutOfRangeException(nameof(relativeIndex));
        return _buffer[GetIndex(relativeIndex)];
    }

    /// <summary>
    /// Gets a snapshot of the current window values in chronological order.
    /// </summary>
    /// <returns>An array containing the current window values.</returns>
    public double[] Snapshot()
    {
        double[] result = new double[_count];
        for (int i = 0; i < _count; i++)
        {
            result[i] = _buffer[GetIndex(i)];
        }
        return result;
    }

    private int GetIndex(int relativeIndex)
    {
        int start = (_head - _count + _windowSize) % _windowSize;
        return (start + relativeIndex) % _windowSize;
    }

    private void RecomputeMinMax()
    {
        _min = double.MaxValue;
        _max = double.MinValue;
        for (int i = 0; i < _count; i++)
        {
            int idx = GetIndex(i);
            if (_buffer[idx] < _min) _min = _buffer[idx];
            if (_buffer[idx] > _max) _max = _buffer[idx];
        }
    }
}
