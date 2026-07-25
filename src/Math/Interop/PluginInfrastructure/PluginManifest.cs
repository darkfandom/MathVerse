namespace MathVerse.Math.Interop.PluginInfrastructure;

using System;
using System.Text;

/// <summary>
/// Represents a plugin manifest file format with serialization support.
/// </summary>
public sealed class PluginManifest
{
    private const char FieldSeparator = '=';

    /// <summary>
    /// Gets or sets the unique plugin identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plugin version.
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
    /// Gets the list of required permissions.
    /// </summary>
    public string[] RequiredPermissions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of supported platforms.
    /// </summary>
    public string[] SupportedPlatforms { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Parses a manifest content string into a <see cref="PluginManifest"/> instance.
    /// </summary>
    /// <param name="manifestContent">The manifest text content.</param>
    /// <returns>The parsed manifest.</returns>
    public static PluginManifest Parse(string manifestContent)
    {
        ArgumentException.ThrowIfNullOrEmpty(manifestContent);

        var manifest = new PluginManifest();
        var lines = manifestContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var idx = trimmed.IndexOf(FieldSeparator);
            if (idx < 0) continue;

            var key = trimmed[..idx].Trim();
            var value = trimmed[(idx + 1)..].Trim();

            switch (key)
            {
                case "Id":
                    manifest.Id = value;
                    break;
                case "Name":
                    manifest.Name = value;
                    break;
                case "Version":
                    manifest.Version = value;
                    break;
                case "Author":
                    manifest.Author = value;
                    break;
                case "Description":
                    manifest.Description = value;
                    break;
                case "RequiredPermissions":
                    manifest.RequiredPermissions = ParseArrayValue(value);
                    break;
                case "SupportedPlatforms":
                    manifest.SupportedPlatforms = ParseArrayValue(value);
                    break;
            }
        }

        return manifest;
    }

    /// <summary>
    /// Serializes the manifest to its text format.
    /// </summary>
    /// <returns>The serialized manifest string.</returns>
    public string Serialize()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# MathVerse Plugin Manifest");
        sb.AppendLine($"Id{FieldSeparator}{Id}");
        sb.AppendLine($"Name{FieldSeparator}{Name}");
        sb.AppendLine($"Version{FieldSeparator}{Version}");
        sb.AppendLine($"Author{FieldSeparator}{Author}");
        sb.AppendLine($"Description{FieldSeparator}{Description}");
        sb.AppendLine($"RequiredPermissions{FieldSeparator}{FormatArrayValue(RequiredPermissions)}");
        sb.AppendLine($"SupportedPlatforms{FieldSeparator}{FormatArrayValue(SupportedPlatforms)}");
        return sb.ToString();
    }

    private static string[] ParseArrayValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        var parts = value.Split(',');
        var result = new string[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            result[i] = parts[i].Trim();
        }
        return result;
    }

    private static string FormatArrayValue(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }
        return string.Join(", ", values);
    }
}
