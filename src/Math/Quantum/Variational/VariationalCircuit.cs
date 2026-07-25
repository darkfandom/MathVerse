namespace MathVerse.Math.Quantum.Variational;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Constructs parameterized quantum circuits (ansätze) for variational quantum algorithms
/// such as VQE and QAOA, supporting configurable layer patterns and hardware-efficient designs.
/// </summary>
public sealed class VariationalCircuit
{
    private readonly int _numQubits;
    private readonly int _depth;
    private readonly List<(string Pattern, int QubitStart, int QubitCount)> _layers;
    private int _parameterCount;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Gets the circuit depth (number of layers).</summary>
    public int Depth => _depth;

    /// <summary>Gets the total number of parameters required.</summary>
    public int ParameterCount => _parameterCount;

    /// <summary>Creates a parameterized variational circuit.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="depth">The number of repeating layers.</param>
    public VariationalCircuit(int numQubits, int depth)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
        _numQubits = numQubits;
        _depth = depth;
        _layers = new List<(string, int, int)>();
        _parameterCount = 0;
    }

    /// <summary>
    /// Adds a parameterized layer with the specified pattern.
    /// Supported patterns: "rz-rx-rz", "rx-rz-rx", "ry-rz", "basic".
    /// </summary>
    /// <param name="pattern">The rotation gate pattern for the layer.</param>
    public void AddLayer(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));

        int paramsPerQubit = pattern switch
        {
            "rz-rx-rz" => 3,
            "rx-rz-rx" => 3,
            "ry-rz" => 2,
            "ry-rx-rz" => 3,
            "basic" => 1,
            _ => throw new ArgumentException($"Unknown pattern: {pattern}")
        };

        _layers.Add((pattern, 0, _numQubits));
        _parameterCount += paramsPerQubit * _numQubits;
    }

    /// <summary>
    /// Builds the quantum circuit with the specified parameter values.
    /// </summary>
    /// <param name="parameters">The parameter values (must match <see cref="ParameterCount"/>).</param>
    /// <returns>A <see cref="QuantumCircuit"/> with the parameterized gates applied.</returns>
    public QuantumCircuit BuildCircuit(double[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (parameters.Length < _parameterCount)
            throw new ArgumentException($"Expected at least {_parameterCount} parameters, got {parameters.Length}.");

        var circuit = new QuantumCircuit(_numQubits);
        int paramIdx = 0;

        for (int layerIdx = 0; layerIdx < _depth; layerIdx++)
        {
            if (layerIdx < _layers.Count)
            {
                var (pattern, _, _) = _layers[layerIdx];
                paramIdx = ApplyPattern(circuit, pattern, parameters, paramIdx);
            }
            else
            {
                paramIdx = ApplyPattern(circuit, "rz-rx-rz", parameters, paramIdx);
            }

            AddEntanglingLayer(circuit);
        }

        return circuit;
    }

    /// <summary>
    /// Creates a hardware-efficient ansatz with alternating rotation and entangling layers.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="layers">The number of layer repetitions.</param>
    /// <returns>A configured <see cref="VariationalCircuit"/>.</returns>
    public static VariationalCircuit HardwareEfficient(int numQubits, int layers)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (layers < 1) throw new ArgumentOutOfRangeException(nameof(layers));

        var circuit = new VariationalCircuit(numQubits, layers);
        for (int i = 0; i < layers; i++)
            circuit.AddLayer("ry-rz");
        return circuit;
    }

    private int ApplyPattern(QuantumCircuit circuit, string pattern, double[] parameters, int paramIdx)
    {
        var gates = pattern.Split('-');
        foreach (string gate in gates)
        {
            for (int q = 0; q < _numQubits; q++)
            {
                double angle = parameters[paramIdx++];
                IQuantumGate quantumGate = gate.ToUpperInvariant() switch
                {
                    "RZ" => RotationGates.RZ(angle),
                    "RX" => RotationGates.RX(angle),
                    "RY" => RotationGates.RY(angle),
                    _ => RotationGates.RZ(angle)
                };
                circuit.AddGate(quantumGate, q);
            }
        }
        return paramIdx;
    }

    private void AddEntanglingLayer(QuantumCircuit circuit)
    {
        for (int q = 0; q < _numQubits - 1; q++)
            circuit.AddGate(MultiQubitGates.CX, q, q + 1);
    }
}
