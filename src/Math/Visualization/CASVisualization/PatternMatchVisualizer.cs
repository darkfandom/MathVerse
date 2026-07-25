namespace MathVerse.Math.Visualization.CASVisualization;

/// <summary>Visualizes pattern matching results on expressions by highlighting matched regions.</summary>
public sealed class PatternMatchVisualizer
{
    private const double CharWidth = 0.6;
    private const double RowHeight = 1.5;

    /// <summary>Visualizes an expression with highlighted pattern matches, showing matched regions and their labels.</summary>
    /// <param name="expression">The expression string to visualize.</param>
    /// <param name="matches">A list of (start index, end index, pattern name) tuples identifying matched regions.</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> with nodes for each token and highlighted match regions.</returns>
    public ExpressionTreeResult Visualize(string expression, List<(int Start, int End, string PatternName)> matches)
    {
        var result = new ExpressionTreeResult();
        int nodeId = 0;

        // Create character-level nodes for the expression
        for (int i = 0; i < expression.Length; i++)
        {
            string ch = expression[i].ToString();

            string matchedColor = "#007ACC";
            string matchedPattern = "";
            bool isMatched = false;

            foreach (var (start, end, patternName) in matches)
            {
                if (i >= start && i < end)
                {
                    isMatched = true;
                    matchedPattern = patternName;
                    matchedColor = GetPatternColor(patternName);
                    break;
                }
            }

            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = ch,
                NodeType = isMatched ? "matched" : "character",
                X = i * CharWidth,
                Y = 0,
                Color = matchedColor
            });

            nodeId++;
        }

        // Create pattern highlight nodes above matched regions
        var processedRanges = new HashSet<string>();
        foreach (var (start, end, patternName) in matches)
        {
            string rangeKey = $"{start}:{end}";
            if (!processedRanges.Add(rangeKey)) continue;

            double midX = (start + end - 1) * CharWidth * 0.5;
            double width = (end - start) * CharWidth;

            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = patternName,
                NodeType = "pattern-label",
                X = midX,
                Y = -RowHeight,
                Color = GetPatternColor(patternName)
            });

            int labelNodeId = nodeId;
            nodeId++;

            // Connect label to each character in the match
            for (int i = start; i < end; i++)
            {
                result.Edges.Add(new ExpressionTreeEdge
                {
                    FromNodeId = labelNodeId,
                    ToNodeId = i,
                    Label = ""
                });
            }
        }

        // Create a baseline node showing the full expression
        result.Nodes.Add(new ExpressionTreeNode
        {
            Id = nodeId,
            Label = "Expression",
            NodeType = "meta",
            X = expression.Length * CharWidth * 0.5,
            Y = RowHeight,
            Color = "#7F8C8D"
        });

        result.Width = (int)(expression.Length * CharWidth + 2);
        result.Height = (int)(RowHeight * 3);
        return result;
    }

    private static string GetPatternColor(string patternName)
    {
        int hash = 0;
        foreach (char c in patternName)
            hash = (hash * 31) + c;

        string[] palette = ["#E74C3C", "#9B59B6", "#2ECC71", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4", "#8BC34A"];
        return palette[System.Math.Abs(hash) % palette.Length];
    }
}
