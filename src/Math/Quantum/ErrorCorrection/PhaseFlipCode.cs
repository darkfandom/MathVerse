namespace MathVerse.Math.Quantum.ErrorCorrection;

using Circuits;
using Gates;

/// <summary>
/// Three-qubit phase flip code. Encodes a single logical qubit into three physical qubits
/// and can detect and correct single phase-flip (Z) errors by measuring in the X basis.
/// </summary>
public static class PhaseFlipCode
{
    /// <summary>Creates the encoding circuit for the 3-qubit phase flip code.
    /// Encodes |ψ⟩ → α|+++⟩ + β|---⟩ via Hadamard and CNOT.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that performs the encoding.</returns>
    public static QuantumCircuit EncodingCircuit()
    {
        var circuit = new QuantumCircuit(3);
        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        circuit.AddGate(SingleQubitGates.Hadamard, 1);
        circuit.AddGate(SingleQubitGates.Hadamard, 2);
        circuit.AddGate(MultiQubitGates.CX, 0, 1);
        circuit.AddGate(MultiQubitGates.CX, 0, 2);
        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        circuit.AddGate(SingleQubitGates.Hadamard, 1);
        circuit.AddGate(SingleQubitGates.Hadamard, 2);
        return circuit;
    }

    /// <summary>Creates the syndrome measurement circuit in the X basis for detecting phase flip errors.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that measures Z-syndrome parity checks in the X basis.</returns>
    public static QuantumCircuit SyndromeCircuit()
    {
        var circuit = new QuantumCircuit(5);
        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        circuit.AddGate(SingleQubitGates.Hadamard, 1);
        circuit.AddGate(SingleQubitGates.Hadamard, 2);
        circuit.AddGate(MultiQubitGates.CX, 0, 3);
        circuit.AddGate(MultiQubitGates.CX, 1, 3);
        circuit.AddGate(MultiQubitGates.CX, 1, 4);
        circuit.AddGate(MultiQubitGates.CX, 2, 4);
        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        circuit.AddGate(SingleQubitGates.Hadamard, 1);
        circuit.AddGate(SingleQubitGates.Hadamard, 2);
        return circuit;
    }

    /// <summary>Decodes syndrome bits to identify which qubit has a phase flip error.</summary>
    /// <param name="syndromeBits">Two syndrome bits [s0, s1].</param>
    /// <returns>Array with the index (0, 1, or 2) of the qubit with the error, or -1 if no error.</returns>
    public static int[] DecodeSyndrome(int[] syndromeBits)
    {
        if (syndromeBits == null || syndromeBits.Length < 2)
            throw new System.ArgumentException("Syndrome must have at least 2 bits.", nameof(syndromeBits));

        int s0 = syndromeBits[0];
        int s1 = syndromeBits[1];

        if (s0 == 0 && s1 == 0)
            return new[] { -1 };
        if (s0 == 1 && s1 == 0)
            return new[] { 0 };
        if (s0 == 1 && s1 == 1)
            return new[] { 1 };
        if (s0 == 0 && s1 == 1)
            return new[] { 2 };

        return new[] { -1 };
    }
}
