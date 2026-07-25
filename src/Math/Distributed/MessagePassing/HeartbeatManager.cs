namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Monitors the health of remote nodes by tracking heartbeat timestamps.
/// When a node fails to heartbeat within its expected interval, it is marked as down.
/// </summary>
public sealed class HeartbeatManager : IDisposable
{
    private readonly ConcurrentDictionary<string, Timer> _timers = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastHeartbeats = new();
    private readonly ConcurrentDictionary<string, bool> _aliveStatus = new();
    private Func<string, ValueTask>? _onNodeDown;
    private bool _disposed;

    /// <summary>
    /// Event raised when a node is detected as down due to a missed heartbeat.
    /// </summary>
    public event Func<string, ValueTask> OnNodeDown
    {
        add
        {
            _onNodeDown += value;
        }
        remove
        {
            _onNodeDown -= value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatManager"/> class.
    /// </summary>
    public HeartbeatManager() { }

    /// <summary>
    /// Starts monitoring a node at the specified heartbeat interval.
    /// If a heartbeat is not received within the interval, the node is considered down.
    /// </summary>
    /// <param name="nodeId">The node ID to monitor.</param>
    /// <param name="interval">The expected heartbeat interval.</param>
    public void StartMonitoring(string nodeId, TimeSpan interval)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(HeartbeatManager));
        if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));

        _lastHeartbeats[nodeId] = DateTime.UtcNow;
        _aliveStatus[nodeId] = true;

        if (_timers.TryGetValue(nodeId, out var existingTimer))
        {
            existingTimer.Dispose();
        }

        var timer = new Timer(
            state => CheckHeartbeat(nodeId, interval),
            null,
            interval,
            interval);

        _timers[nodeId] = timer;
    }

    /// <summary>
    /// Stops monitoring a node and removes its tracking state.
    /// </summary>
    /// <param name="nodeId">The node ID to stop monitoring.</param>
    public void StopMonitoring(string nodeId)
    {
        if (_timers.TryRemove(nodeId, out var timer))
        {
            timer.Dispose();
        }

        _lastHeartbeats.TryRemove(nodeId, out _);
        _aliveStatus.TryRemove(nodeId, out _);
    }

    /// <summary>
    /// Records a heartbeat from a node, resetting its alive timer.
    /// </summary>
    /// <param name="nodeId">The node ID that sent the heartbeat.</param>
    public void RecordHeartbeat(string nodeId)
    {
        _lastHeartbeats[nodeId] = DateTime.UtcNow;
        _aliveStatus[nodeId] = true;
    }

    /// <summary>
    /// Checks whether a node is currently considered alive.
    /// </summary>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if the node is alive; false if it is down or not being monitored.</returns>
    public bool IsAlive(string nodeId)
    {
        return _aliveStatus.TryGetValue(nodeId, out var alive) && alive;
    }

    /// <summary>
    /// Gets the UTC timestamp of the last heartbeat received from a node.
    /// </summary>
    /// <param name="nodeId">The node ID to query.</param>
    /// <param name="timestamp">The last heartbeat timestamp if found.</param>
    /// <returns>True if the node has been monitored; otherwise, false.</returns>
    public bool TryGetLastHeartbeat(string nodeId, out DateTime timestamp)
    {
        return _lastHeartbeats.TryGetValue(nodeId, out timestamp);
    }

    /// <summary>
    /// Checks whether a node's heartbeat has expired and fires the OnNodeDown event if so.
    /// </summary>
    private void CheckHeartbeat(string nodeId, TimeSpan interval)
    {
        if (!_lastHeartbeats.TryGetValue(nodeId, out var lastHeartbeat))
            return;

        var elapsed = DateTime.UtcNow - lastHeartbeat;
        if (elapsed > interval)
        {
            _aliveStatus[nodeId] = false;

            var handler = _onNodeDown;
            if (handler != null)
            {
                _ = handler.Invoke(nodeId);
            }
        }
    }

    /// <summary>
    /// Disposes all timers used by this heartbeat manager.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var kvp in _timers)
        {
            kvp.Value.Dispose();
        }

        _timers.Clear();
        _lastHeartbeats.Clear();
        _aliveStatus.Clear();
    }
}
