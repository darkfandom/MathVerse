namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Provides projective measurement with arbitrary projectors.
/// </summary>
public sealed class ProjectiveMeasurement
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectiveMeasurement"/> class.
    /// </summary>
    public ProjectiveMeasurement()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectiveMeasurement"/> class with a specified random seed.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public ProjectiveMeasurement(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Measures a state using a set of projectors.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="projectors">The projector matrices.</param>
    /// <returns>The measurement result.</returns>
    public MeasurementResult Measure(ComplexVector state, ComplexMatrix[] projectors)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (projectors == null || projectors.Length == 0) throw new ArgumentException("Projectors must be provided.", nameof(projectors));

        double[] probabilities = new double[projectors.Length];

        for (int i = 0; i < projectors.Length; i++)
        {
            probabilities[i] = ComputeProbability(state, projectors[i]);
        }

        int outcome = SampleFromDistribution(probabilities);
        return new MeasurementResult(outcome, probabilities[outcome], null, -1);
    }

    /// <summary>
    /// Computes the expectation value of an observable.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="observable">The observable matrix.</param>
    /// <returns>The expectation value.</returns>
    public double ExpectationValue(ComplexVector state, ComplexMatrix observable)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (observable == null) throw new ArgumentNullException(nameof(observable));

        ComplexVector result = observable.Multiply(state);
        Complex innerProduct = Complex.Conjugate(state.InnerProduct(result));
        return innerProduct.Real;
    }

    private double ComputeProbability(ComplexVector state, ComplexMatrix projector)
    {
        ComplexVector projected = projector.Multiply(state);
        return Complex.Conjugate(state.InnerProduct(projected)).Real;
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
