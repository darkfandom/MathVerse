namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Depolarizing noise channel: ρ → (1 - p) ρ + (p / d) I,
/// where d is the Hilbert space dimension and p is the error probability.
/// </summary>
public static class DepolarizingNoise
{
    /// <summary>Creates a depolarizing noise channel for multiple qubits.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="errorRate">The depolarization probability p ∈ [0, 1].</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing depolarizing noise.</returns>
    public static NoiseChannel Create(int numQubits, double errorRate)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));

        int d = 1 << numQubits;
        return CreateWithDimension(d, errorRate);
    }

    /// <summary>Creates a single-qubit depolarizing noise channel.</summary>
    /// <param name="errorRate">The depolarization probability p ∈ [0, 1].</param>
    /// <returns>A <see cref="NoiseChannel"/> implementing single-qubit depolarizing noise.</returns>
    public static NoiseChannel CreateSingleQubit(double errorRate)
    {
        if (errorRate < 0.0 || errorRate > 1.0) throw new ArgumentOutOfRangeException(nameof(errorRate));
        return CreateWithDimension(2, errorRate);
    }

    private static NoiseChannel CreateWithDimension(int d, double errorRate)
    {
        double p = errorRate;
        double coeffIdentity = 1.0 - p;
        double coeffPauli = p / d;

        var sqrtCoeffIdentity = System.Math.Sqrt(coeffIdentity);
        var sqrtCoeffPauli = System.Math.Sqrt(coeffPauli);

        var identity = ComplexMatrix.Identity(d);
        var identityScaled = identity.Scale(new Complex(sqrtCoeffIdentity, 0.0));

        int numPaulis = d * d;
        var krausList = new ComplexMatrix[numPaulis];
        krausList[0] = identityScaled;

        int idx = 1;
        for (int pauliRow = 0; pauliRow < d; pauliRow++)
        {
            for (int pauliCol = 0; pauliCol < d; pauliCol++)
            {
                if (pauliRow == 0 && pauliCol == 0) continue;
                var pauliMatrix = ComplexMatrix.Zero(d, d);
                var data = new Complex[d, d];
                data[pauliRow, pauliCol] = Complex.One;
                pauliMatrix = new ComplexMatrix(data);
                krausList[idx++] = pauliMatrix.Scale(new Complex(sqrtCoeffPauli, 0.0));
            }
        }

        return new NoiseChannel(krausList);
    }
}
