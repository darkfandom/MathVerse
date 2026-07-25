namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

/// <summary>
/// Represents the network topology of the cluster, tracking distances and connectivity between worker nodes.
/// Uses an adjacency list with Dijkstra's algorithm for shortest-path computation.
/// </summary>
public sealed class ClusterTopology
{
    private readonly ConcurrentDictionary<string, List<(string Neighbor, double Weight)>> _adjacency = new();
    private readonly ConcurrentDictionary<string, WorkerNode> _nodes = new();

    /// <summary>
    /// Initializes a new empty instance of the <see cref="ClusterTopology"/> class.
    /// </summary>
    public ClusterTopology() { }

    /// <summary>
    /// Builds the network topology from an array of worker nodes.
    /// Connects every pair of nodes with an edge whose weight is inversely proportional
    /// to the sum of their core counts (simulating network proximity).
    /// </summary>
    /// <param name="nodes">Array of worker nodes to include in the topology.</param>
    public void BuildTopology(WorkerNode[] nodes)
    {
        _adjacency.Clear();
        _nodes.Clear();

        foreach (var node in nodes)
        {
            _nodes[node.WorkerId] = node;
            _adjacency[node.WorkerId] = new List<(string, double)>();
        }

        var nodeArray = nodes;
        for (int i = 0; i < nodeArray.Length; i++)
        {
            for (int j = i + 1; j < nodeArray.Length; j++)
            {
                double combinedCores = nodeArray[i].CoreCount + nodeArray[j].CoreCount;
                double weight = combinedCores > 0 ? 1.0 / combinedCores : 1.0;

                _adjacency[nodeArray[i].WorkerId].Add((nodeArray[j].WorkerId, weight));
                _adjacency[nodeArray[j].WorkerId].Add((nodeArray[i].WorkerId, weight));
            }
        }
    }

    /// <summary>
    /// Finds the shortest path between two nodes using Dijkstra's algorithm.
    /// </summary>
    /// <param name="from">The source node ID.</param>
    /// <param name="to">The destination node ID.</param>
    /// <returns>An ordered list of node IDs representing the shortest path, or an empty list if no path exists.</returns>
    public IReadOnlyList<string> GetPath(string from, string to)
    {
        if (!_adjacency.ContainsKey(from) || !_adjacency.ContainsKey(to))
            return Array.Empty<string>();

        if (from == to)
            return new[] { from };

        var distances = new Dictionary<string, double>();
        var previous = new Dictionary<string, string?>();
        var unvisited = new HashSet<string>();

        foreach (var nodeId in _adjacency.Keys)
        {
            distances[nodeId] = double.MaxValue;
            previous[nodeId] = null;
            unvisited.Add(nodeId);
        }

        distances[from] = 0.0;

        while (unvisited.Count > 0)
        {
            string? current = null;
            double minDist = double.MaxValue;
            foreach (var nodeId in unvisited)
            {
                if (distances[nodeId] < minDist)
                {
                    minDist = distances[nodeId];
                    current = nodeId;
                }
            }

            if (current == null || current == to)
                break;

            unvisited.Remove(current);

            foreach (var (neighbor, weight) in _adjacency[current])
            {
                if (!unvisited.Contains(neighbor))
                    continue;

                double alt = distances[current] + weight;
                if (alt < distances[neighbor])
                {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                }
            }
        }

        if (previous[to] == null && from != to)
            return Array.Empty<string>();

        var path = new List<string>();
        string? cursor = to;
        while (cursor != null)
        {
            path.Add(cursor);
            previous.TryGetValue(cursor, out cursor);
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Returns the shortest-path distance between two nodes.
    /// </summary>
    /// <param name="from">The source node ID.</param>
    /// <param name="to">The destination node ID.</param>
    /// <returns>The shortest-path distance, or <see cref="double.MaxValue"/> if unreachable.</returns>
    public double GetDistance(string from, string to)
    {
        if (from == to)
            return 0.0;

        if (!_adjacency.ContainsKey(from) || !_adjacency.ContainsKey(to))
            return double.MaxValue;

        var distances = new Dictionary<string, double>();
        var unvisited = new HashSet<string>();

        foreach (var nodeId in _adjacency.Keys)
        {
            distances[nodeId] = double.MaxValue;
            unvisited.Add(nodeId);
        }

        distances[from] = 0.0;

        while (unvisited.Count > 0)
        {
            string? current = null;
            double minDist = double.MaxValue;
            foreach (var nodeId in unvisited)
            {
                if (distances[nodeId] < minDist)
                {
                    minDist = distances[nodeId];
                    current = nodeId;
                }
            }

            if (current == null || current == to)
                break;

            unvisited.Remove(current);

            foreach (var (neighbor, weight) in _adjacency[current])
            {
                if (!unvisited.Contains(neighbor))
                    continue;

                double alt = distances[current] + weight;
                if (alt < distances[neighbor])
                    distances[neighbor] = alt;
            }
        }

        return distances[to];
    }

    /// <summary>
    /// Returns the directly connected neighbors of a given node.
    /// </summary>
    /// <param name="nodeId">The node ID whose neighbors to retrieve.</param>
    /// <returns>A list of neighbor node IDs, or an empty list if the node is not in the topology.</returns>
    public IReadOnlyList<string> GetNeighbors(string nodeId)
    {
        if (!_adjacency.TryGetValue(nodeId, out var edges))
            return Array.Empty<string>();

        var neighbors = new string[edges.Count];
        for (int i = 0; i < edges.Count; i++)
        {
            neighbors[i] = edges[i].Neighbor;
        }
        return neighbors;
    }

    /// <summary>
    /// Gets all node IDs currently tracked in the topology.
    /// </summary>
    /// <returns>An array of all node IDs.</returns>
    public IReadOnlyCollection<string> GetAllNodeIds()
    {
        return _adjacency.Keys.ToArray();
    }
}
