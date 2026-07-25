namespace MathVerse.Math.Interop.CloudExecution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Client for submitting and managing remote computation jobs.
/// </summary>
public sealed class RemoteExecutionClient
{
    private readonly Dictionary<string, RemoteJob> _jobs = new();
    private readonly object _lock = new();
    private bool _connected;

    /// <summary>
    /// Gets whether the client is connected to a remote endpoint.
    /// </summary>
    public bool IsConnected => _connected;

    /// <summary>
    /// Gets the remote endpoint URL, or null if not connected.
    /// </summary>
    public string? Endpoint { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteExecutionClient"/> class.
    /// </summary>
    /// <param name="endpoint">The remote endpoint URL.</param>
    public RemoteExecutionClient(string? endpoint = null)
    {
        Endpoint = endpoint;
        _connected = endpoint is not null;
    }

    /// <summary>
    /// Submits a job for remote execution.
    /// </summary>
    /// <param name="job">The job to submit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the submitted job.</returns>
    public ValueTask<InteropResult<RemoteJob>> SubmitJobAsync(RemoteJob job, CancellationToken ct = default)
    {
        _ = job ?? throw new ArgumentNullException(nameof(job));

        if (!_connected)
        {
            return new ValueTask<InteropResult<RemoteJob>>(
                InteropResult<RemoteJob>.Failure("Not connected to a remote endpoint."));
        }

        lock (_lock)
        {
            job.Status = JobStatus.Pending;
            job.SubmittedAt = DateTimeOffset.UtcNow;
            _jobs[job.JobId] = job;
        }

        return new ValueTask<InteropResult<RemoteJob>>(InteropResult<RemoteJob>.Success(job));
    }

    /// <summary>
    /// Retrieves the result of a previously submitted job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the job outcome.</returns>
    public ValueTask<RemoteResult> GetResultAsync(string jobId, CancellationToken ct = default)
    {
        _ = jobId ?? throw new ArgumentNullException(nameof(jobId));

        if (!_connected)
        {
            return new ValueTask<RemoteResult>(new RemoteResult
            {
                JobId = jobId,
                Status = JobStatus.Failed,
                ErrorMessage = "Not connected to a remote endpoint."
            });
        }

        lock (_lock)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return new ValueTask<RemoteResult>(new RemoteResult
                {
                    JobId = jobId,
                    Status = JobStatus.Failed,
                    ErrorMessage = $"Job '{jobId}' not found."
                });
            }

            var result = new RemoteResult
            {
                JobId = job.JobId,
                Status = job.Status,
                CompletedAt = job.CompletedAt ?? DateTimeOffset.UtcNow,
            };

            if (job.CompletedAt.HasValue && job.SubmittedAt != default)
            {
                result.Duration = job.CompletedAt.Value - job.SubmittedAt;
            }

            return new ValueTask<RemoteResult>(result);
        }
    }

    /// <summary>
    /// Cancels a running or pending job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> CancelJobAsync(string jobId, CancellationToken ct = default)
    {
        _ = jobId ?? throw new ArgumentNullException(nameof(jobId));

        if (!_connected)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Not connected to a remote endpoint."));
        }

        lock (_lock)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return new ValueTask<InteropResult>(InteropResult.Failure($"Job '{jobId}' not found."));
            }

            if (job.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Cancelled)
            {
                return new ValueTask<InteropResult>(
                    InteropResult.Failure($"Job '{jobId}' is already in terminal state '{job.Status}'."));
            }

            job.Status = JobStatus.Cancelled;
            job.CompletedAt = DateTimeOffset.UtcNow;
        }

        return new ValueTask<InteropResult>(InteropResult.Success());
    }
}
