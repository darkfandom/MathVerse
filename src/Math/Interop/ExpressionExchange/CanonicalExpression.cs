namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Converts expressions to a canonical (normalized) form for equivalence checking and comparison.
/// </summary>
public static class CanonicalExpression
{
    /// <summary>
    /// Normalizes an expression node into a canonical form.
    /// Commutative operations have their operands sorted lexicographically.
    /// </summary>
    /// <param name="expression">The expression node to normalize.</param>
    /// <returns>The normalized expression node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public static ExpressionNode Normalize(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        return NormalizeCore(expression);
    }

    /// <summary>
    /// Converts an expression node to its canonical string representation.
    /// </summary>
    /// <param name="expression">The expression node to convert.</param>
    /// <returns>A canonical string representation of the expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public static string ToCanonicalString(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var normalized = NormalizeCore(expression);
        var sb = new StringBuilder();
        WriteCanonical(sb, normalized);
        return sb.ToString();
    }

    /// <summary>
    /// Determines whether two expression nodes are structurally equivalent after normalization.
    /// </summary>
    /// <param name="a">The first expression node.</param>
    /// <param name="b">The second expression node.</param>
    /// <returns>True if the expressions are equivalent; otherwise, false.</returns>
    public static bool Equivalent(ExpressionNode a, ExpressionNode b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;

        return string.Equals(ToCanonicalString(a), ToCanonicalString(b), StringComparison.Ordinal);
    }

    private static ExpressionNode NormalizeCore(ExpressionNode node)
    {
        var children = node.Children ?? Array.Empty<ExpressionNode>();
        var normalizedChildren = new ExpressionNode[children.Count];

        for (int i = 0; i < children.Count; i++)
        {
            normalizedChildren[i] = NormalizeCore(children[i]);
        }

        if (IsCommutative(node.NodeType))
        {
            Array.Sort(normalizedChildren, (a, b) =>
            {
                var sa = ToCanonicalString(a);
                var sb2 = ToCanonicalString(b);
                return string.Compare(sa, sb2, StringComparison.Ordinal);
            });
        }

        var sortedMetadata = node.Metadata is { Count: > 0 }
            ? (IReadOnlyDictionary<string, string>)new SortedDictionary<string, string>(new Dictionary<string, string>(node.Metadata), StringComparer.Ordinal)
            : null;

        return new ExpressionNode(node.NodeType, node.Value, normalizedChildren, sortedMetadata);
    }

    private static bool IsCommutative(string nodeType)
    {
        return string.Equals(nodeType, "Add", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "Mul", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "And", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "Or", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "Addition", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeType, "Multiplication", StringComparison.OrdinalIgnoreCase);
    }

    private static void WriteCanonical(StringBuilder sb, ExpressionNode node)
    {
        sb.Append(node.NodeType);
        sb.Append('(');
        sb.Append(node.Value ?? string.Empty);

        if (node.Children is { Count: > 0 })
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                sb.Append(',');
                WriteCanonical(sb, node.Children[i]);
            }
        }

        if (node.Metadata is { Count: > 0 })
        {
            sb.Append('|');
            bool first = true;
            foreach (var kvp in node.Metadata)
            {
                if (!first) sb.Append(';');
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.Append(kvp.Value ?? string.Empty);
                first = false;
            }
        }

        sb.Append(')');
    }
}
