namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Represents a single qubit with complex amplitude coefficients α|0⟩ + β|1⟩.
/// </summary>
public sealed class Qubit
{
    /// <summary>Gets the qubit index in the register.</summary>
    public int Index { get; }

    /// <summary>Gets the amplitude for |0⟩.</summary>
    public Complex Alpha { get; }

    /// <summary>Gets the amplitude for |1⟩.</summary>
    public Complex Beta { get; }

    /// <summary>Creates a qubit with the specified amplitudes.</summary>
    public Qubit(int index, Complex alpha, Complex beta)
    {
        Index = index;
        Alpha = alpha;
        Beta = beta;
    }

    /// <summary>Returns the probability of measuring |0⟩.</summary>
    public double Probability0()
    {
        return Alpha.Magnitude * Alpha.Magnitude;
    }

    /// <summary>Returns the probability of measuring |1⟩.</summary>
    public double Probability1()
    {
        return Beta.Magnitude * Beta.Magnitude;
    }

    /// <summary>Returns true if the qubit state is normalized (|α|² + |β|² ≈ 1).</summary>
    public bool IsNormalized()
    {
        double sum = Probability0() + Probability1();
        return System.Math.Abs(sum - 1.0) < 1e-10;
    }

    /// <summary>Returns a normalized copy of this qubit.</summary>
    public Qubit Normalize()
    {
        double norm = System.Math.Sqrt(Probability0() + Probability1());
        if (norm < 1e-15) throw new InvalidOperationException("Cannot normalize a zero qubit state.");
        return new Qubit(Index, Alpha / norm, Beta / norm);
    }

    /// <summary>Returns a deep copy of this qubit.</summary>
    public Qubit Clone()
    {
        return new Qubit(Index, Alpha, Beta);
    }

    /// <summary>Creates a qubit in the |0⟩ state.</summary>
    public static Qubit Zero(int index) => new Qubit(index, Complex.One, Complex.Zero);

    /// <summary>Creates a qubit in the |1⟩ state.</summary>
    public static Qubit One(int index) => new Qubit(index, Complex.Zero, Complex.One);

    /// <summary>Creates a qubit in the |+⟩ = (|0⟩+|1⟩)/√2 state.</summary>
    public static Qubit Plus(int index)
    {
        double inv = 1.0 / System.Math.Sqrt(2.0);
        return new Qubit(index, new Complex(inv, 0), new Complex(inv, 0));
    }

    /// <summary>Creates a qubit in the |−⟩ = (|0⟩−|1⟩)/√2 state.</summary>
    public static Qubit Minus(int index)
    {
        double inv = 1.0 / System.Math.Sqrt(2.0);
        return new Qubit(index, new Complex(inv, 0), new Complex(-inv, 0));
    }
}
