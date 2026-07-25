namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;
using System.Numerics;
using MathVerse.Math.Quantum.Gates;

/// <summary>
/// Represents a quantum circuit composed of gates applied to qubits.
/// </summary>
public sealed class QuantumCircuit
{
    private readonly List<CircuitGate> _gates = new();
    private readonly int _numQubits;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumCircuit"/> class.
    /// </summary>
    /// <param name="numQubits">The number of qubits in the circuit.</param>
    public QuantumCircuit(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentException("Number of qubits must be at least 1.", nameof(numQubits));
        _numQubits = numQubits;
    }

    /// <summary>Gets the number of qubits in the circuit.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Gets the ordered list of gates in the circuit.</summary>
    public IReadOnlyList<CircuitGate> Gates => _gates;

    /// <summary>Gets the total number of gates in the circuit.</summary>
    public int GateCount => _gates.Count;

    /// <summary>
    /// Gets the depth of the circuit (number of sequential layers).
    /// </summary>
    public int Depth
    {
        get
        {
            if (_gates.Count == 0) return 0;
            int maxLayer = 0;
            foreach (var gate in _gates)
            {
                if (gate.Layer > maxLayer) maxLayer = gate.Layer;
            }
            return maxLayer + 1;
        }
    }

    /// <summary>
    /// Adds a gate to the circuit at the specified qubit indices.
    /// </summary>
    /// <param name="gate">The gate to add.</param>
    /// <param name="qubitIndices">The qubit indices the gate acts on.</param>
    public void AddGate(IQuantumGate gate, params int[] qubitIndices)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        if (qubitIndices == null || qubitIndices.Length == 0) throw new ArgumentException("Qubit indices must be provided.", nameof(qubitIndices));

        int layer = ComputeLayer(gate, qubitIndices);
        _gates.Add(new CircuitGate(gate, qubitIndices, layer));
    }

    /// <summary>
    /// Gets the gate at the specified index.
    /// </summary>
    /// <param name="index">The index of the gate.</param>
    /// <returns>The circuit gate at the specified index.</returns>
    public CircuitGate GetGate(int index)
    {
        if (index < 0 || index >= _gates.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return _gates[index];
    }

    private int ComputeLayer(IQuantumGate gate, int[] qubitIndices)
    {
        int maxLayer = -1;
        foreach (var existing in _gates)
        {
            foreach (int q in qubitIndices)
            {
                foreach (int eq in existing.QubitIndices)
                {
                    if (q == eq && existing.Layer > maxLayer)
                    {
                        maxLayer = existing.Layer;
                    }
                }
            }
        }
        return maxLayer + 1;
    }
}

/// <summary>
/// Represents a gate in a quantum circuit with its qubit indices and layer.
/// </summary>
/// <param name="Gate">The quantum gate.</param>
/// <param name="QubitIndices">The qubit indices the gate acts on.</param>
/// <param name="Layer">The layer index in the circuit.</param>
public sealed record CircuitGate(IQuantumGate Gate, int[] QubitIndices, int Layer);
