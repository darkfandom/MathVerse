namespace MathVerse.Math.DataScience.Import;

using System;
using System.IO;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Unified importer that auto-detects format from file extension.
/// </summary>
public sealed class DatasetImporter
{
    private readonly DataConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatasetImporter"/> class with default configuration.
    /// </summary>
    public DatasetImporter() : this(DataConfiguration.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatasetImporter"/> class.
    /// </summary>
    /// <param name="configuration">The data configuration to use.</param>
    public DatasetImporter(DataConfiguration configuration)
    {
        _configuration = configuration ?? DataConfiguration.Default;
    }

    /// <summary>
    /// Imports a dataset from the specified file path, auto-detecting the format from the extension.
    /// </summary>
    /// <param name="path">The file path to import from.</param>
    /// <returns>The imported dataset.</returns>
    public Dataset Import(string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}", path);
        }

        string extension = Path.GetExtension(path).ToLowerInvariant();
        string content = File.ReadAllText(path, Encoding.UTF8);

        return extension switch
        {
            ".csv" => new CsvReader().Read(content, _configuration.DefaultDelimiter),
            ".tsv" => new TsvReader().Read(content),
            ".json" => new JsonReader().Read(content),
            ".xml" => new XmlReader().Read(content),
            ".bin" => new BinaryReader().Read(File.ReadAllBytes(path)),
            _ => new CsvReader().Read(content, _configuration.DefaultDelimiter)
        };
    }
}