namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interface for reading HDF5-like datasets and attributes.
/// </summary>
public interface IHDF5Reader
{
    /// <summary>
    /// Reads a dataset by name.
    /// </summary>
    /// <param name="name">The dataset name.</param>
    /// <returns>The dataset as an array, or null if not found.</returns>
    Array? ReadDataset(string name);

    /// <summary>
    /// Reads an attribute at the specified path.
    /// </summary>
    /// <param name="path">The attribute path (e.g., "/group/attr").</param>
    /// <returns>The attribute value, or null if not found.</returns>
    string? ReadAttribute(string path);

    /// <summary>
    /// Lists all available dataset names.
    /// </summary>
    /// <returns>An array of dataset names.</returns>
    string[] ListDatasets();

    /// <summary>
    /// Lists all available group names.
    /// </summary>
    /// <returns>An array of group names.</returns>
    string[] ListGroups();

    /// <summary>
    /// Checks if a dataset exists.
    /// </summary>
    /// <param name="name">The dataset name.</param>
    /// <returns>True if the dataset exists.</returns>
    bool DatasetExists(string name);

    /// <summary>
    /// Checks if an attribute exists at the specified path.
    /// </summary>
    /// <param name="path">The attribute path.</param>
    /// <returns>True if the attribute exists.</returns>
    bool AttributeExists(string path);
}

/// <summary>
/// Interface for writing HDF5-like datasets and attributes.
/// </summary>
public interface IHDF5Writer
{
    /// <summary>
    /// Writes a dataset with the specified name.
    /// </summary>
    /// <param name="name">The dataset name.</param>
    /// <param name="data">The data to write.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteDataset(string name, Array data);

    /// <summary>
    /// Writes an attribute at the specified path.
    /// </summary>
    /// <param name="path">The attribute path.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteAttribute(string path, string value);

    /// <summary>
    /// Removes a dataset by name.
    /// </summary>
    /// <param name="name">The dataset name.</param>
    /// <returns>True if the dataset was removed.</returns>
    bool RemoveDataset(string name);

    /// <summary>
    /// Removes an attribute at the specified path.
    /// </summary>
    /// <param name="path">The attribute path.</param>
    /// <returns>True if the attribute was removed.</returns>
    bool RemoveAttribute(string path);

    /// <summary>
    /// Creates a group at the specified path.
    /// </summary>
    /// <param name="path">The group path.</param>
    /// <returns>True if the group was created.</returns>
    bool CreateGroup(string path);
}

/// <summary>
/// In-memory implementation of HDF5-like file abstraction for AOT safety.
/// Backed by dictionaries storing datasets and attributes.
/// </summary>
public sealed class HDF5File : IHDF5Reader, IHDF5Writer
{
    private readonly Dictionary<string, Array> _datasets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _attributes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _groups = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of datasets in the file.
    /// </summary>
    public int DatasetCount => _datasets.Count;

    /// <summary>
    /// Gets the number of attributes in the file.
    /// </summary>
    public int AttributeCount => _attributes.Count;

    /// <summary>
    /// Gets the number of groups in the file.
    /// </summary>
    public int GroupCount => _groups.Count;

    /// <inheritdoc/>
    public Array? ReadDataset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        _datasets.TryGetValue(name, out var data);
        return data;
    }

    /// <inheritdoc/>
    public string? ReadAttribute(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        _attributes.TryGetValue(path, out var value);
        return value;
    }

    /// <inheritdoc/>
    public string[] ListDatasets()
    {
        return _datasets.Keys.ToArray();
    }

    /// <inheritdoc/>
    public string[] ListGroups()
    {
        return _groups.ToArray();
    }

    /// <inheritdoc/>
    public bool DatasetExists(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && _datasets.ContainsKey(name);
    }

    /// <inheritdoc/>
    public bool AttributeExists(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && _attributes.ContainsKey(path);
    }

    /// <inheritdoc/>
    public bool WriteDataset(string name, Array data)
    {
        if (string.IsNullOrWhiteSpace(name) || data is null) return false;
        _datasets[name] = data;
        return true;
    }

    /// <inheritdoc/>
    public bool WriteAttribute(string path, string value)
    {
        if (string.IsNullOrWhiteSpace(path) || value is null) return false;
        _attributes[path] = value;
        return true;
    }

    /// <inheritdoc/>
    public bool RemoveDataset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return _datasets.Remove(name);
    }

    /// <inheritdoc/>
    public bool RemoveAttribute(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return _attributes.Remove(path);
    }

    /// <inheritdoc/>
    public bool CreateGroup(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return _groups.Add(path);
    }

    /// <summary>
    /// Clears all datasets, attributes, and groups.
    /// </summary>
    public void Clear()
    {
        _datasets.Clear();
        _attributes.Clear();
        _groups.Clear();
    }
}
