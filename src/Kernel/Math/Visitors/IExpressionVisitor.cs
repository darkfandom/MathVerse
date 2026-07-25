namespace MathVerse.Math.Visitors;

/// <summary>
/// Visitor interface that returns a value for each expression type.
/// </summary>
/// <typeparam name="T">The return type.</typeparam>
public interface IExpressionVisitor<out T>
{
    /// <summary>Visits a literal expression.</summary>
    T Visit(LiteralExpression expression);

    /// <summary>Visits a variable expression.</summary>
    T Visit(VariableExpression expression);

    /// <summary>Visits a constant expression.</summary>
    T Visit(ConstantExpression expression);

    /// <summary>Visits a binary expression.</summary>
    T Visit(BinaryExpression expression);

    /// <summary>Visits a unary expression.</summary>
    T Visit(UnaryExpression expression);

    /// <summary>Visits a function call expression.</summary>
    T Visit(FunctionCallExpression expression);

    /// <summary>Visits a lambda expression.</summary>
    T Visit(LambdaExpression expression);

    /// <summary>Visits a parameter expression.</summary>
    T Visit(ParameterExpression expression);

    /// <summary>Visits an equation expression.</summary>
    T Visit(EquationExpression expression);

    /// <summary>Visits a piecewise expression.</summary>
    T Visit(PiecewiseExpression expression);

    /// <summary>Visits a conditional expression.</summary>
    T Visit(ConditionalExpression expression);

    /// <summary>Visits a tuple expression.</summary>
    T Visit(TupleExpression expression);

    /// <summary>Visits a vector expression.</summary>
    T Visit(VectorExpression expression);

    /// <summary>Visits a matrix expression.</summary>
    T Visit(MatrixExpression expression);

    /// <summary>Visits a tensor expression.</summary>
    T Visit(TensorExpression expression);

    /// <summary>Visits an index expression.</summary>
    T Visit(IndexExpression expression);

    /// <summary>Visits a slice expression.</summary>
    T Visit(SliceExpression expression);

    /// <summary>Visits a derivative expression.</summary>
    T Visit(DerivativeExpression expression);

    /// <summary>Visits an integral expression.</summary>
    T Visit(IntegralExpression expression);

    /// <summary>Visits a summation expression.</summary>
    T Visit(SummationExpression expression);

    /// <summary>Visits a product expression.</summary>
    T Visit(ProductExpression expression);

    /// <summary>Visits a limit expression.</summary>
    T Visit(LimitExpression expression);

    /// <summary>Visits a factorial expression.</summary>
    T Visit(FactorialExpression expression);

    /// <summary>Visits a range expression.</summary>
    T Visit(RangeExpression expression);

    /// <summary>Visits an interval expression.</summary>
    T Visit(IntervalExpression expression);

    /// <summary>Visits a set expression.</summary>
    T Visit(SetExpression expression);

    /// <summary>Visits a complex expression.</summary>
    T Visit(ComplexExpression expression);

    /// <summary>Visits a polynomial expression.</summary>
    T Visit(PolynomialExpression expression);

    /// <summary>Visits a boolean expression.</summary>
    T Visit(BooleanExpression expression);

    /// <summary>Visits a relation expression.</summary>
    T Visit(RelationExpression expression);

    /// <summary>Visits an assignment expression.</summary>
    T Visit(AssignmentExpression expression);

    /// <summary>Visits a composition expression.</summary>
    T Visit(CompositionExpression expression);

    /// <summary>Visits an identity expression.</summary>
    T Visit(IdentityExpression expression);

    /// <summary>Visits a null expression.</summary>
    T Visit(NullExpression expression);

    /// <summary>Visits an annotated expression.</summary>
    T Visit(AnnotatedExpression expression);
}
