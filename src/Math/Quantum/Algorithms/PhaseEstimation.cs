namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements Quantum Phase Estimation (QPE) for estimating the phase θ of an eigenvalue
/// e^{2πiθ} of a unitary operator U given an eigenstate |ψ⟩.
/// </summary>
public static class PhaseEstimation
{
    /// <summary>
    /// Runs QPE to estimate the phase of the eigenvalue of the given unitary and eigenstate.
    /// </summary>
    /// <param name="unitary">The unitary matrix U such that U|ψ⟩ = e^{2πiθ}|ψ⟩.</param>
    /// <param name="eigenstate">The eigenstate |ψ⟩.</param>
    /// <param name="precisionQubits">The number of precision qubits.</param>
    /// <returns>The estimated phase θ ∈ [0,1).</returns>
    public static double Run(ComplexMatrix unitary, ComplexVector eigenstate, int precisionQubits)
    {
        if (unitary == null) throw new ArgumentNullException(nameof(unitary));
        if (eigenstate == null) throw new ArgumentNullException(nameof(eigenstate));
        if (precisionQubits < 1) throw new ArgumentOutOfRangeException(nameof(precisionQubits));
        if (unitary.Rows != unitary.Cols)
            throw new ArgumentException("Unitary matrix must be square.", nameof(unitary));
        if (unitary.Rows != eigenstate.Dimension)
            throw new ArgumentException("Eigenstate dimension must match unitary dimension.");

        int eigenDim = eigenstate.Dimension;
        int totalQubits = precisionQubits + (int)System.Math.Log2(eigenDim);
        int totalStateSize = 1 << totalQubits;
        var state = new Complex[totalStateSize];

        state[0] = Complex.One;

        for (int q = 0; q < precisionQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        for (int q = 0; q < precisionQubits; q++)
        {
            int power = 1 << q;
            ApplyControlledPower(unitary, state, q, precisionQubits, eigenDim, power, totalQubits);
        }

        ApplyInverseQFT(state, precisionQubits, totalQubits);

        int measured = 0;
        for (int q = 0; q < precisionQubits; q++)
        {
            double probOne = 0.0;
            int mask = 1 << q;
            for (int i = 0; i < totalStateSize; i++)
            {
                if ((i & mask) != 0)
                    probOne += state[i].Magnitude * state[i].Magnitude;
            }
            if (probOne > 0.5)
                measured |= 1 << q;
        }

        return (double)measured / (double)(1 << precisionQubits);
    }

    /// <summary>
    /// Builds the QPE circuit for a given unitary matrix.
    /// </summary>
    /// <param name="unitary">The unitary matrix to estimate the phase of.</param>
    /// <param name="precisionQubits">The number of precision qubits.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing QPE.</returns>
    public static QuantumCircuit BuildCircuit(ComplexMatrix unitary, int precisionQubits)
    {
        if (unitary == null) throw new ArgumentNullException(nameof(unitary));
        if (precisionQubits < 1) throw new ArgumentOutOfRangeException(nameof(precisionQubits));
        if (unitary.Rows != unitary.Cols)
            throw new ArgumentException("Unitary matrix must be square.", nameof(unitary));

        int eigenDim = unitary.Rows;
        int eigenQubits = (int)System.Math.Log2(eigenDim);
        int totalQubits = precisionQubits + eigenQubits;
        var circuit = new QuantumCircuit(totalQubits);

        for (int q = 0; q < precisionQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        int eigenDim2 = unitary.Rows;
        int eigenQubits2 = (int)System.Math.Log2(eigenDim2);
        Complex[,] unitaryData = new Complex[eigenDim2, eigenDim2];
        for (int i = 0; i < eigenDim2; i++)
            for (int j = 0; j < eigenDim2; j++)
                unitaryData[i, j] = unitary[i, j];
        var synthesized = GateSynthesis.Synthesize(unitaryData, eigenQubits2);
        var baseUnitaryGate = synthesized[0];

        for (int q = 0; q < precisionQubits; q++)
        {
            int power = 1 << q;
            var poweredGate = ParameterizedGates.Power(baseUnitaryGate, power);
            var controlledGate = ControlledGateFactory.CreateControlled(poweredGate, 1);
            int[] qubitIndices = new int[1 + eigenQubits2];
            qubitIndices[0] = q;
            for (int e = 0; e < eigenQubits2; e++)
                qubitIndices[1 + e] = precisionQubits + e;
            circuit.AddGate(controlledGate, qubitIndices);
        }

        for (int i = 0; i < precisionQubits / 2; i++)
            circuit.AddGate(MultiQubitGates.Swap, i, precisionQubits - 1 - i);

        for (int j = precisionQubits - 1; j >= 0; j--)
        {
            for (int k = precisionQubits - 1; k > j; k--)
            {
                double angle = -System.Math.PI / (double)(1 << (k - j));
                circuit.AddGate(ParameterizedGates.ControlledPhase(angle), k, j);
            }
            circuit.AddGate(SingleQubitGates.Hadamard, j);
        }

        return circuit;
    }

    private static void ApplyControlledPower(ComplexMatrix unitary, Complex[] state, int controlQubit, int precisionQubits, int eigenDim, int power, int totalQubits)
    {
        int eigenQubits = totalQubits - precisionQubits;
        int eigenMask = eigenDim - 1;
        int controlMask = 1 << controlQubit;

        var poweredUnitary = MatrixPower(unitary, power);

        for (int i = 0; i < state.Length; i++)
        {
            if ((i & controlMask) == 0) continue;
            int eigenIndex = 0;
            for (int q = 0; q < eigenQubits; q++)
            {
                int bit = (i >> (precisionQubits + q)) & 1;
                eigenIndex |= bit << q;
            }
            if (eigenIndex >= eigenDim) continue;

            int baseIndex = i & ~((eigenDim - 1) << precisionQubits);
            for (int j = 0; j < eigenDim; j++)
            {
                Complex coeff = poweredUnitary[j, eigenIndex];
                if (coeff == Complex.Zero) continue;
                int targetIndex = baseIndex | (j << precisionQubits);
                state[targetIndex] += coeff * state[i];
            }
        }

        for (int i = 0; i < state.Length; i++)
        {
            if ((i & controlMask) != 0) continue;
            bool hasEigenBits = false;
            for (int q = 0; q < eigenQubits; q++)
            {
                if ((i & (1 << (precisionQubits + q))) != 0) { hasEigenBits = true; break; }
            }
            if (!hasEigenBits) continue;
            state[i] = Complex.Zero;
        }
    }

    private static ComplexMatrix MatrixPower(ComplexMatrix m, int power)
    {
        int n = m.Rows;
        if (power == 0) return ComplexMatrix.Identity(n);
        if (power == 1) return m;

        var result = ComplexMatrix.Identity(n);
        var baseMatrix = m;

        int p = power;
        while (p > 0)
        {
            if ((p & 1) == 1)
                result = result.Multiply(baseMatrix);
            baseMatrix = baseMatrix.Multiply(baseMatrix);
            p >>= 1;
        }
        return result;
    }

    private static void ApplyInverseQFT(Complex[] state, int precisionQubits, int totalQubits)
    {
        for (int i = 0; i < precisionQubits / 2; i++)
            MultiQubitGates.Swap.Apply(state, new[] { i, precisionQubits - 1 - i }, totalQubits);

        for (int j = 0; j < precisionQubits; j++)
        {
            SingleQubitGates.Hadamard.Apply(state, new[] { j }, totalQubits);
            for (int k = j + 1; k < precisionQubits; k++)
            {
                double angle = System.Math.PI / (double)(1 << (k - j));
                var phase = Complex.FromPolarCoordinates(1.0, -angle);
                int controlMask = 1 << k;
                int targetMask = 1 << j;
                for (int idx = 0; idx < (1 << totalQubits); idx++)
                {
                    if ((idx & controlMask) != 0 && (idx & targetMask) != 0)
                        state[idx] *= phase;
                }
            }
        }
    }
}
