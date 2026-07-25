namespace MathVerse.Math.DataScience.CASIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Generates derived columns in a dataset by evaluating formulas on existing data.
/// </summary>
public static class DerivedVariableGenerator
{
    /// <summary>
    /// Adds a new column to the dataset whose values are computed by evaluating a formula
    /// with each row's values substituted into the formula's variables.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="newColumnName">The name of the new column to create.</param>
    /// <param name="formula">The formula string (e.g., "price * quantity").</param>
    /// <param name="columnMapping">Maps formula variable names to dataset column names.</param>
    /// <returns>The modified dataset with the new column.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the column name already exists or mapping is invalid.</exception>
    public static Dataset Generate(
        Dataset ds,
        string newColumnName,
        string formula,
        Dictionary<string, string> columnMapping)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(newColumnName))
            throw new ArgumentException("New column name cannot be null or empty.", nameof(newColumnName));
        if (string.IsNullOrWhiteSpace(formula))
            throw new ArgumentException("Formula cannot be null or empty.", nameof(formula));
        if (columnMapping is null) throw new ArgumentNullException(nameof(columnMapping));

        var evaluator = new FormulaEvaluator();

        foreach (var row in ds.Rows)
        {
            var variables = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var mapping in columnMapping)
            {
                string formulaVar = mapping.Key;
                string datasetCol = mapping.Value;

                if (row.TryGetValue(datasetCol, out object? val) && val is not null && IsNumeric(val))
                    variables[formulaVar] = Convert.ToDouble(val);
                else
                    variables[formulaVar] = 0.0;
            }

            double result = evaluator.Evaluate(formula, variables);
            row[newColumnName] = result;
        }

        return ds;
    }

    /// <summary>
    /// Generates multiple derived columns at once.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="newColumnDefinitions">A dictionary mapping new column names to (formula, columnMapping) pairs.</param>
    /// <returns>The modified dataset with all new columns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    public static Dataset GenerateMultiple(
        Dataset ds,
        Dictionary<string, (string Formula, Dictionary<string, string> ColumnMapping)> newColumnDefinitions)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (newColumnDefinitions is null) throw new ArgumentNullException(nameof(newColumnDefinitions));

        foreach (var kvp in newColumnDefinitions)
        {
            Generate(ds, kvp.Key, kvp.Value.Formula, kvp.Value.ColumnMapping);
        }

        return ds;
    }

    /// <summary>
    /// Adds a column computed by a formula that references other computed columns.
    /// Columns are computed in topological order based on dependencies.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="derivedColumns">A list of (name, formula, mapping) tuples in any order.</param>
    /// <returns>The modified dataset with all derived columns.</returns>
    public static Dataset GenerateWithDependencies(
        Dataset ds,
        List<(string Name, string Formula, Dictionary<string, string> Mapping)> derivedColumns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (derivedColumns is null) throw new ArgumentNullException(nameof(derivedColumns));

        var computed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var remaining = new List<(string Name, string Formula, Dictionary<string, string> Mapping)>(derivedColumns);

        int maxIterations = remaining.Count + 1;
        int iteration = 0;

        while (remaining.Count > 0 && iteration < maxIterations)
        {
            bool progress = false;
            int i = 0;

            while (i < remaining.Count)
            {
                var def = remaining[i];
                bool allDepsReady = true;

                foreach (var mapping in def.Mapping.Values)
                {
                    if (!computed.Contains(mapping) && !ColumnExists(ds, mapping))
                    {
                        allDepsReady = false;
                        break;
                    }
                }

                if (allDepsReady)
                {
                    Generate(ds, def.Name, def.Formula, def.Mapping);
                    computed.Add(def.Name);
                    remaining.RemoveAt(i);
                    progress = true;
                }
                else
                {
                    i++;
                }
            }

            if (!progress)
                throw new ArgumentException("Circular dependency detected in derived column definitions.");

            iteration++;
        }

        return ds;
    }

    private static bool ColumnExists(Dataset ds, string columnName)
    {
        if (ds.Count == 0) return false;
        return ds.Rows[0].ContainsKey(columnName);
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
