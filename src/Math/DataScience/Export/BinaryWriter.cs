namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;

using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Writes a dataset to binary format using a length-prefixed encoding.
/// </summary>
public sealed class BinaryWriter
{
    /// <summary>
    /// Writes a dataset to a byte array.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <returns>The binary representation as a byte array.</returns>
    public byte[] Write(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var ms = new System.IO.MemoryStream();
        var headers = dataset.Schema.ColumnNames.ToArray();

        WriteInt32(ms, headers.Length);

        for (int i = 0; i < headers.Length; i++)
        {
            WriteString(ms, headers[i]);
            ColumnType colType = ColumnType.String;
            if (dataset.Schema.HasColumn(headers[i]))
            {
                colType = dataset.Schema.GetColumn(headers[i]).Type;
            }
            WriteInt32(ms, (int)colType);
        }

        WriteInt32(ms, dataset.Rows.Count);

        foreach (var row in dataset.Rows)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (row.TryGetValue(headers[i], out var val) && val != null)
                {
                    WriteBoolean(ms, false);
                    WriteValue(ms, val, dataset.Schema.HasColumn(headers[i]) ? dataset.Schema.GetColumn(headers[i]).Type : ColumnType.String);
                }
                else
                {
                    WriteBoolean(ms, true);
                }
            }
        }

        return ms.ToArray();
    }

    private static void WriteInt32(System.IO.MemoryStream ms, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        ms.Write(bytes, 0, 4);
    }

    private static void WriteDouble(System.IO.MemoryStream ms, double value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        ms.Write(bytes, 0, 8);
    }

    private static void WriteBoolean(System.IO.MemoryStream ms, bool value)
    {
        ms.WriteByte(value ? (byte)1 : (byte)0);
    }

    private static void WriteString(System.IO.MemoryStream ms, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        WriteInt32(ms, bytes.Length);
        ms.Write(bytes, 0, bytes.Length);
    }

    private static void WriteValue(System.IO.MemoryStream ms, object value, ColumnType type)
    {
        switch (type)
        {
            case ColumnType.Double:
                WriteDouble(ms, value is double d ? d : Convert.ToDouble(value));
                break;
            case ColumnType.Int:
                WriteInt32(ms, value is int i ? i : Convert.ToInt32(value));
                break;
            case ColumnType.Bool:
                WriteBoolean(ms, value is bool b && b);
                break;
            default:
                WriteString(ms, value.ToString() ?? "");
                break;
        }
    }
}