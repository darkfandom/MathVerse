namespace MathVerse.Math.Visualization.Integration;
using System.Collections.Generic;

/// <summary>Represents a node in an expression tree.</summary>
public sealed class ExpressionNode
{
    /// <summary>Gets the node type.</summary>
    public ExpressionNodeType NodeType { get; init; }

    /// <summary>Gets the operator or function name.</summary>
    public string? Operator { get; init; }

    /// <summary>Gets the value for leaf nodes.</summary>
    public double? Value { get; init; }

    /// <summary>Gets the variable name for variable nodes.</summary>
    public string? VariableName { get; init; }

    /// <summary>Gets the child nodes.</summary>
    public List<ExpressionNode> Children { get; init; } = new();

    /// <summary>Gets the position for tree layout.</summary>
    public (double X, double Y) LayoutPosition { get; set; }
}

/// <summary>Defines types of expression tree nodes.</summary>
public enum ExpressionNodeType
{
    /// <summary>A numeric constant.</summary>
    Constant,

    /// <summary>A variable.</summary>
    Variable,

    /// <summary>A binary operator (+, -, *, /, ^).</summary>
    BinaryOperator,

    /// <summary>A unary operator (-, +).</summary>
    UnaryOperator,

    /// <summary>A function call (sin, cos, etc.).</summary>
    Function,

    /// <summary>A number with a base and exponent.</summary>
    Power
}

/// <summary>Integrates with CAS for expression tree visualization.</summary>
public sealed class CASIntegration
{
    /// <summary>Parses a mathematical expression string into an expression tree.</summary>
    /// <param name="expression">The expression to parse.</param>
    /// <returns>The root node of the expression tree.</returns>
    public static ExpressionNode ParseExpression(string expression)
    {
        var tokens = Tokenize(expression);
        int index = 0;
        return ParseAddSub(tokens, ref index);
    }

    /// <summary>Generates layout positions for rendering an expression tree.</summary>
    /// <param name="root">The root node of the expression tree.</param>
    /// <param name="nodeWidth">The width allocated per node.</param>
    /// <param name="nodeHeight">The height allocated per node.</param>
    /// <returns>The root node with layout positions assigned.</returns>
    public static ExpressionNode LayoutTree(ExpressionNode root, double nodeWidth = 60.0, double nodeHeight = 40.0)
    {
        if (root == null)
            return root!;

        var levels = new List<List<ExpressionNode>>();
        TraverseLevels(root, 0, levels);

        for (int level = 0; level < levels.Count; level++)
        {
            var nodesAtLevel = levels[level];
            double totalWidth = nodesAtLevel.Count * nodeWidth;
            double startX = -totalWidth / 2.0;

            for (int i = 0; i < nodesAtLevel.Count; i++)
            {
                nodesAtLevel[i].LayoutPosition = (startX + i * nodeWidth + nodeWidth / 2.0, level * nodeHeight);
            }
        }

        return root;
    }

    /// <summary>Converts an expression tree to visualization objects.</summary>
    /// <param name="root">The expression tree root.</param>
    /// <param name="nodeColor">The node color.</param>
    /// <param name="edgeColor">The edge color.</param>
    /// <returns>Visualization objects for nodes and edges.</returns>
    public static (List<Core.VisualizationObject> Nodes, List<(System.Numerics.Vector2 Start, System.Numerics.Vector2 End)> Edges)
        ToVisualization(ExpressionNode root, string nodeColor = "#4488CC", string edgeColor = "#666666")
    {
        var nodes = new List<Core.VisualizationObject>();
        var edges = new List<(System.Numerics.Vector2, System.Numerics.Vector2)>();

        if (root == null)
            return (nodes, edges);

        LayoutTree(root);

        AddNodeVisualization(root, nodes, nodeColor);
        AddEdgeVisualization(root, edges, edgeColor);

        return (nodes, edges);
    }

    /// <summary>Computes the depth of the expression tree.</summary>
    /// <param name="node">The root node.</param>
    /// <returns>The depth of the tree.</returns>
    public static int ComputeDepth(ExpressionNode node)
    {
        if (node == null || node.Children.Count == 0)
            return 0;

        int maxChildDepth = 0;
        foreach (var child in node.Children)
        {
            int childDepth = ComputeDepth(child);
            if (childDepth > maxChildDepth)
                maxChildDepth = childDepth;
        }

        return maxChildDepth + 1;
    }

    /// <summary>Computes the width (number of leaves) of the expression tree.</summary>
    /// <param name="node">The root node.</param>
    /// <returns>The width of the tree.</returns>
    public static int ComputeWidth(ExpressionNode node)
    {
        if (node == null)
            return 0;

        if (node.Children.Count == 0)
            return 1;

        int totalWidth = 0;
        foreach (var child in node.Children)
        {
            totalWidth += ComputeWidth(child);
        }

        return totalWidth;
    }

    /// <summary>Counts the total number of nodes in the tree.</summary>
    /// <param name="node">The root node.</param>
    /// <returns>The node count.</returns>
    public static int CountNodes(ExpressionNode node)
    {
        if (node == null)
            return 0;

        int count = 1;
        foreach (var child in node.Children)
        {
            count += CountNodes(child);
        }

        return count;
    }

    /// <summary>Converts an expression tree back to a string expression.</summary>
    /// <param name="node">The root node.</param>
    /// <returns>The expression string.</returns>
    public static string ToExpressionString(ExpressionNode node)
    {
        if (node == null)
            return "";

        switch (node.NodeType)
        {
            case ExpressionNodeType.Constant:
                return node.Value?.ToString("G6") ?? "0";

            case ExpressionNodeType.Variable:
                return node.VariableName ?? "x";

            case ExpressionNodeType.UnaryOperator:
                string childExpr = ToExpressionString(node.Children.Count > 0 ? node.Children[0] : null!);
                return $"({node.Operator}{childExpr})";

            case ExpressionNodeType.BinaryOperator:
                if (node.Children.Count >= 2)
                {
                    string left = ToExpressionString(node.Children[0]);
                    string right = ToExpressionString(node.Children[1]);
                    return $"({left} {node.Operator} {right})";
                }
                return "";

            case ExpressionNodeType.Function:
                if (node.Children.Count > 0)
                {
                    string arg = ToExpressionString(node.Children[0]);
                    return $"{node.Operator}({arg})";
                }
                return $"{node.Operator}()";

            default:
                return "";
        }
    }

    private static void AddNodeVisualization(ExpressionNode node, List<Core.VisualizationObject> nodes, string color)
    {
        nodes.Add(new Core.VisualizationObject
        {
            Id = "expr-node-" + System.Math.Abs(node.GetHashCode()).ToString(),
            Color = color,
            Position = new System.Numerics.Vector3((float)node.LayoutPosition.X, (float)node.LayoutPosition.Y, 0)
        });

        foreach (var child in node.Children)
        {
            AddNodeVisualization(child, nodes, color);
        }
    }

    private static void AddEdgeVisualization(ExpressionNode node, List<(System.Numerics.Vector2, System.Numerics.Vector2)> edges, string color)
    {
        var parentPos = new System.Numerics.Vector2((float)node.LayoutPosition.X, (float)node.LayoutPosition.Y);

        foreach (var child in node.Children)
        {
            var childPos = new System.Numerics.Vector2((float)child.LayoutPosition.X, (float)child.LayoutPosition.Y);
            edges.Add((parentPos, childPos));
            AddEdgeVisualization(child, edges, color);
        }
    }

    private static void TraverseLevels(ExpressionNode node, int level, List<List<ExpressionNode>> levels)
    {
        if (node == null)
            return;

        while (levels.Count <= level)
            levels.Add(new List<ExpressionNode>());

        levels[level].Add(node);

        foreach (var child in node.Children)
        {
            TraverseLevels(child, level + 1, levels);
        }
    }

    private static List<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < expression.Length)
        {
            if (char.IsWhiteSpace(expression[i]))
            {
                i++;
                continue;
            }

            if (char.IsDigit(expression[i]) || expression[i] == '.')
            {
                int start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    i++;
                tokens.Add(expression.Substring(start, i - start));
            }
            else if (char.IsLetter(expression[i]))
            {
                int start = i;
                while (i < expression.Length && char.IsLetterOrDigit(expression[i]))
                    i++;
                tokens.Add(expression.Substring(start, i - start));
            }
            else
            {
                tokens.Add(expression[i].ToString());
                i++;
            }
        }

        return tokens;
    }

    private static ExpressionNode ParseAddSub(List<string> tokens, ref int index)
    {
        var left = ParseMulDiv(tokens, ref index);

        while (index < tokens.Count && (tokens[index] == "+" || tokens[index] == "-"))
        {
            string op = tokens[index];
            index++;
            var right = ParseMulDiv(tokens, ref index);

            left = new ExpressionNode
            {
                NodeType = ExpressionNodeType.BinaryOperator,
                Operator = op,
                Children = new List<ExpressionNode> { left, right }
            };
        }

        return left;
    }

    private static ExpressionNode ParseMulDiv(List<string> tokens, ref int index)
    {
        var left = ParsePower(tokens, ref index);

        while (index < tokens.Count && (tokens[index] == "*" || tokens[index] == "/"))
        {
            string op = tokens[index];
            index++;
            var right = ParsePower(tokens, ref index);

            left = new ExpressionNode
            {
                NodeType = ExpressionNodeType.BinaryOperator,
                Operator = op,
                Children = new List<ExpressionNode> { left, right }
            };
        }

        return left;
    }

    private static ExpressionNode ParsePower(List<string> tokens, ref int index)
    {
        var left = ParseUnary(tokens, ref index);

        if (index < tokens.Count && tokens[index] == "^")
        {
            index++;
            var right = ParsePower(tokens, ref index);

            return new ExpressionNode
            {
                NodeType = ExpressionNodeType.Power,
                Operator = "^",
                Children = new List<ExpressionNode> { left, right }
            };
        }

        return left;
    }

    private static ExpressionNode ParseUnary(List<string> tokens, ref int index)
    {
        if (index < tokens.Count && (tokens[index] == "-" || tokens[index] == "+"))
        {
            string op = tokens[index];
            index++;
            var child = ParsePrimary(tokens, ref index);

            return new ExpressionNode
            {
                NodeType = ExpressionNodeType.UnaryOperator,
                Operator = op,
                Children = new List<ExpressionNode> { child }
            };
        }

        return ParsePrimary(tokens, ref index);
    }

    private static ExpressionNode ParsePrimary(List<string> tokens, ref int index)
    {
        if (index >= tokens.Count)
            return new ExpressionNode { NodeType = ExpressionNodeType.Constant, Value = 0 };

        string token = tokens[index];

        if (token == "(")
        {
            index++;
            var expr = ParseAddSub(tokens, ref index);

            if (index < tokens.Count && tokens[index] == ")")
                index++;

            return expr;
        }

        if (double.TryParse(token, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double value))
        {
            index++;
            return new ExpressionNode { NodeType = ExpressionNodeType.Constant, Value = value };
        }

        if (IsFunctionName(token))
        {
            string funcName = token;
            index++;

            if (index < tokens.Count && tokens[index] == "(")
            {
                index++;
                var arg = ParseAddSub(tokens, ref index);
                if (index < tokens.Count && tokens[index] == ")")
                    index++;

                return new ExpressionNode
                {
                    NodeType = ExpressionNodeType.Function,
                    Operator = funcName,
                    Children = new List<ExpressionNode> { arg }
                };
            }

            return new ExpressionNode
            {
                NodeType = ExpressionNodeType.Function,
                Operator = funcName
            };
        }

        index++;
        return new ExpressionNode { NodeType = ExpressionNodeType.Variable, VariableName = token };
    }

    private static bool IsFunctionName(string token)
    {
        string[] functions = { "sin", "cos", "tan", "asin", "acos", "atan", "sinh", "cosh", "tanh", "sqrt", "abs", "log", "log10", "exp", "floor", "ceil", "round" };
        foreach (var f in functions)
        {
            if (string.Equals(f, token, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
