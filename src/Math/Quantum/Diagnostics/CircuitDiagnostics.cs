namespace MathVerse.Math.Quantum.Diagnostics;

/// <summary>
/// Provides diagnostic information about quantum circuit structure and properties.
/// </summary>
public sealed class CircuitDiagnostics
{
    /// <summary>
    /// Gets the total number of gates in the circuit.
    /// </summary>
    public int GateCount { get; }

    /// <summary>
    /// Gets the circuit depth (longest path through the circuit).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets the number of qubits in the circuit.
    /// </summary>
    public int QubitCount { get; }

    /// <summary>
    /// Gets the count of each gate type in the circuit.
    /// </summary>
    public Dictionary<string, int> GateTypeCounts { get; }

    /// <summary>
    /// Gets a measure of entanglement in the circuit (0.0 to 1.0).
    /// </summary>
    public double EntanglementMeasure { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitDiagnostics"/> class.
    /// </summary>
    /// <param name="gateCount">The total gate count.</param>
    /// <param name="depth">The circuit depth.</param>
    /// <param name="qubitCount">The number of qubits.</param>
    /// <param name="gateTypeCounts">The count of each gate type.</param>
    /// <param name="entanglementMeasure">The entanglement measure.</param>
    public CircuitDiagnostics(
        int gateCount,
        int depth,
        int qubitCount,
        Dictionary<string, int> gateTypeCounts,
        double entanglementMeasure)
    {
        GateCount = gateCount;
        Depth = depth;
        QubitCount = qubitCount;
        GateTypeCounts = gateTypeCounts ?? throw new ArgumentNullException(nameof(gateTypeCounts));
        EntanglementMeasure = entanglementMeasure;
    }

    /// <summary>
    /// Returns a summary string of the circuit diagnostics.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"Circuit: {QubitCount} qubits, {GateCount} gates, depth {Depth}, entanglement {EntanglementMeasure:F4}";
    }
}
