namespace MathVerse.Math.Visualization.CASVisualization;

/// <summary>Visualizes calculus derivation chains showing successive differentiation or integration steps.</summary>
public sealed class DerivationTreeVisualizer
{
    private const double StepVerticalSpacing = 2.0;

    /// <summary>Visualizes a derivation chain from an original expression through successive rule applications.</summary>
    /// <param name="original">The original expression string.</param>
    /// <param name="steps">A list of (result expression, rule name) tuples representing each derivation step.</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> containing the derivation chain.</returns>
    public ExpressionTreeResult Visualize(string original, List<(string Result, string Rule)> steps)
    {
        var result = new ExpressionTreeResult();
        int nodeId = 0;

        // Original expression node
        result.Nodes.Add(new ExpressionTreeNode
        {
            Id = nodeId,
            Label = original,
            NodeType = "expression",
            X = 0,
            Y = 0,
            Color = "#2C3E50"
        });

        int prevNodeId = nodeId;
        nodeId++;

        for (int i = 0; i < steps.Count; i++)
        {
            var (resultExpr, rule) = steps[i];

            // Rule annotation node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = $"{rule}",
                NodeType = "rule",
                X = -1.5,
                Y = (i + 1) * StepVerticalSpacing - 0.3,
                Color = "#E67E22"
            });

            int ruleId = nodeId;
            nodeId++;

            // Result expression node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = resultExpr,
                NodeType = "expression",
                X = 0,
                Y = (i + 1) * StepVerticalSpacing,
                Color = i == steps.Count - 1 ? "#2ECC71" : "#3498DB"
            });

            int resultId = nodeId;
            nodeId++;

            // Edges: prev -> rule -> result
            result.Edges.Add(new ExpressionTreeEdge
            {
                FromNodeId = prevNodeId,
                ToNodeId = ruleId,
                Label = $"Step {i + 1}"
            });

            result.Edges.Add(new ExpressionTreeEdge
            {
                FromNodeId = ruleId,
                ToNodeId = resultId,
                Label = ""
            });

            prevNodeId = resultId;
        }

        result.Width = 4;
        result.Height = (int)((steps.Count + 1) * StepVerticalSpacing + 1);
        return result;
    }
}
