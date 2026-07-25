namespace MathVerse.Math.Interop.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for serialization operations.
/// </summary>
public sealed class SerializationConfiguration
{
    /// <summary>
    /// Gets or sets the indentation level for text-based formats.
    /// </summary>
    public int Indentation { get; set; }

    /// <summary>
    /// Gets or sets whether to include type information in serialized output.
    /// </summary>
    public bool IncludeTypeInformation { get; set; }

    /// <summary>
    /// Gets or sets whether to include null values in serialized output.
    /// </summary>
    public bool IncludeNullValues { get; set; } = true;

    /// <summary>
    /// Gets or sets the date format string for datetime serialization.
    /// </summary>
    public string DateFormat { get; set; } = "O";

    /// <summary>
    /// Gets or sets the maximum depth for nested object serialization.
    /// </summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>
    /// Gets or sets custom type mappings for serialization.
    /// </summary>
    public Dictionary<string, string> TypeMappings { get; } = new();

    /// <summary>
    /// Creates a default serialization configuration.
    /// </summary>
    /// <returns>A new SerializationConfiguration.</returns>
    public static SerializationConfiguration CreateDefault()
    {
        return new SerializationConfiguration();
    }
}
