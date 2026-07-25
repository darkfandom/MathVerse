namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a vector expression.
/// </summary>
public sealed class VectorExpression : Expression
{
    /// <summary>Initializes a vector expression.</summary>
    public VectorExpression(IReadOnlyList<Expression> components)
        : base(ExpressionKind.Vector, ComputeDepth(components), ComputeNodeCount(components))
    {
        Components = components.ToArray();
        Dimension = components.Count;
    }

    /// <summary>Gets the vector components.</summary>
    public IReadOnlyList<Expression> Components { get; }

    /// <summary>Gets the dimension of the vector.</summary>
    public int Dimension { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Components;

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
        if (other is not VectorExpression v || Components.Count != v.Components.Count)
            return false;

        for (var i = 0; i < Components.Count; i++)
        {
            if (!Components[i].Equals(v.Components[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Vector);
        foreach (var c in Components)
            hash.Add(c);
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
/// Represents a matrix expression as a flat array of rows.
/// </summary>
public sealed class MatrixExpression : Expression
{
    /// <summary>Initializes a matrix expression from rows.</summary>
    public MatrixExpression(IReadOnlyList<Expression> rows)
        : base(ExpressionKind.Matrix, ComputeDepth(rows), ComputeNodeCount(rows))
    {
        Rows = rows.ToArray();
        RowCount = rows.Count;
        ColumnCount = rows.Count > 0 && rows[0] is VectorExpression v ? v.Dimension : 0;
    }

    /// <summary>Gets the matrix rows (each row is a VectorExpression).</summary>
    public IReadOnlyList<Expression> Rows { get; }

    /// <summary>Gets the number of rows.</summary>
    public int RowCount { get; }

    /// <summary>Gets the number of columns.</summary>
    public int ColumnCount { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Rows;

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
        if (other is not MatrixExpression m || Rows.Count != m.Rows.Count)
            return false;

        for (var i = 0; i < Rows.Count; i++)
        {
            if (!Rows[i].Equals(m.Rows[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Matrix);
        foreach (var r in Rows)
            hash.Add(r);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<Expression> rows)
    {
        var max = 0;
        foreach (var r in rows)
            if (r.Depth > max) max = r.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> rows)
    {
        var count = 1;
        foreach (var r in rows)
            count += r.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents a tensor expression with arbitrary rank.
/// </summary>
public sealed class TensorExpression : Expression
{
    /// <summary>Initializes a tensor expression.</summary>
    public TensorExpression(IReadOnlyList<int> shape, IReadOnlyList<Expression> components)
        : base(ExpressionKind.Tensor, 1, 1 + components.Count)
    {
        Shape = shape.ToArray();
        Components = components.ToArray();
    }

    /// <summary>Gets the tensor shape (dimensions per axis).</summary>
    public IReadOnlyList<int> Shape { get; }

    /// <summary>Gets the tensor components in flattened order.</summary>
    public IReadOnlyList<Expression> Components { get; }

    /// <summary>Gets the rank (number of dimensions).</summary>
    public int Rank => Shape.Count;

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Components;

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
        if (other is not TensorExpression t || Rank != t.Rank || Components.Count != t.Components.Count)
            return false;

        for (var i = 0; i < Rank; i++)
        {
            if (Shape[i] != t.Shape[i])
                return false;
        }

        for (var i = 0; i < Components.Count; i++)
        {
            if (!Components[i].Equals(t.Components[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Tensor);
        foreach (var s in Shape)
            hash.Add(s);
        foreach (var c in Components)
            hash.Add(c);
        return hash.ToHashCode();
    }
}

/// <summary>
/// Represents an indexing expression (e.g., A[i, j]).
/// </summary>
public sealed class IndexExpression : Expression
{
    /// <summary>Initializes an index expression.</summary>
    public IndexExpression(Expression target, IReadOnlyList<Expression> indices)
        : base(ExpressionKind.Index, 1 + ComputeMaxDepth(target, indices), 1 + target.NodeCount + ComputeNodeCount(indices))
    {
        Target = Guard.NotNull(target, nameof(target));
        Indices = indices.ToArray();
    }

    /// <summary>Gets the expression being indexed.</summary>
    public Expression Target { get; }

    /// <summary>Gets the index expressions.</summary>
    public IReadOnlyList<Expression> Indices { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression> { Target };
            list.AddRange(Indices);
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
        if (other is not IndexExpression idx || Indices.Count != idx.Indices.Count)
            return false;

        if (!Target.Equals(idx.Target))
            return false;

        for (var i = 0; i < Indices.Count; i++)
        {
            if (!Indices[i].Equals(idx.Indices[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Index);
        hash.Add(Target);
        foreach (var idx in Indices)
            hash.Add(idx);
        return hash.ToHashCode();
    }

    private static int ComputeMaxDepth(Expression target, IReadOnlyList<Expression> indices)
    {
        var max = target.Depth;
        foreach (var idx in indices)
            if (idx.Depth > max) max = idx.Depth;
        return max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> indices)
    {
        var count = 0;
        foreach (var idx in indices)
            count += idx.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents a slice expression (e.g., A[1:3, :]).
/// </summary>
public sealed class SliceExpression : Expression
{
    /// <summary>Initializes a slice expression.</summary>
    public SliceExpression(Expression target, IReadOnlyList<Expression?> slices)
        : base(ExpressionKind.Slice, 1 + target.Depth, 1 + target.NodeCount)
    {
        Target = Guard.NotNull(target, nameof(target));
        Slices = slices.ToArray();
    }

    /// <summary>Gets the expression being sliced.</summary>
    public Expression Target { get; }

    /// <summary>Gets the slice specifiers (null means ':').</summary>
    public IReadOnlyList<Expression?> Slices { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Target];

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
        other is SliceExpression s && Target.Equals(s.Target) && Slices.Count == s.Slices.Count;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Slice, Target, Slices.Count);
}
