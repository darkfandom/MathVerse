namespace MathVerse.TypeSystem.Tests;

public class VisitorTests
{
    [Fact]
    public void TypeWalker_VisitsAllTypes()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var ft = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        walker.Walk(ft);
        visited.Should().Contain(ft);
        visited.Should().Contain(RealType.Instance);
        visited.Should().Contain(IntegerType.Instance);
    }

    [Fact]
    public void TypeWalker_Vector()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var vt = new VectorType(RealType.Instance, 3);
        walker.Walk(vt);
        visited.Should().Contain(vt);
        visited.Should().Contain(RealType.Instance);
    }

    [Fact]
    public void TypeWalker_Matrix()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var mt = new MatrixType(IntegerType.Instance, 2, 3);
        walker.Walk(mt);
        visited.Should().Contain(mt);
        visited.Should().Contain(IntegerType.Instance);
    }

    [Fact]
    public void TypeWalker_Tensor()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        walker.Walk(tt);
        visited.Should().Contain(tt);
        visited.Should().Contain(RealType.Instance);
    }

    [Fact]
    public void TypeWalker_Tuple()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var tuple = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        walker.Walk(tuple);
        visited.Should().Contain(tuple);
        visited.Should().Contain(RealType.Instance);
        visited.Should().Contain(IntegerType.Instance);
    }

    [Fact]
    public void TypeWalker_Set()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var st = new SetType(RealType.Instance);
        walker.Walk(st);
        visited.Should().Contain(st);
    }

    [Fact]
    public void TypeWalker_Sequence()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var seq = new SequenceType(IntegerType.Instance, 5);
        walker.Walk(seq);
        visited.Should().Contain(seq);
    }

    [Fact]
    public void TypeWalker_Domain()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var dt = new DomainType("ℝ", RealType.Instance);
        walker.Walk(dt);
        visited.Should().Contain(dt);
    }

    [Fact]
    public void TypeWalker_Equation()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var eq = new EquationType(RealType.Instance, IntegerType.Instance, "=");
        walker.Walk(eq);
        visited.Should().Contain(eq);
    }

    [Fact]
    public void TypeWalker_Polynomial()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var pt = new PolynomialType(RealType.Instance);
        walker.Walk(pt);
        visited.Should().Contain(pt);
    }

    [Fact]
    public void TypeWalker_SimpleTypes()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        walker.Walk(RealType.Instance);
        walker.Walk(IntegerType.Instance);
        walker.Walk(BooleanType.Instance);
        walker.Walk(ComplexType.Instance);
        visited.Should().HaveCount(4);
    }

    [Fact]
    public void TypeWalker_UnknownType()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        walker.Walk(UnknownType.Instance);
        visited.Should().Contain(UnknownType.Instance);
    }

    [Fact]
    public void TypeWalker_ErrorType()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        walker.Walk(ErrorType.Instance);
        visited.Should().Contain(ErrorType.Instance);
    }

    [Fact]
    public void TypeWalker_UnitType()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        walker.Walk(UnitType.Instance);
        visited.Should().Contain(UnitType.Instance);
    }

    [Fact]
    public void TypeWalker_StringType()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        walker.Walk(StringType.Instance);
        visited.Should().Contain(StringType.Instance);
    }

    [Fact]
    public void TypeWalker_GenericInstantiation()
    {
        var visited = new List<MathType>();
        var walker = new TypeWalker(t => visited.Add(t));
        var gt = new GenericType("Vector", new[] { new TypeParameter("T") });
        var inst = gt.Instantiate(new[] { RealType.Instance });
        walker.Walk(inst);
        visited.Should().Contain(inst);
    }

    [Fact]
    public void TypeCollector_Collects()
    {
        var collector = new TypeCollector();
        var ft = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        collector.Visit(ft);
        collector.Types.Should().Contain(ft);
        collector.Types.Should().Contain(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewritesTypeVariable()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var result = rewriter.Rewrite(tv);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_PreservesUnchanged()
    {
        var rewriter = new TypeRewriter(_ => null);
        var result = rewriter.Rewrite(RealType.Instance);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteFunction()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? IntegerType.Instance : null);
        var ft = new FunctionType(new[] { tv }, RealType.Instance);
        var result = (FunctionType)rewriter.Rewrite(ft);
        result.ParameterTypes[0].Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteVector()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var vt = new VectorType(tv, 3);
        var result = (VectorType)rewriter.Rewrite(vt);
        result.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteMatrix()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? IntegerType.Instance : null);
        var mt = new MatrixType(tv, 2, 2);
        var result = (MatrixType)rewriter.Rewrite(mt);
        result.ElementType.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteTuple()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var tuple = new TupleType(new MathType[] { tv, IntegerType.Instance });
        var result = (TupleType)rewriter.Rewrite(tuple);
        result.ElementTypes[0].Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteSet()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var st = new SetType(tv);
        var result = (SetType)rewriter.Rewrite(st);
        result.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteSequence()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? IntegerType.Instance : null);
        var seq = new SequenceType(tv);
        var result = (SequenceType)rewriter.Rewrite(seq);
        result.ElementType.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewritePolynomial()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var pt = new PolynomialType(tv);
        var result = (PolynomialType)rewriter.Rewrite(pt);
        result.CoefficientType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_RewriteEquation()
    {
        var tv = new TypeVariable(0);
        var rewriter = new TypeRewriter(t => t.Equals(tv) ? RealType.Instance : null);
        var eq = new EquationType(tv, IntegerType.Instance);
        var result = (EquationType)rewriter.Rewrite(eq);
        result.LeftType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_FromSubstitution()
    {
        var sub = new TypeSubstitution().Add(0, RealType.Instance);
        var rewriter = TypeRewriter.FromSubstitution(sub);
        var tv = new TypeVariable(0);
        var result = rewriter.Rewrite(tv);
        result.Should().Be(RealType.Instance);
    }

    [Fact]
    public void TypeRewriter_FromSubstitution_Unmapped()
    {
        var sub = new TypeSubstitution();
        var rewriter = TypeRewriter.FromSubstitution(sub);
        var tv = new TypeVariable(0);
        var result = rewriter.Rewrite(tv);
        result.Should().Be(tv);
    }

    [Fact]
    public void TypeComparer_Equal()
    {
        var comparer = new TypeComparer(RealType.Instance);
        comparer.CompareWith(RealType.Instance).Should().Be(0);
    }

    [Fact]
    public void TypeComparer_LessThan()
    {
        var comparer = new TypeComparer(IntegerType.Instance);
        comparer.CompareWith(RealType.Instance).Should().BeLessThan(0);
    }

    [Fact]
    public void TypeComparer_GreaterThan()
    {
        var comparer = new TypeComparer(RealType.Instance);
        comparer.CompareWith(IntegerType.Instance).Should().BeGreaterThan(0);
    }

    [Fact]
    public void TypeComparer_DifferentTypes()
    {
        var comparer = new TypeComparer(BooleanType.Instance);
        comparer.CompareWith(IntegerType.Instance).Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_HashesSame()
    {
        var hasher = new TypeHasher();
        var h1 = hasher.Hash(RealType.Instance);
        var h2 = hasher.Hash(RealType.Instance);
        h1.Should().Be(h2);
    }

    [Fact]
    public void TypeHasher_DifferentTypes_DifferentHash()
    {
        var hasher = new TypeHasher();
        var h1 = hasher.Hash(RealType.Instance);
        var h2 = hasher.Hash(IntegerType.Instance);
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void TypeHasher_FunctionType()
    {
        var hasher = new TypeHasher();
        var ft = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        var h = hasher.Hash(ft);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_VectorType()
    {
        var hasher = new TypeHasher();
        var vt = new VectorType(RealType.Instance, 3);
        var h = hasher.Hash(vt);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_MatrixType()
    {
        var hasher = new TypeHasher();
        var mt = new MatrixType(RealType.Instance, 2, 2);
        var h = hasher.Hash(mt);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_TensorType()
    {
        var hasher = new TypeHasher();
        var tt = new TensorType(RealType.Instance, new int?[] { 2, 3 });
        var h = hasher.Hash(tt);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_TupleType()
    {
        var hasher = new TypeHasher();
        var tuple = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        var h = hasher.Hash(tuple);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_SetType()
    {
        var hasher = new TypeHasher();
        var st = new SetType(RealType.Instance, 5);
        var h = hasher.Hash(st);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_TypeParameter()
    {
        var hasher = new TypeHasher();
        var tp = new TypeParameter("T");
        var h = hasher.Hash(tp);
        h.Should().NotBe(0);
    }

    [Fact]
    public void TypeHasher_Boolean()
    {
        var hasher = new TypeHasher();
        hasher.Hash(BooleanType.Instance).Should().Be(10);
    }

    [Fact]
    public void TypeHasher_Error()
    {
        var hasher = new TypeHasher();
        hasher.Hash(ErrorType.Instance).Should().Be(1);
    }

    [Fact]
    public void TypeHasher_Unknown()
    {
        var hasher = new TypeHasher();
        hasher.Hash(UnknownType.Instance).Should().Be(0);
    }

    [Fact]
    public void TypeHasher_Unit()
    {
        var hasher = new TypeHasher();
        hasher.Hash(UnitType.Instance).Should().Be(2);
    }

    [Fact]
    public void TypeHasher_String()
    {
        var hasher = new TypeHasher();
        hasher.Hash(StringType.Instance).Should().Be(30);
    }
}
