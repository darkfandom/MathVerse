namespace MathVerse.Math.Quantum.Simulation;

using System;
using System.Numerics;
using Gates;
using LinearAlgebra;
using Measurement;

/// <summary>
/// State-vector quantum simulator that maintains and manipulates a pure quantum state
/// via direct application of gate matrices to the state vector.
/// </summary>
public sealed class StateVectorSimulator
{
    private readonly int _numQubits;
    private Complex[] _state;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a state vector simulator initialized to |0...0⟩.</summary>
    public StateVectorSimulator(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _state = new Complex[1 << numQubits];
        _state[0] = Complex.One;
    }

    /// <summary>
    /// Initializes the simulator to the specified state vector.
    /// </summary>
    /// <param name="state">The initial state vector.</param>
    public void Initialize(ComplexVector state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (state.Dimension != (1 << _numQubits))
            throw new ArgumentException($"State dimension {state.Dimension} does not match {1 << _numQubits}.");
        _state = new Complex[state.Dimension];
        for (int i = 0; i < state.Dimension; i++)
            _state[i] = state[i];
    }

    /// <summary>
    /// Applies a quantum gate to the specified qubit indices.
    /// </summary>
    /// <param name="gate">The gate to apply.</param>
    /// <param name="qubitIndices">The qubit indices the gate acts on.</param>
    public void ApplyGate(IQuantumGate gate, int[] qubitIndices)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
        gate.Apply(_state, qubitIndices, _numQubits);
    }

    /// <summary>
    /// Gets the current state vector as a <see cref="ComplexVector"/>.
    /// </summary>
    public ComplexVector GetStateVector()
    {
        var copy = new Complex[_state.Length];
        Array.Copy(_state, copy, _state.Length);
        return new ComplexVector(copy);
    }

    /// <summary>
    /// Measures the specified qubit in the computational basis, collapsing the state.
    /// </summary>
    /// <param name="qubitIndex">The qubit to measure.</param>
    /// <returns>The measurement result (0 or 1).</returns>
    public MeasurementResult Measure(int qubitIndex)
    {
        if (qubitIndex < 0 || qubitIndex >= _numQubits)
            throw new ArgumentOutOfRangeException(nameof(qubitIndex));

        int mask = 1 << qubitIndex;
        double prob0 = 0.0;
        for (int i = 0; i < _state.Length; i++)
        {
            if ((i & mask) == 0)
                prob0 += _state[i].Magnitude * _state[i].Magnitude;
        }

        var rng = new Random();
        bool result = rng.NextDouble() > prob0;
        double norm = result ? System.Math.Sqrt(1.0 - prob0) : System.Math.Sqrt(prob0);

        for (int i = 0; i < _state.Length; i++)
        {
            bool bitSet = (i & mask) != 0;
            if (bitSet == result)
                _state[i] /= norm;
            else
                _state[i] = Complex.Zero;
        }

        return new MeasurementResult(new[] { result ? 1 : 0 }, qubitIndex);
    }

    /// <summary>
    /// Returns the probability of measuring the specified computational basis state.
    /// </summary>
    /// <param name="basisState">The basis state index.</param>
    /// <returns>The probability |⟨basisState|ψ⟩|².</returns>
    public double Probability(int basisState)
    {
        if (basisState < 0 || basisState >= (1 << _numQubits))
            throw new ArgumentOutOfRangeException(nameof(basisState));
        return _state[basisState].Magnitude * _state[basisState].Magnitude;
    }

    /// <summary>
    /// Resets the simulator to the |0...0⟩ state.
    /// </summary>
    public void Reset()
    {
        _state = new Complex[1 << _numQubits];
        _state[0] = Complex.One;
    }
}
