namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;

/// <summary>
/// Measurement/readout noise model that flips classical measurement outcomes
/// with a specified probability, independent of the quantum state.
/// </summary>
public static class ReadoutNoise
{
    /// <summary>Creates a readout noise channel that flips measurement outcomes.</summary>
    /// <param name="errorRate">The probability p ∈ [0, 1] of flipping a measurement outcome.</param>
    /// <returns>A <see cref="NoiseChannel"/> that classically flips measurement results.</returns>
    public static NoiseChannel Create(double errorRate)
    {
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));

        double sqrtP = System.Math.Sqrt(errorRate);
        double sqrtQ = System.Math.Sqrt(1.0 - errorRate);

        var k0 = LinearAlgebra.ComplexMatrix.Identity(2).Scale(new Complex(sqrtQ, 0.0));

        var xData = new Complex[2, 2];
        xData[0, 1] = Complex.One;
        xData[1, 0] = Complex.One;
        var k1 = new LinearAlgebra.ComplexMatrix(xData).Scale(new Complex(sqrtP, 0.0));

        return new NoiseChannel(new[] { k0, k1 });
    }

    /// <summary>Applies readout noise to a measurement outcome by probabilistically flipping it.</summary>
    /// <param name="outcome">The original measurement outcome (0 or 1).</param>
    /// <param name="errorRate">The probability p ∈ [0, 1] of flipping the outcome.</param>
    /// <param name="random">An optional <see cref="Random"/> instance. If null, a new instance is created.</param>
    /// <returns>The potentially flipped measurement outcome.</returns>
    public static int ApplyReadoutNoise(int outcome, double errorRate, Random? random = null)
    {
        if (outcome != 0 && outcome != 1) throw new ArgumentOutOfRangeException(nameof(outcome), "Outcome must be 0 or 1.");
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));

        Random rng = random ?? new Random();
        if (rng.NextDouble() < errorRate)
            return 1 - outcome;
        return outcome;
    }
}
