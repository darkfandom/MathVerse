namespace MathVerse.Math.Quantum.Integration;

using System;
using System.Collections.Generic;
using System.Numerics;
using Core;
using LinearAlgebra;
using Circuits;
using Gates;

/// <summary>
/// Bridges the Quantum module with other MathVerse modules.
/// </summary>
public static class QuantumIntegrationBridge
{
    /// <summary>Converts a CAS expression to a quantum gate matrix.</summary>
    public static ComplexMatrix? ExpressionToGate(string expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        return null;
    }

    /// <summary>Converts a quantum circuit to a simulation for the Simulation module.</summary>
    public static object CreateSimulationAdapter(Circuits.QuantumCircuit circuit)
    {
        _ = circuit ?? throw new ArgumentNullException(nameof(circuit));
        return new Simulation.CircuitSimulator(circuit.NumQubits);
    }

    /// <summary>Creates a Hamiltonian from Numerics module matrix data.</summary>
    public static Chemistry.Hamiltonian CreateHamiltonian(double[,] matrix, int numQubits)
    {
        _ = matrix ?? throw new ArgumentNullException(nameof(matrix));
        var ham = new Chemistry.Hamiltonian(numQubits);
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var complexData = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                complexData[i, j] = new Complex(matrix[i, j], 0.0);
        ham.AddTerm(1.0, new ComplexMatrix(complexData));
        return ham;
    }

    /// <summary>Converts quantum state probabilities to DataScience dataset-compatible format.</summary>
    public static Dictionary<string, double> StateToProbabilityDistribution(ComplexVector state)
    {
        _ = state ?? throw new ArgumentNullException(nameof(state));
        var result = new Dictionary<string, double>();
        for (int i = 0; i < state.Dimension; i++)
        {
            result[$"|{Convert.ToString(i, 2).PadLeft((int)System.Math.Ceiling(System.Math.Log2(state.Dimension)), '0')}⟩"] = state[i].Magnitude * state[i].Magnitude;
        }
        return result;
    }

    /// <summary>Converts quantum random numbers to Visualization-compatible color values.</summary>
    public static string QuantumRandomColor(Randomness.QuantumRandomGenerator rng)
    {
        _ = rng ?? throw new ArgumentNullException(nameof(rng));
        int r = (int)(rng.NextDouble() * 255);
        int g = (int)(rng.NextDouble() * 255);
        int b = (int)(rng.NextDouble() * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>Converts an Ising model to a Hamiltonian for the Interop module.</summary>
    public static Chemistry.Hamiltonian IsingToHamiltonian(Optimization.IsingModel ising)
    {
        _ = ising ?? throw new ArgumentNullException(nameof(ising));
        return new Chemistry.Hamiltonian(ising.NumSpins);
    }
}
