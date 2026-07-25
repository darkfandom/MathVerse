namespace MathVerse.Math.Quantum.Simulation;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;
using Measurement;
using Noise;

/// <summary>
/// Noisy quantum circuit simulator that applies a specified noise channel after each gate,
/// modeling the effect of decoherence and operational errors on quantum computations.
/// </summary>
public sealed class NoiseSimulator
{
    private readonly int _numQubits;
    private readonly NoiseChannel _noiseChannel;
    private Complex[] _state;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Gets the noise channel applied after each gate.</summary>
    public NoiseChannel NoiseChannel => _noiseChannel;

    /// <summary>Creates a noisy simulator with the specified noise channel.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="noiseChannel">The noise channel to apply after each gate.</param>
    public NoiseSimulator(int numQubits, NoiseChannel noiseChannel)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _noiseChannel = noiseChannel ?? throw new ArgumentNullException(nameof(noiseChannel));
        _state = new Complex[1 << numQubits];
        _state[0] = Complex.One;
    }

    /// <summary>
    /// Simulates the circuit with noise applied after each gate.
    /// </summary>
    /// <param name="circuit">The circuit to simulate.</param>
    /// <returns>The final (noisy) state vector.</returns>
    public ComplexVector Simulate(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        Reset();

        var rho = ComplexMatrix.Identity(1 << _numQubits);
        foreach (var circuitGate in circuit.Gates)
        {
            if (circuitGate.Gate.Name == "M") continue;
            var U = ReconstructUnitary(circuitGate.Gate, circuitGate.QubitIndices);
            var Ud = U.ConjugateTranspose();
            rho = U.Multiply(rho).Multiply(Ud);
            rho = _noiseChannel.Apply(rho);
        }

        _state = ExtractDominantState(rho);
        return GetStateVector();
    }

    /// <summary>
    /// Samples from the noisy circuit output distribution.
    /// </summary>
    /// <param name="circuit">The circuit to sample from.</param>
    /// <param name="shots">The number of measurement shots.</param>
    /// <returns>Measurement statistics.</returns>
    public MeasurementStatistics Sample(QuantumCircuit circuit, int shots)
    {
        return SampleWithNoise(circuit, shots, _noiseChannel);
    }

    /// <summary>
    /// Samples from the circuit output distribution using a specified noise channel.
    /// </summary>
    /// <param name="circuit">The circuit to sample from.</param>
    /// <param name="shots">The number of measurement shots.</param>
    /// <param name="channel">The noise channel to apply.</param>
    /// <returns>Measurement statistics.</returns>
    public MeasurementStatistics SampleWithNoise(QuantumCircuit circuit, int shots, NoiseChannel channel)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        if (channel == null) throw new ArgumentNullException(nameof(channel));
        if (shots < 1) throw new ArgumentOutOfRangeException(nameof(shots));

        var rho = ComplexMatrix.Identity(1 << _numQubits);
        foreach (var circuitGate in circuit.Gates)
        {
            if (circuitGate.Gate.Name == "M") continue;
            var U = ReconstructUnitary(circuitGate.Gate, circuitGate.QubitIndices);
            var Ud = U.ConjugateTranspose();
            rho = U.Multiply(rho).Multiply(Ud);
            rho = channel.Apply(rho);
        }

        var counts = new Dictionary<string, int>();
        var rng = new Random(42);
        var probs = ExtractProbabilities(rho);

        for (int shot = 0; shot < shots; shot++)
        {
            int outcome = SampleFromDistribution(probs, rng);
            string bitString = Convert.ToString(outcome, 2).PadLeft(_numQubits, '0');
            if (counts.ContainsKey(bitString))
                counts[bitString]++;
            else
                counts[bitString] = 1;
        }

        return new MeasurementStatistics(counts);
    }

    /// <summary>
    /// Resets the simulator to the |0...0⟩ state.
    /// </summary>
    public void Reset()
    {
        _state = new Complex[1 << _numQubits];
        _state[0] = Complex.One;
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

    private ComplexMatrix ReconstructUnitary(IQuantumGate gate, int[] qubitIndices)
    {
        int n = 1 << _numQubits;
        var matrix = new Complex[n, n];
        for (int col = 0; col < n; col++)
        {
            var basis = new Complex[n];
            basis[col] = Complex.One;
            gate.Apply(basis, qubitIndices, _numQubits);
            for (int row = 0; row < n; row++)
                matrix[row, col] = basis[row];
        }
        return new ComplexMatrix(matrix);
    }

    private static double[] ExtractProbabilities(ComplexMatrix rho)
    {
        int n = rho.Rows;
        var probs = new double[n];
        for (int i = 0; i < n; i++)
            probs[i] = rho[i, i].Real;
        return probs;
    }

    private static Complex[] ExtractDominantState(ComplexMatrix rho)
    {
        int n = rho.Rows;
        int bestIdx = 0;
        double bestProb = 0.0;
        for (int i = 0; i < n; i++)
        {
            double p = rho[i, i].Real;
            if (p > bestProb) { bestProb = p; bestIdx = i; }
        }
        var state = new Complex[n];
        state[bestIdx] = Complex.One;
        return state;
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
