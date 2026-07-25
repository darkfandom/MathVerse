namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Provides standard projective measurement operations on quantum states.
/// </summary>
public sealed class Measurement
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="Measurement"/> class.
    /// </summary>
    public Measurement()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Measurement"/> class with a specified random seed.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public Measurement(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Measures a single qubit in the computational basis.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="qubitIndex">The index of the qubit to measure.</param>
    /// <returns>The measurement result.</returns>
    public MeasurementResult Measure(ComplexVector state, int qubitIndex)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        int totalQubits = (int)System.Math.Log2(state.Dimension);
        if (qubitIndex < 0 || qubitIndex >= totalQubits)
            throw new ArgumentOutOfRangeException(nameof(qubitIndex));

        double prob0 = 0.0;
        int n = state.Dimension;
        int mask = 1 << qubitIndex;

        for (int i = 0; i < n; i++)
        {
            if ((i & mask) == 0)
            {
                prob0 += state[i].Magnitude * state[i].Magnitude;
            }
        }

        double randomValue = _random.NextDouble();
        int outcome = randomValue < prob0 ? 0 : 1;

        ComplexVector postState = CollapseState(state, qubitIndex, outcome);
        double probability = outcome == 0 ? prob0 : 1.0 - prob0;

        return new MeasurementResult(outcome, probability, postState, qubitIndex);
    }

    /// <summary>
    /// Measures all qubits in the computational basis.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <returns>The measurement result.</returns>
    public MeasurementResult MeasureAll(ComplexVector state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        int n = state.Dimension;
        double[] probabilities = new double[n];

        for (int i = 0; i < n; i++)
        {
            probabilities[i] = state[i].Magnitude * state[i].Magnitude;
        }

        int outcome = SampleFromDistribution(probabilities);
        return new MeasurementResult(outcome, probabilities[outcome], null, -1);
    }

    /// <summary>
    /// Measures multiple qubits in the computational basis.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="qubitIndices">The indices of the qubits to measure.</param>
    /// <returns>An array of measurement results.</returns>
    public MeasurementResult[] MeasureMultiple(ComplexVector state, int[] qubitIndices)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));

        var results = new MeasurementResult[qubitIndices.Length];
        ComplexVector currentState = state;

        for (int i = 0; i < qubitIndices.Length; i++)
        {
            results[i] = Measure(currentState, qubitIndices[i]);
            currentState = results[i].PostMeasurementState ?? currentState;
        }

        return results;
    }

    private ComplexVector CollapseState(ComplexVector state, int qubitIndex, int outcome)
    {
        int n = state.Dimension;
        int mask = 1 << qubitIndex;
        var newCoefficients = new Complex[n];

        double norm = 0.0;
        for (int i = 0; i < n; i++)
        {
            bool bitSet = (i & mask) != 0;
            if ((outcome == 1 && bitSet) || (outcome == 0 && !bitSet))
            {
                newCoefficients[i] = state[i];
                norm += state[i].Magnitude * state[i].Magnitude;
            }
        }

        if (norm > 1e-15)
        {
            double sqrtNorm = System.Math.Sqrt(norm);
            for (int i = 0; i < n; i++)
            {
                newCoefficients[i] /= sqrtNorm;
            }
        }

        return new ComplexVector(newCoefficients);
    }

    private int SampleFromDistribution(double[] probabilities)
    {
        double randomValue = _random.NextDouble();
        double cumulative = 0.0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue < cumulative)
            {
                return i;
            }
        }

        return probabilities.Length - 1;
    }
}
