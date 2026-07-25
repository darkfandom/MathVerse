namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Unified exporter that dispatches to the appropriate writer based on format.
/// </summary>
public sealed class DatasetExporter
{
    /// <summary>
    /// Exports a dataset to the specified format.
    /// </summary>
    /// <param name="dataset">The dataset to export.</param>
    /// <param name="format">The target format (csv, tsv, json, xml, md, html, bin).</param>
    /// <returns>The exported data as a byte array.</returns>
    public byte[] Export(Dataset dataset, string format)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        return format.ToLowerInvariant() switch
        {
            "csv" => Encoding.UTF8.GetBytes(new CsvWriter().Write(dataset)),
            "tsv" => Encoding.UTF8.GetBytes(new CsvWriter().Write(dataset, '\t')),
            "json" => Encoding.UTF8.GetBytes(new JsonWriter().Write(dataset)),
            "xml" => Encoding.UTF8.GetBytes(new XmlWriter().Write(dataset)),
            "md" => Encoding.UTF8.GetBytes(new MarkdownWriter().Write(dataset)),
            "html" => Encoding.UTF8.GetBytes(new HtmlWriter().Write(dataset)),
            "bin" => new BinaryWriter().Write(dataset),
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };
    }

    /// <summary>
    /// Exports a dataset to a string in the specified format.
    /// </summary>
    /// <param name="dataset">The dataset to export.</param>
    /// <param name="format">The target format (csv, tsv, json, xml, md, html).</param>
    /// <returns>The exported data as a string.</returns>
    public string ExportAsString(Dataset dataset, string format)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        return format.ToLowerInvariant() switch
        {
            "csv" => new CsvWriter().Write(dataset),
            "tsv" => new CsvWriter().Write(dataset, '\t'),
            "json" => new JsonWriter().Write(dataset),
            "xml" => new XmlWriter().Write(dataset),
            "md" => new MarkdownWriter().Write(dataset),
            "html" => new HtmlWriter().Write(dataset),
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };
    }
}