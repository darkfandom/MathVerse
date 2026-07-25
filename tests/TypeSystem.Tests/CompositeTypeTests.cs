namespace MathVerse.TypeSystem.Tests;

public class CompositeTypeTests
{
    [Fact]
    public void FunctionType_Creates()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        ft.Should().NotBeNull();
    }

    [Fact]
    public void FunctionType_Kind()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        ft.Kind.Should().Be(TypeKind.Function);
    }

    [Fact]
    public void FunctionType_Arity()
    {
        var ft = new FunctionType(new MathType[] { RealType.Instance, IntegerType.Instance }, BooleanType.Instance);
        ft.Arity.Should().Be(2);
    }

    [Fact]
    public void FunctionType_ParameterTypes()
    {
        var ft = new FunctionType(new[] { RealType.Instance, RealType.Instance }, RealType.Instance);
        ft.ParameterTypes.Should().HaveCount(2);
        ft.ParameterTypes[0].Should().Be(RealType.Instance);
    }

    [Fact]
    public void FunctionType_ReturnType()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, ComplexType.Instance);
        ft.ReturnType.Should().Be(ComplexType.Instance);
    }

    [Fact]
    public void FunctionType_Name()
    {
        var ft = new FunctionType(new[] { IntegerType.Instance }, RealType.Instance);
        ft.Name.Should().Be("(Integer) → Real");
    }

    [Fact]
    public void FunctionType_Equals_Same()
    {
        var ft1 = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var ft2 = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        ft1.Equals(ft2).Should().BeTrue();
    }

    [Fact]
    public void FunctionType_NotEquals_DifferentReturn()
    {
        var ft1 = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var ft2 = new FunctionType(new[] { RealType.Instance }, ComplexType.Instance);
        ft1.Equals(ft2).Should().BeFalse();
    }

    [Fact]
    public void FunctionType_NotEquals_DifferentArity()
    {
        var ft1 = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var ft2 = new FunctionType(new[] { RealType.Instance, RealType.Instance }, RealType.Instance);
        ft1.Equals(ft2).Should().BeFalse();
    }

    [Fact]
    public void FunctionType_NotEquals_DifferentParam()
    {
        var ft1 = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var ft2 = new FunctionType(new[] { IntegerType.Instance }, RealType.Instance);
        ft1.Equals(ft2).Should().BeFalse();
    }

    [Fact]
    public void FunctionType_GetHashCode()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        ft.GetHashCode().Should().Be(ft.GetHashCode());
    }

    [Fact]
    public void FunctionType_ZeroArity()
    {
        var ft = new FunctionType(Array.Empty<MathType>(), IntegerType.Instance);
        ft.Arity.Should().Be(0);
    }

    [Fact]
    public void VectorType_Creates()
    {
        var vt = new VectorType(RealType.Instance);
        vt.Should().NotBeNull();
    }

    [Fact]
    public void VectorType_Kind()
    {
        var vt = new VectorType(RealType.Instance);
        vt.Kind.Should().Be(TypeKind.Vector);
    }

    [Fact]
    public void VectorType_ElementType()
    {
        var vt = new VectorType(IntegerType.Instance);
        vt.ElementType.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void VectorType_Dimension()
    {
        var vt = new VectorType(RealType.Instance, 3);
        vt.Dimension.Should().Be(3);
    }

    [Fact]
    public void VectorType_DynamicDimension()
    {
        var vt = new VectorType(RealType.Instance);
        vt.Dimension.Should().BeNull();
    }

    [Fact]
    public void VectorType_Name_WithDimension()
    {
        var vt = new VectorType(RealType.Instance, 5);
        vt.Name.Should().Be("Vector<Real, 5>");
    }

    [Fact]
    public void VectorType_Name_WithoutDimension()
    {
        var vt = new VectorType(RealType.Instance);
        vt.Name.Should().Be("Vector<Real>");
    }

    [Fact]
    public void VectorType_IsNumeric()
    {
        var vt = new VectorType(RealType.Instance);
        vt.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void VectorType_IsNotNumeric_NonScalar()
    {
        var vt = new VectorType(BooleanType.Instance);
        vt.IsNumeric.Should().BeFalse();
    }

    [Fact]
    public void VectorType_IsField()
    {
        var vt = new VectorType(RealType.Instance);
        vt.IsField.Should().BeTrue();
    }

    [Fact]
    public void VectorType_Equals()
    {
        var vt1 = new VectorType(RealType.Instance, 3);
        var vt2 = new VectorType(RealType.Instance, 3);
        vt1.Equals(vt2).Should().BeTrue();
    }

    [Fact]
    public void VectorType_NotEquals_DifferentDim()
    {
        var vt1 = new VectorType(RealType.Instance, 3);
        var vt2 = new VectorType(RealType.Instance, 4);
        vt1.Equals(vt2).Should().BeFalse();
    }

    [Fact]
    public void VectorType_NotEquals_DifferentElem()
    {
        var vt1 = new VectorType(RealType.Instance, 3);
        var vt2 = new VectorType(IntegerType.Instance, 3);
        vt1.Equals(vt2).Should().BeFalse();
    }

    [Fact]
    public void VectorType_IsRowVector()
    {
        var vt = new VectorType(RealType.Instance, 3) { IsRowVector = true };
        vt.IsRowVector.Should().BeTrue();
    }

    [Fact]
    public void VectorType_GetHashCode()
    {
        var vt = new VectorType(RealType.Instance, 3);
        vt.GetHashCode().Should().Be(vt.GetHashCode());
    }

    [Fact]
    public void MatrixType_Creates()
    {
        var mt = new MatrixType(RealType.Instance);
        mt.Should().NotBeNull();
    }

    [Fact]
    public void MatrixType_Kind()
    {
        var mt = new MatrixType(RealType.Instance);
        mt.Kind.Should().Be(TypeKind.Matrix);
    }

    [Fact]
    public void MatrixType_WithDimensions()
    {
        var mt = new MatrixType(RealType.Instance, 3, 4);
        mt.Rows.Should().Be(3);
        mt.Columns.Should().Be(4);
    }

    [Fact]
    public void MatrixType_Name_Static()
    {
        var mt = new MatrixType(RealType.Instance, 2, 3);
        mt.Name.Should().Be("Matrix<Real, 2×3>");
    }

    [Fact]
    public void MatrixType_Name_Dynamic()
    {
        var mt = new MatrixType(RealType.Instance);
        mt.Name.Should().Be("Matrix<Real>");
    }

    [Fact]
    public void MatrixType_IsSquare()
    {
        var mt = new MatrixType(RealType.Instance, 3, 3);
        mt.IsSquare.Should().BeTrue();
    }

    [Fact]
    public void MatrixType_IsNotSquare()
    {
        var mt = new MatrixType(RealType.Instance, 2, 3);
        mt.IsSquare.Should().BeFalse();
    }

    [Fact]
    public void MatrixType_IsNumeric()
    {
        var mt = new MatrixType(RealType.Instance);
        mt.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void MatrixType_Equals()
    {
        var mt1 = new MatrixType(RealType.Instance, 3, 3);
        var mt2 = new MatrixType(RealType.Instance, 3, 3);
        mt1.Equals(mt2).Should().BeTrue();
    }

    [Fact]
    public void MatrixType_NotEquals_DifferentRows()
    {
        var mt1 = new MatrixType(RealType.Instance, 2, 3);
        var mt2 = new MatrixType(RealType.Instance, 3, 3);
        mt1.Equals(mt2).Should().BeFalse();
    }

    [Fact]
    public void MatrixType_GetHashCode()
    {
        var mt = new MatrixType(RealType.Instance, 3, 3);
        mt.GetHashCode().Should().Be(mt.GetHashCode());
    }

    [Fact]
    public void TensorType_Creates()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3, 4 });
        tt.Should().NotBeNull();
    }

    [Fact]
    public void TensorType_Kind()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        tt.Kind.Should().Be(TypeKind.Tensor);
    }

    [Fact]
    public void TensorType_Rank()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3, 4 });
        tt.Rank.Should().Be(3);
    }

    [Fact]
    public void TensorType_Name()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        tt.Name.Should().Be("Tensor<Real, [2×3]>");
    }

    [Fact]
    public void TensorType_IsNumeric()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2 });
        tt.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void TensorType_Equals()
    {
        var tt1 = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        var tt2 = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        tt1.Equals(tt2).Should().BeTrue();
    }

    [Fact]
    public void TensorType_NotEquals_DifferentRank()
    {
        var tt1 = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        var tt2 = new TensorType(RealType.Instance, new int?[] { 2, 3, 4 });
        tt1.Equals(tt2).Should().BeFalse();
    }

    [Fact]
    public void TensorType_GetHashCode()
    {
        var tt = new TensorType(RealType.Instance, new int?[] { 2 });
        tt.GetHashCode().Should().Be(tt.GetHashCode());
    }

    [Fact]
    public void PolynomialType_Creates()
    {
        var pt = new PolynomialType(RealType.Instance);
        pt.Should().NotBeNull();
    }

    [Fact]
    public void PolynomialType_Kind()
    {
        var pt = new PolynomialType(RealType.Instance);
        pt.Kind.Should().Be(TypeKind.Polynomial);
    }

    [Fact]
    public void PolynomialType_IsUnivariate()
    {
        var pt = new PolynomialType(RealType.Instance, 1);
        pt.IsUnivariate.Should().BeTrue();
    }

    [Fact]
    public void PolynomialType_IsNotUnivariate()
    {
        var pt = new PolynomialType(RealType.Instance, 2);
        pt.IsUnivariate.Should().BeFalse();
    }

    [Fact]
    public void PolynomialType_Name()
    {
        var pt = new PolynomialType(IntegerType.Instance, 1, 5);
        pt.Name.Should().Be("Poly<Integer, x, deg≤5>");
    }

    [Fact]
    public void PolynomialType_Equals()
    {
        var pt1 = new PolynomialType(RealType.Instance, 1, 3);
        var pt2 = new PolynomialType(RealType.Instance, 1, 3);
        pt1.Equals(pt2).Should().BeTrue();
    }

    [Fact]
    public void PolynomialType_GetHashCode()
    {
        var pt = new PolynomialType(RealType.Instance);
        pt.GetHashCode().Should().Be(pt.GetHashCode());
    }

    [Fact]
    public void EquationType_Creates()
    {
        var et = new EquationType(RealType.Instance, RealType.Instance);
        et.Should().NotBeNull();
    }

    [Fact]
    public void EquationType_Kind()
    {
        var et = new EquationType(RealType.Instance, RealType.Instance);
        et.Kind.Should().Be(TypeKind.Equation);
    }

    [Fact]
    public void EquationType_Operator()
    {
        var et = new EquationType(RealType.Instance, RealType.Instance, "=");
        et.Operator.Should().Be("=");
    }

    [Fact]
    public void EquationType_Name()
    {
        var et = new EquationType(RealType.Instance, IntegerType.Instance, "=");
        et.Name.Should().Be("Real = Integer");
    }

    [Fact]
    public void EquationType_Equals()
    {
        var et1 = new EquationType(RealType.Instance, RealType.Instance, "=");
        var et2 = new EquationType(RealType.Instance, RealType.Instance, "=");
        et1.Equals(et2).Should().BeTrue();
    }

    [Fact]
    public void EquationType_NotEquals_DifferentOp()
    {
        var et1 = new EquationType(RealType.Instance, RealType.Instance, "=");
        var et2 = new EquationType(RealType.Instance, RealType.Instance, "≠");
        et1.Equals(et2).Should().BeFalse();
    }

    [Fact]
    public void EquationType_GetHashCode()
    {
        var et = new EquationType(RealType.Instance, RealType.Instance);
        et.GetHashCode().Should().Be(et.GetHashCode());
    }

    [Fact]
    public void SetType_Creates()
    {
        var st = new SetType(RealType.Instance);
        st.Should().NotBeNull();
    }

    [Fact]
    public void SetType_Kind()
    {
        var st = new SetType(RealType.Instance);
        st.Kind.Should().Be(TypeKind.Set);
    }

    [Fact]
    public void SetType_Name()
    {
        var st = new SetType(IntegerType.Instance);
        st.Name.Should().Be("Set<Integer>");
    }

    [Fact]
    public void SetType_IsFinite()
    {
        var st = new SetType(IntegerType.Instance, 5);
        st.IsFinite.Should().BeTrue();
    }

    [Fact]
    public void SetType_IsNotFinite()
    {
        var st = new SetType(IntegerType.Instance);
        st.IsFinite.Should().BeFalse();
    }

    [Fact]
    public void SetType_Equals()
    {
        var st1 = new SetType(RealType.Instance, 5);
        var st2 = new SetType(RealType.Instance, 5);
        st1.Equals(st2).Should().BeTrue();
    }

    [Fact]
    public void SetType_GetHashCode()
    {
        var st = new SetType(RealType.Instance);
        st.GetHashCode().Should().Be(st.GetHashCode());
    }

    [Fact]
    public void TupleType_Creates()
    {
        var tt = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        tt.Should().NotBeNull();
    }

    [Fact]
    public void TupleType_Kind()
    {
        var tt = new TupleType(new[] { RealType.Instance });
        tt.Kind.Should().Be(TypeKind.Tuple);
    }

    [Fact]
    public void TupleType_Arity()
    {
        var tt = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance, BooleanType.Instance });
        tt.Arity.Should().Be(3);
    }

    [Fact]
    public void TupleType_Name()
    {
        var tt = new TupleType(new MathType[] { RealType.Instance, BooleanType.Instance });
        tt.Name.Should().Be("(Real, Boolean)");
    }

    [Fact]
    public void TupleType_Equals()
    {
        var tt1 = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        var tt2 = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        tt1.Equals(tt2).Should().BeTrue();
    }

    [Fact]
    public void TupleType_NotEquals_DifferentArity()
    {
        var tt1 = new TupleType(new[] { RealType.Instance });
        var tt2 = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        tt1.Equals(tt2).Should().BeFalse();
    }

    [Fact]
    public void TupleType_GetHashCode()
    {
        var tt = new TupleType(new[] { RealType.Instance });
        tt.GetHashCode().Should().Be(tt.GetHashCode());
    }

    [Fact]
    public void SequenceType_Creates()
    {
        var st = new SequenceType(RealType.Instance);
        st.Should().NotBeNull();
    }

    [Fact]
    public void SequenceType_Kind()
    {
        var st = new SequenceType(RealType.Instance);
        st.Kind.Should().Be(TypeKind.Sequence);
    }

    [Fact]
    public void SequenceType_Name_WithLength()
    {
        var st = new SequenceType(IntegerType.Instance, 10);
        st.Name.Should().Be("Seq<Integer, 10>");
    }

    [Fact]
    public void SequenceType_Name_WithoutLength()
    {
        var st = new SequenceType(RealType.Instance);
        st.Name.Should().Be("Seq<Real>");
    }

    [Fact]
    public void SequenceType_Equals()
    {
        var st1 = new SequenceType(RealType.Instance, 5);
        var st2 = new SequenceType(RealType.Instance, 5);
        st1.Equals(st2).Should().BeTrue();
    }

    [Fact]
    public void SequenceType_GetHashCode()
    {
        var st = new SequenceType(RealType.Instance);
        st.GetHashCode().Should().Be(st.GetHashCode());
    }

    [Fact]
    public void DomainType_Creates()
    {
        var dt = new DomainType("ℕ", IntegerType.Instance);
        dt.Should().NotBeNull();
    }

    [Fact]
    public void DomainType_Kind()
    {
        var dt = new DomainType("ℝ", RealType.Instance);
        dt.Kind.Should().Be(TypeKind.Domain);
    }

    [Fact]
    public void DomainType_Name()
    {
        var dt = new DomainType("ℂ", ComplexType.Instance);
        dt.Name.Should().Be("ℂ");
    }

    [Fact]
    public void DomainType_Equals()
    {
        var dt1 = new DomainType("ℝ", RealType.Instance);
        var dt2 = new DomainType("ℝ", RealType.Instance);
        dt1.Equals(dt2).Should().BeTrue();
    }

    [Fact]
    public void DomainType_NotEquals_DifferentSymbol()
    {
        var dt1 = new DomainType("ℝ", RealType.Instance);
        var dt2 = new DomainType("ℂ", ComplexType.Instance);
        dt1.Equals(dt2).Should().BeFalse();
    }

    [Fact]
    public void DomainType_GetHashCode()
    {
        var dt = new DomainType("ℝ", RealType.Instance);
        dt.GetHashCode().Should().Be(dt.GetHashCode());
    }
}
