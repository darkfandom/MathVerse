namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.Gates;

/// <summary>
/// Fluent builder for constructing quantum circuits.
/// </summary>
public sealed class CircuitBuilder
{
    private readonly QuantumCircuit _circuit;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBuilder"/> class.
    /// </summary>
    /// <param name="numQubits">The number of qubits in the circuit.</param>
    public CircuitBuilder(int numQubits)
    {
        _circuit = new QuantumCircuit(numQubits);
    }

    /// <summary>
    /// Adds a Hadamard gate to the specified qubit.
    /// </summary>
    /// <param name="qubit">The qubit index.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder H(int qubit)
    {
        _circuit.AddGate(SingleQubitGates.Hadamard, qubit);
        return this;
    }

    /// <summary>
    /// Adds a Pauli-X gate to the specified qubit.
    /// </summary>
    /// <param name="qubit">The qubit index.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder X(int qubit)
    {
        _circuit.AddGate(SingleQubitGates.PauliX, qubit);
        return this;
    }

    /// <summary>
    /// Adds a CNOT gate with the specified control and target qubits.
    /// </summary>
    /// <param name="control">The control qubit index.</param>
    /// <param name="target">The target qubit index.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder CNOT(int control, int target)
    {
        _circuit.AddGate(MultiQubitGates.CX, control, target);
        return this;
    }

    /// <summary>
    /// Adds a Toffoli gate with two control qubits and one target qubit.
    /// </summary>
    /// <param name="c1">The first control qubit index.</param>
    /// <param name="c2">The second control qubit index.</param>
    /// <param name="target">The target qubit index.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder Toffoli(int c1, int c2, int target)
    {
        _circuit.AddGate(MultiQubitGates.CCX, c1, c2, target);
        return this;
    }

    /// <summary>
    /// Adds a measurement operation to the specified qubit.
    /// </summary>
    /// <param name="qubit">The qubit index.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder Measure(int qubit)
    {
        _circuit.AddGate(new MeasurementGate(), qubit);
        return this;
    }

    /// <summary>
    /// Adds an arbitrary gate to the circuit.
    /// </summary>
    /// <param name="gate">The gate to add.</param>
    /// <param name="qubits">The qubit indices the gate acts on.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder Apply(IQuantumGate gate, params int[] qubits)
    {
        _circuit.AddGate(gate, qubits);
        return this;
    }

    /// <summary>
    /// Adds a barrier (visual separator) to the circuit.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    public CircuitBuilder Barrier()
    {
        _circuit.AddGate(new BarrierGate(), 0);
        return this;
    }

    /// <summary>
    /// Builds and returns the quantum circuit.
    /// </summary>
    /// <returns>The constructed quantum circuit.</returns>
    public QuantumCircuit Build()
    {
        return _circuit;
    }

    private sealed class MeasurementGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "M";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0 },
            { 0, 1 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            // Measurement is handled by the measurement system
        }
    }

    private sealed class BarrierGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "Barrier";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0 },
            { 0, 1 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            // Barrier has no effect on the state
        }
    }
}
