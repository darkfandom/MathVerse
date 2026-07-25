namespace MathVerse.Math.DataScience.CASIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a dataset column whose values are derived from a formula expression.
/// </summary>
public sealed class ExpressionColumn
{
    /// <summary>
    /// Gets or sets the name of the expression column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formula expression string.
    /// </summary>
    public string Formula { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of column names that this expression depends on.
    /// </summary>
    public List<string> Dependencies { get; } = new();

    private readonly FormulaEvaluator _evaluator = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionColumn"/> class.
    /// </summary>
    public ExpressionColumn()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionColumn"/> class with the specified name and formula.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="formula">The formula expression.</param>
    /// <param name="dependencies">The column names this formula depends on.</param>
    public ExpressionColumn(string name, string formula, params string[] dependencies)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Formula = formula ?? throw new ArgumentNullException(nameof(formula));
        Dependencies.AddRange(dependencies);
    }

    /// <summary>
    /// Evaluates the formula for a given data row, substituting column values into the formula variables.
    /// </summary>
    /// <param name="row">A dictionary representing one data row.</param>
    /// <returns>The computed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the formula cannot be evaluated.</exception>
    public double Evaluate(Dictionary<string, object?> row)
    {
        if (row is null) throw new ArgumentNullException(nameof(row));

        var variables = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (string dep in Dependencies)
        {
            if (row.TryGetValue(dep, out object? val) && val is not null && IsNumeric(val))
                variables[dep] = Convert.ToDouble(val);
            else
                variables[dep] = 0.0;
        }

        return _evaluator.Evaluate(Formula, variables);
    }

    /// <summary>
    /// Evaluates the formula for a row and returns the result as an object suitable for dataset storage.
    /// </summary>
    /// <param name="row">A dictionary representing one data row.</param>
    /// <returns>The computed value as an object, or null if evaluation fails.</returns>
    public object? EvaluateToObject(Dictionary<string, object?> row)
    {
        try
        {
            return Evaluate(row);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to auto-discover dependencies from the formula by analyzing its tokens.
    /// </summary>
    public void DiscoverDependencies()
    {
        Dependencies.Clear();

        string[] tokens = FormulaTokenizer.Tokenize(Formula);
        var knownFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "sinh", "cosh", "tanh",
            "log", "ln", "log2", "exp", "sqrt", "abs",
            "ceil", "floor", "round", "pi", "e"
        };

        foreach (string token in tokens)
        {
            if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                continue;

            if (knownFunctions.Contains(token))
                continue;

            if (token is "+" or "-" or "*" or "/" or "^" or "(" or ")" or ",")
                continue;

            if (!Dependencies.Contains(token))
                Dependencies.Add(token);
        }
    }

    /// <summary>
    /// Returns a string representation of this expression column.
    /// </summary>
    /// <returns>A descriptive string.</returns>
    public override string ToString()
    {
        return $"{Name} = {Formula} [{string.Join(", ", Dependencies)}]";
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
