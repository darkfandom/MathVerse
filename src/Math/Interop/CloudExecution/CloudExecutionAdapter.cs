namespace MathVerse.Math.Interop.CloudExecution;

using System;
using System.Threading;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Adapter for submitting and managing jobs on a cloud execution provider.
/// </summary>
public sealed class CloudExecutionAdapter
{
    private readonly string _apiKey;
    private readonly RemoteExecutionClient _client;

    /// <summary>
    /// Gets the cloud provider identifier.
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudExecutionAdapter"/> class.
    /// </summary>
    /// <param name="provider">The cloud provider identifier (e.g., "azure", "aws", "gcp").</param>
    /// <param name="apiKey">The API key for authentication.</param>
    public CloudExecutionAdapter(string provider, string apiKey)
    {
        _ = provider ?? throw new ArgumentNullException(nameof(provider));
        _ = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

        Provider = provider;
        _apiKey = apiKey;
        _client = new RemoteExecutionClient($"https://{provider}.cloud.compute");
    }

    /// <summary>
    /// Submits a job to the cloud provider for execution.
    /// </summary>
    /// <param name="job">The job to submit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the submitted job.</returns>
    public ValueTask<InteropResult<RemoteJob>> SubmitAsync(RemoteJob job, CancellationToken ct = default)
    {
        _ = job ?? throw new ArgumentNullException(nameof(job));
        return _client.SubmitJobAsync(job, ct);
    }

    /// <summary>
    /// Polls for the result of a previously submitted job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current result of the job.</returns>
    public ValueTask<RemoteResult> PollAsync(string jobId, CancellationToken ct = default)
    {
        _ = jobId ?? throw new ArgumentNullException(nameof(jobId));
        return _client.GetResultAsync(jobId, ct);
    }

    /// <summary>
    /// Cancels a running or pending job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> CancelAsync(string jobId, CancellationToken ct = default)
    {
        _ = jobId ?? throw new ArgumentNullException(nameof(jobId));
        return _client.CancelJobAsync(jobId, ct);
    }
}
