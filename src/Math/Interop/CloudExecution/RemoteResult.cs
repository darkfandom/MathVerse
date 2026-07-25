namespace MathVerse.Math.Interop.CloudExecution;

using System;
using System.Collections.Generic;

/// <summary>
/// Result returned after remote job execution completes.
/// </summary>
public sealed class RemoteResult
{
    /// <summary>Gets or sets the job identifier this result belongs to.</summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>Gets or sets the final status of the job.</summary>
    public JobStatus Status { get; set; }

    /// <summary>Gets or sets the output data produced by the job, or null if none.</summary>
    public byte[]? OutputData { get; set; }

    /// <summary>Gets or sets the error message if the job failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets the total execution duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Gets or sets the time the result was produced.</summary>
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the metadata dictionary for additional result information.</summary>
    public Dictionary<string, object> Metadata { get; } = new();
}
