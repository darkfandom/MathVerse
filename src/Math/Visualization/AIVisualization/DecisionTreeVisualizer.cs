namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a node in the visualized decision tree.</summary>
public sealed record DecisionTreeNode
{
    /// <summary>Unique identifier of the node.</summary>
    public required int NodeId { get; init; }

    /// <summary>Feature name used for splitting at this node.</summary>
    public required string Feature { get; init; }

    /// <summary>Threshold value for the split.</summary>
    public required double Threshold { get; init; }

    /// <summary>Left child node id (-1 if leaf).</summary>
    public required int LeftChild { get; init; }

    /// <summary>Right child node id (-1 if leaf).</summary>
    public required int RightChild { get; init; }

    /// <summary>Class label if this is a leaf node, otherwise null.</summary>
    public int? ClassLabel { get; init; }

    /// <summary>X position for layout.</summary>
    public double X { get; init; }

    /// <summary>Y position for layout.</summary>
    public double Y { get; init; }
}

/// <summary>Represents a connection between two nodes in the tree.</summary>
public sealed record DecisionTreeEdge
{
    /// <summary>Source node id.</summary>
    public required int FromNodeId { get; init; }

    /// <summary>Target node id.</summary>
    public required int ToNodeId { get; init; }

    /// <summary>Whether this is the left branch (true) or right branch (false).</summary>
    public required bool IsLeft { get; init; }
}

/// <summary>Complete plot data for the decision tree visualization.</summary>
public sealed record DecisionTreePlotData
{
    /// <summary>All nodes in the tree.</summary>
    public required IReadOnlyList<DecisionTreeNode> Nodes { get; init; }

    /// <summary>All edges connecting nodes.</summary>
    public required IReadOnlyList<DecisionTreeEdge> Edges { get; init; }

    /// <summary>Depth of the tree.</summary>
    public required int TreeDepth { get; init; }

    /// <summary>Total number of nodes.</summary>
    public required int NodeCount { get; init; }
}

/// <summary>Visualizes decision tree structures as a hierarchical graph layout.</summary>
public sealed class DecisionTreeVisualizer
{
    /// <summary>
    /// Visualizes a decision tree from its node definitions, computing layout positions and edges.
    /// </summary>
    /// <param name="nodes">List of tree nodes with id, feature, threshold, children, and optional class label.</param>
    /// <returns>Complete plot data with positioned nodes and edges.</returns>
    public DecisionTreePlotData Visualize(
        List<(int nodeId, string feature, double threshold, int leftChild, int rightChild, int? classLabel)> nodes)
    {
        var nodeList = new List<DecisionTreeNode>();
        var edges = new List<DecisionTreeEdge>();
        int depth = 0;

        var nodeLookup = new Dictionary<int, (int leftChild, int rightChild)>();

        foreach (var (nodeId, feature, threshold, leftChild, rightChild, classLabel) in nodes)
        {
            nodeLookup[nodeId] = (leftChild, rightChild);
        }

        var levels = new Dictionary<int, List<int>>();
        ComputeDepthAndLevels(nodes, nodeLookup, 0, ref depth, levels);

        int maxDepth = depth;

        foreach (var (nodeId, feature, threshold, leftChild, rightChild, classLabel) in nodes)
        {
            int level = FindLevel(levels, nodeId);
            int nodesAtLevel = levels.ContainsKey(level) ? levels[level].Count : 1;
            int indexInLevel = levels.ContainsKey(level) ? levels[level].IndexOf(nodeId) : 0;

            double x = nodesAtLevel > 1
                ? (double)indexInLevel / (double)(nodesAtLevel - 1)
                : 0.5;

            double y = maxDepth > 0
                ? (double)level / (double)maxDepth
                : 0.0;

            nodeList.Add(new DecisionTreeNode
            {
                NodeId = nodeId,
                Feature = feature,
                Threshold = threshold,
                LeftChild = leftChild,
                RightChild = rightChild,
                ClassLabel = classLabel,
                X = x,
                Y = y
            });

            if (leftChild >= 0)
            {
                edges.Add(new DecisionTreeEdge
                {
                    FromNodeId = nodeId,
                    ToNodeId = leftChild,
                    IsLeft = true
                });
            }

            if (rightChild >= 0)
            {
                edges.Add(new DecisionTreeEdge
                {
                    FromNodeId = nodeId,
                    ToNodeId = rightChild,
                    IsLeft = false
                });
            }
        }

        return new DecisionTreePlotData
        {
            Nodes = nodeList,
            Edges = edges,
            TreeDepth = depth,
            NodeCount = nodes.Count
        };
    }

    private static void ComputeDepthAndLevels(
        List<(int nodeId, string feature, double threshold, int leftChild, int rightChild, int? classLabel)> nodes,
        Dictionary<int, (int leftChild, int rightChild)> nodeLookup,
        int currentDepth,
        ref int maxDepth,
        Dictionary<int, List<int>> levels)
    {
        if (nodes.Count == 0) return;

        var queue = new Queue<(int nodeId, int depth)>();
        queue.Enqueue((nodes[0].nodeId, 0));

        while (queue.Count > 0)
        {
            var (nodeId, d) = queue.Dequeue();

            if (d > maxDepth) maxDepth = d;

            if (!levels.ContainsKey(d))
                levels[d] = new List<int>();
            levels[d].Add(nodeId);

            if (nodeLookup.TryGetValue(nodeId, out var children))
            {
                if (children.leftChild >= 0)
                    queue.Enqueue((children.leftChild, d + 1));
                if (children.rightChild >= 0)
                    queue.Enqueue((children.rightChild, d + 1));
            }
        }
    }

    private static int FindLevel(Dictionary<int, List<int>> levels, int nodeId)
    {
        foreach (var kvp in levels)
        {
            if (kvp.Value.Contains(nodeId))
                return kvp.Key;
        }
        return 0;
    }
}
