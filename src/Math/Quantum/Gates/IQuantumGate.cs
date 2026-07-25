namespace MathVerse.Math.Quantum.Gates;

using System.Numerics;

/// <summary>
/// Defines the interface for all quantum gates.
/// </summary>
public interface IQuantumGate
{
    /// <summary>Gets the gate name.</summary>
    string Name { get; }

    /// <summary>Gets the number of qubits this gate acts on.</summary>
    int NumQubits { get; }

    /// <summary>Gets the unitary matrix representation.</summary>
    Complex[,] Matrix { get; }

    /// <summary>Applies this gate to a state vector at the specified qubit indices.</summary>
    /// <param name="stateVector">The state vector to modify.</param>
    /// <param name="qubitIndices">The indices of the qubits this gate acts on.</param>
    /// <param name="totalQubits">The total number of qubits in the system.</param>
    void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits);
}
