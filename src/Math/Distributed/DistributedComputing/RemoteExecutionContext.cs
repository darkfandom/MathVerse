namespace MathVerse.Math.Distributed.DistributedComputing;

using System;

/// <summary>
/// Provides context information for a task being executed on a remote worker node.
/// </summary>
public sealed class RemoteExecutionContext
{
    /// <summary>Gets the unique identifier of the remote task execution.</summary>
    public string RemoteTaskId { get; }

    /// <summary>Gets the node ID that initiated the task.</summary>
    public string SourceNode { get; }

    /// <summary>Gets the node ID assigned to execute the task.</summary>
    public string TargetNode { get; }

    /// <summary>Gets the UTC timestamp when this context was created.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Gets the UTC deadline by which the task must complete.</summary>
    public DateTime Deadline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteExecutionContext"/> class.
    /// </summary>
    /// <param name="remoteTaskId">Unique identifier for this remote execution.</param>
    /// <param name="sourceNode">Node that initiated the task.</param>
    /// <param name="targetNode">Node assigned to execute the task.</param>
    /// <param name="createdAt">UTC creation timestamp.</param>
    /// <param name="deadline">UTC deadline for completion.</param>
    public RemoteExecutionContext(
        string remoteTaskId,
        string sourceNode,
        string targetNode,
        DateTime createdAt,
        DateTime deadline)
    {
        RemoteTaskId = remoteTaskId ?? throw new ArgumentNullException(nameof(remoteTaskId));
        SourceNode = sourceNode ?? throw new ArgumentNullException(nameof(sourceNode));
        TargetNode = targetNode ?? throw new ArgumentNullException(nameof(targetNode));
        CreatedAt = createdAt;
        Deadline = deadline;
    }
}
