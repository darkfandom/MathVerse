namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Provides POVM (Positive Operator-Valued Measure) measurement operations.
/// </summary>
public sealed class POVMMeasurement
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="POVMMeasurement"/> class.
    /// </summary>
    public POVMMeasurement()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="POVMMeasurement"/> class with a specified random seed.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public POVMMeasurement(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Measures a state using a set of POVM elements.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="povmElements">The POVM element matrices.</param>
    /// <returns>The measurement result.</returns>
    public MeasurementResult Measure(ComplexVector state, ComplexMatrix[] povmElements)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (povmElements == null || povmElements.Length == 0) throw new ArgumentException("POVM elements must be provided.", nameof(povmElements));

        double[] probabilities = new double[povmElements.Length];

        for (int i = 0; i < povmElements.Length; i++)
        {
            probabilities[i] = ComputeProbability(state, povmElements[i]);
        }

        int outcome = SampleFromDistribution(probabilities);
        return new MeasurementResult(outcome, probabilities[outcome], null, -1);
    }

    /// <summary>
    /// Performs multiple measurements using POVM elements.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="povmElements">The POVM element matrices.</param>
    /// <param name="shots">The number of measurements to perform.</param>
    /// <returns>An array of measurement outcomes.</returns>
    public int[] MeasureMultiple(ComplexVector state, ComplexMatrix[] povmElements, int shots)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (povmElements == null || povmElements.Length == 0) throw new ArgumentException("POVM elements must be provided.", nameof(povmElements));
        if (shots < 1) throw new ArgumentException("Number of shots must be at least 1.", nameof(shots));

        double[] probabilities = new double[povmElements.Length];
        for (int i = 0; i < povmElements.Length; i++)
        {
            probabilities[i] = ComputeProbability(state, povmElements[i]);
        }

        var outcomes = new int[shots];
        for (int s = 0; s < shots; s++)
        {
            outcomes[s] = SampleFromDistribution(probabilities);
        }

        return outcomes;
    }

    private double ComputeProbability(ComplexVector state, ComplexMatrix povmElement)
    {
        ComplexVector result = povmElement.Multiply(state);
        return Complex.Conjugate(state.InnerProduct(result)).Real;
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
