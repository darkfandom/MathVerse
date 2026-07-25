namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Provides statistics about a quantum circuit.
/// </summary>
public sealed class CircuitStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitStatistics"/> class.
    /// </summary>
    /// <param name="totalGates">Total number of gates.</param>
    /// <param name="singleQubitGates">Number of single-qubit gates.</param>
    /// <param name="twoQubitGates">Number of two-qubit gates.</param>
    /// <param name="multiQubitGates">Number of multi-qubit gates (3+).</param>
    /// <param name="depth">Circuit depth.</param>
    /// <param name="numQubits">Number of qubits.</param>
    /// <param name="gateCounts">Count per gate type.</param>
    public CircuitStatistics(int totalGates, int singleQubitGates, int twoQubitGates, int multiQubitGates,
        int depth, int numQubits, Dictionary<string, int> gateCounts)
    {
        TotalGates = totalGates;
        SingleQubitGates = singleQubitGates;
        TwoQubitGates = twoQubitGates;
        MultiQubitGates = multiQubitGates;
        Depth = depth;
        NumQubits = numQubits;
        GateCounts = gateCounts;
    }

    /// <summary>Gets the total number of gates.</summary>
    public int TotalGates { get; }

    /// <summary>Gets the number of single-qubit gates.</summary>
    public int SingleQubitGates { get; }

    /// <summary>Gets the number of two-qubit gates.</summary>
    public int TwoQubitGates { get; }

    /// <summary>Gets the number of multi-qubit gates (3+).</summary>
    public int MultiQubitGates { get; }

    /// <summary>Gets the circuit depth.</summary>
    public int Depth { get; }

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the count per gate type.</summary>
    public Dictionary<string, int> GateCounts { get; }

    /// <summary>
    /// Gets the ratio of entangling gates (2+ qubit gates) to total gates.
    /// </summary>
    public double EntanglingGateRatio => TotalGates > 0 ? (double)(TwoQubitGates + MultiQubitGates) / TotalGates : 0.0;

    /// <summary>
    /// Computes statistics for a quantum circuit.
    /// </summary>
    /// <param name="circuit">The circuit to analyze.</param>
    /// <returns>The computed statistics.</returns>
    public static CircuitStatistics Compute(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        int single = 0, two = 0, multi = 0;
        var gateCounts = new Dictionary<string, int>();

        foreach (var gate in circuit.Gates)
        {
            string name = gate.Gate.Name;
            if (gateCounts.ContainsKey(name))
                gateCounts[name]++;
            else
                gateCounts[name] = 1;

            switch (gate.Gate.NumQubits)
            {
                case 1: single++; break;
                case 2: two++; break;
                default: multi++; break;
            }
        }

        return new CircuitStatistics(circuit.GateCount, single, two, multi, circuit.Depth, circuit.NumQubits, gateCounts);
    }

    /// <summary>
    /// Gets a summary string of the circuit statistics.
    /// </summary>
    /// <returns>A summary string.</returns>
    public string GetSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Circuit Statistics:");
        sb.AppendLine($"  Qubits: {NumQubits}");
        sb.AppendLine($"  Depth: {Depth}");
        sb.AppendLine($"  Total Gates: {TotalGates}");
        sb.AppendLine($"  Single-Qubit: {SingleQubitGates}");
        sb.AppendLine($"  Two-Qubit: {TwoQubitGates}");
        sb.AppendLine($"  Multi-Qubit: {MultiQubitGates}");
        sb.AppendLine($"  Entangling Ratio: {EntanglingGateRatio:P2}");
        sb.AppendLine("  Gate Counts:");
        foreach (var kvp in GateCounts)
        {
            sb.AppendLine($"    {kvp.Key}: {kvp.Value}");
        }
        return sb.ToString();
    }
}
