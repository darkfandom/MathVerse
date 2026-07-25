namespace MathVerse.Math.Interop.DataExchange;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Column-oriented storage format that stores data column-by-column for better compression.
/// </summary>
public sealed class ColumnarDatasetFormat
{
    private const int FormatSignature = 0x4D564346;
    private const int FormatVersion = 1;

    /// <summary>
    /// Serializes a dataset into a columnar binary format.
    /// </summary>
    /// <param name="ds">The dataset to serialize.</param>
    /// <returns>A byte array containing the columnar-encoded dataset.</returns>
    public byte[] Serialize(Dataset ds)
    {
        if (ds is null)
            throw new ArgumentNullException(nameof(ds));

        using var ms = new System.IO.MemoryStream();
        using var bw = new System.IO.BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        bw.Write(FormatSignature);
        bw.Write(FormatVersion);
        bw.Write(ds.Name ?? string.Empty);
        bw.Write(ds.Rows.Count);
        bw.Write(ds.Schema.Columns.Count);

        foreach (var col in ds.Schema.Columns)
        {
            bw.Write(col.Name);
            bw.Write((int)col.Type);
        }

        foreach (var col in ds.Schema.Columns)
        {
            WriteColumn(bw, ds, col);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes a columnar binary format back into a dataset.
    /// </summary>
    /// <param name="data">The columnar binary data to deserialize.</param>
    /// <returns>The deserialized dataset.</returns>
    public Dataset Deserialize(byte[] data)
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        using var ms = new System.IO.MemoryStream(data);
        using var br = new System.IO.BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        int signature = br.ReadInt32();
        if (signature != FormatSignature)
            throw new InvalidOperationException("Invalid columnar dataset format signature.");

        _ = br.ReadInt32();
        var ds = new Dataset { Name = br.ReadString() };
        int rowCount = br.ReadInt32();
        int colCount = br.ReadInt32();

        var columns = new List<ColumnDefinition>(colCount);
        for (int i = 0; i < colCount; i++)
        {
            string name = br.ReadString();
            var type = (ColumnType)br.ReadInt32();
            var colDef = new ColumnDefinition(name, type);
            columns.Add(colDef);
            ds.Schema.Columns.Add(colDef);
        }

        var columnData = new List<object?[]>();
        for (int i = 0; i < colCount; i++)
        {
            columnData.Add(ReadColumn(br, columns[i].Type, rowCount));
        }

        for (int r = 0; r < rowCount; r++)
        {
            var row = new Dictionary<string, object?>(colCount, StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < colCount; c++)
            {
                row[columns[c].Name] = columnData[c][r];
            }
            ds.Rows.Add(row);
        }

        return ds;
    }

    private static void WriteColumn(System.IO.BinaryWriter bw, Dataset ds, ColumnDefinition col)
    {
        int rowCount = ds.Rows.Count;
        bw.Write(rowCount);

        for (int r = 0; r < rowCount; r++)
        {
            var row = ds.Rows[r];
            if (!row.TryGetValue(col.Name, out var value) || value is null)
            {
                bw.Write((byte)0);
                continue;
            }

            switch (col.Type)
            {
                case ColumnType.Double:
                    bw.Write((byte)1);
                    bw.Write(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case ColumnType.Int:
                    bw.Write((byte)1);
                    bw.Write(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case ColumnType.Bool:
                    bw.Write((byte)1);
                    bw.Write(Convert.ToBoolean(value));
                    break;
                case ColumnType.String:
                    bw.Write((byte)1);
                    bw.Write(value.ToString() ?? string.Empty);
                    break;
                case ColumnType.DateTime:
                    bw.Write((byte)1);
                    bw.Write(value is DateTime dt ? dt.ToBinary() : 0L);
                    break;
                default:
                    bw.Write((byte)1);
                    bw.Write(value.ToString() ?? string.Empty);
                    break;
            }
        }
    }

    private static object?[] ReadColumn(System.IO.BinaryReader br, ColumnType type, int rowCount)
    {
        var values = new object?[rowCount];
        int count = br.ReadInt32();
        int limit = count < rowCount ? count : rowCount;

        for (int r = 0; r < limit; r++)
        {
            byte tag = br.ReadByte();
            if (tag == 0)
            {
                values[r] = null;
                continue;
            }

            values[r] = type switch
            {
                ColumnType.Double => br.ReadDouble(),
                ColumnType.Int => br.ReadInt32(),
                ColumnType.Bool => br.ReadBoolean(),
                ColumnType.String => br.ReadString(),
                ColumnType.DateTime => DateTime.FromBinary(br.ReadInt64()),
                _ => br.ReadString()
            };
        }
        return values;
    }
}
