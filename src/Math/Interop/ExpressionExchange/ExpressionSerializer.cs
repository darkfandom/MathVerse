namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core;

/// <summary>
/// Represents a node in an expression tree.
/// </summary>
/// <param name="NodeType">The type of the expression node (e.g., "Number", "Variable", "BinaryOp", "FunctionCall").</param>
/// <param name="Value">The value associated with the node.</param>
/// <param name="Children">The child nodes of this expression node.</param>
/// <param name="Metadata">Optional metadata dictionary attached to the node.</param>
public sealed record ExpressionNode(
    string NodeType,
    string Value,
    IReadOnlyList<ExpressionNode> Children,
    IReadOnlyDictionary<string, string>? Metadata)
{
    /// <summary>
    /// Initializes a leaf node with no children and no metadata.
    /// </summary>
    /// <param name="nodeType">The type of the expression node.</param>
    /// <param name="value">The value associated with the node.</param>
    public ExpressionNode(string nodeType, string value)
        : this(nodeType, value, Array.Empty<ExpressionNode>(), null)
    {
    }

    /// <summary>
    /// Initializes an expression node with children but no metadata.
    /// </summary>
    /// <param name="nodeType">The type of the expression node.</param>
    /// <param name="value">The value associated with the node.</param>
    /// <param name="children">The child nodes.</param>
    public ExpressionNode(string nodeType, string value, IReadOnlyList<ExpressionNode> children)
        : this(nodeType, value, children, null)
    {
    }
}

/// <summary>
/// Serializes and deserializes expression trees to and from byte arrays and string representations.
/// </summary>
public sealed class ExpressionSerializer
{
    private const string MagicBytes = "MVE";
    private const int HeaderVersion = 1;

    /// <summary>
    /// Serializes an expression node to a byte array.
    /// </summary>
    /// <param name="expression">The expression node to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the serialized byte array.</returns>
    public InteropResult<byte[]> Serialize(ExpressionNode expression)
    {
        if (expression is null)
        {
            return InteropResult<byte[]>.Failure("Expression cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            writer.Write(MagicBytes);
            writer.Write(HeaderVersion);

            WriteNode(writer, expression);

            writer.Flush();
            sw.Stop();
            return InteropResult<byte[]>.Success(stream.ToArray(), duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<byte[]>.Failure($"Serialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an expression node to a human-readable string representation.
    /// </summary>
    /// <param name="expression">The expression node to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the serialized string.</returns>
    public InteropResult<string> SerializeToString(ExpressionNode expression)
    {
        if (expression is null)
        {
            return InteropResult<string>.Failure("Expression cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var sb = new StringBuilder();
            WriteNodeToString(sb, expression, 0);
            sw.Stop();
            return InteropResult<string>.Success(sb.ToString(), duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<string>.Failure($"String serialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an expression node from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized expression.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized expression node.</returns>
    public InteropResult<ExpressionNode> Deserialize(byte[] data)
    {
        if (data is null || data.Length == 0)
        {
            return InteropResult<ExpressionNode>.Failure("Data cannot be null or empty.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            var magic = reader.ReadString();
            if (magic != MagicBytes)
            {
                return InteropResult<ExpressionNode>.Failure("Invalid magic bytes in header.");
            }

            var version = reader.ReadInt32();
            if (version != HeaderVersion)
            {
                return InteropResult<ExpressionNode>.Failure($"Unsupported header version: {version}.");
            }

            var node = ReadNode(reader);
            sw.Stop();
            return InteropResult<ExpressionNode>.Success(node, duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<ExpressionNode>.Failure($"Deserialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an expression node from a string representation.
    /// </summary>
    /// <param name="text">The string containing the serialized expression.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized expression node.</returns>
    public InteropResult<ExpressionNode> DeserializeFromString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return InteropResult<ExpressionNode>.Failure("Text cannot be null or empty.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var result = Deserialize(bytes);
            sw.Stop();
            return result.IsSuccess
                ? InteropResult<ExpressionNode>.Success(result.Value!, duration: sw.Elapsed)
                : InteropResult<ExpressionNode>.Failure(result.ErrorMessage!, result.Error, result.Diagnostics);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<ExpressionNode>.Failure($"String deserialization failed: {ex.Message}", ex);
        }
    }

    private static void WriteNode(BinaryWriter writer, ExpressionNode node)
    {
        writer.Write(node.NodeType ?? string.Empty);
        writer.Write(node.Value ?? string.Empty);

        var children = node.Children ?? Array.Empty<ExpressionNode>();
        writer.Write(children.Count);
        foreach (var child in children)
        {
            WriteNode(writer, child);
        }

        var metadata = node.Metadata;
        if (metadata is null)
        {
            writer.Write(0);
        }
        else
        {
            writer.Write(metadata.Count);
            foreach (var kvp in metadata)
            {
                writer.Write(kvp.Key ?? string.Empty);
                writer.Write(kvp.Value ?? string.Empty);
            }
        }
    }

    private static ExpressionNode ReadNode(BinaryReader reader)
    {
        var nodeType = reader.ReadString();
        var value = reader.ReadString();

        var childCount = reader.ReadInt32();
        var children = new ExpressionNode[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = ReadNode(reader);
        }

        var metaCount = reader.ReadInt32();
        Dictionary<string, string>? metadata = null;
        if (metaCount > 0)
        {
            metadata = new Dictionary<string, string>(metaCount);
            for (int i = 0; i < metaCount; i++)
            {
                var key = reader.ReadString();
                var val = reader.ReadString();
                metadata[key] = val;
            }
        }

        return new ExpressionNode(nodeType, value, children, metadata);
    }

    private static void WriteNodeToString(StringBuilder sb, ExpressionNode node, int indent)
    {
        var pad = new string(' ', indent * 2);
        sb.Append(pad);
        sb.Append('(');
        sb.Append(node.NodeType);
        sb.Append(' ');
        sb.Append(node.Value);

        if (node.Children is { Count: > 0 })
        {
            sb.AppendLine();
            for (int i = 0; i < node.Children.Count; i++)
            {
                WriteNodeToString(sb, node.Children[i], indent + 1);
                if (i < node.Children.Count - 1)
                {
                    sb.AppendLine();
                }
            }
        }

        if (node.Metadata is { Count: > 0 })
        {
            sb.Append(" {");
            bool first = true;
            foreach (var kvp in node.Metadata)
            {
                if (!first) sb.Append(", ");
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.Append(kvp.Value);
                first = false;
            }
            sb.Append('}');
        }

        sb.Append(')');
    }
}
