namespace MathVerse.Math.Visitors;

/// <summary>
/// Creates a deep clone of an expression tree.
/// </summary>
public sealed class ExpressionCloner : IExpressionTransformer
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionCloner Instance = new();

    private ExpressionCloner() { }

    /// <summary>Creates a deep clone of the specified expression.</summary>
    public static Expression Clone(Expression expression) =>
        expression.Accept(Instance);

    /// <inheritdoc/>
    public Expression Visit(LiteralExpression expression) =>
        new LiteralExpression(expression.Value);

    /// <inheritdoc/>
    public Expression Visit(VariableExpression expression) =>
        new VariableExpression(expression.Name);

    /// <inheritdoc/>
    public Expression Visit(ConstantExpression expression) =>
        new ConstantExpression(expression.Name, expression.Value);

    /// <inheritdoc/>
    public Expression Visit(BinaryExpression expression) =>
        new BinaryExpression(expression.Operator, expression.Left.Accept(this), expression.Right.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(UnaryExpression expression) =>
        new UnaryExpression(expression.Operator, expression.Operand.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(FunctionCallExpression expression)
    {
        var args = new Expression[expression.Arguments.Count];
        for (var i = 0; i < args.Length; i++)
            args[i] = expression.Arguments[i].Accept(this);
        return new FunctionCallExpression(expression.Name, args);
    }

    /// <inheritdoc/>
    public Expression Visit(LambdaExpression expression)
    {
        var @params = new ParameterExpression[expression.Parameters.Count];
        for (var i = 0; i < @params.Length; i++)
            @params[i] = (ParameterExpression)expression.Parameters[i].Accept(this);
        return new LambdaExpression(@params, expression.Body.Accept(this));
    }

    /// <inheritdoc/>
    public Expression Visit(ParameterExpression expression) =>
        new ParameterExpression(expression.Name);

    /// <inheritdoc/>
    public Expression Visit(EquationExpression expression) =>
        new EquationExpression(expression.Left.Accept(this), expression.Right.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(PiecewiseExpression expression)
    {
        var cases = new PiecewiseCase[expression.Cases.Count];
        for (var i = 0; i < cases.Length; i++)
            cases[i] = new PiecewiseCase(
                expression.Cases[i].Value.Accept(this),
                expression.Cases[i].Condition.Accept(this));
        return new PiecewiseExpression(cases, expression.DefaultCase?.Accept(this));
    }

    /// <inheritdoc/>
    public Expression Visit(ConditionalExpression expression) =>
        new ConditionalExpression(
            expression.Condition.Accept(this),
            expression.ThenBranch.Accept(this),
            expression.ElseBranch.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(TupleExpression expression)
    {
        var elements = new Expression[expression.Elements.Count];
        for (var i = 0; i < elements.Length; i++)
            elements[i] = expression.Elements[i].Accept(this);
        return new TupleExpression(elements);
    }

    /// <inheritdoc/>
    public Expression Visit(VectorExpression expression)
    {
        var components = new Expression[expression.Components.Count];
        for (var i = 0; i < components.Length; i++)
            components[i] = expression.Components[i].Accept(this);
        return new VectorExpression(components);
    }

    /// <inheritdoc/>
    public Expression Visit(MatrixExpression expression)
    {
        var rows = new Expression[expression.Rows.Count];
        for (var i = 0; i < rows.Length; i++)
            rows[i] = expression.Rows[i].Accept(this);
        return new MatrixExpression(rows);
    }

    /// <inheritdoc/>
    public Expression Visit(TensorExpression expression)
    {
        var components = new Expression[expression.Components.Count];
        for (var i = 0; i < components.Length; i++)
            components[i] = expression.Components[i].Accept(this);
        return new TensorExpression(expression.Shape, components);
    }

    /// <inheritdoc/>
    public Expression Visit(IndexExpression expression)
    {
        var indices = new Expression[expression.Indices.Count];
        for (var i = 0; i < indices.Length; i++)
            indices[i] = expression.Indices[i].Accept(this);
        return new IndexExpression(expression.Target.Accept(this), indices);
    }

    /// <inheritdoc/>
    public Expression Visit(SliceExpression expression) =>
        new SliceExpression(expression.Target.Accept(this), expression.Slices);

    /// <inheritdoc/>
    public Expression Visit(DerivativeExpression expression) =>
        new DerivativeExpression(expression.Function.Accept(this), expression.Variable.Accept(this), expression.Order);

    /// <inheritdoc/>
    public Expression Visit(IntegralExpression expression)
    {
        var integrand = expression.Integrand.Accept(this);
        var variable = expression.Variable.Accept(this);
        var lower = expression.LowerBound?.Accept(this);
        var upper = expression.UpperBound?.Accept(this);

        return lower is not null && upper is not null
            ? new IntegralExpression(integrand, variable, lower, upper)
            : new IntegralExpression(integrand, variable);
    }

    /// <inheritdoc/>
    public Expression Visit(SummationExpression expression) =>
        new SummationExpression(
            expression.Variable.Accept(this),
            expression.LowerBound.Accept(this),
            expression.UpperBound.Accept(this),
            expression.Body.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(ProductExpression expression) =>
        new ProductExpression(
            expression.Variable.Accept(this),
            expression.LowerBound.Accept(this),
            expression.UpperBound.Accept(this),
            expression.Body.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(LimitExpression expression) =>
        new LimitExpression(
            expression.Body.Accept(this),
            expression.Variable.Accept(this),
            expression.Target.Accept(this),
            expression.Direction);

    /// <inheritdoc/>
    public Expression Visit(FactorialExpression expression) =>
        new FactorialExpression(expression.Operand.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(RangeExpression expression) =>
        new RangeExpression(
            expression.Start.Accept(this),
            expression.End.Accept(this),
            expression.Step?.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(IntervalExpression expression) =>
        new IntervalExpression(
            expression.Lower.Accept(this),
            expression.Upper.Accept(this),
            expression.LowerClosed,
            expression.UpperClosed);

    /// <inheritdoc/>
    public Expression Visit(SetExpression expression)
    {
        var elements = new Expression[expression.Elements.Count];
        for (var i = 0; i < elements.Length; i++)
            elements[i] = expression.Elements[i].Accept(this);
        return new SetExpression(elements);
    }

    /// <inheritdoc/>
    public Expression Visit(ComplexExpression expression) =>
        new ComplexExpression(expression.Real.Accept(this), expression.Imaginary.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(PolynomialExpression expression)
    {
        var coefficients = new Expression[expression.Coefficients.Count];
        for (var i = 0; i < coefficients.Length; i++)
            coefficients[i] = expression.Coefficients[i].Accept(this);
        return new PolynomialExpression(expression.Variable.Accept(this), coefficients);
    }

    /// <inheritdoc/>
    public Expression Visit(BooleanExpression expression) =>
        new BooleanExpression(expression.Value);

    /// <inheritdoc/>
    public Expression Visit(RelationExpression expression) =>
        new RelationExpression(expression.Operator, expression.Left.Accept(this), expression.Right.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(AssignmentExpression expression) =>
        new AssignmentExpression(expression.Target.Accept(this), expression.Value.Accept(this));

    /// <inheritdoc/>
    public Expression Visit(CompositionExpression expression)
    {
        var functions = new Expression[expression.Functions.Count];
        for (var i = 0; i < functions.Length; i++)
            functions[i] = expression.Functions[i].Accept(this);
        return new CompositionExpression(functions);
    }

    /// <inheritdoc/>
    public Expression Visit(IdentityExpression expression) =>
        new IdentityExpression(expression.Operation);

    /// <inheritdoc/>
    public Expression Visit(NullExpression expression) => expression;

    /// <inheritdoc/>
    public Expression Visit(AnnotatedExpression expression) =>
        new AnnotatedExpression(expression.Inner.Accept(this), expression.Key, expression.AnnotationValue);
}
