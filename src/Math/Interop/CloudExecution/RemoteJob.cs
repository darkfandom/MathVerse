namespace MathVerse.Math.Interop.CloudExecution;

using System;
using System.Collections.Generic;

/// <summary>
/// Priority levels for remote computation jobs.
/// </summary>
public enum JobPriority
{
    /// <summary>Low priority.</summary>
    Low,

    /// <summary>Normal priority.</summary>
    Normal,

    /// <summary>High priority.</summary>
    High,

    /// <summary>Critical priority.</summary>
    Critical
}

/// <summary>
/// Execution status of a remote computation job.
/// </summary>
public enum JobStatus
{
    /// <summary>The job is waiting to be scheduled.</summary>
    Pending,

    /// <summary>The job is executing.</summary>
    Running,

    /// <summary>The job completed successfully.</summary>
    Completed,

    /// <summary>The job failed.</summary>
    Failed,

    /// <summary>The job was cancelled.</summary>
    Cancelled
}

/// <summary>
/// Represents a remote computation job submitted to a cluster or cloud.
/// </summary>
public sealed class RemoteJob
{
    /// <summary>Gets or sets the unique job identifier.</summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Gets or sets the display name of the job.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the job type identifier.</summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>Gets the input parameters for the job.</summary>
    public Dictionary<string, object> Parameters { get; } = new();

    /// <summary>Gets or sets the priority of the job.</summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>Gets or sets the optional timeout for the job.</summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>Gets or sets the cluster node this job is assigned to.</summary>
    public string? AssignedCluster { get; set; }

    /// <summary>Gets or sets the current status of the job.</summary>
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>Gets or sets the time the job was submitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the time the job completed, or null if still running.</summary>
    public DateTimeOffset? CompletedAt { get; set; }
}
