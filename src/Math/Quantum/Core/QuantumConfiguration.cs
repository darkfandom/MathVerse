namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Represents global configuration for the quantum computing framework.
/// </summary>
public sealed class QuantumConfiguration
{
    /// <summary>
    /// Gets or sets the default number of measurement shots.
    /// </summary>
    public int DefaultShots { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether noise simulation is enabled.
    /// </summary>
    public bool EnableNoiseSimulation { get; set; }

    /// <summary>
    /// Gets or sets the default simulator name.
    /// </summary>
    public string DefaultSimulator { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of qubits supported.
    /// </summary>
    public int MaxQubits { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether circuit optimization is enabled.
    /// </summary>
    public bool EnableOptimization { get; set; }

    /// <summary>
    /// Gets or sets the default timeout for operations.
    /// </summary>
    public TimeSpan OperationTimeout { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumConfiguration"/> class with default values.
    /// </summary>
    public QuantumConfiguration()
    {
        DefaultShots = 1024;
        EnableNoiseSimulation = false;
        DefaultSimulator = "StateVectorSimulator";
        MaxQubits = 20;
        EnableOptimization = true;
        OperationTimeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Creates a default configuration instance.
    /// </summary>
    /// <returns>A new <see cref="QuantumConfiguration"/> with default settings.</returns>
    public static QuantumConfiguration CreateDefault()
    {
        return new QuantumConfiguration();
    }
}
