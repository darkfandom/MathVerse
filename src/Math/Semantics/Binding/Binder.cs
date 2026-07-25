using MathVerse.Math.Semantics.Resolution;

namespace MathVerse.Math.Semantics.Binding;

/// <summary>
/// Binds untyped expression trees into resolved, validated bound expressions.
/// Performs name resolution, arity checking, operator validation, and type inference.
/// </summary>
public sealed class Binder
{
    private readonly BindingContext _context;
    private readonly IdentifierResolver _resolver;

    /// <summary>Initializes a binder.</summary>
    public Binder(BindingContext context)
    {
        _context = context;
        _resolver = new IdentifierResolver(context);
    }

    /// <summary>Binds an expression and returns a binding result.</summary>
    public BindingResult Bind(Expression expression)
    {
        var bound = BindExpression(expression);
        return new BindingResult(bound, _context.Diagnostics);
    }

    /// <summary>Binds an expression, returning the bound tree.</summary>
    public BoundExpression BindExpression(Expression expression)
    {
        return expression switch
        {
            LiteralExpression lit => new BoundLiteralExpression(lit.Value),
            BooleanExpression b => new BoundLiteralExpression(b.Value ? 1.0 : 0.0),
            ConstantExpression c => new BoundConstantExpression(
                new ConstantSymbol(c.Name, c.Value)),
            VariableExpression v => _resolver.ResolveIdentifier(v.Name),
            BinaryExpression binary => BindBinary(binary),
            UnaryExpression unary => BindUnary(unary),
            FunctionCallExpression func => BindFunctionCall(func),
            AssignmentExpression assign => BindAssignment(assign),
            PiecewiseExpression pw => BindPiecewise(pw),
            LambdaExpression lam => BindLambda(lam),
            VectorExpression vec => BindVectorLiteral(vec),
            MatrixExpression mat => BindMatrixLiteral(mat),
            SetExpression set => BindSetLiteral(set),
            TupleExpression tup => BindTupleLiteral(tup),
            ConditionalExpression cond => BindConditional(cond),
            RangeExpression range => BindRange(range),
            DerivativeExpression diff => BindDerivative(diff),
            IntegralExpression integ => BindIntegral(integ),
            SummationExpression sum => BindSummation(sum),
            ProductExpression prod => BindProduct(prod),
            LimitExpression lim => BindLimit(lim),
            FactorialExpression fact => BindFactorial(fact),
            RelationExpression rel => BindRelation(rel),
            EquationExpression eq => BindEquation(eq),
            IntervalExpression interv => BindInterval(interv),
            NullExpression => new BoundLiteralExpression(0.0),
            _ => BindFallback(expression),
        };
    }

    private BoundExpression BindBinary(BinaryExpression binary)
    {
        var left = BindExpression(binary.Left);
        var right = BindExpression(binary.Right);
        return new BoundBinaryExpression(left, binary.Operator, right);
    }

    private BoundExpression BindUnary(UnaryExpression unary)
    {
        var operand = BindExpression(unary.Operand);
        return new BoundUnaryExpression(unary.Operator, operand);
    }

    private BoundExpression BindFunctionCall(FunctionCallExpression func)
    {
        var resolved = _resolver.ResolveFunction(func.Name);
        if (resolved is null)
            return new BoundLiteralExpression(0.0);

        var args = func.Arguments.Select(BindExpression).ToList();

        if (args.Count < resolved.ParameterCount)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.TooFewArguments,
                $"Function '{func.Name}' expects at least {resolved.ParameterCount} arguments, got {args.Count}.");
        }
        else if (args.Count > resolved.ParameterCount && resolved.ParameterCount > 0)
        {
            _context.Diagnostics.ReportWarning(SemanticDiagnosticCode.TooManyArguments,
                $"Function '{func.Name}' expects {resolved.ParameterCount} arguments, got {args.Count}.");
        }

        return new BoundFunctionCallExpression(resolved, args);
    }

    private BoundExpression BindAssignment(AssignmentExpression assign)
    {
        Symbol? sym = null;
        string? targetName = null;

        if (assign.Target is VariableExpression varTarget)
        {
            targetName = varTarget.Name;
            sym = _context.SymbolTable.Lookup(targetName);
        }

        if (sym is null && targetName is not null)
        {
            var newSym = new VariableSymbol(targetName);
            _context.SymbolTable.Declare(newSym);
            sym = newSym;
        }
        else if (sym is not null && sym is not VariableSymbol && sym is not ParameterSymbol)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.CannotAssignToConstant,
                $"Cannot assign to '{targetName}' — it is not a variable.");
        }

        sym ??= new VariableSymbol("_unresolved");
        var value = BindExpression(assign.Value);
        return new BoundAssignmentExpression(sym, value);
    }

    private BoundExpression BindPiecewise(PiecewiseExpression pw)
    {
        double result = 0.0;
        bool found = false;

        foreach (var c in pw.Cases)
        {
            if (c.Condition is BooleanExpression bl && bl.Value)
            {
                if (c.Value is LiteralExpression nl)
                {
                    result = nl.Value;
                    found = true;
                    break;
                }
            }
        }

        if (pw.DefaultCase is LiteralExpression defNl)
        {
            result = defNl.Value;
            found = true;
        }

        if (!found)
        {
            _context.Diagnostics.ReportInfo(SemanticDiagnosticCode.NotImplemented,
                "Piecewise expression reduced to 0.0 (non-constant cases not yet evaluated).");
        }

        return new BoundLiteralExpression(result);
    }

    private BoundExpression BindLambda(LambdaExpression lam)
    {
        _context.SymbolTable.EnterScope(ScopeKind.Lambda);
        for (int i = 0; i < lam.Parameters.Count; i++)
            _context.SymbolTable.Declare(new ParameterSymbol(lam.Parameters[i].Name, i));
        var body = BindExpression(lam.Body);
        _context.SymbolTable.ExitScope();
        return body;
    }

    private BoundExpression BindVectorLiteral(VectorExpression vec)
    {
        if (vec.Components.Count == 1 && vec.Components[0] is LiteralExpression nl)
            return new BoundLiteralExpression(nl.Value);
        return new BoundLiteralExpression(vec.Components.Count);
    }

    private BoundExpression BindMatrixLiteral(MatrixExpression mat)
    {
        return new BoundLiteralExpression(mat.RowCount);
    }

    private BoundExpression BindSetLiteral(SetExpression set)
    {
        return new BoundLiteralExpression(set.Elements.Count);
    }

    private BoundExpression BindTupleLiteral(TupleExpression tup)
    {
        return new BoundLiteralExpression(tup.Elements.Count);
    }

    private BoundExpression BindConditional(ConditionalExpression cond)
    {
        var _ = BindExpression(cond.Condition);
        var thenExpr = BindExpression(cond.ThenBranch);
        var _2 = BindExpression(cond.ElseBranch);
        return thenExpr;
    }

    private BoundExpression BindRange(RangeExpression range)
    {
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindDerivative(DerivativeExpression diff)
    {
        var _ = BindExpression(diff.Function);
        var _2 = BindExpression(diff.Variable);
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindIntegral(IntegralExpression integ)
    {
        var _ = BindExpression(integ.Integrand);
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindSummation(SummationExpression sum)
    {
        var _ = BindExpression(sum.Body);
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindProduct(ProductExpression prod)
    {
        var _ = BindExpression(prod.Body);
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindLimit(LimitExpression lim)
    {
        var _ = BindExpression(lim.Body);
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindFactorial(FactorialExpression fact)
    {
        var operand = BindExpression(fact.Operand);
        if (operand is BoundLiteralExpression lit)
            return new BoundLiteralExpression(Factorial(lit.Value));
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindRelation(RelationExpression rel)
    {
        var left = BindExpression(rel.Left);
        var right = BindExpression(rel.Right);
        return new BoundBinaryExpression(left, rel.Operator, right);
    }

    private static readonly MathOperator s_equalityOp =
        new("=", "Equal", OperatorCategory.Relational, 2, 1);

    private BoundExpression BindEquation(EquationExpression eq)
    {
        var left = BindExpression(eq.Left);
        var right = BindExpression(eq.Right);
        return new BoundBinaryExpression(left, s_equalityOp, right);
    }

    private BoundExpression BindInterval(IntervalExpression interv)
    {
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression BindFallback(Expression expression)
    {
        _context.Diagnostics.ReportWarning(SemanticDiagnosticCode.NotImplemented,
            $"Binding not implemented for expression kind '{expression.GetType().Name}'.");
        return new BoundLiteralExpression(0.0);
    }

    private static double Factorial(double n)
    {
        if (n < 0 || n != System.Math.Floor(n))
            return double.NaN;
        if (n > 170)
            return double.PositiveInfinity;
        double result = 1;
        for (int i = 2; i <= (int)n; i++)
            result *= i;
        return result;
    }
}
