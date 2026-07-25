namespace MathVerse.Math.Interop.SimulationExchange;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Manages restart file creation and loading for simulation resumption.
/// </summary>
public sealed class RestartFile
{
    private const int Magic = 0x52535441;
    private const int FormatVersion = 1;

    /// <summary>
    /// Creates restart data from the full simulation state and checkpoint step.
    /// </summary>
    /// <param name="fullState">The complete simulation state.</param>
    /// <param name="checkpointStep">The checkpoint step number to associate with.</param>
    /// <returns>A byte array containing the restart data.</returns>
    public byte[] CreateRestartData(Dictionary<string, object> fullState, int checkpointStep)
    {
        ArgumentNullException.ThrowIfNull(fullState);

        using var ms = new System.IO.MemoryStream();
        var now = DateTimeOffset.UtcNow;

        WriteInt(ms, Magic);
        WriteInt(ms, FormatVersion);
        WriteInt(ms, checkpointStep);
        WriteLong(ms, now.Ticks);

        var stateSerializer = new StateSerializer();
        var stateBytes = stateSerializer.SerializeState(fullState);
        WriteInt(ms, stateBytes.Length);
        ms.Write(stateBytes);

        return ms.ToArray();
    }

    /// <summary>
    /// Loads the full simulation state from restart data.
    /// </summary>
    /// <param name="data">The byte array containing restart data.</param>
    /// <returns>The restored simulation state dictionary.</returns>
    public Dictionary<string, object> LoadRestartData(byte[] data)
    {
        var offset = ValidateAndSkipHeader(data);

        var stateLen = ReadInt(data, ref offset);
        var stateBytes = new byte[stateLen];
        Buffer.BlockCopy(data, offset, stateBytes, 0, stateLen);

        var stateSerializer = new StateSerializer();
        return stateSerializer.DeserializeState(stateBytes);
    }

    /// <summary>
    /// Gets the checkpoint step number from restart data without fully deserializing.
    /// </summary>
    /// <param name="data">The byte array containing restart data.</param>
    /// <returns>The checkpoint step number.</returns>
    public int GetCheckpointStep(byte[] data)
    {
        var offset = ValidateAndSkipHeader(data);
        return ReadInt(data, ref offset);
    }

    private static int ValidateAndSkipHeader(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = ReadInt(data, ref offset);
        if (magic != Magic)
        {
            throw new FormatException("Invalid restart file magic number.");
        }
        var version = ReadInt(data, ref offset);
        if (version > FormatVersion)
        {
            throw new FormatException($"Unsupported restart file version {version}.");
        }
        return offset;
    }

    private static void WriteInt(System.IO.MemoryStream ms, int value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buf, value);
        ms.Write(buf);
    }

    private static void WriteLong(System.IO.MemoryStream ms, long value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buf, value);
        ms.Write(buf);
    }

    private static int ReadInt(byte[] data, ref int offset)
    {
        var val = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += 4;
        return val;
    }
}
