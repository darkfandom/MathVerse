namespace MathVerse.Math.Distributed.IncrementalComputation;

using System.Collections.Concurrent;

/// <summary>Represents a single computation node in a dependency graph.</summary>
public sealed class ComputationNode
{
    /// <summary>Unique identifier for this node.</summary>
    public int Id { get; init; }

    /// <summary>The compute function. Accepts an input array and returns an output array.</summary>
    public Func<double[], double[]> Compute { get; init; } = static input => (double[])input.Clone();

    /// <summary>IDs of nodes that must execute before this node can execute.</summary>
    public List<int> Dependencies { get; init; } = new();

    /// <summary>Cached result from the most recent execution.</summary>
    public double[] CachedResult { get; set; } = Array.Empty<double>();

    /// <summary>Whether this node needs re-execution.</summary>
    public bool IsDirty { get; set; } = true;
}

/// <summary>Executes computation nodes in dependency order, only re-executing dirty nodes.</summary>
public sealed class DependencyGraphExecutor
{
    private readonly ConcurrentDictionary<int, ComputationNode> _nodes = new();
    private readonly object _executionLock = new();

    /// <summary>Gets the number of registered nodes.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Registers a computation node in the graph.</summary>
    /// <param name="node">The node to register.</param>
    public void RegisterNode(ComputationNode node)
    {
        _nodes[node.Id] = node;
    }

    /// <summary>Retrieves a node by its identifier.</summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <returns>The computation node.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the node does not exist.</exception>
    public ComputationNode GetNode(int nodeId)
    {
        return _nodes[nodeId];
    }

    /// <summary>Executes the specified node and all its dirty dependencies in topological order.</summary>
    /// <param name="nodeId">The ID of the node to execute.</param>
    /// <param name="input">The input data for the root node.</param>
    /// <returns>The computed result array.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the node does not exist.</exception>
    public double[] Execute(int nodeId, double[] input)
    {
        lock (_executionLock)
        {
            var sortedNodes = TopologicalSort(nodeId);

            foreach (var node in sortedNodes)
            {
                if (!node.IsDirty)
                {
                    continue;
                }

                double[] nodeInput;

                if (node.Id == nodeId)
                {
                    nodeInput = input;
                }
                else
                {
                    nodeInput = GatherDependencyResults(node);
                }

                node.CachedResult = node.Compute(nodeInput);
                node.IsDirty = false;
            }

            return _nodes[nodeId].CachedResult;
        }
    }

    /// <summary>Marks the specified node and all its downstream dependents as dirty.</summary>
    /// <param name="nodeId">The ID of the node to mark dirty.</param>
    public void MarkDirty(int nodeId)
    {
        lock (_executionLock)
        {
            if (_nodes.TryGetValue(nodeId, out var node))
            {
                node.IsDirty = true;
                PropagateDirty(nodeId);
            }
        }
    }

    /// <summary>Removes a node from the graph.</summary>
    /// <param name="nodeId">The ID of the node to remove.</param>
    /// <returns>True if the node was removed; false if it did not exist.</returns>
    public bool RemoveNode(int nodeId)
    {
        return _nodes.TryRemove(nodeId, out _);
    }

    /// <summary>Returns the number of nodes currently marked as dirty.</summary>
    /// <returns>The count of dirty nodes.</returns>
    public int GetDirtyCount()
    {
        int count = 0;
        foreach (var node in _nodes.Values)
        {
            if (node.IsDirty)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>Clears all nodes from the graph.</summary>
    public void Clear()
    {
        _nodes.Clear();
    }

    private void PropagateDirty(int nodeId)
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Dependencies.Contains(nodeId) && !node.IsDirty)
            {
                node.IsDirty = true;
                PropagateDirty(node.Id);
            }
        }
    }

    private List<ComputationNode> TopologicalSort(int targetNodeId)
    {
        var visited = new HashSet<int>();
        var result = new List<ComputationNode>();

        VisitNode(targetNodeId, visited, result);
        result.Reverse();
        return result;
    }

    private void VisitNode(int nodeId, HashSet<int> visited, List<ComputationNode> result)
    {
        if (!visited.Add(nodeId))
        {
            return;
        }

        if (!_nodes.TryGetValue(nodeId, out var node))
        {
            return;
        }

        foreach (var depId in node.Dependencies)
        {
            VisitNode(depId, visited, result);
        }

        result.Add(node);
    }

    private double[] GatherDependencyResults(ComputationNode node)
    {
        var results = new List<double[]>();
        foreach (var depId in node.Dependencies)
        {
            if (_nodes.TryGetValue(depId, out var depNode))
            {
                results.Add(depNode.CachedResult);
            }
        }

        if (results.Count == 0)
        {
            return Array.Empty<double>();
        }

        int totalLength = 0;
        foreach (var r in results)
        {
            totalLength += r.Length;
        }

        var combined = new double[totalLength];
        int offset = 0;
        foreach (var r in results)
        {
            Array.Copy(r, 0, combined, offset, r.Length);
            offset += r.Length;
        }

        return combined;
    }
}
