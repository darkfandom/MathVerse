namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a derivative expression (d/dx f(x)).
/// </summary>
public sealed class DerivativeExpression : Expression
{
    /// <summary>Initializes a derivative expression.</summary>
    public DerivativeExpression(Expression function, Expression variable, int order = 1)
        : base(ExpressionKind.Derivative, 1 + function.Depth, 1 + function.NodeCount + variable.NodeCount)
    {
        Function = Guard.NotNull(function, nameof(function));
        Variable = Guard.NotNull(variable, nameof(variable));
        Order = Guard.GreaterThan(order, 0, nameof(order));
    }

    /// <summary>Gets the function being differentiated.</summary>
    public Expression Function { get; }

    /// <summary>Gets the differentiation variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the order of the derivative.</summary>
    public int Order { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Function, Variable];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is DerivativeExpression d &&
        Function.Equals(d.Function) &&
        Variable.Equals(d.Variable) &&
        Order == d.Order;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Derivative, Function, Variable, Order);
}

/// <summary>
/// Represents an integral expression.
/// </summary>
public sealed class IntegralExpression : Expression
{
    /// <summary>Initializes an indefinite integral.</summary>
    public IntegralExpression(Expression integrand, Expression variable)
        : base(ExpressionKind.Integral, 1 + integrand.Depth, 1 + integrand.NodeCount + variable.NodeCount)
    {
        Integrand = Guard.NotNull(integrand, nameof(integrand));
        Variable = Guard.NotNull(variable, nameof(variable));
    }

    /// <summary>Initializes a definite integral.</summary>
    public IntegralExpression(Expression integrand, Expression variable, Expression lowerBound, Expression upperBound)
        : base(ExpressionKind.Integral,
            1 + Max4(integrand.Depth, variable.Depth, lowerBound.Depth, upperBound.Depth),
            1 + integrand.NodeCount + variable.NodeCount + lowerBound.NodeCount + upperBound.NodeCount)
    {
        Integrand = Guard.NotNull(integrand, nameof(integrand));
        Variable = Guard.NotNull(variable, nameof(variable));
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    /// <summary>Gets the integrand.</summary>
    public Expression Integrand { get; }

    /// <summary>Gets the integration variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the lower bound (null for indefinite integral).</summary>
    public Expression? LowerBound { get; }

    /// <summary>Gets the upper bound (null for indefinite integral).</summary>
    public Expression? UpperBound { get; }

    /// <summary>Gets whether this is a definite integral.</summary>
    public bool IsDefinite => LowerBound is not null && UpperBound is not null;

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression> { Integrand, Variable };
            if (LowerBound is not null) list.Add(LowerBound);
            if (UpperBound is not null) list.Add(UpperBound);
            return list;
        }
    }

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other)
    {
        if (other is not IntegralExpression i)
            return false;

        return Integrand.Equals(i.Integrand) &&
               Variable.Equals(i.Variable) &&
               Equals(LowerBound, i.LowerBound) &&
               Equals(UpperBound, i.UpperBound);
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Integral, Integrand, Variable, LowerBound, UpperBound);

    private static int Max4(int a, int b, int c, int d) =>
        System.Math.Max(a, System.Math.Max(b, System.Math.Max(c, d)));
}

/// <summary>
/// Represents a summation expression (Sigma).
/// </summary>
public sealed class SummationExpression : Expression
{
    /// <summary>Initializes a summation expression.</summary>
    public SummationExpression(Expression variable, Expression lowerBound, Expression upperBound, Expression body)
        : base(ExpressionKind.Summation,
            1 + Max4(variable.Depth, lowerBound.Depth, upperBound.Depth, body.Depth),
            1 + variable.NodeCount + lowerBound.NodeCount + upperBound.NodeCount + body.NodeCount)
    {
        Variable = Guard.NotNull(variable, nameof(variable));
        LowerBound = Guard.NotNull(lowerBound, nameof(lowerBound));
        UpperBound = Guard.NotNull(upperBound, nameof(upperBound));
        Body = Guard.NotNull(body, nameof(body));
    }

    /// <summary>Gets the summation variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the lower bound.</summary>
    public Expression LowerBound { get; }

    /// <summary>Gets the upper bound.</summary>
    public Expression UpperBound { get; }

    /// <summary>Gets the summation body.</summary>
    public Expression Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Variable, LowerBound, UpperBound, Body];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is SummationExpression s &&
        Variable.Equals(s.Variable) &&
        LowerBound.Equals(s.LowerBound) &&
        UpperBound.Equals(s.UpperBound) &&
        Body.Equals(s.Body);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Summation, Variable, LowerBound, UpperBound, Body);

    private static int Max4(int a, int b, int c, int d) =>
        System.Math.Max(a, System.Math.Max(b, System.Math.Max(c, d)));
}

/// <summary>
/// Represents a product expression (Pi notation).
/// </summary>
public sealed class ProductExpression : Expression
{
    /// <summary>Initializes a product expression.</summary>
    public ProductExpression(Expression variable, Expression lowerBound, Expression upperBound, Expression body)
        : base(ExpressionKind.Product,
            1 + Max4(variable.Depth, lowerBound.Depth, upperBound.Depth, body.Depth),
            1 + variable.NodeCount + lowerBound.NodeCount + upperBound.NodeCount + body.NodeCount)
    {
        Variable = Guard.NotNull(variable, nameof(variable));
        LowerBound = Guard.NotNull(lowerBound, nameof(lowerBound));
        UpperBound = Guard.NotNull(upperBound, nameof(upperBound));
        Body = Guard.NotNull(body, nameof(body));
    }

    /// <summary>Gets the product variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the lower bound.</summary>
    public Expression LowerBound { get; }

    /// <summary>Gets the upper bound.</summary>
    public Expression UpperBound { get; }

    /// <summary>Gets the product body.</summary>
    public Expression Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Variable, LowerBound, UpperBound, Body];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is ProductExpression p &&
        Variable.Equals(p.Variable) &&
        LowerBound.Equals(p.LowerBound) &&
        UpperBound.Equals(p.UpperBound) &&
        Body.Equals(p.Body);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Product, Variable, LowerBound, UpperBound, Body);

    private static int Max4(int a, int b, int c, int d) =>
        System.Math.Max(a, System.Math.Max(b, System.Math.Max(c, d)));
}

/// <summary>
/// Represents a limit expression.
/// </summary>
public sealed class LimitExpression : Expression
{
    /// <summary>Initializes a limit expression.</summary>
    public LimitExpression(Expression body, Expression variable, Expression target, LimitDirection direction = LimitDirection.Both)
        : base(ExpressionKind.Limit,
            1 + Max3(body.Depth, variable.Depth, target.Depth),
            1 + body.NodeCount + variable.NodeCount + target.NodeCount)
    {
        Body = Guard.NotNull(body, nameof(body));
        Variable = Guard.NotNull(variable, nameof(variable));
        Target = Guard.NotNull(target, nameof(target));
        Direction = direction;
    }

    /// <summary>Gets the expression whose limit is taken.</summary>
    public Expression Body { get; }

    /// <summary>Gets the limit variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the target value.</summary>
    public Expression Target { get; }

    /// <summary>Gets the limit direction.</summary>
    public LimitDirection Direction { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Body, Variable, Target];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is LimitExpression l &&
        Body.Equals(l.Body) &&
        Variable.Equals(l.Variable) &&
        Target.Equals(l.Target) &&
        Direction == l.Direction;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Limit, Body, Variable, Target, Direction);

    private static int Max3(int a, int b, int c) =>
        System.Math.Max(a, System.Math.Max(b, c));
}

/// <summary>
/// Specifies the direction of a limit.
/// </summary>
public enum LimitDirection
{
    /// <summary>Two-sided limit.</summary>
    Both,

    /// <summary>Left-hand limit.</summary>
    Left,

    /// <summary>Right-hand limit.</summary>
    Right
}

/// <summary>
/// Represents a factorial expression (n!).
/// </summary>
public sealed class FactorialExpression : Expression
{
    /// <summary>Initializes a factorial expression.</summary>
    public FactorialExpression(Expression operand)
        : base(ExpressionKind.Factorial, 1 + operand.Depth, 1 + operand.NodeCount)
    {
        Operand = Guard.NotNull(operand, nameof(operand));
    }

    /// <summary>Gets the operand.</summary>
    public Expression Operand { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Operand];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is FactorialExpression f && Operand.Equals(f.Operand);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Factorial, Operand);
}

/// <summary>
/// Represents an integer range expression (e.g., 1..10).
/// </summary>
public sealed class RangeExpression : Expression
{
    /// <summary>Initializes a range expression.</summary>
    public RangeExpression(Expression start, Expression end, Expression? step = null)
        : base(ExpressionKind.Range,
            1 + Max3(start.Depth, end.Depth, step?.Depth ?? 0),
            1 + start.NodeCount + end.NodeCount + (step?.NodeCount ?? 0))
    {
        Start = Guard.NotNull(start, nameof(start));
        End = Guard.NotNull(end, nameof(end));
        Step = step;
    }

    /// <summary>Gets the range start.</summary>
    public Expression Start { get; }

    /// <summary>Gets the range end.</summary>
    public Expression End { get; }

    /// <summary>Gets the optional step.</summary>
    public Expression? Step { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression> { Start, End };
            if (Step is not null) list.Add(Step);
            return list;
        }
    }

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is RangeExpression r &&
        Start.Equals(r.Start) &&
        End.Equals(r.End) &&
        Equals(Step, r.Step);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Range, Start, End, Step);

    private static int Max3(int a, int b, int c) => System.Math.Max(a, System.Math.Max(b, c));
}
