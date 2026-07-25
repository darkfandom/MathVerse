namespace MathVerse.Math.CAS.PatternMatching;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;
using System.Numerics;

public sealed class PatternMatcher
{
    private static readonly Lazy<PatternMatcher> _instance = new(() => new PatternMatcher());
    public static PatternMatcher Instance => _instance.Value;

    private PatternMatcher() { }

    public PatternMatchResult Match(Pattern pattern, Expression expr)
    {
        var bindings = ImmutableDictionary.CreateBuilder<string, Expression>();
        var errors = ImmutableArray.CreateBuilder<PatternMatchError>();
        var success = MatchCore(pattern, expr, bindings, errors);
        return new PatternMatchResult
        {
            Success = success,
            Bindings = bindings.ToImmutable(),
            Errors = errors.ToImmutable()
        };
    }

    public bool Matches(Pattern pattern, Expression expr) => Match(pattern, expr).Success;

    private bool MatchCore(Pattern pattern, Expression expr, ImmutableDictionary<string, Expression>.Builder bindings, ImmutableArray<PatternMatchError>.Builder errors)
    {
        return pattern switch
        {
            WildcardPattern => true,
            VariablePattern vp => MatchVariable(vp, expr, bindings, errors),
            PredicatePattern pp => pp.MatchPredicate(expr),
            SequencePattern sp => MatchSequence(sp, expr, bindings, errors),
            StructuralPattern stp => MatchStructural(stp, expr, bindings, errors),
            _ => false
        };
    }

    private bool MatchVariable(VariablePattern pattern, Expression expr, ImmutableDictionary<string, Expression>.Builder bindings, ImmutableArray<PatternMatchError>.Builder errors)
    {
        if (pattern.TypeConstraint is not null && !pattern.TypeConstraint.IsInstanceOfType(expr))
        {
            errors.Add(new PatternMatchError
            {
                Message = $"Type constraint failed: expected {pattern.TypeConstraint.Name}, got {expr.GetType().Name}",
                Expression = expr,
                Pattern = pattern
            });
            return false;
        }

        if (pattern.Constraint is not null && !pattern.Constraint(expr))
        {
            errors.Add(new PatternMatchError
            {
                Message = $"Constraint failed for variable {pattern.Name}",
                Expression = expr,
                Pattern = pattern
            });
            return false;
        }

        if (bindings.TryGetValue(pattern.Name, out var existing))
        {
            if (!StructuralEquals(existing, expr))
                return false;
        }
        else
        {
            bindings[pattern.Name] = expr;
        }

        return true;
    }

    private bool MatchSequence(SequencePattern pattern, Expression expr, ImmutableDictionary<string, Expression>.Builder bindings, ImmutableArray<PatternMatchError>.Builder errors)
    {
        if (expr is not TupleExpression tuple)
        {
            errors.Add(new PatternMatchError
            {
                Message = "Sequence pattern requires tuple expression",
                Expression = expr,
                Pattern = pattern
            });
            return false;
        }

        if (tuple.Elements.Count != pattern.Patterns.Length)
        {
            errors.Add(new PatternMatchError
            {
                Message = $"Sequence length mismatch: pattern has {pattern.Patterns.Length} elements, expression has {tuple.Elements.Count}",
                Expression = expr,
                Pattern = pattern
            });
            return false;
        }

        for (int i = 0; i < pattern.Patterns.Length; i++)
        {
            if (!MatchCore(pattern.Patterns[i], tuple.Elements[i], bindings, errors))
                return false;
        }

        return true;
    }

    private bool MatchStructural(StructuralPattern pattern, Expression expr, ImmutableDictionary<string, Expression>.Builder bindings, ImmutableArray<PatternMatchError>.Builder errors)
    {
        return StructuralMatch(pattern.Template, expr, bindings, errors);
    }

    private bool StructuralMatch(Expression pattern, Expression expr, ImmutableDictionary<string, Expression>.Builder bindings, ImmutableArray<PatternMatchError>.Builder errors)
    {
        if (pattern is VariableExpression pv)
        {
            return MatchVariable(new VariablePattern(pv.Name), expr, bindings, errors);
        }

        if (pattern.GetType() != expr.GetType())
            return false;

        return pattern switch
        {
            LiteralExpression pl when expr is LiteralExpression el => pl.Value.Equals(el.Value),
            ConstantExpression pc when expr is ConstantExpression ec => pc.Name == ec.Name && pc.Value.Equals(ec.Value),
            BinaryExpression pb when expr is BinaryExpression eb =>
                pb.Operator.Equals(eb.Operator) &&
                StructuralMatch(pb.Left, eb.Left, bindings, errors) &&
                StructuralMatch(pb.Right, eb.Right, bindings, errors),
            UnaryExpression pu when expr is UnaryExpression eu =>
                pu.Operator.Equals(eu.Operator) &&
                StructuralMatch(pu.Operand, eu.Operand, bindings, errors),
            FunctionCallExpression pf when expr is FunctionCallExpression ef =>
                pf.Name == ef.Name &&
                pf.Arguments.Count == ef.Arguments.Count &&
                pf.Arguments.Zip(ef.Arguments).All(p => StructuralMatch(p.First, p.Second, bindings, errors)),
            _ => pattern.Equals(expr)
        };
    }

    private static bool StructuralEquals(Expression a, Expression b)
    {
        if (a.GetType() != b.GetType())
            return false;

        return a switch
        {
            LiteralExpression la when b is LiteralExpression lb => la.Value.Equals(lb.Value),
            VariableExpression va when b is VariableExpression vb => va.Name == vb.Name,
            ConstantExpression ca when b is ConstantExpression cb => ca.Name == cb.Name && ca.Value.Equals(cb.Value),
            BinaryExpression ba when b is BinaryExpression bb =>
                ba.Operator.Equals(bb.Operator) &&
                StructuralEquals(ba.Left, bb.Left) &&
                StructuralEquals(ba.Right, bb.Right),
            UnaryExpression ua when b is UnaryExpression ub =>
                ua.Operator.Equals(ub.Operator) &&
                StructuralEquals(ua.Operand, ub.Operand),
            FunctionCallExpression fa when b is FunctionCallExpression fb =>
                fa.Name == fb.Name &&
                fa.Arguments.Count == fb.Arguments.Count &&
                fa.Arguments.Zip(fb.Arguments).All(p => StructuralEquals(p.First, p.Second)),
            _ => a.Equals(b)
        };
    }
}