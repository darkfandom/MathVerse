namespace MathVerse.Math.Semantics.Binding;

/// <summary>Kind of bound node.</summary>
public enum BoundNodeKind
{
    /// <summary>A numeric literal.</summary>
    Literal,
    /// <summary>A variable reference.</summary>
    Variable,
    /// <summary>A binary operation.</summary>
    Binary,
    /// <summary>A unary operation.</summary>
    Unary,
    /// <summary>A function call.</summary>
    FunctionCall,
    /// <summary>An assignment expression.</summary>
    Assignment,
    /// <summary>A constant reference.</summary>
    Constant,
}

/// <summary>Abstract base for all bound nodes.</summary>
public abstract class BoundNode
{
    /// <summary>Gets the node kind.</summary>
    public abstract BoundNodeKind Kind { get; }
}

/// <summary>Abstract base for bound expressions.</summary>
public abstract class BoundExpression : BoundNode
{
    /// <summary>Gets the inferred type name.</summary>
    public abstract string TypeName { get; }
}

/// <summary>A bound numeric literal.</summary>
public sealed class BoundLiteralExpression : BoundExpression
{
    /// <summary>Initializes a bound literal.</summary>
    public BoundLiteralExpression(double value) => Value = value;

    /// <summary>Gets the literal value.</summary>
    public double Value { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Literal;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound constant reference.</summary>
public sealed class BoundConstantExpression : BoundExpression
{
    /// <summary>Initializes a bound constant.</summary>
    public BoundConstantExpression(ConstantSymbol constant) => Constant = constant;

    /// <summary>Gets the constant symbol.</summary>
    public ConstantSymbol Constant { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Constant;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound variable reference.</summary>
public sealed class BoundVariableExpression : BoundExpression
{
    /// <summary>Initializes a bound variable.</summary>
    public BoundVariableExpression(Symbol symbol) => Symbol = symbol;

    /// <summary>Gets the resolved symbol.</summary>
    public Symbol Symbol { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Variable;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound binary operation.</summary>
public sealed class BoundBinaryExpression : BoundExpression
{
    /// <summary>Initializes a bound binary expression.</summary>
    public BoundBinaryExpression(BoundExpression left, MathOperator op, BoundExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    /// <summary>Gets the left operand.</summary>
    public BoundExpression Left { get; }

    /// <summary>Gets the operator.</summary>
    public MathOperator Operator { get; }

    /// <summary>Gets the right operand.</summary>
    public BoundExpression Right { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Binary;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound unary operation.</summary>
public sealed class BoundUnaryExpression : BoundExpression
{
    /// <summary>Initializes a bound unary expression.</summary>
    public BoundUnaryExpression(MathOperator op, BoundExpression operand)
    {
        Operator = op;
        Operand = operand;
    }

    /// <summary>Gets the operator.</summary>
    public MathOperator Operator { get; }

    /// <summary>Gets the operand.</summary>
    public BoundExpression Operand { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Unary;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound function call.</summary>
public sealed class BoundFunctionCallExpression : BoundExpression
{
    /// <summary>Initializes a bound function call.</summary>
    public BoundFunctionCallExpression(FunctionSymbol function, IReadOnlyList<BoundExpression> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    /// <summary>Gets the resolved function symbol.</summary>
    public FunctionSymbol Function { get; }

    /// <summary>Gets the bound arguments.</summary>
    public IReadOnlyList<BoundExpression> Arguments { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.FunctionCall;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}

/// <summary>A bound assignment expression.</summary>
public sealed class BoundAssignmentExpression : BoundExpression
{
    /// <summary>Initializes a bound assignment.</summary>
    public BoundAssignmentExpression(Symbol target, BoundExpression value)
    {
        Target = target;
        Value = value;
    }

    /// <summary>Gets the target symbol.</summary>
    public Symbol Target { get; }

    /// <summary>Gets the assigned value expression.</summary>
    public BoundExpression Value { get; }

    /// <inheritdoc/>
    public override BoundNodeKind Kind => BoundNodeKind.Assignment;

    /// <inheritdoc/>
    public override string TypeName => "Real";
}
