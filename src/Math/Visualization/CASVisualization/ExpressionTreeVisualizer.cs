namespace MathVerse.Math.Visualization.CASVisualization;

/// <summary>Visualizes mathematical expression trees as a node-graph layout using Reingold-Tilford style positioning.</summary>
public sealed class ExpressionTreeVisualizer
{
    private const double HorizontalSpacing = 2.0;
    private const double VerticalSpacing = 1.5;

    /// <summary>Parses a mathematical expression string and visualizes it as an expression tree.</summary>
    /// <param name="expression">The expression string (e.g., "sin(x) + 2 * y").</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> containing the positioned nodes and edges.</returns>
    public ExpressionTreeResult Visualize(string expression)
    {
        var tokens = Tokenize(expression);
        var parser = new ExpressionParser(tokens);
        var root = parser.Parse();

        var result = new ExpressionTreeResult();
        if (root == null) return result;

        var nodes = new List<ExpressionTreeNode>();
        var edges = new List<ExpressionTreeEdge>();

        LayoutTree(root, nodes, edges);

        int maxX = nodes.Count > 0 ? (int)nodes.Max(n => n.X) + 1 : 0;
        int maxY = nodes.Count > 0 ? (int)nodes.Max(n => n.Y) + 1 : 0;

        return new ExpressionTreeResult
        {
            Nodes = nodes,
            Edges = edges,
            Width = maxX,
            Height = maxY
        };
    }

    private void LayoutTree(AstNode node, List<ExpressionTreeNode> nodes, List<ExpressionTreeEdge> edges)
    {
        var subtreeWidths = new Dictionary<int, int>();
        ComputeWidths(node, subtreeWidths);

        AssignPositions(node, 0, 0, subtreeWidths, nodes, edges);
    }

    private int ComputeWidths(AstNode node, Dictionary<int, int> widths)
    {
        if (node == null) return 0;
        if (node.Children.Count == 0)
        {
            widths[node.Id] = 1;
            return 1;
        }

        int total = 0;
        foreach (var child in node.Children)
            total += ComputeWidths(child!, widths);

        widths[node.Id] = total;
        return total;
    }

    private void AssignPositions(AstNode node, double x, double y,
        Dictionary<int, int> widths, List<ExpressionTreeNode> nodes, List<ExpressionTreeEdge> edges)
    {
        if (node == null) return;

        int nodeWidth = widths.ContainsKey(node.Id) ? widths[node.Id] : 1;
        double nodeX = x + (nodeWidth - 1) * HorizontalSpacing * 0.5;
        double nodeY = y;

        string color = node.Type switch
        {
            "operator" => "#E74C3C",
            "function" => "#9B59B6",
            "number" => "#2ECC71",
            "variable" => "#3498DB",
            _ => "#007ACC"
        };

        nodes.Add(new ExpressionTreeNode
        {
            Id = node.Id,
            Label = node.Label,
            NodeType = node.Type,
            X = nodeX,
            Y = nodeY,
            Color = color
        });

        double childX = x;
        foreach (var child in node.Children)
        {
            if (child == null) continue;

            int childWidth = widths.ContainsKey(child.Id) ? widths[child.Id] : 1;

            edges.Add(new ExpressionTreeEdge
            {
                FromNodeId = node.Id,
                ToNodeId = child.Id,
                Label = ""
            });

            AssignPositions(child, childX, y + VerticalSpacing, widths, nodes, edges);
            childX += childWidth * HorizontalSpacing;
        }
    }

    private static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < expression.Length)
        {
            char c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
            {
                int start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    i++;
                tokens.Add(new Token(TokenType.Number, expression[start..i]));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                    i++;
                string name = expression[start..i];

                if (name == "sin" || name == "cos" || name == "tan" || name == "log" || name == "exp" ||
                    name == "asin" || name == "acos" || name == "atan" || name == "sqrt" || name == "abs")
                    tokens.Add(new Token(TokenType.Function, name));
                else
                    tokens.Add(new Token(TokenType.Variable, name));
                continue;
            }

            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
            {
                tokens.Add(new Token(TokenType.Operator, c.ToString()));
                i++;
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new Token(TokenType.LParen, "("));
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new Token(TokenType.RParen, ")"));
                i++;
                continue;
            }

            if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ","));
                i++;
                continue;
            }

            i++;
        }

        return tokens;
    }

    private sealed class ExpressionParser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private int _nextId;

        public ExpressionParser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
            _nextId = 0;
        }

        private Token? Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private Token? Advance()
        {
            var tok = Current;
            _pos++;
            return tok;
        }

        private bool Match(TokenType type)
        {
            if (Current?.Type == type)
            {
                _pos++;
                return true;
            }
            return false;
        }

        public AstNode? Parse()
        {
            if (Current == null) return null;
            var node = ParseAddSub();
            return node;
        }

        private AstNode? ParseAddSub()
        {
            var left = ParseMulDiv();
            while (Current?.Type == TokenType.Operator && (Current.Value == "+" || Current.Value == "-"))
            {
                string op = Advance()!.Value;
                var right = ParseMulDiv();
                var id = _nextId++;
                left = new AstNode(id, op, "operator", [left, right]);
            }
            return left;
        }

        private AstNode? ParseMulDiv()
        {
            var left = ParsePower();
            while (Current?.Type == TokenType.Operator && (Current.Value == "*" || Current.Value == "/"))
            {
                string op = Advance()!.Value;
                var right = ParsePower();
                var id = _nextId++;
                left = new AstNode(id, op, "operator", [left, right]);
            }
            return left;
        }

        private AstNode? ParsePower()
        {
            var left = ParseUnary();
            if (Current?.Type == TokenType.Operator && Current.Value == "^")
            {
                Advance();
                var right = ParsePower(); // right-associative
                var id = _nextId++;
                left = new AstNode(id, "^", "operator", [left, right]);
            }
            return left;
        }

        private AstNode? ParseUnary()
        {
            if (Current?.Type == TokenType.Operator && Current.Value == "-")
            {
                Advance();
                var operand = ParsePrimary();
                var id = _nextId++;
                var negOne = new AstNode(_nextId++, "-1", "number", []);
                return new AstNode(id, "*", "operator", [negOne, operand]);
            }
            if (Current?.Type == TokenType.Operator && Current.Value == "+")
            {
                Advance();
                return ParsePrimary();
            }
            return ParsePrimary();
        }

        private AstNode? ParsePrimary()
        {
            if (Current == null) return null;

            if (Current.Type == TokenType.Number)
            {
                var tok = Advance()!;
                return new AstNode(_nextId++, tok.Value, "number", []);
            }

            if (Current.Type == TokenType.Variable)
            {
                var tok = Advance()!;
                return new AstNode(_nextId++, tok.Value, "variable", []);
            }

            if (Current.Type == TokenType.Function)
            {
                string funcName = Advance()!.Value;
                var args = new List<AstNode?>();

                if (Match(TokenType.LParen))
                {
                    args.Add(ParseAddSub());
                    while (Current?.Type == TokenType.Comma)
                    {
                        Advance();
                        args.Add(ParseAddSub());
                    }
                    Match(TokenType.RParen);
                }

                var id = _nextId++;
                return new AstNode(id, funcName, "function", args!);
            }

            if (Current.Type == TokenType.LParen)
            {
                Advance();
                var expr = ParseAddSub();
                Match(TokenType.RParen);
                return expr;
            }

            Advance(); // skip unexpected token
            return null;
        }
    }

    private sealed class AstNode
    {
        public int Id { get; }
        public string Label { get; }
        public string Type { get; }
        public List<AstNode?> Children { get; }

        public AstNode(int id, string label, string type, List<AstNode?> children)
        {
            Id = id;
            Label = label;
            Type = type;
            Children = children;
        }
    }

    private sealed record Token(TokenType Type, string Value);

    private enum TokenType
    {
        Number,
        Variable,
        Operator,
        Function,
        LParen,
        RParen,
        Comma
    }
}
