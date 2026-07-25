namespace MathVerse.Math.DataScience.Core;

using System.Globalization;

/// <summary>
/// Configuration options for data operations.
/// </summary>
public sealed class DataOptions
{
    /// <summary>
    /// Gets or sets the maximum number of rows allowed in a dataset.
    /// </summary>
    public int MaxRows { get; set; } = 100_000;

    /// <summary>
    /// Gets or sets the maximum number of columns allowed in a dataset.
    /// </summary>
    public int MaxColumns { get; set; } = 1_000;

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether parallel processing is enabled.
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the chunk size for batch processing.
    /// </summary>
    public int ChunkSize { get; set; } = 1_000;

    /// <summary>
    /// Gets or sets the maximum degree of concurrency for parallel operations.
    /// </summary>
    public int MaxConcurrency { get; set; } = System.Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the default culture used for parsing and formatting.
    /// </summary>
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets the default instance with standard settings.
    /// </summary>
    public static DataOptions Default { get; } = new();
}