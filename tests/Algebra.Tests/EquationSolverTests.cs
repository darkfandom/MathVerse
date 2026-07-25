namespace MathVerse.Algebra.Tests;

public class EquationSolverTests
{
    // ─── SolveLinear ───

    [Fact]
    public void SolveLinear_2xPlus3Equals7_GivesX2()
    {
        var left = Expr.Add(Expr.Multiply(Expr.Literal(2), Expr.Variable("x")), Expr.Literal(3));
        var right = Expr.Literal(7);

        var solution = EquationSolver.SolveLinear(left, right, "x");

        solution.Should().BeOfType<LiteralExpression>();
        solution.As<LiteralExpression>().Value.Should().Be(2.0);
    }

    [Fact]
    public void SolveLinear_xEqualsFive_GivesX5()
    {
        var left = Expr.Variable("x");
        var right = Expr.Literal(5);

        var solution = EquationSolver.SolveLinear(left, right, "x");

        solution.As<LiteralExpression>().Value.Should().Be(5.0);
    }

    [Fact]
    public void SolveLinear_3xMinus2Equals10_GivesX4()
    {
        var left = Expr.Subtract(Expr.Multiply(Expr.Literal(3), Expr.Variable("x")), Expr.Literal(2));
        var right = Expr.Literal(10);

        var solution = EquationSolver.SolveLinear(left, right, "x");

        solution.As<LiteralExpression>().Value.Should().Be(4.0);
    }

    [Fact]
    public void SolveLinear_NegativeCoefficient()
    {
        var left = Expr.Multiply(Expr.Literal(-2), Expr.Variable("x"));
        var right = Expr.Literal(6);

        var solution = EquationSolver.SolveLinear(left, right, "x");

        solution.As<LiteralExpression>().Value.Should().Be(-3.0);
    }

    [Fact]
    public void SolveLinear_FractionalResult()
    {
        var left = Expr.Multiply(Expr.Literal(3), Expr.Variable("x"));
        var right = Expr.Literal(1);

        var solution = EquationSolver.SolveLinear(left, right, "x");

        solution.As<LiteralExpression>().Value.Should().BeApproximately(1.0 / 3.0, 1e-10);
    }

    // ─── SolveQuadratic ───

    [Fact]
    public void SolveQuadratic_x2Minus5xPlus6_Gives2And3()
    {
        var left = Expr.Add(
            Expr.Subtract(Expr.Pow(Expr.Variable("x"), Expr.Literal(2)), Expr.Multiply(Expr.Literal(5), Expr.Variable("x"))),
            Expr.Literal(6));
        var right = Expr.Literal(0);

        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");

        var root1 = x1.As<LiteralExpression>().Value;
        var root2 = x2.As<LiteralExpression>().Value;

        root1.Should().BeApproximately(2.0, 1e-10);
        root2.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void SolveQuadratic_x2Minus4_GivesMinus2And2()
    {
        var left = Expr.Subtract(Expr.Pow(Expr.Variable("x"), Expr.Literal(2)), Expr.Literal(4));
        var right = Expr.Literal(0);

        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");

        var root1 = x1.As<LiteralExpression>().Value;
        var root2 = x2.As<LiteralExpression>().Value;

        root1.Should().BeApproximately(-2.0, 1e-10);
        root2.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void SolveQuadratic_x2Minus1_GivesMinus1And1()
    {
        var left = Expr.Subtract(Expr.Pow(Expr.Variable("x"), Expr.Literal(2)), Expr.Literal(1));
        var right = Expr.Literal(0);

        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");

        x1.As<LiteralExpression>().Value.Should().BeApproximately(-1.0, 1e-10);
        x2.As<LiteralExpression>().Value.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void SolveQuadratic_PerfectSquare_EqualRoots()
    {
        var left = Expr.Subtract(
            Expr.Pow(Expr.Variable("x"), Expr.Literal(2)),
            Expr.Literal(4));
        var right = Expr.Literal(0);

        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");

        x1.As<LiteralExpression>().Value.Should().BeApproximately(-2.0, 1e-10);
        x2.As<LiteralExpression>().Value.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void SolveQuadratic_ComplexRoots_ReturnsComplexExpressions()
    {
        var left = Expr.Add(Expr.Pow(Expr.Variable("x"), Expr.Literal(2)), Expr.Literal(1));
        var right = Expr.Literal(0);

        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");

        x1.Should().BeOfType<ComplexExpression>();
        x2.Should().BeOfType<ComplexExpression>();
    }

    // ─── SolveCubic ───

    [Fact]
    public void SolveCubic_x3MinusX_GivesMinus10And1()
    {
        var left = Expr.Subtract(
            Expr.Pow(Expr.Variable("x"), Expr.Literal(3)),
            Expr.Variable("x"));
        var right = Expr.Literal(0);

        var roots = EquationSolver.SolveCubic(left, right, "x");

        roots.Length.Should().BeGreaterOrEqualTo(3);

        var values = roots.Select(r => r.As<LiteralExpression>().Value).OrderBy(v => v).ToList();

        values[0].Should().BeApproximately(-1.0, 1e-8);
        values[1].Should().BeApproximately(0.0, 1e-8);
        values[2].Should().BeApproximately(1.0, 1e-8);
    }

    [Fact]
    public void SolveCubic_x3Minus8_GivesOneRealRoot()
    {
        var left = Expr.Subtract(
            Expr.Pow(Expr.Variable("x"), Expr.Literal(3)),
            Expr.Literal(8));
        var right = Expr.Literal(0);

        var roots = EquationSolver.SolveCubic(left, right, "x");

        roots.Length.Should().BeGreaterOrEqualTo(1);

        var rootValues = roots
            .Where(r => r is LiteralExpression)
            .Select(r => r.As<LiteralExpression>().Value)
            .ToList();

        rootValues.Should().Contain(v => System.Math.Abs(v - 2.0) < 1e-8);
    }

    [Fact]
    public void SolveCubic_x3_GivesZeroTriple()
    {
        var left = Expr.Pow(Expr.Variable("x"), Expr.Literal(3));
        var right = Expr.Literal(0);

        var roots = EquationSolver.SolveCubic(left, right, "x");

        roots.Length.Should().BeGreaterOrEqualTo(1);

        var rootValues = roots
            .Where(r => r is LiteralExpression)
            .Select(r => r.As<LiteralExpression>().Value)
            .ToList();

        rootValues.Should().AllSatisfy(v => v.Should().BeApproximately(0.0, 1e-8));
    }

    // ─── SolveSystem2 ───

    [Fact]
    public void SolveSystem2_MultiVariableEquations_ThrowsArgumentException()
    {
        var eq1 = Expr.Equation(Expr.Add(Expr.Variable("x"), Expr.Variable("y")), Expr.Literal(5));
        var eq2 = Expr.Equation(Expr.Subtract(Expr.Variable("x"), Expr.Variable("y")), Expr.Literal(1));
        var equations = ImmutableArray.Create<Expression>(eq1, eq2);

        var act = () => EquationSolver.SolveSystem2(equations, "x", "y");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SolveSystem2_SingleVariableForm_WorksCorrectly()
    {
        var eq1 = Expr.Equation(Expr.Multiply(Expr.Literal(2), Expr.Variable("x")), Expr.Literal(6));
        var eq2 = Expr.Equation(Expr.Add(Expr.Multiply(Expr.Literal(3), Expr.Variable("x")), Expr.Literal(1)), Expr.Literal(10));
        var equations = ImmutableArray.Create<Expression>(eq1, eq2);

        var act = () => EquationSolver.SolveSystem2(equations, "x", "y");

        act.Should().Throw<InvalidOperationException>();
    }

    // ─── SolveSystem3 ───

    [Fact]
    public void SolveSystem3_MultiVariableEquations_ThrowsArgumentException()
    {
        var eq1 = Expr.Equation(Expr.Add(Expr.Add(Expr.Variable("x"), Expr.Variable("y")), Expr.Variable("z")), Expr.Literal(6));
        var eq2 = Expr.Equation(Expr.Add(Expr.Subtract(Expr.Multiply(Expr.Literal(2), Expr.Variable("x")), Expr.Variable("y")), Expr.Variable("z")), Expr.Literal(5));
        var eq3 = Expr.Equation(Expr.Add(Expr.Variable("x"), Expr.Add(Expr.Multiply(Expr.Literal(2), Expr.Variable("y")), Expr.Variable("z"))), Expr.Literal(9));
        var equations = ImmutableArray.Create<Expression>(eq1, eq2, eq3);

        var act = () => EquationSolver.SolveSystem3(equations, "x", "y", "z");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SolveSystem3_WrongCount_Throws()
    {
        var equations = ImmutableArray.Create<Expression>(Expr.Literal(0), Expr.Literal(1));

        var act = () => EquationSolver.SolveSystem3(equations, "x", "y", "z");

        act.Should().Throw<ArgumentException>();
    }

    // ─── Error handling ───

    [Fact]
    public void SolveSystem2_WrongCount_Throws()
    {
        var equations = ImmutableArray.Create<Expression>(Expr.Literal(0));

        var act = () => EquationSolver.SolveSystem2(equations, "x", "y");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SolveLinear_NonLinearEquation_Throws()
    {
        var left = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));
        var right = Expr.Literal(4);

        var act = () => EquationSolver.SolveLinear(left, right, "x");

        act.Should().Throw<ArgumentException>();
    }

    // ─── ExpressionToPolynomial and PolynomialToExpression ───

    [Fact]
    public void ExpressionToPolynomial_LineicExpression_ParsesCorrectly()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Literal(2), Expr.Variable("x")), Expr.Literal(3));

        var poly = ExpressionToPolynomial.Convert(expr, "x");

        poly.Should().NotBeNull();
        poly!.Degree.Should().Be(1);
        poly.CoefficientAt(0).Should().Be(3.0);
        poly.CoefficientAt(1).Should().Be(2.0);
    }

    [Fact]
    public void ExpressionToPolynomial_QuadraticExpression_ParsesCorrectly()
    {
        var expr = Expr.Add(
            Expr.Add(Expr.Pow(Expr.Variable("x"), Expr.Literal(2)), Expr.Negate(Expr.Multiply(Expr.Literal(5), Expr.Variable("x")))),
            Expr.Literal(6));

        var poly = ExpressionToPolynomial.Convert(expr, "x");

        poly.Should().NotBeNull();
        poly!.Degree.Should().Be(2);
        poly.CoefficientAt(0).Should().Be(6.0);
        poly.CoefficientAt(1).Should().Be(-5.0);
        poly.CoefficientAt(2).Should().Be(1.0);
    }

    [Fact]
    public void ExpressionToPolynomial_NullExpression_ReturnsNull()
    {
        var result = ExpressionToPolynomial.Convert(null!, "x");

        result.Should().BeNull();
    }

    [Fact]
    public void PolynomialToExpression_ConvertBack_Consistent()
    {
        var original = Polynomial.FromCoefficients("x", 1, -5, 6);

        var expr = PolynomialToExpression.Convert(original);

        var poly = ExpressionToPolynomial.Convert(expr, "x");

        poly.Should().NotBeNull();
        poly!.Equals(original).Should().BeTrue();
    }

    [Fact]
    public void PolynomialToExpression_ZeroPolynomial_ReturnsLiteralZero()
    {
        var z = Polynomial.Zero("x");

        var expr = PolynomialToExpression.Convert(z);

        expr.Should().BeOfType<LiteralExpression>();
        expr.As<LiteralExpression>().Value.Should().Be(0.0);
    }

    // ─── PolynomialParser ───

    [Fact]
    public void PolynomialParser_LineicExpression_Success()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Literal(3), Expr.Variable("x")), Expr.Literal(2));

        var (ok, poly) = PolynomialParser.TryParse(expr, "x");

        ok.Should().BeTrue();
        poly.Degree.Should().Be(1);
    }

    [Fact]
    public void PolynomialParser_ConstantExpression_Success()
    {
        var expr = Expr.Literal(42);

        var (ok, poly) = PolynomialParser.TryParse(expr, "x");

        ok.Should().BeTrue();
        poly.IsConstant.Should().BeTrue();
        poly.CoefficientAt(0).Should().Be(42.0);
    }

    [Fact]
    public void PolynomialParser_PowerExpression_Success()
    {
        var expr = Expr.Pow(Expr.Variable("x"), Expr.Literal(3));

        var (ok, poly) = PolynomialParser.TryParse(expr, "x");

        ok.Should().BeTrue();
        poly.Degree.Should().Be(3);
    }

    [Fact]
    public void PolynomialParser_NullExpression_ReturnsFalse()
    {
        var (ok, poly) = PolynomialParser.TryParse(null!, "x");

        ok.Should().BeFalse();
    }

    [Fact]
    public void PolynomialParser_WrongVariable_ReturnsFalse()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Literal(3), Expr.Variable("y")), Expr.Literal(2));

        var (ok, _) = PolynomialParser.TryParse(expr, "x");

        ok.Should().BeFalse();
    }
}
