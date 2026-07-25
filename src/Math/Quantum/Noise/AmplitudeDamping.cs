namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Amplitude damping channel modeling T1 energy relaxation decay.
/// Kraus operators: K₀ = [[1,0],[0,√(1-γ)]], K₁ = [[0,√γ],[0,0]],
/// where γ is the damping rate.
/// </summary>
public static class AmplitudeDamping
{
    /// <summary>Creates an amplitude damping noise channel.</summary>
    /// <param name="dampingRate">The damping rate γ ∈ [0, 1].</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing amplitude damping.</returns>
    public static NoiseChannel Create(double dampingRate)
    {
        if (dampingRate < 0.0 || dampingRate > 1.0) throw new ArgumentOutOfRangeException(nameof(dampingRate));

        double sqrtGamma = System.Math.Sqrt(dampingRate);
        double sqrtOneMinusGamma = System.Math.Sqrt(1.0 - dampingRate);

        var k0Data = new Complex[2, 2];
        k0Data[0, 0] = Complex.One;
        k0Data[1, 1] = new Complex(sqrtOneMinusGamma, 0.0);
        var k0 = new ComplexMatrix(k0Data);

        var k1Data = new Complex[2, 2];
        k1Data[0, 1] = new Complex(sqrtGamma, 0.0);
        var k1 = new ComplexMatrix(k1Data);

        return new NoiseChannel(new[] { k0, k1 });
    }
}
