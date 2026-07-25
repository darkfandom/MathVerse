namespace MathVerse.Math.Performance.Metrics;

/// <summary>
/// Thread-safe named performance counter that tracks increment/decrement operations
/// and records observed values for computing averages.
/// </summary>
public sealed class PerformanceCounter
{
    private long _value;
    private long _sum;
    private long _count;

    /// <summary>
    /// Initializes a new performance counter with the specified name.
    /// </summary>
    /// <param name="name">The name of this counter.</param>
    public PerformanceCounter(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the name of this counter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the current value of the counter.
    /// </summary>
    public long Value => Interlocked.Read(ref _value);

    /// <summary>
    /// Gets the average of all recorded values.
    /// </summary>
    public double Average
    {
        get
        {
            var count = Interlocked.Read(ref _count);
            return count > 0 ? (double)Interlocked.Read(ref _sum) / count : 0.0;
        }
    }

    /// <summary>
    /// Increments the counter by one.
    /// </summary>
    public void Increment()
    {
        Interlocked.Increment(ref _value);
    }

    /// <summary>
    /// Increments the counter by the specified value.
    /// </summary>
    /// <param name="value">The amount to add.</param>
    public void Increment(long value)
    {
        Interlocked.Add(ref _value, value);
    }

    /// <summary>
    /// Decrements the counter by one.
    /// </summary>
    public void Decrement()
    {
        Interlocked.Decrement(ref _value);
    }

    /// <summary>
    /// Records an observed value for average computation without changing the current value.
    /// </summary>
    /// <param name="value">The observed value to record.</param>
    public void Record(double value)
    {
        Interlocked.Add(ref _sum, (long)value);
        Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Resets the counter and all accumulated statistics to zero.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _value, 0);
        Interlocked.Exchange(ref _sum, 0);
        Interlocked.Exchange(ref _count, 0);
    }
}
