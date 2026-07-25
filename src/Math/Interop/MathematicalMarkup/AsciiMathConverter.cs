namespace MathVerse.Math.Interop.MathematicalMarkup;

using System;
using System.Collections.Generic;
using System.Text;
using ExpressionExchange;

/// <summary>
/// Bidirectional converter between MathVerse expression nodes and AsciiMath notation.
/// </summary>
public sealed class AsciiMathConverter
{
    private static readonly Dictionary<string, string> ToAsciiMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "+", "+" },
        { "-", "-" },
        { "*", "*" },
        { "/", "/" },
        { "=", "=" },
        { "<", "<" },
        { ">", ">" },
        { "<=", "<=" },
        { ">=", ">=" },
        { "!=", "!=" },
        { "(", "(" },
        { ")", ")" },
        { "[", "[" },
        { "]", "]" },
        { "{", "{" },
        { "}", "}" },
        { "pi", "pi" },
        { "e", "e" },
        { "i", "i" },
        { "infinity", "oo" },
        { "true", "true" },
        { "false", "false" },
        { "and", "and" },
        { "or", "or" },
        { "not", "not" },
        { "sin", "sin" },
        { "cos", "cos" },
        { "tan", "tan" },
        { "log", "log" },
        { "ln", "ln" },
        { "exp", "exp" },
        { "sqrt", "sqrt" },
        { "abs", "abs" },
        { "max", "max" },
        { "min", "min" }
    };

    private static readonly Dictionary<string, string> FromAsciiMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "pi", "pi" },
        { "e", "e" },
        { "oo", "infinity" },
        { "and", "and" },
        { "or", "or" },
        { "not", "not" },
        { "sin", "sin" },
        { "cos", "cos" },
        { "tan", "tan" },
        { "log", "log" },
        { "ln", "ln" },
        { "exp", "exp" },
        { "sqrt", "sqrt" },
        { "abs", "abs" },
        { "max", "max" },
        { "min", "min" },
        { "alpha", "alpha" },
        { "beta", "beta" },
        { "gamma", "gamma" },
        { "delta", "delta" },
        { "epsilon", "epsilon" },
        { "theta", "theta" },
        { "lambda", "lambda" },
        { "mu", "mu" },
        { "sigma", "sigma" },
        { "phi", "phi" },
        { "omega", "omega" }
    };

    /// <summary>
    /// Converts an expression node to AsciiMath notation.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A string containing the AsciiMath representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public string ToAsciiMath(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var sb = new StringBuilder();
        AppendNode(sb, expression);
        return sb.ToString();
    }

    /// <summary>
    /// Converts an AsciiMath string to an expression node.
    /// </summary>
    /// <param name="asciimath">The AsciiMath string.</param>
    /// <returns>The resulting expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asciimath"/> is null or empty.</exception>
    public ExpressionNode FromAsciiMath(string asciimath)
    {
        if (string.IsNullOrWhiteSpace(asciimath))
        {
            throw new ArgumentException("AsciiMath string cannot be null or empty.", nameof(asciimath));
        }

        var tokens = Tokenize(asciimath);
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
                sb.Append(EscapeAscii(node.Value));
                break;
            case "Operator":
                sb.Append(EscapeAscii(node.Value));
                break;
            case "BinaryOp":
                sb.Append('(');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(' ');
                    sb.Append(MapOperatorToAscii(node.Value));
                    sb.Append(' ');
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append(')');
                break;
            case "Negation":
                sb.Append("-( ");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append(" )");
                break;
            case "Fraction":
                sb.Append("(( ");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(" ) / ( ");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append(" ))");
                break;
            case "Power":
                sb.Append('(');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("^");
                    sb.Append('(');
                    AppendNode(sb, node.Children[1]);
                    sb.Append(')');
                }
                sb.Append(')');
                break;
            case "SquareRoot":
                sb.Append("sqrt(");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append(')');
                break;
            case "Root":
                sb.Append("root(");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(", ");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append(')');
                break;
            case "Subscript":
                sb.Append('(');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append('_');
                    sb.Append('(');
                    AppendNode(sb, node.Children[1]);
                    sb.Append(')');
                }
                sb.Append(')');
                break;
            case "FunctionCall":
                sb.Append(MapFunctionToAscii(node.Value));
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
                sb.Append("int ");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append(" d");
                sb.Append(node.Value);
                break;
            case "Summation":
                sb.Append("sum ");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                break;
            case "Product":
                sb.Append("prod ");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                break;
            default:
                sb.Append(node.NodeType);
                sb.Append('(');
                sb.Append(EscapeAscii(node.Value));
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

    private static string MapOperatorToAscii(string op)
    {
        return op switch
        {
            "+" => "+",
            "-" => "-",
            "*" => "*",
            "/" => "/",
            "^" => "^",
            "_" => "_",
            "=" => "=",
            "<" => "<",
            ">" => ">",
            "<=" => "<=",
            ">=" => ">=",
            "!=" => "!=",
            _ => op
        };
    }

    private static string MapFunctionToAscii(string name)
    {
        return ToAsciiMap.TryGetValue(name, out var mapped) ? mapped : name;
    }

    private static string EscapeAscii(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value;
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

            if (input[i] == '+' || input[i] == '-' || input[i] == '*' || input[i] == '/' ||
                input[i] == '^' || input[i] == '_' || input[i] == '=' || input[i] == '<' ||
                input[i] == '>' || input[i] == '!')
            {
                if (i + 1 < input.Length && input[i + 1] == '=')
                {
                    tokens.Add(input.Substring(i, 2));
                    i += 2;
                }
                else
                {
                    tokens.Add(input[i].ToString());
                    i++;
                }
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

            tokens.Add(input[i].ToString());
            i++;
        }

        return tokens;
    }

    private static ExpressionNode ParseExpression(List<string> tokens, ref int pos)
    {
        var left = ParseTerm(tokens, ref pos);

        while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
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

        while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/"))
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

        if (tokens[pos] == "-")
        {
            pos++;
            var operand = ParseFactor(tokens, ref pos);
            return new ExpressionNode("Negation", "-", new[] { operand });
        }

        if (tokens[pos] == "sqrt")
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

        if (tokens[pos] == "int" || tokens[pos] == "sum" || tokens[pos] == "prod")
        {
            var name = tokens[pos];
            pos++;
            var varName = name switch
            {
                "int" => "x",
                "sum" => "sum",
                "prod" => "prod",
                _ => name
            };
            return new ExpressionNode(name switch
            {
                "int" => "Integral",
                "sum" => "Summation",
                "prod" => "Product",
                _ => name
            }, varName);
        }

        if (double.TryParse(tokens[pos], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            var num = tokens[pos];
            pos++;
            var node = new ExpressionNode("Number", num);

            if (pos < tokens.Count && tokens[pos] == "^")
            {
                pos++;
                var exp = ParseFactor(tokens, ref pos);
                node = new ExpressionNode("Power", "^", new[] { node, exp });
            }

            if (pos < tokens.Count && tokens[pos] == "_")
            {
                pos++;
                var sub = ParseFactor(tokens, ref pos);
                node = new ExpressionNode("Subscript", "_", new[] { node, sub });
            }

            return node;
        }

        if (char.IsLetter(tokens[pos][0]) || tokens[pos][0] == '_')
        {
            var name = tokens[pos];
            pos++;

            string mappedName = FromAsciiMap.TryGetValue(name, out var m) ? m : name;

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

            if (pos < tokens.Count && tokens[pos] == "^")
            {
                pos++;
                var exp = ParseFactor(tokens, ref pos);
                result = new ExpressionNode("Power", "^", new[] { result, exp });
            }

            if (pos < tokens.Count && tokens[pos] == "_")
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
