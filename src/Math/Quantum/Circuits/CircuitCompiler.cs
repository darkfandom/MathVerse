namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;
using MathVerse.Math.Quantum.Gates;

/// <summary>
/// Compiles quantum circuits into optimized gate sequences.
/// </summary>
public static class CircuitCompiler
{
    /// <summary>
    /// Compiles a circuit into a gate operation sequence.
    /// </summary>
    /// <param name="circuit">The circuit to compile.</param>
    /// <returns>The compiled circuit.</returns>
    public static CompiledCircuit Compile(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var operations = new GateOperation[circuit.GateCount];
        for (int i = 0; i < circuit.GateCount; i++)
        {
            operations[i] = new GateOperation(i, circuit.GetGate(i).QubitIndices);
        }

        return new CompiledCircuit(circuit.NumQubits, operations, circuit.Depth);
    }

    /// <summary>
    /// Optimizes and then compiles a circuit into a gate operation sequence.
    /// </summary>
    /// <param name="circuit">The circuit to compile.</param>
    /// <returns>The compiled circuit.</returns>
    public static CompiledCircuit CompileWithOptimization(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        QuantumCircuit optimized = CircuitOptimizer.Optimize(circuit);
        return Compile(optimized);
    }
}

/// <summary>
/// Represents a compiled quantum circuit as a sequence of gate operations.
/// </summary>
public sealed class CompiledCircuit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledCircuit"/> class.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="operations">The gate operations.</param>
    /// <param name="depth">The circuit depth.</param>
    public CompiledCircuit(int numQubits, GateOperation[] operations, int depth)
    {
        NumQubits = numQubits;
        Operations = operations;
        Depth = depth;
    }

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the gate operations.</summary>
    public GateOperation[] Operations { get; }

    /// <summary>Gets the circuit depth.</summary>
    public int Depth { get; }
}

/// <summary>
/// Represents a single gate operation in a compiled circuit.
/// </summary>
/// <param name="GateIndex">The index of the gate in the circuit.</param>
/// <param name="QubitIndices">The qubit indices the gate acts on.</param>
public sealed record GateOperation(int GateIndex, int[] QubitIndices);
