namespace MathVerse.Math.Interop.PluginInfrastructure;

using System;
using System.Collections.Generic;

/// <summary>
/// Describes a plugin's metadata and dependencies.
/// </summary>
public sealed class PluginDescriptor
{
    /// <summary>
    /// Gets or sets the unique plugin identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin version string.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of plugin IDs that this plugin depends on.
    /// </summary>
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the metadata dictionary for custom key-value pairs.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = new();
}
