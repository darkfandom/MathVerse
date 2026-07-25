namespace MathVerse.Math.DataScience.Import;

using System;
using System.Collections.Generic;
using System.Text;

using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Reads binary-format data into a dataset using a length-prefixed format.
/// </summary>
public sealed class BinaryReader
{
    /// <summary>
    /// Reads binary data and returns a dataset.
    /// </summary>
    /// <param name="data">The binary data to read.</param>
    /// <returns>A dataset containing the parsed data.</returns>
    public Dataset Read(byte[] data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));
        if (data.Length < 4)
        {
            throw new FormatException("Binary data is too short to contain valid header.");
        }

        int offset = 0;

        int columnCount = ReadInt32(data, ref offset);
        var columnNames = new string[columnCount];
        var columnTypes = new ColumnType[columnCount];

        for (int i = 0; i < columnCount; i++)
        {
            columnNames[i] = ReadString(data, ref offset);
            columnTypes[i] = (ColumnType)ReadInt32(data, ref offset);
        }

        int rowCount = ReadInt32(data, ref offset);
        var schema = new Schema();
        var rows = new List<Dictionary<string, object?>>();

        for (int i = 0; i < columnCount; i++)
        {
            schema.AddColumn(columnNames[i], columnTypes[i]);
        }

        for (int r = 0; r < rowCount; r++)
        {
            var row = new Dictionary<string, object?>();
            for (int c = 0; c < columnCount; c++)
            {
                row[columnNames[c]] = ReadValue(data, ref offset, columnTypes[c]);
            }
            rows.Add(row);
        }

        var ds = new Dataset
        {
            Name = "imported_binary",
            Metadata = new DatasetMetadata
            {
                Name = "imported_binary",
                RowCount = rowCount,
                ColumnCount = columnCount,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            },
            Schema = schema
        };
        ds.Rows.AddRange(rows);
        return ds;
    }

    private static int ReadInt32(byte[] data, ref int offset)
    {
        if (offset + 4 > data.Length)
            throw new FormatException("Unexpected end of binary data.");

        int value = BitConverter.ToInt32(data, offset);
        offset += 4;
        return value;
    }

    private static double ReadDouble(byte[] data, ref int offset)
    {
        if (offset + 8 > data.Length)
            throw new FormatException("Unexpected end of binary data.");

        double value = BitConverter.ToDouble(data, offset);
        offset += 8;
        return value;
    }

    private static bool ReadBoolean(byte[] data, ref int offset)
    {
        if (offset + 1 > data.Length)
            throw new FormatException("Unexpected end of binary data.");

        bool value = data[offset] != 0;
        offset += 1;
        return value;
    }

    private static string ReadString(byte[] data, ref int offset)
    {
        int length = ReadInt32(data, ref offset);
        if (offset + length > data.Length)
            throw new FormatException("Unexpected end of binary data.");

        string value = Encoding.UTF8.GetString(data, offset, length);
        offset += length;
        return value;
    }

    private static object? ReadValue(byte[] data, ref int offset, ColumnType type)
    {
        bool isNull = ReadBoolean(data, ref offset);
        if (isNull) return null;

        return type switch
        {
            ColumnType.Double => ReadDouble(data, ref offset),
            ColumnType.Int => ReadInt32(data, ref offset),
            ColumnType.Bool => ReadBoolean(data, ref offset),
            ColumnType.String => ReadString(data, ref offset),
            _ => ReadString(data, ref offset)
        };
    }
}