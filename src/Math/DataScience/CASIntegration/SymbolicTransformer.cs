namespace MathVerse.Math.DataScience.CASIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Transforms dataset columns using symbolic expressions, creating new derived columns.
/// </summary>
public static class SymbolicTransformer
{
    /// <summary>
    /// Transforms a dataset by applying symbolic expressions to create new columns.
    /// Each key in the transformations dictionary is a new column name,
    /// and each value is a formula expression referencing existing columns.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="transformations">A dictionary mapping new column names to formula strings.</param>
    /// <returns>The modified dataset with new columns added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="transformations"/> is null.</exception>
    public static Dataset Transform(Dataset ds, Dictionary<string, string> transformations)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (transformations is null) throw new ArgumentNullException(nameof(transformations));

        foreach (var kvp in transformations)
        {
            string newColName = kvp.Key;
            string formula = kvp.Value;

            var deps = DiscoverDependencies(formula);

            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string dep in deps)
                mapping[dep] = dep;

            DerivedVariableGenerator.Generate(ds, newColName, formula, mapping);
        }

        return ds;
    }

    /// <summary>
    /// Transforms a dataset by applying symbolic expressions that reference existing columns,
    /// with explicit variable-to-column mapping.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="transformations">A dictionary mapping new column names to (formula, column mapping) pairs.</param>
    /// <returns>The modified dataset with new columns added.</returns>
    public static Dataset TransformWithMapping(
        Dataset ds,
        Dictionary<string, (string Formula, Dictionary<string, string> Mapping)> transformations)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (transformations is null) throw new ArgumentNullException(nameof(transformations));

        foreach (var kvp in transformations)
        {
            DerivedVariableGenerator.Generate(ds, kvp.Key, kvp.Value.Formula, kvp.Value.Mapping);
        }

        return ds;
    }

    /// <summary>
    /// Normalizes a column using a symbolic expression. The result replaces the original column.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="column">The column to normalize.</param>
    /// <param name="normalizationFormula">The normalization formula where 'x' refers to the column value.</param>
    /// <returns>The modified dataset.</returns>
    public static Dataset NormalizeColumn(Dataset ds, string column, string normalizationFormula)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(column));
        if (string.IsNullOrWhiteSpace(normalizationFormula))
            throw new ArgumentException("Normalization formula cannot be null or empty.", nameof(normalizationFormula));

        string tempCol = $"__temp_{column}__";
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["x"] = column };

        DerivedVariableGenerator.Generate(ds, tempCol, normalizationFormula, mapping);

        foreach (var row in ds.Rows)
        {
            if (row.ContainsKey(tempCol))
            {
                row[column] = row[tempCol];
                row.Remove(tempCol);
            }
        }

        return ds;
    }

    /// <summary>
    /// Applies a rolling symbolic transformation over a specified window size.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="sourceColumn">The column to apply the rolling transform to.</param>
    /// <param name="newColumnName">The name for the output column.</param>
    /// <param name="windowSize">The rolling window size.</param>
    /// <param name="formula">The formula where 'x' refers to the current window element.</param>
    /// <returns>The modified dataset.</returns>
    public static Dataset RollingTransform(
        Dataset ds,
        string sourceColumn,
        string newColumnName,
        int windowSize,
        string formula)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(sourceColumn))
            throw new ArgumentException("Source column name cannot be null or empty.", nameof(sourceColumn));
        if (string.IsNullOrWhiteSpace(newColumnName))
            throw new ArgumentException("New column name cannot be null or empty.", nameof(newColumnName));
        if (windowSize < 1)
            throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be at least 1.");

        var evaluator = new FormulaEvaluator();

        for (int i = 0; i < ds.Count; i++)
        {
            int start = System.Math.Max(0, i - windowSize + 1);
            double aggregated = 0.0;
            int count = 0;

            for (int j = start; j <= i; j++)
            {
                if (ds.Rows[j].TryGetValue(sourceColumn, out object? val) && val is not null && IsNumeric(val))
                {
                    double x = Convert.ToDouble(val);
                    aggregated += evaluator.Evaluate(formula, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { ["x"] = x });
                    count++;
                }
            }

            ds.Rows[i][newColumnName] = count > 0 ? aggregated / count : 0.0;
        }

        return ds;
    }

    private static List<string> DiscoverDependencies(string formula)
    {
        var deps = new List<string>();
        var knownFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "sinh", "cosh", "tanh",
            "log", "ln", "log2", "exp", "sqrt", "abs",
            "ceil", "floor", "round", "pi", "e"
        };

        string[] tokens = FormulaTokenizer.Tokenize(formula);
        foreach (string token in tokens)
        {
            if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                continue;

            if (knownFunctions.Contains(token))
                continue;

            if (token is "+" or "-" or "*" or "/" or "^" or "(" or ")" or ",")
                continue;

            if (!deps.Contains(token))
                deps.Add(token);
        }

        return deps;
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
