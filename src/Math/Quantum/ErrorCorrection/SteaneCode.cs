namespace MathVerse.Math.Quantum.ErrorCorrection;

using System;
using Circuits;
using Gates;

/// <summary>
/// Seven-qubit Steane code [[7,1,3]]. A CSS quantum error-correcting code
/// that can correct any single-qubit error, derived from the classical [7,4,3] Hamming code.
/// </summary>
public static class SteaneCode
{
    /// <summary>The number of physical qubits in the Steane code.</summary>
    public const int PhysicalQubits = 7;

    /// <summary>Creates the encoding circuit for the 7-qubit Steane code.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that performs the Steane encoding.</returns>
    public static QuantumCircuit EncodingCircuit()
    {
        var circuit = new QuantumCircuit(7);

        circuit.AddGate(SingleQubitGates.Hadamard, 1);
        circuit.AddGate(SingleQubitGates.Hadamard, 2);
        circuit.AddGate(SingleQubitGates.Hadamard, 4);

        circuit.AddGate(MultiQubitGates.CX, 0, 3);
        circuit.AddGate(MultiQubitGates.CX, 0, 5);
        circuit.AddGate(MultiQubitGates.CX, 0, 6);

        circuit.AddGate(MultiQubitGates.CX, 1, 3);
        circuit.AddGate(MultiQubitGates.CX, 1, 4);
        circuit.AddGate(MultiQubitGates.CX, 1, 6);

        circuit.AddGate(MultiQubitGates.CX, 2, 4);
        circuit.AddGate(MultiQubitGates.CX, 2, 5);
        circuit.AddGate(MultiQubitGates.CX, 2, 6);

        return circuit;
    }

    /// <summary>Creates the syndrome measurement circuit for the Steane code.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that extracts X and Z syndrome bits.</returns>
    public static QuantumCircuit SyndromeCircuit()
    {
        var circuit = new QuantumCircuit(7 + 6);

        circuit.AddGate(MultiQubitGates.CX, 3, 7);
        circuit.AddGate(MultiQubitGates.CX, 5, 7);
        circuit.AddGate(MultiQubitGates.CX, 6, 7);

        circuit.AddGate(MultiQubitGates.CX, 4, 8);
        circuit.AddGate(MultiQubitGates.CX, 5, 8);
        circuit.AddGate(MultiQubitGates.CX, 6, 8);

        circuit.AddGate(MultiQubitGates.CX, 3, 9);
        circuit.AddGate(MultiQubitGates.CX, 4, 9);
        circuit.AddGate(MultiQubitGates.CX, 6, 9);

        circuit.AddGate(MultiQubitGates.CX, 0, 10);
        circuit.AddGate(MultiQubitGates.CX, 2, 10);
        circuit.AddGate(MultiQubitGates.CX, 4, 10);
        circuit.AddGate(MultiQubitGates.CX, 6, 10);

        circuit.AddGate(MultiQubitGates.CX, 0, 11);
        circuit.AddGate(MultiQubitGates.CX, 1, 11);
        circuit.AddGate(MultiQubitGates.CX, 4, 11);
        circuit.AddGate(MultiQubitGates.CX, 5, 11);

        circuit.AddGate(MultiQubitGates.CX, 0, 12);
        circuit.AddGate(MultiQubitGates.CX, 1, 12);
        circuit.AddGate(MultiQubitGates.CX, 2, 12);
        circuit.AddGate(MultiQubitGates.CX, 3, 12);

        return circuit;
    }

    /// <summary>Decodes the 6-bit syndrome of the Steane code into an error qubit index.</summary>
    /// <param name="syndromeBits">Six syndrome bits [z1, z2, z3, x1, x2, x3].</param>
    /// <returns>Array with the index (0–6) of the errored qubit, or -1 if no error detected.</returns>
    public static int[] DecodeSyndrome(int[] syndromeBits)
    {
        if (syndromeBits == null || syndromeBits.Length < 6)
            throw new ArgumentException("Steane code syndrome must have at least 6 bits.", nameof(syndromeBits));

        int z1 = syndromeBits[0], z2 = syndromeBits[1], z3 = syndromeBits[2];
        int x1 = syndromeBits[3], x2 = syndromeBits[4], x3 = syndromeBits[5];

        int zIdx = z1 | (z2 << 1) | (z3 << 2);
        int xIdx = x1 | (x2 << 1) | (x3 << 2);

        if (zIdx == 0 && xIdx == 0)
            return new[] { -1 };

        if (zIdx != 0 && zIdx <= 7)
            return new[] { zIdx - 1 };

        if (xIdx != 0 && xIdx <= 7)
            return new[] { xIdx - 1 };

        return new[] { -1 };
    }
}
