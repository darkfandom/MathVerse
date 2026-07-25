namespace MathVerse.TypeSystem.Tests;

public class GenericTypeTests
{
    [Fact]
    public void TypeParameter_Creates()
    {
        var tp = new TypeParameter("T");
        tp.Should().NotBeNull();
    }

    [Fact]
    public void TypeParameter_Kind()
    {
        var tp = new TypeParameter("T");
        tp.Kind.Should().Be(TypeKind.Generic);
    }

    [Fact]
    public void TypeParameter_Name()
    {
        var tp = new TypeParameter("T");
        tp.Name.Should().Be("T");
    }

    [Fact]
    public void TypeParameter_IsGenericParameter()
    {
        var tp = new TypeParameter("T");
        tp.IsGenericParameter.Should().BeTrue();
    }

    [Fact]
    public void TypeParameter_Equals()
    {
        var tp1 = new TypeParameter("T");
        var tp2 = new TypeParameter("T");
        tp1.Equals(tp2).Should().BeTrue();
    }

    [Fact]
    public void TypeParameter_NotEquals_DifferentName()
    {
        var tp1 = new TypeParameter("T");
        var tp2 = new TypeParameter("U");
        tp1.Equals(tp2).Should().BeFalse();
    }

    [Fact]
    public void TypeParameter_GetHashCode()
    {
        var tp = new TypeParameter("T");
        tp.GetHashCode().Should().Be(tp.GetHashCode());
    }

    [Fact]
    public void TypeParameter_HasConstraints()
    {
        var constraints = new[] { new GenericConstraint(GenericConstraintKind.Numeric) };
        var tp = new TypeParameter("T", constraints);
        tp.Constraints.Should().HaveCount(1);
    }

    [Fact]
    public void TypeParameter_Variance()
    {
        var tp = new TypeParameter("T", variance: TypeVariance.Covariant);
        tp.Variance.Should().Be(TypeVariance.Covariant);
    }

    [Fact]
    public void GenericConstraint_Creates()
    {
        var gc = new GenericConstraint(GenericConstraintKind.Numeric);
        gc.Should().NotBeNull();
    }

    [Fact]
    public void GenericConstraint_Kind()
    {
        var gc = new GenericConstraint(GenericConstraintKind.Numeric);
        gc.Kind.Should().Be(GenericConstraintKind.Numeric);
    }

    [Fact]
    public void GenericConstraint_WithType()
    {
        var gc = new GenericConstraint(GenericConstraintKind.TypeConstraint, RealType.Instance);
        gc.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void GenericConstraint_Equals()
    {
        var gc1 = new GenericConstraint(GenericConstraintKind.Numeric);
        var gc2 = new GenericConstraint(GenericConstraintKind.Numeric);
        gc1.Equals(gc2).Should().BeTrue();
    }

    [Fact]
    public void GenericConstraint_NotEquals_DifferentKind()
    {
        var gc1 = new GenericConstraint(GenericConstraintKind.Numeric);
        var gc2 = new GenericConstraint(GenericConstraintKind.FieldConstraint);
        gc1.Equals(gc2).Should().BeFalse();
    }

    [Fact]
    public void GenericConstraint_Equals_WithType()
    {
        var gc1 = new GenericConstraint(GenericConstraintKind.TypeConstraint, RealType.Instance);
        var gc2 = new GenericConstraint(GenericConstraintKind.TypeConstraint, RealType.Instance);
        gc1.Equals(gc2).Should().BeTrue();
    }

    [Fact]
    public void GenericConstraint_NotEquals_DifferentType()
    {
        var gc1 = new GenericConstraint(GenericConstraintKind.TypeConstraint, RealType.Instance);
        var gc2 = new GenericConstraint(GenericConstraintKind.TypeConstraint, IntegerType.Instance);
        gc1.Equals(gc2).Should().BeFalse();
    }

    [Fact]
    public void GenericConstraint_GetHashCode()
    {
        var gc = new GenericConstraint(GenericConstraintKind.Numeric);
        gc.GetHashCode().Should().Be(gc.GetHashCode());
    }

    [Fact]
    public void GenericType_Creates()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        gt.Should().NotBeNull();
    }

    [Fact]
    public void GenericType_Name()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        gt.Name.Should().Be("Vector<T>");
    }

    [Fact]
    public void GenericType_Arity()
    {
        var gt = new GenericType("Tuple", new[] { new TypeParameter("T1"), new TypeParameter("T2") });
        gt.Arity.Should().Be(2);
    }

    [Fact]
    public void GenericType_Instantiate()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        inst.Should().NotBeNull();
    }

    [Fact]
    public void GenericType_Instantiate_WrongArity_Throws()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        Action act = () => gt.Instantiate(new MathType[] { RealType.Instance, IntegerType.Instance });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenericType_Equals()
    {
        var gt1 = new GenericType("Vector", new[] { new TypeParameter("T") });
        var gt2 = new GenericType("Vector", new[] { new TypeParameter("U") });
        gt1.Equals(gt2).Should().BeTrue();
    }

    [Fact]
    public void GenericType_NotEquals_DifferentName()
    {
        var gt1 = new GenericType("Vector", new[] { new TypeParameter("T") });
        var gt2 = new GenericType("Matrix", new[] { new TypeParameter("T") });
        gt1.Equals(gt2).Should().BeFalse();
    }

    [Fact]
    public void GenericType_GetHashCode()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        gt.GetHashCode().Should().Be(gt.GetHashCode());
    }

    [Fact]
    public void GenericInstantiation_Name()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        inst.Name.Should().Be("Vector<Real>");
    }

    [Fact]
    public void GenericInstantiation_Equals()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst1 = gt.Instantiate(new[] { RealType.Instance });
        var inst2 = gt.Instantiate(new[] { RealType.Instance });
        inst1.Equals(inst2).Should().BeTrue();
    }

    [Fact]
    public void GenericInstantiation_NotEquals_DifferentArg()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst1 = gt.Instantiate(new[] { RealType.Instance });
        var inst2 = gt.Instantiate(new[] { IntegerType.Instance });
        inst1.Equals(inst2).Should().BeFalse();
    }

    [Fact]
    public void GenericInstantiation_GetHashCode()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        inst.GetHashCode().Should().Be(inst.GetHashCode());
    }

    [Fact]
    public void GenericTypeDefinitions_Vector()
    {
        GenericTypeDefinitions.Vector.Name.Should().Be("Vector<T>");
    }

    [Fact]
    public void GenericTypeDefinitions_Matrix()
    {
        GenericTypeDefinitions.Matrix.Name.Should().Be("Matrix<T>");
    }

    [Fact]
    public void GenericTypeDefinitions_Tensor()
    {
        GenericTypeDefinitions.Tensor.Name.Should().Be("Tensor<T>");
    }

    [Fact]
    public void GenericTypeDefinitions_Set()
    {
        GenericTypeDefinitions.Set.Name.Should().Be("Set<T>");
    }

    [Fact]
    public void GenericTypeDefinitions_Tuple2()
    {
        GenericTypeDefinitions.Tuple2.Name.Should().Be("Tuple<T1, T2>");
    }

    [Fact]
    public void GenericTypeDefinitions_Tuple3()
    {
        GenericTypeDefinitions.Tuple3.Name.Should().Be("Tuple<T1, T2, T3>");
    }

    [Fact]
    public void GenericTypeDefinitions_Polynomial()
    {
        GenericTypeDefinitions.Polynomial.Name.Should().Be("Poly<T>");
    }

    [Fact]
    public void GenericTypeDefinitions_Dictionary()
    {
        GenericTypeDefinitions.Dictionary.Name.Should().Be("Dictionary<TKey, TValue>");
    }

    [Fact]
    public void GenericTypeDefinitions_Sequence()
    {
        GenericTypeDefinitions.Sequence.Name.Should().Be("Seq<T>");
    }

    [Fact]
    public void GenericInstantiation_IsNotGenericParameter()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        inst.IsGenericParameter.Should().BeFalse();
    }

    [Fact]
    public void TypeParameter_NullConstraints_DefaultsEmpty()
    {
        var tp = new TypeParameter("T");
        tp.Constraints.Should().BeEmpty();
    }

    [Fact]
    public void TypeVariance_DefaultIsInvariant()
    {
        var tp = new TypeParameter("T");
        tp.Variance.Should().Be(TypeVariance.Invariant);
    }

    [Fact]
    public void GenericConstraint_NullType()
    {
        var gc = new GenericConstraint(GenericConstraintKind.NotNull);
        gc.Type.Should().BeNull();
    }

    [Fact]
    public void TypeParameter_Equals_Null()
    {
        var tp = new TypeParameter("T");
        tp.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void TypeParameter_Equals_Object()
    {
        var tp = new TypeParameter("T");
        object other = new TypeParameter("T");
        tp.Equals(other).Should().BeTrue();
    }

    [Fact]
    public void GenericType_NotEquals_Null()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        gt.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GenericInstantiation_NotEquals_Null()
    {
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        inst.Equals(null).Should().BeFalse();
    }
}
