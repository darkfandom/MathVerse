namespace MathVerse.Math.Interop.AIModelExchange;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents metadata associated with an AI model.
/// </summary>
public sealed class ModelMetadata
{
    /// <summary>
    /// Gets or sets the model author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Gets the custom properties dictionary.
    /// </summary>
    public Dictionary<string, string> Properties { get; } = new();
}

/// <summary>
/// Describes an AI model architecture, weights, and metadata.
/// </summary>
public sealed class ModelDescriptor
{
    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model metadata.
    /// </summary>
    public ModelMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the serialized model weights.
    /// </summary>
    public byte[] Weights { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the architecture description.
    /// </summary>
    public string Architecture { get; set; } = string.Empty;
}

/// <summary>
/// Contains versioning information for a model format.
/// </summary>
public sealed class ModelVersionInfo
{
    /// <summary>
    /// Gets or sets the current format version.
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of supported versions.
    /// </summary>
    public string[] SupportedVersions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the minimum compatible version.
    /// </summary>
    public string MinCompatibleVersion { get; set; } = string.Empty;
}

/// <summary>
/// Manages model version compatibility checks.
/// </summary>
public sealed class VersionManager
{
    /// <summary>
    /// Determines whether two version strings are compatible.
    /// </summary>
    /// <param name="versionA">The first version string.</param>
    /// <param name="versionB">The second version string.</param>
    /// <returns>True if the versions are compatible.</returns>
    public bool IsCompatible(string versionA, string versionB)
    {
        if (string.IsNullOrEmpty(versionA) || string.IsNullOrEmpty(versionB))
        {
            return false;
        }

        if (!Version.TryParse(NormalizeVersion(versionA), out var vA) ||
            !Version.TryParse(NormalizeVersion(versionB), out var vB))
        {
            return string.Equals(versionA, versionB, StringComparison.OrdinalIgnoreCase);
        }

        return vA.Major == vB.Major;
    }

    /// <summary>
    /// Gets the latest version from a collection of version strings.
    /// </summary>
    /// <param name="versions">The collection of version strings.</param>
    /// <returns>The highest version string, or null if the collection is empty.</returns>
    public string? GetLatestVersion(string[] versions)
    {
        if (versions == null || versions.Length == 0)
        {
            return null;
        }

        Version? latest = null;
        string? latestStr = null;

        foreach (var v in versions)
        {
            if (Version.TryParse(NormalizeVersion(v), out var parsed))
            {
                if (latest == null || parsed > latest)
                {
                    latest = parsed;
                    latestStr = v;
                }
            }
        }

        return latestStr ?? versions[versions.Length - 1];
    }

    private static string NormalizeVersion(string version)
    {
        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            return version[1..];
        }
        return version;
    }
}
