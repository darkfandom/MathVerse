namespace MathVerse.Math.Performance.Memory;

/// <summary>
/// Monitors system memory pressure by observing GC total memory and collection counts.
/// </summary>
public sealed class MemoryPressureMonitor
{
    private long _warningThresholdBytes = 512L * 1024 * 1024;
    private long _criticalThresholdBytes = 1024L * 1024 * 1024;
    private int _lastGen2Collection;
    private long _lastTotalMemory;
    private double _pressureLevel;

    /// <summary>Gets whether the system is under high memory pressure.</summary>
    public bool IsHighPressure => _pressureLevel >= 0.8;

    /// <summary>Gets the current memory pressure level as a value between 0.0 and 1.0.</summary>
    public double PressureLevel => _pressureLevel;

    /// <summary>Configures the warning and critical memory thresholds.</summary>
    /// <param name="warningThresholdBytes">The byte count at which warning pressure begins.</param>
    /// <param name="criticalThresholdBytes">The byte count at which critical pressure is reached.</param>
    public void Configure(long warningThresholdBytes, long criticalThresholdBytes)
    {
        if (warningThresholdBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(warningThresholdBytes), "Warning threshold must be positive.");

        if (criticalThresholdBytes <= warningThresholdBytes)
            throw new ArgumentOutOfRangeException(nameof(criticalThresholdBytes), "Critical threshold must exceed warning threshold.");

        _warningThresholdBytes = warningThresholdBytes;
        _criticalThresholdBytes = criticalThresholdBytes;
    }

    /// <summary>Updates the pressure level by sampling current GC metrics.</summary>
    public void Update()
    {
        var currentMemory = GC.GetTotalMemory(forceFullCollection: false);
        var gen2Collections = GC.CollectionCount(2);

        var memoryPressure = ComputeMemoryPressure(currentMemory);
        var collectionPressure = ComputeCollectionPressure(gen2Collections);

        var rawPressure = System.Math.Max(memoryPressure, collectionPressure);
        _pressureLevel = System.Math.Clamp(rawPressure, 0.0, 1.0);

        _lastTotalMemory = currentMemory;
        _lastGen2Collection = gen2Collections;
    }

    private double ComputeMemoryPressure(long currentMemory)
    {
        if (currentMemory >= _criticalThresholdBytes)
            return 1.0;

        if (currentMemory >= _warningThresholdBytes)
        {
            var range = _criticalThresholdBytes - _warningThresholdBytes;
            return 0.5 + 0.5 * (currentMemory - _warningThresholdBytes) / (double)range;
        }

        return currentMemory / (double)_warningThresholdBytes * 0.5;
    }

    private double ComputeCollectionPressure(int gen2Collections)
    {
        var delta = gen2Collections - _lastGen2Collection;
        return System.Math.Clamp(delta * 0.1, 0.0, 0.5);
    }
}
