namespace MathVerse.Math.Quantum.Configuration;

/// <summary>
/// Builder pattern for constructing <see cref="Core.QuantumConfiguration"/> instances with fluent API.
/// </summary>
public sealed class QuantumConfigurationBuilder
{
    private int _shots = 1024;
    private bool _noiseSimulation = false;
    private string _simulator = "StateVectorSimulator";
    private int _maxQubits = 20;
    private bool _optimization = true;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Sets the default number of measurement shots.
    /// </summary>
    /// <param name="shots">The number of shots.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithShots(int shots)
    {
        _shots = shots;
        return this;
    }

    /// <summary>
    /// Enables or disables noise simulation.
    /// </summary>
    /// <param name="enabled">Whether noise simulation is enabled.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithNoiseSimulation(bool enabled)
    {
        _noiseSimulation = enabled;
        return this;
    }

    /// <summary>
    /// Sets the default simulator name.
    /// </summary>
    /// <param name="simulator">The simulator name.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithSimulator(string simulator)
    {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        return this;
    }

    /// <summary>
    /// Sets the maximum number of qubits supported.
    /// </summary>
    /// <param name="maxQubits">The maximum qubit count.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithMaxQubits(int maxQubits)
    {
        _maxQubits = maxQubits;
        return this;
    }

    /// <summary>
    /// Enables or disables circuit optimization.
    /// </summary>
    /// <param name="enabled">Whether optimization is enabled.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithOptimization(bool enabled)
    {
        _optimization = enabled;
        return this;
    }

    /// <summary>
    /// Sets the default operation timeout.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public QuantumConfigurationBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Builds and returns a <see cref="Core.QuantumConfiguration"/> instance with the configured values.
    /// </summary>
    /// <returns>A new <see cref="Core.QuantumConfiguration"/> instance.</returns>
    public Core.QuantumConfiguration Build()
    {
        return new Core.QuantumConfiguration
        {
            DefaultShots = _shots,
            EnableNoiseSimulation = _noiseSimulation,
            DefaultSimulator = _simulator,
            MaxQubits = _maxQubits,
            EnableOptimization = _optimization,
            OperationTimeout = _timeout
        };
    }
}
