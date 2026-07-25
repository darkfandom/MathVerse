namespace MathVerse.Math.Visitors;

/// <summary>
/// Walks an expression tree without modifying it. Override methods to perform custom operations.
/// </summary>
public class ExpressionWalker : IExpressionVisitor
{
    /// <inheritdoc/>
    public virtual void Visit(LiteralExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(VariableExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(ConstantExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(BinaryExpression expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(UnaryExpression expression)
    {
        expression.Operand.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(FunctionCallExpression expression)
    {
        foreach (var arg in expression.Arguments)
            arg.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(LambdaExpression expression)
    {
        foreach (var p in expression.Parameters)
            p.Accept(this);
        expression.Body.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(ParameterExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(EquationExpression expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(PiecewiseExpression expression)
    {
        foreach (var c in expression.Cases)
        {
            c.Value.Accept(this);
            c.Condition.Accept(this);
        }
        expression.DefaultCase?.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(ConditionalExpression expression)
    {
        expression.Condition.Accept(this);
        expression.ThenBranch.Accept(this);
        expression.ElseBranch.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(TupleExpression expression)
    {
        foreach (var e in expression.Elements)
            e.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(VectorExpression expression)
    {
        foreach (var c in expression.Components)
            c.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(MatrixExpression expression)
    {
        foreach (var r in expression.Rows)
            r.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(TensorExpression expression)
    {
        foreach (var c in expression.Components)
            c.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(IndexExpression expression)
    {
        expression.Target.Accept(this);
        foreach (var idx in expression.Indices)
            idx.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(SliceExpression expression)
    {
        expression.Target.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(DerivativeExpression expression)
    {
        expression.Function.Accept(this);
        expression.Variable.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(IntegralExpression expression)
    {
        expression.Integrand.Accept(this);
        expression.Variable.Accept(this);
        expression.LowerBound?.Accept(this);
        expression.UpperBound?.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(SummationExpression expression)
    {
        expression.Variable.Accept(this);
        expression.LowerBound.Accept(this);
        expression.UpperBound.Accept(this);
        expression.Body.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(ProductExpression expression)
    {
        expression.Variable.Accept(this);
        expression.LowerBound.Accept(this);
        expression.UpperBound.Accept(this);
        expression.Body.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(LimitExpression expression)
    {
        expression.Body.Accept(this);
        expression.Variable.Accept(this);
        expression.Target.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(FactorialExpression expression)
    {
        expression.Operand.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(RangeExpression expression)
    {
        expression.Start.Accept(this);
        expression.End.Accept(this);
        expression.Step?.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(IntervalExpression expression)
    {
        expression.Lower.Accept(this);
        expression.Upper.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(SetExpression expression)
    {
        foreach (var e in expression.Elements)
            e.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(ComplexExpression expression)
    {
        expression.Real.Accept(this);
        expression.Imaginary.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(PolynomialExpression expression)
    {
        expression.Variable.Accept(this);
        foreach (var c in expression.Coefficients)
            c.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(BooleanExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(RelationExpression expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(AssignmentExpression expression)
    {
        expression.Target.Accept(this);
        expression.Value.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(CompositionExpression expression)
    {
        foreach (var f in expression.Functions)
            f.Accept(this);
    }

    /// <inheritdoc/>
    public virtual void Visit(IdentityExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(NullExpression expression) { }

    /// <inheritdoc/>
    public virtual void Visit(AnnotatedExpression expression)
    {
        expression.Inner.Accept(this);
    }
}
