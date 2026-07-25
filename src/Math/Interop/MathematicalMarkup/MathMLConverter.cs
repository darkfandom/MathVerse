namespace MathVerse.Math.Interop.MathematicalMarkup;

using System;
using System.Collections.Generic;
using System.Text;
using ExpressionExchange;

/// <summary>
/// Bidirectional converter between MathVerse expression nodes and MathML 3.0 markup.
/// </summary>
public sealed class MathMLConverter
{
    /// <summary>
    /// Converts an expression node to MathML 3.0 XML markup.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A string containing the MathML markup.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public string ToMathML(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var sb = new StringBuilder();
        sb.Append("<math xmlns=\"http://www.w3.org/1998/Math/MathML\">");
        AppendNode(sb, expression);
        sb.Append("</math>");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a MathML string to an expression node.
    /// </summary>
    /// <param name="mathml">The MathML XML string.</param>
    /// <returns>The resulting expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mathml"/> is null or empty.</exception>
    public ExpressionNode FromMathML(string mathml)
    {
        if (string.IsNullOrWhiteSpace(mathml))
        {
            throw new ArgumentException("MathML string cannot be null or empty.", nameof(mathml));
        }

        var trimmed = mathml.Trim();
        int bodyStart = FindBodyStart(trimmed);
        int bodyEnd = FindBodyEnd(trimmed, bodyStart);

        if (bodyStart < 0 || bodyEnd <= bodyStart)
        {
            return new ExpressionNode("MathML", trimmed);
        }

        var body = trimmed.Substring(bodyStart, bodyEnd - bodyStart);
        return ParseMathMLElement(body, 0);
    }

    private static void AppendNode(StringBuilder sb, ExpressionNode node)
    {
        switch (node.NodeType)
        {
            case "Number":
                sb.Append("<mn>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mn>");
                break;
            case "Variable":
                sb.Append("<mi>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mi>");
                break;
            case "Operator":
                sb.Append("<mo>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mo>");
                break;
            case "FunctionCall":
                sb.Append("<mrow><mi>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mi><mo>(</mo>");
                if (node.Children is { Count: > 0 })
                {
                    AppendChildren(sb, node.Children);
                }
                sb.Append("<mo>)</mo></mrow>");
                break;
            case "BinaryOp":
                sb.Append("<mrow>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    sb.Append("<mo>");
                    sb.Append(EscapeXml(node.Value));
                    sb.Append("</mo>");
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</mrow>");
                break;
            case "Negation":
                sb.Append("<mrow><mo>-</mo>");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append("</mrow>");
                break;
            case "Fraction":
                sb.Append("<mfrac>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</mfrac>");
                break;
            case "Power":
                sb.Append("<msup>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</msup>");
                break;
            case "SquareRoot":
                sb.Append("<msqrt>");
                if (node.Children is { Count: > 0 })
                {
                    AppendNode(sb, node.Children[0]);
                }
                sb.Append("</msqrt>");
                break;
            case "Root":
                sb.Append("<mroot>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</mroot>");
                break;
            case "Subscript":
                sb.Append("<msub>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</msub>");
                break;
            case "Superscript":
                sb.Append("<msup>");
                if (node.Children is { Count: >= 2 })
                {
                    AppendNode(sb, node.Children[0]);
                    AppendNode(sb, node.Children[1]);
                }
                sb.Append("</msup>");
                break;
            case "Integral":
                sb.Append("<mrow><mo>∫</mo>");
                if (node.Children is { Count: > 0 })
                {
                    AppendChildren(sb, node.Children);
                }
                sb.Append("<mo>𝑑</mo><mi>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mi></mrow>");
                break;
            case "Summation":
                sb.Append("<mrow><mo>∑</mo>");
                if (node.Children is { Count: > 0 })
                {
                    AppendChildren(sb, node.Children);
                }
                sb.Append("</mrow>");
                break;
            case "Product":
                sb.Append("<mrow><mo>∏</mo>");
                if (node.Children is { Count: > 0 })
                {
                    AppendChildren(sb, node.Children);
                }
                sb.Append("</mrow>");
                break;
            default:
                sb.Append("<mrow><mi>");
                sb.Append(EscapeXml(node.NodeType));
                sb.Append("</mi><mo>(</mo>");
                sb.Append("<mi>");
                sb.Append(EscapeXml(node.Value));
                sb.Append("</mi>");
                if (node.Children is { Count: > 0 })
                {
                    sb.Append("<mo>,</mo>");
                    AppendChildren(sb, node.Children);
                }
                sb.Append("<mo>)</mo></mrow>");
                break;
        }
    }

    private static void AppendChildren(StringBuilder sb, IReadOnlyList<ExpressionNode> children)
    {
        for (int i = 0; i < children.Count; i++)
        {
            if (i > 0) sb.Append("<mo>,</mo>");
            AppendNode(sb, children[i]);
        }
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

    private static int FindBodyStart(string mathml)
    {
        int idx = mathml.IndexOf('>');
        return idx >= 0 ? idx + 1 : -1;
    }

    private static int FindBodyEnd(string mathml, int start)
    {
        if (start < 0) return -1;
        int closeIdx = mathml.LastIndexOf('<');
        return closeIdx > start ? closeIdx : mathml.Length;
    }

    private static ExpressionNode ParseMathMLElement(string content, int depth)
    {
        if (depth > 50)
        {
            return new ExpressionNode("Unknown", content.Trim());
        }

        var trimmed = content.Trim();

        if (trimmed.StartsWith("<mn>", StringComparison.Ordinal) && trimmed.EndsWith("</mn>", StringComparison.Ordinal))
        {
            return new ExpressionNode("Number", UnescapeXml(trimmed.Substring(4, trimmed.Length - 9)));
        }

        if (trimmed.StartsWith("<mi>", StringComparison.Ordinal) && trimmed.EndsWith("</mi>", StringComparison.Ordinal))
        {
            return new ExpressionNode("Variable", UnescapeXml(trimmed.Substring(4, trimmed.Length - 9)));
        }

        if (trimmed.StartsWith("<mo>", StringComparison.Ordinal) && trimmed.EndsWith("</mo>", StringComparison.Ordinal))
        {
            return new ExpressionNode("Operator", UnescapeXml(trimmed.Substring(4, trimmed.Length - 9)));
        }

        if (trimmed.StartsWith("<mfrac>", StringComparison.Ordinal) && trimmed.EndsWith("</mfrac>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(7, trimmed.Length - 15);
            var children = SplitTopLevel(inner);
            if (children.Count >= 2)
            {
                return new ExpressionNode("Fraction", "/", new[]
                {
                    ParseMathMLElement(children[0], depth + 1),
                    ParseMathMLElement(children[1], depth + 1)
                });
            }
        }

        if (trimmed.StartsWith("<msup>", StringComparison.Ordinal) && trimmed.EndsWith("</msup>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(6, trimmed.Length - 13);
            var children = SplitTopLevel(inner);
            if (children.Count >= 2)
            {
                return new ExpressionNode("Power", "^", new[]
                {
                    ParseMathMLElement(children[0], depth + 1),
                    ParseMathMLElement(children[1], depth + 1)
                });
            }
        }

        if (trimmed.StartsWith("<msqrt>", StringComparison.Ordinal) && trimmed.EndsWith("</msqrt>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(7, trimmed.Length - 15);
            return new ExpressionNode("SquareRoot", "sqrt", new[]
            {
                ParseMathMLElement(inner, depth + 1)
            });
        }

        if (trimmed.StartsWith("<mrow>", StringComparison.Ordinal) && trimmed.EndsWith("</mrow>", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(6, trimmed.Length - 13);
            var children = SplitTopLevel(inner);
            if (children.Count == 1)
            {
                return ParseMathMLElement(children[0], depth + 1);
            }

            return new ExpressionNode("Group", "group", children.ConvertAll(c => ParseMathMLElement(c, depth + 1)));
        }

        return new ExpressionNode("MathML", trimmed);
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
