namespace MathVerse.Math.Interop.MathematicalMarkup;

using System;
using System.Collections.Generic;
using System.Text;
using ExpressionExchange;

/// <summary>
/// Bidirectional converter between MathVerse expression nodes and OpenMath CD-based markup.
/// </summary>
public sealed class OpenMathConverter
{
    private static readonly Dictionary<string, string> CommonCDMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "+", "plus" },
        { "-", "minus" },
        { "*", "times" },
        { "/", "divide" },
        { "=", "eq" },
        { "<", "lt" },
        { ">", "gt" },
        { "<=", "leq" },
        { ">=", "geq" },
        { "!=", "neq" },
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
        { "and", "and" },
        { "or", "or" },
        { "not", "not" },
        { "pi", "pi" },
        { "e", "e" },
        { "i", "imaginaryi" },
        { "infinity", "infinity" },
        { "true", "true" },
        { "false", "false" }
    };

    /// <summary>
    /// Converts an expression node to OpenMath XML markup.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A string containing the OpenMath XML representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public string ToOpenMath(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var sb = new StringBuilder();
        sb.Append("<OMOBJ xmlns=\"http://www.openmath.org/OpenMath\">");
        AppendNode(sb, expression);
        sb.Append("</OMOBJ>");
        return sb.ToString();
    }

    /// <summary>
    /// Converts an OpenMath XML string to an expression node.
    /// </summary>
    /// <param name="openmath">The OpenMath XML string.</param>
    /// <returns>The resulting expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="openmath"/> is null or empty.</exception>
    public ExpressionNode FromOpenMath(string openmath)
    {
        if (string.IsNullOrWhiteSpace(openmath))
        {
            throw new ArgumentException("OpenMath string cannot be null or empty.", nameof(openmath));
        }

        var trimmed = openmath.Trim();
        int bodyStart = FindTagEnd(trimmed, 0);
        int bodyEnd = FindCloseTag(trimmed, bodyStart);

        if (bodyStart < 0 || bodyEnd <= bodyStart)
        {
            return new ExpressionNode("OpenMath", trimmed);
        }

        var body = trimmed.Substring(bodyStart, bodyEnd - bodyStart);
        return ParseOMOBJ(body, 0);
    }

    private static void AppendNode(StringBuilder sb, ExpressionNode node)
    {
        switch (node.NodeType)
        {
            case "Number":
                sb.Append("<OMI>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</OMI>");
                break;
            case "Float":
                sb.Append("<OMF dec=\"");
                sb.Append(EscapeXml(node.Value));
                sb.Append("\"/>");
                break;
            case "Variable":
                sb.Append("<OME><OMS cd=\"variables\" name=\"");
                sb.Append(EscapeXml(node.Value));
                sb.Append("\"/></OME>");
                break;
            case "Symbol":
                sb.Append("<OMS cd=\"");
                sb.Append(EscapeXml(GetCD(node.Value)));
                sb.Append("\" name=\"");
                sb.Append(EscapeXml(node.Value));
                sb.Append("\"/>");
                break;
            case "BinaryOp":
                sb.Append("<OMB><OMS cd=\"");
                sb.Append(EscapeXml(GetCD(node.Value)));
                sb.Append("\" name=\"");
                sb.Append(EscapeXml(MapOperatorName(node.Value)));
                sb.Append("\"/>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</OMB>");
                break;
            case "Negation":
                sb.Append("<OMB><OMS cd=\"arith1\" name=\"minus\"/>");
                sb.Append("<OMI>0</OMI>");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append("</OMB>");
                break;
            case "FunctionCall":
                sb.Append("<OMB><OMS cd=\"");
                sb.Append(EscapeXml(GetCD(node.Value)));
                sb.Append("\" name=\"");
                sb.Append(EscapeXml(MapFunctionName(node.Value)));
                sb.Append("\"/>");
                if (node.Children is { Count: > 0 })
                {
                    foreach (var child in node.Children)
                    {
                        AppendNode(sb, child);
                    }
                }
                sb.Append("</OMB>");
                break;
            case "Application":
                sb.Append("<OMB>");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                    for (int i = 1; i < node.Children.Count; i++)
                    {
                        AppendNode(sb, node.Children[i]);
                    }
                }
                sb.Append("</OMB>");
                break;
            default:
                sb.Append("<OMA><OMS cd=\"");
                sb.Append(EscapeXml(node.NodeType));
                sb.Append("\" name=\"");
                sb.Append(EscapeXml(node.Value));
                sb.Append("\"/>");
                if (node.Children is { Count: > 0 })
                {
                    foreach (var child in node.Children)
                    {
                        AppendNode(sb, child);
                    }
                }
                sb.Append("</OMA>");
                break;
        }
    }

    private static string GetCD(string symbol)
    {
        if (CommonCDMap.TryGetValue(symbol, out var cd))
        {
            return cd;
        }

        if (symbol is "sin" or "cos" or "tan" or "log" or "ln" or "exp" or "sqrt" or "abs" or "max" or "min")
        {
            return "transc1";
        }

        if (symbol is "+" or "-" or "*" or "/")
        {
            return "arith1";
        }

        if (symbol is "=" or "<" or ">" or "<=" or ">=" or "!=")
        {
            return "relation1";
        }

        if (symbol is "and" or "or" or "not")
        {
            return "logic1";
        }

        if (symbol is "pi" or "e" or "infinity")
        {
            return "nums1";
        }

        return "unknown";
    }

    private static string MapOperatorName(string op)
    {
        return op switch
        {
            "+" => "plus",
            "-" => "minus",
            "*" => "times",
            "/" => "divide",
            "=" => "eq",
            "<" => "lt",
            ">" => "gt",
            "<=" => "leq",
            ">=" => "geq",
            "!=" => "neq",
            _ => op
        };
    }

    private static string MapFunctionName(string name)
    {
        return CommonCDMap.TryGetValue(name, out var mapped) ? mapped : name;
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static int FindTagEnd(string text, int start)
    {
        int idx = text.IndexOf('>', start);
        return idx >= 0 ? idx + 1 : -1;
    }

    private static int FindCloseTag(string text, int start)
    {
        if (start < 0) return -1;
        int idx = text.LastIndexOf('<');
        return idx > start ? idx : text.Length;
    }

    private static ExpressionNode ParseOMOBJ(string content, int depth)
    {
        if (depth > 50)
        {
            return new ExpressionNode("OpenMath", content.Trim());
        }

        var trimmed = content.Trim();

        if (trimmed.StartsWith("<OMI>", StringComparison.Ordinal) && trimmed.EndsWith("</OMI>", StringComparison.Ordinal))
        {
            return new ExpressionNode("Number", UnescapeXml(trimmed.Substring(5, trimmed.Length - 11)));
        }

        if (trimmed.StartsWith("<OMF", StringComparison.Ordinal) && trimmed.EndsWith("/>", StringComparison.Ordinal))
        {
            int decIdx = trimmed.IndexOf("dec=\"", StringComparison.Ordinal);
            if (decIdx >= 0)
            {
                int valStart = decIdx + 5;
                int valEnd = trimmed.IndexOf('"', valStart);
                if (valEnd > valStart)
                {
                    return new ExpressionNode("Float", trimmed.Substring(valStart, valEnd - valStart));
                }
            }
        }

        if (trimmed.StartsWith("<OME>", StringComparison.Ordinal) && trimmed.EndsWith("</OME>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(5, trimmed.Length - 11);
            int nameIdx = inner.IndexOf("name=\"", StringComparison.Ordinal);
            if (nameIdx >= 0)
            {
                int valStart = nameIdx + 6;
                int valEnd = inner.IndexOf('"', valStart);
                if (valEnd > valStart)
                {
                    return new ExpressionNode("Variable", inner.Substring(valStart, valEnd - valStart));
                }
            }
        }

        if (trimmed.StartsWith("<OMB>", StringComparison.Ordinal) && trimmed.EndsWith("</OMB>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(5, trimmed.Length - 11);
            return new ExpressionNode("Application", "apply",
                new[] { ParseOMOBJ(inner, depth + 1) });
        }

        if (trimmed.StartsWith("<OMA>", StringComparison.Ordinal) && trimmed.EndsWith("</OMA>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(5, trimmed.Length - 11);
            var children = SplitTopLevel(inner);
            if (children.Count >= 2)
            {
                return new ExpressionNode("Application", children[0].Trim(),
                    children.GetRange(1, children.Count - 1).ConvertAll(c => ParseOMOBJ(c, depth + 1)));
            }
        }

        return new ExpressionNode("OpenMath", trimmed);
    }

    private static List<string> SplitTopLevel(string content)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;
        bool inTag = false;

        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '<')
            {
                if (!inTag && depth == 0 && i > start)
                {
                    var segment = content.Substring(start, i - start).Trim();
                    if (segment.Length > 0) result.Add(segment);
                }
                inTag = true;
                if (i + 1 < content.Length && content[i + 1] == '/')
                {
                    depth--;
                }
                else
                {
                    depth++;
                }
            }
            else if (content[i] == '>' && inTag)
            {
                inTag = false;
                if (depth == 0)
                {
                    start = i + 1;
                }
            }
        }

        if (start < content.Length)
        {
            var remaining = content.Substring(start).Trim();
            if (remaining.Length > 0) result.Add(remaining);
        }

        return result;
    }

    private static string UnescapeXml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'");
    }
}
