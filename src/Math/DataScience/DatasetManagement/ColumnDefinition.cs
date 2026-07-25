namespace MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Represents the type of data stored in a column.
/// </summary>
public enum ColumnType
{
    /// <summary>Double-precision floating-point value.</summary>
    Double,

    /// <summary>Integer value.</summary>
    Int,

    /// <summary>String value.</summary>
    String,

    /// <summary>Boolean value.</summary>
    Bool,

    /// <summary>DateTime value.</summary>
    DateTime,

    /// <summary>Null/missing value.</summary>
    Null
}

/// <summary>
/// Defines a column's name, type, and metadata.
/// </summary>
public sealed class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the column data type.
    /// </summary>
    public ColumnType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column allows null values.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a description of the column.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnDefinition"/> class.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="type">The column data type.</param>
    public ColumnDefinition(string name, ColumnType type)
    {
        Name = name;
        Type = type;
        IsNullable = true;
    }
}