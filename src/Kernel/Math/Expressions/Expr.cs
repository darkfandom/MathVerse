namespace MathVerse.Math.Expressions;

/// <summary>
/// Ergonomic factory for constructing mathematical expressions.
/// All methods validate arguments and return immutable expression trees.
/// </summary>
public static class Expr
{
    // ─── Leaf Nodes ───

    /// <summary>Creates a literal expression.</summary>
    public static LiteralExpression Literal(double value) => new(value);

    /// <summary>Creates a variable expression.</summary>
    public static VariableExpression Variable(string name) => new(name);

    /// <summary>Creates a constant expression.</summary>
    public static ConstantExpression Constant(string name, double value) => new(name, value);

    /// <summary>Creates a boolean expression.</summary>
    public static BooleanExpression Boolean(bool value) => new(value);

    /// <summary>Creates a parameter expression.</summary>
    public static ParameterExpression Parameter(string name) => new(name);

    /// <summary>Creates an identity expression.</summary>
    public static IdentityExpression Identity(string operation) => new(operation);

    /// <summary>Gets the null expression singleton.</summary>
    public static NullExpression Null => NullExpression.Instance;

    // ─── Arithmetic ───

    /// <summary>Addition: left + right</summary>
    public static BinaryExpression Add(Expression left, Expression right) =>
        new(MathOperator.Add, left, right);

    /// <summary>Subtraction: left - right</summary>
    public static BinaryExpression Subtract(Expression left, Expression right) =>
        new(MathOperator.Subtract, left, right);

    /// <summary>Multiplication: left * right</summary>
    public static BinaryExpression Multiply(Expression left, Expression right) =>
        new(MathOperator.Multiply, left, right);

    /// <summary>Division: left / right</summary>
    public static BinaryExpression Divide(Expression left, Expression right) =>
        new(MathOperator.Divide, left, right);

    /// <summary>Modulo: left % right</summary>
    public static BinaryExpression Modulo(Expression left, Expression right) =>
        new(MathOperator.Modulo, left, right);

    /// <summary>Power: base ^ exponent</summary>
    public static BinaryExpression Pow(Expression baseExpr, Expression exponent) =>
        new(MathOperator.Power, baseExpr, exponent);

    /// <summary>Negation: -operand</summary>
    public static UnaryExpression Negate(Expression operand) =>
        new(MathOperator.Negate, operand);

    /// <summary>Absolute value: |operand|</summary>
    public static UnaryExpression Abs(Expression operand) =>
        new(MathOperator.Abs, operand);

    // ─── Relational ───

    /// <summary>Equal to: left == right</summary>
    public static RelationExpression Equal(Expression left, Expression right) =>
        new(MathOperator.Equal, left, right);

    /// <summary>Not equal to: left != right</summary>
    public static RelationExpression NotEqual(Expression left, Expression right) =>
        new(MathOperator.NotEqual, left, right);

    /// <summary>Less than: left &lt; right</summary>
    public static RelationExpression LessThan(Expression left, Expression right) =>
        new(MathOperator.LessThan, left, right);

    /// <summary>Greater than: left &gt; right</summary>
    public static RelationExpression GreaterThan(Expression left, Expression right) =>
        new(MathOperator.GreaterThan, left, right);

    /// <summary>Less than or equal: left &lt;= right</summary>
    public static RelationExpression LessThanOrEqual(Expression left, Expression right) =>
        new(MathOperator.LessThanOrEqual, left, right);

    /// <summary>Greater than or equal: left &gt;= right</summary>
    public static RelationExpression GreaterThanOrEqual(Expression left, Expression right) =>
        new(MathOperator.GreaterThanOrEqual, left, right);

    // ─── Logical ───

    /// <summary>Logical AND: left ∧ right</summary>
    public static BinaryExpression And(Expression left, Expression right) =>
        new(MathOperator.And, left, right);

    /// <summary>Logical OR: left ∨ right</summary>
    public static BinaryExpression Or(Expression left, Expression right) =>
        new(MathOperator.Or, left, right);

    /// <summary>Logical NOT: ¬operand</summary>
    public static UnaryExpression Not(Expression operand) =>
        new(MathOperator.Not, operand);

    // ─── Functions ───

    /// <summary>Creates a function call expression.</summary>
    public static FunctionCallExpression Call(string name, params Expression[] arguments) =>
        new(name, arguments);

    /// <summary>Sine function.</summary>
    public static FunctionCallExpression Sin(Expression x) => Call("sin", x);

    /// <summary>Cosine function.</summary>
    public static FunctionCallExpression Cos(Expression x) => Call("cos", x);

    /// <summary>Tangent function.</summary>
    public static FunctionCallExpression Tan(Expression x) => Call("tan", x);

    /// <summary>Arcsine function.</summary>
    public static FunctionCallExpression Asin(Expression x) => Call("asin", x);

    /// <summary>Arccosine function.</summary>
    public static FunctionCallExpression Acos(Expression x) => Call("acos", x);

    /// <summary>Arctangent function.</summary>
    public static FunctionCallExpression Atan(Expression x) => Call("atan", x);

    /// <summary>Natural logarithm.</summary>
    public static FunctionCallExpression Ln(Expression x) => Call("ln", x);

    /// <summary>Logarithm base 10.</summary>
    public static FunctionCallExpression Log10(Expression x) => Call("log10", x);

    /// <summary>Logarithm with arbitrary base.</summary>
    public static FunctionCallExpression Log(Expression x, Expression baseExpr) => Call("log", x, baseExpr);

    /// <summary>Exponential function (e^x).</summary>
    public static FunctionCallExpression Exp(Expression x) => Call("exp", x);

    /// <summary>Square root.</summary>
    public static FunctionCallExpression Sqrt(Expression x) => Call("sqrt", x);

    /// <summary>Cube root.</summary>
    public static FunctionCallExpression Cbrt(Expression x) => Call("cbrt", x);

    /// <summary>Hyperbolic sine.</summary>
    public static FunctionCallExpression Sinh(Expression x) => Call("sinh", x);

    /// <summary>Hyperbolic cosine.</summary>
    public static FunctionCallExpression Cosh(Expression x) => Call("cosh", x);

    /// <summary>Hyperbolic tangent.</summary>
    public static FunctionCallExpression Tanh(Expression x) => Call("tanh", x);

    // ─── Lambda & Parameters ───

    /// <summary>Creates a lambda expression.</summary>
    public static LambdaExpression Lambda(IReadOnlyList<ParameterExpression> parameters, Expression body) =>
        new(parameters, body);

    /// <summary>Creates a lambda expression with a single parameter.</summary>
    public static LambdaExpression Lambda(ParameterExpression parameter, Expression body) =>
        new([parameter], body);

    // ─── Equations ───

    /// <summary>Creates an equation: left = right</summary>
    public static EquationExpression Equation(Expression left, Expression right) =>
        new(left, right);

    // ─── Conditionals ───

    /// <summary>Creates a conditional: if condition then thenBranch else elseBranch</summary>
    public static ConditionalExpression Conditional(Expression condition, Expression thenBranch, Expression elseBranch) =>
        new(condition, thenBranch, elseBranch);

    /// <summary>Creates a piecewise expression.</summary>
    public static PiecewiseExpression Piecewise(IReadOnlyList<PiecewiseCase> cases, Expression? defaultCase = null) =>
        new(cases, defaultCase);

    // ─── Tuples ───

    /// <summary>Creates a tuple expression.</summary>
    public static TupleExpression Tuple(params Expression[] elements) =>
        new(elements);

    // ─── Linear Algebra ───

    /// <summary>Creates a vector expression.</summary>
    public static VectorExpression Vector(params Expression[] components) =>
        new(components);

    /// <summary>Creates a matrix expression from rows.</summary>
    public static MatrixExpression Matrix(params Expression[] rows) =>
        new(rows);

    /// <summary>Creates a matrix from a 2D array of values.</summary>
    public static MatrixExpression Matrix(double[,] values)
    {
        var rows = new VectorExpression[values.GetLength(0)];
        for (var i = 0; i < rows.Length; i++)
        {
            var components = new Expression[values.GetLength(1)];
            for (var j = 0; j < components.Length; j++)
                components[j] = new LiteralExpression(values[i, j]);
            rows[i] = new VectorExpression(components);
        }
        return new MatrixExpression(rows);
    }

    /// <summary>Creates a tensor expression.</summary>
    public static TensorExpression Tensor(IReadOnlyList<int> shape, params Expression[] components) =>
        new(shape, components);

    /// <summary>Creates an index expression.</summary>
    public static IndexExpression Index(Expression target, params Expression[] indices) =>
        new(target, indices);

    /// <summary>Creates a slice expression.</summary>
    public static SliceExpression Slice(Expression target, params Expression?[] slices) =>
        new(target, slices);

    /// <summary>Creates a transpose expression.</summary>
    public static UnaryExpression Transpose(Expression matrix) =>
        new(MathOperator.Transpose, matrix);

    // ─── Calculus ───

    /// <summary>Creates a first derivative: d/dvariable function</summary>
    public static DerivativeExpression Derivative(Expression function, Expression variable) =>
        new(function, variable);

    /// <summary>Creates an n-th derivative.</summary>
    public static DerivativeExpression Derivative(Expression function, Expression variable, int order) =>
        new(function, variable, order);

    /// <summary>Creates an indefinite integral.</summary>
    public static IntegralExpression Integral(Expression integrand, Expression variable) =>
        new(integrand, variable);

    /// <summary>Creates a definite integral.</summary>
    public static IntegralExpression Integral(Expression integrand, Expression variable, Expression lowerBound, Expression upperBound) =>
        new(integrand, variable, lowerBound, upperBound);

    /// <summary>Creates a summation.</summary>
    public static SummationExpression Summation(Expression variable, Expression lowerBound, Expression upperBound, Expression body) =>
        new(variable, lowerBound, upperBound, body);

    /// <summary>Creates a product.</summary>
    public static ProductExpression Product(Expression variable, Expression lowerBound, Expression upperBound, Expression body) =>
        new(variable, lowerBound, upperBound, body);

    /// <summary>Creates a limit expression.</summary>
    public static LimitExpression Limit(Expression body, Expression variable, Expression target, LimitDirection direction = LimitDirection.Both) =>
        new(body, variable, target, direction);

    // ─── Combinatorics & Discrete ───

    /// <summary>Creates a factorial expression.</summary>
    public static FactorialExpression Factorial(Expression operand) =>
        new(operand);

    // ─── Sets & Intervals ───

    /// <summary>Creates a range expression.</summary>
    public static RangeExpression Range(Expression start, Expression end, Expression? step = null) =>
        new(start, end, step);

    /// <summary>Creates an interval expression.</summary>
    public static IntervalExpression Interval(Expression lower, Expression upper, bool lowerClosed = true, bool upperClosed = true) =>
        new(lower, upper, lowerClosed, upperClosed);

    /// <summary>Creates a set expression.</summary>
    public static SetExpression Set(params Expression[] elements) =>
        new(elements);

    // ─── Complex Numbers ───

    /// <summary>Creates a complex number expression: real + imaginary*i</summary>
    public static ComplexExpression Complex(Expression real, Expression imaginary) =>
        new(real, imaginary);

    // ─── Polynomials ───

    /// <summary>Creates a polynomial expression.</summary>
    public static PolynomialExpression Polynomial(Expression variable, params Expression[] coefficients) =>
        new(variable, coefficients);

    // ─── Assignment & Composition ───

    /// <summary>Creates an assignment expression.</summary>
    public static AssignmentExpression Assign(Expression target, Expression value) =>
        new(target, value);

    /// <summary>Creates a function composition expression.</summary>
    public static CompositionExpression Compose(params Expression[] functions) =>
        new(functions);

    // ─── String-Based Convenience Methods ───

    /// <summary>Creates a binary expression from an operator string.</summary>
    public static BinaryExpression Binary(Expression left, string op, Expression right) =>
        new(GetOperator(op), left, right);

    /// <summary>Creates a unary expression from an operator string.</summary>
    public static UnaryExpression Unary(string op, Expression operand) =>
        new(GetOperator(op), operand);

    /// <summary>Creates an assignment: variable name = value.</summary>
    public static AssignmentExpression Assign(string targetName, Expression value) =>
        new(new VariableExpression(targetName), value);

    /// <summary>Creates a matrix from a jagged array of rows.</summary>
    public static MatrixExpression Matrix(Expression[][] rows) =>
        new(rows.Select(r => new VectorExpression(r)).Cast<Expression>().ToArray());

    /// <summary>Looks up a MathOperator by its symbol string.</summary>
    public static MathOperator GetOperator(string symbol) => symbol switch
    {
        "+" => MathOperator.Add,
        "-" => MathOperator.Subtract,
        "*" => MathOperator.Multiply,
        "/" => MathOperator.Divide,
        "%" => MathOperator.Modulo,
        "^" => MathOperator.Power,
        "==" => MathOperator.Equal,
        "!=" => MathOperator.NotEqual,
        "<" => MathOperator.LessThan,
        ">" => MathOperator.GreaterThan,
        "<=" => MathOperator.LessThanOrEqual,
        ">=" => MathOperator.GreaterThanOrEqual,
        "&&" or "∧" => MathOperator.And,
        "||" or "∨" => MathOperator.Or,
        "!" or "¬" => MathOperator.Not,
        "=" => MathOperator.Assign,
        _ => new MathOperator(symbol, symbol, OperatorCategory.Arithmetic, symbol.Length > 0 && "¬-|ᵀ⁻¹∂∇det".Contains(symbol[0]) ? 1 : 2, 1),
    };

    // ─── Convenience Methods ───

    /// <summary>Creates x².</summary>
    public static BinaryExpression Square(Expression x) => Pow(x, Literal(2));

    /// <summary>Creates x³.</summary>
    public static BinaryExpression Cube(Expression x) => Pow(x, Literal(3));

    /// <summary>Creates x*y.</summary>
    public static BinaryExpression Times(Expression x, Expression y) => Multiply(x, y);

    /// <summary>Creates a + b.</summary>
    public static BinaryExpression Plus(Expression a, Expression b) => Add(a, b);

    /// <summary>Creates -x.</summary>
    public static UnaryExpression Minus(Expression x) => Negate(x);
}
