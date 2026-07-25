namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MathVerse.Math.Distributed.DistributedComputing;

/// <summary>
/// Simulates node discovery via broadcast for single-machine and test environments.
/// In production, replace the simulated broadcast with actual UDP multicast or gossip protocol.
/// </summary>
public sealed class NodeDiscovery
{
    private readonly ConcurrentDictionary<string, WorkerNode> _discoveredNodes = new();
    private readonly ConcurrentDictionary<string, DateTime> _advertisedTimestamps = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeDiscovery"/> class.
    /// </summary>
    public NodeDiscovery() { }

    /// <summary>
    /// Simulates discovering nodes by waiting for the specified timeout and collecting all advertised nodes.
    /// </summary>
    /// <param name="timeout">Maximum duration to listen for broadcast advertisements.</param>
    /// <returns>A list of all worker nodes discovered during the timeout period.</returns>
    public async ValueTask<IReadOnlyList<WorkerNode>> DiscoverNodes(TimeSpan timeout)
    {
        await Task.Delay(timeout).ConfigureAwait(false);
        return _discoveredNodes.Values.ToArray();
    }

    /// <summary>
    /// Advertises a node so that other nodes can discover it during discovery.
    /// </summary>
    /// <param name="node">The worker node to advertise.</param>
    public void AdvertiseNode(WorkerNode node)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));

        _discoveredNodes[node.WorkerId] = node;
        _advertisedTimestamps[node.WorkerId] = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a previously advertised node from the discovery registry.
    /// </summary>
    /// <param name="workerId">The ID of the worker to remove.</param>
    /// <returns>True if the node was found and removed; otherwise, false.</returns>
    public bool RemoveNode(string workerId)
    {
        _advertisedTimestamps.TryRemove(workerId, out _);
        return _discoveredNodes.TryRemove(workerId, out _);
    }

    /// <summary>
    /// Gets all currently discovered worker nodes.
    /// </summary>
    /// <returns>An array of all discovered worker nodes.</returns>
    public IReadOnlyList<WorkerNode> GetDiscoveredNodes()
    {
        return _discoveredNodes.Values.ToArray();
    }

    /// <summary>
    /// Gets the UTC timestamp when a node was last advertised.
    /// </summary>
    /// <param name="workerId">The worker node ID.</param>
    /// <param name="timestamp">The timestamp if found; otherwise, <see cref="DateTime.MinValue"/>.</param>
    /// <returns>True if the node has been advertised; otherwise, false.</returns>
    public bool TryGetAdvertisementTime(string workerId, out DateTime timestamp)
    {
        if (_advertisedTimestamps.TryGetValue(workerId, out var ts))
        {
            timestamp = ts;
            return true;
        }
        timestamp = DateTime.MinValue;
        return false;
    }
}
