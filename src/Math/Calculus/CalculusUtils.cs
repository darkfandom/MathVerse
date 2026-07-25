namespace MathVerse.Math.Calculus;

/// <summary>
/// Provides utility methods for calculus operations including
/// expression evaluation, classification, and term collection.
/// </summary>
public static class CalculusUtils
{
    /// <summary>
    /// Checks whether the expression evaluates to zero.
    /// Handles literal zero, negative zero, and expressions that simplify to zero.
    /// </summary>
    /// <param name="expr">The expression to check.</param>
    /// <returns><c>true</c> if the expression is zero; otherwise, <c>false</c>.</returns>
    public static bool IsZero(Expression expr)
    {
        if (expr is LiteralExpression lit)
            return System.Math.Abs(lit.Value) < 1e-12;

        if (expr is ConstantExpression constant)
            return System.Math.Abs(constant.Value) < 1e-12;

        if (expr is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
            return IsZero(unary.Operand);

        return false;
    }

    /// <summary>
    /// Determines whether the expression is constant with respect to the given variable.
    /// A constant expression does not contain the specified variable anywhere in its tree.
    /// </summary>
    /// <param name="expr">The expression to check.</param>
    /// <param name="variable">The variable name.</param>
    /// <returns><c>true</c> if the expression is constant with respect to the variable; otherwise, <c>false</c>.</returns>
    public static bool IsConstant(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        return expr.Kind switch
        {
            ExpressionKind.Literal => true,
            ExpressionKind.Constant => true,
            ExpressionKind.Boolean => true,
            ExpressionKind.Identity => true,
            ExpressionKind.Null => true,
            ExpressionKind.Variable => expr is VariableExpression v && v.Name != variable,
            ExpressionKind.Parameter => expr is ParameterExpression p && p.Name != variable,
            ExpressionKind.Binary => IsBinaryConstant((BinaryExpression)expr, variable),
            ExpressionKind.Unary => IsConstant(((UnaryExpression)expr).Operand, variable),
            ExpressionKind.FunctionCall => IsFunctionCallConstant((FunctionCallExpression)expr, variable),
            ExpressionKind.Derivative => IsDerivativeConstant((DerivativeExpression)expr, variable),
            _ => false
        };
    }

    /// <summary>
    /// Evaluates the expression at the specified variable value.
    /// Returns <see cref="double.NaN"/> if evaluation is not possible.
    /// </summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <param name="variable">The variable to substitute.</param>
    /// <param name="value">The value to substitute for the variable.</param>
    /// <returns>The numeric result of evaluation, or <see cref="double.NaN"/>.</returns>
    public static double EvaluateAt(Expression expr, string variable, double value)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        return expr.Kind switch
        {
            ExpressionKind.Literal => ((LiteralExpression)expr).Value,
            ExpressionKind.Constant => ((ConstantExpression)expr).Value,
            ExpressionKind.Variable => ((VariableExpression)expr).Name == variable ? value : double.NaN,
            ExpressionKind.Binary => EvaluateBinaryAt((BinaryExpression)expr, variable, value),
            ExpressionKind.Unary => EvaluateUnaryAt((UnaryExpression)expr, variable, value),
            ExpressionKind.FunctionCall => EvaluateFunctionCallAt((FunctionCallExpression)expr, variable, value),
            _ => double.NaN
        };
    }

    /// <summary>
    /// Attempts to collect like terms in the expression.
    /// Groups terms that are multiplication by the same variable powers.
    /// </summary>
    /// <param name="expr">The expression to collect terms in.</param>
    /// <returns>A simplified expression with like terms collected.</returns>
    public static Expression CollectTerms(Expression expr)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));

        if (expr is not BinaryExpression topBin || !topBin.Operator.Equals(MathOperator.Add))
            return expr;

        var terms = FlattenAddition(topBin);
        var grouped = new Dictionary<string, (Expression Term, double Coefficient)>();

        foreach (var term in terms)
        {
            var (key, coefficient) = DecomposeTerm(term);

            if (grouped.TryGetValue(key, out var existing))
                grouped[key] = (existing.Term, existing.Coefficient + coefficient);
            else
                grouped[key] = (term, coefficient);
        }

        Expression? result = null;
        foreach (var (_, entry) in grouped)
        {
            if (System.Math.Abs(entry.Coefficient) < 1e-12)
                continue;

            var term = System.Math.Abs(entry.Coefficient - 1.0) < 1e-10
                ? entry.Term
                : Expr.Multiply(Expr.Literal(entry.Coefficient), StripCoefficient(entry.Term));

            result = result is null ? term : Expr.Add(result, term);
        }

        return result ?? Expr.Literal(0.0);
    }

    /// <summary>
    /// Recursively checks whether a binary expression is constant.
    /// </summary>
    private static bool IsBinaryConstant(BinaryExpression bin, string variable)
    {
        return IsConstant(bin.Left, variable) && IsConstant(bin.Right, variable);
    }

    /// <summary>
    /// Recursively checks whether a function call is constant.
    /// </summary>
    private static bool IsFunctionCallConstant(FunctionCallExpression funcCall, string variable)
    {
        foreach (var arg in funcCall.Arguments)
        {
            if (!IsConstant(arg, variable))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether a derivative expression is constant.
    /// </summary>
    private static bool IsDerivativeConstant(DerivativeExpression deriv, string variable)
    {
        if (deriv.Variable is VariableExpression varExpr && varExpr.Name == variable)
            return false;

        return IsConstant(deriv.Function, variable);
    }

    /// <summary>
    /// Evaluates a binary expression at a specific variable value.
    /// </summary>
    private static double EvaluateBinaryAt(BinaryExpression bin, string variable, double value)
    {
        var left = EvaluateAt(bin.Left, variable, value);
        var right = EvaluateAt(bin.Right, variable, value);

        if (double.IsNaN(left) || double.IsNaN(right))
            return double.NaN;

        return bin.Operator switch
        {
            var op when op.Equals(MathOperator.Add) => left + right,
            var op when op.Equals(MathOperator.Subtract) => left - right,
            var op when op.Equals(MathOperator.Multiply) => left * right,
            var op when op.Equals(MathOperator.Divide) => right != 0.0 ? left / right : double.NaN,
            var op when op.Equals(MathOperator.Power) => System.Math.Pow(left, right),
            var op when op.Equals(MathOperator.Modulo) => right != 0.0 ? left % right : double.NaN,
            _ => double.NaN
        };
    }

    /// <summary>
    /// Evaluates a unary expression at a specific variable value.
    /// </summary>
    private static double EvaluateUnaryAt(UnaryExpression unary, string variable, double value)
    {
        var operand = EvaluateAt(unary.Operand, variable, value);

        if (double.IsNaN(operand))
            return double.NaN;

        return unary.Operator switch
        {
            var op when op.Equals(MathOperator.Negate) => -operand,
            var op when op.Equals(MathOperator.Abs) => System.Math.Abs(operand),
            _ => double.NaN
        };
    }

    /// <summary>
    /// Evaluates a function call at a specific variable value.
    /// </summary>
    private static double EvaluateFunctionCallAt(FunctionCallExpression funcCall, string variable, double value)
    {
        if (funcCall.Arguments.Count < 1)
            return double.NaN;

        var argValue = EvaluateAt(funcCall.Arguments[0], variable, value);
        if (double.IsNaN(argValue))
            return double.NaN;

        return funcCall.Name switch
        {
            "sin" => System.Math.Sin(argValue),
            "cos" => System.Math.Cos(argValue),
            "tan" => System.Math.Tan(argValue),
            "asin" => System.Math.Asin(argValue),
            "acos" => System.Math.Acos(argValue),
            "atan" => System.Math.Atan(argValue),
            "exp" => System.Math.Exp(argValue),
            "ln" => argValue > 0.0 ? System.Math.Log(argValue) : double.NaN,
            "log10" => argValue > 0.0 ? System.Math.Log10(argValue) : double.NaN,
            "sqrt" => argValue >= 0.0 ? System.Math.Sqrt(argValue) : double.NaN,
            "cbrt" => System.Math.Cbrt(argValue),
            "abs" => System.Math.Abs(argValue),
            "sinh" => System.Math.Sinh(argValue),
            "cosh" => System.Math.Cosh(argValue),
            "tanh" => System.Math.Tanh(argValue),
            "floor" => System.Math.Floor(argValue),
            "ceil" => System.Math.Ceiling(argValue),
            "round" => System.Math.Round(argValue),
            "log" when funcCall.Arguments.Count == 2 =>
                EvaluateLogWithBase(funcCall, variable, value),
            _ => double.NaN
        };
    }

    /// <summary>
    /// Evaluates log_base(x) with a specified base.
    /// </summary>
    private static double EvaluateLogWithBase(FunctionCallExpression funcCall, string variable, double value)
    {
        var xValue = EvaluateAt(funcCall.Arguments[0], variable, value);
        var baseValue = EvaluateAt(funcCall.Arguments[1], variable, value);

        if (xValue > 0.0 && baseValue > 0.0 && baseValue != 1.0)
            return System.Math.Log(xValue) / System.Math.Log(baseValue);

        return double.NaN;
    }

    /// <summary>
    /// Flattens a nested addition tree into a list of terms.
    /// </summary>
    private static List<Expression> FlattenAddition(Expression expr)
    {
        var terms = new List<Expression>();
        FlattenAdditionCore(expr, terms);
        return terms;
    }

    /// <summary>
    /// Core recursion for flattening addition.
    /// </summary>
    private static void FlattenAdditionCore(Expression expr, List<Expression> terms)
    {
        if (expr is BinaryExpression bin && bin.Operator.Equals(MathOperator.Add))
        {
            FlattenAdditionCore(bin.Left, terms);
            FlattenAdditionCore(bin.Right, terms);
        }
        else
        {
            terms.Add(expr);
        }
    }

    /// <summary>
    /// Decomposes a term into a canonical key and its numeric coefficient.
    /// </summary>
    private static (string Key, double Coefficient) DecomposeTerm(Expression term)
    {
        if (term is LiteralExpression lit)
            return ("1", lit.Value);

        if (term is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
        {
            var (key, coeff) = DecomposeTerm(unary.Operand);
            return (key, -coeff);
        }

        if (term is BinaryExpression bin && bin.Operator.Equals(MathOperator.Multiply))
        {
            if (bin.Left is LiteralExpression litCoeff)
            {
                var (key, innerCoeff) = DecomposeTerm(bin.Right);
                return (key, litCoeff.Value * innerCoeff);
            }

            if (bin.Right is LiteralExpression litCoeffR)
            {
                var (key, innerCoeff) = DecomposeTerm(bin.Left);
                return (key, litCoeffR.Value * innerCoeff);
            }
        }

        return (term.ToString(), 1.0);
    }

    /// <summary>
    /// Strips the numeric coefficient from a term, returning the remaining factor.
    /// </summary>
    private static Expression StripCoefficient(Expression term)
    {
        if (term is LiteralExpression)
            return Expr.Literal(1.0);

        if (term is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
            return StripCoefficient(unary.Operand);

        if (term is BinaryExpression bin && bin.Operator.Equals(MathOperator.Multiply))
        {
            if (bin.Left is LiteralExpression)
                return bin.Right;

            if (bin.Right is LiteralExpression)
                return bin.Left;
        }

        return term;
    }
}
