namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public sealed class AlgebraicSimplifier
{
    public ImmutableArray<string> LastAppliedRules { get; private set; }

    public Expression Simplify(Expression expr)
    {
        LastAppliedRules = [];
        return SimplifyRecursive(expr);
    }

    private Expression SimplifyRecursive(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b => SimplifyBinary(b),
            UnaryExpression u => SimplifyUnary(u),
            FunctionCallExpression f => SimplifyFunction(f),
            _ => expr
        };
    }

    private Expression SimplifyBinary(BinaryExpression expr)
    {
        var left = SimplifyRecursive(expr.Left);
        var right = SimplifyRecursive(expr.Right);

        var simplified = new BinaryExpression(expr.Operator, left, right);

        if (expr.Operator.Equals(MathOperator.Add))
            return SimplifyAdd(simplified);
        if (expr.Operator.Equals(MathOperator.Subtract))
            return SimplifySubtract(simplified);
        if (expr.Operator.Equals(MathOperator.Multiply))
            return SimplifyMul(simplified);
        if (expr.Operator.Equals(MathOperator.Divide))
            return SimplifyDiv(simplified);
        if (expr.Operator.Equals(MathOperator.Power))
            return SimplifyPow(simplified);
        if (expr.Operator.Equals(MathOperator.Modulo))
            return SimplifyMod(simplified);

        return simplified;
    }

    private Expression SimplifyUnary(UnaryExpression expr)
    {
        var operand = SimplifyRecursive(expr.Operand);
        var simplified = new UnaryExpression(expr.Operator, operand);

        if (expr.Operator.Equals(MathOperator.Negate))
            return SimplifyNeg(simplified);

        return simplified;
    }

    private Expression SimplifyFunction(FunctionCallExpression expr)
    {
        var args = expr.Arguments.Select(SimplifyRecursive).ToArray();
        return new FunctionCallExpression(expr.Name, args);
    }

    private Expression SimplifyAdd(BinaryExpression expr)
    {
        var terms = FlattenAdd(expr);
        var simplified = CombineConstants(terms);

        if (simplified.Count == 0) return Expr.Literal(0);
        if (simplified.Count == 1) return simplified[0];

        var grouped = GroupLikeTerms(simplified);
        if (grouped.Count == 1) return grouped[0];

        var combined = CombineLikeTerms(grouped);
        if (combined.Count == 1) return combined[0];

        LastAppliedRules = LastAppliedRules.Add("SimplifyAdd");
        return BuildAddChain(combined);
    }

    private Expression SimplifySubtract(BinaryExpression expr)
    {
        var negatedRight = Expr.Negate(expr.Right);
        return SimplifyAdd(new BinaryExpression(MathOperator.Add, expr.Left, negatedRight));
    }

    private Expression SimplifyMul(BinaryExpression expr)
    {
        var factors = FlattenMul(expr);
        var simplified = CombineConstants(factors);

        if (simplified.Count == 0) return Expr.Literal(1);
        if (simplified.Count == 1) return simplified[0];

        var combined = CombineLikePowers(simplified);
        if (combined.Count == 1) return combined[0];

        LastAppliedRules = LastAppliedRules.Add("SimplifyMul");
        return BuildMulChain(combined);
    }

    private Expression SimplifyDiv(BinaryExpression expr)
    {
        var num = SimplifyRecursive(expr.Left);
        var den = SimplifyRecursive(expr.Right);

        if (num is LiteralExpression nl && den is LiteralExpression dl)
        {
            if (dl.Value != 0) return Expr.Literal(nl.Value / dl.Value);
        }

        if (num.Equals(den)) return Expr.Literal(1);
        if (den is LiteralExpression d && d.Value == 1) return num;

        LastAppliedRules = LastAppliedRules.Add("SimplifyDiv");
        return Expr.Divide(num, den);
    }

    private Expression SimplifyPow(BinaryExpression expr)
    {
        var baseExpr = SimplifyRecursive(expr.Left);
        var expExpr = SimplifyRecursive(expr.Right);

        if (expExpr is LiteralExpression le)
        {
            if (le.Value == 0) return Expr.Literal(1);
            if (le.Value == 1) return baseExpr;
            if (le.Value == -1) return Expr.Divide(Expr.Literal(1), baseExpr);
            if (le.Value == 0.5) return Expr.Call("sqrt", baseExpr);
            if (le.Value == 2 && baseExpr is not FunctionCallExpression)
                return Expr.Multiply(baseExpr, baseExpr);
            if (le.Value == 3 && baseExpr is not FunctionCallExpression)
                return Expr.Multiply(Expr.Multiply(baseExpr, baseExpr), baseExpr);
            if (le.Value < 0) return Expr.Divide(Expr.Literal(1), Expr.Pow(baseExpr, Expr.Literal(-le.Value)));
        }

        if (baseExpr is LiteralExpression bl)
        {
            if (bl.Value == 0 && expExpr is LiteralExpression el && el.Value > 0) return Expr.Literal(0);
            if (bl.Value == 1) return Expr.Literal(1);
            if (bl.Value == -1 && expExpr is LiteralExpression el2)
                return Expr.Literal(el2.Value % 2 == 0 ? 1 : -1);
        }

        if (baseExpr is BinaryExpression bp && bp.Operator.Equals(MathOperator.Power))
        {
            var newExp = Expr.Multiply(bp.Right, expExpr);
            return SimplifyPow(new BinaryExpression(MathOperator.Power, bp.Left, newExp));
        }

        if (baseExpr is BinaryExpression bm && bm.Operator.Equals(MathOperator.Multiply))
        {
            return Expr.Multiply(
                SimplifyPow(new BinaryExpression(MathOperator.Power, bm.Left, expExpr)),
                SimplifyPow(new BinaryExpression(MathOperator.Power, bm.Right, expExpr))
            );
        }

        LastAppliedRules = LastAppliedRules.Add("SimplifyPow");
        return Expr.Pow(baseExpr, expExpr);
    }

    private Expression SimplifyMod(BinaryExpression expr)
    {
        if (expr.Left is LiteralExpression l && expr.Right is LiteralExpression r && r.Value != 0)
            return Expr.Literal(l.Value % r.Value);

        LastAppliedRules = LastAppliedRules.Add("SimplifyMod");
        return expr;
    }

    private Expression SimplifyNeg(UnaryExpression expr)
    {
        var operand = expr.Operand;

        if (operand is LiteralExpression l) return Expr.Literal(-l.Value);
        if (operand is UnaryExpression u && u.Operator.Equals(MathOperator.Negate)) return u.Operand;
        if (operand is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Add))
                return Expr.Add(Expr.Negate(b.Left), Expr.Negate(b.Right));
            if (b.Operator.Equals(MathOperator.Subtract))
                return Expr.Subtract(Expr.Negate(b.Left), Expr.Negate(b.Right));
        }

        LastAppliedRules = LastAppliedRules.Add("SimplifyNeg");
        return expr;
    }

    private List<Expression> FlattenAdd(BinaryExpression expr)
    {
        var terms = new List<Expression>();
        FlattenAddRecursive(expr, terms);
        return terms;
    }

    private void FlattenAddRecursive(Expression expr, List<Expression> terms)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Add))
        {
            FlattenAddRecursive(b.Left, terms);
            FlattenAddRecursive(b.Right, terms);
        }
        else
        {
            terms.Add(expr);
        }
    }

    private List<Expression> FlattenMul(BinaryExpression expr)
    {
        var factors = new List<Expression>();
        FlattenMulRecursive(expr, factors);
        return factors;
    }

    private void FlattenMulRecursive(Expression expr, List<Expression> factors)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            FlattenMulRecursive(b.Left, factors);
            FlattenMulRecursive(b.Right, factors);
        }
        else
        {
            factors.Add(expr);
        }
    }

    private List<Expression> CombineConstants(List<Expression> terms)
    {
        var constants = new List<LiteralExpression>();
        var others = new List<Expression>();

        foreach (var term in terms)
        {
            if (term is LiteralExpression l) constants.Add(l);
            else others.Add(term);
        }

        if (constants.Count > 0)
        {
            var sum = 0.0;
            foreach (var c in constants) sum += c.Value;
            if (System.Math.Abs(sum) > 1e-15)
                others.Insert(0, Expr.Literal(sum));
        }

        return others;
    }

    private List<Expression> GroupLikeTerms(List<Expression> terms)
    {
        var groups = new Dictionary<string, List<Expression>>();

        foreach (var term in terms)
        {
            var key = GetTermKey(term);
            if (!groups.ContainsKey(key)) groups[key] = [];
            groups[key].Add(term);
        }

        var result = new List<Expression>();
        foreach (var group in groups.Values)
        {
            if (group.Count == 1) result.Add(group[0]);
            else
            {
                var coeff = CombineCoefficients(group);
                var varPart = group[0] is BinaryExpression be ? ExtractVariablePartFromMul(be) : group[0];
                if (coeff is LiteralExpression cl && System.Math.Abs(cl.Value) < 1e-15)
                    continue;
                result.Add(coeff.Equals(Expr.Literal(1)) ? varPart! : Expr.Multiply(coeff, varPart!));
            }
        }

        return result;
    }

    private string GetTermKey(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            var varPart = ExtractVariablePartFromMul(b);
            return varPart!.ToString() ?? "const";
        }
        return expr.ToString();
    }

    private Expression? ExtractVariablePartFromMul(BinaryExpression expr)
    {
        if (expr.Left is LiteralExpression) return expr.Right;
        if (expr.Right is LiteralExpression) return expr.Left;
        return expr;
    }

    private Expression ExtractVariablePartRecursive(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) => ExtractVariablePartFromMul(b)!,
            _ => expr
        };
    }

    private Expression CombineCoefficients(List<Expression> terms)
    {
        if (terms.Count == 1) return ExtractCoefficient(terms[0]);

        var first = ExtractCoefficient(terms[0]);
        var result = first;

        for (var i = 1; i < terms.Count; i++)
        {
            var coeff = ExtractCoefficient(terms[i]);
            if (result is LiteralExpression rl && coeff is LiteralExpression cl)
                result = Expr.Literal(rl.Value + cl.Value);
            else
                result = Expr.Add(result, coeff);
        }

        return result;
    }

    private Expression ExtractCoefficient(Expression expr)
    {
        if (expr is LiteralExpression) return expr;
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            if (b.Left is LiteralExpression) return b.Left;
            if (b.Right is LiteralExpression) return b.Right;
        }
        return Expr.Literal(1);
    }

    private List<Expression> CombineLikeTerms(List<Expression> terms)
    {
        var groups = new Dictionary<string, List<Expression>>();

        foreach (var term in terms)
        {
            var key = GetCanonicalKey(term);
            if (!groups.ContainsKey(key)) groups[key] = [];
            groups[key].Add(term);
        }

        var result = new List<Expression>();
        foreach (var group in groups.Values)
        {
            if (group.Count == 1) result.Add(group[0]);
            else
            {
                var combined = CombineCoefficients(group);
                var varPart = group[0] is BinaryExpression be ? ExtractVariablePartFromMul(be) : group[0];
                if (combined is LiteralExpression cl && System.Math.Abs(cl.Value) < 1e-15)
                    continue;
                result.Add(combined.Equals(Expr.Literal(1)) ? varPart! : Expr.Multiply(combined, varPart!));
            }
        }

        return result;
    }

    private string GetCanonicalKey(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            var varPart = ExtractVariablePartFromMul(b);
            return varPart!.ToString() ?? "1";
        }
        return expr.ToString();
    }

    private List<Expression> CombineLikePowers(List<Expression> factors)
    {
        var groups = new Dictionary<string, List<Expression>>();

        foreach (var factor in factors)
        {
            var key = GetPowerBaseKey(factor);
            if (!groups.ContainsKey(key)) groups[key] = [];
            groups[key].Add(factor);
        }

        var result = new List<Expression>();
        foreach (var group in groups.Values)
        {
            if (group.Count == 1) result.Add(group[0]);
            else
            {
                var combined = CombinePowers(group);
                result.Add(combined);
            }
        }

        return result;
    }

    private string GetPowerBaseKey(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power))
            return b.Left.ToString();
        return expr.ToString();
    }

    private Expression CombinePowers(List<Expression> powers)
    {
        var baseExpr = powers[0] is BinaryExpression bp ? bp.Left : powers[0];
        var exp = powers[0] is BinaryExpression bp0 ? bp0.Right : Expr.Literal(1);

        for (var i = 1; i < powers.Count; i++)
        {
            if (powers[i] is BinaryExpression bpi)
                exp = Expr.Add(exp, bpi.Right);
            else
                exp = Expr.Add(exp, Expr.Literal(1));
        }

        if (exp is BinaryExpression addExp && addExp.Operator.Equals(MathOperator.Add))
        {
            var lv = addExp.Left is LiteralExpression ll ? (double?)ll.Value :
                     addExp.Left is ConstantExpression cl ? (double?)cl.Value : null;
            var rv = addExp.Right is LiteralExpression lr ? (double?)lr.Value :
                     addExp.Right is ConstantExpression cr ? (double?)cr.Value : null;
            if (lv.HasValue && rv.HasValue)
                exp = Expr.Literal(lv.Value + rv.Value);
        }

        return Expr.Pow(baseExpr, exp);
    }

    private Expression BuildAddChain(List<Expression> terms)
    {
        var result = terms[0];
        for (var i = 1; i < terms.Count; i++)
            result = Expr.Add(result, terms[i]);
        return result;
    }

    private Expression BuildMulChain(List<Expression> factors)
    {
        var result = factors[0];
        for (var i = 1; i < factors.Count; i++)
            result = Expr.Multiply(result, factors[i]);
        return result;
    }

    public Expression CombineLikeTerms(Expression expr) => SimplifyAdd((BinaryExpression)expr);
    public Expression FactorCommonTerms(Expression expr) => SimplifyMul((BinaryExpression)expr);
    public Expression DistributeMultiplication(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            if (b.Left is BinaryExpression bl && bl.Operator.Equals(MathOperator.Add))
                return Expr.Add(DistributeMultiplication(Expr.Multiply(bl.Left, b.Right)),
                               DistributeMultiplication(Expr.Multiply(bl.Right, b.Right)));
            if (b.Right is BinaryExpression br && br.Operator.Equals(MathOperator.Add))
                return Expr.Add(DistributeMultiplication(Expr.Multiply(b.Left, br.Left)),
                               DistributeMultiplication(Expr.Multiply(b.Left, br.Right)));
        }
        return expr;
    }
}