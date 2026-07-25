namespace MathVerse.Math.DataScience.StreamingAnalytics;

using System;

/// <summary>
/// Computes running statistics over a data stream using Welford's online algorithm.
/// No storage of individual data points is required.
/// </summary>
public sealed class IncrementalStatistics
{
    private long _count;
    private double _mean;
    private double _m2;
    private double _min = double.MaxValue;
    private double _max = double.MinValue;

    /// <summary>
    /// Gets the number of values that have been processed.
    /// </summary>
    public long Count => _count;

    /// <summary>
    /// Gets the running arithmetic mean.
    /// </summary>
    public double Mean => _count > 0 ? _mean : 0.0;

    /// <summary>
    /// Gets the running population variance.
    /// </summary>
    public double Variance => _count > 1 ? _m2 / _count : 0.0;

    /// <summary>
    /// Gets the running sample variance (Bessel's correction).
    /// </summary>
    public double SampleVariance => _count > 1 ? _m2 / (_count - 1) : 0.0;

    /// <summary>
    /// Gets the running standard deviation (population).
    /// </summary>
    public double StdDev
    {
        get
        {
            double v = Variance;
            return v > 0.0 ? System.Math.Sqrt(v) : 0.0;
        }
    }

    /// <summary>
    /// Gets the running sample standard deviation.
    /// </summary>
    public double SampleStdDev
    {
        get
        {
            double v = SampleVariance;
            return v > 0.0 ? System.Math.Sqrt(v) : 0.0;
        }
    }

    /// <summary>
    /// Gets the minimum value observed so far.
    /// </summary>
    public double Min => _count > 0 ? _min : double.PositiveInfinity;

    /// <summary>
    /// Gets the maximum value observed so far.
    /// </summary>
    public double Max => _count > 0 ? _max : double.NegativeInfinity;

    /// <summary>
    /// Updates the running statistics with a new value using Welford's online algorithm.
    /// </summary>
    /// <param name="value">The new value to incorporate.</param>
    public void Update(double value)
    {
        _count++;
        double delta = value - _mean;
        _mean += delta / _count;
        double delta2 = value - _mean;
        _m2 += delta * delta2;

        if (value < _min) _min = value;
        if (value > _max) _max = value;
    }

    /// <summary>
    /// Resets all running statistics to their initial state.
    /// </summary>
    public void Reset()
    {
        _count = 0;
        _mean = 0.0;
        _m2 = 0.0;
        _min = double.MaxValue;
        _max = double.MinValue;
    }

    /// <summary>
    /// Merges another <see cref="IncrementalStatistics"/> into this instance using parallel combination.
    /// Both instances must have been initialized with the same scale of data for meaningful results.
    /// </summary>
    /// <param name="other">The other statistics to merge.</param>
    public void Merge(IncrementalStatistics other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (other._count == 0) return;

        long combinedCount = _count + other._count;
        double combinedMean = (_mean * _count + other._mean * other._count) / combinedCount;
        double deltaB = other._mean - combinedMean;

        _m2 += other._m2 + deltaB * deltaB * _count * other._count / combinedCount;
        _mean = combinedMean;
        _count = combinedCount;

        if (other._min < _min) _min = other._min;
        if (other._max > _max) _max = other._max;
    }
}
