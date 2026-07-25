namespace MathVerse.Math.Core;

/// <summary>
/// Provides extension methods on <see cref="Expression"/> for common queries and transformations.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>Determines whether the expression is a constant (contains no variables or parameters).</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression tree contains no variable or parameter nodes; otherwise <c>false</c>.</returns>
    public static bool IsConstant(this Expression expression)
    {
        var hasVariable = false;
        expression.Accept(new VariableCollectorVisitor(name =>
        {
            hasVariable = true;
            return false;
        }));
        return !hasVariable;
    }

    /// <summary>Determines whether the expression is a variable node.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression is a <see cref="VariableExpression"/>; otherwise <c>false</c>.</returns>
    public static bool IsVariable(this Expression expression) =>
        expression.Kind == ExpressionKind.Variable;

    /// <summary>Determines whether the expression evaluates to zero.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression is a literal or constant with value 0; otherwise <c>false</c>.</returns>
    public static bool IsZero(this Expression expression) =>
        expression.GetDoubleValue().Match(
            v => System.Math.Abs(v) < double.Epsilon,
            _ => false);

    /// <summary>Determines whether the expression evaluates to one.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression is a literal or constant with value 1; otherwise <c>false</c>.</returns>
    public static bool IsOne(this Expression expression) =>
        expression.GetDoubleValue().Match(
            v => System.Math.Abs(v - 1.0) < double.Epsilon,
            _ => false);

    /// <summary>Determines whether the expression is a numeric integer value.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression is a constant with an integer value; otherwise <c>false</c>.</returns>
    public static bool IsInteger(this Expression expression) =>
        expression.GetDoubleValue().Match(
            v => v == System.Math.Floor(v) && !double.IsInfinity(v) && !double.IsNaN(v),
            _ => false);

    /// <summary>Determines whether the expression is a numeric literal (constant with a finite value).</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><c>true</c> if the expression is a <see cref="LiteralExpression"/> or a <see cref="ConstantExpression"/> with a finite value; otherwise <c>false</c>.</returns>
    public static bool IsNumericLiteral(this Expression expression) =>
        expression is LiteralExpression or ConstantExpression;

    /// <summary>Attempts to evaluate the expression as a <see cref="double"/> value.</summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>A <see cref="Maybe{T}"/> containing the numeric value if the expression is a constant; otherwise <c>undefined</c>.</returns>
    public static Maybe<double> GetDoubleValue(this Expression expression)
    {
        return expression switch
        {
            LiteralExpression lit => Maybe<double>.Defined(lit.Value),
            ConstantExpression c => Maybe<double>.Defined(c.Value),
            BooleanExpression b => Maybe<double>.Defined(b.Value ? 1.0 : 0.0),
            IdentityExpression => Maybe<double>.Defined(0.0),
            UnaryExpression u when u.Operator.Equals(MathOperator.Negate) =>
                u.Operand.GetDoubleValue().Map(v => -v),
            _ => Maybe<double>.Undefined(MaybeReason.NotSupported)
        };
    }

    /// <summary>Recursively collects all variable names from the expression tree.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns>An unordered set of all variable names found in the expression.</returns>
    public static IReadOnlySet<string> Variables(this Expression expression)
    {
        var variables = new HashSet<string>();
        expression.Accept(new VariableCollectorVisitor(name =>
        {
            variables.Add(name);
            return true;
        }));
        return variables;
    }

    /// <summary>
    /// Replaces all occurrences of a variable with the specified name by a replacement expression.
    /// </summary>
    /// <param name="expression">The expression to transform.</param>
    /// <param name="name">The variable name to replace.</param>
    /// <param name="replacement">The expression to substitute in place of the variable.</param>
    /// <returns>A new expression tree with all occurrences of the specified variable replaced.</returns>
    public static Expression ReplaceVariable(this Expression expression, string name, Expression replacement)
    {
        return expression.Accept(new VariableReplacer(name, replacement));
    }

    private sealed class VariableCollectorVisitor : IExpressionVisitor<bool>
    {
        private readonly Func<string, bool> _visitor;

        public VariableCollectorVisitor(Func<string, bool> visitor)
        {
            _visitor = visitor;
        }

        public bool Visit(LiteralExpression expression) => true;

        public bool Visit(VariableExpression expression) => _visitor(expression.Name);

        public bool Visit(ConstantExpression expression) => true;

        public bool Visit(BinaryExpression expression)
        {
            if (!expression.Left.Accept(this)) return false;
            return expression.Right.Accept(this);
        }

        public bool Visit(UnaryExpression expression) => expression.Operand.Accept(this);

        public bool Visit(FunctionCallExpression expression)
        {
            foreach (var arg in expression.Arguments)
            {
                if (!arg.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(LambdaExpression expression)
        {
            foreach (var p in expression.Parameters)
            {
                if (!p.Accept(this)) return false;
            }
            return expression.Body.Accept(this);
        }

        public bool Visit(ParameterExpression expression) => _visitor(expression.Name);

        public bool Visit(EquationExpression expression)
        {
            if (!expression.Left.Accept(this)) return false;
            return expression.Right.Accept(this);
        }

        public bool Visit(RelationExpression expression)
        {
            if (!expression.Left.Accept(this)) return false;
            return expression.Right.Accept(this);
        }

        public bool Visit(PiecewiseExpression expression)
        {
            foreach (var c in expression.Cases)
            {
                if (!c.Value.Accept(this)) return false;
                if (!c.Condition.Accept(this)) return false;
            }
            if (expression.DefaultCase is not null && !expression.DefaultCase.Accept(this)) return false;
            return true;
        }

        public bool Visit(ConditionalExpression expression)
        {
            if (!expression.Condition.Accept(this)) return false;
            if (!expression.ThenBranch.Accept(this)) return false;
            return expression.ElseBranch.Accept(this);
        }

        public bool Visit(TupleExpression expression)
        {
            foreach (var e in expression.Elements)
            {
                if (!e.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(VectorExpression expression)
        {
            foreach (var c in expression.Components)
            {
                if (!c.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(MatrixExpression expression)
        {
            foreach (var r in expression.Rows)
            {
                if (!r.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(TensorExpression expression)
        {
            foreach (var c in expression.Components)
            {
                if (!c.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(IndexExpression expression)
        {
            if (!expression.Target.Accept(this)) return false;
            foreach (var idx in expression.Indices)
            {
                if (!idx.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(SliceExpression expression) => expression.Target.Accept(this);

        public bool Visit(DerivativeExpression expression)
        {
            if (!expression.Function.Accept(this)) return false;
            return expression.Variable.Accept(this);
        }

        public bool Visit(IntegralExpression expression)
        {
            if (!expression.Integrand.Accept(this)) return false;
            if (!expression.Variable.Accept(this)) return false;
            if (expression.LowerBound is not null && !expression.LowerBound.Accept(this)) return false;
            if (expression.UpperBound is not null && !expression.UpperBound.Accept(this)) return false;
            return true;
        }

        public bool Visit(SummationExpression expression)
        {
            if (!expression.Variable.Accept(this)) return false;
            if (!expression.LowerBound.Accept(this)) return false;
            if (!expression.UpperBound.Accept(this)) return false;
            return expression.Body.Accept(this);
        }

        public bool Visit(ProductExpression expression)
        {
            if (!expression.Variable.Accept(this)) return false;
            if (!expression.LowerBound.Accept(this)) return false;
            if (!expression.UpperBound.Accept(this)) return false;
            return expression.Body.Accept(this);
        }

        public bool Visit(LimitExpression expression)
        {
            if (!expression.Body.Accept(this)) return false;
            if (!expression.Variable.Accept(this)) return false;
            return expression.Target.Accept(this);
        }

        public bool Visit(FactorialExpression expression) => expression.Operand.Accept(this);

        public bool Visit(RangeExpression expression)
        {
            if (!expression.Start.Accept(this)) return false;
            if (!expression.End.Accept(this)) return false;
            if (expression.Step is not null && !expression.Step.Accept(this)) return false;
            return true;
        }

        public bool Visit(IntervalExpression expression)
        {
            if (!expression.Lower.Accept(this)) return false;
            return expression.Upper.Accept(this);
        }

        public bool Visit(SetExpression expression)
        {
            foreach (var e in expression.Elements)
            {
                if (!e.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(ComplexExpression expression)
        {
            if (!expression.Real.Accept(this)) return false;
            return expression.Imaginary.Accept(this);
        }

        public bool Visit(PolynomialExpression expression)
        {
            if (!expression.Variable.Accept(this)) return false;
            foreach (var c in expression.Coefficients)
            {
                if (!c.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(BooleanExpression expression) => true;

        public bool Visit(AssignmentExpression expression)
        {
            if (!expression.Target.Accept(this)) return false;
            return expression.Value.Accept(this);
        }

        public bool Visit(CompositionExpression expression)
        {
            foreach (var f in expression.Functions)
            {
                if (!f.Accept(this)) return false;
            }
            return true;
        }

        public bool Visit(IdentityExpression expression) => true;

        public bool Visit(NullExpression expression) => true;

        public bool Visit(AnnotatedExpression expression) => expression.Inner.Accept(this);
    }

    private sealed class VariableReplacer : IExpressionTransformer
    {
        private readonly string _name;
        private readonly Expression _replacement;

        public VariableReplacer(string name, Expression replacement)
        {
            _name = name;
            _replacement = replacement;
        }

        public Expression Visit(LiteralExpression expression) => expression;

        public Expression Visit(VariableExpression expression) =>
            expression.Name == _name ? _replacement : expression;

        public Expression Visit(ConstantExpression expression) => expression;

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
            var changed = false;
            var args = new Expression[expression.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = expression.Arguments[i].Accept(this);
                if (!ReferenceEquals(args[i], expression.Arguments[i])) changed = true;
            }
            return changed ? new FunctionCallExpression(expression.Name, args) : expression;
        }

        public Expression Visit(LambdaExpression expression)
        {
            var body = expression.Body.Accept(this);
            return ReferenceEquals(body, expression.Body)
                ? expression
                : new LambdaExpression(expression.Parameters, body);
        }

        public Expression Visit(ParameterExpression expression) =>
            expression.Name == _name ? _replacement : expression;

        public Expression Visit(EquationExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new EquationExpression(left, right);
        }

        public Expression Visit(RelationExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new RelationExpression(expression.Operator, left, right);
        }

        public Expression Visit(PiecewiseExpression expression)
        {
            var changed = false;
            var cases = new PiecewiseCase[expression.Cases.Count];
            for (var i = 0; i < cases.Length; i++)
            {
                var value = expression.Cases[i].Value.Accept(this);
                var condition = expression.Cases[i].Condition.Accept(this);
                if (!ReferenceEquals(value, expression.Cases[i].Value) ||
                    !ReferenceEquals(condition, expression.Cases[i].Condition))
                    changed = true;
                cases[i] = new PiecewiseCase(value, condition);
            }

            Expression? defaultCase = null;
            if (expression.DefaultCase is not null)
            {
                defaultCase = expression.DefaultCase.Accept(this);
                if (!ReferenceEquals(defaultCase, expression.DefaultCase)) changed = true;
            }

            return changed ? new PiecewiseExpression(cases, defaultCase) : expression;
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
            var changed = false;
            var elements = new Expression[expression.Elements.Count];
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = expression.Elements[i].Accept(this);
                if (!ReferenceEquals(elements[i], expression.Elements[i])) changed = true;
            }
            return changed ? new TupleExpression(elements) : expression;
        }

        public Expression Visit(VectorExpression expression)
        {
            var changed = false;
            var components = new Expression[expression.Components.Count];
            for (var i = 0; i < components.Length; i++)
            {
                components[i] = expression.Components[i].Accept(this);
                if (!ReferenceEquals(components[i], expression.Components[i])) changed = true;
            }
            return changed ? new VectorExpression(components) : expression;
        }

        public Expression Visit(MatrixExpression expression)
        {
            var changed = false;
            var rows = new Expression[expression.Rows.Count];
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = expression.Rows[i].Accept(this);
                if (!ReferenceEquals(rows[i], expression.Rows[i])) changed = true;
            }
            return changed ? new MatrixExpression(rows) : expression;
        }

        public Expression Visit(TensorExpression expression)
        {
            var changed = false;
            var components = new Expression[expression.Components.Count];
            for (var i = 0; i < components.Length; i++)
            {
                components[i] = expression.Components[i].Accept(this);
                if (!ReferenceEquals(components[i], expression.Components[i])) changed = true;
            }
            return changed ? new TensorExpression(expression.Shape, components) : expression;
        }

        public Expression Visit(IndexExpression expression)
        {
            var target = expression.Target.Accept(this);
            var changed = !ReferenceEquals(target, expression.Target);
            var indices = new Expression[expression.Indices.Count];
            for (var i = 0; i < indices.Length; i++)
            {
                indices[i] = expression.Indices[i].Accept(this);
                if (!ReferenceEquals(indices[i], expression.Indices[i])) changed = true;
            }
            return changed ? new IndexExpression(target, indices) : expression;
        }

        public Expression Visit(SliceExpression expression)
        {
            var target = expression.Target.Accept(this);
            return ReferenceEquals(target, expression.Target)
                ? expression
                : new SliceExpression(target, expression.Slices);
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
            return ReferenceEquals(integrand, expression.Integrand) &&
                   ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(lower, expression.LowerBound) &&
                   ReferenceEquals(upper, expression.UpperBound)
                ? expression
                : lower is not null && upper is not null
                    ? new IntegralExpression(integrand, variable, lower, upper)
                    : new IntegralExpression(integrand, variable);
        }

        public Expression Visit(SummationExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var lowerBound = expression.LowerBound.Accept(this);
            var upperBound = expression.UpperBound.Accept(this);
            var body = expression.Body.Accept(this);
            return ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(lowerBound, expression.LowerBound) &&
                   ReferenceEquals(upperBound, expression.UpperBound) &&
                   ReferenceEquals(body, expression.Body)
                ? expression
                : new SummationExpression(variable, lowerBound, upperBound, body);
        }

        public Expression Visit(ProductExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var lowerBound = expression.LowerBound.Accept(this);
            var upperBound = expression.UpperBound.Accept(this);
            var body = expression.Body.Accept(this);
            return ReferenceEquals(variable, expression.Variable) &&
                   ReferenceEquals(lowerBound, expression.LowerBound) &&
                   ReferenceEquals(upperBound, expression.UpperBound) &&
                   ReferenceEquals(body, expression.Body)
                ? expression
                : new ProductExpression(variable, lowerBound, upperBound, body);
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
            return ReferenceEquals(operand, expression.Operand)
                ? expression
                : new FactorialExpression(operand);
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
            var changed = false;
            var elements = new Expression[expression.Elements.Count];
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = expression.Elements[i].Accept(this);
                if (!ReferenceEquals(elements[i], expression.Elements[i])) changed = true;
            }
            return changed ? new SetExpression(elements) : expression;
        }

        public Expression Visit(ComplexExpression expression)
        {
            var real = expression.Real.Accept(this);
            var imaginary = expression.Imaginary.Accept(this);
            return ReferenceEquals(real, expression.Real) && ReferenceEquals(imaginary, expression.Imaginary)
                ? expression
                : new ComplexExpression(real, imaginary);
        }

        public Expression Visit(PolynomialExpression expression)
        {
            var variable = expression.Variable.Accept(this);
            var changed = !ReferenceEquals(variable, expression.Variable);
            var coefficients = new Expression[expression.Coefficients.Count];
            for (var i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] = expression.Coefficients[i].Accept(this);
                if (!ReferenceEquals(coefficients[i], expression.Coefficients[i])) changed = true;
            }
            return changed ? new PolynomialExpression(variable, coefficients) : expression;
        }

        public Expression Visit(BooleanExpression expression) => expression;

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
            var changed = false;
            var functions = new Expression[expression.Functions.Count];
            for (var i = 0; i < functions.Length; i++)
            {
                functions[i] = expression.Functions[i].Accept(this);
                if (!ReferenceEquals(functions[i], expression.Functions[i])) changed = true;
            }
            return changed ? new CompositionExpression(functions) : expression;
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
    }
}
