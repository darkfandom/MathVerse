namespace MathVerse.Math.Types.Inference;

/// <summary>Result of type inference.</summary>
public sealed class InferenceResult
{
    /// <summary>The inferred type.</summary>
    public MathType Type { get; }

    /// <summary>The final substitution.</summary>
    public TypeSubstitution Substitution { get; }

    /// <summary>Diagnostics from inference.</summary>
    public IReadOnlyList<TypeDiagnostic> Diagnostics { get; }

    /// <summary>Whether inference was successful.</summary>
    public bool IsSuccess => Diagnostics.Count == 0;

    /// <summary>Creates an inference result.</summary>
    public InferenceResult(MathType type, TypeSubstitution substitution,
        IReadOnlyList<TypeDiagnostic> diagnostics)
    {
        Type = type;
        Substitution = substitution;
        Diagnostics = diagnostics;
    }
}

/// <summary>Hindley-Milner style type inference engine.</summary>
public sealed class TypeInferenceEngine
{
    private readonly ConstraintSolver _solver = new();
    private readonly TypeEnvironment _globalEnv;

    /// <summary>Creates a type inference engine with default built-in types.</summary>
    public TypeInferenceEngine()
    {
        _globalEnv = CreateGlobalEnvironment();
    }

    /// <summary>Infers the type of an expression.</summary>
    public InferenceResult Infer(Expression expression)
    {
        return Infer(expression, _globalEnv);
    }

    /// <summary>Infers the type of an expression within a given environment.</summary>
    public InferenceResult Infer(Expression expression, TypeEnvironment environment)
    {
        var context = new InferenceContext();
        var inferred = InferExpression(expression, environment, context);
        var substitution = _solver.Solve(context.Constraints);
        var resolved = substitution.ApplyTo(inferred);

        var diags = new List<TypeDiagnostic>(_solver.Diagnostics);
        return new InferenceResult(resolved, substitution, diags);
    }

    /// <summary>Infers the type of a bound expression.</summary>
    public InferenceResult InferBound(BoundExpression expression)
    {
        var context = new InferenceContext();
        var inferred = InferBoundExpression(expression, _globalEnv, context);
        var substitution = _solver.Solve(context.Constraints);
        var resolved = substitution.ApplyTo(inferred);

        var diags = new List<TypeDiagnostic>(_solver.Diagnostics);
        return new InferenceResult(resolved, substitution, diags);
    }

    /// <summary>Infers function return type from parameter types and body.</summary>
    public InferenceResult InferFunction(
        IReadOnlyList<(string Name, MathType Type)> parameters,
        Expression body)
    {
        var env = _globalEnv;
        var context = new InferenceContext();

        foreach (var (name, type) in parameters)
        {
            env = env.Bind(name, type);
        }

        var returnType = InferExpression(body, env, context);
        var paramTypes = parameters.Select(p => p.Type).ToList();
        var funcType = new FunctionType(paramTypes, returnType);
        var substitution = _solver.Solve(context.Constraints);
        var resolved = substitution.ApplyTo(funcType);

        var diags = new List<TypeDiagnostic>(_solver.Diagnostics);
        return new InferenceResult(resolved, substitution, diags);
    }

    /// <summary>Infers a lambda expression type.</summary>
    public InferenceResult InferLambda(IReadOnlyList<string> parameters, Expression body)
    {
        var context = new InferenceContext();
        var env = _globalEnv;
        var paramTypes = new List<MathType>();

        foreach (var name in parameters)
        {
            var tv = context.FreshVariable(name);
            paramTypes.Add(tv);
            env = env.Bind(name, tv);
        }

        var returnType = InferExpression(body, env, context);
        var funcType = new FunctionType(paramTypes, returnType);
        var substitution = _solver.Solve(context.Constraints);
        var resolved = substitution.ApplyTo(funcType);

        var diags = new List<TypeDiagnostic>(_solver.Diagnostics);
        return new InferenceResult(resolved, substitution, diags);
    }

    private MathType InferExpression(Expression expression, TypeEnvironment env,
        InferenceContext context)
    {
        switch (expression)
        {
            case LiteralExpression lit:
                return InferLiteral(lit);

            case VariableExpression var:
                return InferVariable(var, env, context);

            case ConstantExpression con:
                return InferKnownConstant(con);

            case BinaryExpression bin:
                return InferBinary(bin, env, context);

            case UnaryExpression unary:
                return InferUnary(unary, env, context);

            case FunctionCallExpression func:
                return InferFunctionCall(func, env, context);

            case AssignmentExpression assign:
                return InferAssignment(assign, env, context);

            case ConditionalExpression cond:
                return InferConditional(cond, env, context);

            case LambdaExpression lambda:
                return InferLambdaBody(lambda, env, context);

            case PiecewiseExpression piecewise:
                return InferPiecewise(piecewise, env, context);

            case SummationExpression sum:
                return InferSummation(sum, env, context);

            case ProductExpression prod:
                return InferProduct(prod, env, context);

            case DerivativeExpression deriv:
                return InferDerivative(deriv, env, context);

            case IntegralExpression integ:
                return InferIntegral(integ, env, context);

            case LimitExpression limit:
                return InferLimit(limit, env, context);

            case VectorExpression vec:
                return InferVector(vec, env, context);

            case MatrixExpression mat:
                return InferMatrix(mat, env, context);

            case RangeExpression range:
                return InferRange(range, env, context);

            default:
                return UnknownType.Instance;
        }
    }

    private MathType InferBoundExpression(BoundExpression expression, TypeEnvironment env,
        InferenceContext context)
    {
        switch (expression)
        {
            case BoundLiteralExpression lit:
                return InferLiteralValue(lit.Value);

            case BoundConstantExpression con:
                return InferBoundConstant(con);

            case BoundVariableExpression var:
                return InferVariableName(var.Symbol.Name, env, context);

            case BoundBinaryExpression bin:
                return InferBoundBinary(bin, env, context);

            case BoundUnaryExpression unary:
                return InferBoundUnary(unary, env, context);

            case BoundFunctionCallExpression func:
                return InferBoundFunctionCall(func, env, context);

            case BoundAssignmentExpression assign:
                return InferBoundAssignment(assign, env, context);

            default:
                return UnknownType.Instance;
        }
    }

    private MathType InferLiteral(LiteralExpression lit)
    {
        return InferLiteralValue(lit.Value);
    }

    private MathType InferLiteralValue(double value)
    {
        if (value == System.Math.Floor(value) && System.Math.Abs(value) < int.MaxValue)
            return IntegerType.Instance;
        return RealType.Instance;
    }

    private MathType InferVariable(VariableExpression var, TypeEnvironment env,
        InferenceContext context)
    {
        return InferVariableName(var.Name, env, context);
    }

    private MathType InferVariableName(string name, TypeEnvironment env,
        InferenceContext context)
    {
        var type = env.Lookup(name);
        if (type is not null) return type;

        var tv = context.FreshVariable(name);
        return tv;
    }

    private MathType InferKnownConstant(ConstantExpression con)
    {
        var name = con.Name;
        if (name is "π" or "pi" or "e" or "τ" or "tau" or "φ" or "phi")
            return RealType.Instance;
        if (name is "i")
            return ComplexType.Instance;
        return RealType.Instance;
    }

    private MathType InferBoundConstant(BoundConstantExpression con)
    {
        return RealType.Instance;
    }

    private MathType InferBinary(BinaryExpression bin, TypeEnvironment env,
        InferenceContext context)
    {
        var left = InferExpression(bin.Left, env, context);
        var right = InferExpression(bin.Right, env, context);
        var op = bin.Operator.Symbol;

        if (op is "+" or "-" or "*" or "/" or "^" or "%")
        {
            var tv = context.FreshVariable("binResult");
            context.AddEquality(left, right, $"binary {op}");
            context.AddEquality(tv, left, $"binary {op} result");
            return tv;
        }

        if (op is "=" or "==" or "!=" or "<" or ">" or "<=" or ">=")
        {
            context.AddEquality(left, right, $"comparison {op}");
            return BooleanType.Instance;
        }

        if (op is "&&" or "||" or "∧" or "∨")
        {
            context.AddEquality(left, BooleanType.Instance, $"logical {op} left");
            context.AddEquality(right, BooleanType.Instance, $"logical {op} right");
            return BooleanType.Instance;
        }

        return UnknownType.Instance;
    }

    private MathType InferBoundBinary(BoundBinaryExpression bin, TypeEnvironment env,
        InferenceContext context)
    {
        var left = InferBoundExpression(bin.Left, env, context);
        var right = InferBoundExpression(bin.Right, env, context);
        var op = bin.Operator.Symbol;

        if (op is "+" or "-" or "*" or "/" or "^" or "%")
        {
            var tv = context.FreshVariable("binResult");
            context.AddEquality(left, right, $"binary {op}");
            context.AddEquality(tv, left, $"binary {op} result");
            return tv;
        }

        if (op is "=" or "==" or "!=" or "<" or ">" or "<=" or ">=")
        {
            context.AddEquality(left, right, $"comparison {op}");
            return BooleanType.Instance;
        }

        if (op is "&&" or "||" or "∧" or "∨")
        {
            context.AddEquality(left, BooleanType.Instance, $"logical {op} left");
            context.AddEquality(right, BooleanType.Instance, $"logical {op} right");
            return BooleanType.Instance;
        }

        return UnknownType.Instance;
    }

    private MathType InferUnary(UnaryExpression unary, TypeEnvironment env,
        InferenceContext context)
    {
        var operand = InferExpression(unary.Operand, env, context);
        var op = unary.Operator.Symbol;

        if (op is "!" or "¬")
        {
            return BooleanType.Instance;
        }

        return operand;
    }

    private MathType InferBoundUnary(BoundUnaryExpression unary, TypeEnvironment env,
        InferenceContext context)
    {
        var operand = InferBoundExpression(unary.Operand, env, context);
        var op = unary.Operator.Symbol;

        if (op is "!" or "¬")
        {
            return BooleanType.Instance;
        }

        return operand;
    }

    private MathType InferFunctionCall(FunctionCallExpression func, TypeEnvironment env,
        InferenceContext context)
    {
        var funcType = env.Lookup(func.Name);
        var argTypes = func.Arguments.Select(a => InferExpression(a, env, context)).ToList();

        if (funcType is FunctionType ft)
        {
            if (ft.Arity == argTypes.Count)
            {
                for (int i = 0; i < ft.Arity; i++)
                {
                    context.AddEquality(ft.ParameterTypes[i], argTypes[i], $"arg {i} of {func.Name}");
                }
                return ft.ReturnType;
            }
        }

        if (funcType is not null)
        {
            return funcType;
        }

        return InferBuiltinFunction(func.Name, argTypes, context);
    }

    private MathType InferBoundFunctionCall(BoundFunctionCallExpression func, TypeEnvironment env,
        InferenceContext context)
    {
        var argTypes = func.Arguments.Select(a => InferBoundExpression(a, env, context)).ToList();
        return InferBuiltinFunction(func.Function.Name, argTypes, context);
    }

    private MathType InferBuiltinFunction(string name, List<MathType> argTypes,
        InferenceContext context)
    {
        var singleArgFuncs = new HashSet<string>
        {
            "sin", "cos", "tan", "cot", "sec", "csc",
            "asin", "acos", "atan", "acot", "asec", "acsc",
            "sinh", "cosh", "tanh", "coth", "sech", "csch",
            "sqrt", "cbrt", "abs", "floor", "ceil", "round",
            "exp", "ln", "log", "log2", "log10",
            "sign", "fact", "gamma", "erf", "erfc",
        };

        if (singleArgFuncs.Contains(name) && argTypes.Count == 1)
        {
            return RealType.Instance;
        }

        if (name is "atan2" or "pow" or "logbase" && argTypes.Count == 2)
        {
            return RealType.Instance;
        }

        if (name is "min" or "max" or "gcd" or "lcm" && argTypes.Count >= 2)
        {
            if (argTypes.All(a => a is IntegerType or TypedInteger))
                return IntegerType.Instance;
            return RealType.Instance;
        }

        if (name is "dot" && argTypes.Count == 2)
        {
            return RealType.Instance;
        }

        if (name is "cross" && argTypes.Count == 2)
        {
            if (argTypes[0] is VectorType vt)
                return vt;
            return new VectorType(RealType.Instance);
        }

        if (name is "det" && argTypes.Count == 1)
        {
            return RealType.Instance;
        }

        if (name is "transpose" && argTypes.Count == 1)
        {
            if (argTypes[0] is MatrixType mt)
                return new MatrixType(mt.ElementType, mt.Columns, mt.Rows);
            return new MatrixType(RealType.Instance);
        }

        if (name is "norm" && argTypes.Count >= 1)
        {
            return RealType.Instance;
        }

        return UnknownType.Instance;
    }

    private MathType InferAssignment(AssignmentExpression assign, TypeEnvironment env,
        InferenceContext context)
    {
        var valueType = InferExpression(assign.Value, env, context);

        if (assign.Target is VariableExpression targetVar)
        {
            var existing = env.Lookup(targetVar.Name);
            if (existing is not null)
            {
                context.AddEquality(existing, valueType, $"assignment to {targetVar.Name}");
                return existing;
            }
            return valueType;
        }

        return valueType;
    }

    private MathType InferBoundAssignment(BoundAssignmentExpression assign, TypeEnvironment env,
        InferenceContext context)
    {
        var valueType = InferBoundExpression(assign.Value, env, context);
        return valueType;
    }

    private MathType InferConditional(ConditionalExpression cond, TypeEnvironment env,
        InferenceContext context)
    {
        var conditionType = InferExpression(cond.Condition, env, context);
        context.AddEquality(conditionType, BooleanType.Instance, "conditional condition");

        var thenType = InferExpression(cond.ThenBranch, env, context);

        if (cond.ElseBranch is Expression elseBranch)
        {
            var elseType = InferExpression(elseBranch, env, context);
            context.AddEquality(thenType, elseType, "conditional branches");
        }

        return thenType;
    }

    private MathType InferLambdaBody(LambdaExpression lambda, TypeEnvironment env,
        InferenceContext context)
    {
        var paramTypes = new List<MathType>();
        foreach (var param in lambda.Parameters)
        {
            var tv = context.FreshVariable(param.Name);
            paramTypes.Add(tv);
            env = env.Bind(param.Name, tv);
        }

        var bodyType = InferExpression(lambda.Body, env, context);
        return new FunctionType(paramTypes, bodyType);
    }

    private MathType InferPiecewise(PiecewiseExpression piecewise, TypeEnvironment env,
        InferenceContext context)
    {
        MathType? resultType = null;

        foreach (var case_ in piecewise.Cases)
        {
            var condType = InferExpression(case_.Condition, env, context);
            context.AddEquality(condType, BooleanType.Instance, "piecewise condition");

            var caseType = InferExpression(case_.Value, env, context);
            if (resultType is not null)
            {
                context.AddEquality(resultType, caseType, "piecewise branch");
            }
            else
            {
                resultType = caseType;
            }
        }

        return resultType ?? UnknownType.Instance;
    }

    private MathType InferSummation(SummationExpression sum, TypeEnvironment env,
        InferenceContext context)
    {
        InferExpression(sum.Variable, env, context);
        InferExpression(sum.LowerBound, env, context);
        InferExpression(sum.UpperBound, env, context);
        var bodyType = InferExpression(sum.Body, env, context);
        return bodyType;
    }

    private MathType InferProduct(ProductExpression prod, TypeEnvironment env,
        InferenceContext context)
    {
        InferExpression(prod.Variable, env, context);
        InferExpression(prod.LowerBound, env, context);
        InferExpression(prod.UpperBound, env, context);
        var bodyType = InferExpression(prod.Body, env, context);
        return bodyType;
    }

    private MathType InferDerivative(DerivativeExpression deriv, TypeEnvironment env,
        InferenceContext context)
    {
        var bodyType = InferExpression(deriv.Function, env, context);
        InferExpression(deriv.Variable, env, context);
        return bodyType;
    }

    private MathType InferIntegral(IntegralExpression integ, TypeEnvironment env,
        InferenceContext context)
    {
        var bodyType = InferExpression(integ.Integrand, env, context);
        InferExpression(integ.Variable, env, context);
        if (integ.LowerBound is Expression lb) InferExpression(lb, env, context);
        if (integ.UpperBound is Expression ub) InferExpression(ub, env, context);
        return bodyType;
    }

    private MathType InferLimit(LimitExpression limit, TypeEnvironment env,
        InferenceContext context)
    {
        var bodyType = InferExpression(limit.Body, env, context);
        InferExpression(limit.Variable, env, context);
        InferExpression(limit.Target, env, context);
        return bodyType;
    }

    private MathType InferVector(VectorExpression vec, TypeEnvironment env,
        InferenceContext context)
    {
        var componentTypes = vec.Components.Select(c => InferExpression(c, env, context)).ToList();

        if (componentTypes.Count == 0)
            return new VectorType(UnknownType.Instance, 0);

        var elementType = componentTypes[0];
        foreach (var ct in componentTypes.Skip(1))
        {
            context.AddEquality(elementType, ct, "vector component");
        }

        return new VectorType(elementType, componentTypes.Count);
    }

    private MathType InferMatrix(MatrixExpression mat, TypeEnvironment env,
        InferenceContext context)
    {
        if (mat.Rows.Count == 0)
            return new MatrixType(UnknownType.Instance, 0, 0);

        MathType? elemType = null;
        foreach (var rowExpr in mat.Rows)
        {
            if (rowExpr is VectorExpression rowVec)
            {
                foreach (var cell in rowVec.Components)
                {
                    var cellType = InferExpression(cell, env, context);
                    if (elemType is null)
                        elemType = cellType;
                    else
                        context.AddEquality(elemType, cellType, "matrix element");
                }
            }
        }

        return new MatrixType(elemType ?? UnknownType.Instance, mat.RowCount, mat.ColumnCount);
    }

    private MathType InferRange(RangeExpression range, TypeEnvironment env,
        InferenceContext context)
    {
        var startType = InferExpression(range.Start, env, context);
        var endType = InferExpression(range.End, env, context);
        context.AddEquality(startType, endType, "range bounds");
        if (range.Step is Expression step) InferExpression(step, env, context);
        return new SequenceType(startType);
    }

    private static TypeEnvironment CreateGlobalEnvironment()
    {
        var env = new TypeEnvironment();

        env = env.Bind("π", RealType.Instance);
        env = env.Bind("pi", RealType.Instance);
        env = env.Bind("e", RealType.Instance);
        env = env.Bind("τ", RealType.Instance);
        env = env.Bind("tau", RealType.Instance);
        env = env.Bind("φ", RealType.Instance);
        env = env.Bind("phi", RealType.Instance);
        env = env.Bind("i", ComplexType.Instance);

        return env;
    }
}
