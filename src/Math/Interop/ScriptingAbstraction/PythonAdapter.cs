namespace MathVerse.Math.Interop.ScriptingAbstraction;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Core;

/// <summary>
/// Transpiles MathVerse expressions to Python syntax.
/// </summary>
public sealed class PythonAdapter : IScriptingAdapter
{
    /// <inheritdoc />
    public string LanguageId => "python";

    /// <inheritdoc />
    public string DisplayName => "Python";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".py", ".pyx" };

    /// <inheritdoc />
    public bool CanTranspile(object expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        return expression is double or float or int or long or decimal
            or string
            or IDictionary<string, object>
            or IList<object>
            or System.Numerics.Complex;
    }

    /// <inheritdoc />
    public string Transpile(object expression, string? expressionType = null)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));

        return expression switch
        {
            double d => TranspileDouble(d),
            float f => TranspileDouble((double)f),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture) + "L",
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            string s => TranspileString(s),
            System.Numerics.Complex c => TranspileComplex(c),
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => $"# Unsupported type: {expression.GetType().Name}"
        };
    }

    private static string TranspileDouble(double value)
    {
        if (double.IsNaN(value))
        {
            return "float('nan')";
        }

        if (double.IsPositiveInfinity(value))
        {
            return "float('inf')";
        }

        if (double.IsNegativeInfinity(value))
        {
            return "float('-inf')";
        }

        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    private static string TranspileString(string value)
    {
        var sb = new StringBuilder();
        sb.Append('"');
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static string TranspileComplex(System.Numerics.Complex c)
    {
        if (System.Math.Abs(c.Imaginary) < 1e-15)
        {
            return TranspileDouble(c.Real);
        }

        if (System.Math.Abs(c.Real) < 1e-15)
        {
            return TranspileDouble(c.Imaginary) + "j";
        }

        return $"complex({TranspileDouble(c.Real)}, {TranspileDouble(c.Imaginary)})";
    }

    private static string TranspileDictionary(IDictionary<string, object> dict)
    {
        var entries = new List<string>(dict.Count);
        foreach (var kvp in dict)
        {
            entries.Add($"{TranspileString(kvp.Key)}: {TranspileObject(kvp.Value)}");
        }

        return "{" + string.Join(", ", entries) + "}";
    }

    private static string TranspileList(IList<object> list)
    {
        var items = new List<string>(list.Count);
        foreach (var item in list)
        {
            items.Add(TranspileObject(item));
        }

        return "[" + string.Join(", ", items) + "]";
    }

    private static string TranspileObject(object? value)
    {
        if (value is null)
        {
            return "None";
        }

        return value switch
        {
            double d => TranspileDouble(d),
            float f => TranspileDouble((double)f),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture) + "L",
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            string s => TranspileString(s),
            bool b => b ? "True" : "False",
            System.Numerics.Complex c => TranspileComplex(c),
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => value.ToString() ?? "None"
        };
    }
}
