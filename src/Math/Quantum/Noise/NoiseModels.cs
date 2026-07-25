namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Provides factory methods for common single-qubit quantum noise channels.
/// </summary>
public static class NoiseModels
{
    /// <summary>Creates a depolarizing channel: ρ → (1−p)ρ + (p/3)(XρX + YρY + ZρZ).</summary>
    /// <param name="p">The depolarizing probability in [0,1].</param>
    /// <returns>A single-qubit depolarizing noise channel.</returns>
    public static NoiseChannel Depolarizing(double p)
    {
        if (p < 0.0 || p > 1.0) throw new ArgumentOutOfRangeException(nameof(p));
        double sqrt1mp = System.Math.Sqrt(1.0 - p);
        double sqrtP3 = System.Math.Sqrt(p / 3.0);
        var I = ComplexMatrix.Identity(2);
        var X = new ComplexMatrix(new Complex[,] { { 0, 1 }, { 1, 0 } });
        var Y = new ComplexMatrix(new Complex[,] { { 0, new Complex(0, -1) }, { new Complex(0, 1), 0 } });
        var Z = new ComplexMatrix(new Complex[,] { { 1, 0 }, { 0, -1 } });
        return new NoiseChannel(new[] {
            I.Scale(new Complex(sqrt1mp, 0)),
            X.Scale(new Complex(sqrtP3, 0)),
            Y.Scale(new Complex(sqrtP3, 0)),
            Z.Scale(new Complex(sqrtP3, 0))
        });
    }

    /// <summary>Creates a bit-flip channel: ρ → (1−p)ρ + p·XρX.</summary>
    /// <param name="p">The bit-flip probability in [0,1].</param>
    /// <returns>A single-qubit bit-flip noise channel.</returns>
    public static NoiseChannel BitFlip(double p)
    {
        if (p < 0.0 || p > 1.0) throw new ArgumentOutOfRangeException(nameof(p));
        double sqrt1mp = System.Math.Sqrt(1.0 - p);
        double sqrtp = System.Math.Sqrt(p);
        var I = ComplexMatrix.Identity(2);
        var X = new ComplexMatrix(new Complex[,] { { 0, 1 }, { 1, 0 } });
        return new NoiseChannel(new[] {
            I.Scale(new Complex(sqrt1mp, 0)),
            X.Scale(new Complex(sqrtp, 0))
        });
    }

    /// <summary>Creates a phase-flip channel: ρ → (1−p)ρ + p·ZρZ.</summary>
    /// <param name="p">The phase-flip probability in [0,1].</param>
    /// <returns>A single-qubit phase-flip noise channel.</returns>
    public static NoiseChannel PhaseFlip(double p)
    {
        if (p < 0.0 || p > 1.0) throw new ArgumentOutOfRangeException(nameof(p));
        double sqrt1mp = System.Math.Sqrt(1.0 - p);
        double sqrtp = System.Math.Sqrt(p);
        var I = ComplexMatrix.Identity(2);
        var Z = new ComplexMatrix(new Complex[,] { { 1, 0 }, { 0, -1 } });
        return new NoiseChannel(new[] {
            I.Scale(new Complex(sqrt1mp, 0)),
            Z.Scale(new Complex(sqrtp, 0))
        });
    }

    /// <summary>Creates an amplitude damping channel with decay parameter γ.</summary>
    /// <param name="gamma">The damping parameter in [0,1].</param>
    /// <returns>A single-qubit amplitude damping noise channel.</returns>
    public static NoiseChannel AmplitudeDamping(double gamma)
    {
        if (gamma < 0.0 || gamma > 1.0) throw new ArgumentOutOfRangeException(nameof(gamma));
        double sqrtG = System.Math.Sqrt(gamma);
        double sqrt1G = System.Math.Sqrt(1.0 - gamma);
        var K0 = new ComplexMatrix(new Complex[,]
        {
            { 1, 0 },
            { 0, new Complex(sqrt1G, 0) }
        });
        var K1 = new ComplexMatrix(new Complex[,]
        {
            { 0, new Complex(sqrtG, 0) },
            { 0, 0 }
        });
        return new NoiseChannel(new[] { K0, K1 });
    }

    /// <summary>Creates a phase damping channel with decay parameter γ.</summary>
    /// <param name="gamma">The damping parameter in [0,1].</param>
    /// <returns>A single-qubit phase damping noise channel.</returns>
    public static NoiseChannel PhaseDamping(double gamma)
    {
        if (gamma < 0.0 || gamma > 1.0) throw new ArgumentOutOfRangeException(nameof(gamma));
        double sqrt1G = System.Math.Sqrt(1.0 - gamma);
        double sqrtG = System.Math.Sqrt(gamma);
        var K0 = new ComplexMatrix(new Complex[,]
        {
            { 1, 0 },
            { 0, new Complex(sqrt1G, 0) }
        });
        var K1 = new ComplexMatrix(new Complex[,]
        {
            { 0, 0 },
            { 0, new Complex(sqrtG, 0) }
        });
        return new NoiseChannel(new[] { K0, K1 });
    }
}
