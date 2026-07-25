namespace MathVerse.Math.Quantum.Optimization;

using System;
using System.Collections.Generic;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Ising model for combinatorial optimization. Represents spin interactions via
/// coupling constants Jᵢⱼ and external fields hᵢ, with brute-force ground state search.
/// </summary>
public sealed class IsingModel
{
    private readonly int _numSpins;
    private readonly Dictionary<(int, int), double> _couplings = new();
    private readonly Dictionary<int, double> _fields = new();
    private readonly Random _rng;

    /// <summary>Gets the number of spins in the model.</summary>
    public int NumSpins => _numSpins;

    /// <summary>Creates an Ising model with the specified number of spins.</summary>
    /// <param name="numSpins">The number of spins.</param>
    public IsingModel(int numSpins)
    {
        if (numSpins < 1) throw new ArgumentOutOfRangeException(nameof(numSpins));
        _numSpins = numSpins;
        _rng = new Random(42);
    }

    /// <summary>Adds a coupling interaction Jᵢⱼ σᵢ σⱼ between spins i and j.</summary>
    /// <param name="i">The first spin index.</param>
    /// <param name="j">The second spin index.</param>
    /// <param name="J">The coupling constant.</param>
    public void AddCoupling(int i, int j, double J)
    {
        if (i < 0 || i >= _numSpins) throw new ArgumentOutOfRangeException(nameof(i));
        if (j < 0 || j >= _numSpins) throw new ArgumentOutOfRangeException(nameof(j));
        if (i == j) throw new ArgumentException("Coupling must be between distinct spins.");
        int key1 = System.Math.Min(i, j);
        int key2 = System.Math.Max(i, j);
        _couplings[(key1, key2)] = J;
    }

    /// <summary>Adds an external field hᵢ σᵢ on spin i.</summary>
    /// <param name="i">The spin index.</param>
    /// <param name="h">The field strength.</param>
    public void AddField(int i, double h)
    {
        if (i < 0 || i >= _numSpins) throw new ArgumentOutOfRangeException(nameof(i));
        _fields[i] = h;
    }

    /// <summary>Computes the energy of a spin configuration.</summary>
    /// <param name="spinConfiguration">Array of spin values (+1 or -1).</param>
    /// <returns>The total energy of the configuration.</returns>
    public double Energy(int[] spinConfiguration)
    {
        if (spinConfiguration == null) throw new ArgumentNullException(nameof(spinConfiguration));
        if (spinConfiguration.Length != _numSpins)
            throw new ArgumentException($"Configuration length ({spinConfiguration.Length}) must equal numSpins ({_numSpins}).");

        double energy = 0.0;

        foreach (var kvp in _couplings)
        {
            int i = kvp.Key.Item1;
            int j = kvp.Key.Item2;
            energy += kvp.Value * spinConfiguration[i] * spinConfiguration[j];
        }

        foreach (var kvp in _fields)
        {
            energy += kvp.Value * spinConfiguration[kvp.Key];
        }

        return energy;
    }

    /// <summary>Finds the ground state by brute-force enumeration (feasible for small systems).</summary>
    /// <returns>The spin configuration with minimum energy.</returns>
    public int[] FindGroundState()
    {
        if (_numSpins > 20)
            throw new InvalidOperationException("Brute-force search is not feasible for more than 20 spins.");

        int numStates = 1 << _numSpins;
        int[] bestConfig = new int[_numSpins];
        double bestEnergy = double.MaxValue;

        for (int state = 0; state < numStates; state++)
        {
            var config = new int[_numSpins];
            for (int i = 0; i < _numSpins; i++)
                config[i] = ((state & (1 << i)) != 0) ? 1 : -1;

            double energy = Energy(config);
            if (energy < bestEnergy)
            {
                bestEnergy = energy;
                bestConfig = config;
            }
        }

        return bestConfig;
    }

    /// <summary>Builds the full matrix Hamiltonian for the Ising model.</summary>
    /// <returns>The Hamiltonian matrix of dimension 2ⁿ × 2ⁿ.</returns>
    public ComplexMatrix ToHamiltonian()
    {
        int dim = 1 << _numSpins;
        var data = new Complex[dim, dim];

        for (int i = 0; i < _numSpins; i++)
        {
            int mask = 1 << i;
            for (int state = 0; state < dim; state++)
            {
                int spinVal = ((state & mask) != 0) ? 1 : -1;
                data[state, state] += new Complex(_fields.GetValueOrDefault(i, 0.0) * spinVal, 0.0);
            }
        }

        foreach (var kvp in _couplings)
        {
            int si = kvp.Key.Item1;
            int sj = kvp.Key.Item2;
            double J = kvp.Value;
            int maskI = 1 << si;
            int maskJ = 1 << sj;
            for (int state = 0; state < dim; state++)
            {
                int spinI = ((state & maskI) != 0) ? 1 : -1;
                int spinJ = ((state & maskJ) != 0) ? 1 : -1;
                data[state, state] += new Complex(J * spinI * spinJ, 0.0);
            }
        }

        return new ComplexMatrix(data);
    }
}
