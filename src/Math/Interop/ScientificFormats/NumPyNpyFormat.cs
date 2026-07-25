namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Reads NumPy .npy format files.
/// </summary>
public sealed class NumPyNpyReader
{
    private const ushort NpyMagic = 0x4E4E;

    /// <summary>
    /// Reads a NumPy .npy file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the .npy file data.</param>
    /// <returns>The array data read from the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
    public Array Read(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        var magic = reader.ReadUInt16();
        if (magic != NpyMagic)
        {
            throw new InvalidDataException("Invalid NumPy .npy file header.");
        }

        var majorVersion = reader.ReadByte();
        var minorVersion = reader.ReadByte();

        uint headerLen;
        if (majorVersion >= 2)
        {
            headerLen = reader.ReadUInt32();
        }
        else
        {
            headerLen = reader.ReadUInt16();
        }

        var headerBytes = reader.ReadBytes((int)headerLen);
        var header = Encoding.ASCII.GetString(headerBytes).TrimEnd('\0');

        var descr = ParseDescription(header);
        var shape = ParseShape(header);
        var fortranOrder = ParseFortranOrder(header);

        long elementSize = descr.Size;
        long totalElements = 1;
        foreach (var dim in shape)
        {
            totalElements *= dim;
        }

        var dataBytes = reader.ReadBytes((int)(totalElements * elementSize));

        return DecodeData(dataBytes, descr, shape);
    }

    /// <summary>
    /// Reads a NumPy .npy file from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the .npy file data.</param>
    /// <returns>The array data read from the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public Array Read(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using var stream = new MemoryStream(data);
        return Read(stream);
    }

    private static DtypeInfo ParseDescription(string header)
    {
        int descrStart = header.IndexOf("descr", StringComparison.Ordinal);
        if (descrStart < 0)
        {
            return new DtypeInfo("<f8", typeof(double), 8);
        }

        int colonIdx = header.IndexOf(':', descrStart);
        int lbracket = header.IndexOf('[', colonIdx);
        int rbracket = header.IndexOf(']', lbracket);
        var descrStr = header.Substring(lbracket + 1, rbracket - lbracket - 1).Trim();

        if (descrStr.StartsWith("'</", StringComparison.Ordinal) || descrStr.StartsWith("\"</", StringComparison.Ordinal))
        {
            descrStr = descrStr.Substring(3, descrStr.Length - 6);
        }

        return ParseDtypeString(descrStr);
    }

    private static DtypeInfo ParseDtypeString(string dtypeStr)
    {
        if (string.IsNullOrEmpty(dtypeStr))
        {
            return new DtypeInfo("<f8", typeof(double), 8);
        }

        string actualDtype = dtypeStr;

        if (dtypeStr.StartsWith("<", StringComparison.Ordinal))
        {
            actualDtype = dtypeStr.Substring(1);
        }
        else if (dtypeStr.StartsWith(">", StringComparison.Ordinal) || dtypeStr.StartsWith("=", StringComparison.Ordinal))
        {
            actualDtype = dtypeStr.Substring(1);
        }

        return actualDtype switch
        {
            "f8" or "float64" => new DtypeInfo(dtypeStr, typeof(double), 8),
            "f4" or "float32" => new DtypeInfo(dtypeStr, typeof(float), 4),
            "f2" or "float16" => new DtypeInfo(dtypeStr, typeof(float), 4),
            "i4" or "int32" => new DtypeInfo(dtypeStr, typeof(int), 4),
            "i2" or "int16" => new DtypeInfo(dtypeStr, typeof(short), 2),
            "i8" or "int64" => new DtypeInfo(dtypeStr, typeof(long), 8),
            "i1" or "int8" => new DtypeInfo(dtypeStr, typeof(sbyte), 1),
            "u4" or "uint32" => new DtypeInfo(dtypeStr, typeof(uint), 4),
            "u2" or "uint16" => new DtypeInfo(dtypeStr, typeof(ushort), 2),
            "u8" or "uint64" => new DtypeInfo(dtypeStr, typeof(ulong), 8),
            "u1" or "uint8" => new DtypeInfo(dtypeStr, typeof(byte), 1),
            "bool" or "b1" => new DtypeInfo(dtypeStr, typeof(bool), 1),
            "c16" or "complex128" => new DtypeInfo(dtypeStr, typeof(double), 16),
            "c8" or "complex64" => new DtypeInfo(dtypeStr, typeof(float), 8),
            _ => new DtypeInfo(dtypeStr, typeof(double), 8)
        };
    }

    private static int[] ParseShape(string header)
    {
        int shapeStart = header.IndexOf("shape", StringComparison.Ordinal);
        if (shapeStart < 0) return Array.Empty<int>();

        int lparen = header.IndexOf('(', shapeStart);
        int rparen = header.IndexOf(')', lparen);
        var shapeStr = header.Substring(lparen + 1, rparen - lparen - 1).Trim();

        if (string.IsNullOrEmpty(shapeStr))
        {
            return Array.Empty<int>();
        }

        var parts = shapeStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var shape = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            int.TryParse(parts[i].Trim(), out shape[i]);
        }
        return shape;
    }

    private static bool ParseFortranOrder(string header)
    {
        int orderStart = header.IndexOf("fortran_order", StringComparison.Ordinal);
        if (orderStart < 0) return false;

        int colonIdx = header.IndexOf(':', orderStart);
        var valueStr = header.Substring(colonIdx + 1).Trim().TrimEnd(',');
        return valueStr.StartsWith("True", StringComparison.OrdinalIgnoreCase);
    }

    private static Array DecodeData(byte[] data, DtypeInfo descr, int[] shape)
    {
        if (shape.Length == 0)
        {
            shape = new[] { data.Length / descr.Size };
        }

        if (descr.ElementType == typeof(double))
        {
            var result = new double[data.Length / 8];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(float))
        {
            var result = new float[data.Length / 4];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(int))
        {
            var result = new int[data.Length / 4];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(short))
        {
            var result = new short[data.Length / 2];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(long))
        {
            var result = new long[data.Length / 8];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(byte))
        {
            var result = new byte[data.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(ushort))
        {
            var result = new ushort[data.Length / 2];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(uint))
        {
            var result = new uint[data.Length / 4];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        if (descr.ElementType == typeof(ulong))
        {
            var result = new ulong[data.Length / 8];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return Reshape(result, shape);
        }

        var fallback = new double[data.Length / 8];
        Buffer.BlockCopy(data, 0, fallback, 0, System.Math.Min(data.Length, fallback.Length * 8));
        return Reshape(fallback, shape);
    }

    private static Array Reshape<T>(T[] flat, int[] shape)
    {
        if (shape.Length <= 1)
        {
            return flat;
        }

        var result = Array.CreateInstance(typeof(T), shape);
        var strides = new int[shape.Length];
        strides[shape.Length - 1] = 1;
        for (int i = shape.Length - 2; i >= 0; i--)
        {
            strides[i] = strides[i + 1] * shape[i + 1];
        }

        for (int idx = 0; idx < flat.Length; idx++)
        {
            var indices = new int[shape.Length];
            int remaining = idx;
            for (int d = 0; d < shape.Length; d++)
            {
                indices[d] = remaining / strides[d];
                remaining %= strides[d];
            }
            result.SetValue(flat[idx], indices);
        }

        return result;
    }

    private sealed record DtypeInfo(string DTypeString, Type ElementType, int Size);
}

/// <summary>
/// Writes NumPy .npy format files.
/// </summary>
public sealed class NumPyNpyWriter
{
    private const ushort NpyMagic = 0x4E4E;

    /// <summary>
    /// Writes an array to a NumPy .npy file byte array.
    /// </summary>
    /// <param name="data">The array data to write.</param>
    /// <param name="dtype">The dtype string (e.g., "float64", "int32"). Defaults to "float64".</param>
    /// <returns>A byte array containing the .npy file data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public byte[] Write(Array data, string dtype = "float64")
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(dtype))
        {
            dtype = "float64";
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        writer.Write(NpyMagic);
        writer.Write((byte)1);
        writer.Write((byte)0);

        var shape = new int[data.Rank];
        for (int i = 0; i < data.Rank; i++)
        {
            shape[i] = data.GetLength(i);
        }

        var shapeStr = "(" + string.Join(", ", shape) + (shape.Length == 1 ? "," : "") + ")";
        var header = $"{{'descr': '<{dtype}', 'fortran_order': False, 'shape': {shapeStr}, }}";

        var headerLen = (uint)(header.Length + 1);
        while ((headerLen + 4) % 64 != 0)
        {
            headerLen++;
        }

        writer.Write(headerLen);
        writer.Write(Encoding.ASCII.GetBytes(header));
        writer.Write((byte)'\n');
        var padding = (int)(headerLen - header.Length - 1);
        if (padding > 0)
        {
            writer.Write(new byte[padding]);
        }

        var dataBytes = EncodeArray(data, dtype);
        writer.Write(dataBytes);

        return stream.ToArray();
    }

    private static byte[] EncodeArray(Array data, string dtype)
    {
        int elementSize = GetElementSize(dtype);
        var bytes = new byte[data.Length * elementSize];

        int offset = 0;
        foreach (var item in data)
        {
            switch (item)
            {
                case double d:
                    Buffer.BlockCopy(BitConverter.GetBytes(d), 0, bytes, offset, 8);
                    offset += 8;
                    break;
                case float f:
                    Buffer.BlockCopy(BitConverter.GetBytes(f), 0, bytes, offset, 4);
                    offset += 4;
                    break;
                case int intVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(intVal), 0, bytes, offset, 4);
                    offset += 4;
                    break;
                case short shortVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(shortVal), 0, bytes, offset, 2);
                    offset += 2;
                    break;
                case long longVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(longVal), 0, bytes, offset, 8);
                    offset += 8;
                    break;
                case byte byteVal:
                    bytes[offset] = byteVal;
                    offset += 1;
                    break;
                case sbyte sbyteVal:
                    bytes[offset] = (byte)sbyteVal;
                    offset += 1;
                    break;
                case ushort ushortVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(ushortVal), 0, bytes, offset, 2);
                    offset += 2;
                    break;
                case uint uintVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(uintVal), 0, bytes, offset, 4);
                    offset += 4;
                    break;
                case ulong ulongVal:
                    Buffer.BlockCopy(BitConverter.GetBytes(ulongVal), 0, bytes, offset, 8);
                    offset += 8;
                    break;
                default:
                    var asDouble = Convert.ToDouble(item);
                    Buffer.BlockCopy(BitConverter.GetBytes(asDouble), 0, bytes, offset, 8);
                    offset += 8;
                    break;
            }
        }

        return bytes;
    }

    private static int GetElementSize(string dtype)
    {
        return dtype switch
        {
            "float64" or "f8" => 8,
            "float32" or "f4" => 4,
            "float16" or "f2" => 2,
            "int64" or "i8" => 8,
            "int32" or "i4" => 4,
            "int16" or "i2" => 2,
            "int8" or "i1" => 1,
            "uint64" or "u8" => 8,
            "uint32" or "u4" => 4,
            "uint16" or "u2" => 2,
            "uint8" or "u1" => 1,
            "bool" or "b1" => 1,
            _ => 8
        };
    }
}
