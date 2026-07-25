namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Phase flip noise channel. Applies the Pauli-Z error with probability p.
/// Kraus operators: K₀ = √(1-p) I, K₁ = √p Z.
/// </summary>
public static class PhaseFlipNoise
{
    /// <summary>Creates a phase flip noise channel.</summary>
    /// <param name="errorRate">The probability p ∈ [0, 1] of a phase flip error.</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing phase flip noise.</returns>
    public static NoiseChannel Create(double errorRate)
    {
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));

        double sqrtP = System.Math.Sqrt(errorRate);
        double sqrtQ = System.Math.Sqrt(1.0 - errorRate);

        var k0 = ComplexMatrix.Identity(2).Scale(new Complex(sqrtQ, 0.0));

        var zData = new Complex[2, 2];
        zData[0, 0] = Complex.One;
        zData[1, 1] = -Complex.One;
        var zMatrix = new ComplexMatrix(zData);
        var k1 = zMatrix.Scale(new Complex(sqrtP, 0.0));

        return new NoiseChannel(new[] { k0, k1 });
    }
}
