namespace MathVerse.Math.CAS.Substitution;

using MathVerse.Math.Expressions;
using MathVerse.Math.Visitors;
using System.Collections.Immutable;

public static class SubstitutionEngine
{
    public static Expression Substitute(Expression expr, Substitution sub)
    {
        return expr.Accept(new SubstitutionVisitor(sub.Variables, sub.Functions));
    }

    public static Expression SubstituteVariables(Expression expr, ImmutableDictionary<string, Expression> vars)
    {
        return expr.Accept(new SubstitutionVisitor(vars, ImmutableDictionary<string, Expression>.Empty));
    }

    public static Expression SubstituteFunctions(Expression expr, ImmutableDictionary<string, Expression> funcs)
    {
        return expr.Accept(new SubstitutionVisitor(ImmutableDictionary<string, Expression>.Empty, funcs));
    }

    public static Expression SubstitutePattern(Expression expr, Expression pattern, Expression replacement)
    {
        var matchResult = PatternMatching.PatternMatcher.Instance.Match(new PatternMatching.StructuralPattern(pattern), expr);
        if (!matchResult.Success)
            return expr;

        var substitution = new Substitution
        {
            Variables = matchResult.Bindings
        };
        return Substitute(replacement, substitution);
    }

    private sealed class SubstitutionVisitor : IExpressionTransformer
    {
        private readonly ImmutableDictionary<string, Expression> _variables;
        private readonly ImmutableDictionary<string, Expression> _functions;

        public SubstitutionVisitor(ImmutableDictionary<string, Expression> variables, ImmutableDictionary<string, Expression> functions)
        {
            _variables = variables;
            _functions = functions;
        }

        public Expression Visit(LiteralExpression expression) => expression;
        public Expression Visit(ConstantExpression expression) => expression;

        public Expression Visit(VariableExpression expression)
        {
            if (_variables.TryGetValue(expression.Name, out var replacement))
                return replacement;
            return expression;
        }

        public Expression Visit(BinaryExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new BinaryExpression(expression.Operator, left, right);
        }

        public Expression Visit(UnaryExpression expression)
        {
            var operand = expression.Operand.Accept(this);
            return ReferenceEquals(operand, expression.Operand)
                ? expression
                : new UnaryExpression(expression.Operator, operand);
        }

        public Expression Visit(FunctionCallExpression expression)
        {
            var args = expression.Arguments.Select(a => a.Accept(this)).ToArray();
            bool changed = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!ReferenceEquals(args[i], expression.Arguments[i]))
                    changed = true;
            }

            if (_functions.TryGetValue(expression.Name, out var funcReplacement))
            {
                var substituted = Substitute(funcReplacement, new Substitution
                {
                    Variables = args.Select((arg, i) => (Name: $"_{i}", Expression: arg))
                        .ToImmutableDictionary(x => x.Name, x => x.Expression)
                });
                return substituted;
            }

            return changed
                ? new FunctionCallExpression(expression.Name, args)
                : expression;
        }

        public Expression Visit(LambdaExpression expression)
        {
            var newParams = expression.Parameters.Select(p => (ParameterExpression)p.Accept(this)).ToArray();
            var body = expression.Body.Accept(this);
            bool paramsChanged = !newParams.SequenceEqual(expression.Parameters);
            return paramsChanged || !ReferenceEquals(body, expression.Body)
                ? new LambdaExpression(newParams, body)
                : expression;
        }

        public Expression Visit(ParameterExpression expression) => expression;

        public Expression Visit(EquationExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new EquationExpression(left, right);
        }

        public Expression Visit(PiecewiseExpression expression)
        {
            var cases = expression.Cases.Select(c => new PiecewiseCase(c.Value.Accept(this), c.Condition.Accept(this))).ToArray();
            var defaultCase = expression.DefaultCase?.Accept(this);
            if (cases.SequenceEqual(expression.Cases, new PiecewiseCaseEqualityComparer()) &&
                ReferenceEquals(defaultCase, expression.DefaultCase))
                return expression;
            return new PiecewiseExpression(cases, defaultCase);
        }

        public Expression Visit(ConditionalExpression expression)
        {
            var condition = expression.Condition.Accept(this);
            var thenBranch = expression.ThenBranch.Accept(this);
            var elseBranch = expression.ElseBranch.Accept(this);
            return ReferenceEquals(condition, expression.Condition) &&
                   ReferenceEquals(thenBranch, expression.ThenBranch) &&
                   ReferenceEquals(elseBranch, expression.ElseBranch)
                ? expression
                : new ConditionalExpression(condition, thenBranch, elseBranch);
        }

        public Expression Visit(TupleExpression expression)
        {
            var elements = expression.Elements.Select(e => e.Accept(this)).ToArray();
            return elements.SequenceEqual(expression.Elements) ? expression : new TupleExpression(elements);
        }

        public Expression Visit(VectorExpression expression)
        {
            var components = expression.Components.Select(c => c.Accept(this)).ToArray();
            return components.SequenceEqual(expression.Components) ? expression : new VectorExpression(components);
        }

        public Expression Visit(MatrixExpression expression)
        {
            var rows = expression.Rows.Select(r => r.Accept(this)).Cast<VectorExpression>().ToArray();
            return rows.SequenceEqual(expression.Rows) ? expression : new MatrixExpression(rows);
        }

        public Expression Visit(TensorExpression expression)
        {
            var components = expression.Components.Select(c => c.Accept(this)).ToArray();
            return components.SequenceEqual(expression.Components) ? expression : new TensorExpression(expression.Shape, components);
        }

        public Expression Visit(IndexExpression expression)
        {
            var target = expression.Target.Accept(this);
            var indices = expression.Indices.Select(i => i.Accept(this)).ToArray();
            return ReferenceEquals(target, expression.Target) && indices.SequenceEqual(expression.Indices)
                ? expression
                : new IndexExpression(target, indices);
        }

        public Expression Visit(SliceExpression expression)
        {
            var target = expression.Target.Accept(this);
            var slices = expression.Slices.Select(s => s?.Accept(this)).ToArray();
            return ReferenceEquals(target, expression.Target) && slices.SequenceEqual(expression.Slices)
                ? expression
                : new SliceExpression(target, slices);
        }

        public Expression Visit(DerivativeExpression expression)
        {
            var function = expression.Function.Accept(this);
            var variable = expression.Variable.Accept(this);
            return ReferenceEquals(function, expression.Function) && ReferenceEquals(variable, expression.Variable)
                ? expression
                : new DerivativeExpression(function, variable, expression.Order);
        }

public Expression Visit(IntegralExpression expression)
        {
            var integrand = expression.Integrand.Accept(this);
            var variable = expression.Variable.Accept(this);
            var lower = expression.LowerBound?.Accept(this);
            var upper = expression.UpperBound?.Accept(this);
            if (ReferenceEquals(integrand, expression.Integrand) &&
                ReferenceEquals(variable, expression.Variable) &&
                ReferenceEquals(lower, expression.LowerBound) &&
                ReferenceEquals(upper, expression.UpperBound))
                return expression;

            if (lower is not null && upper is not null)
                return new IntegralExpression(integrand, variable, lower, upper);

            return new IntegralExpression(integrand, variable);
        }

        public Expression Visit(SummationExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var lower = expression.LowerBound.Accept(this);
            var upper = expression.UpperBound.Accept(this);
            var body = expression.Body.Accept(this);
            return ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(lower, expression.LowerBound) &&
                   ReferenceEquals(upper, expression.UpperBound) &&
                   ReferenceEquals(body, expression.Body)
                ? expression
                : new SummationExpression(variable, lower, upper, body);
        }

        public Expression Visit(ProductExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var lower = expression.LowerBound.Accept(this);
            var upper = expression.UpperBound.Accept(this);
            var body = expression.Body.Accept(this);
            return ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(lower, expression.LowerBound) &&
                   ReferenceEquals(upper, expression.UpperBound) &&
                   ReferenceEquals(body, expression.Body)
                ? expression
                : new ProductExpression(variable, lower, upper, body);
        }

        public Expression Visit(LimitExpression expression)
        {
            var body = expression.Body.Accept(this);
            var variable = expression.Variable.Accept(this);
            var target = expression.Target.Accept(this);
            return ReferenceEquals(body, expression.Body) &&
                   ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(target, expression.Target)
                ? expression
                : new LimitExpression(body, variable, target, expression.Direction);
        }

        public Expression Visit(FactorialExpression expression)
        {
            var operand = expression.Operand.Accept(this);
            return ReferenceEquals(operand, expression.Operand) ? expression : new FactorialExpression(operand);
        }

        public Expression Visit(RangeExpression expression)
        {
            var start = expression.Start.Accept(this);
            var end = expression.End.Accept(this);
            var step = expression.Step?.Accept(this);
            return ReferenceEquals(start, expression.Start) &&
                   ReferenceEquals(end, expression.End) &&
                   ReferenceEquals(step, expression.Step)
                ? expression
                : new RangeExpression(start, end, step);
        }

        public Expression Visit(IntervalExpression expression)
        {
            var lower = expression.Lower.Accept(this);
            var upper = expression.Upper.Accept(this);
            return ReferenceEquals(lower, expression.Lower) && ReferenceEquals(upper, expression.Upper)
                ? expression
                : new IntervalExpression(lower, upper, expression.LowerClosed, expression.UpperClosed);
        }

        public Expression Visit(SetExpression expression)
        {
            var elements = expression.Elements.Select(e => e.Accept(this)).ToArray();
            return elements.SequenceEqual(expression.Elements) ? expression : new SetExpression(elements);
        }

        public Expression Visit(ComplexExpression expression)
        {
            var real = expression.Real.Accept(this);
            var imag = expression.Imaginary.Accept(this);
            return ReferenceEquals(real, expression.Real) && ReferenceEquals(imag, expression.Imaginary)
                ? expression
                : new ComplexExpression(real, imag);
        }

        public Expression Visit(PolynomialExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var coeffs = expression.Coefficients.Select(c => c.Accept(this)).ToArray();
            return ReferenceEquals(variable, expression.Variable) && coeffs.SequenceEqual(expression.Coefficients)
                ? expression
                : new PolynomialExpression(variable, coeffs);
        }

        public Expression Visit(BooleanExpression expression) => expression;
        public Expression Visit(RelationExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new RelationExpression(expression.Operator, left, right);
        }

        public Expression Visit(AssignmentExpression expression)
        {
            var target = expression.Target.Accept(this);
            var value = expression.Value.Accept(this);
            return ReferenceEquals(target, expression.Target) && ReferenceEquals(value, expression.Value)
                ? expression
                : new AssignmentExpression(target, value);
        }

        public Expression Visit(CompositionExpression expression)
        {
            var funcs = expression.Functions.Select(f => f.Accept(this)).ToArray();
            return funcs.SequenceEqual(expression.Functions) ? expression : new CompositionExpression(funcs);
        }

        public Expression Visit(IdentityExpression expression) => expression;
        public Expression Visit(NullExpression expression) => expression;

        public Expression Visit(AnnotatedExpression expression)
        {
            var inner = expression.Inner.Accept(this);
            return ReferenceEquals(inner, expression.Inner)
                ? expression
                : new AnnotatedExpression(inner, expression.Key, expression.AnnotationValue);
        }

        private sealed class PiecewiseCaseEqualityComparer : IEqualityComparer<PiecewiseCase>
        {
            public bool Equals(PiecewiseCase? x, PiecewiseCase? y) =>
                x?.Value.Equals(y?.Value) == true && x?.Condition.Equals(y?.Condition) == true;

            public int GetHashCode(PiecewiseCase obj) => HashCode.Combine(obj.Value, obj.Condition);
        }
    }
}