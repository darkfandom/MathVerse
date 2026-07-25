namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Executes distributed tasks across a cluster using the master-worker pattern.
/// The master selects a worker, dispatches the task, and awaits completion.
/// </summary>
public sealed class DistributedExecutor
{
    private readonly ClusterManager _clusterManager;
    private readonly JobScheduler _jobScheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedExecutor"/> class.
    /// </summary>
    /// <param name="clusterManager">The cluster manager for worker selection.</param>
    /// <param name="jobScheduler">The job scheduler for task dispatch.</param>
    public DistributedExecutor(ClusterManager clusterManager, JobScheduler jobScheduler)
    {
        _clusterManager = clusterManager ?? throw new ArgumentNullException(nameof(clusterManager));
        _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
    }

    /// <summary>
    /// Executes a single distributed task across the cluster.
    /// Selects the best available worker, creates a remote execution context,
    /// dispatches the job, and returns the result.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The computation result as a double array.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no suitable worker is available.</exception>
    public async ValueTask<double[]> Execute(DistributedTask task, CancellationToken ct = default)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var worker = _clusterManager.SelectWorker(task);
        if (worker == null)
            throw new InvalidOperationException("No suitable worker available in the cluster.");

        var context = new RemoteExecutionContext(
            task.TaskId,
            "master",
            worker.WorkerId,
            DateTime.UtcNow,
            DateTime.UtcNow.Add(task.Timeout));

        _jobScheduler.SubmitJob(task);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(task.Timeout);

        try
        {
            var result = await task.Execute(timeoutCts.Token).ConfigureAwait(false);
            _jobScheduler.ReportCompletion(task.TaskId, result);
            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _jobScheduler.ReportFailure(task.TaskId, "Task cancelled by caller.");
            throw;
        }
        catch (OperationCanceledException)
        {
            _jobScheduler.ReportFailure(task.TaskId, "Task timed out.");
            throw new TimeoutException(
                $"Task '{task.TaskId}' exceeded its timeout of {task.Timeout.TotalSeconds} seconds.");
        }
        catch (Exception ex)
        {
            int retryCount = task.MaxRetries;
            if (retryCount > 0)
            {
                var retryTask = new DistributedTask(
                    task.TaskId,
                    task.Priority,
                    task.Execute,
                    task.Dependencies,
                    task.EstimatedDuration,
                    retryCount - 1,
                    task.Timeout);
                return await Execute(retryTask, ct).ConfigureAwait(false);
            }

            _jobScheduler.ReportFailure(task.TaskId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Executes multiple distributed tasks across the cluster, distributing them across available workers.
    /// Tasks are dispatched concurrently and results are collected in order.
    /// </summary>
    /// <param name="tasks">The array of tasks to execute.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>An array of results corresponding to each input task.</returns>
    public async ValueTask<double[][]> ExecuteAll(DistributedTask[] tasks, CancellationToken ct = default)
    {
        if (tasks == null) throw new ArgumentNullException(nameof(tasks));

        var results = new double[tasks.Length][];
        var runningTasks = new ValueTask<double[]>[tasks.Length];

        for (int i = 0; i < tasks.Length; i++)
        {
            runningTasks[i] = Execute(tasks[i], ct);
        }

        for (int i = 0; i < tasks.Length; i++)
        {
            results[i] = await runningTasks[i].ConfigureAwait(false);
        }

        return results;
    }
}
