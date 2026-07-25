namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Bit flip noise channel. Applies the Pauli-X error with probability p.
/// Kraus operators: K₀ = √(1-p) I, K₁ = √p X.
/// </summary>
public static class BitFlipNoise
{
    /// <summary>Creates a bit flip noise channel.</summary>
    /// <param name="errorRate">The probability p ∈ [0, 1] of a bit flip error.</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing bit flip noise.</returns>
    public static NoiseChannel Create(double errorRate)
    {
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));

        double sqrtP = System.Math.Sqrt(errorRate);
        double sqrtQ = System.Math.Sqrt(1.0 - errorRate);

        var k0 = ComplexMatrix.Identity(2).Scale(new Complex(sqrtQ, 0.0));

        var xData = new Complex[2, 2];
        xData[0, 1] = Complex.One;
        xData[1, 0] = Complex.One;
        var xMatrix = new ComplexMatrix(xData);
        var k1 = xMatrix.Scale(new Complex(sqrtP, 0.0));

        return new NoiseChannel(new[] { k0, k1 });
    }
}
