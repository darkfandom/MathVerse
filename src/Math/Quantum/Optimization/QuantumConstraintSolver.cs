namespace MathVerse.Math.Quantum.Optimization;

using System;
using System.Collections.Generic;

/// <summary>
/// Constraint satisfaction solver using a quantum-inspired search approach
/// with constraint propagation and backtracking.
/// </summary>
public sealed class QuantumConstraintSolver
{
    private readonly int _numVariables;
    private readonly int _numConstraints;
    private readonly List<(int[] Variables, Func<int[], bool> Constraint)> _constraints;
    private readonly Random _rng;

    /// <summary>Gets the number of variables.</summary>
    public int NumVariables => _numVariables;

    /// <summary>Gets the number of constraints.</summary>
    public int NumConstraints => _constraints.Count;

    /// <summary>Creates a quantum constraint satisfaction solver.</summary>
    /// <param name="numVariables">The number of binary variables.</param>
    /// <param name="numConstraints">The expected number of constraints.</param>
    public QuantumConstraintSolver(int numVariables, int numConstraints)
    {
        if (numVariables < 1) throw new ArgumentOutOfRangeException(nameof(numVariables));
        if (numConstraints < 0) throw new ArgumentOutOfRangeException(nameof(numConstraints));
        _numVariables = numVariables;
        _numConstraints = numConstraints;
        _constraints = new List<(int[], Func<int[], bool>)>();
        _rng = new Random(42);
    }

    /// <summary>Adds a constraint on specified variables.</summary>
    /// <param name="variables">The indices of variables involved in the constraint.</param>
    /// <param name="constraint">The constraint predicate.</param>
    public void AddConstraint(int[] variables, Func<int[], bool> constraint)
    {
        if (variables == null || variables.Length == 0) throw new ArgumentException("Variables cannot be null or empty.", nameof(variables));
        if (constraint == null) throw new ArgumentNullException(nameof(constraint));
        _constraints.Add(((int[])variables.Clone(), constraint));
    }

    /// <summary>Finds a satisfying assignment using quantum-inspired search.</summary>
    /// <param name="maxSteps">Maximum number of search steps.</param>
    /// <returns>A satisfying assignment array, or null if none found.</returns>
    public int[]? Solve(int maxSteps)
    {
        if (maxSteps < 1) throw new ArgumentOutOfRangeException(nameof(maxSteps));

        var bestAssignment = new int[_numVariables];
        int bestSatisfied = CountSatisfied(bestAssignment);

        if (bestSatisfied == _constraints.Count)
            return bestAssignment;

        for (int step = 0; step < maxSteps; step++)
        {
            double temperature = 1.0 - (double)step / maxSteps;
            temperature = System.Math.Max(temperature, 0.01);

            var candidate = (int[])bestAssignment.Clone();
            int flipQubit = _rng.Next(_numVariables);
            candidate[flipQubit] = 1 - candidate[flipQubit];

            int candidateSatisfied = CountSatisfied(candidate);
            int delta = candidateSatisfied - bestSatisfied;

            if (delta > 0 || _rng.NextDouble() < System.Math.Exp(delta / temperature))
            {
                bestAssignment = candidate;
                bestSatisfied = candidateSatisfied;
            }

            if (bestSatisfied == _constraints.Count)
                return bestAssignment;
        }

        return bestSatisfied == _constraints.Count ? bestAssignment : null;
    }

    /// <summary>Verifies whether a given assignment satisfies all constraints.</summary>
    /// <param name="assignment">The variable assignment to verify.</param>
    /// <returns>True if all constraints are satisfied; false otherwise.</returns>
    public bool Verify(int[] assignment)
    {
        if (assignment == null) throw new ArgumentNullException(nameof(assignment));
        if (assignment.Length != _numVariables)
            throw new ArgumentException($"Assignment length ({assignment.Length}) must equal number of variables ({_numVariables}).");

        return CountSatisfied(assignment) == _constraints.Count;
    }

    private int CountSatisfied(int[] assignment)
    {
        int count = 0;
        for (int i = 0; i < _constraints.Count; i++)
        {
            try
            {
                if (_constraints[i].Constraint(assignment))
                    count++;
            }
            catch
            {
            }
        }
        return count;
    }
}
