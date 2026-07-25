namespace MathVerse.Math.Interop.MathematicalMarkup;

using System;
using System.Collections.Generic;
using System.Text;
using ExpressionExchange;

/// <summary>
/// Bidirectional converter between MathVerse expression nodes and LaTeX math markup.
/// </summary>
public sealed class LaTeXConverter
{
    /// <summary>
    /// Converts an expression node to LaTeX math string.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A string containing the LaTeX representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public string ToLaTeX(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var sb = new StringBuilder();
        AppendNode(sb, expression);
        return sb.ToString();
    }

    /// <summary>
    /// Converts a LaTeX math string to an expression node.
    /// </summary>
    /// <param name="latex">The LaTeX math string (without delimiters).</param>
    /// <returns>The resulting expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="latex"/> is null or empty.</exception>
    public ExpressionNode FromLaTeX(string latex)
    {
        if (string.IsNullOrWhiteSpace(latex))
        {
            throw new ArgumentException("LaTeX string cannot be null or empty.", nameof(latex));
        }

        var tokens = Tokenize(latex);
        int pos = 0;
        return ParseExpression(tokens, ref pos);
    }

    private static void AppendNode(StringBuilder sb, ExpressionNode node)
    {
        switch (node.NodeType)
        {
            case "Number":
                sb.Append(EscapeLatexNumber(node.Value));
                break;
            case "Variable":
                sb.Append(EscapeLatexIdent(node.Value));
                break;
            case "Operator":
                sb.Append(EscapeLatexIdent(node.Value));
                break;
            case "BinaryOp":
                sb.Append('{');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append(' ');
                    sb.Append(MapOperator(node.Value));
                    sb.Append(' ');
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append('}');
                break;
            case "Negation":
                sb.Append("{-");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append('}');
                break;
            case "Fraction":
                sb.Append("\\frac{");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("}{");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append('}');
                break;
            case "Power":
                sb.Append('{');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("}^{");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append('}');
                break;
            case "SquareRoot":
                sb.Append("\\sqrt{");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append('}');
                break;
            case "Root":
                sb.Append("\\sqrt[");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[1]);
                    sb.Append("]{");
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append('}');
                break;
            case "Subscript":
                sb.Append('{');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("}_{");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append('}');
                break;
            case "Superscript":
                sb.Append('{');
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("}^{");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append('}');
                break;
            case "Integral":
                sb.Append("\\int");
                if (node.Children is { Count: > 0 })
                {
                    sb.Append(" ");
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append("\\,d");
                sb.Append(EscapeLatexIdent(node.Value));
                break;
            case "Summation":
                sb.Append("\\sum");
                if (node.Children is { Count: > 0 })
                {
                    sb.Append(" ");
                    AppendNode(sb, node.Children[0]);
                }
                break;
            case "Product":
                sb.Append("\\prod");
                if (node.Children is { Count: > 0 })
                {
                    sb.Append(" ");
                    AppendNode(sb, node.Children[0]);
                }
                break;
            case "FunctionCall":
                sb.Append(MapFunctionName(node.Value));
                sb.Append('{');
                if (node.Children is { Count: > 0 })
                {
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        AppendNode(sb, node.Children[i]);
                    }
                }
                sb.Append('}');
                break;
            default:
                sb.Append("\\mathrm{");
                sb.Append(EscapeLatexIdent(node.NodeType));
                sb.Append("}{");
                sb.Append(EscapeLatexIdent(node.Value));
                sb.Append("}");
                break;
        }
    }

    private static string MapOperator(string op)
    {
        return op switch
        {
            "+" => "+",
            "-" => "-",
            "*" => "\\cdot ",
            "/" => "/",
            "^" => "^",
            "_" => "_",
            "=" => "=",
            "<" => "<",
            ">" => ">",
            "<=" => "\\leq ",
            ">=" => "\\geq ",
            "!=" => "\\neq ",
            "and" => "\\land ",
            "or" => "\\lor ",
            "not" => "\\neg ",
            _ => op
        };
    }

    private static string MapFunctionName(string name)
    {
        return name switch
        {
            "sin" => "\\sin",
            "cos" => "\\cos",
            "tan" => "\\tan",
            "log" => "\\log",
            "ln" => "\\ln",
            "exp" => "\\exp",
            "sqrt" => "\\sqrt",
            "abs" => "\\lvert",
            "max" => "\\max",
            "min" => "\\min",
            _ => "\\mathrm{" + EscapeLatexIdent(name) + "}"
        };
    }

    private static string EscapeLatexNumber(string value)
    {
        return value.Replace(".", "{.}");
    }

    private static string EscapeLatexIdent(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z]+$"))
        {
            return value;
        }
        return "\\mathrm{" + value + "}";
    }

    private static List<string> Tokenize(string latex)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < latex.Length)
        {
            if (char.IsWhiteSpace(latex[i]))
            {
                i++;
                continue;
            }

            if (latex[i] == '\\')
            {
                int start = i;
                i++;
                while (i < latex.Length && char.IsLetter(latex[i]))
                {
                    i++;
                }
                tokens.Add(latex.Substring(start, i - start));
                continue;
            }

            if (latex[i] == '{' || latex[i] == '}' || latex[i] == '(' || latex[i] == ')' ||
                latex[i] == '[' || latex[i] == ']' || latex[i] == ',' || latex[i] == ';' ||
                latex[i] == '^' || latex[i] == '_' || latex[i] == '+' || latex[i] == '-' ||
                latex[i] == '*' || latex[i] == '/' || latex[i] == '=' || latex[i] == '<' ||
                latex[i] == '>' || latex[i] == '!')
            {
                if (i + 1 < latex.Length && (latex[i] == '<' || latex[i] == '>' || latex[i] == '!') && latex[i + 1] == '=')
                {
                    tokens.Add(latex.Substring(i, 2));
                    i += 2;
                }
                else
                {
                    tokens.Add(latex[i].ToString());
                    i++;
                }
                continue;
            }

            int numStart = i;
            while (i < latex.Length && (char.IsDigit(latex[i]) || latex[i] == '.'))
            {
                i++;
            }
            if (i > numStart)
            {
                tokens.Add(latex.Substring(numStart, i - numStart));
                continue;
            }

            int identStart = i;
            while (i < latex.Length && (char.IsLetterOrDigit(latex[i]) || latex[i] == '_'))
            {
                i++;
            }
            if (i > identStart)
            {
                tokens.Add(latex.Substring(identStart, i - identStart));
                continue;
            }

            tokens.Add(latex[i].ToString());
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

        while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/" || tokens[pos] == "\\cdot"))
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

        if (tokens[pos] == "{")
        {
            pos++;
            var expr = ParseExpression(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == "}")
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

        if (tokens[pos] == "\\frac")
        {
            pos++;
            var numerator = ParseFactor(tokens, ref pos);
            var denominator = ParseFactor(tokens, ref pos);
            return new ExpressionNode("Fraction", "/", new[] { numerator, denominator });
        }

        if (tokens[pos] == "\\sqrt")
        {
            pos++;
            if (pos < tokens.Count && tokens[pos] == "[")
            {
                pos++;
                var degree = ParseExpression(tokens, ref pos);
                if (pos < tokens.Count && tokens[pos] == "]")
                {
                    pos++;
                }
                var radicand = ParseFactor(tokens, ref pos);
                return new ExpressionNode("Root", "root", new[] { radicand, degree });
            }
            var content = ParseFactor(tokens, ref pos);
            return new ExpressionNode("SquareRoot", "sqrt", new[] { content });
        }

        if (tokens[pos] == "\\sin" || tokens[pos] == "\\cos" || tokens[pos] == "\\tan" ||
            tokens[pos] == "\\log" || tokens[pos] == "\\ln" || tokens[pos] == "\\exp")
        {
            var funcName = tokens[pos].TrimStart('\\');
            pos++;
            var arg = ParseFactor(tokens, ref pos);
            return new ExpressionNode("FunctionCall", funcName, new[] { arg });
        }

        if (tokens[pos] == "\\mathrm")
        {
            pos++;
            if (pos < tokens.Count && tokens[pos] == "{")
            {
                pos++;
                var name = "";
                if (pos < tokens.Count && tokens[pos] != "}")
                {
                    name = tokens[pos];
                    pos++;
                }
                if (pos < tokens.Count && tokens[pos] == "}")
                {
                    pos++;
                }
                if (pos < tokens.Count && tokens[pos] == "{")
                {
                    pos++;
                    var val = "";
                    if (pos < tokens.Count && tokens[pos] != "}")
                    {
                        val = tokens[pos];
                        pos++;
                    }
                    if (pos < tokens.Count && tokens[pos] == "}")
                    {
                        pos++;
                    }
                    return new ExpressionNode(name, val);
                }
                return new ExpressionNode("Variable", name);
            }
        }

        if (tokens[pos] == "\\int")
        {
            pos++;
            var varName = "x";
            if (pos < tokens.Count && tokens[pos] == "\\,d")
            {
                pos++;
                if (pos < tokens.Count)
                {
                    varName = tokens[pos];
                    pos++;
                }
            }
            else if (pos < tokens.Count && tokens[pos].StartsWith("\\,d", StringComparison.Ordinal))
            {
                varName = tokens[pos].Substring(3);
                pos++;
            }
            return new ExpressionNode("Integral", varName);
        }

        if (tokens[pos] == "\\sum")
        {
            pos++;
            return new ExpressionNode("Summation", "sum");
        }

        if (tokens[pos] == "\\prod")
        {
            pos++;
            return new ExpressionNode("Product", "prod");
        }

        if (double.TryParse(tokens[pos], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            var num = tokens[pos];
            pos++;
            return new ExpressionNode("Number", num);
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

        var ident = tokens[pos];
        pos++;

        if (pos < tokens.Count && tokens[pos] == "_")
        {
            pos++;
            var sub = ParseFactor(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == "^")
            {
                pos++;
                var sup = ParseFactor(tokens, ref pos);
                return new ExpressionNode("Subscript", ident, new[]
                {
                    new ExpressionNode("Variable", ident),
                    sub,
                    sup
                });
            }
            return new ExpressionNode("Subscript", ident, new[]
            {
                new ExpressionNode("Variable", ident),
                sub
            });
        }

        if (pos < tokens.Count && tokens[pos] == "^")
        {
            pos++;
            var sup = ParseFactor(tokens, ref pos);
            return new ExpressionNode("Power", "^", new[]
            {
                new ExpressionNode("Variable", ident),
                sup
            });
        }

        return new ExpressionNode("Variable", ident);
    }
}
