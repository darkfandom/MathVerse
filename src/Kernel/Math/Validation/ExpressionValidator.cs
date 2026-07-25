namespace MathVerse.Math.Validation;

using MathVerse.Math.Expressions;
using MathVerse.Math.Visitors;

/// <summary>
/// Result of expression validation.
/// </summary>
public sealed class ExpressionValidationResult
{
    /// <summary>Initializes a validation result.</summary>
    public ExpressionValidationResult(bool isValid, IReadOnlyList<ExpressionValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>Gets whether the expression is valid.</summary>
    public bool IsValid { get; }

    /// <summary>Gets the validation errors.</summary>
    public IReadOnlyList<ExpressionValidationError> Errors { get; }

    /// <summary>Gets a success result.</summary>
    public static ExpressionValidationResult Success() =>
        new(true, []);

    /// <summary>Gets a failure result with the specified errors.</summary>
    public static ExpressionValidationResult Failure(IReadOnlyList<ExpressionValidationError> errors) =>
        new(false, errors);

    /// <summary>Gets a failure result with a single error.</summary>
    public static ExpressionValidationResult Failure(ExpressionValidationError error) =>
        new(false, [error]);
}

/// <summary>
/// Represents a validation error on an expression.
/// </summary>
public sealed record ExpressionValidationError
{
    /// <summary>Gets the error code.</summary>
    public required string Code { get; init; }

    /// <summary>Gets the error message.</summary>
    public required string Message { get; init; }

    /// <summary>Gets the expression node ID where the error occurred.</summary>
    public int? NodeId { get; init; }

    /// <summary>Gets the expression kind where the error occurred.</summary>
    public ExpressionKind? ExpressionKind { get; init; }
}

/// <summary>
/// Validates expression trees for structural correctness.
/// </summary>
public sealed class ExpressionValidator
{
    private readonly List<ExpressionValidationError> _errors = [];

    /// <summary>Validates the expression and returns a result.</summary>
    public ExpressionValidationResult Validate(Expression expression)
    {
        _errors.Clear();
        expression.Accept(new ValidationWalker(this));
        return _errors.Count > 0
            ? ExpressionValidationResult.Failure(_errors)
            : ExpressionValidationResult.Success();
    }

    private void AddError(string code, string message, Expression expression)
    {
        _errors.Add(new ExpressionValidationError
        {
            Code = code,
            Message = message,
            NodeId = expression.NodeId,
            ExpressionKind = expression.Kind
        });
    }

    private sealed class ValidationWalker : ExpressionWalker
    {
        private readonly ExpressionValidator _validator;

        public ValidationWalker(ExpressionValidator validator)
        {
            _validator = validator;
        }

        public override void Visit(BinaryExpression expression)
        {
            base.Visit(expression);
            if (expression.Left.Kind == ExpressionKind.Null || expression.Right.Kind == ExpressionKind.Null)
                _validator.AddError("MV_EXNullOperand", "Binary expression has null operand.", expression);
        }

        public override void Visit(UnaryExpression expression)
        {
            base.Visit(expression);
            if (expression.Operand.Kind == ExpressionKind.Null)
                _validator.AddError("MV_EXNullOperand", "Unary expression has null operand.", expression);
        }

        public override void Visit(FunctionCallExpression expression)
        {
            base.Visit(expression);
            if (expression.Arguments.Count == 0 && expression.Name != "identity")
                _validator.AddError("MV_EXEmptyArgs", $"Function '{expression.Name}' has no arguments.", expression);

            foreach (var arg in expression.Arguments)
            {
                if (arg.Kind == ExpressionKind.Null)
                    _validator.AddError("MV_EXNullArg", $"Function '{expression.Name}' has a null argument.", expression);
            }
        }

        public override void Visit(IntegralExpression expression)
        {
            base.Visit(expression);
            if (expression.IsDefinite)
            {
                if (expression.LowerBound!.Kind == ExpressionKind.Null)
                    _validator.AddError("MV_EXNullBound", "Definite integral has null lower bound.", expression);
                if (expression.UpperBound!.Kind == ExpressionKind.Null)
                    _validator.AddError("MV_EXNullBound", "Definite integral has null upper bound.", expression);
            }
        }

        public override void Visit(DerivativeExpression expression)
        {
            base.Visit(expression);
            if (expression.Order < 1)
                _validator.AddError("MV_EXInvalidOrder", "Derivative order must be >= 1.", expression);
        }

        public override void Visit(MatrixExpression expression)
        {
            base.Visit(expression);
            if (expression.RowCount == 0)
                _validator.AddError("MV_EXEmptyMatrix", "Matrix has no rows.", expression);

            var expectedCols = expression.ColumnCount;
            foreach (var row in expression.Rows)
            {
                if (row is VectorExpression v && v.Dimension != expectedCols)
                {
                    _validator.AddError("MV_EXInconsistentDimensions", $"Matrix row has {v.Dimension} columns but expected {expectedCols}.", expression);
                    break;
                }
            }
        }

        public override void Visit(VectorExpression expression)
        {
            base.Visit(expression);
            if (expression.Dimension == 0)
                _validator.AddError("MV_EXEmptyVector", "Vector has no components.", expression);
        }

        public override void Visit(SetExpression expression)
        {
            base.Visit(expression);
        }

        public override void Visit(LimitExpression expression)
        {
            base.Visit(expression);
        }

        public override void Visit(SummationExpression expression)
        {
            base.Visit(expression);
        }

        public override void Visit(ProductExpression expression)
        {
            base.Visit(expression);
        }
    }
}
