namespace MathVerse.Math.Interop.AIModelExchange;

using System;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

/// <summary>
/// Serializes and deserializes AI model architectures and weights.
/// </summary>
public sealed class ModelSerializer
{
    private const int FormatVersion = 1;

    /// <summary>
    /// Serializes a model descriptor to a binary byte array.
    /// </summary>
    /// <param name="model">The model descriptor to serialize.</param>
    /// <returns>A byte array containing the serialized model data.</returns>
    public byte[] SerializeModel(ModelDescriptor model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var nameBytes = Encoding.UTF8.GetBytes(model.Name);
        var versionBytes = Encoding.UTF8.GetBytes(model.Version);
        var archBytes = Encoding.UTF8.GetBytes(model.Architecture);
        var metadataJson = SerializeMetadata(model.Metadata);
        var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);

        var totalSize = sizeof(int) + nameBytes.Length +
                        sizeof(int) + versionBytes.Length +
                        sizeof(int) + archBytes.Length +
                        sizeof(int) + metadataBytes.Length +
                        sizeof(int) + model.Weights.Length;
        var buffer = new byte[totalSize];
        var offset = 0;

        WriteString(buffer, ref offset, nameBytes);
        WriteString(buffer, ref offset, versionBytes);
        WriteString(buffer, ref offset, archBytes);
        WriteString(buffer, ref offset, metadataBytes);

        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), model.Weights.Length);
        offset += sizeof(int);
        Buffer.BlockCopy(model.Weights, 0, buffer, offset, model.Weights.Length);

        return buffer;
    }

    /// <summary>
    /// Deserializes a model descriptor from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized model data.</param>
    /// <returns>The deserialized model descriptor.</returns>
    public ModelDescriptor DeserializeModel(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;

        var nameBytes = ReadString(data, ref offset);
        var versionBytes = ReadString(data, ref offset);
        var archBytes = ReadString(data, ref offset);
        var metadataBytes = ReadString(data, ref offset);

        var weightsLen = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var weights = new byte[weightsLen];
        Buffer.BlockCopy(data, offset, weights, 0, weightsLen);

        return new ModelDescriptor
        {
            Name = Encoding.UTF8.GetString(nameBytes),
            Version = Encoding.UTF8.GetString(versionBytes),
            Architecture = Encoding.UTF8.GetString(archBytes),
            Metadata = DeserializeMetadata(Encoding.UTF8.GetString(metadataBytes)),
            Weights = weights
        };
    }

    /// <summary>
    /// Serializes model metadata to a JSON string.
    /// </summary>
    /// <param name="metadata">The metadata to serialize.</param>
    /// <returns>A JSON string representing the metadata.</returns>
    public string SerializeMetadata(ModelMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var sb = new StringBuilder();
        sb.Append('{');
        sb.AppendFormat("\"Author\":\"{0}\"", EscapeJson(metadata.Author));
        sb.AppendFormat(",\"Description\":\"{0}\"", EscapeJson(metadata.Description));
        sb.AppendFormat(",\"Created\":\"{0}\"", metadata.Created.ToString("O"));
        sb.Append(",\"Properties\":{");
        bool first = true;
        foreach (var kvp in metadata.Properties)
        {
            if (!first) sb.Append(',');
            sb.AppendFormat("\"{0}\":\"{1}\"", EscapeJson(kvp.Key), EscapeJson(kvp.Value));
            first = false;
        }
        sb.Append("}}");
        return sb.ToString();
    }

    /// <summary>
    /// Deserializes model metadata from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized model metadata.</returns>
    public ModelMetadata DeserializeMetadata(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new ModelMetadata();
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var metadata = new ModelMetadata();

        if (root.TryGetProperty("Author", out var author))
        {
            metadata.Author = author.GetString() ?? string.Empty;
        }
        if (root.TryGetProperty("Description", out var desc))
        {
            metadata.Description = desc.GetString() ?? string.Empty;
        }
        if (root.TryGetProperty("Created", out var created) &&
            DateTimeOffset.TryParse(created.GetString(), out var dto))
        {
            metadata.Created = dto;
        }
        if (root.TryGetProperty("Properties", out var props))
        {
            foreach (var prop in props.EnumerateObject())
            {
                metadata.Properties[prop.Name] = prop.Value.GetString() ?? string.Empty;
            }
        }

        return metadata;
    }

    private static void WriteString(byte[] buffer, ref int offset, byte[] strBytes)
    {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), strBytes.Length);
        offset += sizeof(int);
        Buffer.BlockCopy(strBytes, 0, buffer, offset, strBytes.Length);
        offset += strBytes.Length;
    }

    private static byte[] ReadString(byte[] data, ref int offset)
    {
        var len = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var result = new byte[len];
        Buffer.BlockCopy(data, offset, result, 0, len);
        offset += len;
        return result;
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
