namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Options for individual interoperability operations.
/// </summary>
public sealed class InteropOptions
{
    /// <summary>
    /// Gets or sets the target format for export operations.
    /// </summary>
    public string? TargetFormat { get; set; }

    /// <summary>
    /// Gets or sets the source format for import operations.
    /// </summary>
    public string? SourceFormat { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the input before processing.
    /// </summary>
    public bool ValidateInput { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include metadata in the output.
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets additional options as key-value pairs.
    /// </summary>
    public Dictionary<string, string> AdditionalOptions { get; } = new();

    /// <summary>
    /// Gets or sets the cancellation token for async operations.
    /// </summary>
    public System.Threading.CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Creates default options for the specified format.
    /// </summary>
    /// <param name="format">The target or source format.</param>
    /// <returns>A new InteropOptions instance.</returns>
    public static InteropOptions Create(string format)
    {
        _ = format ?? throw new ArgumentNullException(nameof(format));
        return new InteropOptions { TargetFormat = format };
    }
}
