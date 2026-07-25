namespace MathVerse.Math.Interop.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for a specific adapter.
/// </summary>
public sealed class AdapterConfiguration
{
    /// <summary>
    /// Gets or sets the adapter identifier.
    /// </summary>
    public string AdapterId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this adapter is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the priority order for this adapter when multiple adapters handle the same format.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the timeout for adapter operations.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets custom adapter settings.
    /// </summary>
    public Dictionary<string, string> Settings { get; } = new();

    /// <summary>
    /// Creates a default configuration for the specified adapter.
    /// </summary>
    /// <param name="adapterId">The adapter identifier.</param>
    /// <returns>A new AdapterConfiguration.</returns>
    public static AdapterConfiguration CreateDefault(string adapterId)
    {
        return new AdapterConfiguration { AdapterId = adapterId };
    }
}
