namespace MathVerse.Math.Visitors;

/// <summary>
/// Prints an expression as a human-readable string.
/// </summary>
public sealed class ExpressionPrinter : IExpressionVisitor<string>
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionPrinter Instance = new();

    private ExpressionPrinter() { }

    /// <summary>Prints the specified expression.</summary>
    public static string Print(Expression expression) =>
        expression.Accept(Instance);

    /// <inheritdoc/>
    public string Visit(LiteralExpression expression) =>
        expression.Value % 1 == 0 ? ((long)expression.Value).ToString() : expression.Value.ToString("G");

    /// <inheritdoc/>
    public string Visit(VariableExpression expression) => expression.Name;

    /// <inheritdoc/>
    public string Visit(ConstantExpression expression) => expression.Name;

    /// <inheritdoc/>
    public string Visit(BinaryExpression expression) =>
        $"({expression.Left.Accept(this)} {expression.Operator.Symbol} {expression.Right.Accept(this)})";

    /// <inheritdoc/>
    public string Visit(UnaryExpression expression) =>
        expression.Operator.Symbol == "-"
            ? $"-{expression.Operand.Accept(this)}"
            : $"{expression.Operator.Symbol}({expression.Operand.Accept(this)})";

    /// <inheritdoc/>
    public string Visit(FunctionCallExpression expression)
    {
        var args = string.Join(", ", expression.Arguments.Select(a => a.Accept(this)));
        return $"{expression.Name}({args})";
    }

    /// <inheritdoc/>
    public string Visit(LambdaExpression expression)
    {
        var @params = string.Join(", ", expression.Parameters.Select(p => p.Name));
        return $"({@params}) => {expression.Body.Accept(this)}";
    }

    /// <inheritdoc/>
    public string Visit(ParameterExpression expression) => expression.Name;

    /// <inheritdoc/>
    public string Visit(EquationExpression expression) =>
        $"{expression.Left.Accept(this)} = {expression.Right.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(PiecewiseExpression expression)
    {
        var cases = string.Join("; ", expression.Cases.Select(c => $"{c.Value.Accept(this)} if {c.Condition.Accept(this)}"));
        if (expression.DefaultCase is not null)
            cases += $"; otherwise {expression.DefaultCase.Accept(this)}";
        return $"piecewise({cases})";
    }

    /// <inheritdoc/>
    public string Visit(ConditionalExpression expression) =>
        $"if {expression.Condition.Accept(this)} then {expression.ThenBranch.Accept(this)} else {expression.ElseBranch.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(TupleExpression expression) =>
        $"({string.Join(", ", expression.Elements.Select(e => e.Accept(this)))})";

    /// <inheritdoc/>
    public string Visit(VectorExpression expression) =>
        $"[{string.Join(", ", expression.Components.Select(c => c.Accept(this)))}]";

    /// <inheritdoc/>
    public string Visit(MatrixExpression expression) =>
        $"[{string.Join("; ", expression.Rows.Select(r => r.Accept(this)))}]";

    /// <inheritdoc/>
    public string Visit(TensorExpression expression) =>
        $"tensor({expression.Shape.Count}D, [{string.Join(", ", expression.Components.Select(c => c.Accept(this)))}])";

    /// <inheritdoc/>
    public string Visit(IndexExpression expression) =>
        $"{expression.Target.Accept(this)}[{string.Join(", ", expression.Indices.Select(i => i.Accept(this)))}]";

    /// <inheritdoc/>
    public string Visit(SliceExpression expression) =>
        $"{expression.Target.Accept(this)}[{string.Join(", ", expression.Slices.Select(s => s?.Accept(this) ?? ""))}]";

    /// <inheritdoc/>
    public string Visit(DerivativeExpression expression) =>
        expression.Order == 1
            ? $"d/d{expression.Variable.Accept(this)} {expression.Function.Accept(this)}"
            : $"d^{expression.Order}/d{expression.Variable.Accept(this)}^{expression.Order} {expression.Function.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(IntegralExpression expression)
    {
        if (expression.IsDefinite)
            return $"∫[{expression.LowerBound!.Accept(this)}..{expression.UpperBound!.Accept(this)}] {expression.Integrand.Accept(this)} d{expression.Variable.Accept(this)}";
        return $"∫ {expression.Integrand.Accept(this)} d{expression.Variable.Accept(this)}";
    }

    /// <inheritdoc/>
    public string Visit(SummationExpression expression) =>
        $"Σ[{expression.Variable.Accept(this)}={expression.LowerBound.Accept(this)}..{expression.UpperBound.Accept(this)}] {expression.Body.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(ProductExpression expression) =>
        $"Π[{expression.Variable.Accept(this)}={expression.LowerBound.Accept(this)}..{expression.UpperBound.Accept(this)}] {expression.Body.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(LimitExpression expression)
    {
        var dir = expression.Direction switch
        {
            LimitDirection.Left => "⁻",
            LimitDirection.Right => "⁺",
            _ => ""
        };
        return $"lim[{expression.Variable.Accept(this)}→{expression.Target.Accept(this)}{dir}] {expression.Body.Accept(this)}";
    }

    /// <inheritdoc/>
    public string Visit(FactorialExpression expression) =>
        $"{expression.Operand.Accept(this)}!";

    /// <inheritdoc/>
    public string Visit(RangeExpression expression)
    {
        var step = expression.Step is not null ? $" by {expression.Step.Accept(this)}" : "";
        return $"{expression.Start.Accept(this)}..{expression.End.Accept(this)}{step}";
    }

    /// <inheritdoc/>
    public string Visit(IntervalExpression expression)
    {
        var l = expression.LowerClosed ? "[" : "(";
        var r = expression.UpperClosed ? "]" : ")";
        return $"{l}{expression.Lower.Accept(this)}, {expression.Upper.Accept(this)}{r}";
    }

    /// <inheritdoc/>
    public string Visit(SetExpression expression) =>
        $"{{{string.Join(", ", expression.Elements.Select(e => e.Accept(this)))}}}";

    /// <inheritdoc/>
    public string Visit(ComplexExpression expression) =>
        $"({expression.Real.Accept(this)} + {expression.Imaginary.Accept(this)}i)";

    /// <inheritdoc/>
    public string Visit(PolynomialExpression expression) =>
        $"poly({expression.Variable.Accept(this)}, deg={expression.Degree})";

    /// <inheritdoc/>
    public string Visit(BooleanExpression expression) =>
        expression.Value ? "true" : "false";

    /// <inheritdoc/>
    public string Visit(RelationExpression expression) =>
        $"({expression.Left.Accept(this)} {expression.Operator.Symbol} {expression.Right.Accept(this)})";

    /// <inheritdoc/>
    public string Visit(AssignmentExpression expression) =>
        $"{expression.Target.Accept(this)} := {expression.Value.Accept(this)}";

    /// <inheritdoc/>
    public string Visit(CompositionExpression expression) =>
        $"({string.Join(" ∘ ", expression.Functions.Select(f => f.Accept(this)))})";

    /// <inheritdoc/>
    public string Visit(IdentityExpression expression) =>
        $"id({expression.Operation})";

    /// <inheritdoc/>
    public string Visit(NullExpression expression) => "null";

    /// <inheritdoc/>
    public string Visit(AnnotatedExpression expression) =>
        expression.Inner.Accept(this);
}

/// <summary>
/// Void visitor that prints expressions to a TextWriter.
/// </summary>
public sealed class ExpressionPrettyPrinter : IExpressionVisitor
{
    private readonly TextWriter _writer;
    private int _indent = 0;

    /// <summary>Initializes a pretty printer with the specified writer.</summary>
    public ExpressionPrettyPrinter(TextWriter writer)
    {
        _writer = Guard.NotNull(writer, nameof(writer));
    }

    /// <summary>Pretty-prints the expression to a string.</summary>
    public static string Print(Expression expression)
    {
        using var writer = new StringWriter();
        var printer = new ExpressionPrettyPrinter(writer);
        expression.Accept(printer);
        return writer.ToString();
    }

    private void Indent() => _writer.Write(new string(' ', _indent * 2));

    /// <inheritdoc/>
    public void Visit(LiteralExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(VariableExpression expression) => _writer.Write(expression.Name);

    /// <inheritdoc/>
    public void Visit(ConstantExpression expression) => _writer.Write(expression.Name);

    /// <inheritdoc/>
    public void Visit(BinaryExpression expression)
    {
        _writer.Write("(");
        expression.Left.Accept(this);
        _writer.Write($" {expression.Operator.Symbol} ");
        expression.Right.Accept(this);
        _writer.Write(")");
    }

    /// <inheritdoc/>
    public void Visit(UnaryExpression expression)
    {
        _writer.Write(expression.Operator.Symbol);
        _writer.Write("(");
        expression.Operand.Accept(this);
        _writer.Write(")");
    }

    /// <inheritdoc/>
    public void Visit(FunctionCallExpression expression)
    {
        _writer.Write($"{expression.Name}(");
        for (var i = 0; i < expression.Arguments.Count; i++)
        {
            if (i > 0) _writer.Write(", ");
            expression.Arguments[i].Accept(this);
        }
        _writer.Write(")");
    }

    /// <inheritdoc/>
    public void Visit(LambdaExpression expression)
    {
        _writer.Write("(");
        for (var i = 0; i < expression.Parameters.Count; i++)
        {
            if (i > 0) _writer.Write(", ");
            expression.Parameters[i].Accept(this);
        }
        _writer.Write(") => ");
        expression.Body.Accept(this);
    }

    /// <inheritdoc/>
    public void Visit(ParameterExpression expression) => _writer.Write(expression.Name);

    /// <inheritdoc/>
    public void Visit(EquationExpression expression)
    {
        expression.Left.Accept(this);
        _writer.Write(" = ");
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public void Visit(PiecewiseExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(ConditionalExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(TupleExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(VectorExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(MatrixExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(TensorExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(IndexExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(SliceExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(DerivativeExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(IntegralExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(SummationExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(ProductExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(LimitExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(FactorialExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(RangeExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(IntervalExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(SetExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(ComplexExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(PolynomialExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(BooleanExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(RelationExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(AssignmentExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(CompositionExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(IdentityExpression expression) => _writer.Write(ExpressionPrinter.Print(expression));

    /// <inheritdoc/>
    public void Visit(NullExpression expression) => _writer.Write("null");

    /// <inheritdoc/>
    public void Visit(AnnotatedExpression expression) => expression.Inner.Accept(this);
}
