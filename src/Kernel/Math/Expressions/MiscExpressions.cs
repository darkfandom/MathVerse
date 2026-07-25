namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a continuous interval.
/// </summary>
public sealed class IntervalExpression : Expression
{
    /// <summary>Initializes an interval expression.</summary>
    public IntervalExpression(Expression lower, Expression upper, bool lowerClosed = true, bool upperClosed = true)
        : base(ExpressionKind.Interval, 1 + System.Math.Max(lower.Depth, upper.Depth), 1 + lower.NodeCount + upper.NodeCount)
    {
        Lower = Guard.NotNull(lower, nameof(lower));
        Upper = Guard.NotNull(upper, nameof(upper));
        LowerClosed = lowerClosed;
        UpperClosed = upperClosed;
    }

    /// <summary>Gets the lower bound.</summary>
    public Expression Lower { get; }

    /// <summary>Gets the upper bound.</summary>
    public Expression Upper { get; }

    /// <summary>Gets whether the lower bound is included.</summary>
    public bool LowerClosed { get; }

    /// <summary>Gets whether the upper bound is included.</summary>
    public bool UpperClosed { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Lower, Upper];

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
        other is IntervalExpression i &&
        Lower.Equals(i.Lower) &&
        Upper.Equals(i.Upper) &&
        LowerClosed == i.LowerClosed &&
        UpperClosed == i.UpperClosed;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Interval, Lower, Upper, LowerClosed, UpperClosed);
}

/// <summary>
/// Represents a set literal expression.
/// </summary>
public sealed class SetExpression : Expression
{
    /// <summary>Initializes a set expression.</summary>
    public SetExpression(IReadOnlyList<Expression> elements)
        : base(ExpressionKind.Set, ComputeDepth(elements), ComputeNodeCount(elements))
    {
        Elements = elements.ToArray();
    }

    /// <summary>Gets the set elements.</summary>
    public IReadOnlyList<Expression> Elements { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Elements;

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
        if (other is not SetExpression s || Elements.Count != s.Elements.Count)
            return false;

        for (var i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Equals(s.Elements[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Set);
        foreach (var e in Elements)
            hash.Add(e);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<Expression> elements)
    {
        var max = 0;
        foreach (var e in elements)
            if (e.Depth > max) max = e.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> elements)
    {
        var count = 1;
        foreach (var e in elements)
            count += e.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents a complex number expression (a + bi).
/// </summary>
public sealed class ComplexExpression : Expression
{
    /// <summary>Initializes a complex expression.</summary>
    public ComplexExpression(Expression real, Expression imaginary)
        : base(ExpressionKind.Complex, 1 + System.Math.Max(real.Depth, imaginary.Depth), 1 + real.NodeCount + imaginary.NodeCount)
    {
        Real = Guard.NotNull(real, nameof(real));
        Imaginary = Guard.NotNull(imaginary, nameof(imaginary));
    }

    /// <summary>Gets the real part.</summary>
    public Expression Real { get; }

    /// <summary>Gets the imaginary part.</summary>
    public Expression Imaginary { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Real, Imaginary];

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
        other is ComplexExpression c && Real.Equals(c.Real) && Imaginary.Equals(c.Imaginary);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Complex, Real, Imaginary);
}

/// <summary>
/// Represents a polynomial expression with explicit coefficients.
/// </summary>
public sealed class PolynomialExpression : Expression
{
    /// <summary>Initializes a polynomial expression.</summary>
    public PolynomialExpression(Expression variable, IReadOnlyList<Expression> coefficients)
        : base(ExpressionKind.Polynomial, 1 + variable.Depth, ComputeNodeCount(variable, coefficients))
    {
        Variable = Guard.NotNull(variable, nameof(variable));
        Coefficients = coefficients.ToArray();
        Degree = coefficients.Count - 1;
    }

    /// <summary>Gets the polynomial variable.</summary>
    public Expression Variable { get; }

    /// <summary>Gets the coefficients from lowest to highest degree.</summary>
    public IReadOnlyList<Expression> Coefficients { get; }

    /// <summary>Gets the polynomial degree.</summary>
    public int Degree { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression> { Variable };
            list.AddRange(Coefficients);
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
        if (other is not PolynomialExpression p || Coefficients.Count != p.Coefficients.Count)
            return false;

        if (!Variable.Equals(p.Variable))
            return false;

        for (var i = 0; i < Coefficients.Count; i++)
        {
            if (!Coefficients[i].Equals(p.Coefficients[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Polynomial);
        hash.Add(Variable);
        foreach (var c in Coefficients)
            hash.Add(c);
        return hash.ToHashCode();
    }

    private static int ComputeNodeCount(Expression variable, IReadOnlyList<Expression> coefficients)
    {
        var count = 1 + variable.NodeCount;
        foreach (var c in coefficients)
            count += c.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents a boolean expression.
/// </summary>
public sealed class BooleanExpression : Expression
{
    /// <summary>Initializes a boolean expression.</summary>
    public BooleanExpression(bool value)
        : base(ExpressionKind.Boolean, 0, 1)
    {
        Value = value;
    }

    /// <summary>Gets the boolean value.</summary>
    public bool Value { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [];

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
        other is BooleanExpression b && Value == b.Value;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Boolean, Value);
}

/// <summary>
/// Represents a relational expression (==, !=, &lt;, &gt;, &lt;=, &gt;=).
/// </summary>
public sealed class RelationExpression : Expression
{
    /// <summary>Initializes a relation expression.</summary>
    public RelationExpression(MathOperator op, Expression left, Expression right)
        : base(ExpressionKind.Relation, 1 + System.Math.Max(left.Depth, right.Depth), 1 + left.NodeCount + right.NodeCount)
    {
        Operator = Guard.NotNull(op, nameof(op));
        Left = Guard.NotNull(left, nameof(left));
        Right = Guard.NotNull(right, nameof(right));
    }

    /// <summary>Gets the relational operator.</summary>
    public MathOperator Operator { get; }

    /// <summary>Gets the left operand.</summary>
    public Expression Left { get; }

    /// <summary>Gets the right operand.</summary>
    public Expression Right { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Left, Right];

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
        other is RelationExpression r &&
        Operator.Equals(r.Operator) &&
        Left.Equals(r.Left) &&
        Right.Equals(r.Right);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Relation, Operator, Left, Right);
}

/// <summary>
/// Represents an assignment expression.
/// </summary>
public sealed class AssignmentExpression : Expression
{
    /// <summary>Initializes an assignment expression.</summary>
    public AssignmentExpression(Expression target, Expression value)
        : base(ExpressionKind.Assignment, 1 + System.Math.Max(target.Depth, value.Depth), 1 + target.NodeCount + value.NodeCount)
    {
        Target = Guard.NotNull(target, nameof(target));
        Value = Guard.NotNull(value, nameof(value));
    }

    /// <summary>Gets the assignment target.</summary>
    public Expression Target { get; }

    /// <summary>Gets the assigned value.</summary>
    public Expression Value { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Target, Value];

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
        other is AssignmentExpression a && Target.Equals(a.Target) && Value.Equals(a.Value);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Assignment, Target, Value);
}

/// <summary>
/// Represents a function composition expression (f ∘ g).
/// </summary>
public sealed class CompositionExpression : Expression
{
    /// <summary>Initializes a composition expression.</summary>
    public CompositionExpression(IReadOnlyList<Expression> functions)
        : base(ExpressionKind.Composition, ComputeDepth(functions), ComputeNodeCount(functions))
    {
        Functions = Guard.NotNullOrEmpty(functions, nameof(functions)).ToArray();
    }

    /// <summary>Gets the composed functions (applied right to left).</summary>
    public IReadOnlyList<Expression> Functions { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Functions;

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
        if (other is not CompositionExpression c || Functions.Count != c.Functions.Count)
            return false;

        for (var i = 0; i < Functions.Count; i++)
        {
            if (!Functions[i].Equals(c.Functions[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Composition);
        foreach (var f in Functions)
            hash.Add(f);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<Expression> functions)
    {
        var max = 0;
        foreach (var f in functions)
            if (f.Depth > max) max = f.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> functions)
    {
        var count = 1;
        foreach (var f in functions)
            count += f.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents an identity element expression (0 for add, 1 for mul, etc.).
/// </summary>
public sealed class IdentityExpression : Expression
{
    /// <summary>Initializes an identity expression.</summary>
    public IdentityExpression(string operation)
        : base(ExpressionKind.Identity, 0, 1)
    {
        Operation = Guard.NotNullOrWhiteSpace(operation, nameof(operation));
    }

    /// <summary>Gets the operation this is the identity for.</summary>
    public string Operation { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [];

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
        other is IdentityExpression i && Operation == i.Operation;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Identity, Operation);
}

/// <summary>
/// Represents a null/undefined expression.
/// </summary>
public sealed class NullExpression : Expression
{
    /// <summary>Singleton instance.</summary>
    public static readonly NullExpression Instance = new();

    private NullExpression()
        : base(ExpressionKind.Null, 0, 1) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [];

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
        other is NullExpression;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        ExpressionKind.Null.GetHashCode();
}

/// <summary>
/// Wraps an expression with metadata annotations.
/// </summary>
public sealed class AnnotatedExpression : Expression
{
    /// <summary>Initializes an annotated expression.</summary>
    public AnnotatedExpression(Expression inner, string key, object value)
        : base(inner.Kind, inner.Depth, inner.NodeCount)
    {
        Inner = Guard.NotNull(inner, nameof(inner));
        Key = Guard.NotNullOrWhiteSpace(key, nameof(key));
        AnnotationValue = Guard.NotNull(value, nameof(value));
    }

    /// <summary>Gets the inner expression.</summary>
    public Expression Inner { get; }

    /// <summary>Gets the annotation key.</summary>
    public string Key { get; }

    /// <summary>Gets the annotation value.</summary>
    public object AnnotationValue { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Inner];

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
        other is AnnotatedExpression a &&
        Inner.Equals(a.Inner) &&
        Key == a.Key;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(Kind, Inner, Key);
}
