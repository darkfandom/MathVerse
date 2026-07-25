namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Diagnostics;

/// <summary>
/// Tracks diagnostics specific to serialization and deserialization operations.
/// </summary>
public sealed class SerializationDiagnostics
{
    /// <summary>
    /// Gets or sets the number of objects serialized.
    /// </summary>
    public long ObjectsSerialized { get; set; }

    /// <summary>
    /// Gets or sets the number of objects deserialized.
    /// </summary>
    public long ObjectsDeserialized { get; set; }

    /// <summary>
    /// Gets or sets the total bytes written.
    /// </summary>
    public long BytesWritten { get; set; }

    /// <summary>
    /// Gets or sets the total bytes read.
    /// </summary>
    public long BytesRead { get; set; }

    /// <summary>
    /// Gets or sets the total serialization time.
    /// </summary>
    public TimeSpan TotalSerializationTime { get; set; }

    /// <summary>
    /// Gets or sets the total deserialization time.
    /// </summary>
    public TimeSpan TotalDeserializationTime { get; set; }

    /// <summary>
    /// Gets the average bytes per object for serialization.
    /// </summary>
    public double AverageBytesPerObject => ObjectsSerialized > 0 ? (double)BytesWritten / ObjectsSerialized : 0;

    /// <summary>
    /// Gets the serialization throughput in bytes per second.
    /// </summary>
    public double SerializationThroughput => TotalSerializationTime.TotalSeconds > 0 ? BytesWritten / TotalSerializationTime.TotalSeconds : 0;

    /// <summary>
    /// Creates a new instance with default values.
    /// </summary>
    /// <returns>A new SerializationDiagnostics instance.</returns>
    public static SerializationDiagnostics Create()
    {
        return new SerializationDiagnostics();
    }

    /// <summary>
    /// Merges diagnostics from another instance.
    /// </summary>
    /// <param name="other">The other diagnostics instance.</param>
    public void MergeFrom(SerializationDiagnostics other)
    {
        _ = other ?? throw new ArgumentNullException(nameof(other));
        ObjectsSerialized += other.ObjectsSerialized;
        ObjectsDeserialized += other.ObjectsDeserialized;
        BytesWritten += other.BytesWritten;
        BytesRead += other.BytesRead;
        TotalSerializationTime += other.TotalSerializationTime;
        TotalDeserializationTime += other.TotalDeserializationTime;
    }
}
