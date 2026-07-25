namespace MathVerse.Math.Interop.ScriptingAbstraction;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Core;

/// <summary>
/// Transpiles MathVerse expressions to MATLAB syntax.
/// </summary>
public sealed class MATLABAdapter : IScriptingAdapter
{
    /// <inheritdoc />
    public string LanguageId => "matlab";

    /// <inheritdoc />
    public string DisplayName => "MATLAB";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".m" };

    /// <inheritdoc />
    public bool CanTranspile(object expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        return expression is double or float or int or long or decimal
            or string
            or IDictionary<string, object>
            or IList<object>;
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
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => "% Unsupported type: " + expression.GetType().Name
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
        sb.Append('\'');
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\'': sb.Append("''"); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        sb.Append('\'');
        return sb.ToString();
    }

    private static string TranspileDictionary(IDictionary<string, object> dict)
    {
        var keys = new List<string>(dict.Count);
        var values = new List<string>(dict.Count);
        foreach (var kvp in dict)
        {
            keys.Add(TranspileString(kvp.Key));
            values.Add(TranspileObject(kvp.Value));
        }

        return $"containers.Map({{{string.Join(", ", keys)}}}, {{{string.Join(", ", values)}}})";
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
            return "[]";
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
            IDictionary<string, object> dict => TranspileDictionary(dict),
            IList<object> list => TranspileList(list),
            _ => value.ToString() ?? "[]"
        };
    }
}
