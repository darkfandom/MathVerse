namespace MathVerse.Math.Visitors;

/// <summary>
/// Base class for expression rewriters that apply rules to transform expression trees.
/// Override specific visit methods to implement rewriting logic.
/// </summary>
public abstract class ExpressionRewriter : ExpressionTransformerBase
{
    /// <summary>Gets or sets whether the rewriter should apply rules repeatedly until fixpoint.</summary>
    public bool ApplyToFixpoint { get; protected set; }

    /// <summary>Applies the rewriter to the expression, possibly multiple times if fixpoint is enabled.</summary>
    public Expression Rewrite(Expression expression)
    {
        var current = expression;
        Expression previous;

        do
        {
            previous = current;
            current = current.Accept(this);
        }
        while (ApplyToFixpoint && !ReferenceEquals(current, previous));

        return current;
    }
}

/// <summary>
/// Rewriter that replaces one expression with another.
/// </summary>
public sealed class ExpressionReplacer : ExpressionRewriter
{
    private readonly Expression _target;
    private readonly Expression _replacement;

    /// <summary>Initializes a replacer.</summary>
    public ExpressionReplacer(Expression target, Expression replacement)
    {
        _target = Guard.NotNull(target, nameof(target));
        _replacement = Guard.NotNull(replacement, nameof(replacement));
    }

    /// <summary>Replaces all occurrences of the target with the replacement.</summary>
    public static Expression Replace(Expression expression, Expression target, Expression replacement) =>
        new ExpressionReplacer(target, replacement).Rewrite(expression);

    /// <inheritdoc/>
    public override Expression Visit(LiteralExpression expression) =>
        expression.Equals(_target) ? _replacement : expression;

    /// <inheritdoc/>
    public override Expression Visit(VariableExpression expression) =>
        expression.Equals(_target) ? _replacement : expression;

    /// <inheritdoc/>
    public override Expression Visit(ConstantExpression expression) =>
        expression.Equals(_target) ? _replacement : expression;

    /// <inheritdoc/>
    public override Expression Visit(BinaryExpression expression)
    {
        var result = base.Visit(expression);
        return result.Equals(_target) ? _replacement : result;
    }

    /// <inheritdoc/>
    public override Expression Visit(UnaryExpression expression)
    {
        var result = base.Visit(expression);
        return result.Equals(_target) ? _replacement : result;
    }

    /// <inheritdoc/>
    public override Expression Visit(FunctionCallExpression expression)
    {
        var result = base.Visit(expression);
        return result.Equals(_target) ? _replacement : result;
    }
}
