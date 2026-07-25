namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Interface for reading Apache Arrow IPC format data.
/// </summary>
public interface IArrowReader
{
    /// <summary>
    /// Reads a record batch by index.
    /// </summary>
    /// <param name="batchIndex">The batch index.</param>
    /// <returns>A dictionary mapping column names to their data, or null if not found.</returns>
    Dictionary<string, Array>? ReadBatch(int batchIndex);

    /// <summary>
    /// Reads a column by name across all batches.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The concatenated column data, or null if not found.</returns>
    Array? ReadColumn(string name);

    /// <summary>
    /// Gets the number of record batches.
    /// </summary>
    /// <returns>The batch count.</returns>
    int GetBatchCount();

    /// <summary>
    /// Lists all available column names.
    /// </summary>
    /// <returns>An array of column names.</returns>
    string[] ListColumns();

    /// <summary>
    /// Gets the schema metadata.
    /// </summary>
    /// <returns>A dictionary of schema metadata key-value pairs.</returns>
    Dictionary<string, string> GetSchemaMetadata();

    /// <summary>
    /// Gets the total number of rows across all batches.
    /// </summary>
    /// <returns>The total row count.</returns>
    int GetTotalRowCount();

    /// <summary>
    /// Checks if a column exists.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>True if the column exists.</returns>
    bool ColumnExists(string name);
}

/// <summary>
/// Interface for writing Apache Arrow IPC format data.
/// </summary>
public interface IArrowWriter
{
    /// <summary>
    /// Writes a record batch.
    /// </summary>
    /// <param name="batch">The batch data mapping column names to arrays.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteBatch(Dictionary<string, Array> batch);

    /// <summary>
    /// Writes schema metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteSchemaMetadata(string key, string value);

    /// <summary>
    /// Clears all batches and metadata.
    /// </summary>
    void Clear();
}

/// <summary>
/// In-memory implementation of Apache Arrow IPC format abstraction for AOT safety.
/// Stores record batches and schema metadata in dictionaries.
/// </summary>
public sealed class ArrowIPCFile : IArrowReader, IArrowWriter
{
    private readonly List<Dictionary<string, Array>> _batches = new();
    private readonly Dictionary<string, string> _schemaMetadata = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of record batches.
    /// </summary>
    public int BatchCount => _batches.Count;

    /// <summary>
    /// Gets the number of schema metadata entries.
    /// </summary>
    public int SchemaMetadataCount => _schemaMetadata.Count;

    /// <inheritdoc/>
    public Dictionary<string, Array>? ReadBatch(int batchIndex)
    {
        if (batchIndex < 0 || batchIndex >= _batches.Count)
        {
            return null;
        }

        return new Dictionary<string, Array>(_batches[batchIndex], StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public Array? ReadColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var columns = new List<Array>();
        foreach (var batch in _batches)
        {
            if (batch.TryGetValue(name, out var data))
            {
                columns.Add(data);
            }
        }

        if (columns.Count == 0) return null;
        if (columns.Count == 1) return columns[0];

        return ConcatenateArrays(columns.ToArray());
    }

    /// <inheritdoc/>
    public int GetBatchCount()
    {
        return _batches.Count;
    }

    /// <inheritdoc/>
    public string[] ListColumns()
    {
        if (_batches.Count == 0) return Array.Empty<string>();

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var batch in _batches)
        {
            foreach (var key in batch.Keys)
            {
                columns.Add(key);
            }
        }
        return columns.ToArray();
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetSchemaMetadata()
    {
        return new Dictionary<string, string>(_schemaMetadata, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public int GetTotalRowCount()
    {
        int total = 0;
        foreach (var batch in _batches)
        {
            foreach (var kvp in batch)
            {
                total += kvp.Value.Length;
                break;
            }
        }
        return total;
    }

    /// <inheritdoc/>
    public bool ColumnExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        foreach (var batch in _batches)
        {
            if (batch.ContainsKey(name)) return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool WriteBatch(Dictionary<string, Array> batch)
    {
        if (batch is null || batch.Count == 0) return false;
        _batches.Add(new Dictionary<string, Array>(batch, StringComparer.OrdinalIgnoreCase));
        return true;
    }

    /// <inheritdoc/>
    public bool WriteSchemaMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || value is null) return false;
        _schemaMetadata[key] = value;
        return true;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _batches.Clear();
        _schemaMetadata.Clear();
    }

    /// <summary>
    /// Serializes the Arrow IPC file to a byte array.
    /// </summary>
    /// <returns>A byte array containing the serialized Arrow IPC data.</returns>
    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(_batches.Count);
        writer.Write(_schemaMetadata.Count);

        foreach (var kvp in _schemaMetadata)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        foreach (var batch in _batches)
        {
            writer.Write(batch.Count);
            foreach (var kvp in batch)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Length);

                var elementType = kvp.Value.GetType().GetElementType()!;
                writer.Write(elementType.FullName ?? elementType.Name);

                foreach (var item in kvp.Value)
                {
                    switch (item)
                    {
                        case double d: writer.Write(d); break;
                        case float f: writer.Write(f); break;
                        case int i: writer.Write(i); break;
                        case long l: writer.Write(l); break;
                        case short s: writer.Write(s); break;
                        case bool b: writer.Write(b); break;
                        case string str: writer.Write(str ?? string.Empty); break;
                        case byte bt: writer.Write(bt); break;
                        default: writer.Write(Convert.ToDouble(item)); break;
                    }
                }
            }
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Deserializes an Arrow IPC file from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized Arrow IPC data.</param>
    /// <returns>A new <see cref="ArrowIPCFile"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public static ArrowIPCFile Deserialize(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var file = new ArrowIPCFile();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var batchCount = reader.ReadInt32();
        var metadataCount = reader.ReadInt32();

        for (int m = 0; m < metadataCount; m++)
        {
            var key = reader.ReadString();
            var value = reader.ReadString();
            file._schemaMetadata[key] = value;
        }

        for (int b = 0; b < batchCount; b++)
        {
            var columnCount = reader.ReadInt32();
            var batch = new Dictionary<string, Array>(StringComparer.OrdinalIgnoreCase);

            for (int c = 0; c < columnCount; c++)
            {
                var name = reader.ReadString();
                var length = reader.ReadInt32();
                var typeName = reader.ReadString();

                Array columnData;
                if (typeName.Contains("Double", StringComparison.Ordinal))
                {
                    var arr = new double[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadDouble();
                    columnData = arr;
                }
                else if (typeName.Contains("Single", StringComparison.Ordinal))
                {
                    var arr = new float[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadSingle();
                    columnData = arr;
                }
                else if (typeName.Contains("Int32", StringComparison.Ordinal))
                {
                    var arr = new int[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadInt32();
                    columnData = arr;
                }
                else if (typeName.Contains("Int64", StringComparison.Ordinal))
                {
                    var arr = new long[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadInt64();
                    columnData = arr;
                }
                else if (typeName.Contains("Int16", StringComparison.Ordinal))
                {
                    var arr = new short[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadInt16();
                    columnData = arr;
                }
                else if (typeName.Contains("Boolean", StringComparison.Ordinal))
                {
                    var arr = new bool[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadBoolean();
                    columnData = arr;
                }
                else if (typeName.Contains("String", StringComparison.Ordinal))
                {
                    var arr = new string[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadString();
                    columnData = arr;
                }
                else
                {
                    var arr = new double[length];
                    for (int i = 0; i < length; i++) arr[i] = reader.ReadDouble();
                    columnData = arr;
                }

                batch[name] = columnData;
            }

            file._batches.Add(batch);
        }

        return file;
    }

    private static Array ConcatenateArrays(Array[] arrays)
    {
        if (arrays.Length == 0) return Array.Empty<double>();
        if (arrays.Length == 1) return arrays[0];

        var elementType = arrays[0].GetType().GetElementType()!;
        int totalLength = 0;
        foreach (var arr in arrays)
        {
            totalLength += arr.Length;
        }

        if (elementType == typeof(double))
        {
            var result = new double[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset * sizeof(double), arr.Length * sizeof(double));
                offset += arr.Length;
            }
            return result;
        }

        if (elementType == typeof(float))
        {
            var result = new float[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset * sizeof(float), arr.Length * sizeof(float));
                offset += arr.Length;
            }
            return result;
        }

        if (elementType == typeof(int))
        {
            var result = new int[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset * sizeof(int), arr.Length * sizeof(int));
                offset += arr.Length;
            }
            return result;
        }

        if (elementType == typeof(long))
        {
            var result = new long[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset * sizeof(long), arr.Length * sizeof(long));
                offset += arr.Length;
            }
            return result;
        }

        if (elementType == typeof(short))
        {
            var result = new short[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset * sizeof(short), arr.Length * sizeof(short));
                offset += arr.Length;
            }
            return result;
        }

        if (elementType == typeof(string))
        {
            var result = new string[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Array.Copy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }

        var fallback = new double[totalLength];
        int fOffset = 0;
        foreach (var arr in arrays)
        {
            foreach (var item in arr)
            {
                fallback[fOffset++] = Convert.ToDouble(item);
            }
        }
        return fallback;
    }
}
