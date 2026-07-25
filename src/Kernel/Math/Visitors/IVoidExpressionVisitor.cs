namespace MathVerse.Math.Visitors;

/// <summary>
/// Void visitor interface for side-effect operations on expressions.
/// </summary>
public interface IExpressionVisitor
{
    /// <summary>Visits a literal expression.</summary>
    void Visit(LiteralExpression expression);

    /// <summary>Visits a variable expression.</summary>
    void Visit(VariableExpression expression);

    /// <summary>Visits a constant expression.</summary>
    void Visit(ConstantExpression expression);

    /// <summary>Visits a binary expression.</summary>
    void Visit(BinaryExpression expression);

    /// <summary>Visits a unary expression.</summary>
    void Visit(UnaryExpression expression);

    /// <summary>Visits a function call expression.</summary>
    void Visit(FunctionCallExpression expression);

    /// <summary>Visits a lambda expression.</summary>
    void Visit(LambdaExpression expression);

    /// <summary>Visits a parameter expression.</summary>
    void Visit(ParameterExpression expression);

    /// <summary>Visits an equation expression.</summary>
    void Visit(EquationExpression expression);

    /// <summary>Visits a piecewise expression.</summary>
    void Visit(PiecewiseExpression expression);

    /// <summary>Visits a conditional expression.</summary>
    void Visit(ConditionalExpression expression);

    /// <summary>Visits a tuple expression.</summary>
    void Visit(TupleExpression expression);

    /// <summary>Visits a vector expression.</summary>
    void Visit(VectorExpression expression);

    /// <summary>Visits a matrix expression.</summary>
    void Visit(MatrixExpression expression);

    /// <summary>Visits a tensor expression.</summary>
    void Visit(TensorExpression expression);

    /// <summary>Visits an index expression.</summary>
    void Visit(IndexExpression expression);

    /// <summary>Visits a slice expression.</summary>
    void Visit(SliceExpression expression);

    /// <summary>Visits a derivative expression.</summary>
    void Visit(DerivativeExpression expression);

    /// <summary>Visits an integral expression.</summary>
    void Visit(IntegralExpression expression);

    /// <summary>Visits a summation expression.</summary>
    void Visit(SummationExpression expression);

    /// <summary>Visits a product expression.</summary>
    void Visit(ProductExpression expression);

    /// <summary>Visits a limit expression.</summary>
    void Visit(LimitExpression expression);

    /// <summary>Visits a factorial expression.</summary>
    void Visit(FactorialExpression expression);

    /// <summary>Visits a range expression.</summary>
    void Visit(RangeExpression expression);

    /// <summary>Visits an interval expression.</summary>
    void Visit(IntervalExpression expression);

    /// <summary>Visits a set expression.</summary>
    void Visit(SetExpression expression);

    /// <summary>Visits a complex expression.</summary>
    void Visit(ComplexExpression expression);

    /// <summary>Visits a polynomial expression.</summary>
    void Visit(PolynomialExpression expression);

    /// <summary>Visits a boolean expression.</summary>
    void Visit(BooleanExpression expression);

    /// <summary>Visits a relation expression.</summary>
    void Visit(RelationExpression expression);

    /// <summary>Visits an assignment expression.</summary>
    void Visit(AssignmentExpression expression);

    /// <summary>Visits a composition expression.</summary>
    void Visit(CompositionExpression expression);

    /// <summary>Visits an identity expression.</summary>
    void Visit(IdentityExpression expression);

    /// <summary>Visits a null expression.</summary>
    void Visit(NullExpression expression);

    /// <summary>Visits an annotated expression.</summary>
    void Visit(AnnotatedExpression expression);
}
