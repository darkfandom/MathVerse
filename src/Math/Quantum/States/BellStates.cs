namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Factory class for creating the four Bell states (maximally entangled 2-qubit states).
/// </summary>
public static class BellStates
{
    private static readonly Complex InvSqrt2 = new Complex(1.0 / System.Math.Sqrt(2.0), 0.0);

    /// <summary>Creates the |Phi+> = (|00> + |11>)/sqrt(2) state.</summary>
    public static StateVector PhiPlus()
    {
        var amps = new Complex[4];
        amps[0] = InvSqrt2;
        amps[3] = InvSqrt2;
        return new StateVector(amps);
    }

    /// <summary>Creates the |Phi-> = (|00> - |11>)/sqrt(2) state.</summary>
    public static StateVector PhiMinus()
    {
        var amps = new Complex[4];
        amps[0] = InvSqrt2;
        amps[3] = -InvSqrt2;
        return new StateVector(amps);
    }

    /// <summary>Creates the |Psi+> = (|01> + |10>)/sqrt(2) state.</summary>
    public static StateVector PsiPlus()
    {
        var amps = new Complex[4];
        amps[1] = InvSqrt2;
        amps[2] = InvSqrt2;
        return new StateVector(amps);
    }

    /// <summary>Creates the |Psi-> = (|01> - |10>)/sqrt(2) state.</summary>
    public static StateVector PsiMinus()
    {
        var amps = new Complex[4];
        amps[1] = InvSqrt2;
        amps[2] = -InvSqrt2;
        return new StateVector(amps);
    }

    /// <summary>Creates a Bell state by name: "Phi+", "Phi-", "Psi+", or "Psi-".</summary>
    public static StateVector Create(string type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return type switch
        {
            "Phi+" or "\u03A6+" => PhiPlus(),
            "Phi-" or "\u03A6-" => PhiMinus(),
            "Psi+" or "\u03A8+" => PsiPlus(),
            "Psi-" or "\u03A8-" => PsiMinus(),
            _ => throw new ArgumentException($"Unknown Bell state type: '{type}'. Use Phi+, Phi-, Psi+, or Psi-.", nameof(type))
        };
    }
}
