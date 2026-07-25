namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Collections.Generic;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Result of a quantum measurement.
/// </summary>
public sealed class MeasurementResult
{
    /// <summary>Gets the measured outcome (bit value).</summary>
    public int Outcome { get; }

    /// <summary>Gets the probability of this outcome.</summary>
    public double Probability { get; }

    /// <summary>Gets the post-measurement state.</summary>
    public ComplexVector? PostMeasurementState { get; }

    /// <summary>Gets the qubit index that was measured.</summary>
    public int QubitIndex { get; }

    /// <summary>Gets all sampled outcomes.</summary>
    public IReadOnlyList<int> SampledOutcomes { get; }

    /// <summary>
    /// Creates a single measurement result.
    /// </summary>
    /// <param name="outcome">The measured outcome.</param>
    /// <param name="probability">The probability of the outcome.</param>
    /// <param name="postState">The post-measurement state.</param>
    /// <param name="qubitIndex">The qubit index that was measured.</param>
    public MeasurementResult(int outcome, double probability, ComplexVector? postState, int qubitIndex)
    {
        Outcome = outcome;
        Probability = probability;
        PostMeasurementState = postState;
        QubitIndex = qubitIndex;
        SampledOutcomes = new[] { outcome };
    }

    /// <summary>
    /// Creates a measurement result with multiple samples.
    /// </summary>
    /// <param name="outcomes">The sampled outcomes.</param>
    /// <param name="qubitIndex">The qubit index that was measured.</param>
    public MeasurementResult(IReadOnlyList<int> outcomes, int qubitIndex)
    {
        SampledOutcomes = outcomes;
        QubitIndex = qubitIndex;
        Outcome = outcomes.Count > 0 ? outcomes[^1] : 0;
        Probability = 1.0;
    }
}
