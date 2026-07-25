namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core;

/// <summary>
/// Represents a node in an Abstract Syntax Tree.
/// </summary>
/// <param name="Operator">The operator or node type identifier.</param>
/// <param name="Operands">The operand nodes of this AST node.</param>
/// <param name="Attributes">Optional attributes dictionary attached to the node.</param>
public sealed record ASTNode(
    string Operator,
    IReadOnlyList<ASTNode> Operands,
    IReadOnlyDictionary<string, string>? Attributes)
{
    /// <summary>
    /// Initializes a leaf AST node with no operands and no attributes.
    /// </summary>
    /// <param name="op">The operator or node type identifier.</param>
    public ASTNode(string op)
        : this(op, Array.Empty<ASTNode>(), null)
    {
    }

    /// <summary>
    /// Initializes an AST node with operands but no attributes.
    /// </summary>
    /// <param name="op">The operator or node type identifier.</param>
    /// <param name="operands">The operand nodes.</param>
    public ASTNode(string op, IReadOnlyList<ASTNode> operands)
        : this(op, operands, null)
    {
    }
}

/// <summary>
/// Serializes and deserializes Abstract Syntax Tree nodes.
/// </summary>
public sealed class ASTSerializer
{
    private const string MagicBytes = "MVAS";
    private const int FormatVersion = 1;

    /// <summary>
    /// Serializes an AST node to a byte array.
    /// </summary>
    /// <param name="node">The AST node to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the serialized byte array.</returns>
    public InteropResult<byte[]> Serialize(ASTNode node)
    {
        if (node is null)
        {
            return InteropResult<byte[]>.Failure("AST node cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            writer.Write(MagicBytes);
            writer.Write(FormatVersion);
            WriteNode(writer, node);

            writer.Flush();
            sw.Stop();
            return InteropResult<byte[]>.Success(stream.ToArray(), duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<byte[]>.Failure($"AST serialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an AST node from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized AST.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized AST node.</returns>
    public InteropResult<ASTNode> Deserialize(byte[] data)
    {
        if (data is null || data.Length == 0)
        {
            return InteropResult<ASTNode>.Failure("Data cannot be null or empty.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            var magic = reader.ReadString();
            if (magic != MagicBytes)
            {
                return InteropResult<ASTNode>.Failure("Invalid magic bytes in header.");
            }

            var version = reader.ReadInt32();
            if (version != FormatVersion)
            {
                return InteropResult<ASTNode>.Failure($"Unsupported format version: {version}.");
            }

            var node = ReadNode(reader);
            sw.Stop();
            return InteropResult<ASTNode>.Success(node, duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<ASTNode>.Failure($"AST deserialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an AST node to a human-readable text representation.
    /// </summary>
    /// <param name="node">The AST node to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the text representation.</returns>
    public InteropResult<string> SerializeToText(ASTNode node)
    {
        if (node is null)
        {
            return InteropResult<string>.Failure("AST node cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var sb = new StringBuilder();
            WriteNodeToText(sb, node, 0);
            sw.Stop();
            return InteropResult<string>.Success(sb.ToString(), duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<string>.Failure($"AST text serialization failed: {ex.Message}", ex);
        }
    }

    private static void WriteNode(BinaryWriter writer, ASTNode node)
    {
        writer.Write(node.Operator ?? string.Empty);

        var operands = node.Operands ?? Array.Empty<ASTNode>();
        writer.Write(operands.Count);
        foreach (var operand in operands)
        {
            WriteNode(writer, operand);
        }

        var attributes = node.Attributes;
        if (attributes is null)
        {
            writer.Write(0);
        }
        else
        {
            writer.Write(attributes.Count);
            foreach (var kvp in attributes)
            {
                writer.Write(kvp.Key ?? string.Empty);
                writer.Write(kvp.Value ?? string.Empty);
            }
        }
    }

    private static ASTNode ReadNode(BinaryReader reader)
    {
        var op = reader.ReadString();

        var operandCount = reader.ReadInt32();
        var operands = new ASTNode[operandCount];
        for (int i = 0; i < operandCount; i++)
        {
            operands[i] = ReadNode(reader);
        }

        var attrCount = reader.ReadInt32();
        Dictionary<string, string>? attributes = null;
        if (attrCount > 0)
        {
            attributes = new Dictionary<string, string>(attrCount);
            for (int i = 0; i < attrCount; i++)
            {
                var key = reader.ReadString();
                var val = reader.ReadString();
                attributes[key] = val;
            }
        }

        return new ASTNode(op, operands, attributes);
    }

    private static void WriteNodeToText(StringBuilder sb, ASTNode node, int indent)
    {
        var pad = new string(' ', indent * 2);
        sb.Append(pad);
        sb.Append('(');
        sb.Append(node.Operator ?? string.Empty);

        if (node.Operands is { Count: > 0 })
        {
            foreach (var operand in node.Operands)
            {
                sb.AppendLine();
                WriteNodeToText(sb, operand, indent + 1);
            }
        }

        if (node.Attributes is { Count: > 0 })
        {
            sb.Append(" [");
            bool first = true;
            foreach (var kvp in node.Attributes)
            {
                if (!first) sb.Append(", ");
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.Append(kvp.Value ?? string.Empty);
                first = false;
            }
            sb.Append(']');
        }

        sb.Append(')');
    }
}
