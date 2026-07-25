namespace MathVerse.Math.Visitors;

/// <summary>
/// Compares two expression trees for structural equality.
/// </summary>
public sealed class ExpressionComparer : IExpressionVisitor<bool>
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionComparer Instance = new();

    private Expression? _other;

    private ExpressionComparer() { }

    /// <summary>Compares two expressions for structural equality.</summary>
    public static bool Compare(Expression left, Expression right) =>
        left.Equals(right);

    /// <summary>Compares two expressions by visiting the left and comparing with the right.</summary>
    public bool CompareTrees(Expression left, Expression right)
    {
        _other = right;
        return left.Accept(this);
    }

    /// <inheritdoc/>
    public bool Visit(LiteralExpression expression) =>
        _other is LiteralExpression l && expression.Value.Equals(l.Value);

    /// <inheritdoc/>
    public bool Visit(VariableExpression expression) =>
        _other is VariableExpression v && expression.Name == v.Name;

    /// <inheritdoc/>
    public bool Visit(ConstantExpression expression) =>
        _other is ConstantExpression c && expression.Name == c.Name && expression.Value.Equals(c.Value);

    /// <inheritdoc/>
    public bool Visit(BinaryExpression expression)
    {
        if (_other is not BinaryExpression b || !expression.Operator.Equals(b.Operator))
            return false;

        var leftResult = expression.Left.Accept(this);
        var prev = _other;
        _other = b.Right;
        var rightResult = expression.Right.Accept(this);
        _other = prev;
        return leftResult && rightResult;
    }

    /// <inheritdoc/>
    public bool Visit(UnaryExpression expression)
    {
        if (_other is not UnaryExpression u || !expression.Operator.Equals(u.Operator))
            return false;

        var prev = _other;
        _other = u.Operand;
        var result = expression.Operand.Accept(this);
        _other = prev;
        return result;
    }

    /// <inheritdoc/>
    public bool Visit(FunctionCallExpression expression)
    {
        if (_other is not FunctionCallExpression f || expression.Name != f.Name || expression.Arguments.Count != f.Arguments.Count)
            return false;

        for (var i = 0; i < expression.Arguments.Count; i++)
        {
            var prev = _other;
            _other = f.Arguments[i];
            if (!expression.Arguments[i].Accept(this))
            {
                _other = prev;
                return false;
            }
            _other = prev;
        }

        return true;
    }

    /// <inheritdoc/>
    public bool Visit(LambdaExpression expression) =>
        _other is LambdaExpression l &&
        expression.Parameters.Count == l.Parameters.Count &&
        expression.Body.Equals(l.Body);

    /// <inheritdoc/>
    public bool Visit(ParameterExpression expression) =>
        _other is ParameterExpression p && expression.Name == p.Name;

    /// <inheritdoc/>
    public bool Visit(EquationExpression expression) =>
        _other is EquationExpression e && expression.Left.Equals(e.Left) && expression.Right.Equals(e.Right);

    /// <inheritdoc/>
    public bool Visit(PiecewiseExpression expression) =>
        _other is PiecewiseExpression pw && expression.Cases.Count == pw.Cases.Count;

    /// <inheritdoc/>
    public bool Visit(ConditionalExpression expression) =>
        _other is ConditionalExpression c &&
        expression.Condition.Equals(c.Condition) &&
        expression.ThenBranch.Equals(c.ThenBranch) &&
        expression.ElseBranch.Equals(c.ElseBranch);

    /// <inheritdoc/>
    public bool Visit(TupleExpression expression) =>
        _other is TupleExpression t && expression.Elements.Count == t.Elements.Count;

    /// <inheritdoc/>
    public bool Visit(VectorExpression expression) =>
        _other is VectorExpression v && expression.Components.Count == v.Components.Count;

    /// <inheritdoc/>
    public bool Visit(MatrixExpression expression) =>
        _other is MatrixExpression m && expression.Rows.Count == m.Rows.Count;

    /// <inheritdoc/>
    public bool Visit(TensorExpression expression) =>
        _other is TensorExpression t && expression.Rank == t.Rank && expression.Components.Count == t.Components.Count;

    /// <inheritdoc/>
    public bool Visit(IndexExpression expression) =>
        _other is IndexExpression i && expression.Target.Equals(i.Target) && expression.Indices.Count == i.Indices.Count;

    /// <inheritdoc/>
    public bool Visit(SliceExpression expression) =>
        _other is SliceExpression s && expression.Target.Equals(s.Target) && expression.Slices.Count == s.Slices.Count;

    /// <inheritdoc/>
    public bool Visit(DerivativeExpression expression) =>
        _other is DerivativeExpression d && expression.Order == d.Order && expression.Function.Equals(d.Function) && expression.Variable.Equals(d.Variable);

    /// <inheritdoc/>
    public bool Visit(IntegralExpression expression) =>
        _other is IntegralExpression i && expression.Integrand.Equals(i.Integrand) && expression.Variable.Equals(i.Variable);

    /// <inheritdoc/>
    public bool Visit(SummationExpression expression) =>
        _other is SummationExpression s && expression.Body.Equals(s.Body);

    /// <inheritdoc/>
    public bool Visit(ProductExpression expression) =>
        _other is ProductExpression p && expression.Body.Equals(p.Body);

    /// <inheritdoc/>
    public bool Visit(LimitExpression expression) =>
        _other is LimitExpression l && expression.Body.Equals(l.Body) && expression.Direction == l.Direction;

    /// <inheritdoc/>
    public bool Visit(FactorialExpression expression) =>
        _other is FactorialExpression f && expression.Operand.Equals(f.Operand);

    /// <inheritdoc/>
    public bool Visit(RangeExpression expression) =>
        _other is RangeExpression r && expression.Start.Equals(r.Start) && expression.End.Equals(r.End);

    /// <inheritdoc/>
    public bool Visit(IntervalExpression expression) =>
        _other is IntervalExpression i && expression.Lower.Equals(i.Lower) && expression.Upper.Equals(i.Upper);

    /// <inheritdoc/>
    public bool Visit(SetExpression expression) =>
        _other is SetExpression s && expression.Elements.Count == s.Elements.Count;

    /// <inheritdoc/>
    public bool Visit(ComplexExpression expression) =>
        _other is ComplexExpression c && expression.Real.Equals(c.Real) && expression.Imaginary.Equals(c.Imaginary);

    /// <inheritdoc/>
    public bool Visit(PolynomialExpression expression) =>
        _other is PolynomialExpression p && expression.Degree == p.Degree;

    /// <inheritdoc/>
    public bool Visit(BooleanExpression expression) =>
        _other is BooleanExpression b && expression.Value == b.Value;

    /// <inheritdoc/>
    public bool Visit(RelationExpression expression) =>
        _other is RelationExpression r && expression.Operator.Equals(r.Operator);

    /// <inheritdoc/>
    public bool Visit(AssignmentExpression expression) =>
        _other is AssignmentExpression a && expression.Target.Equals(a.Target) && expression.Value.Equals(a.Value);

    /// <inheritdoc/>
    public bool Visit(CompositionExpression expression) =>
        _other is CompositionExpression c && expression.Functions.Count == c.Functions.Count;

    /// <inheritdoc/>
    public bool Visit(IdentityExpression expression) =>
        _other is IdentityExpression i && expression.Operation == i.Operation;

    /// <inheritdoc/>
    public bool Visit(NullExpression expression) =>
        _other is NullExpression;

    /// <inheritdoc/>
    public bool Visit(AnnotatedExpression expression) =>
        _other is AnnotatedExpression a && expression.Key == a.Key && expression.Inner.Equals(a.Inner);
}

/// <summary>
/// Computes a hash code for an expression tree.
/// </summary>
public sealed class ExpressionHasher : IExpressionVisitor<int>
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionHasher Instance = new();

    private ExpressionHasher() { }

    /// <summary>Computes the hash code of the expression.</summary>
    public static int Hash(Expression expression) =>
        expression.GetHashCode();

    /// <inheritdoc/>
    public int Visit(LiteralExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Value);

    /// <inheritdoc/>
    public int Visit(VariableExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Name);

    /// <inheritdoc/>
    public int Visit(ConstantExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Name, expression.Value);

    /// <inheritdoc/>
    public int Visit(BinaryExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        hash.Add(expression.Operator);
        hash.Add(expression.Left.Accept(this));
        hash.Add(expression.Right.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(UnaryExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        hash.Add(expression.Operator);
        hash.Add(expression.Operand.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(FunctionCallExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        hash.Add(expression.Name);
        foreach (var arg in expression.Arguments)
            hash.Add(arg.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(LambdaExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var p in expression.Parameters)
            hash.Add(p.Name);
        hash.Add(expression.Body.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(ParameterExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Name);

    /// <inheritdoc/>
    public int Visit(EquationExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Left.Accept(this), expression.Right.Accept(this));

    /// <inheritdoc/>
    public int Visit(PiecewiseExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var c in expression.Cases)
        {
            hash.Add(c.Value.Accept(this));
            hash.Add(c.Condition.Accept(this));
        }
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(ConditionalExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Condition.Accept(this), expression.ThenBranch.Accept(this), expression.ElseBranch.Accept(this));

    /// <inheritdoc/>
    public int Visit(TupleExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var e in expression.Elements)
            hash.Add(e.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(VectorExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var c in expression.Components)
            hash.Add(c.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(MatrixExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var r in expression.Rows)
            hash.Add(r.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(TensorExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var s in expression.Shape)
            hash.Add(s);
        foreach (var c in expression.Components)
            hash.Add(c.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(IndexExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        hash.Add(expression.Target.Accept(this));
        foreach (var idx in expression.Indices)
            hash.Add(idx.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(SliceExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Target.Accept(this), expression.Slices.Count);

    /// <inheritdoc/>
    public int Visit(DerivativeExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Function.Accept(this), expression.Variable.Accept(this), expression.Order);

    /// <inheritdoc/>
    public int Visit(IntegralExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Integrand.Accept(this), expression.Variable.Accept(this));

    /// <inheritdoc/>
    public int Visit(SummationExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Variable.Accept(this), expression.Body.Accept(this));

    /// <inheritdoc/>
    public int Visit(ProductExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Variable.Accept(this), expression.Body.Accept(this));

    /// <inheritdoc/>
    public int Visit(LimitExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Body.Accept(this), expression.Direction);

    /// <inheritdoc/>
    public int Visit(FactorialExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Operand.Accept(this));

    /// <inheritdoc/>
    public int Visit(RangeExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Start.Accept(this), expression.End.Accept(this));

    /// <inheritdoc/>
    public int Visit(IntervalExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Lower.Accept(this), expression.Upper.Accept(this));

    /// <inheritdoc/>
    public int Visit(SetExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var e in expression.Elements)
            hash.Add(e.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(ComplexExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Real.Accept(this), expression.Imaginary.Accept(this));

    /// <inheritdoc/>
    public int Visit(PolynomialExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        hash.Add(expression.Variable.Accept(this));
        foreach (var c in expression.Coefficients)
            hash.Add(c.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(BooleanExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Value);

    /// <inheritdoc/>
    public int Visit(RelationExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Operator, expression.Left.Accept(this), expression.Right.Accept(this));

    /// <inheritdoc/>
    public int Visit(AssignmentExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Target.Accept(this), expression.Value.Accept(this));

    /// <inheritdoc/>
    public int Visit(CompositionExpression expression)
    {
        var hash = new HashCode();
        hash.Add(expression.Kind);
        foreach (var f in expression.Functions)
            hash.Add(f.Accept(this));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public int Visit(IdentityExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Operation);

    /// <inheritdoc/>
    public int Visit(NullExpression expression) =>
        expression.Kind.GetHashCode();

    /// <inheritdoc/>
    public int Visit(AnnotatedExpression expression) =>
        HashCode.Combine(expression.Kind, expression.Inner.Accept(this), expression.Key);
}
