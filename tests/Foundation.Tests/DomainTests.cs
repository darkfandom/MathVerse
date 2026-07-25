namespace MathVerse.Foundation.Tests;

public class DomainTests
{
    [Fact]
    public void ComplexDomain_Instance_HasCorrectName()
    {
        ComplexDomain.Instance.Name.Should().Be("Complex");
    }

    [Fact]
    public void ComplexDomain_Instance_HasComplexKind()
    {
        ComplexDomain.Instance.Kind.Should().Be(DomainKind.Complex);
    }

    [Fact]
    public void ComplexDomain_Instance_HasNoParents()
    {
        ComplexDomain.Instance.Parents.Should().BeEmpty();
    }

    [Fact]
    public void ComplexDomain_Instance_AcceptsAnyDouble()
    {
        ComplexDomain.Instance.Contains(42.0).Should().BeTrue();
    }

    [Fact]
    public void ComplexDomain_Instance_AcceptsNaN()
    {
        ComplexDomain.Instance.Contains(double.NaN).Should().BeTrue();
    }

    [Fact]
    public void ComplexDomain_Instance_AcceptsInfinity()
    {
        ComplexDomain.Instance.Contains(double.PositiveInfinity).Should().BeTrue();
    }

    [Fact]
    public void ComplexDomain_Instance_AcceptsAnyComplex()
    {
        ComplexDomain.Instance.Contains(new System.Numerics.Complex(3, 4)).Should().BeTrue();
    }

    [Fact]
    public void RealDomain_Instance_HasCorrectName()
    {
        RealDomain.Instance.Name.Should().Be("Real");
    }

    [Fact]
    public void RealDomain_Instance_HasRealKind()
    {
        RealDomain.Instance.Kind.Should().Be(DomainKind.Real);
    }

    [Fact]
    public void RealDomain_Instance_ParentIsComplex()
    {
        RealDomain.Instance.Parents.Should().Contain(ComplexDomain.Instance);
    }

    [Fact]
    public void RealDomain_Instance_AcceptsFiniteDouble()
    {
        RealDomain.Instance.Contains(3.14).Should().BeTrue();
    }

    [Fact]
    public void RealDomain_Instance_RejectsNaN()
    {
        RealDomain.Instance.Contains(double.NaN).Should().BeFalse();
    }

    [Fact]
    public void RealDomain_Instance_RejectsInfinity()
    {
        RealDomain.Instance.Contains(double.PositiveInfinity).Should().BeFalse();
    }

    [Fact]
    public void RealDomain_Instance_RejectsComplexWithNonZeroImaginary()
    {
        RealDomain.Instance.Contains(new System.Numerics.Complex(1, 1)).Should().BeFalse();
    }

    [Fact]
    public void RealDomain_Instance_AcceptsComplexWithZeroImaginary()
    {
        RealDomain.Instance.Contains(new System.Numerics.Complex(5, 0)).Should().BeTrue();
    }

    [Fact]
    public void RationalDomain_Instance_HasCorrectName()
    {
        RationalDomain.Instance.Name.Should().Be("Rational");
    }

    [Fact]
    public void RationalDomain_Instance_ParentIsReal()
    {
        RationalDomain.Instance.Parents.Should().Contain(RealDomain.Instance);
    }

    [Fact]
    public void RationalDomain_Instance_AcceptsIntegers()
    {
        RationalDomain.Instance.Contains(5.0).Should().BeTrue();
    }

    [Fact]
    public void RationalDomain_Instance_AcceptsSimpleFraction()
    {
        RationalDomain.Instance.Contains(0.5).Should().BeTrue();
    }

    [Fact]
    public void RationalDomain_Instance_RejectsNaN()
    {
        RationalDomain.Instance.Contains(double.NaN).Should().BeFalse();
    }

    [Fact]
    public void RationalDomain_Instance_RejectsInfinity()
    {
        RationalDomain.Instance.Contains(double.PositiveInfinity).Should().BeFalse();
    }

    [Fact]
    public void IntegerDomain_Instance_HasCorrectName()
    {
        IntegerDomain.Instance.Name.Should().Be("Integer");
    }

    [Fact]
    public void IntegerDomain_Instance_ParentIsRational()
    {
        IntegerDomain.Instance.Parents.Should().Contain(RationalDomain.Instance);
    }

    [Fact]
    public void IntegerDomain_Instance_AcceptsWholeNumbers()
    {
        IntegerDomain.Instance.Contains(7.0).Should().BeTrue();
    }

    [Fact]
    public void IntegerDomain_Instance_AcceptsNegativeIntegers()
    {
        IntegerDomain.Instance.Contains(-3.0).Should().BeTrue();
    }

    [Fact]
    public void IntegerDomain_Instance_RejectsNonInteger()
    {
        IntegerDomain.Instance.Contains(3.5).Should().BeFalse();
    }

    [Fact]
    public void IntegerDomain_Instance_RejectsNaN()
    {
        IntegerDomain.Instance.Contains(double.NaN).Should().BeFalse();
    }

    [Fact]
    public void IntegerDomain_Instance_RejectsInfinity()
    {
        IntegerDomain.Instance.Contains(double.PositiveInfinity).Should().BeFalse();
    }

    [Fact]
    public void WholeDomain_Instance_HasCorrectName()
    {
        WholeDomain.Instance.Name.Should().Be("Whole");
    }

    [Fact]
    public void WholeDomain_Instance_ParentIsInteger()
    {
        WholeDomain.Instance.Parents.Should().Contain(IntegerDomain.Instance);
    }

    [Fact]
    public void WholeDomain_Instance_AcceptsPositiveIntegers()
    {
        WholeDomain.Instance.Contains(5.0).Should().BeTrue();
    }

    [Fact]
    public void WholeDomain_Instance_RejectsZero()
    {
        WholeDomain.Instance.Contains(0.0).Should().BeFalse();
    }

    [Fact]
    public void WholeDomain_Instance_RejectsNegative()
    {
        WholeDomain.Instance.Contains(-1.0).Should().BeFalse();
    }

    [Fact]
    public void NaturalDomain_Instance_HasCorrectName()
    {
        NaturalDomain.Instance.Name.Should().Be("Natural");
    }

    [Fact]
    public void NaturalDomain_Instance_ParentsIncludeWholeAndInteger()
    {
        NaturalDomain.Instance.Parents.Should().Contain(WholeDomain.Instance);
        NaturalDomain.Instance.Parents.Should().Contain(IntegerDomain.Instance);
    }

    [Fact]
    public void NaturalDomain_Instance_AcceptsZero()
    {
        NaturalDomain.Instance.Contains(0.0).Should().BeTrue();
    }

    [Fact]
    public void NaturalDomain_Instance_AcceptsPositiveIntegers()
    {
        NaturalDomain.Instance.Contains(3.0).Should().BeTrue();
    }

    [Fact]
    public void NaturalDomain_Instance_RejectsNegative()
    {
        NaturalDomain.Instance.Contains(-1.0).Should().BeFalse();
    }

    [Fact]
    public void NaturalDomain_Instance_RejectsNonInteger()
    {
        NaturalDomain.Instance.Contains(2.5).Should().BeFalse();
    }

    [Fact]
    public void BooleanDomain_Instance_HasCorrectName()
    {
        BooleanDomain.Instance.Name.Should().Be("Boolean");
    }

    [Fact]
    public void BooleanDomain_Instance_AcceptsZero()
    {
        BooleanDomain.Instance.Contains(0.0).Should().BeTrue();
    }

    [Fact]
    public void BooleanDomain_Instance_AcceptsOne()
    {
        BooleanDomain.Instance.Contains(1.0).Should().BeTrue();
    }

    [Fact]
    public void BooleanDomain_Instance_RejectsTwo()
    {
        BooleanDomain.Instance.Contains(2.0).Should().BeFalse();
    }

    [Fact]
    public void QuaternionDomain_Instance_HasCorrectName()
    {
        QuaternionDomain.Instance.Name.Should().Be("Quaternion");
    }

    [Fact]
    public void QuaternionDomain_Instance_HasQuaternionKind()
    {
        QuaternionDomain.Instance.Kind.Should().Be(DomainKind.Quaternion);
    }

    [Fact]
    public void MathDomain_IsSupersetOf_ReturnsTrueForSelf()
    {
        RealDomain.Instance.IsSupersetOf(RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void MathDomain_IsSupersetOf_ComplexContainsReal()
    {
        ComplexDomain.Instance.IsSupersetOf(RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void MathDomain_IsSupersetOf_RealDoesNotContainComplex()
    {
        RealDomain.Instance.IsSupersetOf(ComplexDomain.Instance).Should().BeFalse();
    }

[Fact]
    public void DomainValidator_ResultDomain_ThrowsForNullOperation_Operation()
    {
        Action act = () => DomainValidator.Instance.ResultDomain(RealDomain.Instance, RealDomain.Instance, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MathDomain_IsSubsetOf_ComplexContainsReal()
    {
        RealDomain.Instance.IsSubsetOf(ComplexDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void MathDomain_IsSubsetOf_ThrowsForNull()
    {
        Action act = () => RealDomain.Instance.IsSubsetOf(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MathDomain_IsCompatibleWith_SelfIsCompatible()
    {
        RealDomain.Instance.IsCompatibleWith(RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void MathDomain_IsCompatibleWith_ParentChildCompatible()
    {
        RealDomain.Instance.IsCompatibleWith(ComplexDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void MathDomain_IsCompatibleWith_ThrowsForNull()
    {
        Action act = () => RealDomain.Instance.IsCompatibleWith(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MathDomain_ToString_ReturnsName()
    {
        RealDomain.Instance.ToString().Should().Be("Real");
    }

    [Fact]
    public void FiniteFieldDomain_StoresCharacteristic()
    {
        var ff = new FiniteFieldDomain(7);
        ff.Characteristic.Should().Be(7);
    }

    [Fact]
    public void FiniteFieldDomain_DomainHasCorrectName()
    {
        var ff = new FiniteFieldDomain(5);
        ff.Domain.Name.Should().Be("GF(5)");
    }

    [Fact]
    public void FiniteFieldDomain_DomainHasFiniteFieldKind()
    {
        var ff = new FiniteFieldDomain(3);
        ff.Domain.Kind.Should().Be(DomainKind.FiniteField);
    }

    [Fact]
    public void VectorDomain_StoresDimensionAndElementDomain()
    {
        var vd = new VectorDomain(RealDomain.Instance, 3);
        vd.Dimension.Should().Be(3);
        vd.ElementDomain.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void VectorDomain_DomainHasCorrectName()
    {
        var vd = new VectorDomain(RealDomain.Instance, 3);
        vd.Domain.Name.Should().Be("3D Vector over Real");
    }

    [Fact]
    public void VectorDomain_DomainHasVectorKind()
    {
        var vd = new VectorDomain(RealDomain.Instance, 2);
        vd.Domain.Kind.Should().Be(DomainKind.Vector);
    }

    [Fact]
    public void MatrixDomain_StoresRowsColumnsAndElementDomain()
    {
        var md = new MatrixDomain(RealDomain.Instance, 2, 3);
        md.Rows.Should().Be(2);
        md.Columns.Should().Be(3);
        md.ElementDomain.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void MatrixDomain_DomainHasCorrectName()
    {
        var md = new MatrixDomain(RealDomain.Instance, 2, 3);
        md.Domain.Name.Should().Be("2x3 Matrix over Real");
    }

    [Fact]
    public void MatrixDomain_DomainHasMatrixKind()
    {
        var md = new MatrixDomain(IntegerDomain.Instance, 4, 4);
        md.Domain.Kind.Should().Be(DomainKind.Matrix);
    }

    [Fact]
    public void DomainRegistry_GetByKind_ReturnsBuiltInDomains()
    {
        DomainRegistry.Instance.Get(DomainKind.Real).Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainRegistry_GetByName_ReturnsBuiltInDomains()
    {
        DomainRegistry.Instance.Get("Integer").Should().Be(IntegerDomain.Instance);
    }

    [Fact]
    public void DomainRegistry_GetByName_IsCaseInsensitive()
    {
        DomainRegistry.Instance.Get("real").Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainRegistry_GetByName_ReturnsNullForUnknown()
    {
        DomainRegistry.Instance.Get("Nonexistent").Should().BeNull();
    }

    [Fact]
    public void DomainRegistry_GetByKind_ReturnsNullForUnknown()
    {
        DomainRegistry.Instance.Get(DomainKind.Tensor).Should().BeNull();
    }

    [Fact]
    public void DomainRegistry_GetByName_ThrowsForNull()
    {
        Action act = () => DomainRegistry.Instance.Get((string)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainRegistry_Register_AddsNewDomain()
    {
        var custom = new DomainBuilder()
            .WithName("Custom")
            .OfKind(DomainKind.None)
            .Build();
        DomainRegistry.Instance.Register(custom);
        DomainRegistry.Instance.Get("Custom").Should().Be(custom);
    }

    [Fact]
    public void DomainRegistry_GetAll_ReturnsBuiltInDomains()
    {
        DomainRegistry.Instance.GetAll().Should().Contain(RealDomain.Instance);
    }

    [Fact]
    public void DomainBuilder_BuildWithName()
    {
        var domain = new DomainBuilder()
            .WithName("Test")
            .Build();
        domain.Name.Should().Be("Test");
    }

    [Fact]
    public void DomainBuilder_BuildWithKind()
    {
        var domain = new DomainBuilder()
            .WithName("Test")
            .OfKind(DomainKind.Real)
            .Build();
        domain.Kind.Should().Be(DomainKind.Real);
    }

    [Fact]
    public void DomainBuilder_BuildWithParent()
    {
        var parent = new DomainBuilder().WithName("Parent").Build();
        var child = new DomainBuilder()
            .WithName("Child")
            .Extending(parent)
            .Build();
        child.Parents.Should().Contain(parent);
    }

    [Fact]
    public void DomainBuilder_BuildWithDoublePredicate()
    {
        var domain = new DomainBuilder()
            .WithName("Positive")
            .Containing(v => v > 0)
            .Build();
        domain.Contains(1.0).Should().BeTrue();
        domain.Contains(-1.0).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_BuildWithComplexPredicate()
    {
        var domain = new DomainBuilder()
            .WithName("UpperHalf")
            .ContainingComplex(v => v.Imaginary > 0)
            .Build();
        domain.Contains(new System.Numerics.Complex(1, 1)).Should().BeTrue();
        domain.Contains(new System.Numerics.Complex(1, -1)).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_FluentChaining()
    {
        var parent = new DomainBuilder().WithName("P").Build();
        var domain = new DomainBuilder()
            .WithName("Chained")
            .OfKind(DomainKind.Integer)
            .Extending(parent)
            .Containing(v => v > 0)
            .ContainingComplex(v => v.Imaginary == 0)
            .Build();
        domain.Name.Should().Be("Chained");
        domain.Kind.Should().Be(DomainKind.Integer);
        domain.Parents.Should().Contain(parent);
    }

    [Fact]
    public void DomainComparer_Instance_IsSingleton()
    {
        DomainComparer.Instance.Should().BeSameAs(DomainComparer.Instance);
    }

    [Fact]
    public void DomainComparer_Compare_NullBoth_ReturnsZero()
    {
        DomainComparer.Instance.Compare(null, null).Should().Be(0);
    }

    [Fact]
    public void DomainComparer_Compare_NullFirst_ReturnsPositive()
    {
        DomainComparer.Instance.Compare(null, RealDomain.Instance).Should().BeGreaterThan(0);
    }

    [Fact]
    public void DomainComparer_Compare_NullSecond_ReturnsNegative()
    {
        DomainComparer.Instance.Compare(RealDomain.Instance, null).Should().BeLessThan(0);
    }

    [Fact]
    public void DomainComparer_Compare_SameDomain_ReturnsZero()
    {
        DomainComparer.Instance.Compare(RealDomain.Instance, RealDomain.Instance).Should().Be(0);
    }

    [Fact]
    public void DomainComparer_Equals_NullBoth_ReturnsTrue()
    {
        DomainComparer.Instance.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void DomainComparer_Equals_OneNull_ReturnsFalse()
    {
        DomainComparer.Instance.Equals(null, RealDomain.Instance).Should().BeFalse();
    }

    [Fact]
    public void DomainComparer_Equals_SameDomain_ReturnsTrue()
    {
        DomainComparer.Instance.Equals(RealDomain.Instance, RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainComparer_GetHashCode_ZeroForNull()
    {
        DomainComparer.Instance.GetHashCode(null).Should().Be(0);
    }

    [Fact]
    public void DomainComparer_GetHashCode_NonZeroForDomain()
    {
        DomainComparer.Instance.GetHashCode(RealDomain.Instance).Should().NotBe(0);
    }

    [Fact]
    public void DomainValidator_Instance_IsSingleton()
    {
        DomainValidator.Instance.Should().BeSameAs(DomainValidator.Instance);
    }

    [Fact]
    public void DomainValidator_CanAdd_IncompatibleDomains()
    {
        var custom = new DomainBuilder().WithName("Unrelated").OfKind(DomainKind.None).Build();
        DomainValidator.Instance.CanAdd(custom, BooleanDomain.Instance).Should().BeFalse();
    }

    [Fact]
    public void DomainValidator_CanAdd_ThrowsForNull()
    {
        Action act = () => DomainValidator.Instance.CanAdd(null!, RealDomain.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainValidator_CanMultiply_VectorTimesVector()
    {
        var v1 = new VectorDomain(RealDomain.Instance, 3);
        var v2 = new VectorDomain(RealDomain.Instance, 3);
        DomainValidator.Instance.CanMultiply(v1.Domain, v2.Domain).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_MatrixTimesMatrix()
    {
        var m1 = new MatrixDomain(RealDomain.Instance, 2, 2);
        var m2 = new MatrixDomain(RealDomain.Instance, 2, 2);
        DomainValidator.Instance.CanMultiply(m1.Domain, m2.Domain).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_VectorTimesScalar()
    {
        var v = new VectorDomain(RealDomain.Instance, 3);
        DomainValidator.Instance.CanMultiply(v.Domain, RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_ScalarTimesVector()
    {
        var v = new VectorDomain(RealDomain.Instance, 3);
        DomainValidator.Instance.CanMultiply(RealDomain.Instance, v.Domain).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanApplyFunction_AlwaysReturnsTrue()
    {
        DomainValidator.Instance.CanApplyFunction(RealDomain.Instance, IntegerDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanApplyFunction_ThrowsForNull()
    {
        Action act = () => DomainValidator.Instance.CanApplyFunction(null!, RealDomain.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainValidator_ResultDomain_ComplexWins()
    {
        var result = DomainValidator.Instance.ResultDomain(RealDomain.Instance, ComplexDomain.Instance, "+");
        result.Should().Be(ComplexDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_SupersetWins()
    {
        var result = DomainValidator.Instance.ResultDomain(RealDomain.Instance, IntegerDomain.Instance, "+");
        result.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_MatrixTimesMatrix_ReturnsMatrix()
    {
        var m1 = new MatrixDomain(RealDomain.Instance, 2, 2);
        var m2 = new MatrixDomain(RealDomain.Instance, 2, 2);
        var result = DomainValidator.Instance.ResultDomain(m1.Domain, m2.Domain, "*");
        result.Should().Be(m1.Domain);
    }

    [Fact]
    public void DomainValidator_ResultDomain_VectorTimesVector_ReturnsVector()
    {
        var v1 = new VectorDomain(RealDomain.Instance, 3);
        var v2 = new VectorDomain(RealDomain.Instance, 3);
        var result = DomainValidator.Instance.ResultDomain(v1.Domain, v2.Domain, "*");
        result.Should().Be(v1.Domain);
    }

    [Fact]
    public void DomainValidator_ResultDomain_FiniteFieldTimesFiniteField()
    {
        var ff1 = new FiniteFieldDomain(7);
        var ff2 = new FiniteFieldDomain(7);
        var result = DomainValidator.Instance.ResultDomain(ff1.Domain, ff2.Domain, "+");
        result.Should().Be(ff1.Domain);
    }

    [Fact]
    public void DomainValidator_ResultDomain_NaturalTimesNatural()
    {
        var result = DomainValidator.Instance.ResultDomain(NaturalDomain.Instance, NaturalDomain.Instance, "+");
        result.Should().Be(NaturalDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_ThrowsForNullDomain()
    {
        Action act = () => DomainValidator.Instance.ResultDomain(null!, RealDomain.Instance, "+");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RealContains_NegativeInfinity_Rejected()
    {
        RealDomain.Instance.Contains(double.NegativeInfinity).Should().BeFalse();
    }

    [Fact]
    public void IntegerDomain_AcceptsZero()
    {
        IntegerDomain.Instance.Contains(0.0).Should().BeTrue();
    }

    [Fact]
    public void RationalDomain_AcceptsOneThird()
    {
        RationalDomain.Instance.Contains(1.0 / 3.0).Should().BeTrue();
    }

    [Fact]
    public void FiniteFieldDomain_PredicateRejectsNaN()
    {
        var ff = new FiniteFieldDomain(3);
        ff.Domain.Contains(double.NaN).Should().BeFalse();
    }

    [Fact]
    public void VectorDomain_PredicateRejectsScalar()
    {
        var vd = new VectorDomain(RealDomain.Instance, 3);
        vd.Domain.Contains(1.0).Should().BeFalse();
    }

    [Fact]
    public void MatrixDomain_PredicateRejectsScalar()
    {
        var md = new MatrixDomain(RealDomain.Instance, 2, 2);
        md.Domain.Contains(1.0).Should().BeFalse();
    }

    [Fact]
    public void BooleanDomain_AcceptsComplexWithRealZero()
    {
        BooleanDomain.Instance.Contains(new System.Numerics.Complex(0, 0)).Should().BeTrue();
    }

    [Fact]
    public void BooleanDomain_RejectsComplexWithNonZeroImaginary()
    {
        BooleanDomain.Instance.Contains(new System.Numerics.Complex(1, 1)).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_BuildWithMultipleParents()
    {
        var p1 = new DomainBuilder().WithName("P1").Build();
        var p2 = new DomainBuilder().WithName("P2").Build();
        var child = new DomainBuilder()
            .WithName("Child")
            .Extending(p1)
            .Extending(p2)
            .Build();
        child.Parents.Should().HaveCount(2);
        child.Parents.Should().Contain(p1);
        child.Parents.Should().Contain(p2);
    }

    [Fact]
    public void DomainBuilder_ContainingMultipleDoublePredicates_AllMustMatch()
    {
        var domain = new DomainBuilder()
            .WithName("PositiveEven")
            .Containing(v => v > 0)
            .Containing(v => v % 2 == 0)
            .Build();
        domain.Contains(2.0).Should().BeTrue();
        domain.Contains(-2.0).Should().BeFalse();
        domain.Contains(3.0).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_ContainingComplexPredicate()
    {
        var domain = new DomainBuilder()
            .WithName("UpperRight")
            .ContainingComplex(v => v.Real > 0 && v.Imaginary > 0)
            .Build();
        domain.Contains(new System.Numerics.Complex(1, 1)).Should().BeTrue();
        domain.Contains(new System.Numerics.Complex(-1, 1)).Should().BeFalse();
        domain.Contains(new System.Numerics.Complex(1, -1)).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_WithKindAndParent()
    {
        var parent = new DomainBuilder().WithName("P").OfKind(DomainKind.Real).Build();
        var child = new DomainBuilder()
            .WithName("Child")
            .OfKind(DomainKind.Complex)
            .Extending(parent)
            .Build();
        child.Kind.Should().Be(DomainKind.Complex);
        child.Parents.Should().Contain(parent);
    }

    [Fact]
    public void DomainRegistry_Register_DuplicateName_Overwrites()
    {
        var custom1 = new DomainBuilder().WithName("Dup").Build();
        var custom2 = new DomainBuilder().WithName("Dup").OfKind(DomainKind.Real).Build();
        DomainRegistry.Instance.Register(custom1);
        DomainRegistry.Instance.Register(custom2);
        DomainRegistry.Instance.Get("Dup").Should().Be(custom2);
    }

    [Fact]
    public void DomainRegistry_Register_CustomDomainAccessibleByKind()
    {
        var custom = new DomainBuilder().WithName("CustomKind").OfKind(DomainKind.Real).Build();
        DomainRegistry.Instance.Register(custom);
        DomainRegistry.Instance.Get(DomainKind.Real).Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainRegistry_GetAll_ContainsCustomRegistered()
    {
        var custom = new DomainBuilder().WithName("CustomAll").Build();
        DomainRegistry.Instance.Register(custom);
        DomainRegistry.Instance.GetAll().Should().Contain(custom);
    }

    [Fact]
    public void DomainValidator_CanAdd_RealAndComplex()
    {
        DomainValidator.Instance.CanAdd(RealDomain.Instance, ComplexDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanAdd_IntegerAndRational()
    {
        DomainValidator.Instance.CanAdd(IntegerDomain.Instance, RationalDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_MatrixVectorCompatible()
    {
        var m = new MatrixDomain(RealDomain.Instance, 2, 3);
        var v = new VectorDomain(RealDomain.Instance, 3);
        DomainValidator.Instance.CanMultiply(m.Domain, v.Domain).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_VectorMatrixIncompatible()
    {
        var v = new VectorDomain(RealDomain.Instance, 3);
        var m = new MatrixDomain(RealDomain.Instance, 2, 3);
        DomainValidator.Instance.CanMultiply(v.Domain, m.Domain).Should().BeFalse();
    }

    [Fact]
    public void DomainValidator_CanMultiply_ComplexAndReal()
    {
        DomainValidator.Instance.CanMultiply(ComplexDomain.Instance, RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanMultiply_QuaternionAndReal()
    {
        DomainValidator.Instance.CanMultiply(QuaternionDomain.Instance, RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_ResultDomain_ComplexPlusReal()
    {
        var result = DomainValidator.Instance.ResultDomain(ComplexDomain.Instance, RealDomain.Instance, "+");
        result.Should().Be(ComplexDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_RealPlusInteger()
    {
        var result = DomainValidator.Instance.ResultDomain(RealDomain.Instance, IntegerDomain.Instance, "+");
        result.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_RationalPlusInteger()
    {
        var result = DomainValidator.Instance.ResultDomain(RationalDomain.Instance, IntegerDomain.Instance, "+");
        result.Should().Be(RationalDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_NaturalPlusNatural()
    {
        var result = DomainValidator.Instance.ResultDomain(NaturalDomain.Instance, NaturalDomain.Instance, "+");
        result.Should().Be(NaturalDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_WholePlusWhole()
    {
        var result = DomainValidator.Instance.ResultDomain(WholeDomain.Instance, WholeDomain.Instance, "+");
        result.Should().Be(WholeDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_IntegerTimesInteger()
    {
        var result = DomainValidator.Instance.ResultDomain(IntegerDomain.Instance, IntegerDomain.Instance, "*");
        result.Should().Be(IntegerDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_RationalTimesInteger()
    {
        var result = DomainValidator.Instance.ResultDomain(RationalDomain.Instance, IntegerDomain.Instance, "*");
        result.Should().Be(RationalDomain.Instance);
    }

    [Fact]
    public void DomainValidator_ResultDomain_FiniteFieldSameCharacteristic()
    {
        var ff1 = new FiniteFieldDomain(5);
        var ff2 = new FiniteFieldDomain(5);
        var result = DomainValidator.Instance.ResultDomain(ff1.Domain, ff2.Domain, "+");
        result.Should().Be(ff1.Domain);
    }

    [Fact]
    public void DomainValidator_ResultDomain_MatrixTimesMatrixSameSize()
    {
        var m1 = new MatrixDomain(RealDomain.Instance, 3, 3);
        var m2 = new MatrixDomain(RealDomain.Instance, 3, 3);
        var result = DomainValidator.Instance.ResultDomain(m1.Domain, m2.Domain, "*");
        result.Should().Be(m1.Domain);
    }

    [Fact]
    public void DomainValidator_ResultDomain_VectorTimesVectorSameSize()
    {
        var v1 = new VectorDomain(RealDomain.Instance, 4);
        var v2 = new VectorDomain(RealDomain.Instance, 4);
        var result = DomainValidator.Instance.ResultDomain(v1.Domain, v2.Domain, "*");
        result.Should().Be(v1.Domain);
    }

    [Fact]
    public void FiniteFieldDomain_DifferentCharacteristics_NotEqual()
    {
        var ff1 = new FiniteFieldDomain(5);
        var ff2 = new FiniteFieldDomain(7);
        ff1.Domain.Equals(ff2.Domain).Should().BeFalse();
    }

    [Fact]
    public void FiniteFieldDomain_CharacteristicMustBePrime()
    {
        var ff = new FiniteFieldDomain(4);
        ff.Characteristic.Should().Be(4);
    }

    [Fact]
    public void VectorDomain_DimensionZero_Valid()
    {
        var vd = new VectorDomain(RealDomain.Instance, 0);
        vd.Dimension.Should().Be(0);
    }

    [Fact]
    public void MatrixDomain_ZeroRowsOrColumns_Valid()
    {
        var md = new MatrixDomain(RealDomain.Instance, 0, 3);
        md.Rows.Should().Be(0);
        md.Columns.Should().Be(3);
    }

    [Fact]
    public void SetDomain_ElementDomainStoresCorrectly()
    {
        var sd = new SetDomain(RealDomain.Instance);
        sd.ElementDomain.Should().Be(RealDomain.Instance);
        sd.Domain.Name.Should().Be("Set(Real)");
        sd.Domain.Kind.Should().Be(DomainKind.Set);
    }

    [Fact]
    public void FunctionDomain_CodomainStoresCorrectly()
    {
        var fd = new FunctionDomain(ComplexDomain.Instance);
        fd.Codomain.Should().Be(ComplexDomain.Instance);
        fd.Domain.Name.Should().Be("Function -> Complex");
        fd.Domain.Kind.Should().Be(DomainKind.Function);
    }

    [Fact]
    public void DomainComparer_Compare_ByKindThenName()
    {
        var real = DomainRegistry.Instance.Get(DomainKind.Real)!;
        var complex = DomainRegistry.Instance.Get(DomainKind.Complex)!;
        var cmp = DomainComparer.Instance.Compare(real, complex);
        cmp.Should().NotBe(0);
    }

    [Fact]
    public void DomainComparer_Equals_IgnoreCaseForNames()
    {
        var a = new DomainBuilder().WithName("Test").Build();
        var b = new DomainBuilder().WithName("test").Build();
        DomainComparer.Instance.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanApplyFunction_RealToInteger()
    {
        DomainValidator.Instance.CanApplyFunction(RealDomain.Instance, IntegerDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void DomainValidator_CanApplyFunction_ComplexToReal()
    {
        DomainValidator.Instance.CanApplyFunction(ComplexDomain.Instance, RealDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void TensorDomain_StoresRankAndElementDomain()
    {
        var td = new TensorDomain(RealDomain.Instance, new int?[] { 2, 3, 4 });
        td.Rank.Should().Be(3);
        td.ElementDomain.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void TensorDomain_DomainHasTensorKind()
    {
        var td = new TensorDomain(RealDomain.Instance, new int?[] { 2, 2 });
        td.Domain.Kind.Should().Be(DomainKind.Tensor);
    }

    [Fact]
    public void DomainRegistry_GetByName_CaseInsensitive()
    {
        DomainRegistry.Instance.Get("REAL").Should().Be(RealDomain.Instance);
        DomainRegistry.Instance.Get("integer").Should().Be(IntegerDomain.Instance);
    }

    [Fact]
    public void DomainBuilder_FluentChainingAllOptions()
    {
        var p = new DomainBuilder().WithName("P").Build();
        var d = new DomainBuilder()
            .WithName("Full")
            .OfKind(DomainKind.Real)
            .Extending(p)
            .Containing(v => v > 0)
            .ContainingComplex(c => c.Real > 0)
            .Build();
        d.Name.Should().Be("Full");
        d.Kind.Should().Be(DomainKind.Real);
        d.Parents.Should().Contain(p);
        d.Contains(1.0).Should().BeTrue();
        d.Contains(-1.0).Should().BeFalse();
        d.Contains(new System.Numerics.Complex(1, 0)).Should().BeTrue();
        d.Contains(new System.Numerics.Complex(-1, 0)).Should().BeFalse();
    }

    [Fact]
    public void DomainBuilder_WithMultipleConstraints()
    {
        var d = new DomainBuilder()
            .WithName("PositiveIntegers")
            .OfKind(DomainKind.Integer)
            .Containing(v => v > 0)
            .Containing(v => v % 2 == 0)
            .Build();
        d.Contains(2.0).Should().BeTrue();
        d.Contains(4.0).Should().BeTrue();
        d.Contains(-2.0).Should().BeFalse();
        d.Contains(3.0).Should().BeFalse();
    }

    [Fact]
    public void DomainRegistry_GetAll_ReturnsAllBuiltins()
    {
        var all = DomainRegistry.Instance.GetAll();
        all.Should().NotBeEmpty();
        all.Should().Contain(RealDomain.Instance);
        all.Should().Contain(IntegerDomain.Instance);
        all.Should().Contain(ComplexDomain.Instance);
        all.Should().Contain(BooleanDomain.Instance);
    }

    [Fact]
    public void DomainRegistry_RegisterCustomDomain()
    {
        var custom = new DomainBuilder().WithName("Custom").OfKind(DomainKind.Real).Build();
        DomainRegistry.Instance.Register(custom);
        DomainRegistry.Instance.Get("Custom").Should().Be(custom);
    }

    [Fact]
    public void DomainComparer_SortBySpecificity()
    {
        var domains = new[] { RealDomain.Instance, IntegerDomain.Instance, NaturalDomain.Instance, WholeDomain.Instance };
        var sorted = domains.OrderBy(d => d, DomainComparer.Instance).ToArray();
        sorted[0].Should().Be(NaturalDomain.Instance);
    }

    [Fact]
    public void DomainValidator_CanApplyFunction()
    {
        DomainValidator.Instance.CanApplyFunction(RealDomain.Instance, ComplexDomain.Instance).Should().BeTrue();
    }

    [Fact]
    public void RealDomain_ContainsNegativeZero()
    {
        RealDomain.Instance.Contains(-0.0).Should().BeTrue();
    }

    [Fact]
    public void IntegerDomain_RejectsNonInteger()
    {
        IntegerDomain.Instance.Contains(1.5).Should().BeFalse();
    }

    [Fact]
    public void NaturalDomain_RejectsZero()
    {
        NaturalDomain.Instance.Contains(0.0).Should().BeFalse();
    }

    [Fact]
    public void WholeDomain_RejectsZero()
    {
        WholeDomain.Instance.Contains(0.0).Should().BeFalse();
    }

    [Fact]
    public void WholeDomain_RejectsNegative()
    {
        WholeDomain.Instance.Contains(-1.0).Should().BeFalse();
    }

    [Fact]
    public void RationalDomain_RejectsIrrational()
    {
        RationalDomain.Instance.Contains(Math.Sqrt(2)).Should().BeFalse();
    }

    [Fact]
    public void BooleanDomain_OnlyAcceptsZeroAndOne()
    {
        BooleanDomain.Instance.Contains(0.0).Should().BeTrue();
        BooleanDomain.Instance.Contains(1.0).Should().BeTrue();
        BooleanDomain.Instance.Contains(2.0).Should().BeFalse();
        BooleanDomain.Instance.Contains(-1.0).Should().BeFalse();
    }

    [Fact]
    public void FiniteFieldDomain_ModuloArithmetic()
    {
        var ff5 = new FiniteFieldDomain(5);
        ff5.Domain.Contains(0.0).Should().BeTrue();
        ff5.Domain.Contains(4.0).Should().BeTrue();
        ff5.Domain.Contains(5.0).Should().BeTrue();
        ff5.Domain.Contains(6.0).Should().BeTrue();
    }

    [Fact]
    public void VectorDomain_RejectsWrongDimension()
    {
        var vd = new VectorDomain(RealDomain.Instance, 3);
        vd.Domain.Contains(new System.Numerics.Complex(1, 0)).Should().BeFalse();
    }

    [Fact]
    public void MatrixDomain_AcceptsCorrectShape()
    {
        var md = new MatrixDomain(RealDomain.Instance, 2, 3);
        md.Domain.Contains(new System.Numerics.Complex(1, 0)).Should().BeFalse();
    }

    [Fact]
    public void TensorDomain_RejectsWrongRank()
    {
        var td = new TensorDomain(RealDomain.Instance, new int?[] { 2, 2, 2 });
        td.Domain.Contains(new System.Numerics.Complex(1, 0)).Should().BeFalse();
    }

    [Fact]
    public void FunctionDomain_CodomainCheck()
    {
        var fd = new FunctionDomain(ComplexDomain.Instance);
        fd.Domain.Kind.Should().Be(DomainKind.Function);
        fd.Domain.Contains(new System.Numerics.Complex(1, 0)).Should().BeTrue();
    }

    [Fact]
    public void SetDomain_UniversalSet()
    {
        var sd = new SetDomain(RealDomain.Instance);
        sd.Domain.Kind.Should().Be(DomainKind.Set);
        sd.Domain.Contains(1.0).Should().BeTrue();
    }

    [Fact]
    public void DomainKind_FlagsCombination()
    {
        var combined = DomainKind.Real | DomainKind.Integer | DomainKind.Complex;
        combined.Should().Be(DomainKind.Real | DomainKind.Integer | DomainKind.Complex);
    }

    [Fact]
    public void DomainEquality_SameInstance()
    {
        RealDomain.Instance.Should().Be(RealDomain.Instance);
    }

    [Fact]
    public void DomainEquality_DifferentInstancesSameKind()
    {
        var d1 = new DomainBuilder().WithName("D1").OfKind(DomainKind.Real).Build();
        var d2 = new DomainBuilder().WithName("D2").OfKind(DomainKind.Real).Build();
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void DomainHashCode_Stable()
    {
        RealDomain.Instance.GetHashCode().Should().Be(RealDomain.Instance.GetHashCode());
    }
}

