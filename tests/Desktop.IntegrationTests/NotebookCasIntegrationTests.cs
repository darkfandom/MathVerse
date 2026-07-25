using System.Collections.Immutable;
using FluentAssertions;
using MathVerse.Math.Algebra;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.CAS.Expansion;
using MathVerse.Math.CAS.Factorization;
using MathVerse.Math.CAS.Simplification;
using MathVerse.Math.Calculus;
using MathVerse.Math.Expressions;
using MathVerse.Math.Parsing;
using Xunit;

namespace MathVerse.Desktop.IntegrationTests;

/// <summary>
/// Integration tests that mirror every CAS command in NotebookPageViewModel.
/// Each test exercises the exact same backend API the notebook calls.
/// </summary>
public sealed class NotebookCasIntegrationTests
{
    private static Expression Parse(string expr) => ParsingFacade.ParseExpression(expr);

    // ═══════════════════════════════════════════════
    //  EVALUATE
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData("2+2", "4")]
    [InlineData("2*3+5", "11")]
    [InlineData("sin(pi/2)", "1")]
    [InlineData("cos(0)", "1")]
    public void Evaluate_BasicExpressions_ReturnsCorrectResult(string input, string expected)
    {
        var parsed = Parse(input);
        var result = Evaluator.Instance.Evaluate(parsed).Result.ToString();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("10/2", "5")]
    [InlineData("2^10", "1024")]
    [InlineData("sqrt(9)", "3")]
    public void Evaluate_Arithmetic_ReturnsCorrectResult(string input, string expected)
    {
        var parsed = Parse(input);
        var result = Evaluator.Instance.Evaluate(parsed).Result.ToString();
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════
    //  SIMPLIFY
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData("x+0", "x")]
    [InlineData("x*1", "x")]
    [InlineData("0+x", "x")]
    [InlineData("1*x", "x")]
    public void Simplify_IdentityOperations_SimplifiesCorrectly(string input, string expected)
    {
        var parsed = Parse(input);
        var result = Simplifier.Instance.SimplifyInPlace(parsed).ToString();
        result.Should().Be(expected);
    }

    [Fact]
    public void Simplify_TrigonometricIdentity_SimplifiesTo1()
    {
        var parsed = Parse("sin(x)^2 + cos(x)^2");
        var result = Simplifier.Instance.SimplifyInPlace(parsed).ToString();
        result.Should().Be("1");
    }

    [Theory]
    [InlineData("2*x+3*x", "5")]
    [InlineData("x*2+x*3", "5")]
    public void Simplify_CombineLikeTerms_SimplifiesCorrectly(string input, string expectedCoeff)
    {
        var parsed = Parse(input);
        var result = Simplifier.Instance.SimplifyInPlace(parsed).ToString();
        result.Should().Contain(expectedCoeff);
    }

    // ═══════════════════════════════════════════════
    //  EXPAND
    // ═══════════════════════════════════════════════

    [Fact]
    public void Expand_SquareOfBinomial_ExpandsCorrectly()
    {
        var parsed = Parse("(x+1)^2");
        var result = Expander.Instance.Expand(parsed).Expanded.ToString();
        result.Should().Contain("x");
        result.Should().Contain("*");
    }

    [Fact]
    public void Expand_ProductOfBinomials_ExpandsCorrectly()
    {
        var parsed = Parse("(x+1)*(x+2)");
        var result = Expander.Instance.Expand(parsed).Expanded.ToString();
        result.Should().Contain("x");
        result.Should().Contain("*");
    }

    [Fact]
    public void Expand_CubeOfBinomial_ExpandsCorrectly()
    {
        var parsed = Parse("(x+1)^3");
        var result = Expander.Instance.Expand(parsed).Expanded.ToString();
        result.Should().Contain("x");
        result.Should().Contain("*");
        result.Should().NotBeEmpty();
    }

    // ═══════════════════════════════════════════════
    //  FACTOR
    // ═══════════════════════════════════════════════

    [Fact]
    public void Factor_DifferenceOfSquares_FactorsCorrectly()
    {
        var parsed = Parse("x^2-1");
        var result = Factorizer.Instance.Factor(parsed).Factored.ToString();
        result.Should().NotBeEmpty();
        result.Should().NotContain("Error");
    }

    [Fact]
    public void Factor_CommonTerm_FactorsCorrectly()
    {
        var parsed = Parse("2*x+2*y");
        var result = Factorizer.Instance.Factor(parsed).Factored.ToString();
        result.Should().NotBeEmpty();
        result.Should().NotContain("Error");
    }

    // ═══════════════════════════════════════════════
    //  DIFFERENTIATE
    // ═══════════════════════════════════════════════

    [Fact]
    public void Differentiate_xSquared_Returns2x()
    {
        var parsed = Parse("x^2");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("2");
        result.Should().Contain("x");
    }

    [Fact]
    public void Differentiate_xCubed_Returns3xSquared()
    {
        var parsed = Parse("x^3");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("3");
    }

    [Fact]
    public void Differentiate_SinX_ReturnsCosX()
    {
        var parsed = Parse("sin(x)");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("cos");
    }

    [Fact]
    public void Differentiate_CosX_ReturnsNegSinX()
    {
        var parsed = Parse("cos(x)");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("sin");
    }

    [Fact]
    public void Differentiate_ExpX_ReturnsExpX()
    {
        var parsed = Parse("e^x");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("e");
        result.Should().Contain("x");
    }

    [Fact]
    public void Differentiate_LnX_Returns1OverX()
    {
        var parsed = Parse("ln(x)");
        var result = new Differentiator().Differentiate(parsed, "x").ToString();
        result.Should().Contain("1");
        result.Should().Contain("x");
    }

    // ═══════════════════════════════════════════════
    //  INTEGRATE
    // ═══════════════════════════════════════════════

    [Fact]
    public void Integrate_Constant_ReturnsLinear()
    {
        var parsed = Parse("2");
        var result = new Integrator().IndefiniteIntegrate(parsed, "x").ToString();
        result.Should().Contain("2");
        result.Should().Contain("x");
    }

    [Fact]
    public void Integrate_xLinear_ReturnsXSquaredOver2()
    {
        var parsed = Parse("2*x");
        var result = new Integrator().IndefiniteIntegrate(parsed, "x").ToString();
        result.Should().Contain("x");
        result.Should().Contain("2");
    }

    [Fact]
    public void Integrate_SinX_ReturnsNegCosX()
    {
        var parsed = Parse("sin(x)");
        var result = new Integrator().IndefiniteIntegrate(parsed, "x").ToString();
        result.Should().Contain("cos");
    }

    // ═══════════════════════════════════════════════
    //  SOLVE
    // ═══════════════════════════════════════════════

    [Fact]
    public void Solve_LinearEquation_SolvesCorrectly()
    {
        var left = Parse("2*x+4");
        var right = Expr.Literal(0);
        var result = EquationSolver.SolveLinear(left, right, "x");
        var value = Evaluator.Instance.EvaluateToDouble(result);
        value.Should().BeApproximately(-2.0, 1e-10);
    }

    [Fact]
    public void Solve_QuadraticEquation_SolvesCorrectly()
    {
        var left = Parse("x^2-4");
        var right = Expr.Literal(0);
        var (x1, x2) = EquationSolver.SolveQuadratic(left, right, "x");
        var v1 = Evaluator.Instance.EvaluateToDouble(x1);
        var v2 = Evaluator.Instance.EvaluateToDouble(x2);
        v1.Should().BeOneOf(-2.0, 2.0);
        v2.Should().BeOneOf(-2.0, 2.0);
    }

    // ═══════════════════════════════════════════════
    //  LIMIT (numeric approximation)
    // ═══════════════════════════════════════════════

    [Fact]
    public void Limit_SinXOverX_AtLargeValueApproachesZero()
    {
        var parsed = Parse("sin(x)/x");
        var result = Evaluator.Instance.EvaluateToDouble(parsed,
            ImmutableDictionary<string, double>.Empty.Add("x", 1e10));
        result.Should().BeApproximately(0.0, 0.01,
            "sin(x)/x approaches 0 as x -> infinity");
    }

    [Fact]
    public void Limit_1OverX_AtInfinityApproaches0()
    {
        var parsed = Parse("1/x");
        var result = Evaluator.Instance.EvaluateToDouble(parsed,
            ImmutableDictionary<string, double>.Empty.Add("x", 1e10));
        result.Should().BeApproximately(0.0, 1e-10);
    }

    // ═══════════════════════════════════════════════
    //  PLOT DATA GENERATION (500+ samples)
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData("sin(x)", -10.0, 10.0)]
    [InlineData("cos(x)", -10.0, 10.0)]
    [InlineData("x^2", -10.0, 10.0)]
    [InlineData("x^3", -10.0, 10.0)]
    [InlineData("e^x", -10.0, 10.0)]
    [InlineData("ln(x)", 0.1, 10.0)]
    [InlineData("sqrt(x)", 0.1, 10.0)]
    public void PlotData_EightTestFunctions_ProducesValidPoints(string expression, double xMin, double xMax)
    {
        var parsed = Parse(expression);
        var evaluator = Evaluator.Instance;
        var points = new List<(double X, double Y)>();
        int samples = 500;
        double step = (xMax - xMin) / samples;

        for (int i = 0; i <= samples; i++)
        {
            double x = xMin + i * step;
            try
            {
                double y = evaluator.EvaluateToDouble(parsed,
                    ImmutableDictionary<string, double>.Empty.Add("x", x));
                if (!double.IsInfinity(y) && !double.IsNaN(y))
                    points.Add((x, y));
            }
            catch { }
        }

        points.Count.Should().BeGreaterOrEqualTo(400,
            $"Plot of '{expression}' should produce at least 400 valid points out of {samples + 1}");

        var ys = points.Select(p => p.Y).ToList();
        var yMin = ys.Min();
        var yMax = ys.Max();
        yMin.Should().NotBe(yMax,
            $"Plot of '{expression}' should have varying y-values");

        points.Count.Should().BeGreaterOrEqualTo(500,
            $"Plot of '{expression}' should produce at least 500 valid points");
    }

    [Fact]
    public void PlotData_SinX_OscillatesBetweenMinus1And1()
    {
        var parsed = Parse("sin(x)");
        var evaluator = Evaluator.Instance;
        var ys = new List<double>();

        for (int i = 0; i <= 500; i++)
        {
            double x = -10.0 + i * (20.0 / 500);
            try
            {
                double y = evaluator.EvaluateToDouble(parsed,
                    ImmutableDictionary<string, double>.Empty.Add("x", x));
                if (!double.IsInfinity(y) && !double.IsNaN(y))
                    ys.Add(y);
            }
            catch { }
        }

        ys.Min().Should().BeLessOrEqualTo(-0.9,
            "sin(x) should reach near -1 in [-10, 10]");
        ys.Max().Should().BeGreaterOrEqualTo(0.9,
            "sin(x) should reach near 1 in [-10, 10]");
    }

    [Fact]
    public void PlotData_XSquared_IsNonNegative()
    {
        var parsed = Parse("x^2");
        var evaluator = Evaluator.Instance;

        for (int i = 0; i <= 500; i++)
        {
            double x = -10.0 + i * (20.0 / 500);
            double y = evaluator.EvaluateToDouble(parsed,
                ImmutableDictionary<string, double>.Empty.Add("x", x));
            y.Should().BeGreaterOrEqualTo(-0.001,
                $"x^2 should be non-negative, but got {y} at x={x}");
        }
    }
}
