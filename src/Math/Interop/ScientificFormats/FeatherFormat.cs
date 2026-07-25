namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Interface for reading Apache Arrow Feather format data.
/// </summary>
public interface IFeatherReader
{
    /// <summary>
    /// Reads a column by name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The column data as an array, or null if not found.</returns>
    Array? ReadColumn(string name);

    /// <summary>
    /// Lists all available column names.
    /// </summary>
    /// <returns>An array of column names.</returns>
    string[] ListColumns();

    /// <summary>
    /// Gets the number of rows in the table.
    /// </summary>
    /// <returns>The row count.</returns>
    int GetRowCount();

    /// <summary>
    /// Gets the metadata associated with the file.
    /// </summary>
    /// <returns>A dictionary of metadata key-value pairs.</returns>
    Dictionary<string, string> GetMetadata();

    /// <summary>
    /// Checks if a column exists.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>True if the column exists.</returns>
    bool ColumnExists(string name);
}

/// <summary>
/// Interface for writing Apache Arrow Feather format data.
/// </summary>
public interface IFeatherWriter
{
    /// <summary>
    /// Writes a column with the specified name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="data">The column data.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteColumn(string name, Array data);

    /// <summary>
    /// Writes metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteMetadata(string key, string value);

    /// <summary>
    /// Removes a column by name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>True if the column was removed.</returns>
    bool RemoveColumn(string name);

    /// <summary>
    /// Clears all columns and metadata.
    /// </summary>
    void Clear();
}

/// <summary>
/// In-memory implementation of Apache Arrow Feather format abstraction for AOT safety.
/// Stores columnar data in dictionaries.
/// </summary>
public sealed class FeatherFile : IFeatherReader, IFeatherWriter
{
    private readonly Dictionary<string, Array> _columns = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of columns in the file.
    /// </summary>
    public int ColumnCount => _columns.Count;

    /// <summary>
    /// Gets the number of metadata entries.
    /// </summary>
    public int MetadataCount => _metadata.Count;

    /// <inheritdoc/>
    public Array? ReadColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        _columns.TryGetValue(name, out var data);
        return data;
    }

    /// <inheritdoc/>
    public string[] ListColumns()
    {
        return _columns.Keys.ToArray();
    }

    /// <inheritdoc/>
    public int GetRowCount()
    {
        foreach (var kvp in _columns)
        {
            return kvp.Value.Length;
        }
        return 0;
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetMetadata()
    {
        return new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool ColumnExists(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && _columns.ContainsKey(name);
    }

    /// <inheritdoc/>
    public bool WriteColumn(string name, Array data)
    {
        if (string.IsNullOrWhiteSpace(name) || data is null) return false;
        _columns[name] = data;
        return true;
    }

    /// <inheritdoc/>
    public bool WriteMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || value is null) return false;
        _metadata[key] = value;
        return true;
    }

    /// <inheritdoc/>
    public bool RemoveColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return _columns.Remove(name);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _columns.Clear();
        _metadata.Clear();
    }

    /// <summary>
    /// Serializes the Feather file to a byte array.
    /// </summary>
    /// <returns>A byte array containing the serialized Feather data.</returns>
    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(_columns.Count);
        writer.Write(_metadata.Count);

        foreach (var kvp in _metadata)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        foreach (var kvp in _columns)
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

        return stream.ToArray();
    }

    /// <summary>
    /// Deserializes a Feather file from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized Feather data.</param>
    /// <returns>A new <see cref="FeatherFile"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public static FeatherFile Deserialize(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var file = new FeatherFile();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var columnCount = reader.ReadInt32();
        var metadataCount = reader.ReadInt32();

        for (int m = 0; m < metadataCount; m++)
        {
            var key = reader.ReadString();
            var value = reader.ReadString();
            file._metadata[key] = value;
        }

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

            file._columns[name] = columnData;
        }

        return file;
    }
}
