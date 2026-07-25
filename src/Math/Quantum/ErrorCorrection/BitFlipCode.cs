namespace MathVerse.Math.Quantum.ErrorCorrection;

using Circuits;
using Gates;

/// <summary>
/// Three-qubit bit flip code. Encodes a single logical qubit into three physical qubits
/// and can detect and correct single bit-flip (X) errors.
/// </summary>
public static class BitFlipCode
{
    /// <summary>Creates the encoding circuit for the 3-qubit bit flip code.
    /// Encodes |ψ⟩ = α|0⟩ + β|1⟩ → α|000⟩ + β|111⟩.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that performs the encoding.</returns>
    public static QuantumCircuit EncodingCircuit()
    {
        var circuit = new QuantumCircuit(3);
        circuit.AddGate(MultiQubitGates.CX, 0, 1);
        circuit.AddGate(MultiQubitGates.CX, 0, 2);
        return circuit;
    }

    /// <summary>Creates the syndrome measurement circuit for detecting bit flip errors.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that measures X-syndrome parity checks.</returns>
    public static QuantumCircuit SyndromeCircuit()
    {
        var circuit = new QuantumCircuit(5);
        circuit.AddGate(MultiQubitGates.CX, 0, 3);
        circuit.AddGate(MultiQubitGates.CX, 1, 3);
        circuit.AddGate(MultiQubitGates.CX, 1, 4);
        circuit.AddGate(MultiQubitGates.CX, 2, 4);
        return circuit;
    }

    /// <summary>Decodes syndrome bits to identify which qubit has a bit flip error.</summary>
    /// <param name="syndromeBits">Two syndrome bits [s0, s1].</param>
    /// <returns>The index (0, 1, or 2) of the qubit with the error, or -1 if no error detected.</returns>
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

    /// <summary>Creates the correction circuit that applies the appropriate X gate.</summary>
    /// <returns>A <see cref="QuantumCircuit"/> that corrects the detected error.</returns>
    public static QuantumCircuit CorrectionCircuit()
    {
        var circuit = new QuantumCircuit(5);
        circuit.AddGate(MultiQubitGates.CX, 3, 0);
        circuit.AddGate(MultiQubitGates.CX, 4, 1);
        circuit.AddGate(MultiQubitGates.CCX, 3, 4, 2);
        return circuit;
    }
}
