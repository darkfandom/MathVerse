namespace MathVerse.Math.Visualization.CASVisualization;

using System.Collections.Immutable;

/// <summary>Result of expression tree visualization.</summary>
public sealed class ExpressionTreeResult
{
    /// <summary>Gets the list of nodes in the expression tree.</summary>
    public List<ExpressionTreeNode> Nodes { get; init; } = [];

    /// <summary>Gets the list of edges connecting the nodes.</summary>
    public List<ExpressionTreeEdge> Edges { get; init; } = [];

    /// <summary>Gets or sets the width of the tree layout.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets the height of the tree layout.</summary>
    public int Height { get; set; }
}

/// <summary>A single node in an expression tree visualization.</summary>
public sealed class ExpressionTreeNode
{
    /// <summary>Gets the unique identifier for this node.</summary>
    public int Id { get; init; }

    /// <summary>Gets the display label for this node.</summary>
    public string Label { get; init; } = "";

    /// <summary>Gets the type of this node (operator, number, variable, function).</summary>
    public string NodeType { get; init; } = "";

    /// <summary>Gets the X position for layout.</summary>
    public double X { get; init; }

    /// <summary>Gets the Y position for layout.</summary>
    public double Y { get; init; }

    /// <summary>Gets the color for this node.</summary>
    public string Color { get; init; } = "#007ACC";
}

/// <summary>An edge connecting two nodes in the expression tree.</summary>
public sealed class ExpressionTreeEdge
{
    /// <summary>Gets the source node identifier.</summary>
    public int FromNodeId { get; init; }

    /// <summary>Gets the target node identifier.</summary>
    public int ToNodeId { get; init; }

    /// <summary>Gets the label for this edge.</summary>
    public string Label { get; init; } = "";
}
