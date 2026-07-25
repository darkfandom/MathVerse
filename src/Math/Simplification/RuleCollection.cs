namespace MathVerse.Math.Simplification;

/// <summary>
/// Provides predefined collections of simplification rules for common mathematical identities.
/// </summary>
public static class RuleCollection
{
    /// <summary>
    /// Gets arithmetic identity rules: additive/multiplicative identities,
    /// double negation, self-addition, and self-multiplication.
    /// </summary>
    public static IReadOnlyList<SimplificationRule> ArithmeticRules { get; } =
    [
        // x + 0 = x
        SimplificationRule.Create("AdditiveIdentityRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "+" } } b &&
            IsZero(b.Right) ? b.Left : null, 10),

        // 0 + x = x
        SimplificationRule.Create("AdditiveIdentityLeft", expr =>
            expr is BinaryExpression { Operator: { Symbol: "+" } } b &&
            IsZero(b.Left) ? b.Right : null, 10),

        // x - 0 = x
        SimplificationRule.Create("SubtractZeroRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "-" } } b &&
            IsZero(b.Right) ? b.Left : null, 10),

        // x * 1 = x
        SimplificationRule.Create("MultiplicativeIdentityRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" } } b &&
            IsOne(b.Right) ? b.Left : null, 10),

        // 1 * x = x
        SimplificationRule.Create("MultiplicativeIdentityLeft", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" } } b &&
            IsOne(b.Left) ? b.Right : null, 10),

        // x * 0 = 0
        SimplificationRule.Create("MultiplyByZeroRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" } } b &&
            IsZero(b.Right) ? Expr.Literal(0.0) : null, 10),

        // 0 * x = 0
        SimplificationRule.Create("MultiplyByZeroLeft", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" } } b &&
            IsZero(b.Left) ? Expr.Literal(0.0) : null, 10),

        // 0 / x = 0
        SimplificationRule.Create("ZeroNumeratorDivision", expr =>
            expr is BinaryExpression { Operator: { Symbol: "/" } } b &&
            IsZero(b.Left) ? Expr.Literal(0.0) : null, 10),

        // x / 1 = x
        SimplificationRule.Create("DivideByOne", expr =>
            expr is BinaryExpression { Operator: { Symbol: "/" } } b &&
            IsOne(b.Right) ? b.Left : null, 10),

        // x ^ 0 = 1
        SimplificationRule.Create("PowerOfZero", expr =>
            expr is BinaryExpression { Operator: { Symbol: "^" } } b &&
            IsZero(b.Right) ? Expr.Literal(1.0) : null, 10),

        // x ^ 1 = x
        SimplificationRule.Create("PowerOfOne", expr =>
            expr is BinaryExpression { Operator: { Symbol: "^" } } b &&
            IsOne(b.Right) ? b.Left : null, 10),

        // 1 ^ x = 1
        SimplificationRule.Create("BaseIsOne", expr =>
            expr is BinaryExpression { Operator: { Symbol: "^" } } b &&
            IsOne(b.Left) ? Expr.Literal(1.0) : null, 10),

        // 0 ^ x = 0 (for positive x)
        SimplificationRule.Create("BaseIsZero", expr =>
            expr is BinaryExpression { Operator: { Symbol: "^" } } b &&
            IsZero(b.Left) ? Expr.Literal(0.0) : null, 10),

        // x + x = 2 * x
        SimplificationRule.Create("SelfAddition", expr =>
            expr is BinaryExpression { Operator: { Symbol: "+" }, Left: var l, Right: var r } &&
            l.Equals(r) ? Expr.Multiply(Expr.Literal(2.0), l) : null, 5),

        // x * x = x ^ 2
        SimplificationRule.Create("SelfMultiplication", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" }, Left: var l, Right: var r } &&
            l.Equals(r) ? Expr.Pow(l, Expr.Literal(2.0)) : null, 5),

        // --x = x
        SimplificationRule.Create("DoubleNegation", expr =>
            expr is UnaryExpression { Operator: { Symbol: "-" }, Operand: UnaryExpression { Operator: { Symbol: "-" }, Operand: var inner } } ? inner : null, 8),

        // x + (-x) = 0
        SimplificationRule.Create("AddInverseRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "+" }, Left: var l, Right: UnaryExpression { Operator: { Symbol: "-" }, Operand: var r } } &&
            l.Equals(r) ? Expr.Literal(0.0) : null, 5),

        // -x + x = 0
        SimplificationRule.Create("AddInverseLeft", expr =>
            expr is BinaryExpression { Operator: { Symbol: "+" }, Left: UnaryExpression { Operator: { Symbol: "-" }, Operand: var l }, Right: var r } &&
            l.Equals(r) ? Expr.Literal(0.0) : null, 5),

        // x - x = 0
        SimplificationRule.Create("SelfSubtraction", expr =>
            expr is BinaryExpression { Operator: { Symbol: "-" }, Left: var l, Right: var r } &&
            l.Equals(r) ? Expr.Literal(0.0) : null, 5),

        // x * -1 = -x
        SimplificationRule.Create("MultiplyByNegOneRight", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" }, Left: var l, Right: var r } &&
            IsNegativeOne(r) ? Expr.Negate(l) : null, 5),

        // -1 * x = -x
        SimplificationRule.Create("MultiplyByNegOneLeft", expr =>
            expr is BinaryExpression { Operator: { Symbol: "*" }, Left: var l, Right: var r } &&
            IsNegativeOne(l) ? Expr.Negate(r) : null, 5),
    ];

    /// <summary>
    /// Gets power exponent rules: power of a power, product of same base, quotient of same base.
    /// </summary>
    public static IReadOnlyList<SimplificationRule> PowerRules { get; } =
    [
        // (x^a)^b = x^(a*b)
        SimplificationRule.Create("PowerOfPower", expr =>
        {
            if (expr is not BinaryExpression { Operator: { Symbol: "^" }, Left: BinaryExpression { Operator: { Symbol: "^" }, Left: var x, Right: var a }, Right: var b })
                return null;
            return Expr.Pow(x, Expr.Multiply(a, b));
        }, 15),

        // x^a * x^b = x^(a+b)
        SimplificationRule.Create("ProductSameBase", expr =>
        {
            if (expr is not BinaryExpression { Operator: { Symbol: "*" }, Left: BinaryExpression { Operator: { Symbol: "^" }, Left: var x1, Right: var a }, Right: BinaryExpression { Operator: { Symbol: "^" }, Left: var x2, Right: var b } })
                return null;
            return x1.Equals(x2) ? Expr.Pow(x1, Expr.Add(a, b)) : null;
        }, 15),

        // x^a * x = x^(a+1)
        SimplificationRule.Create("ProductSameBaseRight", expr =>
        {
            if (expr is not BinaryExpression { Operator: { Symbol: "*" }, Left: BinaryExpression { Operator: { Symbol: "^" }, Left: var x1, Right: var a }, Right: var x2 })
                return null;
            return x1.Equals(x2) ? Expr.Pow(x1, Expr.Add(a, Expr.Literal(1.0))) : null;
        }, 15),

        // x * x^a = x^(1+a)
        SimplificationRule.Create("ProductSameBaseLeft", expr =>
        {
            if (expr is not BinaryExpression { Operator: { Symbol: "*" }, Left: var x1, Right: BinaryExpression { Operator: { Symbol: "^" }, Left: var x2, Right: var a } })
                return null;
            return x1.Equals(x2) ? Expr.Pow(x1, Expr.Add(Expr.Literal(1.0), a)) : null;
        }, 15),

        // x^a / x^b = x^(a-b)
        SimplificationRule.Create("QuotientSameBase", expr =>
        {
            if (expr is not BinaryExpression { Operator: { Symbol: "/" }, Left: BinaryExpression { Operator: { Symbol: "^" }, Left: var x1, Right: var a }, Right: BinaryExpression { Operator: { Symbol: "^" }, Left: var x2, Right: var b } })
                return null;
            return x1.Equals(x2) ? Expr.Pow(x1, Expr.Subtract(a, b)) : null;
        }, 15),
    ];

    /// <summary>
    /// Gets logarithm and exponential simplification rules.
    /// </summary>
    public static IReadOnlyList<SimplificationRule> LogRules { get; } =
    [
        // ln(1) = 0
        SimplificationRule.Create("LnOfOne", expr =>
            TryGetSingleArg(expr, "ln", out var a0) && IsOne(a0!) ? Expr.Literal(0.0) : null, 10),

        // ln(e) = 1
        SimplificationRule.Create("LnOfE", expr =>
            TryGetSingleArg(expr, "ln", out var a1) && IsEulerNumber(a1!) ? Expr.Literal(1.0) : null, 10),

        // exp(0) = 1
        SimplificationRule.Create("ExpOfZero", expr =>
            TryGetSingleArg(expr, "exp", out var a2) && IsZero(a2!) ? Expr.Literal(1.0) : null, 10),

        // exp(ln(x)) = x
        SimplificationRule.Create("ExpOfLn", expr =>
            TryGetSingleArg(expr, "exp", out var a3) &&
            TryGetSingleArg(a3!, "ln", out var inner1) ? inner1! : null, 15),

        // ln(exp(x)) = x
        SimplificationRule.Create("LnOfExp", expr =>
            TryGetSingleArg(expr, "ln", out var a4) &&
            TryGetSingleArg(a4!, "exp", out var inner2) ? inner2! : null, 15),

        // ln(a * b) = ln(a) + ln(b)
        SimplificationRule.Create("LnProduct", expr =>
        {
            if (!TryGetSingleArg(expr, "ln", out var a5))
                return null;
            if (a5 is not BinaryExpression { Operator: { Symbol: "*" }, Left: var l, Right: var r })
                return null;
            return Expr.Add(Expr.Ln(l), Expr.Ln(r));
        }, 5),

        // ln(a / b) = ln(a) - ln(b)
        SimplificationRule.Create("LnQuotient", expr =>
        {
            if (!TryGetSingleArg(expr, "ln", out var a6))
                return null;
            if (a6 is not BinaryExpression { Operator: { Symbol: "/" }, Left: var l, Right: var r })
                return null;
            return Expr.Subtract(Expr.Ln(l), Expr.Ln(r));
        }, 5),

        // ln(a^n) = n * ln(a)
        SimplificationRule.Create("LnPower", expr =>
        {
            if (!TryGetSingleArg(expr, "ln", out var a7))
                return null;
            if (a7 is not BinaryExpression { Operator: { Symbol: "^" }, Left: var l, Right: var n })
                return null;
            return Expr.Multiply(n, Expr.Ln(l));
        }, 5),
    ];

    /// <summary>
    /// Gets trigonometric identity simplification rules.
    /// </summary>
    public static IReadOnlyList<SimplificationRule> TrigRules { get; } =
    [
        // sin(0) = 0
        SimplificationRule.Create("SinOfZero", expr =>
            TryGetSingleArg(expr, "sin", out var t0) && IsZero(t0!) ? Expr.Literal(0.0) : null, 10),

        // cos(0) = 1
        SimplificationRule.Create("CosOfZero", expr =>
            TryGetSingleArg(expr, "cos", out var t1) && IsZero(t1!) ? Expr.Literal(1.0) : null, 10),

        // sin(pi/2) = 1
        SimplificationRule.Create("SinOfPiOver2", expr =>
            TryGetSingleArg(expr, "sin", out var t2) && IsPiOverTwo(t2!) ? Expr.Literal(1.0) : null, 10),

        // cos(pi/2) = 0
        SimplificationRule.Create("CosOfPiOver2", expr =>
            TryGetSingleArg(expr, "cos", out var t3) && IsPiOverTwo(t3!) ? Expr.Literal(0.0) : null, 10),

        // sin(pi) = 0
        SimplificationRule.Create("SinOfPi", expr =>
            TryGetSingleArg(expr, "sin", out var t4) && IsPi(t4!) ? Expr.Literal(0.0) : null, 10),

        // cos(pi) = -1
        SimplificationRule.Create("CosOfPi", expr =>
            TryGetSingleArg(expr, "cos", out var t5) && IsPi(t5!) ? Expr.Literal(-1.0) : null, 10),

        // tan(0) = 0
        SimplificationRule.Create("TanOfZero", expr =>
            TryGetSingleArg(expr, "tan", out var t6) && IsZero(t6!) ? Expr.Literal(0.0) : null, 10),
    ];

    /// <summary>
    /// Gets all predefined rules combined, ordered by priority.
    /// </summary>
    public static IReadOnlyList<SimplificationRule> AllRules { get; } =
        ArithmeticRules
            .Concat(PowerRules)
            .Concat(LogRules)
            .Concat(TrigRules)
            .OrderByDescending(r => r.Priority)
            .ToList();

    private static bool IsZero(Expression expr) =>
        expr is LiteralExpression { Value: 0.0 } ||
        (expr is ConstantExpression c && System.Math.Abs(c.Value) < 1e-15);

    private static bool IsOne(Expression expr) =>
        expr is LiteralExpression { Value: 1.0 } ||
        (expr is ConstantExpression c && System.Math.Abs(c.Value - 1.0) < 1e-15);

    private static bool IsNegativeOne(Expression expr) =>
        expr is LiteralExpression { Value: -1.0 } ||
        (expr is ConstantExpression c && System.Math.Abs(c.Value + 1.0) < 1e-15);

    private static bool IsEulerNumber(Expression expr) =>
        ConstantExpression.E.Equals(expr);

    private static bool IsPi(Expression expr) =>
        ConstantExpression.Pi.Equals(expr) ||
        (expr is LiteralExpression lit && System.Math.Abs(lit.Value - System.Math.PI) < 1e-10);

    private static bool IsPiOverTwo(Expression expr) =>
        expr is BinaryExpression { Operator: { Symbol: "/" }, Left: var l, Right: var r } &&
        IsPi(l) && IsTwo(r);

    private static bool IsTwo(Expression expr) =>
        expr is LiteralExpression { Value: 2.0 };

    private static bool TryGetSingleArg(Expression expr, string name, out Expression? arg)
    {
        if (expr is FunctionCallExpression f && f.Name == name && f.Arguments.Count == 1)
        {
            arg = f.Arguments[0];
            return true;
        }
        arg = null;
        return false;
    }
}
