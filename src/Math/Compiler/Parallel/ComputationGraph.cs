namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A directed acyclic graph of computation nodes with dependency tracking.
/// Used for parallel scheduling and execution of independent computation branches.
/// </summary>
public sealed class ComputationGraph
{
    private readonly Dictionary<string, GraphNode> _nodes = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets all nodes in the graph.
    /// </summary>
    public IReadOnlyList<GraphNode> Nodes
    {
        get
        {
            lock (_lock)
                return _nodes.Values.ToList();
        }
    }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    public int NodeCount
    {
        get
        {
            lock (_lock)
                return _nodes.Count;
        }
    }

    /// <summary>
    /// Gets the node with the specified ID.
    /// </summary>
    /// <param name="id">The node ID to look up.</param>
    /// <returns>The node, or null if not found.</returns>
    public GraphNode? GetNode(string id)
    {
        lock (_lock)
        {
            _nodes.TryGetValue(id, out var node);
            return node;
        }
    }

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(GraphNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        lock (_lock)
        {
            _nodes[node.Id] = node;
        }
    }

    /// <summary>
    /// Removes a node from the graph.
    /// </summary>
    /// <param name="id">The ID of the node to remove.</param>
    /// <returns>True if the node was found and removed.</returns>
    public bool RemoveNode(string id)
    {
        lock (_lock)
            return _nodes.Remove(id);
    }

    /// <summary>
    /// Adds a dependency edge from one node to another.
    /// </summary>
    /// <param name="nodeId">The dependent node ID.</param>
    /// <param name="dependencyId">The dependency node ID.</param>
    public void AddDependency(string nodeId, string dependencyId)
    {
        lock (_lock)
        {
            if (_nodes.TryGetValue(nodeId, out var node))
            {
                if (!node.Dependencies.Contains(dependencyId))
                    node.Dependencies.Add(dependencyId);
            }
        }
    }

    /// <summary>
    /// Gets all nodes that have no dependencies (root nodes).
    /// </summary>
    public IReadOnlyList<GraphNode> GetRootNodes()
    {
        lock (_lock)
            return _nodes.Values.Where(n => n.Dependencies.Count == 0).ToList();
    }

    /// <summary>
    /// Gets all nodes that depend on the specified node.
    /// </summary>
    /// <param name="nodeId">The node ID to find dependents for.</param>
    public IReadOnlyList<GraphNode> GetDependents(string nodeId)
    {
        lock (_lock)
            return _nodes.Values.Where(n => n.Dependencies.Contains(nodeId)).ToList();
    }

    /// <summary>
    /// Checks whether the graph contains any cycles.
    /// </summary>
    public bool HasCycles()
    {
        lock (_lock)
        {
            var visited = new HashSet<string>();
            var inStack = new HashSet<string>();

            foreach (var node in _nodes.Values)
            {
                if (!visited.Contains(node.Id))
                {
                    if (DetectCycleDfs(node.Id, visited, inStack))
                        return true;
                }
            }
            return false;
        }
    }

    private bool DetectCycleDfs(string nodeId, HashSet<string> visited, HashSet<string> inStack)
    {
        if (!visited.Add(nodeId))
        {
            return inStack.Contains(nodeId);
        }

        inStack.Add(nodeId);

        if (_nodes.TryGetValue(nodeId, out var node))
        {
            foreach (var dep in node.Dependencies)
            {
                if (DetectCycleDfs(dep, visited, inStack))
                    return true;
            }
        }

        inStack.Remove(nodeId);
        return false;
    }
}

/// <summary>
/// Represents a single computation node in the graph.
/// </summary>
public sealed class GraphNode
{
    /// <summary>Unique identifier for this node.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>IDs of nodes that this node depends on.</summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// The computation function. Receives input values from dependencies
    /// and returns the computed result.
    /// </summary>
    public Func<List<double>, double> Execute { get; set; } = _ => 0.0;

    /// <summary>
    /// Optional metadata for scheduling hints.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
