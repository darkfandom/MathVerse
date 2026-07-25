namespace MathVerse.Math.Quantum.ErrorCorrection;

using System;
using Circuits;
using Gates;

/// <summary>
/// Nine-qubit Shor code. The first quantum error-correcting code, capable of correcting
/// arbitrary single-qubit errors by concatenating the 3-qubit bit flip and phase flip codes.
/// </summary>
public static class ShorCode
{
    /// <summary>The number of physical qubits in the Shor code.</summary>
    public const int PhysicalQubits = 9;

    /// <summary>Creates the encoding circuit for the 9-qubit Shor code.
    /// Encodes one logical qubit into nine physical qubits.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that performs the Shor encoding.</returns>
    public static QuantumCircuit EncodingCircuit()
    {
        var circuit = new QuantumCircuit(9);

        circuit.AddGate(SingleQubitGates.Hadamard, 0);

        circuit.AddGate(MultiQubitGates.CX, 0, 3);
        circuit.AddGate(MultiQubitGates.CX, 0, 6);

        circuit.AddGate(MultiQubitGates.CX, 0, 1);
        circuit.AddGate(MultiQubitGates.CX, 3, 4);
        circuit.AddGate(MultiQubitGates.CX, 6, 7);

        circuit.AddGate(MultiQubitGates.CX, 0, 2);
        circuit.AddGate(MultiQubitGates.CX, 3, 5);
        circuit.AddGate(MultiQubitGates.CX, 6, 8);

        return circuit;
    }

    /// <summary>Creates the syndrome measurement circuit for the Shor code.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that extracts syndrome information.</returns>
    public static QuantumCircuit SyndromeCircuit()
    {
        var circuit = new QuantumCircuit(9 + 8);

        circuit.AddGate(MultiQubitGates.CX, 0, 9);
        circuit.AddGate(MultiQubitGates.CX, 1, 9);
        circuit.AddGate(MultiQubitGates.CX, 1, 10);
        circuit.AddGate(MultiQubitGates.CX, 2, 10);

        circuit.AddGate(MultiQubitGates.CX, 3, 11);
        circuit.AddGate(MultiQubitGates.CX, 4, 11);
        circuit.AddGate(MultiQubitGates.CX, 4, 12);
        circuit.AddGate(MultiQubitGates.CX, 5, 12);

        circuit.AddGate(MultiQubitGates.CX, 6, 13);
        circuit.AddGate(MultiQubitGates.CX, 7, 13);
        circuit.AddGate(MultiQubitGates.CX, 7, 14);
        circuit.AddGate(MultiQubitGates.CX, 8, 14);

        circuit.AddGate(SingleQubitGates.Hadamard, 9);
        circuit.AddGate(SingleQubitGates.Hadamard, 11);
        circuit.AddGate(SingleQubitGates.Hadamard, 13);
        circuit.AddGate(MultiQubitGates.CX, 9, 15);
        circuit.AddGate(MultiQubitGates.CX, 10, 15);
        circuit.AddGate(MultiQubitGates.CX, 11, 16);
        circuit.AddGate(MultiQubitGates.CX, 12, 16);
        circuit.AddGate(MultiQubitGates.CX, 13, 17);
        circuit.AddGate(MultiQubitGates.CX, 14, 17);

        return circuit;
    }

    /// <summary>Decodes syndrome bits to identify which qubit has an error.</summary>
    /// <param name="syndromeBits">Eight syndrome bits from the Shor code syndrome extraction.</param>
    /// <returns>Array with the index (0–8) of the errored qubit, or -1 if no error detected.</returns>
    public static int[] DecodeSyndrome(int[] syndromeBits)
    {
        if (syndromeBits == null || syndromeBits.Length < 8)
            throw new ArgumentException("Shor code syndrome must have at least 8 bits.", nameof(syndromeBits));

        int bf0 = syndromeBits[0] ^ syndromeBits[1];
        int bf1 = syndromeBits[2] ^ syndromeBits[3];
        int bf2 = syndromeBits[4] ^ syndromeBits[5];
        int pf0 = syndromeBits[6] ^ syndromeBits[7];

        if (bf0 == 0 && bf1 == 0 && bf2 == 0)
            return new[] { -1 };

        int groupIdx = -1;
        if (bf0 == 1) groupIdx = 0;
        else if (bf1 == 1) groupIdx = 1;
        else if (bf2 == 1) groupIdx = 2;

        int bitIdx = pf0;
        int qubitIdx = groupIdx * 3 + bitIdx;
        return new[] { qubitIdx };
    }
}
