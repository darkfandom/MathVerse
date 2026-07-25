namespace MathVerse.Math.Interop.SimulationExchange;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

/// <summary>
/// Serializes and deserializes simulation state dictionaries using
/// a simple binary format with type tags for AOT safety.
/// </summary>
public sealed class StateSerializer
{
    private const int Magic = 0x53544154;

    private const int TypeTagNull = 0;
    private const int TypeTagInt = 1;
    private const int TypeTagDouble = 2;
    private const int TypeTagString = 3;
    private const int TypeTagBool = 4;
    private const int TypeTagDoubleArray = 5;
    private const int TypeTagIntArray = 6;
    private const int TypeTagObject = 7;

    /// <summary>
    /// Serializes a state dictionary to a binary byte array.
    /// </summary>
    /// <param name="state">The state dictionary to serialize.</param>
    /// <returns>A byte array containing the serialized state.</returns>
    public byte[] SerializeState(Dictionary<string, object> state)
    {
        ArgumentNullException.ThrowIfNull(state);

        using var ms = new System.IO.MemoryStream();

        WriteInt(ms, Magic);
        WriteInt(ms, state.Count);

        foreach (var kvp in state)
        {
            WriteString(ms, kvp.Key);
            WriteTypedValue(ms, kvp.Value);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes a state dictionary from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized state.</param>
    /// <returns>The deserialized state dictionary.</returns>
    public Dictionary<string, object> DeserializeState(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = ReadInt(data, ref offset);
        if (magic != Magic)
        {
            throw new FormatException("Invalid state data magic number.");
        }

        var count = ReadInt(data, ref offset);
        var state = new Dictionary<string, object>(count);

        for (var i = 0; i < count; i++)
        {
            var key = ReadString(data, ref offset);
            var value = ReadTypedValue(data, ref offset);
            state[key] = value;
        }

        return state;
    }

    /// <summary>
    /// Serializes a state dictionary to a JSON string.
    /// </summary>
    /// <param name="state">The state dictionary to serialize.</param>
    /// <returns>A JSON string representing the state.</returns>
    public string SerializeStateJson(Dictionary<string, object> state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var sb = new StringBuilder();
        sb.Append('{');
        bool first = true;
        foreach (var kvp in state)
        {
            if (!first) sb.Append(',');
            sb.AppendFormat("\"{0}\":", EscapeJson(kvp.Key));
            AppendJsonValue(sb, kvp.Value);
            first = false;
        }
        sb.Append('}');
        return sb.ToString();
    }

    private static void WriteTypedValue(System.IO.MemoryStream ms, object? value)
    {
        switch (value)
        {
            case null:
                WriteInt(ms, TypeTagNull);
                break;
            case int intVal:
                WriteInt(ms, TypeTagInt);
                WriteInt(ms, intVal);
                break;
            case double doubleVal:
                WriteInt(ms, TypeTagDouble);
                WriteDouble(ms, doubleVal);
                break;
            case string strVal:
                WriteInt(ms, TypeTagString);
                WriteString(ms, strVal);
                break;
            case bool boolVal:
                WriteInt(ms, TypeTagBool);
                WriteByte(ms, (byte)(boolVal ? 1 : 0));
                break;
            case double[] doubleArr:
                WriteInt(ms, TypeTagDoubleArray);
                WriteInt(ms, doubleArr.Length);
                foreach (var v in doubleArr)
                {
                    WriteDouble(ms, v);
                }
                break;
            case int[] intArr:
                WriteInt(ms, TypeTagIntArray);
                WriteInt(ms, intArr.Length);
                foreach (var v in intArr)
                {
                    WriteInt(ms, v);
                }
                break;
            default:
                WriteInt(ms, TypeTagObject);
                var json = JsonSerializer.Serialize(value);
                WriteString(ms, json);
                break;
        }
    }

    private static object ReadTypedValue(byte[] data, ref int offset)
    {
        var typeTag = ReadInt(data, ref offset);
        return typeTag switch
        {
            TypeTagNull => string.Empty,
            TypeTagInt => ReadInt(data, ref offset),
            TypeTagDouble => ReadDouble(data, ref offset),
            TypeTagString => ReadString(data, ref offset),
            TypeTagBool => ReadByte(data, ref offset) == 1,
            TypeTagDoubleArray => ReadDoubleArray(data, ref offset),
            TypeTagIntArray => ReadIntArray(data, ref offset),
            TypeTagObject => ReadString(data, ref offset),
            _ => throw new FormatException($"Unknown type tag: {typeTag}")
        };
    }

    private static double[] ReadDoubleArray(byte[] data, ref int offset)
    {
        var len = ReadInt(data, ref offset);
        var result = new double[len];
        for (var i = 0; i < len; i++)
        {
            result[i] = ReadDouble(data, ref offset);
        }
        return result;
    }

    private static int[] ReadIntArray(byte[] data, ref int offset)
    {
        var len = ReadInt(data, ref offset);
        var result = new int[len];
        for (var i = 0; i < len; i++)
        {
            result[i] = ReadInt(data, ref offset);
        }
        return result;
    }

    private static void AppendJsonValue(StringBuilder sb, object? value)
    {
        switch (value)
        {
            case null:
                sb.Append("null");
                break;
            case int intVal:
                sb.Append(intVal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            case double doubleVal:
                sb.Append(doubleVal.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case string strVal:
                sb.AppendFormat("\"{0}\"", EscapeJson(strVal));
                break;
            case bool boolVal:
                sb.Append(boolVal ? "true" : "false");
                break;
            default:
                sb.AppendFormat("\"{0}\"", EscapeJson(value.ToString() ?? string.Empty));
                break;
        }
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

    private static void WriteInt(System.IO.MemoryStream ms, int value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buf, value);
        ms.Write(buf);
    }

    private static void WriteDouble(System.IO.MemoryStream ms, double value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buf, value);
        ms.Write(buf);
    }

    private static void WriteString(System.IO.MemoryStream ms, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteInt(ms, bytes.Length);
        ms.Write(bytes);
    }

    private static void WriteByte(System.IO.MemoryStream ms, byte value)
    {
        ms.WriteByte(value);
    }

    private static int ReadInt(byte[] data, ref int offset)
    {
        var val = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += 4;
        return val;
    }

    private static double ReadDouble(byte[] data, ref int offset)
    {
        var val = BinaryPrimitives.ReadDoubleLittleEndian(data.AsSpan(offset));
        offset += 8;
        return val;
    }

    private static string ReadString(byte[] data, ref int offset)
    {
        var len = ReadInt(data, ref offset);
        var result = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return result;
    }

    private static byte ReadByte(byte[] data, ref int offset)
    {
        return data[offset++];
    }
}
