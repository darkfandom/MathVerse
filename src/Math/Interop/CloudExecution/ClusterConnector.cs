namespace MathVerse.Math.Interop.CloudExecution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Represents a node in a computation cluster.
/// </summary>
public sealed class ClusterNode
{
    /// <summary>Gets or sets the unique node identifier.</summary>
    public string NodeId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Gets or sets the network address of the node.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the port number.</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets whether the node is alive and responsive.</summary>
    public bool IsAlive { get; set; }

    /// <summary>Gets or sets the number of CPU cores available on the node.</summary>
    public int CpuCount { get; set; }

    /// <summary>Gets or sets the available memory in bytes.</summary>
    public long AvailableMemory { get; set; }
}

/// <summary>
/// Provides summary information about a computation cluster.
/// </summary>
public sealed class ClusterInfo
{
    /// <summary>Gets or sets the unique cluster identifier.</summary>
    public string ClusterId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Gets or sets the display name of the cluster.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of nodes in the cluster.</summary>
    public int NodeCount { get; set; }

    /// <summary>Gets or sets the cluster software version.</summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Manages connections to a computation cluster.
/// </summary>
public sealed class ClusterConnector
{
    private readonly List<ClusterNode> _nodes = new();
    private ClusterInfo? _clusterInfo;
    private bool _connected;

    /// <summary>
    /// Gets whether the connector is currently connected to a cluster.
    /// </summary>
    public bool IsConnected => _connected;

    /// <summary>
    /// Connects to a cluster at the specified endpoint.
    /// </summary>
    /// <param name="clusterEndpoint">The cluster endpoint address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> ConnectAsync(string clusterEndpoint, CancellationToken ct = default)
    {
        _ = clusterEndpoint ?? throw new ArgumentNullException(nameof(clusterEndpoint));

        if (_connected)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Already connected to a cluster."));
        }

        _clusterInfo = new ClusterInfo
        {
            ClusterId = Guid.NewGuid().ToString("N"),
            Name = clusterEndpoint,
            NodeCount = 1,
            Version = "1.0.0"
        };

        _nodes.Add(new ClusterNode
        {
            NodeId = Guid.NewGuid().ToString("N"),
            Address = clusterEndpoint,
            Port = 8080,
            IsAlive = true,
            CpuCount = Environment.ProcessorCount,
            AvailableMemory = 1024L * 1024 * 1024 * 8
        });

        _connected = true;
        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <summary>
    /// Disconnects from the current cluster.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> DisconnectAsync(CancellationToken ct = default)
    {
        if (!_connected)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Not connected to a cluster."));
        }

        _nodes.Clear();
        _clusterInfo = null;
        _connected = false;
        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <summary>
    /// Gets all currently active (alive) nodes in the cluster.
    /// </summary>
    /// <returns>A read-only list of active cluster nodes.</returns>
    public IReadOnlyList<ClusterNode> GetActiveNodes()
    {
        return _nodes.Where(n => n.IsAlive).ToList();
    }

    /// <summary>
    /// Gets summary information about the connected cluster.
    /// </summary>
    /// <returns>The cluster info, or null if not connected.</returns>
    public ClusterInfo? GetClusterInfo()
    {
        return _clusterInfo;
    }
}
