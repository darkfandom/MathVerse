namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Represents a computed fingerprint for an expression tree, useful for deduplication and caching.
/// </summary>
public sealed class ExpressionFingerprint
{
    /// <summary>
    /// Gets the hex-encoded hash of the expression.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Gets the total number of nodes in the expression tree.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the maximum depth of the expression tree.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets the total node count (equivalent to <see cref="Size"/>).
    /// </summary>
    public int NodeCount { get; }

    private ExpressionFingerprint(string hash, int size, int depth, int nodeCount)
    {
        Hash = hash;
        Size = size;
        Depth = depth;
        NodeCount = nodeCount;
    }

    /// <summary>
    /// Computes a fingerprint for the given expression node.
    /// </summary>
    /// <param name="expression">The expression node to fingerprint.</param>
    /// <returns>The computed fingerprint.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public static ExpressionFingerprint Compute(ExpressionNode expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));

        var sb = new StringBuilder();
        int nodeCount = 0;
        int depth = ComputeSubtree(expression, sb, 0, ref nodeCount);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return new ExpressionFingerprint(hash, nodeCount, depth, nodeCount);
    }

    private static int ComputeSubtree(ExpressionNode node, StringBuilder sb, int currentDepth, ref int count)
    {
        count++;

        sb.Append(node.NodeType);
        sb.Append('(');
        sb.Append(node.Value ?? string.Empty);
        sb.Append(')');

        int maxChildDepth = currentDepth;

        if (node.Children is { Count: > 0 })
        {
            sb.Append('[');
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (i > 0) sb.Append(',');
                int childDepth = ComputeSubtree(node.Children[i], sb, currentDepth + 1, ref count);
                if (childDepth > maxChildDepth)
                {
                    maxChildDepth = childDepth;
                }
            }
            sb.Append(']');
        }

        if (node.Metadata is { Count: > 0 })
        {
            sb.Append('{');
            foreach (var kvp in node.Metadata)
            {
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.Append(kvp.Value ?? string.Empty);
                sb.Append(';');
            }
            sb.Append('}');
        }

        return maxChildDepth;
    }
}
