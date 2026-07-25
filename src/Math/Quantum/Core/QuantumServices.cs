namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Service locator that provides access to registered quantum services and global configuration.
/// </summary>
public sealed class QuantumServices
{
    /// <summary>
    /// Gets the service registry.
    /// </summary>
    public QuantumRegistry Registry { get; }

    /// <summary>
    /// Gets the global quantum configuration.
    /// </summary>
    public QuantumConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumServices"/> class.
    /// </summary>
    public QuantumServices()
    {
        Registry = new QuantumRegistry();
        Configuration = QuantumConfiguration.CreateDefault();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumServices"/> class with the specified configuration.
    /// </summary>
    /// <param name="configuration">The quantum configuration to use.</param>
    public QuantumServices(QuantumConfiguration configuration)
    {
        Registry = new QuantumRegistry();
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Registers default quantum services including standard simulators.
    /// </summary>
    public void RegisterDefaultServices()
    {
        Registry.Register<QuantumConfiguration>("Configuration", () => Configuration);
        Registry.Register<QuantumNoiseProfile>("IdealNoiseProfile", () => QuantumNoiseProfile.CreateIdeal());
    }
}
