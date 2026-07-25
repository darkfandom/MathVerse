namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interface for reading NetCDF-like variables and attributes.
/// </summary>
public interface INetCDFReader
{
    /// <summary>
    /// Reads a variable by name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The variable data as an array, or null if not found.</returns>
    Array? ReadVariable(string name);

    /// <summary>
    /// Lists all available variable names.
    /// </summary>
    /// <returns>An array of variable names.</returns>
    string[] ListVariables();

    /// <summary>
    /// Reads all global attributes.
    /// </summary>
    /// <returns>A dictionary of global attribute name-value pairs.</returns>
    Dictionary<string, string> ReadGlobalAttributes();

    /// <summary>
    /// Reads a global attribute by name.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <returns>The attribute value, or null if not found.</returns>
    string? ReadGlobalAttribute(string name);

    /// <summary>
    /// Gets the dimensions of a variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The dimension names, or an empty array if not found.</returns>
    string[] GetDimensions(string name);

    /// <summary>
    /// Checks if a variable exists.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>True if the variable exists.</returns>
    bool VariableExists(string name);
}

/// <summary>
/// Interface for writing NetCDF-like variables and attributes.
/// </summary>
public interface INetCDFWriter
{
    /// <summary>
    /// Writes a variable with the specified name and dimensions.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="data">The variable data.</param>
    /// <param name="dimensions">The dimension names for this variable.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteVariable(string name, Array data, string[] dimensions);

    /// <summary>
    /// Writes a global attribute.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>True if the write succeeded.</returns>
    bool WriteGlobalAttribute(string name, string value);

    /// <summary>
    /// Defines a global dimension.
    /// </summary>
    /// <param name="name">The dimension name.</param>
    /// <param name="length">The dimension length.</param>
    /// <returns>True if the dimension was defined.</returns>
    bool DefineDimension(string name, int length);

    /// <summary>
    /// Removes a variable by name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>True if the variable was removed.</returns>
    bool RemoveVariable(string name);

    /// <summary>
    /// Removes a global attribute by name.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <returns>True if the attribute was removed.</returns>
    bool RemoveGlobalAttribute(string name);
}

/// <summary>
/// In-memory implementation of NetCDF-like file abstraction for AOT safety.
/// Backed by dictionaries storing variables, dimensions, and attributes.
/// </summary>
public sealed class NetCDFFile : INetCDFReader, INetCDFWriter
{
    private readonly Dictionary<string, Array> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _variableDimensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _globalAttributes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _dimensions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of variables in the file.
    /// </summary>
    public int VariableCount => _variables.Count;

    /// <summary>
    /// Gets the number of global attributes in the file.
    /// </summary>
    public int GlobalAttributeCount => _globalAttributes.Count;

    /// <summary>
    /// Gets the number of defined dimensions.
    /// </summary>
    public int DimensionCount => _dimensions.Count;

    /// <inheritdoc/>
    public Array? ReadVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        _variables.TryGetValue(name, out var data);
        return data;
    }

    /// <inheritdoc/>
    public string[] ListVariables()
    {
        return _variables.Keys.ToArray();
    }

    /// <inheritdoc/>
    public Dictionary<string, string> ReadGlobalAttributes()
    {
        return new Dictionary<string, string>(_globalAttributes, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public string? ReadGlobalAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        _globalAttributes.TryGetValue(name, out var value);
        return value;
    }

    /// <inheritdoc/>
    public string[] GetDimensions(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Array.Empty<string>();
        _variableDimensions.TryGetValue(name, out var dims);
        return dims ?? Array.Empty<string>();
    }

    /// <inheritdoc/>
    public bool VariableExists(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && _variables.ContainsKey(name);
    }

    /// <inheritdoc/>
    public bool WriteVariable(string name, Array data, string[] dimensions)
    {
        if (string.IsNullOrWhiteSpace(name) || data is null) return false;
        _variables[name] = data;
        _variableDimensions[name] = dimensions ?? Array.Empty<string>();
        return true;
    }

    /// <inheritdoc/>
    public bool WriteGlobalAttribute(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name) || value is null) return false;
        _globalAttributes[name] = value;
        return true;
    }

    /// <inheritdoc/>
    public bool DefineDimension(string name, int length)
    {
        if (string.IsNullOrWhiteSpace(name) || length < 0) return false;
        _dimensions[name] = length;
        return true;
    }

    /// <inheritdoc/>
    public bool RemoveVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        _variableDimensions.Remove(name);
        return _variables.Remove(name);
    }

    /// <inheritdoc/>
    public bool RemoveGlobalAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return _globalAttributes.Remove(name);
    }

    /// <summary>
    /// Gets the length of a defined dimension.
    /// </summary>
    /// <param name="name">The dimension name.</param>
    /// <returns>The dimension length, or -1 if not defined.</returns>
    public int GetDimensionLength(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return -1;
        _dimensions.TryGetValue(name, out var length);
        return length;
    }

    /// <summary>
    /// Clears all variables, dimensions, and attributes.
    /// </summary>
    public void Clear()
    {
        _variables.Clear();
        _variableDimensions.Clear();
        _globalAttributes.Clear();
        _dimensions.Clear();
    }
}
