namespace MathVerse.Math.DataScience.CASIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Represents a dataset with symbolic expressions as column data.
/// Supports evaluation with concrete variable values.
/// </summary>
public sealed class SymbolicDataset
{
    /// <summary>
    /// Gets or sets the name of the symbolic dataset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the dictionary mapping column names to symbolic expression strings.
    /// </summary>
    public Dictionary<string, string> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the list of variable names that appear in the symbolic expressions.
    /// </summary>
    public List<string> Variables { get; } = new();

    /// <summary>
    /// Adds a symbolic column to the dataset.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="expression">The symbolic expression string.</param>
    public void AddColumn(string columnName, string expression)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        Columns[columnName] = expression;
    }

    /// <summary>
    /// Evaluates all symbolic expressions with the given variable values,
    /// producing a concrete Dataset with numeric results.
    /// </summary>
    /// <param name="variableValues">An array of variable values, one per variable in <see cref="Variables"/>.</param>
    /// <returns>A dataset with evaluated numeric values.</returns>
    /// <exception cref="ArgumentException">Thrown when variable values count does not match variable count.</exception>
    public Dataset Evaluate(double[] variableValues)
    {
        if (variableValues is null) throw new ArgumentNullException(nameof(variableValues));
        if (variableValues.Length != Variables.Count)
            throw new ArgumentException(
                $"Expected {Variables.Count} variable values, got {variableValues.Length}.");

        var varDict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < Variables.Count; i++)
            varDict[Variables[i]] = variableValues[i];

        var ds = new Dataset { Name = Name };

        foreach (var kvp in Columns)
        {
            FormulaEvaluator evaluator = new();
            double result = evaluator.Evaluate(kvp.Value, varDict);

            if (ds.Count == 0)
                ds.Rows.Add(new Dictionary<string, object?>());

            ds.Rows[0][kvp.Key] = result;
        }

        return ds;
    }

    /// <summary>
    /// Evaluates all symbolic expressions for a grid of variable values,
    /// producing a dataset with one row per grid point.
    /// </summary>
    /// <param name="variableGrids">A dictionary mapping variable names to arrays of values to evaluate.</param>
    /// <returns>A dataset with all combinations evaluated.</returns>
    public Dataset EvaluateGrid(Dictionary<string, double[]> variableGrids)
    {
        if (variableGrids is null) throw new ArgumentNullException(nameof(variableGrids));

        var result = new Dataset { Name = Name };

        var varNames = new List<string>(variableGrids.Keys);
        var varArrays = new List<double[]>(variableGrids.Values);

        int[] indices = new int[varNames.Count];
        int[] lengths = new int[varNames.Count];
        for (int i = 0; i < varNames.Count; i++)
            lengths[i] = varArrays[i].Length;

        int totalCombinations = 1;
        foreach (int len in lengths)
            totalCombinations *= len;

        for (int combo = 0; combo < totalCombinations; combo++)
        {
            var varDict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            int temp = combo;
            for (int v = varNames.Count - 1; v >= 0; v--)
            {
                indices[v] = temp % lengths[v];
                temp /= lengths[v];
                varDict[varNames[v]] = varArrays[v][indices[v]];
            }

            var row = new Dictionary<string, object?>();
            foreach (var kvp in Columns)
            {
                FormulaEvaluator evaluator = new();
                row[kvp.Key] = evaluator.Evaluate(kvp.Value, varDict);
            }

            result.Rows.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Gets all unique variable names referenced in the symbolic expressions.
    /// </summary>
    /// <returns>A list of discovered variable names.</returns>
    public List<string> DiscoverVariables()
    {
        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "sinh", "cosh", "tanh",
            "log", "ln", "exp", "sqrt", "abs",
            "pi", "e"
        };

        foreach (var kvp in Columns)
        {
            var tokens = FormulaTokenizer.Tokenize(kvp.Value);
            foreach (string token in tokens)
            {
                if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                    continue;

                if (knownFunctions.Contains(token))
                    continue;

                if (token is "+" or "-" or "*" or "/" or "^" or "(" or ")" or ",")
                    continue;

                discovered.Add(token);
            }
        }

        Variables.Clear();
        Variables.AddRange(discovered);
        return new List<string>(Variables);
    }
}
