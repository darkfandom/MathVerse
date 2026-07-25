namespace MathVerse.Math.Quantum.Variational;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements the Quantum Approximate Optimization Algorithm (QAOA) for combinatorial
/// optimization problems using alternating cost and mixer layers.
/// </summary>
public sealed class QAOA
{
    private readonly ComplexMatrix _costHamiltonian;
    private readonly ComplexMatrix _mixerHamiltonian;
    private readonly int _numQubits;
    private readonly int _depth;

    /// <summary>Gets the cost Hamiltonian.</summary>
    public ComplexMatrix CostHamiltonian => _costHamiltonian;

    /// <summary>Gets the mixer Hamiltonian.</summary>
    public ComplexMatrix MixerHamiltonian => _mixerHamiltonian;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Gets the QAOA circuit depth (number of layers).</summary>
    public int Depth => _depth;

    /// <summary>Creates a QAOA instance.</summary>
    /// <param name="costHamiltonian">The cost Hamiltonian encoding the optimization objective.</param>
    /// <param name="mixerHamiltonian">The mixer Hamiltonian (typically transverse field).</param>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="depth">The number of QAOA layers.</param>
    public QAOA(ComplexMatrix costHamiltonian, ComplexMatrix mixerHamiltonian, int numQubits, int depth)
    {
        _costHamiltonian = costHamiltonian ?? throw new ArgumentNullException(nameof(costHamiltonian));
        _mixerHamiltonian = mixerHamiltonian ?? throw new ArgumentNullException(nameof(mixerHamiltonian));
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
        _numQubits = numQubits;
        _depth = depth;
    }

    /// <summary>
    /// Computes the cost function value ⟨ψ(γ,β)|C|ψ(γ,β)⟩ for the given parameters.
    /// </summary>
    /// <param name="parameters">The QAOA parameters [γ₁,...,γₚ,β₁,...,βₚ].</param>
    /// <returns>The expected cost value.</returns>
    public double ComputeCost(double[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (parameters.Length != 2 * _depth)
            throw new ArgumentException($"Expected {2 * _depth} parameters, got {parameters.Length}.");

        int stateDim = 1 << _numQubits;
        var state = new Complex[stateDim];
        state[0] = Complex.One;

        for (int q = 0; q < _numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, _numQubits);

        for (int p = 0; p < _depth; p++)
        {
            double gamma = parameters[p];
            double beta = parameters[_depth + p];
            ApplyDiagonalUnitary(state, _costHamiltonian, gamma, stateDim);
            ApplyDiagonalUnitary(state, _mixerHamiltonian, beta, stateDim);
        }

        double cost = 0.0;
        for (int i = 0; i < stateDim; i++)
        {
            for (int j = 0; j < stateDim; j++)
            {
                cost += (Complex.Conjugate(state[i]) * _costHamiltonian[i, j] * state[j]).Real;
            }
        }
        return cost;
    }

    /// <summary>
    /// Optimizes the QAOA parameters.
    /// </summary>
    /// <param name="initialParameters">Initial parameter values (2×depth parameters).</param>
    /// <param name="maxIterations">Maximum optimization iterations.</param>
    /// <returns>The optimization result.</returns>
    public QAOAResult Optimize(double[] initialParameters, int maxIterations = 100)
    {
        if (initialParameters == null) throw new ArgumentNullException(nameof(initialParameters));

        var current = (double[])initialParameters.Clone();
        double currentCost = ComputeCost(current);
        var bestParams = (double[])current.Clone();
        double bestCost = currentCost;
        int bestSolution = 0;
        int iter;
        double lr = 0.1;

        for (iter = 0; iter < maxIterations; iter++)
        {
            var gradient = ParameterShiftGradient.ComputeGradient(ComputeCost, current);
            for (int i = 0; i < current.Length; i++)
                current[i] -= lr * gradient[i];

            currentCost = ComputeCost(current);
            if (currentCost < bestCost)
            {
                bestCost = currentCost;
                bestParams = (double[])current.Clone();
            }
        }

        int stateDim = 1 << _numQubits;
        var finalState = PrepareQAOAState(bestParams);
        bestSolution = FindMostLikely(finalState, stateDim);

        return new QAOAResult(bestCost, bestParams, iter, DecodeSolution(bestSolution, _numQubits));
    }

    private void ApplyDiagonalUnitary(Complex[] state, ComplexMatrix hamiltonian, double angle, int dim)
    {
        var expDiag = new Complex[dim];
        for (int i = 0; i < dim; i++)
            expDiag[i] = Complex.FromPolarCoordinates(1.0, -angle * hamiltonian[i, i].Real);

        for (int i = 0; i < dim; i++)
            state[i] *= expDiag[i];
    }

    private Complex[] PrepareQAOAState(double[] parameters)
    {
        int stateDim = 1 << _numQubits;
        var state = new Complex[stateDim];
        state[0] = Complex.One;

        for (int q = 0; q < _numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, _numQubits);

        for (int p = 0; p < _depth; p++)
        {
            double gamma = parameters[p];
            double beta = parameters[_depth + p];
            ApplyDiagonalUnitary(state, _costHamiltonian, gamma, stateDim);
            ApplyDiagonalUnitary(state, _mixerHamiltonian, beta, stateDim);
        }
        return state;
    }

    private static int FindMostLikely(Complex[] state, int dim)
    {
        int best = 0;
        double bestProb = 0.0;
        for (int i = 0; i < dim; i++)
        {
            double prob = state[i].Magnitude * state[i].Magnitude;
            if (prob > bestProb) { bestProb = prob; best = i; }
        }
        return best;
    }

    private static int[] DecodeSolution(int index, int numQubits)
    {
        var bits = new int[numQubits];
        for (int q = 0; q < numQubits; q++)
            bits[q] = (index >> q) & 1;
        return bits;
    }
}

/// <summary>
/// Represents the result of a QAOA optimization.
/// </summary>
public sealed class QAOAResult
{
    /// <summary>Gets the optimal cost value found.</summary>
    public double OptimalCost { get; }

    /// <summary>Gets the optimal parameter values.</summary>
    public double[] OptimalParameters { get; }

    /// <summary>Gets the number of iterations performed.</summary>
    public int Iterations { get; }

    /// <summary>Gets the best solution bit string found.</summary>
    public int[] BestSolution { get; }

    /// <summary>Creates a QAOA result.</summary>
    public QAOAResult(double optimalCost, double[] optimalParameters, int iterations, int[] bestSolution)
    {
        OptimalCost = optimalCost;
        OptimalParameters = optimalParameters ?? throw new ArgumentNullException(nameof(optimalParameters));
        Iterations = iterations;
        BestSolution = bestSolution ?? throw new ArgumentNullException(nameof(bestSolution));
    }
}
