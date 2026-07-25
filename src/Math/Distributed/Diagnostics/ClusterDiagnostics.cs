namespace MathVerse.Math.Distributed.Diagnostics;

using MathVerse.Math.Distributed.Core;

/// <summary>Tracks cluster health, node heartbeats, and load distribution.</summary>
public sealed class ClusterDiagnostics : IDisposable
{
    private readonly ComputeCluster? _cluster;
    private readonly Dictionary<string, DateTime> _lastHeartbeats;
    private readonly Dictionary<string, NodeStatus> _statusHistory;
    private readonly object _lock;
    private bool _disposed;

    /// <summary>Snapshot of cluster health status.</summary>
    public sealed class ClusterHealthSnapshot
    {
        /// <summary>Total number of nodes.</summary>
        public int TotalNodes { get; init; }

        /// <summary>Number of active nodes.</summary>
        public int ActiveNodes { get; init; }

        /// <summary>Number of idle nodes.</summary>
        public int IdleNodes { get; init; }

        /// <summary>Number of offline nodes.</summary>
        public int OfflineNodes { get; init; }

        /// <summary>Number of overloaded nodes.</summary>
        public int OverloadedNodes { get; init; }

        /// <summary>Average load across all active nodes.</summary>
        public double AverageLoad { get; init; }

        /// <summary>Load distribution: node ID to load score.</summary>
        public IReadOnlyDictionary<string, double> LoadDistribution { get; init; } = new Dictionary<string, double>();

        /// <summary>Node health details.</summary>
        public IReadOnlyList<NodeHealthInfo> NodeHealth { get; init; } = Array.Empty<NodeHealthInfo>();
    }

    /// <summary>Health information for a single node.</summary>
    public sealed class NodeHealthInfo
    {
        /// <summary>The node identifier.</summary>
        public string NodeId { get; init; } = "";

        /// <summary>The hostname.</summary>
        public string HostName { get; init; } = "";

        /// <summary>Current status.</summary>
        public NodeStatus Status { get; init; }

        /// <summary>Current load.</summary>
        public double Load { get; init; }

        /// <summary>Time since last heartbeat.</summary>
        public TimeSpan LastHeartbeatAge { get; init; }

        /// <summary>Whether the node is considered healthy.</summary>
        public bool IsHealthy { get; init; }

        /// <summary>Number of assigned tasks.</summary>
        public int AssignedTasks { get; init; }
    }

    /// <summary>Initializes cluster diagnostics with an optional cluster reference.</summary>
    /// <param name="cluster">The compute cluster to monitor.</param>
    public ClusterDiagnostics(ComputeCluster? cluster = null)
    {
        _cluster = cluster;
        _lastHeartbeats = new Dictionary<string, DateTime>();
        _statusHistory = new Dictionary<string, NodeStatus>();
        _lock = new object();
    }

    /// <summary>Records a heartbeat from a node.</summary>
    /// <param name="nodeId">The node identifier.</param>
    public void RecordHeartbeat(string nodeId)
    {
        lock (_lock)
        {
            _lastHeartbeats[nodeId] = DateTime.UtcNow;
        }
    }

    /// <summary>Records a status change for a node.</summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <param name="status">The new status.</param>
    public void RecordStatusChange(string nodeId, NodeStatus status)
    {
        lock (_lock)
        {
            _statusHistory[nodeId] = status;
        }
    }

    /// <summary>Gets a comprehensive health snapshot of the cluster.</summary>
    /// <returns>Current cluster health status.</returns>
    public ClusterHealthSnapshot GetClusterHealth()
    {
        if (_cluster == null)
        {
            return new ClusterHealthSnapshot();
        }

        int totalNodes = _cluster.NodeCount;
        int activeNodes = 0;
        int idleNodes = 0;
        int offlineNodes = 0;
        int overloadedNodes = 0;
        double totalLoad = 0.0;
        var loadDistribution = new Dictionary<string, double>();
        var nodeHealth = new List<NodeHealthInfo>();

        foreach (var node in _cluster.Nodes)
        {
            switch (node.Status)
            {
                case NodeStatus.Active:
                    activeNodes++;
                    break;
                case NodeStatus.Idle:
                    idleNodes++;
                    break;
                case NodeStatus.Offline:
                    offlineNodes++;
                    break;
                case NodeStatus.Overloaded:
                    overloadedNodes++;
                    break;
            }

            totalLoad += node.CurrentLoad;
            loadDistribution[node.NodeId] = node.CurrentLoad;

            DateTime lastHb;
            TimeSpan hbAge;
            lock (_lock)
            {
                if (_lastHeartbeats.TryGetValue(node.NodeId, out lastHb))
                {
                    hbAge = DateTime.UtcNow - lastHb;
                }
                else
                {
                    hbAge = DateTime.UtcNow - node.LastHeartbeat;
                }
            }

            nodeHealth.Add(new NodeHealthInfo
            {
                NodeId = node.NodeId,
                HostName = node.HostName,
                Status = node.Status,
                Load = node.CurrentLoad,
                LastHeartbeatAge = hbAge,
                IsHealthy = node.Status != NodeStatus.Offline,
                AssignedTasks = node.AssignedTaskCount
            });
        }

        int activeCount = activeNodes + idleNodes;

        return new ClusterHealthSnapshot
        {
            TotalNodes = totalNodes,
            ActiveNodes = activeNodes,
            IdleNodes = idleNodes,
            OfflineNodes = offlineNodes,
            OverloadedNodes = overloadedNodes,
            AverageLoad = activeCount > 0 ? totalLoad / activeCount : 0.0,
            LoadDistribution = loadDistribution,
            NodeHealth = nodeHealth
        };
    }

    /// <summary>Checks if a node is considered healthy based on its last heartbeat.</summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <param name="maxAgeSeconds">Maximum heartbeat age in seconds before marking unhealthy.</param>
    /// <returns>True if the node is healthy.</returns>
    public bool IsNodeHealthy(string nodeId, int maxAgeSeconds = 15)
    {
        lock (_lock)
        {
            if (_lastHeartbeats.TryGetValue(nodeId, out var lastHb))
            {
                return (DateTime.UtcNow - lastHb).TotalSeconds <= maxAgeSeconds;
            }
        }

        // Fall back to checking the cluster directly
        if (_cluster != null)
        {
            foreach (var node in _cluster.Nodes)
            {
                if (node.NodeId == nodeId)
                {
                    return (DateTime.UtcNow - node.LastHeartbeat).TotalSeconds <= maxAgeSeconds;
                }
            }
        }

        return false;
    }

    /// <summary>Resets all diagnostic counters.</summary>
    public void Reset()
    {
        lock (_lock)
        {
            _lastHeartbeats.Clear();
            _statusHistory.Clear();
        }
    }

    /// <summary>Disposes the cluster diagnostics.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Reset();
            _disposed = true;
        }
    }
}
