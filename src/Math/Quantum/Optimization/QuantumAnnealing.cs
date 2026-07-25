namespace MathVerse.Math.Quantum.Optimization;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Result of a quantum annealing optimization run.
/// </summary>
public sealed class AnnealingResult
{
    /// <summary>Gets the optimal energy found.</summary>
    public double OptimalEnergy { get; init; }

    /// <summary>Gets the optimal spin configuration found.</summary>
    public int[] OptimalConfiguration { get; init; } = Array.Empty<int>();

    /// <summary>Gets the total number of annealing steps performed.</summary>
    public int Steps { get; init; }
}

/// <summary>
/// Simulated quantum annealing optimizer that interpolates between a problem Hamiltonian
/// and a transverse-field driver Hamiltonian to find the ground state configuration.
/// </summary>
public sealed class QuantumAnnealing
{
    private readonly ComplexMatrix _problemHamiltonian;
    private readonly ComplexMatrix _driverHamiltonian;
    private readonly int _numQubits;
    private readonly Random _rng;

    /// <summary>Gets the problem Hamiltonian.</summary>
    public ComplexMatrix ProblemHamiltonian => _problemHamiltonian;

    /// <summary>Gets the driver Hamiltonian.</summary>
    public ComplexMatrix DriverHamiltonian => _driverHamiltonian;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a quantum annealing optimizer.</summary>
    /// <param name="problemHamiltonian">The problem Hamiltonian matrix.</param>
    /// <param name="driverHamiltonian">The driver (mixing) Hamiltonian matrix.</param>
    /// <param name="numQubits">The number of qubits.</param>
    public QuantumAnnealing(ComplexMatrix problemHamiltonian, ComplexMatrix driverHamiltonian, int numQubits)
    {
        _problemHamiltonian = problemHamiltonian ?? throw new ArgumentNullException(nameof(problemHamiltonian));
        _driverHamiltonian = driverHamiltonian ?? throw new ArgumentNullException(nameof(driverHamiltonian));
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _rng = new Random(42);
    }

    /// <summary>Runs simulated quantum annealing.</summary>
    /// <param name="steps">The number of annealing steps.</param>
    /// <param name="initialTemperature">The initial temperature.</param>
    /// <param name="finalTemperature">The final temperature.</param>
    /// <returns>An <see cref="AnnealingResult"/> with the optimal configuration and energy.</returns>
    public AnnealingResult Run(int steps, double initialTemperature, double finalTemperature)
    {
        if (steps < 1) throw new ArgumentOutOfRangeException(nameof(steps));
        if (initialTemperature <= 0.0) throw new ArgumentOutOfRangeException(nameof(initialTemperature));
        if (finalTemperature <= 0.0) throw new ArgumentOutOfRangeException(nameof(finalTemperature));

        int dim = 1 << _numQubits;
        var currentState = new int[_numQubits];
        for (int i = 0; i < _numQubits; i++)
            currentState[i] = _rng.Next(2) == 0 ? 1 : -1;

        double currentEnergy = ComputeEnergy(currentState);
        int[] bestState = (int[])currentState.Clone();
        double bestEnergy = currentEnergy;

        for (int step = 0; step < steps; step++)
        {
            double progress = (double)step / steps;
            double temperature = initialTemperature * System.Math.Pow(finalTemperature / initialTemperature, progress);
            double mixCoeff = 1.0 - progress;

            int qubit = _rng.Next(_numQubits);
            int[] newState = (int[])currentState.Clone();
            newState[qubit] = -newState[qubit];

            double newEnergy = ComputeEnergy(newState);
            double deltaE = newEnergy - currentEnergy;

            if (deltaE < 0.0 || _rng.NextDouble() < System.Math.Exp(-deltaE / System.Math.Max(temperature, 1e-15)))
            {
                currentState = newState;
                currentEnergy = newEnergy;
            }

            if (currentEnergy < bestEnergy)
            {
                bestEnergy = currentEnergy;
                bestState = (int[])currentState.Clone();
            }
        }

        return new AnnealingResult
        {
            OptimalEnergy = bestEnergy,
            OptimalConfiguration = bestState,
            Steps = steps
        };
    }

    private double ComputeEnergy(int[] spins)
    {
        double energy = 0.0;
        int dim = 1 << _numQubits;
        int basisIdx = 0;
        for (int i = 0; i < _numQubits; i++)
        {
            if (spins[i] == 1)
                basisIdx |= (1 << i);
        }

        if (basisIdx < dim)
        {
            for (int j = 0; j < dim; j++)
            {
                energy += _problemHamiltonian[basisIdx, j].Real;
            }
        }
        return energy;
    }
}
