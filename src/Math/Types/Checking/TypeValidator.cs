namespace MathVerse.Math.Types.Checking;

/// <summary>Validates mathematical type constraints and operations.</summary>
public sealed class TypeValidator
{
    private readonly List<TypeCheckDiagnostic> _diagnostics = new();

    /// <summary>Diagnostics produced during validation.</summary>
    public IReadOnlyList<TypeCheckDiagnostic> Diagnostics => _diagnostics;

    /// <summary>Validates that a binary operation is well-typed.</summary>
    public bool ValidateBinaryOperation(MathType left, string op, MathType right)
    {
        var checker = new TypeCompatibility();

        if (!checker.IsCompatibleForOperator(left, right, op))
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error,
                TypeDiagnosticCode.IncompatibleTypes,
                $"Operator '{op}' cannot be applied to '{left.Name}' and '{right.Name}'"));
            return false;
        }

        if (op is "/" or "%" && right is ScalarType rs && !right.Equals(IntegerType.Instance))
        {
            if (right.Equals(IntegerType.Instance))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Warning,
                    TypeDiagnosticCode.IncompatibleTypes,
                    "Division by integer may produce non-integer result"));
            }
        }

        return true;
    }

    /// <summary>Validates that a unary operation is well-typed.</summary>
    public bool ValidateUnaryOperation(string op, MathType operand)
    {
        if (op is "!" or "¬")
        {
            if (!operand.Equals(BooleanType.Instance))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Logical NOT requires boolean, got '{operand.Name}'"));
                return false;
            }
        }

        if (op is "-" or "+")
        {
            if (operand is not ScalarType && operand is not VectorType)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Unary '{op}' requires numeric type, got '{operand.Name}'"));
                return false;
            }
        }

        if (op is "++" or "--")
        {
            if (operand is not IntegerType && operand is not TypedInteger)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Increment/decrement requires integer, got '{operand.Name}'"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates function call arguments.</summary>
    public bool ValidateFunctionCall(string functionName, MathType funcType,
        IReadOnlyList<MathType> argTypes)
    {
        if (funcType is not FunctionType ft)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error,
                TypeDiagnosticCode.IncompatibleTypes,
                $"'{functionName}' is not callable"));
            return false;
        }

        if (ft.Arity != argTypes.Count)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error,
                TypeDiagnosticCode.IncompatibleTypes,
                $"'{functionName}' expects {ft.Arity} arguments, got {argTypes.Count}"));
            return false;
        }

        var compat = new TypeCompatibility();
        for (int i = 0; i < ft.Arity; i++)
        {
            if (!compat.IsAssignableFrom(ft.ParameterTypes[i], argTypes[i]))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Argument {i + 1} of '{functionName}': expected '{ft.ParameterTypes[i].Name}', got '{argTypes[i].Name}'"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates vector operations.</summary>
    public bool ValidateVectorOperation(string op, VectorType left, VectorType right)
    {
        if (op is "+" or "-" or "==" or "!=")
        {
            if (!left.ElementType.Equals(right.ElementType))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Vector element type mismatch: '{left.ElementType.Name}' vs '{right.ElementType.Name}'"));
                return false;
            }

            if (left.Dimension.HasValue && right.Dimension.HasValue &&
                left.Dimension.Value != right.Dimension.Value)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.InvalidTensorDimensions,
                    $"Vector dimension mismatch: {left.Dimension.Value} vs {right.Dimension.Value}"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates matrix multiplication.</summary>
    public bool ValidateMatrixMultiplication(MatrixType left, MatrixType right)
    {
        if (left.Columns.HasValue && right.Rows.HasValue &&
            left.Columns.Value != right.Rows.Value)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error,
                TypeDiagnosticCode.InvalidMatrixMultiplication,
                $"Cannot multiply {left.Rows ?? 0}×{left.Columns ?? 0} by {right.Rows ?? 0}×{right.Columns ?? 0}: inner dimensions must match"));
            return false;
        }

        return true;
    }

    /// <summary>Validates tensor operations.</summary>
    public bool ValidateTensorOperation(string op, TensorType left, TensorType right)
    {
        if (op is "+" or "-")
        {
            if (left.Rank != right.Rank)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.InvalidTensorDimensions,
                    $"Tensor rank mismatch: {left.Rank} vs {right.Rank}"));
                return false;
            }

            for (int i = 0; i < left.Rank; i++)
            {
                if (left.Shape[i] != right.Shape[i])
                {
                    _diagnostics.Add(new TypeCheckDiagnostic(
                        TypeCheckSeverity.Error,
                        TypeDiagnosticCode.InvalidTensorDimensions,
                        $"Tensor dimension mismatch at axis {i}: {left.Shape[i]} vs {right.Shape[i]}"));
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Validates equation types.</summary>
    public bool ValidateEquation(MathType left, string op, MathType right)
    {
        if (op is "=" or "==" or "!=")
        {
            var compat = new TypeCompatibility();
            if (!compat.IsAssignableFrom(left, right) && !compat.IsAssignableFrom(right, left))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Warning,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Comparing incompatible types '{left.Name}' and '{right.Name}'"));
            }
        }

        if (op is "<" or ">" or "<=" or ">=")
        {
            if (left is not ScalarType || right is not ScalarType)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Ordering requires scalar types, got '{left.Name}' and '{right.Name}'"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates set operations.</summary>
    public bool ValidateSetOperation(string op, SetType left, SetType right)
    {
        if (op is "∪" or "∩" or "Δ")
        {
            if (!left.ElementType.Equals(right.ElementType))
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Set element type mismatch: '{left.ElementType.Name}' vs '{right.ElementType.Name}'"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates tuple operations.</summary>
    public bool ValidateTupleOperation(string op, TupleType left, TupleType right)
    {
        if (op is "==" or "!=")
        {
            if (left.Arity != right.Arity)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Tuple arity mismatch: {left.Arity} vs {right.Arity}"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates lambda expressions.</summary>
    public bool ValidateLambda(IReadOnlyList<MathType> paramTypes, MathType returnType)
    {
        foreach (var pt in paramTypes)
        {
            if (pt is ErrorType)
            {
                _diagnostics.Add(new TypeCheckDiagnostic(
                    TypeCheckSeverity.Error,
                    TypeDiagnosticCode.UnresolvedType,
                    "Lambda parameter has error type"));
                return false;
            }
        }

        return true;
    }

    /// <summary>Validates derivative operations.</summary>
    public bool ValidateDerivative(MathType bodyType, string variable)
    {
        if (bodyType is not ScalarType && bodyType is not FunctionType)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Warning,
                TypeDiagnosticCode.IncompatibleTypes,
                $"Differentiation of '{bodyType.Name}' may not be well-defined"));
        }

        return true;
    }

    /// <summary>Validates summation/product bounds.</summary>
    public bool ValidateIterationBounds(MathType lower, MathType upper)
    {
        if (lower is not IntegerType && lower is not TypedInteger)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Warning,
                TypeDiagnosticCode.IncompatibleTypes,
                $"Lower bound should be integer, got '{lower.Name}'"));
        }

        if (upper is not IntegerType && upper is not TypedInteger)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Warning,
                TypeDiagnosticCode.IncompatibleTypes,
                $"Upper bound should be integer, got '{upper.Name}'"));
        }

        return true;
    }

    /// <summary>Clears all diagnostics.</summary>
    public void Clear() => _diagnostics.Clear();
}
