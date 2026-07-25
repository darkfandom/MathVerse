namespace MathVerse.Math.AI.Probability;

using System;
using System.Collections.Generic;

/// <summary>Discrete-time Markov chain with transition matrix and analysis capabilities.</summary>
public sealed class MarkovChain
{
    private readonly double[][] _transitionMatrix;
    private readonly int _numStates;

    /// <summary>Initializes a new Markov chain with the given transition matrix.</summary>
    /// <param name="transitionMatrix">Row-stochastic transition matrix where rows sum to 1.</param>
    public MarkovChain(double[][] transitionMatrix)
    {
        if (transitionMatrix == null)
            throw new ArgumentNullException(nameof(transitionMatrix));
        if (transitionMatrix.Length == 0)
            throw new ArgumentException("Transition matrix cannot be empty.", nameof(transitionMatrix));

        _numStates = transitionMatrix.Length;
        _transitionMatrix = new double[_numStates][];

        for (int i = 0; i < _numStates; i++)
        {
            if (transitionMatrix[i] == null || transitionMatrix[i].Length != _numStates)
                throw new ArgumentException($"Row {i} must have {_numStates} elements.", nameof(transitionMatrix));

            _transitionMatrix[i] = new double[_numStates];
            double sum = 0.0;
            for (int j = 0; j < _numStates; j++)
            {
                if (transitionMatrix[i][j] < 0.0 || transitionMatrix[i][j] > 1.0)
                    throw new ArgumentException($"Transition probability [{i}][{j}] must be between 0 and 1.", nameof(transitionMatrix));
                _transitionMatrix[i][j] = transitionMatrix[i][j];
                sum += transitionMatrix[i][j];
            }

            if (System.Math.Abs(sum - 1.0) > 1e-6)
                throw new ArgumentException($"Row {i} must sum to 1.0 (got {sum}).", nameof(transitionMatrix));
        }
    }

    /// <summary>Gets the number of states in the Markov chain.</summary>
    public int NumStates => _numStates;

    /// <summary>Gets a copy of the transition matrix.</summary>
    public double[][] TransitionMatrix
    {
        get
        {
            double[][] copy = new double[_numStates][];
            for (int i = 0; i < _numStates; i++)
            {
                copy[i] = new double[_numStates];
                Array.Copy(_transitionMatrix[i], copy[i], _numStates);
            }
            return copy;
        }
    }

    /// <summary>Performs one step of the Markov chain: new_state = state * T.</summary>
    /// <param name="currentState">Current state probability distribution.</param>
    /// <returns>New state distribution after one transition.</returns>
    public double[] Step(double[] currentState)
    {
        if (currentState == null)
            throw new ArgumentNullException(nameof(currentState));
        if (currentState.Length != _numStates)
            throw new ArgumentException($"State vector must have {_numStates} elements.", nameof(currentState));

        double[] newState = new double[_numStates];

        for (int j = 0; j < _numStates; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < _numStates; i++)
                sum += currentState[i] * _transitionMatrix[i][j];
            newState[j] = sum;
        }

        return newState;
    }

    /// <summary>Runs the Markov chain for a specified number of steps.</summary>
    /// <param name="steps">Number of steps to iterate.</param>
    /// <param name="initialState">Initial state probability distribution.</param>
    /// <returns>State distribution after the specified number of steps.</returns>
    public double[] Run(int steps, double[] initialState)
    {
        if (steps < 0)
            throw new ArgumentException("Steps must be non-negative.", nameof(steps));
        if (initialState == null)
            throw new ArgumentNullException(nameof(initialState));

        double[] state = (double[])initialState.Clone();
        for (int t = 0; t < steps; t++)
            state = Step(state);
        return state;
    }

    /// <summary>Computes the stationary distribution via power iteration until convergence.</summary>
    /// <returns>Stationary distribution vector.</returns>
    public double[] StationaryDistribution()
    {
        double[] state = new double[_numStates];
        state[0] = 1.0;

        const int maxIterations = 10000;
        const double tolerance = 1e-10;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] newState = Step(state);
            double diff = 0.0;
            for (int i = 0; i < _numStates; i++)
                diff += System.Math.Abs(newState[i] - state[i]);

            state = newState;

            if (diff < tolerance)
                break;
        }

        return state;
    }

    /// <summary>Checks whether the Markov chain is irreducible (all states communicate).</summary>
    /// <returns>True if the chain is irreducible; otherwise false.</returns>
    public bool IsIrreducible()
    {
        for (int start = 0; start < _numStates; start++)
        {
            bool[] reachable = BFS(start);
            for (int i = 0; i < _numStates; i++)
            {
                if (!reachable[i])
                    return false;
            }
        }
        return true;
    }

    /// <summary>Computes the mean first passage time from one state to another by solving a linear system.</summary>
    /// <param name="from">Source state index.</param>
    /// <param name="to">Target state index.</param>
    /// <returns>Expected number of steps to reach 'to' starting from 'from'.</returns>
    public double MeanFirstPassageTime(int from, int to)
    {
        if (from < 0 || from >= _numStates)
            throw new ArgumentOutOfRangeException(nameof(from));
        if (to < 0 || to >= _numStates)
            throw new ArgumentOutOfRangeException(nameof(to));

        if (from == to)
            return 0.0;

        int n = _numStates - 1;
        double[][] A = new double[n][];
        double[] b = new double[n];

        Dictionary<int, int> indexMap = new();
        int idx = 0;
        for (int i = 0; i < _numStates; i++)
        {
            if (i != to)
            {
                indexMap[i] = idx;
                idx++;
            }
        }

        for (int i = 0; i < _numStates; i++)
        {
            if (i == to)
                continue;

            int row = indexMap[i];
            A[row] = new double[n];
            A[row][row] = 1.0;

            for (int j = 0; j < _numStates; j++)
            {
                if (j == to)
                {
                    b[row] = 1.0;
                    continue;
                }
                if (j != i)
                {
                    int col = indexMap[j];
                    A[row][col] = -_transitionMatrix[i][j];
                }
            }
        }

        double[] solution = SolveLinearSystem(A, b);
        return solution[indexMap[from]];
    }

    /// <summary>Computes the n-step transition probability matrix.</summary>
    /// <param name="n">Number of steps.</param>
    /// <returns>The transition matrix raised to the power n.</returns>
    public double[][] TransitionPower(int n)
    {
        if (n < 0)
            throw new ArgumentException("Power must be non-negative.", nameof(n));

        double[][] result = IdentityMatrix(_numStates);
        double[][] baseMatrix = TransitionMatrix;

        while (n > 0)
        {
            if ((n & 1) == 1)
                result = MultiplyMatrices(result, baseMatrix);
            baseMatrix = MultiplyMatrices(baseMatrix, baseMatrix);
            n >>= 1;
        }

        return result;
    }

    private bool[] BFS(int start)
    {
        bool[] visited = new bool[_numStates];
        Queue<int> queue = new();
        queue.Enqueue(start);
        visited[start] = true;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            for (int next = 0; next < _numStates; next++)
            {
                if (!visited[next] && _transitionMatrix[current][next] > 0.0)
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    private static double[][] IdentityMatrix(int n)
    {
        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[n];
            result[i][i] = 1.0;
        }
        return result;
    }

    private static double[][] MultiplyMatrices(double[][] A, double[][] B)
    {
        int m = A.Length;
        int n = B[0].Length;
        int k = B.Length;

        double[][] result = new double[m][];
        for (int i = 0; i < m; i++)
        {
            result[i] = new double[n];
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int p = 0; p < k; p++)
                    sum += A[i][p] * B[p][j];
                result[i][j] = sum;
            }
        }
        return result;
    }

    private static double[] SolveLinearSystem(double[][] A, double[] b)
    {
        int n = b.Length;
        double[][] augmented = new double[n][];

        for (int i = 0; i < n; i++)
        {
            augmented[i] = new double[n + 1];
            Array.Copy(A[i], augmented[i], n);
            augmented[i][n] = b[i];
        }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(augmented[col][col]);
            for (int row = col + 1; row < n; row++)
            {
                double absVal = System.Math.Abs(augmented[row][col]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                double[] temp = augmented[col];
                augmented[col] = augmented[maxRow];
                augmented[maxRow] = temp;
            }

            double pivot = augmented[col][col];
            if (System.Math.Abs(pivot) < 1e-12)
                throw new InvalidOperationException("Linear system is singular or nearly singular.");

            for (int j = col; j <= n; j++)
                augmented[col][j] /= pivot;

            for (int row = 0; row < n; row++)
            {
                if (row == col)
                    continue;
                double factor = augmented[row][col];
                for (int j = col; j <= n; j++)
                    augmented[row][j] -= factor * augmented[col][j];
            }
        }

        double[] result = new double[n];
        for (int i = 0; i < n; i++)
            result[i] = augmented[i][n];
        return result;
    }
}
