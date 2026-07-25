namespace MathVerse.Math.Interop.NumericalExchange;

using System;
using System.Buffers.Binary;
using System.Text;

/// <summary>
/// Represents an N-dimensional numerical tensor.
/// </summary>
public sealed class Tensor
{
    /// <summary>
    /// Gets the shape of each dimension.
    /// </summary>
    public int[] Shape { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Gets the flat data array in row-major order.
    /// </summary>
    public double[] Data { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the data type identifier string.
    /// </summary>
    public string DataType { get; set; } = "f64";
}

/// <summary>
/// Serializes and deserializes N-dimensional tensors to and from binary format.
/// </summary>
public sealed class TensorSerializer
{
    private const int MagicNumber = 0x54454E53;

    /// <summary>
    /// Serializes a tensor to a binary byte array.
    /// </summary>
    /// <param name="tensor">The tensor to serialize.</param>
    /// <returns>A byte array containing the serialized tensor.</returns>
    public byte[] Serialize(Tensor tensor)
    {
        ArgumentNullException.ThrowIfNull(tensor);

        var dataTypeBytes = Encoding.UTF8.GetBytes(tensor.DataType);
        var totalSize = sizeof(int) + sizeof(int) + dataTypeBytes.Length +
                        sizeof(int) * tensor.Shape.Length +
                        sizeof(double) * tensor.Data.Length;
        var buffer = new byte[totalSize];
        var offset = 0;

        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), MagicNumber);
        offset += sizeof(int);

        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), tensor.Shape.Length);
        offset += sizeof(int);

        WriteByteArray(buffer, ref offset, dataTypeBytes);

        foreach (var dim in tensor.Shape)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), dim);
            offset += sizeof(int);
        }

        foreach (var val in tensor.Data)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(offset), val);
            offset += sizeof(double);
        }

        return buffer;
    }

    /// <summary>
    /// Deserializes a tensor from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized tensor.</param>
    /// <returns>The deserialized tensor.</returns>
    public Tensor Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        if (magic != MagicNumber)
        {
            throw new FormatException("Invalid tensor magic number.");
        }
        offset += sizeof(int);

        var rank = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);

        var dataTypeBytes = ReadByteArray(data, ref offset);
        var dataType = Encoding.UTF8.GetString(dataTypeBytes);

        var shape = new int[rank];
        for (var i = 0; i < rank; i++)
        {
            shape[i] = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
            offset += sizeof(int);
        }

        var totalElements = 1;
        for (var i = 0; i < rank; i++)
        {
            totalElements *= shape[i];
        }

        var dataArr = new double[totalElements];
        for (var i = 0; i < totalElements; i++)
        {
            dataArr[i] = BinaryPrimitives.ReadDoubleLittleEndian(data.AsSpan(offset));
            offset += sizeof(double);
        }

        return new Tensor { Shape = shape, Data = dataArr, DataType = dataType };
    }

    private static void WriteByteArray(byte[] buffer, ref int offset, byte[] bytes)
    {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), bytes.Length);
        offset += sizeof(int);
        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        offset += bytes.Length;
    }

    private static byte[] ReadByteArray(byte[] data, ref int offset)
    {
        var len = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var result = new byte[len];
        Buffer.BlockCopy(data, offset, result, 0, len);
        offset += len;
        return result;
    }
}
