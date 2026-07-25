namespace MathVerse.Math.Interop.NumericalExchange;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

/// <summary>
/// Represents a sparse matrix entry with row, column, and value.
/// </summary>
public sealed class SparseEntry
{
    /// <summary>
    /// Gets or sets the row index.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column index.
    /// </summary>
    public int Col { get; set; }

    /// <summary>
    /// Gets or sets the entry value.
    /// </summary>
    public double Value { get; set; }
}

/// <summary>
/// Represents a sparse matrix stored as a list of non-zero entries.
/// </summary>
public sealed class SparseMatrix
{
    /// <summary>
    /// Gets or sets the number of rows.
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Gets or sets the number of columns.
    /// </summary>
    public int Cols { get; set; }

    /// <summary>
    /// Gets the list of non-zero entries.
    /// </summary>
    public List<SparseEntry> Entries { get; } = new();
}

/// <summary>
/// Serializes and deserializes dense and sparse matrices to and from binary format.
/// </summary>
public sealed class MatrixSerializer
{
    private const int DenseMagic = 0x4D415458;
    private const int SparseMagic = 0x53505253;

    /// <summary>
    /// Serializes a dense 2D double matrix to a binary byte array.
    /// </summary>
    /// <param name="matrix">The dense matrix to serialize.</param>
    /// <returns>A byte array containing the serialized matrix.</returns>
    public byte[] SerializeDense(double[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var totalSize = sizeof(int) * 3 + sizeof(double) * rows * cols;
        var buffer = new byte[totalSize];
        var offset = 0;

        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), DenseMagic);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), rows);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), cols);
        offset += sizeof(int);

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(offset), matrix[r, c]);
                offset += sizeof(double);
            }
        }

        return buffer;
    }

    /// <summary>
    /// Deserializes a dense 2D double matrix from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized matrix.</param>
    /// <returns>The deserialized 2D matrix.</returns>
    public double[,] DeserializeDense(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        if (magic != DenseMagic)
        {
            throw new FormatException("Invalid dense matrix magic number.");
        }
        offset += sizeof(int);

        var rows = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var cols = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);

        var matrix = new double[rows, cols];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                matrix[r, c] = BinaryPrimitives.ReadDoubleLittleEndian(data.AsSpan(offset));
                offset += sizeof(double);
            }
        }

        return matrix;
    }

    /// <summary>
    /// Serializes a sparse matrix to a binary byte array.
    /// </summary>
    /// <param name="sparse">The sparse matrix to serialize.</param>
    /// <returns>A byte array containing the serialized sparse matrix.</returns>
    public byte[] SerializeSparse(SparseMatrix sparse)
    {
        ArgumentNullException.ThrowIfNull(sparse);

        var entryCount = sparse.Entries.Count;
        var totalSize = sizeof(int) * 4 + entryCount * (sizeof(int) * 2 + sizeof(double));
        var buffer = new byte[totalSize];
        var offset = 0;

        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), SparseMagic);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), sparse.Rows);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), sparse.Cols);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), entryCount);
        offset += sizeof(int);

        foreach (var entry in sparse.Entries)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), entry.Row);
            offset += sizeof(int);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), entry.Col);
            offset += sizeof(int);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(offset), entry.Value);
            offset += sizeof(double);
        }

        return buffer;
    }

    /// <summary>
    /// Deserializes a sparse matrix from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized sparse matrix.</param>
    /// <returns>The deserialized sparse matrix.</returns>
    public SparseMatrix DeserializeSparse(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var offset = 0;
        var magic = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        if (magic != SparseMagic)
        {
            throw new FormatException("Invalid sparse matrix magic number.");
        }
        offset += sizeof(int);

        var rows = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var cols = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);
        var entryCount = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
        offset += sizeof(int);

        var sparse = new SparseMatrix { Rows = rows, Cols = cols };
        for (var i = 0; i < entryCount; i++)
        {
            var row = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
            offset += sizeof(int);
            var col = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset));
            offset += sizeof(int);
            var val = BinaryPrimitives.ReadDoubleLittleEndian(data.AsSpan(offset));
            offset += sizeof(double);

            sparse.Entries.Add(new SparseEntry { Row = row, Col = col, Value = val });
        }

        return sparse;
    }
}
