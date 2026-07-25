namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Specifies options for quantum operations and simulations.
/// </summary>
public sealed class QuantumOptions
{
    /// <summary>
    /// Gets or sets the number of qubits to use in the operation.
    /// </summary>
    public int NumQubits { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to simulate ideal (noiseless) behavior.
    /// </summary>
    public bool SimulateIdeal { get; set; }

    /// <summary>
    /// Gets or sets the number of measurement shots to perform.
    /// </summary>
    public int Shots { get; set; }

    /// <summary>
    /// Gets or sets the random seed for reproducible simulations, or <c>null</c> for random behavior.
    /// </summary>
    public int? RandomSeed { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed time for the operation, or <c>null</c> for no timeout.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets additional key-value options for the operation.
    /// </summary>
    public Dictionary<string, string> AdditionalOptions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumOptions"/> class with default values.
    /// </summary>
    public QuantumOptions()
    {
        NumQubits = 2;
        SimulateIdeal = true;
        Shots = 1024;
        RandomSeed = null;
        Timeout = null;
        AdditionalOptions = new Dictionary<string, string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumOptions"/> class with specified values.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="simulateIdeal">Whether to simulate ideal behavior.</param>
    /// <param name="shots">The number of measurement shots.</param>
    public QuantumOptions(int numQubits, bool simulateIdeal, int shots)
    {
        NumQubits = numQubits;
        SimulateIdeal = simulateIdeal;
        Shots = shots;
        RandomSeed = null;
        Timeout = null;
        AdditionalOptions = new Dictionary<string, string>();
    }
}
