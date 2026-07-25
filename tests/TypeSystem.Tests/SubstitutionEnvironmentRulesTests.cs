namespace MathVerse.TypeSystem.Tests;

public class TypeSubstitutionTests
{
    [Fact]
    public void Empty_Count()
    {
        var sub = new TypeSubstitution();
        sub.Count.Should().Be(0);
    }

    [Fact]
    public void Add_Contains()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        sub.Contains(0).Should().BeTrue();
    }

    [Fact]
    public void Add_Get()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Get_Unmapped_ReturnsNull()
    {
        var sub = new TypeSubstitution();
        sub.Get(0).Should().BeNull();
    }

    [Fact]
    public void Add_MappedVariables()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance).Add(1, IntegerType.Instance);
        sub.MappedVariables.Should().Contain(0);
        sub.MappedVariables.Should().Contain(1);
    }

    [Fact]
    public void ApplyTo_TypeVariable()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var result = sub.ApplyTo(tv);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_Unmapped()
    {
        var sub = new TypeSubstitution();
        var tv = new TypeVariable(0);
        var result = sub.ApplyTo(tv);
        result.Should().Be(tv);
    }

    [Fact]
    public void ApplyTo_FunctionType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var ft = new FunctionType(new[] { tv }, IntegerType.Instance);
        var result = (FunctionType)sub.ApplyTo(ft);
        result.ParameterTypes[0].Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_VectorType()
    {
        var sub = new TypeSubstitution().Add(0, IntegerType.Instance);
        var tv = new TypeVariable(0);
        var vt = new VectorType(tv, 3);
        var result = (VectorType)sub.ApplyTo(vt);
        result.ElementType.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void ApplyTo_MatrixType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var mt = new MatrixType(tv, 2, 2);
        var result = (MatrixType)sub.ApplyTo(mt);
        result.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_TensorType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var tt = new TensorType(tv, new int?[] { 2, 3 });
        var result = (TensorType)sub.ApplyTo(tt);
        result.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_TupleType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var tuple = new TupleType(new MathType[] { tv, IntegerType.Instance });
        var result = (TupleType)sub.ApplyTo(tuple);
        result.ElementTypes[0].Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_SetType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var st = new SetType(tv);
        var result = (SetType)sub.ApplyTo(st);
        result.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_SequenceType()
    {
        var sub = new TypeSubstitution().Add(0, IntegerType.Instance);
        var tv = new TypeVariable(0);
        var seq = new SequenceType(tv);
        var result = (SequenceType)sub.ApplyTo(seq);
        result.ElementType.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void ApplyTo_PolynomialType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var pt = new PolynomialType(tv);
        var result = (PolynomialType)sub.ApplyTo(pt);
        result.CoefficientType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_EquationType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var tv = new TypeVariable(0);
        var eq = new EquationType(tv, IntegerType.Instance);
        var result = (EquationType)sub.ApplyTo(eq);
        result.LeftType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void ApplyTo_ConcreteType()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var result = sub.ApplyTo(IntegerType.Instance);
        result.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Compose()
    {
        var sub1 = new TypeSubstitution().Add(0, new TypeVariable(1));
        var sub2 = new TypeSubstitution().Add(1, RealType.Instance);
        var composed = sub1.Compose(sub2);
        composed.ApplyTo(new TypeVariable(0)).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Equals_Same()
    {
        var sub1 = new TypeSubstitution().Add(0, RealType.Instance);
        var sub2 = new TypeSubstitution().Add(0, RealType.Instance);
        sub1.Equals(sub2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Different()
    {
        var sub1 = new TypeSubstitution().Add(0, RealType.Instance);
        var sub2 = new TypeSubstitution().Add(0, IntegerType.Instance);
        sub1.Equals(sub2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null()
    {
        var sub = new TypeSubstitution();
        sub.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object()
    {
        var sub1 = new TypeSubstitution().Add(0, RealType.Instance);
        object sub2 = new TypeSubstitution().Add(0, RealType.Instance);
        sub1.Equals(sub2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ReturnsConsistent()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        sub.GetHashCode().Should().Be(sub.GetHashCode());
    }

    [Fact]
    public void ToString_ContainsExpectedContent()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        sub.ToString().Should().Contain("?0");
    }

    [Fact]
    public void HasUnresolved_False()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        sub.HasUnresolved.Should().BeFalse();
    }

    [Fact]
    public void TypeSubstitution_Constructor()
    {
        var sub = new TypeSubstitution();
        sub.Count.Should().Be(0);
    }

    [Fact]
    public void TypeSubstitution_MultipleAdds()
    {
        var sub = new TypeSubstitution()
            .Add(0, RealType.Instance)
            .Add(1, IntegerType.Instance)
            .Add(2, BooleanType.Instance);
        sub.Count.Should().Be(3);
    }

    [Fact]
    public void TypeSubstitution_Replace()
    {
        var sub = new TypeSubstitution()
            .Add(0, RealType.Instance)
            .Add(0, IntegerType.Instance);
        sub.Get(0).Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeEnvironment_Bind()
    {
        var env = new TypeEnvironment();
        var env2 = env.Bind("x", RealType.Instance);
        env2.Lookup("x").Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeEnvironment_Lookup_Unbound()
    {
        var env = new TypeEnvironment();
        env.Lookup("x").Should().BeNull();
    }

    [Fact]
    public void TypeEnvironment_ChildScope()
    {
        var env = new TypeEnvironment().Bind("x", RealType.Instance);
        var child = env.CreateChild().Bind("y", IntegerType.Instance);
        child.Lookup("x").Should().Be(RealType.Instance);
        child.Lookup("y").Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeEnvironment_ChildShadow()
    {
        var env = new TypeEnvironment().Bind("x", RealType.Instance);
        var child = env.CreateChild().Bind("x", IntegerType.Instance);
        child.Lookup("x").Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeEnvironment_IsDefined()
    {
        var env = new TypeEnvironment().Bind("x", RealType.Instance);
        env.IsDefined("x").Should().BeTrue();
        env.IsDefined("y").Should().BeFalse();
    }

    [Fact]
    public void TypeEnvironment_DefinedNames()
    {
        var env = new TypeEnvironment()
            .Bind("x", RealType.Instance)
            .Bind("y", IntegerType.Instance);
        env.DefinedNames.Should().Contain("x");
        env.DefinedNames.Should().Contain("y");
    }

    [Fact]
    public void TypeEnvironment_BindAll()
    {
        var dict = new Dictionary<string, MathType>
        {
            { "x", RealType.Instance },
            { "y", IntegerType.Instance }
        };
        var env = new TypeEnvironment().BindAll(dict);
        env.Lookup("x").Should().Be(RealType.Instance);
        env.Lookup("y").Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeEnvironment_Merge()
    {
        var env1 = new TypeEnvironment().Bind("x", RealType.Instance);
        var env2 = new TypeEnvironment().Bind("y", IntegerType.Instance);
        var merged = env1.Merge(env2);
        merged.Lookup("x").Should().Be(RealType.Instance);
        merged.Lookup("y").Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeEnvironment_Count()
    {
        var env = new TypeEnvironment()
            .Bind("x", RealType.Instance)
            .Bind("y", IntegerType.Instance);
        env.Count.Should().Be(2);
    }

    [Fact]
    public void TypeEnvironment_ParentLookup()
    {
        var parent = new TypeEnvironment().Bind("x", RealType.Instance);
        var child = parent.CreateChild().Bind("y", IntegerType.Instance);
        parent.Lookup("y").Should().BeNull();
    }

    [Fact]
    public void InferenceContext_PushPopScope()
    {
        var ctx = new InferenceContext();
        ctx.PushScope();
        ctx.PopScope();
    }

    [Fact]
    public void InferenceContext_Resolve()
    {
        var ctx = new InferenceContext();
        var result = ctx.Resolve(RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void InferenceContext_Clear()
    {
        var ctx = new InferenceContext();
        ctx.AddEquality(RealType.Instance, IntegerType.Instance);
        ctx.Clear();
        ctx.Constraints.Should().BeEmpty();
    }

    [Fact]
    public void TypeRules_Promote_IntInt()
    {
        var result = TypeRules.Promote(IntegerType.Instance, IntegerType.Instance);
        result.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRules_Promote_IntReal()
    {
        var result = TypeRules.Promote(IntegerType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_Promote_RealComplex()
    {
        var result = TypeRules.Promote(RealType.Instance, ComplexType.Instance);
        result.Should().Be(ComplexType.Instance);
    }

    [Fact]
    public void TypeRules_Promote_IntComplex()
    {
        var result = TypeRules.Promote(IntegerType.Instance, ComplexType.Instance);
        result.Should().Be(ComplexType.Instance);
    }

    [Fact]
    public void TypeRules_Promote_RealReal()
    {
        var result = TypeRules.Promote(RealType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_PromoteScalar()
    {
        var result = TypeRules.PromoteScalar(IntegerType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_ArithmeticResult()
    {
        var result = TypeRules.ArithmeticResult(IntegerType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_ExponentiationResult()
    {
        var result = TypeRules.ExponentiationResult(RealType.Instance, IntegerType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_ComparisonResult()
    {
        var result = TypeRules.ComparisonResult(RealType.Instance, IntegerType.Instance);
        result.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void TypeRules_LogicalResult()
    {
        var result = TypeRules.LogicalResult(BooleanType.Instance, BooleanType.Instance);
        result.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void TypeRules_VectorAddResult()
    {
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(IntegerType.Instance, 3);
        var result = TypeRules.VectorAddResult(left, right);
        result.Should().BeOfType<VectorType>();
    }

    [Fact]
    public void TypeRules_MatrixMultiplyResult()
    {
        var left = new MatrixType(RealType.Instance, 2, 3);
        var right = new MatrixType(IntegerType.Instance, 3, 4);
        var result = TypeRules.MatrixMultiplyResult(left, right);
        var mt = (MatrixType)result;
        mt.Rows.Should().Be(2);
        mt.Columns.Should().Be(4);
    }

    [Fact]
    public void TypeRules_DotProductResult()
    {
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(IntegerType.Instance, 3);
        var result = TypeRules.DotProductResult(left, right);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_CrossProductResult()
    {
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(IntegerType.Instance, 3);
        var result = TypeRules.CrossProductResult(left, right);
        var vt = (VectorType)result;
        vt.Dimension.Should().Be(3);
    }

    [Fact]
    public void TypeRules_DerivativeResult()
    {
        var result = TypeRules.DerivativeResult(RealType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_IntegralResult()
    {
        var result = TypeRules.IntegralResult(RealType.Instance, RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_LimitResult()
    {
        var result = TypeRules.LimitResult(RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_SummationResult()
    {
        var result = TypeRules.SummationResult(RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_FactorialResult_Integer()
    {
        var result = TypeRules.FactorialResult(IntegerType.Instance);
        result.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRules_FactorialResult_Real()
    {
        var result = TypeRules.FactorialResult(RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRules_PromotionLadder()
    {
        TypeRules.PromotionLadder.Should().HaveCount(4);
    }

    [Fact]
    public void TypeRules_ApplicationResult()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        var result = TypeRules.ApplicationResult(ft, new[] { RealType.Instance });
        result.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRules_ApplicationResult_NonFunction()
    {
        var result = TypeRules.ApplicationResult(RealType.Instance, new[] { RealType.Instance });
        result.Should().Be(UnknownType.Instance);
    }

    [Fact]
    public void TypeRules_ApplicationResult_WrongArity()
    {
        var ft = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        var result = TypeRules.ApplicationResult(ft, new MathType[] { RealType.Instance, IntegerType.Instance });
        result.Should().Be(ErrorType.Instance);
    }
}
