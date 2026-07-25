namespace MathVerse.Math.Quantum.Simulation;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;
using Measurement;

/// <summary>
/// Full quantum circuit simulator that applies gates sequentially to a state vector,
/// supports measurement, and can sample from the output distribution.
/// </summary>
public sealed class CircuitSimulator
{
    private readonly int _numQubits;
    private Complex[] _state;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a circuit simulator for the specified number of qubits, initialized to |0...0⟩.</summary>
    public CircuitSimulator(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _state = new Complex[1 << numQubits];
        _state[0] = Complex.One;
    }

    /// <summary>
    /// Simulates the given circuit starting from the |0...0⟩ state.
    /// </summary>
    /// <param name="circuit">The circuit to simulate.</param>
    /// <returns>The final state vector.</returns>
    public ComplexVector Simulate(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        Reset();
        ApplyCircuit(circuit);
        return GetStateVector();
    }

    /// <summary>
    /// Simulates the given circuit starting from the specified initial state.
    /// </summary>
    /// <param name="circuit">The circuit to simulate.</param>
    /// <param name="initialState">The initial state vector.</param>
    /// <returns>The final state vector.</returns>
    public ComplexVector Simulate(QuantumCircuit circuit, ComplexVector initialState)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        if (initialState == null) throw new ArgumentNullException(nameof(initialState));
        if (initialState.Dimension != (1 << _numQubits))
            throw new ArgumentException($"Initial state dimension {initialState.Dimension} does not match {1 << _numQubits}.");

        _state = new Complex[1 << _numQubits];
        for (int i = 0; i < _numQubits; i++)
            _state[i] = initialState[i];
        ApplyCircuit(circuit);
        return GetStateVector();
    }

    /// <summary>
    /// Samples from the circuit's output distribution.
    /// </summary>
    /// <param name="circuit">The circuit to sample from.</param>
    /// <param name="shots">The number of measurement shots.</param>
    /// <returns>Measurement statistics aggregated over all shots.</returns>
    public MeasurementStatistics Sample(QuantumCircuit circuit, int shots)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        if (shots < 1) throw new ArgumentOutOfRangeException(nameof(shots));

        Simulate(circuit);
        var counts = new Dictionary<string, int>();
        var rng = new Random(42);
        int n = 1 << _numQubits;
        double[] probabilities = new double[n];
        for (int i = 0; i < n; i++)
            probabilities[i] = _state[i].Magnitude * _state[i].Magnitude;

        for (int shot = 0; shot < shots; shot++)
        {
            int outcome = SampleFromDistribution(probabilities, rng);
            string bitString = Convert.ToString(outcome, 2).PadLeft(_numQubits, '0');
            if (counts.ContainsKey(bitString))
                counts[bitString]++;
            else
                counts[bitString] = 1;
        }

        return new MeasurementStatistics(counts);
    }

    /// <summary>
    /// Gets the current state vector.
    /// </summary>
    public ComplexVector GetStateVector()
    {
        var copy = new Complex[_state.Length];
        Array.Copy(_state, copy, _state.Length);
        return new ComplexVector(copy);
    }

    /// <summary>
    /// Resets the simulator to the |0...0⟩ state.
    /// </summary>
    public void Reset()
    {
        _state = new Complex[1 << _numQubits];
        _state[0] = Complex.One;
    }

    private void ApplyCircuit(QuantumCircuit circuit)
    {
        foreach (var circuitGate in circuit.Gates)
        {
            if (circuitGate.Gate.Name == "M") continue;
            circuitGate.Gate.Apply(_state, circuitGate.QubitIndices, _numQubits);
        }
    }

    private static int SampleFromDistribution(double[] probabilities, Random rng)
    {
        double r = rng.NextDouble();
        double cumulative = 0.0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (r <= cumulative) return i;
        }
        return probabilities.Length - 1;
    }
}
