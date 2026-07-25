using MathVerse.Math.Foundation.Integration;
using MathVerse.Math.Foundation.Quantities;
using MathVerse.Math.Types;

namespace MathVerse.Foundation.Tests;

[Collection("DimensionAnalyzer")]
public sealed class ExpressionDimensionExtensionsTests : IDisposable
{
    public ExpressionDimensionExtensionsTests()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    public void Dispose()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    [Fact]
    public void GetDimension_NullExpr_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.GetDimension(null!, DimensionAnalyzer.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDimension_NullAnalyzer_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.GetDimension(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDimension_Literal_ReturnsNone()
    {
        Expr.Literal(5).GetDimension(DimensionAnalyzer.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void GetDimension_VariableWithDimension_ReturnsDimension()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        Expr.Variable("x").GetDimension(DimensionAnalyzer.Instance).Should().Be(dim);
    }

    [Fact]
    public void IsDimensionallyConsistent_NullExpr_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.IsDimensionallyConsistent(null!, DimensionAnalyzer.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsDimensionallyConsistent_NullAnalyzer_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.IsDimensionallyConsistent(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsDimensionallyConsistent_LiteralExpr_ReturnsTrue()
    {
        Expr.Literal(5).IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeTrue();
    }

    [Fact]
    public void IsDimensionallyConsistent_CompatibleAddition_ReturnsTrue()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        DimensionAnalyzer.Instance.SetVariableDimension("y", dim);
        Expr.Add(Expr.Variable("x"), Expr.Variable("y")).IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeTrue();
    }

    [Fact]
    public void IsDimensionallyConsistent_IncompatibleAddition_ReturnsFalse()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("y", mass);
        Expr.Add(Expr.Variable("x"), Expr.Variable("y")).IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeFalse();
    }

    [Fact]
    public void EvaluateAsQuantity_NullExpr_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.EvaluateAsQuantity(null!, new Dictionary<string, PhysicalQuantity>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsQuantity_NullVars_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.EvaluateAsQuantity(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsQuantity_Literal_ReturnsNoneDimension()
    {
        var result = Expr.Literal(42).EvaluateAsQuantity(new Dictionary<string, PhysicalQuantity>());
        result.Should().NotBeNull();
        result!.Value.Should().Be(42);
        result.Dimension.Should().Be(Dimension.None);
    }

    [Fact]
    public void EvaluateAsQuantity_VariableFound_ReturnsQuantity()
    {
        var pq = new PhysicalQuantity { Value = 3.0, Dimension = Dimension.FromBaseDimensions(length: 1) };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq };
        var result = Expr.Variable("x").EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(3.0);
    }

    [Fact]
    public void EvaluateAsQuantity_VariableNotFound_ReturnsNull()
    {
        var result = Expr.Variable("unknown").EvaluateAsQuantity(new Dictionary<string, PhysicalQuantity>());
        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateAsQuantity_Constant_ReturnsZeroNone()
    {
        var result = ConstantExpression.Pi.EvaluateAsQuantity(new Dictionary<string, PhysicalQuantity>());
        result.Should().NotBeNull();
        result!.Value.Should().Be(0.0);
        result.Dimension.Should().Be(Dimension.None);
    }

    [Fact]
    public void EvaluateAsQuantity_Addition_SumsValues()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var left = new PhysicalQuantity { Value = 3.0, Dimension = dim };
        var right = new PhysicalQuantity { Value = 4.0, Dimension = dim };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = left, ["y"] = right };
        var result = Expr.Add(Expr.Variable("x"), Expr.Variable("y")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(7.0);
    }

    [Fact]
    public void EvaluateAsQuantity_Subtraction_SubtractsValues()
    {
        var dim = Dimension.FromBaseDimensions(time: 1);
        var left = new PhysicalQuantity { Value = 10.0, Dimension = dim };
        var right = new PhysicalQuantity { Value = 3.0, Dimension = dim };
        var vars = new Dictionary<string, PhysicalQuantity> { ["a"] = left, ["b"] = right };
        var result = Expr.Subtract(Expr.Variable("a"), Expr.Variable("b")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(7.0);
    }

    [Fact]
    public void EvaluateAsQuantity_Multiplication_MultipliesValues()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var left = new PhysicalQuantity { Value = 3.0, Dimension = length };
        var right = new PhysicalQuantity { Value = 2.0, Dimension = mass };
        var vars = new Dictionary<string, PhysicalQuantity> { ["l"] = left, ["m"] = right };
        var result = Expr.Multiply(Expr.Variable("l"), Expr.Variable("m")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(6.0);
        result!.Dimension.IsCompatibleWith(length.Multiply(mass)).Should().BeTrue();
    }

    [Fact]
    public void EvaluateAsQuantity_Division_DividesValues()
    {
        var energy = DerivedDimension.Energy;
        var time = Dimension.FromBaseDimensions(time: 1);
        var left = new PhysicalQuantity { Value = 100.0, Dimension = energy };
        var right = new PhysicalQuantity { Value = 5.0, Dimension = time };
        var vars = new Dictionary<string, PhysicalQuantity> { ["e"] = left, ["t"] = right };
        var result = Expr.Divide(Expr.Variable("e"), Expr.Variable("t")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(20.0);
        result!.Dimension.IsCompatibleWith(DerivedDimension.Power).Should().BeTrue();
    }

    [Fact]
    public void EvaluateAsQuantity_Negate_NegatesValue()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var pq = new PhysicalQuantity { Value = 5.0, Dimension = dim };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq };
        var result = Expr.Negate(Expr.Variable("x")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(-5.0);
    }

    [Fact]
    public void EvaluateAsQuantity_TrigFunction_ReturnsZeroDimensionless()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var pq = new PhysicalQuantity { Value = 1.0, Dimension = dim };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq };
        var result = Expr.Sin(Expr.Variable("x")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(0.0);
        result.Dimension.Should().Be(Dimension.None);
    }

    [Fact]
    public void EvaluateAsQuantity_SqrtFunction_ReturnsSqrtValue()
    {
        var area = Dimension.FromBaseDimensions(length: 2);
        var pq = new PhysicalQuantity { Value = 9.0, Dimension = area };
        var vars = new Dictionary<string, PhysicalQuantity> { ["A"] = pq };
        var result = Expr.Sqrt(Expr.Variable("A")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(3.0);
    }

    [Fact]
    public void EvaluateAsQuantity_UnknownBinaryOp_ReturnsNull()
    {
        var left = new PhysicalQuantity { Value = 1.0, Dimension = Dimension.None };
        var right = new PhysicalQuantity { Value = 2.0, Dimension = Dimension.None };
        var vars = new Dictionary<string, PhysicalQuantity> { ["a"] = left, ["b"] = right };
        var modExpr = Expr.Modulo(Expr.Variable("a"), Expr.Variable("b"));
        var result = modExpr.EvaluateAsQuantity(vars);
        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateAsQuantity_NullLeftInBinary_ReturnsNull()
    {
        var vars = new Dictionary<string, PhysicalQuantity> { ["y"] = new PhysicalQuantity { Value = 1.0 } };
        var result = Expr.Add(Expr.Variable("missing"), Expr.Variable("y")).EvaluateAsQuantity(vars);
        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateAsQuantity_NullUnaryOperand_ReturnsNull()
    {
        var result = Expr.Negate(Expr.Variable("missing")).EvaluateAsQuantity(new Dictionary<string, PhysicalQuantity>());
        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateAsQuantity_UnknownUnaryOp_ReturnsNull()
    {
        var pq = new PhysicalQuantity { Value = 5.0, Dimension = Dimension.None };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq };
        var absExpr = Expr.Abs(Expr.Variable("x"));
        var result = absExpr.EvaluateAsQuantity(vars);
        result.Should().BeNull();
    }

    [Fact]
    public void WithDimensions_NullExpr_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.WithDimensions(null!, new Dictionary<string, Dimension>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDimensions_NullVars_Throws()
    {
        Action act = () => ExpressionDimensionExtensions.WithDimensions(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDimensions_ReturnsSameExpression()
    {
        var expr = Expr.Variable("x");
        var dims = new Dictionary<string, Dimension> { ["x"] = Dimension.FromBaseDimensions(length: 1) };
        var result = expr.WithDimensions(dims);
        result.Should().BeSameAs(expr);
    }

    [Fact]
    public void WithDimensions_SetsAnalyzerVariableDimensions()
    {
        var expr = Expr.Variable("y");
        var dim = Dimension.FromBaseDimensions(mass: 1);
        var dims = new Dictionary<string, Dimension> { ["y"] = dim };
        expr.WithDimensions(dims);
        DimensionAnalyzer.Instance.GetVariableDimension("y").Should().Be(dim);
    }

    [Fact]
    public void EvaluateAsQuantity_BinaryWithIncompatibleDimensions_Throws()
    {
        var length = new PhysicalQuantity { Value = 1.0, Dimension = Dimension.FromBaseDimensions(length: 1) };
        var mass = new PhysicalQuantity { Value = 2.0, Dimension = Dimension.FromBaseDimensions(mass: 1) };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = length, ["m"] = mass };
        Action act = () => Expr.Add(Expr.Variable("x"), Expr.Variable("m")).EvaluateAsQuantity(vars);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EvaluateAsQuantity_CosFunction_ReturnsZeroDimensionless()
    {
        var pq = new PhysicalQuantity { Value = 0.5, Dimension = Dimension.FromBaseDimensions(time: 1) };
        var vars = new Dictionary<string, PhysicalQuantity> { ["t"] = pq };
        var result = Expr.Cos(Expr.Variable("t")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(0.0);
        result.Dimension.Should().Be(Dimension.None);
    }

    [Fact]
    public void EvaluateAsQuantity_TanFunction_ReturnsZeroDimensionless()
    {
        var pq = new PhysicalQuantity { Value = 1.0, Dimension = Dimension.None };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq };
        var result = Expr.Tan(Expr.Variable("x")).EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(0.0);
    }

    [Fact]
    public void EvaluateAsQuantity_NestedBinary_EvaluatesCorrectly()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var a = new PhysicalQuantity { Value = 2.0, Dimension = dim };
        var b = new PhysicalQuantity { Value = 3.0, Dimension = dim };
        var vars = new Dictionary<string, PhysicalQuantity> { ["a"] = a, ["b"] = b };
        var expr = Expr.Add(Expr.Variable("a"), Expr.Multiply(Expr.Literal(2), Expr.Variable("b")));
        var result = expr.EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(8.0);
    }

    [Fact]
    public void EvaluateAsQuantity_MultiArgFunc_ReturnsNull()
    {
        var pq = new PhysicalQuantity { Value = 1.0, Dimension = Dimension.None };
        var vars = new Dictionary<string, PhysicalQuantity> { ["x"] = pq, ["y"] = pq };
        var expr = Expr.Log(Expr.Variable("x"), Expr.Variable("y"));
        var result = expr.EvaluateAsQuantity(vars);
        result.Should().BeNull();
    }
}

[Collection("DimensionAnalyzer")]
public sealed class QuantityExpressionFactoryTests : IDisposable
{
    public QuantityExpressionFactoryTests()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    public void Dispose()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    [Fact]
    public void CreateQuantityExpression_NullUnit_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityExpression(1.0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityExpression_ReturnsLiteralExpression()
    {
        var unit = new Unit { Symbol = "m", Name = "Meter", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var expr = QuantityExpressionFactory.CreateQuantityExpression(5.0, unit);
        expr.Should().BeOfType<AnnotatedExpression>();
    }

    [Fact]
    public void CreateQuantityExpression_NullLeft_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityAdd(null!, Expr.Literal(1));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityExpression_NullRight_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityAdd(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityAdd_CompatibleDimensions_ReturnsBinaryExpression()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        DimensionAnalyzer.Instance.SetVariableDimension("y", dim);
        var result = QuantityExpressionFactory.CreateQuantityAdd(Expr.Variable("x"), Expr.Variable("y"));
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CreateQuantityAdd_IncompatibleDimensions_Throws()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("m", mass);
        Action act = () => QuantityExpressionFactory.CreateQuantityAdd(Expr.Variable("x"), Expr.Variable("m"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateQuantityMultiply_NullLeft_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityMultiply(null!, Expr.Literal(1));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityMultiply_NullRight_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityMultiply(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityMultiply_ReturnsBinaryExpression()
    {
        var result = QuantityExpressionFactory.CreateQuantityMultiply(Expr.Variable("a"), Expr.Variable("b"));
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CreateQuantityDivide_NullLeft_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityDivide(null!, Expr.Literal(1));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityDivide_NullRight_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityDivide(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityDivide_ReturnsBinaryExpression()
    {
        var result = QuantityExpressionFactory.CreateQuantityDivide(Expr.Variable("a"), Expr.Variable("b"));
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CreateQuantityPower_NullBase_Throws()
    {
        Action act = () => QuantityExpressionFactory.CreateQuantityPower(null!, 2.0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateQuantityPower_ReturnsBinaryExpression()
    {
        var result = QuantityExpressionFactory.CreateQuantityPower(Expr.Variable("x"), 3.0);
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CreateQuantityPower_ExponentIsLiteral()
    {
        var binary = (BinaryExpression)QuantityExpressionFactory.CreateQuantityPower(Expr.Variable("x"), 2.5);
        binary.Right.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)binary.Right).Value.Should().Be(2.5);
    }
}

public sealed class DimensionalTypeBridgeTests
{
    [Fact]
    public void ToDimension_NullType_Throws()
    {
        Action act = () => DimensionalTypeBridge.ToDimension(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDimension_IntegerType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(IntegerType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_RealType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(RealType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_ComplexType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(ComplexType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_VectorType_ReturnsLength()
    {
        var result = DimensionalTypeBridge.ToDimension(new VectorType(RealType.Instance));
        result.Exponents.Should().ContainKey("L");
        result.Exponents["L"].Should().Be(1.0);
    }

    [Fact]
    public void ToDimension_MatrixType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(new MatrixType(RealType.Instance)).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_TensorType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(new TensorType(RealType.Instance, new int?[] { 3 })).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_BooleanType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(BooleanType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_StringType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(StringType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToMathType_NullDim_Throws()
    {
        Action act = () => DimensionalTypeBridge.ToMathType(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToMathType_Dimensionless_ReturnsRealType()
    {
        DimensionalTypeBridge.ToMathType(Dimension.None).Should().Be(RealType.Instance);
    }

    [Fact]
    public void ToMathType_Length_ReturnsVectorType()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionalTypeBridge.ToMathType(length);
        result.Should().BeOfType<VectorType>();
    }

    [Fact]
    public void ToMathType_ComplexDimension_ReturnsRealType()
    {
        var force = DerivedDimension.Force;
        DimensionalTypeBridge.ToMathType(force).Should().Be(RealType.Instance);
    }

    [Fact]
    public void AreEquivalent_NullType_Throws()
    {
        Action act = () => DimensionalTypeBridge.AreEquivalent(null!, Dimension.None);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AreEquivalent_NullDim_Throws()
    {
        Action act = () => DimensionalTypeBridge.AreEquivalent(RealType.Instance, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AreEquivalent_DimensionlessTypeAndDim_ReturnsTrue()
    {
        DimensionalTypeBridge.AreEquivalent(RealType.Instance, Dimension.None).Should().BeTrue();
    }

    [Fact]
    public void AreEquivalent_VectorTypeAndLengthDim_ReturnsTrue()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionalTypeBridge.AreEquivalent(new VectorType(RealType.Instance), length).Should().BeTrue();
    }

    [Fact]
    public void AreEquivalent_VectorTypeAndMassDim_ReturnsFalse()
    {
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionalTypeBridge.AreEquivalent(new VectorType(RealType.Instance), mass).Should().BeFalse();
    }

    [Fact]
    public void ToDimension_RationalType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(RationalType.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_SetType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(new SetType(RealType.Instance)).Should().Be(Dimension.None);
    }

    [Fact]
    public void ToDimension_FunctionType_ReturnsNone()
    {
        DimensionalTypeBridge.ToDimension(new FunctionType(new[] { RealType.Instance }, RealType.Instance)).Should().Be(Dimension.None);
    }
}

[Collection("DimensionAnalyzer")]
public sealed class SemanticDimensionValidatorTests : IDisposable
{
    public SemanticDimensionValidatorTests()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    public void Dispose()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    [Fact]
    public void ValidateSemanticTree_NullExpr_Throws()
    {
        Action act = () => SemanticDimensionValidator.ValidateSemanticTree(null!, new Dictionary<string, Dimension>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateSemanticTree_NullDict_Throws()
    {
        Action act = () => SemanticDimensionValidator.ValidateSemanticTree(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateSemanticTree_EmptyDict_NoErrors()
    {
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Literal(5), new Dictionary<string, Dimension>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSemanticTree_CompatibleAddition_NoErrors()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var dims = new Dictionary<string, Dimension>
        {
            ["x"] = length,
            ["y"] = length
        };
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Add(Expr.Variable("x"), Expr.Variable("y")), dims);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSemanticTree_IncompatibleAddition_HasErrors()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var dims = new Dictionary<string, Dimension>
        {
            ["x"] = length,
            ["y"] = mass
        };
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Add(Expr.Variable("x"), Expr.Variable("y")), dims);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void InferFromSemanticContext_NullOperation_Throws()
    {
        Action act = () => SemanticDimensionValidator.InferFromSemanticContext(null!, [Dimension.None]);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InferFromSemanticContext_NullArgs_Throws()
    {
        Action act = () => SemanticDimensionValidator.InferFromSemanticContext("+", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InferFromSemanticContext_Addition_ReturnsFirstDim()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = SemanticDimensionValidator.InferFromSemanticContext("+", [length, length]);
        result.Should().Be(length);
    }

    [Fact]
    public void InferFromSemanticContext_Multiplication_ReturnsProduct()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: -1);
        var result = SemanticDimensionValidator.InferFromSemanticContext("*", [length, time]);
        result!.IsCompatibleWith(DerivedDimension.Velocity).Should().BeTrue();
    }

    [Fact]
    public void ValidateSemanticTree_Subtraction_Incompatible_HasErrors()
    {
        var time = Dimension.FromBaseDimensions(time: 1);
        var length = Dimension.FromBaseDimensions(length: 1);
        var dims = new Dictionary<string, Dimension>
        {
            ["a"] = time,
            ["b"] = length
        };
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Subtract(Expr.Variable("a"), Expr.Variable("b")), dims);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateSemanticTree_Multiplication_NoErrors()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var dims = new Dictionary<string, Dimension>
        {
            ["x"] = length,
            ["m"] = mass
        };
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Multiply(Expr.Variable("x"), Expr.Variable("m")), dims);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSemanticTree_NestedIncompatible_HasErrors()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var dims = new Dictionary<string, Dimension>
        {
            ["x"] = length,
            ["m"] = mass
        };
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Add(Expr.Variable("x"), Expr.Multiply(Expr.Literal(2), Expr.Variable("m"))),
            dims);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateSemanticTree_CleansAnalyzerFirst()
    {
        DimensionAnalyzer.Instance.SetVariableDimension("pre_existing", DerivedDimension.Force);
        var result = SemanticDimensionValidator.ValidateSemanticTree(
            Expr.Literal(42), new Dictionary<string, Dimension>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExpressionDimensionExtensions_GetDimension_VariableWithDimension()
    {
        var expr = Expr.Variable("x");
        DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
        expr.GetDimension(DimensionAnalyzer.Instance).Should().Be(Meter.Dimension);
    }

    [Fact]
    public void ExpressionDimensionExtensions_GetDimension_Literal()
    {
        Expr.Literal(5.0).GetDimension(DimensionAnalyzer.Instance).Should().Be(Dimension.None);
    }

    [Fact]
    public void ExpressionDimensionExtensions_GetDimension_BinaryAdd()
    {
        var left = Expr.Variable("x");
        var right = Expr.Variable("y");
        DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
        DimensionAnalyzer.Instance.SetVariableDimension("y", Meter.Dimension);
        var expr = Expr.Add(left, right);
        expr.GetDimension(DimensionAnalyzer.Instance).Should().Be(Meter.Dimension);
    }

    [Fact]
    public void ExpressionDimensionExtensions_IsDimensionallyConsistent_Valid()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
        DimensionAnalyzer.Instance.SetVariableDimension("y", Meter.Dimension);
        expr.IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeTrue();
    }

    [Fact]
    public void ExpressionDimensionExtensions_IsDimensionallyConsistent_Invalid()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
        DimensionAnalyzer.Instance.SetVariableDimension("y", Kilogram.Dimension);
        expr.IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeFalse();
    }

    [Fact]
    public void ExpressionDimensionExtensions_EvaluateAsQuantity_Simple()
    {
        var expr = Expr.Literal(5.0);
        var vars = new Dictionary<string, PhysicalQuantity>();
        var result = expr.EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(5.0);
    }

    [Fact]
    public void ExpressionDimensionExtensions_EvaluateAsQuantity_Variable()
    {
        var expr = Expr.Variable("x");
        var vars = new Dictionary<string, PhysicalQuantity>
        {
            ["x"] = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension }
        };
        var result = expr.EvaluateAsQuantity(vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(10.0);
    }

    [Fact]
    public void ExpressionDimensionExtensions_EvaluateAsQuantity_MissingVariable()
    {
        var expr = Expr.Variable("missing");
        var result = expr.EvaluateAsQuantity(new Dictionary<string, PhysicalQuantity>());
        result.Should().BeNull();
    }

    [Fact]
    public void ExpressionDimensionExtensions_WithDimensions_AddsMetadata()
    {
        var expr = Expr.Variable("x");
        var dims = new Dictionary<string, Dimension> { ["x"] = Meter.Dimension };
        var annotated = expr.WithDimensions(dims);
        annotated.Should().NotBeNull();
    }

    [Fact]
    public void QuantityExpressionFactory_CreateQuantityExpression()
    {
        var expr = QuantityExpressionFactory.CreateQuantityExpression(5.0, Meter);
        expr.Should().NotBeNull();
    }

    [Fact]
    public void QuantityExpressionFactory_CreateQuantityAdd()
    {
        var left = Expr.Literal(1.0);
        var right = Expr.Literal(2.0);
        var expr = QuantityExpressionFactory.CreateQuantityAdd(left, right);
        expr.Should().NotBeNull();
    }

    [Fact]
    public void QuantityExpressionFactory_CreateQuantityMultiply()
    {
        var left = Expr.Literal(2.0);
        var right = Expr.Variable("t");
        var expr = QuantityExpressionFactory.CreateQuantityMultiply(left, right);
        expr.Should().NotBeNull();
    }

    [Fact]
    public void QuantityExpressionFactory_CreateQuantityDivide()
    {
        var left = Expr.Variable("d");
        var right = Expr.Variable("t");
        var expr = QuantityExpressionFactory.CreateQuantityDivide(left, right);
        expr.Should().NotBeNull();
    }

    [Fact]
    public void QuantityExpressionFactory_CreateQuantityPower()
    {
        var baseExpr = Expr.Variable("x");
        var expr = QuantityExpressionFactory.CreateQuantityPower(baseExpr, 2.0);
        expr.Should().NotBeNull();
    }

    [Fact]
    public void DimensionalTypeBridge_ToDimension_RealType()
    {
        var dim = DimensionalTypeBridge.ToDimension(MathVerse.Math.Types.RealType.Instance);
        dim.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionalTypeBridge_ToDimension_IntegerType()
    {
        var dim = DimensionalTypeBridge.ToDimension(MathVerse.Math.Types.IntegerType.Instance);
        dim.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionalTypeBridge_ToDimension_VectorType()
    {
        var dim = DimensionalTypeBridge.ToDimension(new MathVerse.Math.Types.VectorType(Meter.Dimension, 3));
        dim.Should().NotBeNull();
    }

    [Fact]
    public void DimensionalTypeBridge_ToDimension_MatrixType()
    {
        var dim = DimensionalTypeBridge.ToDimension(new MathVerse.Math.Types.MatrixType(Meter.Dimension, 2, 3));
        dim.Should().NotBeNull();
    }

    [Fact]
    public void DimensionalTypeBridge_ToMathType_Dimensionless()
    {
        var type = DimensionalTypeBridge.ToMathType(Dimension.None);
        type.Should().Be(MathVerse.Math.Types.RealType.Instance);
    }

    [Fact]
    public void DimensionalTypeBridge_AreEquivalent_SameDimension()
    {
        DimensionalTypeBridge.AreEquivalent(MathVerse.Math.Types.RealType.Instance, Dimension.None).Should().BeTrue();
    }

    [Fact]
    public void DimensionalTypeBridge_AreEquivalent_DifferentDimension()
    {
        DimensionalTypeBridge.AreEquivalent(MathVerse.Math.Types.RealType.Instance, Meter.Dimension).Should().BeFalse();
    }

    [Fact]
    public void SemanticDimensionValidator_ValidateSemanticTree_EmptyExpression()
    {
        var result = SemanticDimensionValidator.ValidateSemanticTree(Expr.Literal(1), new Dictionary<string, Dimension>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void SemanticDimensionValidator_ValidateSemanticTree_ComplexExpression()
    {
        var dims = new Dictionary<string, Dimension>
        {
            ["x"] = Meter.Dimension,
            ["t"] = Second.Dimension
        };
        var expr = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Variable("x")),
            Expr.Divide(Expr.Variable("t"), Expr.Literal(2.0))
        );
        var result = SemanticDimensionValidator.ValidateSemanticTree(expr, dims);
        result.Should().BeEmpty();
    }

    [Fact]
    public void SemanticDimensionValidator_InferFromSemanticContext_Add()
    {
        var args = new[] { Meter.Dimension, Meter.Dimension };
        var result = SemanticDimensionValidator.InferFromSemanticContext("+", args);
        result.Should().Be(Meter.Dimension);
    }

    [Fact]
    public void SemanticDimensionValidator_InferFromSemanticContext_Multiply()
    {
        var args = new[] { Meter.Dimension, Second.Dimension };
        var result = SemanticDimensionValidator.InferFromSemanticContext("*", args);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void SemanticDimensionValidator_InferFromSemanticContext_Divide()
    {
        var args = new[] { Meter.Dimension, Second.Dimension };
        var result = SemanticDimensionValidator.InferFromSemanticContext("/", args);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void SemanticDimensionValidator_InferFromSemanticContext_Power()
    {
        var args = new[] { Meter.Dimension };
        var result = SemanticDimensionValidator.InferFromSemanticContext("^", args);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void SemanticDimensionValidator_InferFromSemanticContext_Sin()
    {
        var args = new[] { Meter.Dimension };
        var result = SemanticDimensionValidator.InferFromSemanticContext("sin", args);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void ExpressionDimensionExtensions_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            var expr = Expr.Add(Expr.Literal(1), Expr.Variable("x"));
            DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
            expr.IsDimensionallyConsistent(DimensionAnalyzer.Instance).Should().BeTrue();
            DimensionAnalyzer.Instance.Clear();
        });
    }

    [Fact]
    public void QuantityExpressionFactory_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            var expr = QuantityExpressionFactory.CreateQuantityMultiply(Expr.Literal(2), Expr.Variable("x"));
            expr.Should().NotBeNull();
        });
    }

    [Fact]
    public void DimensionalTypeBridge_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            DimensionalTypeBridge.AreEquivalent(MathVerse.Math.Types.RealType.Instance, Dimension.None).Should().BeTrue();
        });
    }

    [Fact]
    public void SemanticDimensionValidator_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            var result = SemanticDimensionValidator.ValidateSemanticTree(Expr.Literal(1), new Dictionary<string, Dimension>());
            result.Should().BeEmpty();
        });
    }
}
