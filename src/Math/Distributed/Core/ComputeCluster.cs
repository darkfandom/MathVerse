namespace MathVerse.Math.Distributed.Core;

using System.Collections.Concurrent;

/// <summary>Represents a cluster of compute nodes for distributed execution.</summary>
public sealed class ComputeCluster
{
    private readonly ConcurrentBag<ComputeNode> _nodes;

    /// <summary>Unique identifier for this cluster.</summary>
    public Guid ClusterId { get; init; } = Guid.NewGuid();

    /// <summary>Human-readable name for this cluster.</summary>
    public string Name { get; init; } = "DefaultCluster";

    /// <summary>Gets the collection of nodes in this cluster.</summary>
    public IReadOnlyCollection<ComputeNode> Nodes => _nodes.ToArray();

    /// <summary>Initializes a new compute cluster.</summary>
    public ComputeCluster()
    {
        _nodes = new ConcurrentBag<ComputeNode>();
    }

    /// <summary>Initializes a new compute cluster with a local node.</summary>
    /// <param name="localNode">The local node to add.</param>
    public ComputeCluster(ComputeNode localNode) : this()
    {
        if (localNode == null)
        {
            throw new ArgumentNullException(nameof(localNode));
        }
        _nodes.Add(localNode);
    }

    /// <summary>Adds a compute node to the cluster.</summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(ComputeNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        _nodes.Add(node);
    }

    /// <summary>Removes a compute node from the cluster by ID.</summary>
    /// <param name="nodeId">The ID of the node to remove.</param>
    /// <returns>True if the node was found and removed.</returns>
    public bool RemoveNode(string nodeId)
    {
        var remaining = new ConcurrentBag<ComputeNode>();
        bool found = false;

        foreach (var node in _nodes)
        {
            if (node.NodeId == nodeId)
            {
                found = true;
            }
            else
            {
                remaining.Add(node);
            }
        }

        if (found)
        {
            while (_nodes.TryTake(out _)) { }
            foreach (var node in remaining)
            {
                _nodes.Add(node);
            }
        }

        return found;
    }

    /// <summary>Gets all nodes that are available for work.</summary>
    /// <returns>Collection of available nodes.</returns>
    public IReadOnlyList<ComputeNode> GetAvailableNodes()
    {
        return _nodes.Where(n => n.IsAvailable).ToList();
    }

    /// <summary>Gets the load distribution across all active nodes.</summary>
    /// <returns>Dictionary mapping node IDs to their load scores.</returns>
    public IReadOnlyDictionary<string, double> GetLoadBalance()
    {
        var balance = new Dictionary<string, double>();
        foreach (var node in _nodes)
        {
            balance[node.NodeId] = node.GetLoadScore();
        }
        return balance;
    }

    /// <summary>Selects the best available node for task assignment.</summary>
    /// <param name="requiredCapability">Optional capability the node must have.</param>
    /// <returns>The best node, or null if none are available.</returns>
    public ComputeNode? SelectBestNode(string? requiredCapability = null)
    {
        ComputeNode? best = null;
        double bestScore = double.MaxValue;

        foreach (var node in _nodes)
        {
            if (!node.IsAvailable)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(requiredCapability) && !node.HasCapability(requiredCapability))
            {
                continue;
            }

            double score = node.GetLoadScore();
            if (score < bestScore)
            {
                bestScore = score;
                best = node;
            }
        }

        return best;
    }

    /// <summary>Gets the total number of nodes in the cluster.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Gets the number of active nodes in the cluster.</summary>
    public int ActiveNodeCount
    {
        get
        {
            int count = 0;
            foreach (var node in _nodes)
            {
                if (node.IsAvailable)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>Gets the total core count across all available nodes.</summary>
    public int TotalCores
    {
        get
        {
            int total = 0;
            foreach (var node in _nodes)
            {
                if (node.IsAvailable)
                {
                    total += node.CoreCount;
                }
            }
            return total;
        }
    }
}
