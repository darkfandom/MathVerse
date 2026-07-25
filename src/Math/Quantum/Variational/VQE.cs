namespace MathVerse.Math.Quantum.Variational;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements the Variational Quantum Eigensolver (VQE) for finding the ground-state
/// energy of a Hamiltonian using a parameterized quantum circuit and classical optimization.
/// </summary>
public sealed class VQE
{
    private readonly QuantumCircuit _variationalCircuit;
    private readonly ComplexMatrix _hamiltonian;
    private readonly int _numQubits;

    /// <summary>Gets the variational circuit ansatz.</summary>
    public QuantumCircuit VariationalCircuit => _variationalCircuit;

    /// <summary>Gets the Hamiltonian matrix.</summary>
    public ComplexMatrix Hamiltonian => _hamiltonian;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a VQE instance.</summary>
    /// <param name="variationalCircuit">The parameterized variational circuit.</param>
    /// <param name="hamiltonian">The Hamiltonian whose ground-state energy is sought.</param>
    /// <param name="numQubits">The number of qubits.</param>
    public VQE(QuantumCircuit variationalCircuit, ComplexMatrix hamiltonian, int numQubits)
    {
        _variationalCircuit = variationalCircuit ?? throw new ArgumentNullException(nameof(variationalCircuit));
        _hamiltonian = hamiltonian ?? throw new ArgumentNullException(nameof(hamiltonian));
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
    }

    /// <summary>
    /// Computes the expectation value ⟨ψ(θ)|H|ψ(θ)⟩ for the given parameters.
    /// </summary>
    /// <param name="parameters">The circuit parameters.</param>
    /// <returns>The expectation value (energy).</returns>
    public double ComputeExpectationValue(double[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        int stateDim = 1 << _numQubits;
        var state = new Complex[stateDim];
        state[0] = Complex.One;

        for (int q = 0; q < _numQubits; q++)
            Gates.SingleQubitGates.Hadamard.Apply(state, new[] { q }, _numQubits);

        foreach (var circuitGate in _variationalCircuit.Gates)
        {
            if (circuitGate.Gate.Name == "M") continue;
            circuitGate.Gate.Apply(state, circuitGate.QubitIndices, _numQubits);
        }

        double energy = 0.0;
        for (int i = 0; i < stateDim; i++)
        {
            for (int j = 0; j < stateDim; j++)
            {
                energy += (Complex.Conjugate(state[i]) * _hamiltonian[i, j] * state[j]).Real;
            }
        }
        return energy;
    }

    /// <summary>
    /// Optimizes the VQE parameters using a simple parameter search.
    /// </summary>
    /// <param name="initialParameters">Initial parameter values.</param>
    /// <param name="maxIterations">Maximum optimization iterations.</param>
    /// <returns>The optimization result.</returns>
    public VQEResult Optimize(double[] initialParameters, int maxIterations = 100)
    {
        if (initialParameters == null) throw new ArgumentNullException(nameof(initialParameters));

        var rng = new Random(42);
        var current = (double[])initialParameters.Clone();
        double currentEnergy = ComputeExpectationValue(current);
        var bestParams = (double[])current.Clone();
        double bestEnergy = currentEnergy;
        int iter;
        double lr = 0.1;

        for (iter = 0; iter < maxIterations; iter++)
        {
            var gradient = ParameterShiftGradient.ComputeGradient(ComputeExpectationValue, current);

            for (int i = 0; i < current.Length; i++)
                current[i] -= lr * gradient[i];

            currentEnergy = ComputeExpectationValue(current);

            if (currentEnergy < bestEnergy)
            {
                bestEnergy = currentEnergy;
                bestParams = (double[])current.Clone();
            }
        }

        bool converged = System.Math.Abs(bestEnergy - currentEnergy) < 1e-6;
        return new VQEResult(bestEnergy, bestParams, iter, converged);
    }
}

/// <summary>
/// Represents the result of a VQE optimization.
/// </summary>
public sealed class VQEResult
{
    /// <summary>Gets the optimal energy found.</summary>
    public double OptimalEnergy { get; }

    /// <summary>Gets the optimal parameter values.</summary>
    public double[] OptimalParameters { get; }

    /// <summary>Gets the number of iterations performed.</summary>
    public int Iterations { get; }

    /// <summary>Gets whether the optimization converged.</summary>
    public bool Converged { get; }

    /// <summary>Creates a VQE result.</summary>
    public VQEResult(double optimalEnergy, double[] optimalParameters, int iterations, bool converged)
    {
        OptimalEnergy = optimalEnergy;
        OptimalParameters = optimalParameters ?? throw new ArgumentNullException(nameof(optimalParameters));
        Iterations = iterations;
        Converged = converged;
    }
}
