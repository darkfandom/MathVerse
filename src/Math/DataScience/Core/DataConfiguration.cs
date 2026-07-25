namespace MathVerse.Math.DataScience.Core;

using System.Globalization;

/// <summary>
/// Full configuration for data operations including parsing and formatting settings.
/// </summary>
public sealed class DataConfiguration
{
    /// <summary>
    /// Gets or sets the data options.
    /// </summary>
    public DataOptions Options { get; set; } = DataOptions.Default;

    /// <summary>
    /// Gets or sets the default delimiter used in delimited text files.
    /// </summary>
    public char DefaultDelimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets the default date format string for parsing dates.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets the string representation used for null values.
    /// </summary>
    public string NullRepresentation { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    public long MaxFileSize { get; set; } = 100L * 1024 * 1024;

    /// <summary>
    /// Gets the default configuration instance.
    /// </summary>
    public static DataConfiguration Default { get; } = new();
}