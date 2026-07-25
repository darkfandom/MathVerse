namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Reads MATLAB .mat files (v5 struct-of-arrays simplified format).
/// Provides in-memory dictionary-based access for AOT safety.
/// </summary>
public sealed class MATLABMatReader
{
    private const uint LittleEndianMagic = 0x00004D49;
    private const uint BigEndianMagic = 0x4D490000;

    /// <summary>
    /// Reads a MATLAB .mat file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the .mat file data.</param>
    /// <returns>A dictionary mapping variable names to their array data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
    public Dictionary<string, Array> Read(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        var magic = reader.ReadUInt32();
        bool littleEndian;
        if (magic == LittleEndianMagic)
        {
            littleEndian = true;
        }
        else if (magic == BigEndianMagic)
        {
            littleEndian = false;
        }
        else
        {
            throw new InvalidDataException("Invalid MATLAB .mat file header.");
        }

        var description = Encoding.ASCII.GetString(reader.ReadBytes(116)).TrimEnd('\0');
        var version = reader.ReadUInt16();
        var endianIndicator = Encoding.ASCII.GetString(reader.ReadBytes(2));

        var result = new Dictionary<string, Array>(StringComparer.OrdinalIgnoreCase);

        while (stream.Position < stream.Length - 1)
        {
            try
            {
                var type = ReadUInt32(reader, littleEndian);
                var size = (int)ReadUInt32(reader, littleEndian);

                if (size < 0 || size > stream.Length - stream.Position)
                {
                    break;
                }

                var nameLen = ReadUInt32(reader, littleEndian);
                var paddedNameLen = (int)((nameLen + 7) & ~7u);
                var nameBytes = reader.ReadBytes(paddedNameLen);
                var name = Encoding.ASCII.GetString(nameBytes, 0, (int)nameLen - 1).TrimEnd('\0');

                var dataBytes = reader.ReadBytes(size - paddedNameLen - 8);

                var array = DecodeArray(type, dataBytes, littleEndian);
                if (array is not null)
                {
                    result[name] = array;
                }
            }
            catch (EndOfStreamException)
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Reads a MATLAB .mat file from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the .mat file data.</param>
    /// <returns>A dictionary mapping variable names to their array data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public Dictionary<string, Array> Read(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using var stream = new MemoryStream(data);
        return Read(stream);
    }

    private static uint ReadUInt32(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(4);
        return littleEndian
            ? BitConverter.ToUInt32(bytes, 0)
            : BitConverter.ToUInt32(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }, 0);
    }

    private static double ReadDouble(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(8);
        if (!littleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToDouble(bytes, 0);
    }

    private static float ReadSingle(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(4);
        if (!littleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToSingle(bytes, 0);
    }

    private static Array? DecodeArray(uint type, byte[] data, bool littleEndian)
    {
        if (data.Length == 0) return null;

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        switch (type)
        {
            case 6:
                var miDouble = ReadDouble(reader, littleEndian);
                return new[] { miDouble };

            case 7:
                var miSingle = ReadSingle(reader, littleEndian);
                return new[] { miSingle };

            case 12:
                var miInt32 = ReadInt32(reader, littleEndian);
                return new[] { miInt32 };

            case 9:
                var miInt16 = (short)ReadUInt16(reader, littleEndian);
                return new[] { miInt16 };

            case 13:
                var miUInt16 = ReadUInt16(reader, littleEndian);
                return new[] { miUInt16 };

            case 14:
                var miInt64 = ReadInt64(reader, littleEndian);
                return new[] { miInt64 };

            case 15:
                var miUInt32 = ReadUInt32(reader, littleEndian);
                return new[] { miUInt32 };

            default:
                return null;
        }
    }

    private static int ReadInt32(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(4);
        return littleEndian
            ? BitConverter.ToInt32(bytes, 0)
            : BitConverter.ToInt32(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }, 0);
    }

    private static short ReadInt16(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(2);
        return littleEndian
            ? BitConverter.ToInt16(bytes, 0)
            : BitConverter.ToInt16(new[] { bytes[1], bytes[0] }, 0);
    }

    private static ushort ReadUInt16(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(2);
        return littleEndian
            ? BitConverter.ToUInt16(bytes, 0)
            : BitConverter.ToUInt16(new[] { bytes[1], bytes[0] }, 0);
    }

    private static long ReadInt64(BinaryReader reader, bool littleEndian)
    {
        var bytes = reader.ReadBytes(8);
        return littleEndian
            ? BitConverter.ToInt64(bytes, 0)
            : BitConverter.ToInt64(new[] { bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0] }, 0);
    }
}

/// <summary>
/// Writes MATLAB .mat files (v5 simplified format).
/// </summary>
public sealed class MATLABMatWriter
{
    /// <summary>
    /// Writes a dictionary of variables to a MATLAB .mat file byte array.
    /// </summary>
    /// <param name="variables">The variables to write, mapping names to array data.</param>
    /// <returns>A byte array containing the serialized .mat file data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variables"/> is null.</exception>
    public byte[] Write(Dictionary<string, Array> variables)
    {
        if (variables is null)
        {
            throw new ArgumentNullException(nameof(variables));
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        writer.Write(LittleEndianMagic);

        var description = "MathVerse MATLAB Writer";
        var descBytes = Encoding.ASCII.GetBytes(description);
        writer.Write(descBytes);
        if (descBytes.Length < 116)
        {
            writer.Write(new byte[116 - descBytes.Length]);
        }

        writer.Write((ushort)0x0100);
        writer.Write(Encoding.ASCII.GetBytes("IM"));

        foreach (var kvp in variables)
        {
            var type = GetArrayType(kvp.Value);
            var encodedData = EncodeArray(type, kvp.Value);

            writer.Write(type);
            writer.Write((uint)(encodedData.Length + 8));
            writer.Write((uint)(kvp.Key.Length + 1));
            writer.Write(Encoding.ASCII.GetBytes(kvp.Key + '\0'));

            var padding = (int)((kvp.Key.Length + 1 + 7) & ~7u) - (kvp.Key.Length + 1);
            if (padding > 0)
            {
                writer.Write(new byte[padding]);
            }

            writer.Write(encodedData);
        }

        return stream.ToArray();
    }

    private static uint GetArrayType(Array data)
    {
        var elementType = data.GetType().GetElementType();
        if (elementType == typeof(double)) return 6;
        if (elementType == typeof(float)) return 7;
        if (elementType == typeof(int)) return 12;
        if (elementType == typeof(short)) return 9;
        if (elementType == typeof(ushort)) return 13;
        if (elementType == typeof(long)) return 14;
        if (elementType == typeof(uint)) return 15;
        return 6;
    }

    private static byte[] EncodeArray(uint type, Array data)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        var values = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            if (data.GetValue(i) is double d) values[i] = d;
            else if (data.GetValue(i) is float f) values[i] = f;
            else if (data.GetValue(i) is int intVal) values[i] = intVal;
            else if (data.GetValue(i) is short shortVal) values[i] = shortVal;
            else if (data.GetValue(i) is long longVal) values[i] = longVal;
        }

        foreach (var v in values)
        {
            writer.Write(v);
        }

        return stream.ToArray();
    }

    private const uint LittleEndianMagic = 0x00004D49;
}
