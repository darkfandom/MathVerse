namespace MathVerse.Math.Distributed.Core;

using MathVerse.Math.Distributed.Diagnostics;

/// <summary>Lazy service locator for all distributed subsystem services.</summary>
public sealed class ExecutionServices : IDisposable
{
    private readonly Lazy<ExecutionDiagnostics> _diagnostics;
    private readonly Lazy<DeadlockDetector> _deadlockDetector;
    private readonly Lazy<SchedulerDiagnostics> _schedulerDiagnostics;
    private readonly Lazy<ResourceDiagnostics> _resourceDiagnostics;
    private readonly Lazy<ClusterDiagnostics> _clusterDiagnostics;
    private bool _disposed;

    /// <summary>Initializes the service locator with lazy instances.</summary>
    /// <param name="cluster">The compute cluster for cluster diagnostics.</param>
    /// <param name="scheduler">The task scheduler for scheduler diagnostics.</param>
    public ExecutionServices(ComputeCluster? cluster = null, TaskScheduler? scheduler = null)
    {
        _diagnostics = new Lazy<ExecutionDiagnostics>(() => new ExecutionDiagnostics());
        _deadlockDetector = new Lazy<DeadlockDetector>(() => new DeadlockDetector());
        _schedulerDiagnostics = new Lazy<SchedulerDiagnostics>(() => new SchedulerDiagnostics(scheduler));
        _resourceDiagnostics = new Lazy<ResourceDiagnostics>(() => new ResourceDiagnostics());
        _clusterDiagnostics = new Lazy<ClusterDiagnostics>(() => new ClusterDiagnostics(cluster));
    }

    /// <summary>Execution event recording and summary service.</summary>
    public ExecutionDiagnostics Diagnostics => _diagnostics.Value;

    /// <summary>Deadlock detection for task dependency graphs.</summary>
    public DeadlockDetector DeadlockDetector => _deadlockDetector.Value;

    /// <summary>Scheduler performance diagnostics.</summary>
    public SchedulerDiagnostics SchedulerDiagnostics => _schedulerDiagnostics.Value;

    /// <summary>System resource monitoring service.</summary>
    public ResourceDiagnostics ResourceDiagnostics => _resourceDiagnostics.Value;

    /// <summary>Cluster health and node monitoring service.</summary>
    public ClusterDiagnostics ClusterDiagnostics => _clusterDiagnostics.Value;

    /// <summary>Disposes all initialized services.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_diagnostics.IsValueCreated)
            {
                _diagnostics.Value.Dispose();
            }
            if (_schedulerDiagnostics.IsValueCreated)
            {
                _schedulerDiagnostics.Value.Dispose();
            }
            if (_resourceDiagnostics.IsValueCreated)
            {
                _resourceDiagnostics.Value.Dispose();
            }
            if (_clusterDiagnostics.IsValueCreated)
            {
                _clusterDiagnostics.Value.Dispose();
            }
            _disposed = true;
        }
    }
}
