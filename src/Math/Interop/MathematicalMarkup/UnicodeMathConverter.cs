namespace MathVerse.Math.Interop.MathematicalMarkup;

using System;
using System.Collections.Generic;
using System.Text;
using ExpressionExchange;

/// <summary>
/// Bidirectional converter between MathVerse expression nodes and Unicode math symbol notation.
/// </summary>
public sealed class UnicodeMathConverter
{
    private static readonly Dictionary<string, string> ToUnicodeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "+", "\u002B" },
        { "-", "\u2212" },
        { "*", "\u00D7" },
        { "/", "\u2215" },
        { "=", "\u003D" },
        { "<", "\u003C" },
        { ">", "\u003E" },
        { "<=", "\u2264" },
        { ">=", "\u2265" },
        { "!=", "\u2260" },
        { "and", "\u2227" },
        { "or", "\u2228" },
        { "not", "\u00AC" },
        { "pi", "\u03C0" },
        { "e", "\u2147" },
        { "i", "\u2148" },
        { "infinity", "\u221E" },
        { "true", "\u22A4" },
        { "false", "\u22A5" },
        { "integral", "\u222B" },
        { "sum", "\u2211" },
        { "product", "\u220F" },
        { "partial", "\u2202" },
        { "nabla", "\u2207" },
        { "sqrt", "\u221A" },
        { "forall", "\u2200" },
        { "exists", "\u2203" },
        { "in", "\u2208" },
        { "subset", "\u2282" },
        { "superset", "\u2283" },
        { "union", "\u222A" },
        { "intersection", "\u2229" },
        { "empty", "\u2205" },
        { "rightarrow", "\u2192" },
        { "leftarrow", "\u2190" },
        { "leftrightarrow", "\u2194" },
        { "implies", "\u21D2" },
        { "iff", "\u21D4" },
        { "alpha", "\u03B1" },
        { "beta", "\u03B2" },
        { "gamma", "\u03B3" },
        { "delta", "\u03B4" },
        { "epsilon", "\u03F5" },
        { "theta", "\u03B8" },
        { "lambda", "\u03BB" },
        { "mu", "\u03BC" },
        { "sigma", "\u03C3" },
        { "phi", "\u03D5" },
        { "omega", "\u03C9" },
        { "sin", "sin" },
        { "cos", "cos" },
        { "tan", "tan" },
        { "log", "log" },
        { "ln", "ln" },
        { "exp", "exp" },
        { "abs", "|" },
        { "max", "max" },
        { "min", "min" }
    };

    private static readonly Dictionary<string, string> FromUnicodeMap;

    static UnicodeMathConverter()
    {
        FromUnicodeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kvp in ToUnicodeMap)
        {
            if (!FromUnicodeMap.ContainsKey(kvp.Value))
            {
                FromUnicodeMap[kvp.Value] = kvp.Key;
            }
        }
    }

    /// <summary>
    /// Converts an expression node to Unicode math symbol string.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A string containing Unicode math symbols.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public string ToUnicodeMath(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var sb = new StringBuilder();
        AppendNode(sb, expression);
        return sb.ToString();
    }

    /// <summary>
    /// Converts a Unicode math string to an expression node.
    /// </summary>
    /// <param name="unicode">The Unicode math string.</param>
    /// <returns>The resulting expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="unicode"/> is null or empty.</exception>
    public ExpressionNode FromUnicodeMath(string unicode)
    {
        if (string.IsNullOrWhiteSpace(unicode))
        {
            throw new ArgumentException("Unicode math string cannot be null or empty.", nameof(unicode));
        }

        var tokens = Tokenize(unicode);
        int pos = 0;
        return ParseExpression(tokens, ref pos);
    }

    private static void AppendNode(StringBuilder sb, ExpressionNode node)
    {
        switch (node.NodeType)
        {
            case "Number":
                sb.Append(node.Value);
                break;
            case "Variable":
                sb.Append(MapToUnicode(node.Value));
                break;
            case "Operator":
                sb.Append(MapToUnicode(node.Value));
                break;
            case "BinaryOp":
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(' ');
                    sb.Append(MapToUnicode(node.Value));
                    sb.Append(' ');
                    AppendNode(sb, node.Children[1]);
                }
                break;
            case "Negation":
                sb.Append("\u2212");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                break;
            case "Fraction":
                sb.Append('(');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(" \u2215 ");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append(')');
                break;
            case "Power":
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append('\u02B9');
                    if (node.Children[1].NodeType == "BinaryOp" || node.Children[1].Children is { Count: > 1 })
                    {
                        sb.Append('(');
                        AppendNode(sb, node.Children[1]);
                        sb.Append(')');
                    }
                    else
                    {
                        AppendNode(sb, node.Children[1]);
                    }
                }
                break;
            case "SquareRoot":
                sb.Append('\u221A');
                if (node.Children is { Count: > 0 })
                {
                    sb.Append('(');
                    AppendNode(sb, node.Children[0]);
                    sb.Append(')');
                }
                break;
            case "Root":
                if (node.Children is { Count: >= 2 })
                {
                    sb.Append('(');
                    AppendNode(sb, node.Children[1]);
                    sb.Append(")\u221A(");
                    AppendNode(sb, node.Children[0]);
                    sb.Append(')');
                }
                break;
            case "Subscript":
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append('\u2081');
                    AppendNode(sb, node.Children[1]);
                }
                break;
            case "FunctionCall":
                sb.Append(MapToUnicode(node.Value));
                sb.Append('(');
                if (node.Children is { Count: > 0 })
                {
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        AppendNode(sb, node.Children[i]);
                    }
                }
                sb.Append(')');
                break;
            case "Integral":
                sb.Append('\u222B');
                if (node.Children is { Count: > 0 })
                {
                    sb.Append(' ');
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append(" \u2146");
                sb.Append(MapToUnicode(node.Value));
                break;
            case "Summation":
                sb.Append('\u2211');
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                break;
            case "Product":
                sb.Append('\u220F');
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                break;
            default:
                sb.Append(MapToUnicode(node.NodeType));
                sb.Append('(');
                sb.Append(MapToUnicode(node.Value));
                if (node.Children is { Count: > 0 })
                {
                    foreach (var child in node.Children)
                    {
                        sb.Append(", ");
                        AppendNode(sb, child);
                    }
                }
                sb.Append(')');
                break;
        }
    }

    private static string MapToUnicode(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return ToUnicodeMap.TryGetValue(value, out var mapped) ? mapped : value;
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < input.Length)
        {
            if (char.IsWhiteSpace(input[i]))
            {
                i++;
                continue;
            }

            if (input[i] == '(' || input[i] == ')' || input[i] == '[' || input[i] == ']' ||
                input[i] == '{' || input[i] == '}' || input[i] == ',' || input[i] == ';')
            {
                tokens.Add(input[i].ToString());
                i++;
                continue;
            }

            if (input[i] == '\u2264' || input[i] == '\u2265' || input[i] == '\u2260' ||
                input[i] == '\u2192' || input[i] == '\u2190' || input[i] == '\u21D2')
            {
                tokens.Add(input[i].ToString());
                i++;
                continue;
            }

            if (input[i] == '+' || input[i] == '\u00D7' || input[i] == '\u2215' ||
                input[i] == '=' || input[i] == '<' || input[i] == '>' ||
                input[i] == '^' || input[i] == '_' || input[i] == '!')
            {
                tokens.Add(input[i].ToString());
                i++;
                continue;
            }

            if (char.IsDigit(input[i]) || input[i] == '.')
            {
                int start = i;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                {
                    i++;
                }
                tokens.Add(input.Substring(start, i - start));
                continue;
            }

            if (char.IsLetter(input[i]) || input[i] == '_')
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                {
                    i++;
                }
                tokens.Add(input.Substring(start, i - start));
                continue;
            }

            var name = FromUnicodeMap.TryGetValue(input[i].ToString(), out var mapped) ? mapped : input[i].ToString();
            tokens.Add(name);
            i++;
        }

        return tokens;
    }

    private static ExpressionNode ParseExpression(List<string> tokens, ref int pos)
    {
        var left = ParseTerm(tokens, ref pos);

        while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-" ||
               tokens[pos] == "\u2212" || tokens[pos] == "=" || tokens[pos] == "<" ||
               tokens[pos] == ">" || tokens[pos] == "\u2264" || tokens[pos] == "\u2265" ||
               tokens[pos] == "\u2260"))
        {
            var op = tokens[pos];
            pos++;
            var right = ParseTerm(tokens, ref pos);
            left = new ExpressionNode("BinaryOp", op, new[] { left, right });
        }

        return left;
    }

    private static ExpressionNode ParseTerm(List<string> tokens, ref int pos)
    {
        var left = ParseFactor(tokens, ref pos);

        while (pos < tokens.Count && (tokens[pos] == "\u00D7" || tokens[pos] == "\u2215" || tokens[pos] == "*"))
        {
            var op = tokens[pos];
            pos++;
            var right = ParseFactor(tokens, ref pos);
            left = new ExpressionNode("BinaryOp", op, new[] { left, right });
        }

        return left;
    }

    private static ExpressionNode ParseFactor(List<string> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
        {
            return new ExpressionNode("Number", "0");
        }

        if (tokens[pos] == "(")
        {
            pos++;
            var expr = ParseExpression(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == ")")
            {
                pos++;
            }
            return expr;
        }

        if (tokens[pos] == "\u2212" || tokens[pos] == "-")
        {
            pos++;
            var operand = ParseFactor(tokens, ref pos);
            return new ExpressionNode("Negation", "-", new[] { operand });
        }

        if (tokens[pos] == "\u221A" || tokens[pos] == "sqrt")
        {
            pos++;
            if (pos < tokens.Count && tokens[pos] == "(")
            {
                pos++;
                var arg = ParseExpression(tokens, ref pos);
                if (pos < tokens.Count && tokens[pos] == ")")
                {
                    pos++;
                }
                return new ExpressionNode("SquareRoot", "sqrt", new[] { arg });
            }
            var factor = ParseFactor(tokens, ref pos);
            return new ExpressionNode("SquareRoot", "sqrt", new[] { factor });
        }

        if (tokens[pos] == "\u222B" || tokens[pos] == "int")
        {
            pos++;
            var varName = "x";
            if (pos < tokens.Count && tokens[pos] == "\u2146")
            {
                pos++;
                if (pos < tokens.Count)
                {
                    varName = tokens[pos];
                    pos++;
                }
            }
            return new ExpressionNode("Integral", varName);
        }

        if (tokens[pos] == "\u2211" || tokens[pos] == "sum")
        {
            pos++;
            return new ExpressionNode("Summation", "sum");
        }

        if (tokens[pos] == "\u220F" || tokens[pos] == "prod")
        {
            pos++;
            return new ExpressionNode("Product", "prod");
        }

        if (double.TryParse(tokens[pos], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            var num = tokens[pos];
            pos++;

            if (pos < tokens.Count && (tokens[pos] == "\u02B9" || tokens[pos] == "^"))
            {
                pos++;
                var exp = ParseFactor(tokens, ref pos);
                return new ExpressionNode("Power", "^", new[]
                {
                    new ExpressionNode("Number", num),
                    exp
                });
            }

            return new ExpressionNode("Number", num);
        }

        if (char.IsLetter(tokens[pos][0]) || tokens[pos][0] == '_')
        {
            var name = tokens[pos];
            pos++;

            string mappedName = FromUnicodeMap.TryGetValue(name, out var m) ? m : name;

            if (pos < tokens.Count && tokens[pos] == "(")
            {
                pos++;
                var args = new List<ExpressionNode>();
                if (pos < tokens.Count && tokens[pos] != ")")
                {
                    args.Add(ParseExpression(tokens, ref pos));
                    while (pos < tokens.Count && tokens[pos] == ",")
                    {
                        pos++;
                        args.Add(ParseExpression(tokens, ref pos));
                    }
                }
                if (pos < tokens.Count && tokens[pos] == ")")
                {
                    pos++;
                }
                return new ExpressionNode("FunctionCall", mappedName, args.ToArray());
            }

            var result = new ExpressionNode("Variable", mappedName);

            if (pos < tokens.Count && (tokens[pos] == "\u02B9" || tokens[pos] == "^"))
            {
                pos++;
                var exp = ParseFactor(tokens, ref pos);
                result = new ExpressionNode("Power", "^", new[] { result, exp });
            }

            if (pos < tokens.Count && (tokens[pos] == "\u2081" || tokens[pos] == "_"))
            {
                pos++;
                var sub = ParseFactor(tokens, ref pos);
                result = new ExpressionNode("Subscript", "_", new[] { result, sub });
            }

            return result;
        }

        var fallback = tokens[pos];
        pos++;
        return new ExpressionNode("Variable", fallback);
    }
}
