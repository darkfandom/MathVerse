namespace MathVerse.Math.Parsing.Conversion;

/// <summary>
/// Converts syntax tree nodes from the parsing layer into canonical
/// <see cref="Expression"/> types from the kernel expression tree.
/// </summary>
public sealed class SyntaxToExpressionConverter
{
    /// <summary>
    /// Converts a syntax tree root to an <see cref="Expression"/>.
    /// </summary>
    /// <param name="tree">The syntax tree to convert.</param>
    /// <returns>The converted expression.</returns>
    public Expression ConvertSyntaxTree(SyntaxTree tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        return Convert(tree.Root);
    }

    /// <summary>
    /// Converts a single syntax node to its canonical <see cref="Expression"/> representation.
    /// </summary>
    /// <param name="syntax">The syntax node to convert.</param>
    /// <returns>The converted expression.</returns>
    public Expression Convert(ExpressionSyntax syntax)
    {
        ArgumentNullException.ThrowIfNull(syntax);

        return syntax switch
        {
            LiteralExpressionSyntax lit => ConvertLiteral(lit),
            IdentifierNameSyntax id => ConvertIdentifier(id),
            BinaryExpressionSyntax bin => ConvertBinary(bin),
            UnaryExpressionSyntax unary => ConvertUnary(unary),
            PostfixExpressionSyntax postfix => ConvertPostfix(postfix),
            ParenthesizedExpressionSyntax paren => Convert(paren.Inner),
            FunctionCallExpressionSyntax func => ConvertFunctionCall(func),
            EquationExpressionSyntax eq => ConvertEquation(eq),
            AssignmentExpressionSyntax assign => ConvertAssignment(assign),
            ConditionalExpressionSyntax cond => ConvertConditional(cond),
            PiecewiseExpressionSyntax pw => ConvertPiecewise(pw),
            LambdaExpressionSyntax lam => ConvertLambda(lam),
            VectorLiteralExpressionSyntax vec => ConvertVector(vec),
            MatrixLiteralExpressionSyntax mat => ConvertMatrix(mat),
            SetLiteralExpressionSyntax set => ConvertSet(set),
            TupleExpressionSyntax tuple => ConvertTuple(tuple),
            IntervalExpressionSyntax interval => ConvertInterval(interval),
            IndexExpressionSyntax idx => ConvertIndex(idx),
            DerivativeExpressionSyntax deriv => ConvertDerivative(deriv),
            IntegralExpressionSyntax integ => ConvertIntegral(integ),
            SummationExpressionSyntax summ => ConvertSummation(summ),
            ProductExpressionSyntax prod => ConvertProduct(prod),
            LimitExpressionSyntax lim => ConvertLimit(lim),
            SuperscriptExpressionSyntax super => ConvertSuperscript(super),
            _ => throw new InvalidOperationException($"Unsupported syntax node type: {syntax.Kind}")
        };
    }

    private Expression ConvertLiteral(LiteralExpressionSyntax literal)
    {
        var token = literal.Token;
        switch (token.Kind)
        {
            case SyntaxKind.TrueKeyword:
                return new BooleanExpression(true);
            case SyntaxKind.FalseKeyword:
                return new BooleanExpression(false);
            case SyntaxKind.IntegerLiteralToken:
                if (token.Value is int intVal)
                    return new LiteralExpression(intVal);
                if (int.TryParse(token.Text, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsedInt))
                    return new LiteralExpression(parsedInt);
                return new LiteralExpression(0);
            case SyntaxKind.RealLiteralToken:
                if (token.Value is string s && IsKnownConstant(s, out var knownConst))
                    return knownConst;
                if (token.Value is double doubleVal)
                    return new LiteralExpression(doubleVal);
                if (double.TryParse(token.Text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsedDouble))
                    return new LiteralExpression(parsedDouble);
                return new LiteralExpression(0);
            default:
                if (token.Value is double d)
                    return new LiteralExpression(d);
                if (token.Value is int i)
                    return new LiteralExpression(i);
                if (token.Value is string ds && IsKnownConstant(ds, out var c))
                    return c;
                return new LiteralExpression(0);
        }
    }

    private bool IsKnownConstant(string value, out Expression constant)
    {
        switch (value)
        {
            case "pi":
                constant = ConstantExpression.Pi;
                return true;
            case "e":
                constant = ConstantExpression.E;
                return true;
            case "i":
                constant = ConstantExpression.I;
                return true;
            case "\u221E":
            case "infinity":
            case "inf":
                constant = ConstantExpression.PositiveInfinity;
                return true;
            default:
                constant = NullExpression.Instance;
                return false;
        }
    }

    private Expression ConvertIdentifier(IdentifierNameSyntax identifier)
    {
        var name = identifier.Name;
        if (IsKnownConstant(name, out var constant))
            return constant;
        return new VariableExpression(name);
    }

    private Expression ConvertBinary(BinaryExpressionSyntax binary)
    {
        var op = MapSyntaxKindToOperator(binary.OperatorToken.Kind);
        var left = Convert(binary.Left);
        var right = Convert(binary.Right);

        if (op == MathOperator.Equal || op == MathOperator.NotEqual ||
            op == MathOperator.LessThan || op == MathOperator.GreaterThan ||
            op == MathOperator.LessThanOrEqual || op == MathOperator.GreaterThanOrEqual ||
            op == MathOperator.ElementOf)
            return new RelationExpression(op, left, right);

        if (op == MathOperator.Compose)
            return new CompositionExpression([left, right]);

        return new BinaryExpression(op, left, right);
    }

    private Expression ConvertUnary(UnaryExpressionSyntax unary)
    {
        var operand = Convert(unary.Operand);
        if (unary.IsPrefix)
        {
            return unary.OperatorToken.Kind switch
            {
                SyntaxKind.MinusToken => Expr.Negate(operand),
                SyntaxKind.ExclamationToken or SyntaxKind.NegationToken => Expr.Not(operand),
                SyntaxKind.PlusToken => operand,
                _ => throw new InvalidOperationException($"Unsupported prefix unary operator: {unary.OperatorToken.Kind}")
            };
        }

        return unary.OperatorToken.Kind switch
        {
            SyntaxKind.ExclamationToken => Expr.Factorial(operand),
            SyntaxKind.TransposeToken => Expr.Transpose(operand),
            SyntaxKind.InverseToken => new UnaryExpression(MathOperator.Inverse, operand),
            _ => throw new InvalidOperationException($"Unsupported postfix unary operator: {unary.OperatorToken.Kind}")
        };
    }

    private Expression ConvertPostfix(PostfixExpressionSyntax postfix)
    {
        var operand = Convert(postfix.Operand);
        return postfix.OperatorToken.Kind switch
        {
            SyntaxKind.ExclamationToken => Expr.Factorial(operand),
            SyntaxKind.TransposeToken => Expr.Transpose(operand),
            SyntaxKind.InverseToken => new UnaryExpression(MathOperator.Inverse, operand),
            _ => throw new InvalidOperationException($"Unsupported postfix operator: {postfix.OperatorToken.Kind}")
        };
    }

    private Expression ConvertFunctionCall(FunctionCallExpressionSyntax functionCall)
    {
        var args = new Expression[functionCall.Arguments.Count];
        for (var i = 0; i < functionCall.Arguments.Count; i++)
            args[i] = Convert(functionCall.Arguments[i]);

        var name = functionCall.FunctionName;
        return name switch
        {
            "sin" when args.Length == 1 => Expr.Sin(args[0]),
            "cos" when args.Length == 1 => Expr.Cos(args[0]),
            "tan" when args.Length == 1 => Expr.Tan(args[0]),
            "asin" when args.Length == 1 => Expr.Asin(args[0]),
            "acos" when args.Length == 1 => Expr.Acos(args[0]),
            "atan" when args.Length == 1 => Expr.Atan(args[0]),
            "sinh" when args.Length == 1 => Expr.Sinh(args[0]),
            "cosh" when args.Length == 1 => Expr.Cosh(args[0]),
            "tanh" when args.Length == 1 => Expr.Tanh(args[0]),
            "ln" when args.Length == 1 => Expr.Ln(args[0]),
            "log" when args.Length == 1 => Expr.Ln(args[0]),
            "log" when args.Length == 2 => Expr.Log(args[0], args[1]),
            "log10" when args.Length == 1 => Expr.Log10(args[0]),
            "exp" when args.Length == 1 => Expr.Exp(args[0]),
            "sqrt" when args.Length == 1 => Expr.Sqrt(args[0]),
            "cbrt" when args.Length == 1 => Expr.Cbrt(args[0]),
            "abs" when args.Length == 1 => Expr.Abs(args[0]),
            "mod" when args.Length == 2 => Expr.Modulo(args[0], args[1]),
            _ => Expr.Call(name, args)
        };
    }

    private Expression ConvertEquation(EquationExpressionSyntax equation)
    {
        var left = Convert(equation.Left);
        var right = Convert(equation.Right);
        return Expr.Equation(left, right);
    }

    private Expression ConvertAssignment(AssignmentExpressionSyntax assignment)
    {
        var target = Convert(assignment.Target);
        var value = Convert(assignment.Value);
        return Expr.Assign(target, value);
    }

    private Expression ConvertConditional(ConditionalExpressionSyntax conditional)
    {
        var condition = Convert(conditional.Condition);
        var thenBranch = Convert(conditional.ThenBranch);
        var elseBranch = Convert(conditional.ElseBranch);
        return Expr.Conditional(condition, thenBranch, elseBranch);
    }

    private Expression ConvertPiecewise(PiecewiseExpressionSyntax piecewise)
    {
        var cases = new PiecewiseCase[piecewise.Cases.Count];
        for (var i = 0; i < piecewise.Cases.Count; i++)
        {
            var c = piecewise.Cases[i];
            cases[i] = new PiecewiseCase(Convert(c.Value), Convert(c.Condition));
        }
        return Expr.Piecewise(cases);
    }

    private Expression ConvertLambda(LambdaExpressionSyntax lambda)
    {
        var parameters = new ParameterExpression[lambda.Parameters.Count];
        for (var i = 0; i < lambda.Parameters.Count; i++)
            parameters[i] = new ParameterExpression(lambda.Parameters[i].Name);
        var body = Convert(lambda.Body);
        return Expr.Lambda(parameters, body);
    }

    private Expression ConvertVector(VectorLiteralExpressionSyntax vector)
    {
        var elements = new Expression[vector.Elements.Count];
        for (var i = 0; i < vector.Elements.Count; i++)
            elements[i] = Convert(vector.Elements[i]);
        return Expr.Vector(elements);
    }

    private Expression ConvertMatrix(MatrixLiteralExpressionSyntax matrix)
    {
        var rows = new Expression[matrix.Rows.Count];
        for (var i = 0; i < matrix.Rows.Count; i++)
        {
            var row = matrix.Rows[i];
            var components = new Expression[row.Elements.Count];
            for (var j = 0; j < row.Elements.Count; j++)
                components[j] = Convert(row.Elements[j]);
            rows[i] = Expr.Vector(components);
        }
        return Expr.Matrix(rows);
    }

    private Expression ConvertSet(SetLiteralExpressionSyntax set)
    {
        var elements = new Expression[set.Elements.Count];
        for (var i = 0; i < set.Elements.Count; i++)
            elements[i] = Convert(set.Elements[i]);
        return Expr.Set(elements);
    }

    private Expression ConvertTuple(TupleExpressionSyntax tuple)
    {
        var elements = new Expression[tuple.Elements.Count];
        for (var i = 0; i < tuple.Elements.Count; i++)
            elements[i] = Convert(tuple.Elements[i]);
        return Expr.Tuple(elements);
    }

    private Expression ConvertInterval(IntervalExpressionSyntax interval)
    {
        var lower = Convert(interval.Lower);
        var upper = Convert(interval.Upper);
        return Expr.Interval(lower, upper, interval.LowerClosed, interval.UpperClosed);
    }

    private Expression ConvertIndex(IndexExpressionSyntax index)
    {
        var target = Convert(index.Target);
        var indices = new Expression[index.Indices.Count];
        for (var i = 0; i < index.Indices.Count; i++)
            indices[i] = Convert(index.Indices[i]);
        return Expr.Index(target, indices);
    }

    private Expression ConvertDerivative(DerivativeExpressionSyntax derivative)
    {
        var function = Convert(derivative.Function);
        var variable = new VariableExpression(derivative.VariableToken.Text);
        return Expr.Derivative(function, variable);
    }

    private Expression ConvertIntegral(IntegralExpressionSyntax integral)
    {
        var integrand = Convert(integral.Integrand);
        var variable = new VariableExpression(integral.VariableToken.Text);
        if (integral.LowerBound is not null && integral.UpperBound is not null)
        {
            var lower = Convert(integral.LowerBound);
            var upper = Convert(integral.UpperBound);
            return Expr.Integral(integrand, variable, lower, upper);
        }
        return Expr.Integral(integrand, variable);
    }

    private Expression ConvertSummation(SummationExpressionSyntax summation)
    {
        var variable = new VariableExpression(summation.VariableToken.Text);
        var lower = Convert(summation.LowerBound);
        var upper = Convert(summation.UpperBound);
        var body = Convert(summation.Body);
        return Expr.Summation(variable, lower, upper, body);
    }

    private Expression ConvertProduct(ProductExpressionSyntax product)
    {
        var variable = new VariableExpression(product.VariableToken.Text);
        var lower = Convert(product.LowerBound);
        var upper = Convert(product.UpperBound);
        var body = Convert(product.Body);
        return Expr.Product(variable, lower, upper, body);
    }

    private Expression ConvertLimit(LimitExpressionSyntax limit)
    {
        var body = Convert(limit.Body);
        var variable = new VariableExpression(limit.VariableToken.Text);
        var target = Convert(limit.Target);
        return Expr.Limit(body, variable, target);
    }

    private Expression ConvertSuperscript(SuperscriptExpressionSyntax superscript)
    {
        var baseExpr = Convert(superscript.Base);
        var exponentText = superscript.SuperscriptToken.Text;
        if (TryParseSuperScriptNumber(exponentText, out var exponentValue))
            return Expr.Pow(baseExpr, new LiteralExpression(exponentValue));
        return Expr.Pow(baseExpr, new VariableExpression(exponentText));
    }

    private bool TryParseSuperScriptNumber(string text, out double value)
    {
        value = 0;
        var result = 0.0;
        foreach (var c in text)
        {
            if (UnicodeMathSupport.TryGetSuperScriptValue(c, out var digit))
                result = result * 10 + digit;
            else
                return false;
        }
        value = result;
        return text.Length > 0;
    }

    internal static MathOperator MapSyntaxKindToOperator(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken => MathOperator.Add,
            SyntaxKind.MinusToken => MathOperator.Subtract,
            SyntaxKind.StarToken => MathOperator.Multiply,
            SyntaxKind.SlashToken => MathOperator.Divide,
            SyntaxKind.PercentToken => MathOperator.Modulo,
            SyntaxKind.CaretToken => MathOperator.Power,
            SyntaxKind.EqualsEqualsToken => MathOperator.Equal,
            SyntaxKind.NotEqualsToken => MathOperator.NotEqual,
            SyntaxKind.LessThanToken => MathOperator.LessThan,
            SyntaxKind.GreaterThanToken => MathOperator.GreaterThan,
            SyntaxKind.LessThanOrEqualToken => MathOperator.LessThanOrEqual,
            SyntaxKind.GreaterThanOrEqualToken => MathOperator.GreaterThanOrEqual,
            SyntaxKind.WedgeToken => MathOperator.And,
            SyntaxKind.AmpersandAmpersandToken => MathOperator.And,
            SyntaxKind.VeeToken => MathOperator.Or,
            SyntaxKind.PipePipeToken => MathOperator.Or,
            SyntaxKind.ElementOfToken => MathOperator.ElementOf,
            SyntaxKind.UnionToken => MathOperator.Union,
            SyntaxKind.IntersectionToken => MathOperator.Intersection,
            SyntaxKind.DotProductToken => MathOperator.Dot,
            SyntaxKind.CrossProductToken => MathOperator.Cross,
            SyntaxKind.ComposeToken => MathOperator.Compose,
            _ => throw new InvalidOperationException($"Unsupported operator token kind: {kind}")
        };
    }

    internal static MathOperator MapTokenTypeToOperator(TokenType type)
    {
        return type switch
        {
            TokenType.Plus => MathOperator.Add,
            TokenType.Minus => MathOperator.Subtract,
            TokenType.Star => MathOperator.Multiply,
            TokenType.Slash => MathOperator.Divide,
            TokenType.Percent => MathOperator.Modulo,
            TokenType.Caret => MathOperator.Power,
            TokenType.EqualsEquals => MathOperator.Equal,
            TokenType.NotEquals => MathOperator.NotEqual,
            TokenType.LessThan => MathOperator.LessThan,
            TokenType.GreaterThan => MathOperator.GreaterThan,
            TokenType.LessThanOrEqual => MathOperator.LessThanOrEqual,
            TokenType.GreaterThanOrEqual => MathOperator.GreaterThanOrEqual,
            TokenType.Wedge => MathOperator.And,
            TokenType.AmpersandAmpersand => MathOperator.And,
            TokenType.Vee => MathOperator.Or,
            TokenType.PipePipe => MathOperator.Or,
            TokenType.ElementOf => MathOperator.ElementOf,
            TokenType.Union => MathOperator.Union,
            TokenType.Intersection => MathOperator.Intersection,
            TokenType.DotProduct => MathOperator.Dot,
            TokenType.CrossProduct => MathOperator.Cross,
            TokenType.Compose => MathOperator.Compose,
            _ => throw new InvalidOperationException($"Unsupported token type: {type}")
        };
    }
}
