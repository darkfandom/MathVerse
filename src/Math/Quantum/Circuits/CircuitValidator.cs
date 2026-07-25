namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;

/// <summary>
/// Validates quantum circuits for correctness.
/// </summary>
public static class CircuitValidator
{
    /// <summary>
    /// Validates a quantum circuit and returns a validation result.
    /// </summary>
    /// <param name="circuit">The circuit to validate.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult Validate(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var gate in circuit.Gates)
        {
            foreach (int qubit in gate.QubitIndices)
            {
                if (!IsValidQubitIndex(qubit, circuit.NumQubits))
                {
                    errors.Add($"Qubit index {qubit} is out of range for {circuit.NumQubits}-qubit circuit.");
                }
            }

            if (!AreQubitsDistinct(gate.QubitIndices))
            {
                errors.Add($"Gate {gate.Gate.Name} has duplicate qubit indices.");
            }

            if (gate.QubitIndices.Length != gate.Gate.NumQubits)
            {
                errors.Add($"Gate {gate.Gate.Name} expects {gate.Gate.NumQubits} qubits but was given {gate.QubitIndices.Length}.");
            }
        }

        if (circuit.GateCount == 0)
        {
            warnings.Add("Circuit has no gates.");
        }

        return new ValidationResult(errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// Checks if a qubit index is valid for the given number of qubits.
    /// </summary>
    /// <param name="index">The qubit index.</param>
    /// <param name="numQubits">The total number of qubits.</param>
    /// <returns>True if the index is valid; otherwise, false.</returns>
    public static bool IsValidQubitIndex(int index, int numQubits)
    {
        return index >= 0 && index < numQubits;
    }

    /// <summary>
    /// Checks if all qubit indices are distinct.
    /// </summary>
    /// <param name="indices">The qubit indices to check.</param>
    /// <returns>True if all indices are distinct; otherwise, false.</returns>
    public static bool AreQubitsDistinct(int[] indices)
    {
        if (indices == null || indices.Length <= 1) return true;
        var seen = new HashSet<int>();
        foreach (int idx in indices)
        {
            if (!seen.Add(idx)) return false;
        }
        return true;
    }
}

/// <summary>
/// Represents the result of circuit validation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the circuit is valid.</param>
    /// <param name="errors">The list of errors.</param>
    /// <param name="warnings">The list of warnings.</param>
    public ValidationResult(bool isValid, List<string> errors, List<string> warnings)
    {
        IsValid = isValid;
        Errors = errors;
        Warnings = warnings;
    }

    /// <summary>Gets whether the circuit is valid.</summary>
    public bool IsValid { get; }

    /// <summary>Gets the list of validation errors.</summary>
    public List<string> Errors { get; }

    /// <summary>Gets the list of validation warnings.</summary>
    public List<string> Warnings { get; }
}
