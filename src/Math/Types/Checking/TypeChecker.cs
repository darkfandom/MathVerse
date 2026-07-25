namespace MathVerse.Math.Types.Checking;

/// <summary>Type-checks expressions and validates mathematical operations.</summary>
public sealed class TypeChecker
{
    private readonly TypeValidator _validator = new();
    private readonly TypeCompatibility _compatibility = new();
    private readonly TypeInferenceEngine _inference = new();
    private readonly List<TypeCheckDiagnostic> _diagnostics = new();

    /// <summary>All diagnostics produced during checking.</summary>
    public IReadOnlyList<TypeCheckDiagnostic> Diagnostics => _diagnostics;

    /// <summary>Type-checks an expression and returns its type.</summary>
    public TypeCheckResult Check(Expression expression)
    {
        _diagnostics.Clear();
        var result = _inference.Infer(expression);

        foreach (var diag in result.Diagnostics)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error, diag.Code, diag.Message));
        }

        return new TypeCheckResult(result.Type, result.IsSuccess,
            _diagnostics.ToList());
    }

    /// <summary>Type-checks a bound expression.</summary>
    public TypeCheckResult CheckBound(BoundExpression expression)
    {
        _diagnostics.Clear();
        var result = _inference.InferBound(expression);

        foreach (var diag in result.Diagnostics)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error, diag.Code, diag.Message));
        }

        return new TypeCheckResult(result.Type, result.IsSuccess,
            _diagnostics.ToList());
    }

    /// <summary>Type-checks a binary operation.</summary>
    public TypeCheckResult CheckBinary(MathType left, string op, MathType right)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateBinaryOperation(left, op, right);
        _diagnostics.AddRange(_validator.Diagnostics);

        var resultType = op switch
        {
            "+" or "-" or "*" or "/" or "%" or "^" => TypeRules.Promote(left, right),
            "==" or "!=" or "<" or ">" or "<=" or ">=" => BooleanType.Instance,
            "&&" or "||" => BooleanType.Instance,
            _ => UnknownType.Instance,
        };

        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks a unary operation.</summary>
    public TypeCheckResult CheckUnary(string op, MathType operand)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateUnaryOperation(op, operand);
        _diagnostics.AddRange(_validator.Diagnostics);

        var resultType = op switch
        {
            "!" or "¬" => BooleanType.Instance,
            "-" or "+" => operand,
            _ => operand,
        };

        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks a function call.</summary>
    public TypeCheckResult CheckFunctionCall(string name, MathType funcType,
        IReadOnlyList<MathType> argTypes)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateFunctionCall(name, funcType, argTypes);
        _diagnostics.AddRange(_validator.Diagnostics);

        var resultType = funcType is FunctionType ft ? ft.ReturnType : UnknownType.Instance;
        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks an assignment.</summary>
    public TypeCheckResult CheckAssignment(MathType targetType, MathType valueType)
    {
        _diagnostics.Clear();
        var compatible = _compatibility.IsAssignableFrom(targetType, valueType);

        if (!compatible)
        {
            _diagnostics.Add(new TypeCheckDiagnostic(
                TypeCheckSeverity.Error,
                TypeDiagnosticCode.IncompatibleTypes,
                $"Cannot assign '{valueType.Name}' to '{targetType.Name}'"));
        }

        return new TypeCheckResult(targetType, compatible, _diagnostics.ToList());
    }

    /// <summary>Type-checks matrix multiplication.</summary>
    public TypeCheckResult CheckMatrixMultiply(MatrixType left, MatrixType right)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateMatrixMultiplication(left, right);
        _diagnostics.AddRange(_validator.Diagnostics);

        var resultType = valid ? TypeRules.MatrixMultiplyResult(left, right) : ErrorType.Instance;
        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks vector operations.</summary>
    public TypeCheckResult CheckVectorOp(string op, VectorType left, VectorType right)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateVectorOperation(op, left, right);
        _diagnostics.AddRange(_validator.Diagnostics);

        var resultType = op switch
        {
            "+" or "-" => TypeRules.VectorAddResult(left, right),
            "==" or "!=" => BooleanType.Instance,
            _ => UnknownType.Instance,
        };

        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks a lambda expression.</summary>
    public TypeCheckResult CheckLambda(IReadOnlyList<MathType> paramTypes, MathType returnType)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateLambda(paramTypes, returnType);
        _diagnostics.AddRange(_validator.Diagnostics);

        var funcType = new FunctionType(paramTypes, returnType);
        return new TypeCheckResult(funcType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks an equation.</summary>
    public TypeCheckResult CheckEquation(MathType left, string op, MathType right)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateEquation(left, op, right);
        _diagnostics.AddRange(_validator.Diagnostics);

        var eqType = new EquationType(left, right, op);
        return new TypeCheckResult(eqType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks a derivative.</summary>
    public TypeCheckResult CheckDerivative(MathType bodyType, string variable)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateDerivative(bodyType, variable);
        _diagnostics.AddRange(_validator.Diagnostics);

        return new TypeCheckResult(bodyType, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks summation/product bounds.</summary>
    public TypeCheckResult CheckIterationBounds(MathType lower, MathType upper)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateIterationBounds(lower, upper);
        _diagnostics.AddRange(_validator.Diagnostics);

        return new TypeCheckResult(lower, valid, _diagnostics.ToList());
    }

    /// <summary>Type-checks a set operation.</summary>
    public TypeCheckResult CheckSetOp(string op, SetType left, SetType right)
    {
        _diagnostics.Clear();
        var valid = _validator.ValidateSetOperation(op, left, right);
        _diagnostics.AddRange(_validator.Diagnostics);

        MathType resultType = op switch
        {
            "∪" or "∩" or "Δ" => left,
            _ => BooleanType.Instance,
        };

        return new TypeCheckResult(resultType, valid, _diagnostics.ToList());
    }
}
