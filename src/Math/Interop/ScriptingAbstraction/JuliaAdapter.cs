namespace MathVerse.Math.Interop.ScriptingAbstraction;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Core;

/// <summary>
/// Transpiles MathVerse expressions to Julia syntax.
/// </summary>
public sealed class JuliaAdapter : IScriptingAdapter
{
    /// <inheritdoc />
    public string LanguageId => "julia";

    /// <inheritdoc />
    public string DisplayName => "Julia";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".jl" };

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
            long l => l.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            string s => TranspileString(s),
            System.Numerics.Complex c => TranspileComplex(c),
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => "# Unsupported type: " + expression.GetType().Name
        };
    }

    private static string TranspileDouble(double value)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        if (double.IsPositiveInfinity(value))
        {
            return "Inf";
        }

        if (double.IsNegativeInfinity(value))
        {
            return "-Inf";
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
            return TranspileDouble(c.Imaginary) + "im";
        }

        return $"complex({TranspileDouble(c.Real)}, {TranspileDouble(c.Imaginary)})";
    }

    private static string TranspileDictionary(IDictionary<string, object> dict)
    {
        var entries = new List<string>(dict.Count);
        foreach (var kvp in dict)
        {
            entries.Add($"{TranspileString(kvp.Key)} => {TranspileObject(kvp.Value)}");
        }

        return "Dict(" + string.Join(", ", entries) + ")";
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
            return "nothing";
        }

        return value switch
        {
            double d => TranspileDouble(d),
            float f => TranspileDouble((double)f),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            string s => TranspileString(s),
            bool b => b ? "true" : "false",
            System.Numerics.Complex c => TranspileComplex(c),
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => value.ToString() ?? "nothing"
        };
    }
}
