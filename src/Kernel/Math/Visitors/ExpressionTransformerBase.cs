namespace MathVerse.Math.Visitors;

/// <summary>
/// Base class for expression transformers that recursively walk the tree
/// and rebuild it with possibly transformed children.
/// </summary>
public abstract class ExpressionTransformerBase : IExpressionTransformer
{
    /// <summary>Transforms a child expression (override for custom behavior).</summary>
    internal virtual Expression Transform(Expression expression) =>
        expression.Accept(this);

    /// <summary>Transforms a list of child expressions.</summary>
    protected IReadOnlyList<Expression> TransformChildren(IReadOnlyList<Expression> children)
    {
        var result = new Expression[children.Count];
        var changed = false;

        for (var i = 0; i < children.Count; i++)
        {
            var transformed = Transform(children[i]);
            result[i] = transformed;
            if (!ReferenceEquals(transformed, children[i]))
                changed = true;
        }

        return changed ? result : children;
    }

    /// <inheritdoc/>
    public virtual Expression Visit(LiteralExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(VariableExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(ConstantExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(BinaryExpression expression)
    {
        var left = Transform(expression.Left);
        var right = Transform(expression.Right);

        return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
            ? expression
            : new BinaryExpression(expression.Operator, left, right);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(UnaryExpression expression)
    {
        var operand = Transform(expression.Operand);

        return ReferenceEquals(operand, expression.Operand)
            ? expression
            : new UnaryExpression(expression.Operator, operand);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(FunctionCallExpression expression)
    {
        var args = TransformChildren(expression.Arguments);

        return ReferenceEquals(args, expression.Arguments)
            ? expression
            : new FunctionCallExpression(expression.Name, args);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(LambdaExpression expression)
    {
        var body = Transform(expression.Body);

        return ReferenceEquals(body, expression.Body)
            ? expression
            : new LambdaExpression(expression.Parameters, body);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(ParameterExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(EquationExpression expression)
    {
        var left = Transform(expression.Left);
        var right = Transform(expression.Right);

        return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
            ? expression
            : new EquationExpression(left, right);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(PiecewiseExpression expression)
    {
        var changed = false;
        var cases = new PiecewiseCase[expression.Cases.Count];

        for (var i = 0; i < expression.Cases.Count; i++)
        {
            var value = Transform(expression.Cases[i].Value);
            var condition = Transform(expression.Cases[i].Condition);

            cases[i] = new PiecewiseCase(value, condition);

            if (!ReferenceEquals(value, expression.Cases[i].Value) ||
                !ReferenceEquals(condition, expression.Cases[i].Condition))
                changed = true;
        }

        Expression? def = null;
        if (expression.DefaultCase is not null)
        {
            def = Transform(expression.DefaultCase);
            if (!ReferenceEquals(def, expression.DefaultCase))
                changed = true;
        }

        return changed ? new PiecewiseExpression(cases, def) : expression;
    }

    /// <inheritdoc/>
    public virtual Expression Visit(ConditionalExpression expression)
    {
        var condition = Transform(expression.Condition);
        var then = Transform(expression.ThenBranch);
        var @else = Transform(expression.ElseBranch);

        return ReferenceEquals(condition, expression.Condition) &&
               ReferenceEquals(then, expression.ThenBranch) &&
               ReferenceEquals(@else, expression.ElseBranch)
            ? expression
            : new ConditionalExpression(condition, then, @else);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(TupleExpression expression)
    {
        var elements = TransformChildren(expression.Elements);

        return ReferenceEquals(elements, expression.Elements)
            ? expression
            : new TupleExpression(elements);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(VectorExpression expression)
    {
        var components = TransformChildren(expression.Components);

        return ReferenceEquals(components, expression.Components)
            ? expression
            : new VectorExpression(components);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(MatrixExpression expression)
    {
        var rows = TransformChildren(expression.Rows);

        return ReferenceEquals(rows, expression.Rows)
            ? expression
            : new MatrixExpression(rows);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(TensorExpression expression)
    {
        var components = TransformChildren(expression.Components);

        return ReferenceEquals(components, expression.Components)
            ? expression
            : new TensorExpression(expression.Shape, components);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(IndexExpression expression)
    {
        var target = Transform(expression.Target);
        var indices = TransformChildren(expression.Indices);

        return ReferenceEquals(target, expression.Target) && ReferenceEquals(indices, expression.Indices)
            ? expression
            : new IndexExpression(target, indices);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(SliceExpression expression)
    {
        var target = Transform(expression.Target);

        return ReferenceEquals(target, expression.Target)
            ? expression
            : new SliceExpression(target, expression.Slices);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(DerivativeExpression expression)
    {
        var function = Transform(expression.Function);
        var variable = Transform(expression.Variable);

        return ReferenceEquals(function, expression.Function) && ReferenceEquals(variable, expression.Variable)
            ? expression
            : new DerivativeExpression(function, variable, expression.Order);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(IntegralExpression expression)
    {
        var integrand = Transform(expression.Integrand);
        var variable = Transform(expression.Variable);
        var lower = expression.LowerBound is not null ? Transform(expression.LowerBound) : null;
        var upper = expression.UpperBound is not null ? Transform(expression.UpperBound) : null;

        return ReferenceEquals(integrand, expression.Integrand) &&
               ReferenceEquals(variable, expression.Variable) &&
               ReferenceEquals(lower, expression.LowerBound) &&
               ReferenceEquals(upper, expression.UpperBound)
            ? expression
            : lower is not null && upper is not null
                ? new IntegralExpression(integrand, variable, lower, upper)
                : new IntegralExpression(integrand, variable);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(SummationExpression expression)
    {
        var variable = Transform(expression.Variable);
        var lower = Transform(expression.LowerBound);
        var upper = Transform(expression.UpperBound);
        var body = Transform(expression.Body);

        return ReferenceEquals(variable, expression.Variable) &&
               ReferenceEquals(lower, expression.LowerBound) &&
               ReferenceEquals(upper, expression.UpperBound) &&
               ReferenceEquals(body, expression.Body)
            ? expression
            : new SummationExpression(variable, lower, upper, body);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(ProductExpression expression)
    {
        var variable = Transform(expression.Variable);
        var lower = Transform(expression.LowerBound);
        var upper = Transform(expression.UpperBound);
        var body = Transform(expression.Body);

        return ReferenceEquals(variable, expression.Variable) &&
               ReferenceEquals(lower, expression.LowerBound) &&
               ReferenceEquals(upper, expression.UpperBound) &&
               ReferenceEquals(body, expression.Body)
            ? expression
            : new ProductExpression(variable, lower, upper, body);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(LimitExpression expression)
    {
        var body = Transform(expression.Body);
        var variable = Transform(expression.Variable);
        var target = Transform(expression.Target);

        return ReferenceEquals(body, expression.Body) &&
               ReferenceEquals(variable, expression.Variable) &&
               ReferenceEquals(target, expression.Target)
            ? expression
            : new LimitExpression(body, variable, target, expression.Direction);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(FactorialExpression expression)
    {
        var operand = Transform(expression.Operand);

        return ReferenceEquals(operand, expression.Operand)
            ? expression
            : new FactorialExpression(operand);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(RangeExpression expression)
    {
        var start = Transform(expression.Start);
        var end = Transform(expression.End);
        var step = expression.Step is not null ? Transform(expression.Step) : null;

        return ReferenceEquals(start, expression.Start) &&
               ReferenceEquals(end, expression.End) &&
               ReferenceEquals(step, expression.Step)
            ? expression
            : new RangeExpression(start, end, step);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(IntervalExpression expression)
    {
        var lower = Transform(expression.Lower);
        var upper = Transform(expression.Upper);

        return ReferenceEquals(lower, expression.Lower) && ReferenceEquals(upper, expression.Upper)
            ? expression
            : new IntervalExpression(lower, upper, expression.LowerClosed, expression.UpperClosed);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(SetExpression expression)
    {
        var elements = TransformChildren(expression.Elements);

        return ReferenceEquals(elements, expression.Elements)
            ? expression
            : new SetExpression(elements);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(ComplexExpression expression)
    {
        var real = Transform(expression.Real);
        var imag = Transform(expression.Imaginary);

        return ReferenceEquals(real, expression.Real) && ReferenceEquals(imag, expression.Imaginary)
            ? expression
            : new ComplexExpression(real, imag);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(PolynomialExpression expression)
    {
        var variable = Transform(expression.Variable);
        var coefficients = TransformChildren(expression.Coefficients);

        return ReferenceEquals(variable, expression.Variable) && ReferenceEquals(coefficients, expression.Coefficients)
            ? expression
            : new PolynomialExpression(variable, coefficients);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(BooleanExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(RelationExpression expression)
    {
        var left = Transform(expression.Left);
        var right = Transform(expression.Right);

        return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
            ? expression
            : new RelationExpression(expression.Operator, left, right);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(AssignmentExpression expression)
    {
        var target = Transform(expression.Target);
        var value = Transform(expression.Value);

        return ReferenceEquals(target, expression.Target) && ReferenceEquals(value, expression.Value)
            ? expression
            : new AssignmentExpression(target, value);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(CompositionExpression expression)
    {
        var functions = TransformChildren(expression.Functions);

        return ReferenceEquals(functions, expression.Functions)
            ? expression
            : new CompositionExpression(functions);
    }

    /// <inheritdoc/>
    public virtual Expression Visit(IdentityExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(NullExpression expression) => expression;

    /// <inheritdoc/>
    public virtual Expression Visit(AnnotatedExpression expression)
    {
        var inner = Transform(expression.Inner);

        return ReferenceEquals(inner, expression.Inner)
            ? expression
            : new AnnotatedExpression(inner, expression.Key, expression.AnnotationValue);
    }
}
