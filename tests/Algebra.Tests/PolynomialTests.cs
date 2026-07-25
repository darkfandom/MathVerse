namespace MathVerse.Algebra.Tests;

public class PolynomialTests
{
    // ─── Construction: FromCoefficients ───

    [Fact]
    public void FromCoefficients_Lineic_CorrectDegree()
    {
        var p = Polynomial.FromCoefficients("x", 3, 2);

        p.Variable.Should().Be("x");
        p.Degree.Should().Be(1);
        p.CoefficientAt(0).Should().Be(3);
        p.CoefficientAt(1).Should().Be(2);
    }

    [Fact]
    public void FromCoefficients_Quadratic_CorrectCoefficients()
    {
        var p = Polynomial.FromCoefficients("x", 1, -5, 6);

        p.Degree.Should().Be(2);
        p.CoefficientAt(0).Should().Be(1);
        p.CoefficientAt(1).Should().Be(-5);
        p.CoefficientAt(2).Should().Be(6);
    }

    [Fact]
    public void FromCoefficients_SingleConstant_DegreeZero()
    {
        var p = Polynomial.FromCoefficients("x", 42);

        p.Degree.Should().Be(0);
        p.IsConstant.Should().BeTrue();
    }

    [Fact]
    public void FromCoefficients_MultipleTerms_PreservesOrder()
    {
        var p = Polynomial.FromCoefficients("t", 0, 1, 2, 3);

        p.Degree.Should().Be(3);
        p.CoefficientAt(0).Should().Be(0);
        p.CoefficientAt(1).Should().Be(1);
        p.CoefficientAt(2).Should().Be(2);
        p.CoefficientAt(3).Should().Be(3);
    }

    // ─── Construction: Zero ───

    [Fact]
    public void Zero_HasDegreeMinusOne()
    {
        var z = Polynomial.Zero("x");

        z.Degree.Should().Be(-1);
        z.IsZero.Should().BeTrue();
        z.Coefficients.Should().BeEmpty();
    }

    [Fact]
    public void Zero_Evaluate_ReturnsZero()
    {
        var z = Polynomial.Zero("x");

        z.Evaluate(5).Should().Be(0.0);
        z.Evaluate(0).Should().Be(0.0);
        z.Evaluate(-3).Should().Be(0.0);
    }

    // ─── Construction: One ───

    [Fact]
    public void One_IsConstantOne()
    {
        var one = Polynomial.One("x");

        one.Degree.Should().Be(0);
        one.IsConstant.Should().BeTrue();
        one.IsZero.Should().BeFalse();
        one.CoefficientAt(0).Should().Be(1.0);
    }

    [Fact]
    public void One_Evaluate_ReturnsOne()
    {
        var one = Polynomial.One("x");

        one.Evaluate(0).Should().Be(1.0);
        one.Evaluate(100).Should().Be(1.0);
    }

    // ─── Construction: Monomial ───

    [Fact]
    public void Monomial_DegreeFive_CoefficientThree()
    {
        var m = Polynomial.Monomial("x", 5, 3.0);

        m.Degree.Should().Be(5);
        m.CoefficientAt(5).Should().Be(3.0);
        m.CoefficientAt(0).Should().Be(0.0);
        m.CoefficientAt(4).Should().Be(0.0);
    }

    [Fact]
    public void Monomial_DegreeZero_BehavesLikeConstant()
    {
        var m = Polynomial.Monomial("x", 0, 7.0);

        m.Degree.Should().Be(0);
        m.IsConstant.Should().BeTrue();
        m.CoefficientAt(0).Should().Be(7.0);
    }

    [Fact]
    public void Monomial_NegativeDegree_Throws()
    {
        var act = () => Polynomial.Monomial("x", -1, 1.0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ─── Construction: FromRoots ───

    [Fact]
    public void FromRoots_SingleRoot_Linear()
    {
        var p = Polynomial.FromRoots("x", 5.0);

        p.Degree.Should().Be(1);
        p.Evaluate(5.0).Should().Be(0.0);
    }

    [Fact]
    public void FromRoots_TwoRoots_Quadratic()
    {
        var p = Polynomial.FromRoots("x", 1.0, 2.0);

        p.Degree.Should().Be(2);
        p.Evaluate(1.0).Should().Be(0.0);
        p.Evaluate(2.0).Should().Be(0.0);
    }

    [Fact]
    public void FromRoots_ThreeRoots_Cubic()
    {
        var p = Polynomial.FromRoots("x", 1.0, 2.0, 3.0);

        p.Degree.Should().Be(3);
        p.Evaluate(1.0).Should().BeApproximately(0.0, 1e-10);
        p.Evaluate(2.0).Should().BeApproximately(0.0, 1e-10);
        p.Evaluate(3.0).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void FromRoots_NoRoots_IsOne()
    {
        var p = Polynomial.FromRoots("x");

        p.Degree.Should().Be(0);
        p.CoefficientAt(0).Should().Be(1.0);
    }

    [Fact]
    public void FromRoots_RootAtZero_HasZeroConstantTerm()
    {
        var p = Polynomial.FromRoots("x", 0.0);

        p.CoefficientAt(0).Should().Be(0.0);
    }

    // ─── Properties: Degree ───

    [Fact]
    public void Degree_EmptyCoefficients_ReturnsMinusOne()
    {
        var p = Polynomial.Zero("x");

        p.Degree.Should().Be(-1);
    }

    [Fact]
    public void Degree_HighDegree_CalculatedCorrectly()
    {
        var coeffs = new double[100];
        coeffs[99] = 1.0;
        var p = new Polynomial("x", ImmutableArray.Create(coeffs));

        p.Degree.Should().Be(99);
    }

    // ─── Properties: LeadingCoefficient ───

    [Fact]
    public void LeadingCoefficient_NonZero_IsCorrect()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2, 3);

        p.LeadingCoefficient.Should().Be(3.0);
    }

    [Fact]
    public void LeadingCoefficient_ZeroPolynomial_IsZero()
    {
        var p = Polynomial.Zero("x");

        p.LeadingCoefficient.Should().Be(0.0);
    }

    // ─── Properties: IsZero ───

    [Fact]
    public void IsZero_TrueForZeroPolynomial()
    {
        Polynomial.Zero("x").IsZero.Should().BeTrue();
    }

    [Fact]
    public void IsZero_FalseForNonZeroPolynomial()
    {
        Polynomial.FromCoefficients("x", 0, 0, 0, 1).IsZero.Should().BeFalse();
    }

    // ─── Properties: IsConstant ───

    [Fact]
    public void IsConstant_TrueForConstantNonZero()
    {
        Polynomial.One("x").IsConstant.Should().BeTrue();
        Polynomial.FromCoefficients("x", 42).IsConstant.Should().BeTrue();
    }

    [Fact]
    public void IsConstant_FalseForZero()
    {
        Polynomial.Zero("x").IsConstant.Should().BeFalse();
    }

    [Fact]
    public void IsConstant_FalseForLinear()
    {
        Polynomial.FromCoefficients("x", 0, 1).IsConstant.Should().BeFalse();
    }

    // ─── Properties: IsLinear ───

    [Fact]
    public void IsLinear_TrueForDegreeOne()
    {
        Polynomial.FromCoefficients("x", 3, 2).IsLinear.Should().BeTrue();
    }

    [Fact]
    public void IsLinear_FalseForDegreeTwo()
    {
        Polynomial.FromCoefficients("x", 0, 0, 1).IsLinear.Should().BeFalse();
    }

    [Fact]
    public void IsLinear_FalseForConstant()
    {
        Polynomial.One("x").IsLinear.Should().BeFalse();
    }

    // ─── Properties: IsQuadratic ───

    [Fact]
    public void IsQuadratic_TrueForDegreeTwo()
    {
        Polynomial.FromCoefficients("x", 1, 0, 1).IsQuadratic.Should().BeTrue();
    }

    [Fact]
    public void IsQuadratic_FalseForDegreeOne()
    {
        Polynomial.FromCoefficients("x", 1, 1).IsQuadratic.Should().BeFalse();
    }

    // ─── CoefficientAt ───

    [Fact]
    public void CoefficientAt_BeyondDegree_ReturnsZero()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);

        p.CoefficientAt(100).Should().Be(0.0);
    }

    [Fact]
    public void CoefficientAt_NegativeDegree_ReturnsZero()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);

        p.CoefficientAt(-1).Should().Be(0.0);
    }

    // ─── Evaluate ───

    [Fact]
    public void Evaluate_Lineic_CorrectResult()
    {
        var p = Polynomial.FromCoefficients("x", 3, 2);

        p.Evaluate(0).Should().Be(3.0);
        p.Evaluate(1).Should().Be(5.0);
        p.Evaluate(2).Should().Be(7.0);
    }

    [Fact]
    public void Evaluate_Quadratic_CorrectResult()
    {
        var p = Polynomial.FromCoefficients("x", 0, 0, 1);

        p.Evaluate(3).Should().Be(9.0);
        p.Evaluate(-2).Should().Be(4.0);
    }

    [Fact]
    public void Evaluate_ZeroPolynomial_AlwaysZero()
    {
        var z = Polynomial.Zero("x");

        z.Evaluate(10).Should().Be(0.0);
        z.Evaluate(-5).Should().Be(0.0);
    }

    [Fact]
    public void Evaluate_ComplexPolynomial_CorrectHorner()
    {
        var p = Polynomial.FromCoefficients("x", 1, -3, 0, 2);

        p.Evaluate(1).Should().Be(0.0);
        p.Evaluate(0).Should().Be(1.0);
        p.Evaluate(2).Should().Be(11.0);
    }

    // ─── Arithmetic: Add ───

    [Fact]
    public void Add_SameDegree_CoefficientsSummed()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var b = Polynomial.FromCoefficients("x", 3, 4);

        var result = a.Add(b);

        result.CoefficientAt(0).Should().Be(4.0);
        result.CoefficientAt(1).Should().Be(6.0);
    }

    [Fact]
    public void Add_DifferentDegrees_PadsWithZeros()
    {
        var a = Polynomial.FromCoefficients("x", 1);
        var b = Polynomial.FromCoefficients("x", 0, 0, 5);

        var result = a.Add(b);

        result.Degree.Should().Be(2);
        result.CoefficientAt(0).Should().Be(1.0);
        result.CoefficientAt(2).Should().Be(5.0);
    }

    [Fact]
    public void Add_WithZero_IsIdentity()
    {
        var p = Polynomial.FromCoefficients("x", 3, 7);
        var z = Polynomial.Zero("x");

        var result = p.Add(z);

        result.Equals(p).Should().BeTrue();
    }

    [Fact]
    public void Add_DifferentVariable_Throws()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var b = Polynomial.FromCoefficients("y", 3, 4);

        var act = () => a.Add(b);

        act.Should().Throw<ArgumentException>();
    }

    // ─── Arithmetic: Subtract ───

    [Fact]
    public void Subtract_Identical_IsZero()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2, 3);

        var result = p.Subtract(p);

        result.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Subtract_DifferentDegree_CorrectResult()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2, 3);
        var b = Polynomial.FromCoefficients("x", 1, 1);

        var result = a.Subtract(b);

        result.CoefficientAt(0).Should().Be(0.0);
        result.CoefficientAt(1).Should().Be(1.0);
        result.CoefficientAt(2).Should().Be(3.0);
    }

    // ─── Arithmetic: Multiply ───

    [Fact]
    public void Multiply_LinearTimesLinear_Quadratic()
    {
        var a = Polynomial.FromCoefficients("x", 1, 1);
        var b = Polynomial.FromCoefficients("x", -1, 1);

        var result = a.Multiply(b);

        result.Degree.Should().Be(2);
        result.CoefficientAt(0).Should().Be(-1.0);
        result.CoefficientAt(1).Should().Be(0.0);
        result.CoefficientAt(2).Should().Be(1.0);
    }

    [Fact]
    public void Multiply_ByZero_IsZero()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2, 3);
        var z = Polynomial.Zero("x");

        p.Multiply(z).IsZero.Should().BeTrue();
        z.Multiply(p).IsZero.Should().BeTrue();
    }

    [Fact]
    public void Multiply_ByOne_IsIdentity()
    {
        var p = Polynomial.FromCoefficients("x", 5, -3);
        var one = Polynomial.One("x");

        p.Multiply(one).Equals(p).Should().BeTrue();
    }

    [Fact]
    public void Multiply_ThreeFactors_CorrectDegree()
    {
        var a = Polynomial.FromCoefficients("x", 0, 1);
        var b = Polynomial.FromCoefficients("x", 0, 1);
        var c = Polynomial.FromCoefficients("x", 0, 1);

        var result = a.Multiply(b).Multiply(c);

        result.Degree.Should().Be(3);
    }

    // ─── Arithmetic: Scale ───

    [Fact]
    public void Scale_ByTwo_DoublesCoefficients()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2, 3);

        var result = p.Scale(2.0);

        result.CoefficientAt(0).Should().Be(2.0);
        result.CoefficientAt(1).Should().Be(4.0);
        result.CoefficientAt(2).Should().Be(6.0);
    }

    [Fact]
    public void Scale_ByZero_IsZero()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);

        p.Scale(0.0).IsZero.Should().BeTrue();
    }

    [Fact]
    public void Scale_ByNegative_NegatesCoefficients()
    {
        var p = Polynomial.FromCoefficients("x", 3, -1);

        var result = p.Scale(-1.0);

        result.CoefficientAt(0).Should().Be(-3.0);
        result.CoefficientAt(1).Should().Be(1.0);
    }

    // ─── Arithmetic: Negate ───

    [Fact]
    public void Negate_Twice_IsIdentity()
    {
        var p = Polynomial.FromCoefficients("x", 1, -2, 3);

        p.Negate().Negate().Equals(p).Should().BeTrue();
    }

    [Fact]
    public void Negate_SignsFlipped()
    {
        var p = Polynomial.FromCoefficients("x", 5, -3);

        var result = p.Negate();

        result.CoefficientAt(0).Should().Be(-5.0);
        result.CoefficientAt(1).Should().Be(3.0);
    }

    // ─── Derivative ───

    [Fact]
    public void Derivative_Constant_IsZero()
    {
        var p = Polynomial.One("x");

        p.Derivative().IsZero.Should().BeTrue();
    }

    [Fact]
    public void Derivative_Lineic_IsConstant()
    {
        var p = Polynomial.FromCoefficients("x", 3, 5);

        var d = p.Derivative();

        d.Degree.Should().Be(0);
        d.CoefficientAt(0).Should().Be(5.0);
    }

    [Fact]
    public void Derivative_Quadratic_IsLinear()
    {
        var p = Polynomial.FromCoefficients("x", 1, 0, 3);

        var d = p.Derivative();

        d.Degree.Should().Be(1);
        d.CoefficientAt(0).Should().Be(0.0);
        d.CoefficientAt(1).Should().Be(6.0);
    }

    [Fact]
    public void Derivative_Cubic_CorrectCoefficients()
    {
        var p = Polynomial.FromCoefficients("x", 0, 0, 0, 1);

        var d = p.Derivative();

        d.Degree.Should().Be(2);
        d.CoefficientAt(2).Should().Be(3.0);
    }

    [Fact]
    public void Derivative_ZeroPolynomial_IsZero()
    {
        var z = Polynomial.Zero("x");

        z.Derivative().IsZero.Should().BeTrue();
    }

    // ─── Integral ───

    [Fact]
    public void Integral_Constant_LineicWithZeroConstant()
    {
        var p = Polynomial.One("x");

        var integral = p.Integral();

        integral.Degree.Should().Be(1);
        integral.CoefficientAt(0).Should().Be(0.0);
        integral.CoefficientAt(1).Should().Be(1.0);
    }

    [Fact]
    public void Integral_Lineic_Quadratic()
    {
        var p = Polynomial.FromCoefficients("x", 0, 1);

        var integral = p.Integral();

        integral.Degree.Should().Be(2);
        integral.CoefficientAt(2).Should().Be(0.5);
    }

    [Fact]
    public void Integral_ZeroPolynomial_RemainsZero()
    {
        var z = Polynomial.Zero("x");

        z.Integral().CoefficientAt(0).Should().Be(0.0);
    }

    // ─── ToString ───

    [Fact]
    public void ToString_Zero_ReturnsZero()
    {
        Polynomial.Zero("x").ToString().Should().Be("0");
    }

    [Fact]
    public void ToString_Lineic_FormatsCorrectly()
    {
        var p = Polynomial.FromCoefficients("x", 3, 2);

        var s = p.ToString();

        s.Should().Contain("x");
        s.Should().Contain("2");
        s.Should().Contain("3");
    }

    // ─── Equals ───

    [Fact]
    public void Equals_SameCoefficients_ReturnsTrue()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2, 3);
        var b = Polynomial.FromCoefficients("x", 1, 2, 3);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCoefficients_ReturnsFalse()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2, 3);
        var b = Polynomial.FromCoefficients("x", 1, 2, 4);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentVariable_ReturnsFalse()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var b = Polynomial.FromCoefficients("y", 1, 2);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);

        p.Equals(null).Should().BeFalse();
    }

    // ─── Variable property ───

    [Fact]
    public void Variable_IsPreserved()
    {
        var p = Polynomial.FromCoefficients("t", 1, 2);

        p.Variable.Should().Be("t");
    }

    [Fact]
    public void Variable_NullName_Throws()
    {
        var act = () => new Polynomial(null!, ImmutableArray<double>.Empty);

        act.Should().Throw<ArgumentNullException>();
    }
}
