namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.CAS.PatternMatching;
using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public static class RuleSet
{
    private static readonly Expression Zero = Expr.Literal(0.0);
    private static readonly Expression One = Expr.Literal(1.0);

    public static ImmutableArray<RewriteRule> AlgebraicRules { get; } = ImmutableArray.Create(
        new RewriteRule
        {
            Name = "AddZero",
            Pattern = Pattern.Structural(Expr.Add(Expr.Variable("_x"), Expr.Literal(0.0))),
            Replacement = Expr.Variable("_x"),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "MultiplyByOne",
            Pattern = Pattern.Structural(Expr.Multiply(Expr.Variable("_x"), Expr.Literal(1.0))),
            Replacement = Expr.Variable("_x"),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "MultiplyByZero",
            Pattern = Pattern.Structural(Expr.Multiply(Expr.Variable("_x"), Expr.Literal(0.0))),
            Replacement = Expr.Literal(0.0),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "PowerZero",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Variable("_x"), Expr.Literal(0.0))),
            Replacement = Expr.Literal(1.0),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "PowerOne",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Variable("_x"), Expr.Literal(1.0))),
            Replacement = Expr.Variable("_x"),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "OnePower",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Literal(1.0), Expr.Variable("_x"))),
            Replacement = Expr.Literal(1.0),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "ZeroPower",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Literal(0.0), Expr.Variable("_x"))),
            Replacement = Expr.Literal(0.0),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "NegateNegate",
            Pattern = Pattern.Structural(Expr.Negate(Expr.Negate(Expr.Variable("_x")))),
            Replacement = Expr.Variable("_x"),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "SubtractSelf",
            Pattern = Pattern.Structural(Expr.Subtract(Expr.Variable("_x"), Expr.Variable("_x"))),
            Replacement = Expr.Literal(0.0),
            Priority = 100
        },
        new RewriteRule
        {
            Name = "DivideSelf",
            Pattern = Pattern.Structural(Expr.Divide(Expr.Variable("_x"), Expr.Variable("_x"))),
            Replacement = Expr.Literal(1.0),
            Priority = 100,
            Condition = e => !IsZero(e)
        }
    );

    public static ImmutableArray<RewriteRule> TrigonometricRules { get; } = ImmutableArray.Create(
        new RewriteRule
        {
            Name = "SinSquaredPlusCosSquared",
            Pattern = Pattern.FromPredicate(e =>
            {
                if (e is BinaryExpression b && b.Operator.Equals(MathOperator.Add))
                {
                    if (b.Left is FunctionCallExpression fl && fl.Name.Equals("sin") &&
                        b.Right is FunctionCallExpression fr && fr.Name.Equals("cos") &&
                        fl.Arguments.Count > 0 && fr.Arguments.Count > 0 &&
                        fl.Arguments[0].Equals(fr.Arguments[0]))
                        return true;
                }
                return false;
            }),
            Replacement = Expr.Literal(1.0),
            Priority = 90
        }
    );

    public static ImmutableArray<RewriteRule> LogarithmicRules { get; } = ImmutableArray.Create(
        new RewriteRule
        {
            Name = "LogProduct",
            Pattern = Pattern.FromPredicate(e =>
            {
                if (e is FunctionCallExpression f && f.Name.Equals("log") && f.Arguments.Count == 1 &&
                    f.Arguments[0] is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
                    return true;
                return false;
            }),
            Replacement = Expr.Literal(0.0), // Placeholder, use custom rewriter
            Priority = 80,
            Condition = e => e is FunctionCallExpression f && f.Name.Equals("log") && f.Arguments.Count == 1 &&
                            f.Arguments[0] is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply)
        }
    );

    public static ImmutableArray<RewriteRule> PowerRules { get; } = ImmutableArray.Create(
        new RewriteRule
        {
            Name = "NegativeExponent",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Variable("_x"), Expr.Negate(Expr.Variable("_y")))),
            Replacement = Expr.Divide(Expr.Literal(1.0), Expr.Pow(Expr.Variable("_x"), Expr.Variable("_y"))),
            Priority = 80
        },
        new RewriteRule
        {
            Name = "FractionalExponentRoot",
            Pattern = Pattern.Structural(Expr.Pow(Expr.Variable("_x"), Expr.Divide(Expr.Literal(1.0), Expr.Variable("_n")))),
            Replacement = Expr.Call("root", Expr.Variable("_x"), Expr.Variable("_n")),
            Priority = 80
        }
    );

    public static ImmutableArray<RewriteRule> SimplificationRules { get; } = ImmutableArray.Create(
        new RewriteRule
        {
            Name = "ConstantFolding",
            Pattern = Pattern.FromPredicate(e =>
            {
                if (e is BinaryExpression b && b.Left is LiteralExpression && b.Right is LiteralExpression &&
                    (b.Operator.Equals(MathOperator.Add) || b.Operator.Equals(MathOperator.Subtract) ||
                     b.Operator.Equals(MathOperator.Multiply) || b.Operator.Equals(MathOperator.Divide) ||
                     b.Operator.Equals(MathOperator.Power)))
                    return true;
                return false;
            }),
            Replacement = Expr.Literal(0.0), // Will be computed by constant folder
            Priority = 100,
            Direction = RewriteDirection.BottomUp
        }
    );

    private static bool IsZero(Expression e) => e is LiteralExpression l && l.Value.Equals(0.0);
}