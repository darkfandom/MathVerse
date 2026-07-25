namespace MathVerse.Math.DataScience.Import;

using System;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Reads tab-separated value (TSV) content into a dataset.
/// </summary>
public sealed class TsvReader
{
    /// <summary>
    /// Reads TSV content and returns a dataset.
    /// </summary>
    /// <param name="content">The TSV content string.</param>
    /// <returns>A dataset containing the parsed data.</returns>
    public Dataset Read(string content)
    {
        _ = content ?? throw new ArgumentNullException(nameof(content));
        return new CsvReader().Read(content, '\t');
    }
}