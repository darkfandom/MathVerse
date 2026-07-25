namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Phase damping channel modeling T2 dephasing (pure dephasing).
/// Derived from amplitude damping via transformation into the X basis.
/// Kraus operators: K₀ = [[1,0],[0,√(1-λ)]], K₁ = [[0,0],[0,√λ]],
/// where λ is the damping rate.
/// </summary>
public static class PhaseDamping
{
    /// <summary>Creates a phase damping noise channel.</summary>
    /// <param name="dampingRate">The phase damping rate λ ∈ [0, 1].</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing phase damping (dephasing).</returns>
    public static NoiseChannel Create(double dampingRate)
    {
        if (dampingRate < 0.0 || dampingRate > 1.0) throw new ArgumentOutOfRangeException(nameof(dampingRate));

        double sqrtLambda = System.Math.Sqrt(dampingRate);
        double sqrtOneMinusLambda = System.Math.Sqrt(1.0 - dampingRate);

        var k0Data = new Complex[2, 2];
        k0Data[0, 0] = Complex.One;
        k0Data[1, 1] = new Complex(sqrtOneMinusLambda, 0.0);
        var k0 = new ComplexMatrix(k0Data);

        var k1Data = new Complex[2, 2];
        k1Data[1, 1] = new Complex(sqrtLambda, 0.0);
        var k1 = new ComplexMatrix(k1Data);

        return new NoiseChannel(new[] { k0, k1 });
    }
}
