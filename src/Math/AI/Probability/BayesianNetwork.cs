namespace MathVerse.Math.AI.Probability;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>Bayesian network for probabilistic inference using directed acyclic graphs.</summary>
public sealed class BayesianNetwork
{
    private readonly Dictionary<string, string[]> _parents = new();
    private readonly Dictionary<string, double[][]> _conditionalTables = new();
    private readonly Dictionary<string, int> _cardinalities = new();

    /// <summary>Adds a variable with its cardinality, parent names, and conditional probability table.</summary>
    /// <param name="name">Unique variable name.</param>
    /// <param name="cardinality">Number of possible states.</param>
    /// <param name="parents">Names of parent variables (empty for root nodes).</param>
    /// <param name="conditionalTable">Conditional probability table P(X|parents).</param>
    public void AddVariable(string name, int cardinality, string[] parents, double[][] conditionalTable)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        if (cardinality <= 0)
            throw new ArgumentException("Cardinality must be positive.", nameof(cardinality));
        if (parents == null)
            throw new ArgumentNullException(nameof(parents));
        if (conditionalTable == null)
            throw new ArgumentNullException(nameof(conditionalTable));

        int expectedRows = 1;
        foreach (string parent in parents)
        {
            if (!_cardinalities.TryGetValue(parent, out int parentCard))
                throw new ArgumentException($"Parent variable '{parent}' has not been added.");
            expectedRows *= parentCard;
        }

        if (conditionalTable.Length != expectedRows)
            throw new ArgumentException($"Conditional table must have {expectedRows} rows for the given parents.");

        for (int i = 0; i < conditionalTable.Length; i++)
        {
            if (conditionalTable[i].Length != cardinality)
                throw new ArgumentException($"Conditional table row {i} must have {cardinality} entries.");

            double sum = 0.0;
            for (int j = 0; j < cardinality; j++)
                sum += conditionalTable[i][j];

            if (System.Math.Abs(sum - 1.0) > 1e-6)
                throw new ArgumentException($"Conditional table row {i} must sum to 1.0 (got {sum}).");
        }

        _parents[name] = parents;
        _conditionalTables[name] = conditionalTable;
        _cardinalities[name] = cardinality;
    }

    /// <summary>Computes the joint probability of a complete variable assignment.</summary>
    /// <param name="assignment">Map of variable names to their assigned values.</param>
    /// <returns>Joint probability P(all variables).</returns>
    public double JointProbability(ImmutableDictionary<string, int> assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        double probability = 1.0;

        foreach (KeyValuePair<string, double[][]> entry in _conditionalTables)
        {
            string variable = entry.Key;
            double[][] table = entry.Value;

            if (!assignment.TryGetValue(variable, out int value))
                throw new ArgumentException($"Variable '{variable}' is not assigned.");

            int[] parentIndices = GetParentAssignment(variable, assignment);
            int tableRow = ComputeTableRowIndex(variable, parentIndices);

            probability *= table[tableRow][value];
        }

        return probability;
    }

    /// <summary>Computes conditional probability P(variable=value | evidence) via enumeration.</summary>
    /// <param name="variable">Query variable.</param>
    /// <param name="value">Query value.</param>
    /// <param name="evidence">Observed evidence variables and their values.</param>
    /// <returns>Conditional probability between 0 and 1.</returns>
    public double ConditionalProbability(string variable, int value, ImmutableDictionary<string, int> evidence)
    {
        if (string.IsNullOrEmpty(variable))
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(variable));
        if (!_cardinalities.ContainsKey(variable))
            throw new ArgumentException($"Variable '{variable}' is not in the network.");
        if (evidence == null)
            throw new ArgumentNullException(nameof(evidence));

        List<string> hiddenVariables = new();
        foreach (string v in _cardinalities.Keys)
        {
            if (!evidence.ContainsKey(v) && v != variable)
                hiddenVariables.Add(v);
        }

        double numerator = EnumerateJoint(variable, value, evidence, hiddenVariables, 0);
        double denominator = 0.0;
        for (int val = 0; val < _cardinalities[variable]; val++)
        {
            denominator += EnumerateJoint(variable, val, evidence, hiddenVariables, 0);
        }

        if (System.Math.Abs(denominator) < 1e-15)
            return 0.0;

        return numerator / denominator;
    }

    /// <summary>Finds the most probable explanation for evidence using variable elimination.</summary>
    /// <param name="evidence">Observed evidence variables and their values.</param>
    /// <returns>Map of non-evidence variables to their most probable values.</returns>
    public ImmutableDictionary<string, int> MostProbableExplanation(ImmutableDictionary<string, int> evidence)
    {
        if (evidence == null)
            throw new ArgumentNullException(nameof(evidence));

        List<string> hiddenVariables = new();
        foreach (string v in _cardinalities.Keys)
        {
            if (!evidence.ContainsKey(v))
                hiddenVariables.Add(v);
        }

        Dictionary<string, Dictionary<int, double>> maxValues = new();

        foreach (string varName in hiddenVariables)
        {
            maxValues[varName] = new Dictionary<int, double>();
            for (int val = 0; val < _cardinalities[varName]; val++)
            {
                maxValues[varName][val] = double.MinValue;
            }
        }

        Dictionary<string, int> bestAssignment = new(evidence);
        EnumerateMAP(hiddenVariables, 0, bestAssignment, evidence, maxValues);

        Dictionary<string, int> result = new();
        foreach (string v in hiddenVariables)
        {
            double bestProb = double.MinValue;
            int bestVal = 0;
            foreach (KeyValuePair<int, double> kv in maxValues[v])
            {
                if (kv.Value > bestProb)
                {
                    bestProb = kv.Value;
                    bestVal = kv.Key;
                }
            }
            result[v] = bestVal;
        }

        return result.ToImmutableDictionary();
    }

    private double EnumerateJoint(string queryVar, int queryVal, ImmutableDictionary<string, int> evidence, List<string> hidden, int index)
    {
        if (index == hidden.Count)
        {
            Dictionary<string, int> full = new(evidence);
            full[queryVar] = queryVal;

            double prob = 1.0;
            foreach (string varName in _conditionalTables.Keys)
            {
                string[] parents = _parents[varName];
                int[] parentIndices = new int[parents.Length];
                for (int p = 0; p < parents.Length; p++)
                    parentIndices[p] = full[parents[p]];

                int tableRow = ComputeTableRowIndex(varName, parentIndices);
                prob *= _conditionalTables[varName][tableRow][full[varName]];
            }
            return prob;
        }

        string currentHidden = hidden[index];
        double sum = 0.0;
        for (int val = 0; val < _cardinalities[currentHidden]; val++)
        {
            ImmutableDictionary<string, int>.Builder builder = evidence.ToBuilder();
            builder[currentHidden] = val;
            sum += EnumerateJoint(queryVar, queryVal, builder.ToImmutable(), hidden, index + 1);
        }
        return sum;
    }

    private void EnumerateMAP(List<string> hidden, int index, Dictionary<string, int> assignment, ImmutableDictionary<string, int> evidence, Dictionary<string, Dictionary<int, double>> maxValues)
    {
        if (index == hidden.Count)
        {
            double prob = 1.0;
            foreach (string varName in _conditionalTables.Keys)
            {
                string[] parents = _parents[varName];
                int[] parentIndices = new int[parents.Length];
                for (int p = 0; p < parents.Length; p++)
                    parentIndices[p] = assignment[parents[p]];

                int tableRow = ComputeTableRowIndex(varName, parentIndices);
                prob *= _conditionalTables[varName][tableRow][assignment[varName]];
            }

            System.Math.Log(prob);

            foreach (string v in hidden)
            {
                int val = assignment[v];
                if (prob > maxValues[v][val])
                    maxValues[v][val] = prob;
            }
            return;
        }

        string currentVar = hidden[index];
        for (int val = 0; val < _cardinalities[currentVar]; val++)
        {
            assignment[currentVar] = val;
            EnumerateMAP(hidden, index + 1, assignment, evidence, maxValues);
        }
    }

    private int[] GetParentAssignment(string variable, ImmutableDictionary<string, int> assignment)
    {
        string[] parents = _parents[variable];
        int[] result = new int[parents.Length];
        for (int i = 0; i < parents.Length; i++)
            result[i] = assignment[parents[i]];
        return result;
    }

    private int ComputeTableRowIndex(string variable, int[] parentValues)
    {
        string[] parents = _parents[variable];
        int index = 0;
        int multiplier = 1;

        for (int i = parents.Length - 1; i >= 0; i--)
        {
            index += parentValues[i] * multiplier;
            multiplier *= _cardinalities[parents[i]];
        }

        return index;
    }
}
