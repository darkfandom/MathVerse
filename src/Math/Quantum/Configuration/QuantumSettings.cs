namespace MathVerse.Math.Quantum.Configuration;

/// <summary>
/// Defines configurable settings for quantum operations as an immutable record.
/// </summary>
public readonly record struct QuantumSettings
{
    /// <summary>
    /// Gets the default number of measurement shots.
    /// </summary>
    public int DefaultShots { get; init; }

    /// <summary>
    /// Gets a value indicating whether noise simulation is enabled.
    /// </summary>
    public bool EnableNoise { get; init; }

    /// <summary>
    /// Gets the default simulator name.
    /// </summary>
    public string DefaultSimulator { get; init; }

    /// <summary>
    /// Gets the maximum number of qubits supported.
    /// </summary>
    public int MaxQubits { get; init; }

    /// <summary>
    /// Gets a value indicating whether SIMD acceleration is enabled.
    /// </summary>
    public bool EnableSIMD { get; init; }

    /// <summary>
    /// Gets the maximum thread count for parallel operations.
    /// </summary>
    public int MaxThreadCount { get; init; }

    /// <summary>
    /// Initializes default quantum settings.
    /// </summary>
    public QuantumSettings()
    {
        DefaultShots = 1024;
        EnableNoise = false;
        DefaultSimulator = "StateVectorSimulator";
        MaxQubits = 20;
        EnableSIMD = true;
        MaxThreadCount = Environment.ProcessorCount;
    }
}
