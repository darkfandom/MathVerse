namespace MathVerse.Math.Visitors;

/// <summary>
/// Transformer interface that visits an expression and returns a new (possibly transformed) expression.
/// Used for immutable expression transformations.
/// </summary>
public interface IExpressionTransformer
{
    /// <summary>Transforms a literal expression.</summary>
    Expression Visit(LiteralExpression expression);

    /// <summary>Transforms a variable expression.</summary>
    Expression Visit(VariableExpression expression);

    /// <summary>Transforms a constant expression.</summary>
    Expression Visit(ConstantExpression expression);

    /// <summary>Transforms a binary expression.</summary>
    Expression Visit(BinaryExpression expression);

    /// <summary>Transforms a unary expression.</summary>
    Expression Visit(UnaryExpression expression);

    /// <summary>Transforms a function call expression.</summary>
    Expression Visit(FunctionCallExpression expression);

    /// <summary>Transforms a lambda expression.</summary>
    Expression Visit(LambdaExpression expression);

    /// <summary>Transforms a parameter expression.</summary>
    Expression Visit(ParameterExpression expression);

    /// <summary>Transforms an equation expression.</summary>
    Expression Visit(EquationExpression expression);

    /// <summary>Transforms a piecewise expression.</summary>
    Expression Visit(PiecewiseExpression expression);

    /// <summary>Transforms a conditional expression.</summary>
    Expression Visit(ConditionalExpression expression);

    /// <summary>Transforms a tuple expression.</summary>
    Expression Visit(TupleExpression expression);

    /// <summary>Transforms a vector expression.</summary>
    Expression Visit(VectorExpression expression);

    /// <summary>Transforms a matrix expression.</summary>
    Expression Visit(MatrixExpression expression);

    /// <summary>Transforms a tensor expression.</summary>
    Expression Visit(TensorExpression expression);

    /// <summary>Transforms an index expression.</summary>
    Expression Visit(IndexExpression expression);

    /// <summary>Transforms a slice expression.</summary>
    Expression Visit(SliceExpression expression);

    /// <summary>Transforms a derivative expression.</summary>
    Expression Visit(DerivativeExpression expression);

    /// <summary>Transforms an integral expression.</summary>
    Expression Visit(IntegralExpression expression);

    /// <summary>Transforms a summation expression.</summary>
    Expression Visit(SummationExpression expression);

    /// <summary>Transforms a product expression.</summary>
    Expression Visit(ProductExpression expression);

    /// <summary>Transforms a limit expression.</summary>
    Expression Visit(LimitExpression expression);

    /// <summary>Transforms a factorial expression.</summary>
    Expression Visit(FactorialExpression expression);

    /// <summary>Transforms a range expression.</summary>
    Expression Visit(RangeExpression expression);

    /// <summary>Transforms an interval expression.</summary>
    Expression Visit(IntervalExpression expression);

    /// <summary>Transforms a set expression.</summary>
    Expression Visit(SetExpression expression);

    /// <summary>Transforms a complex expression.</summary>
    Expression Visit(ComplexExpression expression);

    /// <summary>Transforms a polynomial expression.</summary>
    Expression Visit(PolynomialExpression expression);

    /// <summary>Transforms a boolean expression.</summary>
    Expression Visit(BooleanExpression expression);

    /// <summary>Transforms a relation expression.</summary>
    Expression Visit(RelationExpression expression);

    /// <summary>Transforms an assignment expression.</summary>
    Expression Visit(AssignmentExpression expression);

    /// <summary>Transforms a composition expression.</summary>
    Expression Visit(CompositionExpression expression);

    /// <summary>Transforms an identity expression.</summary>
    Expression Visit(IdentityExpression expression);

    /// <summary>Transforms a null expression.</summary>
    Expression Visit(NullExpression expression);

    /// <summary>Transforms an annotated expression.</summary>
    Expression Visit(AnnotatedExpression expression);
}
