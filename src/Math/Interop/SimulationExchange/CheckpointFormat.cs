namespace MathVerse.Math.Interop.SimulationExchange;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents checkpoint data extracted from a checkpoint file.
/// </summary>
public sealed class CheckpointData
{
    /// <summary>
    /// Gets or sets the simulation step number.
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// Gets or sets the simulation time.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Gets the variables dictionary containing named arrays.
    /// </summary>
    public Dictionary<string, Array> Variables { get; } = new();

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset Created { get; set; }
}

/// <summary>
/// Creates and reads checkpoint files using a simple binary format.
/// </summary>
public sealed class CheckpointFormat
{
    private const int Magic = 0x43484B50;
    private const int FormatVersion = 1;

    /// <summary>
    /// Creates a checkpoint from the given simulation state.
    /// </summary>
    /// <param name="step">The current simulation step.</param>
    /// <param name="time">The current simulation time.</param>
    /// <param name="variables">The variables to store in the checkpoint.</param>
    /// <returns>A byte array containing the serialized checkpoint.</returns>
    public byte[] CreateCheckpoint(int step, double time, Dictionary<string, Array> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        using var ms = new System.IO.MemoryStream();
        var now = DateTimeOffset.UtcNow;

        WriteInt(ms, Magic);
        WriteInt(ms, FormatVersion);
        WriteInt(ms, step);
        WriteDouble(ms, time);
        WriteLong(ms, now.Ticks);
        WriteInt(ms, now.Offset.Ticks > 0 ? (int)(now.Offset.TotalMinutes) : (int)(now.Offset.TotalMinutes));

        WriteInt(ms, variables.Count);
        foreach (var kvp in variables)
        {
            WriteString(ms, kvp.Key);
            SerializeArray(ms, kvp.Value);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Reads a checkpoint from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the checkpoint data.</param>
    /// <returns>The deserialized checkpoint data.</returns>
    public CheckpointData ReadCheckpoint(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = ReadInt(data, ref offset);
        if (magic != Magic)
        {
            throw new FormatException("Invalid checkpoint magic number.");
        }

        var version = ReadInt(data, ref offset);
        if (version > FormatVersion)
        {
            throw new FormatException($"Unsupported checkpoint version {version}.");
        }

        var step = ReadInt(data, ref offset);
        var time = ReadDouble(data, ref offset);
        var ticks = ReadLong(data, ref offset);
        var offsetMinutes = ReadInt(data, ref offset);
        var timestamp = new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes));

        var varCount = ReadInt(data, ref offset);
        var checkpoint = new CheckpointData
        {
            Step = step,
            Time = time,
            Created = timestamp
        };

        for (var i = 0; i < varCount; i++)
        {
            var name = ReadString(data, ref offset);
            var arr = DeserializeArray(data, ref offset);
            checkpoint.Variables[name] = arr;
        }

        return checkpoint;
    }

    private static void SerializeArray(System.IO.MemoryStream ms, Array arr)
    {
        var rank = arr.Rank;
        var lengths = new int[rank];
        for (var d = 0; d < rank; d++)
        {
            lengths[d] = arr.GetLength(d);
        }

        WriteInt(ms, rank);
        foreach (var len in lengths)
        {
            WriteInt(ms, len);
        }

        var totalElements = 1;
        for (var d = 0; d < rank; d++)
        {
            totalElements *= lengths[d];
        }

        if (arr is double[] doubles)
        {
            WriteInt(ms, 0);
            foreach (var v in doubles)
            {
                WriteDouble(ms, v);
            }
        }
        else if (arr is int[] ints)
        {
            WriteInt(ms, 1);
            foreach (var v in ints)
            {
                WriteInt(ms, v);
            }
        }
        else if (arr is float[] floats)
        {
            WriteInt(ms, 2);
            foreach (var v in floats)
            {
                WriteFloat(ms, v);
            }
        }
        else
        {
            WriteInt(ms, -1);
        }
    }

    private static Array DeserializeArray(byte[] data, ref int offset)
    {
        var rank = ReadInt(data, ref offset);
        var lengths = new int[rank];
        for (var d = 0; d < rank; d++)
        {
            lengths[d] = ReadInt(data, ref offset);
        }

        var typeTag = ReadInt(data, ref offset);

        return typeTag switch
        {
            0 => ReadDoubleArray(data, ref offset, lengths),
            1 => ReadIntArray(data, ref offset, lengths),
            2 => ReadFloatArray(data, ref offset, lengths),
            _ => throw new FormatException($"Unknown array type tag: {typeTag}")
        };
    }

    private static double[] ReadDoubleArray(byte[] data, ref int offset, int[] lengths)
    {
        var total = 1;
        foreach (var l in lengths) total *= l;
        var result = new double[total];
        for (var i = 0; i < total; i++)
        {
            result[i] = ReadDouble(data, ref offset);
        }
        return result;
    }

    private static int[] ReadIntArray(byte[] data, ref int offset, int[] lengths)
    {
        var total = 1;
        foreach (var l in lengths) total *= l;
        var result = new int[total];
        for (var i = 0; i < total; i++)
        {
            result[i] = ReadInt(data, ref offset);
        }
        return result;
    }

    private static float[] ReadFloatArray(byte[] data, ref int offset, int[] lengths)
    {
        var total = 1;
        foreach (var l in lengths) total *= l;
        var result = new float[total];
        for (var i = 0; i < total; i++)
        {
            result[i] = ReadFloat(data, ref offset);
        }
        return result;
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

    private static void WriteFloat(System.IO.MemoryStream ms, float value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buf, value);
        ms.Write(buf);
    }

    private static void WriteLong(System.IO.MemoryStream ms, long value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buf, value);
        ms.Write(buf);
    }

    private static void WriteString(System.IO.MemoryStream ms, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteInt(ms, bytes.Length);
        ms.Write(bytes);
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

    private static float ReadFloat(byte[] data, ref int offset)
    {
        var val = BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(offset));
        offset += 4;
        return val;
    }

    private static long ReadLong(byte[] data, ref int offset)
    {
        var val = BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(offset));
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
}
