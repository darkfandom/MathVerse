namespace MathVerse.Math.Interop.DataExchange;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Binary dataset format with schema preservation and length-prefixed encoding.
/// </summary>
public sealed class BinaryDatasetFormat
{
    private const int FormatSignature = 0x4D564246;
    private const int FormatVersion = 2;

    /// <summary>
    /// Serializes a dataset to a binary byte array with embedded schema.
    /// </summary>
    /// <param name="ds">The dataset to serialize.</param>
    /// <returns>A byte array containing the binary-encoded dataset.</returns>
    public byte[] Serialize(Dataset ds)
    {
        if (ds is null)
            throw new ArgumentNullException(nameof(ds));

        using var ms = new System.IO.MemoryStream();
        using var bw = new System.IO.BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        bw.Write(FormatSignature);
        bw.Write(FormatVersion);

        bw.Write(ds.Name ?? string.Empty);
        bw.Write(ds.DatasetId.ToString());

        bw.Write(ds.Schema.Columns.Count);
        foreach (var col in ds.Schema.Columns)
        {
            bw.Write(col.Name);
            bw.Write((int)col.Type);
            bw.Write(col.IsNullable);
            bw.Write(col.Description ?? string.Empty);
        }

        bw.Write(ds.Rows.Count);
        foreach (var row in ds.Rows)
        {
            WriteRow(bw, ds.Schema, row);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes a binary byte array into a dataset with restored schema.
    /// </summary>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized dataset with schema.</returns>
    public Dataset Deserialize(byte[] data)
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        using var ms = new System.IO.MemoryStream(data);
        using var br = new System.IO.BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        int signature = br.ReadInt32();
        if (signature != FormatSignature)
            throw new InvalidOperationException("Invalid binary dataset format signature.");

        int version = br.ReadInt32();
        if (version > FormatVersion)
            throw new InvalidOperationException($"Unsupported format version {version}.");

        var ds = new Dataset
        {
            Name = br.ReadString()
        };

        string idStr = br.ReadString();

        int colCount = br.ReadInt32();
        for (int i = 0; i < colCount; i++)
        {
            string colName = br.ReadString();
            var colType = (ColumnType)br.ReadInt32();
            bool nullable = br.ReadBoolean();
            string description = br.ReadString();
            var col = new ColumnDefinition(colName, colType)
            {
                IsNullable = nullable,
                Description = description
            };
            ds.Schema.Columns.Add(col);
        }

        int rowCount = br.ReadInt32();
        for (int i = 0; i < rowCount; i++)
        {
            var row = ReadRow(br, ds.Schema);
            ds.Rows.Add(row);
        }

        return ds;
    }

    private static void WriteRow(System.IO.BinaryWriter bw, Schema schema, Dictionary<string, object?> row)
    {
        bw.Write(schema.Columns.Count);
        foreach (var col in schema.Columns)
        {
            if (!row.TryGetValue(col.Name, out var value))
            {
                bw.Write(false);
                continue;
            }
            bw.Write(true);
            WriteTypedValue(bw, col.Type, value);
        }
    }

    private static Dictionary<string, object?> ReadRow(System.IO.BinaryReader br, Schema schema)
    {
        var row = new Dictionary<string, object?>(schema.Columns.Count, StringComparer.OrdinalIgnoreCase);
        int fieldCount = br.ReadInt32();
        for (int i = 0; i < fieldCount && i < schema.Columns.Count; i++)
        {
            bool hasValue = br.ReadBoolean();
            if (!hasValue)
            {
                row[schema.Columns[i].Name] = null;
                continue;
            }
            row[schema.Columns[i].Name] = ReadTypedValue(br, schema.Columns[i].Type);
        }
        return row;
    }

    private static void WriteTypedValue(System.IO.BinaryWriter bw, ColumnType type, object? value)
    {
        if (value is null)
        {
            bw.Write((byte)0);
            return;
        }

        switch (type)
        {
            case ColumnType.Double:
                bw.Write((byte)1);
                bw.Write(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
                break;
            case ColumnType.Int:
                bw.Write((byte)2);
                bw.Write(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
                break;
            case ColumnType.Bool:
                bw.Write((byte)3);
                bw.Write(Convert.ToBoolean(value));
                break;
            case ColumnType.String:
                bw.Write((byte)4);
                bw.Write(value.ToString() ?? string.Empty);
                break;
            case ColumnType.DateTime:
                bw.Write((byte)5);
                bw.Write(value is DateTime dt ? dt.ToBinary() : 0L);
                break;
            default:
                bw.Write((byte)6);
                bw.Write(value.ToString() ?? string.Empty);
                break;
        }
    }

    private static object? ReadTypedValue(System.IO.BinaryReader br, ColumnType type)
    {
        byte tag = br.ReadByte();
        if (tag == 0) return null;

        return type switch
        {
            ColumnType.Double => br.ReadDouble(),
            ColumnType.Int => br.ReadInt32(),
            ColumnType.Bool => br.ReadBoolean(),
            ColumnType.String => br.ReadString(),
            ColumnType.DateTime => DateTime.FromBinary(br.ReadInt64()),
            _ => br.ReadString()
        };
    }
}
