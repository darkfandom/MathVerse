namespace MathVerse.TypeSystem.Tests;

public class AlgebraicClassifierTests
{
    [Fact]
    public void Classify_Integer_HasIntegralDomain()
    {
        var structures = AlgebraicClassifier.Classify(IntegerType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.IntegralDomain);
    }

    [Fact]
    public void Classify_Integer_HasRing()
    {
        var structures = AlgebraicClassifier.Classify(IntegerType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.Ring);
    }

    [Fact]
    public void Classify_Integer_NotField()
    {
        var structures = AlgebraicClassifier.Classify(IntegerType.Instance);
        structures.Should().NotContain(s => s.Kind == AlgebraicStructureKind.Field);
    }

    [Fact]
    public void Classify_Rational_IsField()
    {
        var structures = AlgebraicClassifier.Classify(RationalType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.Field);
    }

    [Fact]
    public void Classify_Rational_IsIntegralDomain()
    {
        var structures = AlgebraicClassifier.Classify(RationalType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.IntegralDomain);
    }

    [Fact]
    public void Classify_Real_IsField()
    {
        var structures = AlgebraicClassifier.Classify(RealType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.Field);
    }

    [Fact]
    public void Classify_Real_HasOrderedField()
    {
        var structures = AlgebraicClassifier.Classify(RealType.Instance);
        structures.Should().Contain(s => s is OrderedField);
    }

    [Fact]
    public void Classify_Complex_IsField()
    {
        var structures = AlgebraicClassifier.Classify(ComplexType.Instance);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.Field);
    }

    [Fact]
    public void Classify_Vector_WithRealField()
    {
        var vt = new VectorType(RealType.Instance);
        var structures = AlgebraicClassifier.Classify(vt);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.VectorSpace);
    }

    [Fact]
    public void Classify_Vector_HasInnerProductSpace()
    {
        var vt = new VectorType(RealType.Instance);
        var structures = AlgebraicClassifier.Classify(vt);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.InnerProductSpace);
    }

    [Fact]
    public void Classify_Vector_HasMetricSpace()
    {
        var vt = new VectorType(RealType.Instance);
        var structures = AlgebraicClassifier.Classify(vt);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.MetricSpace);
    }

    [Fact]
    public void GetStrongest_Integer_IsIntegralDomain()
    {
        var strongest = AlgebraicClassifier.GetStrongest(IntegerType.Instance);
        strongest.Should().NotBeNull();
        strongest!.Kind.Should().Be(AlgebraicStructureKind.IntegralDomain);
    }

    [Fact]
    public void GetStrongest_Real_IsOrderedField()
    {
        var strongest = AlgebraicClassifier.GetStrongest(RealType.Instance);
        strongest.Should().NotBeNull();
        strongest!.Should().BeOfType<OrderedField>();
    }

    [Fact]
    public void GetStrongest_Complex_IsField()
    {
        var strongest = AlgebraicClassifier.GetStrongest(ComplexType.Instance);
        strongest.Should().NotBeNull();
        strongest!.Kind.Should().Be(AlgebraicStructureKind.Field);
    }

    [Fact]
    public void GetStrongest_Vector_IsInnerProductSpace()
    {
        var vt = new VectorType(RealType.Instance);
        var strongest = AlgebraicClassifier.GetStrongest(vt);
        strongest.Should().NotBeNull();
        strongest!.Kind.Should().Be(AlgebraicStructureKind.InnerProductSpace);
    }

    [Fact]
    public void Classify_Boolean_ReturnsEmpty()
    {
        var structures = AlgebraicClassifier.Classify(BooleanType.Instance);
        structures.Should().BeEmpty();
    }

    [Fact]
    public void Classify_Set_ContainsMagma()
    {
        var st = new SetType(RealType.Instance);
        var structures = AlgebraicClassifier.Classify(st);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.Magma);
    }

    [Fact]
    public void Classify_TypedInteger_IsIntegralDomain()
    {
        var ti = IntegerType.Create(42);
        var structures = AlgebraicClassifier.Classify(ti);
        structures.Should().Contain(s => s.Kind == AlgebraicStructureKind.IntegralDomain);
    }
}
