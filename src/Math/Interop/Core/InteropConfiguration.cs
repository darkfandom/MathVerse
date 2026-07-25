namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration options for the interoperability engine.
/// </summary>
public sealed class InteropConfiguration
{
    /// <summary>
    /// Gets or sets the default serialization format.
    /// </summary>
    public string DefaultFormat { get; set; } = "json";

    /// <summary>
    /// Gets or sets the maximum payload size in bytes.
    /// </summary>
    public long MaxPayloadSize { get; set; } = 1024 * 1024 * 100;

    /// <summary>
    /// Gets or sets whether compression is enabled for serialized payloads.
    /// </summary>
    public bool EnableCompression { get; set; }

    /// <summary>
    /// Gets or sets whether to preserve numerical precision during exchange.
    /// </summary>
    public bool PreservePrecision { get; set; } = true;

    /// <summary>
    /// Gets or sets the encoding to use for text-based formats.
    /// </summary>
    public string TextEncoding { get; set; } = "utf-8";

    /// <summary>
    /// Gets or sets the version compatibility mode.
    /// </summary>
    public VersionCompatibility CompatibilityMode { get; set; } = VersionCompatibility.Latest;

    /// <summary>
    /// Gets or sets the custom adapter configurations.
    /// </summary>
    public Dictionary<string, string> AdapterOptions { get; } = new();

    /// <summary>
    /// Gets or sets the timeout for interop operations.
    /// </summary>
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Creates a default configuration.
    /// </summary>
    /// <returns>A new InteropConfiguration with default values.</returns>
    public static InteropConfiguration CreateDefault()
    {
        return new InteropConfiguration();
    }
}

/// <summary>
/// Specifies version compatibility modes for interoperability.
/// </summary>
public enum VersionCompatibility
{
    /// <summary>Use the latest format version.</summary>
    Latest,

    /// <summary>Maintain backward compatibility with older versions.</summary>
    BackwardCompatible,

    /// <summary>Strict version matching.</summary>
    Strict
}
