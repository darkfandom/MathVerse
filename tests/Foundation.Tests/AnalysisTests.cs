using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Operators;

namespace MathVerse.Foundation.Tests;

public sealed class DimensionRuleTests : IDisposable
{
    public void Dispose() { }

    [Fact]
    public void DimensionRule_HasAllExpectedValues()
    {
        var values = Enum.GetValues<DimensionRule>();
        values.Should().HaveCount(10);
    }

    [Fact]
    public void DimensionRule_Addition_Exists()
    {
        DimensionRule.Addition.Should().Be((DimensionRule)0);
    }

    [Fact]
    public void DimensionRule_Subtraction_Exists()
    {
        DimensionRule.Subtraction.Should().Be((DimensionRule)1);
    }

    [Fact]
    public void DimensionRule_Multiplication_Exists()
    {
        DimensionRule.Multiplication.Should().Be((DimensionRule)2);
    }

    [Fact]
    public void DimensionRule_Division_Exists()
    {
        DimensionRule.Division.Should().Be((DimensionRule)3);
    }

    [Fact]
    public void DimensionRule_Power_Exists()
    {
        DimensionRule.Power.Should().Be((DimensionRule)4);
    }

    [Fact]
    public void DimensionRule_Function_Exists()
    {
        DimensionRule.Function.Should().Be((DimensionRule)5);
    }

    [Fact]
    public void DimensionRule_Assignment_Exists()
    {
        DimensionRule.Assignment.Should().Be((DimensionRule)6);
    }

    [Fact]
    public void DimensionRule_Comparison_Exists()
    {
        DimensionRule.Comparison.Should().Be((DimensionRule)7);
    }

    [Fact]
    public void DimensionRule_Literal_Exists()
    {
        DimensionRule.Literal.Should().Be((DimensionRule)8);
    }

    [Fact]
    public void DimensionRule_Variable_Exists()
    {
        DimensionRule.Variable.Should().Be((DimensionRule)9);
    }
}

public sealed class DimensionDiagnosticTests
{
    [Fact]
    public void Diagnostic_DefaultValues_AreEmpty()
    {
        var diag = new DimensionDiagnostic();
        diag.Message.Should().BeEmpty();
        diag.Expression.Should().BeEmpty();
        diag.ExpectedDimension.Should().BeNull();
        diag.ActualDimension.Should().BeNull();
    }

    [Fact]
    public void Diagnostic_SetRule_ReturnsCorrectValue()
    {
        var diag = new DimensionDiagnostic { Rule = DimensionRule.Addition };
        diag.Rule.Should().Be(DimensionRule.Addition);
    }

    [Fact]
    public void Diagnostic_SetMessage_ReturnsCorrectValue()
    {
        var diag = new DimensionDiagnostic { Message = "test message" };
        diag.Message.Should().Be("test message");
    }

    [Fact]
    public void Diagnostic_SetExpression_ReturnsCorrectValue()
    {
        var diag = new DimensionDiagnostic { Expression = "x + y" };
        diag.Expression.Should().Be("x + y");
    }

    [Fact]
    public void Diagnostic_SetExpectedDimension_ReturnsCorrectValue()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var diag = new DimensionDiagnostic { ExpectedDimension = dim };
        diag.ExpectedDimension.Should().Be(dim);
    }

    [Fact]
    public void Diagnostic_SetActualDimension_ReturnsCorrectValue()
    {
        var dim = Dimension.FromBaseDimensions(mass: 1);
        var diag = new DimensionDiagnostic { ActualDimension = dim };
        diag.ActualDimension.Should().Be(dim);
    }

    [Fact]
    public void Diagnostic_IsRecord_SupportsWithExpression()
    {
        var diag = new DimensionDiagnostic { Rule = DimensionRule.Multiplication, Message = "original" };
        var modified = diag with { Message = "modified" };
        modified.Message.Should().Be("modified");
        diag.Message.Should().Be("original");
    }
}

public sealed class DimensionDiagnosticsTests
{
    [Fact]
    public void Diagnostics_EmptyByDefault()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Diagnostics_HasErrors_FalseWhenEmpty()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Diagnostics_HasWarnings_FalseWhenEmpty()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void Diagnostics_Add_IncreasesCount()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic { Message = "error" });
        diagnostics.Diagnostics.Should().HaveCount(1);
    }

    [Fact]
    public void Diagnostics_HasErrors_TrueAfterAdd()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic { Message = "error" });
        diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Diagnostics_Add_NullThrows()
    {
        var diagnostics = new DimensionDiagnostics();
        Action act = () => diagnostics.Add(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Diagnostics_Clear_RemovesAll()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic { Message = "e1" });
        diagnostics.Add(new DimensionDiagnostic { Message = "e2" });
        diagnostics.Clear();
        diagnostics.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Diagnostics_HasWarnings_TrueWhenBothDimensionsSet()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic
        {
            ExpectedDimension = Dimension.FromBaseDimensions(length: 1),
            ActualDimension = Dimension.FromBaseDimensions(mass: 1)
        });
        diagnostics.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void Diagnostics_HasWarnings_FalseWhenOnlyExpectedSet()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic
        {
            ExpectedDimension = Dimension.FromBaseDimensions(length: 1)
        });
        diagnostics.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void Diagnostics_HasWarnings_FalseWhenOnlyActualSet()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Add(new DimensionDiagnostic
        {
            ActualDimension = Dimension.FromBaseDimensions(mass: 1)
        });
        diagnostics.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void Diagnostics_DiagnosticList_IsReadOnly()
    {
        var diagnostics = new DimensionDiagnostics();
        diagnostics.Diagnostics.Should().BeAssignableTo<IReadOnlyList<DimensionDiagnostic>>();
    }
}

public sealed class DimensionCheckerTests
{
    [Fact]
    public void Check_SameDimensionless_ReturnsTrue()
    {
        DimensionChecker.Check(Dimension.None, Dimension.None).Should().BeTrue();
    }

    [Fact]
    public void Check_SameLength_ReturnsTrue()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionChecker.Check(length, length).Should().BeTrue();
    }

    [Fact]
    public void Check_LengthVsMass_ReturnsFalse()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionChecker.Check(length, mass).Should().BeFalse();
    }

    [Fact]
    public void AreDimensionsCompatible_SameDimensions_ReturnsTrue()
    {
        var time = Dimension.FromBaseDimensions(time: 1);
        DimensionChecker.AreDimensionsCompatible(time, time).Should().BeTrue();
    }

    [Fact]
    public void AreDimensionsCompatible_DifferentDimensions_ReturnsFalse()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: 1);
        DimensionChecker.AreDimensionsCompatible(length, time).Should().BeFalse();
    }

    [Fact]
    public void CheckAddition_CompatibleDimensions_ReturnsTrue()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionChecker.CheckAddition(dim, dim).Should().BeTrue();
    }

    [Fact]
    public void CheckAddition_IncompatibleDimensions_ReturnsFalse()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionChecker.CheckAddition(length, mass).Should().BeFalse();
    }

    [Fact]
    public void CheckEquality_SameDimensionless_ReturnsTrue()
    {
        DimensionChecker.CheckEquality(Dimension.None, Dimension.None).Should().BeTrue();
    }

    [Fact]
    public void CheckAssignment_Compatible_ReturnsTrue()
    {
        var dim = Dimension.FromBaseDimensions(time: 1);
        DimensionChecker.CheckAssignment(dim, dim).Should().BeTrue();
    }

    [Fact]
    public void CheckAssignment_Incompatible_ReturnsFalse()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionChecker.CheckAssignment(length, mass).Should().BeFalse();
    }

    [Fact]
    public void Check_DerivedVelocity_CompatibleWithSelf()
    {
        var velocity = DerivedDimension.Velocity;
        DimensionChecker.Check(velocity, velocity).Should().BeTrue();
    }

    [Fact]
    public void Check_ForceVsEnergy_Incompatible()
    {
        DimensionChecker.Check(DerivedDimension.Force, DerivedDimension.Energy).Should().BeFalse();
    }
}

public sealed class DimensionInferenceEngineTests
{
    [Fact]
    public void InferFromContext_NullOperation_Throws()
    {
        Action act = () => DimensionInferenceEngine.InferFromContext(null!, [Dimension.None]);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InferFromContext_NullArguments_ReturnsNone()
    {
        DimensionInferenceEngine.InferFromContext("+", null!).Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_EmptyArguments_ReturnsNone()
    {
        DimensionInferenceEngine.InferFromContext("+", []).Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Addition_ReturnsFirstArgument()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionInferenceEngine.InferFromContext("+", [length, length]);
        result.Should().Be(length);
    }

    [Fact]
    public void InferFromContext_Subtraction_ReturnsFirstArgument()
    {
        var time = Dimension.FromBaseDimensions(time: 1);
        var result = DimensionInferenceEngine.InferFromContext("-", [time, time]);
        result.Should().Be(time);
    }

    [Fact]
    public void InferFromContext_Multiplication_ProductOfDimensions()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: -1);
        var result = DimensionInferenceEngine.InferFromContext("*", [length, time]);
        result!.IsCompatibleWith(DerivedDimension.Velocity).Should().BeTrue();
    }

    [Fact]
    public void InferFromContext_Division_ResultDimension()
    {
        var energy = DerivedDimension.Energy;
        var time = Dimension.FromBaseDimensions(time: 1);
        var result = DimensionInferenceEngine.InferFromContext("/", [energy, time]);
        result!.IsCompatibleWith(DerivedDimension.Power).Should().BeTrue();
    }

    [Fact]
    public void InferFromContext_DivisionSingleArg_ReturnsArg()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionInferenceEngine.InferFromContext("/", [dim]);
        result.Should().Be(dim);
    }

    [Fact]
    public void InferFromContext_Power_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("^", [Dimension.FromBaseDimensions(length: 1)]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Sqrt_ReturnsSqrtDimension()
    {
        var area = Dimension.FromBaseDimensions(length: 2);
        var result = DimensionInferenceEngine.InferFromContext("sqrt", [area]);
        result!.IsCompatibleWith(Dimension.FromBaseDimensions(length: 1)).Should().BeTrue();
    }

    [Fact]
    public void InferFromContext_Sin_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("sin", [Dimension.FromBaseDimensions(length: 1)]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Cos_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("cos", [Dimension.None]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Tan_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("tan", [Dimension.FromBaseDimensions(time: 1)]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Ln_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("ln", [Dimension.FromBaseDimensions(mass: 1)]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_Exp_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("exp", [Dimension.None]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFromContext_UnknownFunction_ReturnsNone()
    {
        var result = DimensionInferenceEngine.InferFromContext("myFunc", [Dimension.FromBaseDimensions(length: 1)]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferLiteralDimension_AlwaysReturnsNone()
    {
        var result = DimensionInferenceEngine.InferLiteralDimension(42.0, null);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferLiteralDimension_WithContext_ReturnsNone()
    {
        var ctx = new Dictionary<string, Dimension> { ["x"] = Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferLiteralDimension(5.0, ctx);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferBinaryDimension_NullLeft_ReturnsNull()
    {
        var result = DimensionInferenceEngine.InferBinaryDimension(
            MathOperator.Add, null, Dimension.FromBaseDimensions(length: 1));
        result.Should().BeNull();
    }

    [Fact]
    public void InferBinaryDimension_NullRight_ReturnsNull()
    {
        var result = DimensionInferenceEngine.InferBinaryDimension(
            MathOperator.Add, Dimension.FromBaseDimensions(length: 1), null);
        result.Should().BeNull();
    }

    [Fact]
    public void InferBinaryDimension_AddCompatible_ReturnsLeft()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Add, length, length);
        result.Should().Be(length);
    }

    [Fact]
    public void InferBinaryDimension_AddIncompatible_ReturnsNull()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Add, length, mass);
        result.Should().BeNull();
    }

    [Fact]
    public void InferBinaryDimension_SubtractCompatible_ReturnsLeft()
    {
        var time = Dimension.FromBaseDimensions(time: 1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Subtract, time, time);
        result.Should().Be(time);
    }

    [Fact]
    public void InferBinaryDimension_Multiply_ReturnsProduct()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: -1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Multiply, length, time);
        result!.IsCompatibleWith(DerivedDimension.Velocity).Should().BeTrue();
    }

    [Fact]
    public void InferBinaryDimension_Divide_ReturnsQuotient()
    {
        var energy = DerivedDimension.Energy;
        var time = Dimension.FromBaseDimensions(time: 1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Divide, energy, time);
        result!.IsCompatibleWith(DerivedDimension.Power).Should().BeTrue();
    }

    [Fact]
    public void InferBinaryDimension_Power_ReturnsNone()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionInferenceEngine.InferBinaryDimension(MathOperator.Power, length, length);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFunctionDimension_DelegatesToInferFromContext()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionInferenceEngine.InferFunctionDimension("sin", [dim]);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void InferFunctionDimension_Sqrt_ReturnsSqrtDimension()
    {
        var area = Dimension.FromBaseDimensions(length: 2);
        var result = DimensionInferenceEngine.InferFunctionDimension("sqrt", [area]);
        result!.IsCompatibleWith(Dimension.FromBaseDimensions(length: 1)).Should().BeTrue();
    }
}

[Collection("DimensionAnalyzer")]
public sealed class DimensionAnalyzerTests : IDisposable
{
    public DimensionAnalyzerTests()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    public void Dispose()
    {
        DimensionAnalyzer.Instance.Clear();
    }

    [Fact]
    public void Instance_IsSingleton()
    {
        var a = DimensionAnalyzer.Instance;
        var b = DimensionAnalyzer.Instance;
        a.Should().BeSameAs(b);
    }

    [Fact]
    public void AnalyzeExpression_Null_Throws()
    {
        Action act = () => DimensionAnalyzer.Instance.AnalyzeExpression(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AnalyzeExpression_Literal_ReturnsNone()
    {
        var expr = Expr.Literal(42);
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_VariableWithNoDimension_ReturnsNone()
    {
        var expr = Expr.Variable("x");
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_VariableWithSetDimension_ReturnsDimension()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(Expr.Variable("x"));
        result.Should().Be(dim);
    }

    [Fact]
    public void AnalyzeExpression_Constant_ReturnsNone()
    {
        var expr = ConstantExpression.Pi;
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_BinaryAddition_ReturnsLeftDimension()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("y", length);
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.Should().Be(length);
    }

    [Fact]
    public void AnalyzeExpression_BinaryMultiplication_CombinesDimensions()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("t", time);
        var expr = Expr.Multiply(Expr.Variable("x"), Expr.Variable("t"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.IsCompatibleWith(length.Multiply(time)).Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_BinaryDivision_DividesDimensions()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("t", time);
        var expr = Expr.Divide(Expr.Variable("x"), Expr.Variable("t"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.IsCompatibleWith(length.Divide(time)).Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_BinaryPower_ReturnsNone()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        var expr = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_UnaryNegate_PassesThrough()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        var expr = Expr.Negate(Expr.Variable("x"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.Should().Be(length);
    }

    [Fact]
    public void AnalyzeExpression_FunctionCall_SinReturnsNone()
    {
        var expr = Expr.Sin(Expr.Literal(1));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_FunctionCall_SqrtInfersDimension()
    {
        var area = Dimension.FromBaseDimensions(length: 2);
        var analyzer = DimensionAnalyzer.Instance;
        var sqrtExpr = Expr.Sqrt(new VariableExpression("A"));
        var funcCall = (FunctionCallExpression)sqrtExpr;
        analyzer.SetVariableDimension("A", area);
        var result = analyzer.AnalyzeExpression(funcCall);
        result!.IsCompatibleWith(Dimension.FromBaseDimensions(length: 1)).Should().BeTrue();
    }

    [Fact]
    public void CheckDimensionalConsistency_ClearsDiagnostics()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        DimensionAnalyzer.Instance.SetVariableDimension("y", mass);
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        DimensionAnalyzer.Instance.CheckDimensionalConsistency(expr);
        DimensionAnalyzer.Instance.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void GetResultDimension_ReturnsSameAsAnalyzeExpression()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        var expr = Expr.Variable("x");
        var r1 = DimensionAnalyzer.Instance.GetResultDimension(expr);
        var r2 = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        r1.Should().Be(r2);
    }

    [Fact]
    public void SetVariableDimension_NullName_Throws()
    {
        Action act = () => DimensionAnalyzer.Instance.SetVariableDimension(null!, Dimension.None);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetVariableDimension_NullDimension_Throws()
    {
        Action act = () => DimensionAnalyzer.Instance.SetVariableDimension("x", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetVariableDimension_NullName_Throws()
    {
        Action act = () => DimensionAnalyzer.Instance.GetVariableDimension(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetVariableDimension_UnknownVariable_ReturnsNone()
    {
        DimensionAnalyzer.Instance.GetVariableDimension("unknown").Should().Be(Dimension.None);
    }

    [Fact]
    public void Clear_RemovesVariableDimensions()
    {
        DimensionAnalyzer.Instance.SetVariableDimension("x", Dimension.FromBaseDimensions(length: 1));
        DimensionAnalyzer.Instance.Clear();
        DimensionAnalyzer.Instance.GetVariableDimension("x").Should().Be(Dimension.None);
    }

    [Fact]
    public void Clear_RemovesDiagnostics()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", dim);
        DimensionAnalyzer.Instance.SetVariableDimension("y", mass);
        DimensionAnalyzer.Instance.CheckDimensionalConsistency(
            Expr.Add(Expr.Variable("x"), Expr.Variable("y")));
        DimensionAnalyzer.Instance.Clear();
        DimensionAnalyzer.Instance.Diagnostics.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Diagnostics_Property_IsAccessible()
    {
        DimensionAnalyzer.Instance.Diagnostics.Should().NotBeNull();
    }

    [Fact]
    public void AnalyzeExpression_AdditionIncompatible_DiagnosticRecorded()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("y", mass);
        DimensionAnalyzer.Instance.AnalyzeExpression(Expr.Add(Expr.Variable("x"), Expr.Variable("y")));
        DimensionAnalyzer.Instance.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_SubtractionIncompatible_DiagnosticRecorded()
    {
        var time = Dimension.FromBaseDimensions(time: 1);
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("a", time);
        DimensionAnalyzer.Instance.SetVariableDimension("b", length);
        DimensionAnalyzer.Instance.AnalyzeExpression(Expr.Subtract(Expr.Variable("a"), Expr.Variable("b")));
        DimensionAnalyzer.Instance.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_CompoundMultiplication()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("m", mass);
        var expr = Expr.Multiply(Expr.Variable("x"), Expr.Variable("m"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.IsCompatibleWith(length.Multiply(mass)).Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_NestedBinary_CorrectDimension()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("x", length);
        DimensionAnalyzer.Instance.SetVariableDimension("t", time);
        var expr = Expr.Divide(
            Expr.Multiply(Expr.Literal(2), Expr.Variable("x")),
            Expr.Variable("t"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.IsCompatibleWith(length.Divide(time)).Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_AdditionCompatible_NoDiagnostic()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("a", length);
        DimensionAnalyzer.Instance.SetVariableDimension("b", length);
        DimensionAnalyzer.Instance.AnalyzeExpression(Expr.Add(Expr.Variable("a"), Expr.Variable("b")));
        DimensionAnalyzer.Instance.Diagnostics.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void AnalyzeExpression_UnknownExpressionType_ReturnsNone()
    {
        var annExpr = new LiteralExpression(5).WithAnnotation("unit", "meter");
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(annExpr);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void SetVariableDimension_CaseInsensitive()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("X", dim);
        var result = DimensionAnalyzer.Instance.GetVariableDimension("x");
        result.Should().Be(dim);
    }

    [Fact]
    public void AnalyzeExpression_ComplexNestedExpression()
    {
        var mass = Dimension.FromBaseDimensions(mass: 1);
        DimensionAnalyzer.Instance.SetVariableDimension("m", mass);
        var vDim = DerivedDimension.Velocity;
        DimensionAnalyzer.Instance.SetVariableDimension("v", vDim);
        var kineticEnergy = Expr.Multiply(
            Expr.Literal(0.5),
            Expr.Multiply(Expr.Variable("m"), Expr.Pow(Expr.Variable("v"), Expr.Literal(2))));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(kineticEnergy);
        result.IsCompatibleWith(mass).Should().BeTrue();
    }

    [Fact]
    public void AnalyzeExpression_FuncCallCos_ReturnsNone()
    {
        var expr = Expr.Cos(Expr.Literal(0));
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_FuncCallLn_ReturnsNone()
    {
        var expr = Expr.Ln(Expr.Variable("x"));
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_FuncCallExp_ReturnsNone()
    {
        var expr = Expr.Exp(Expr.Literal(1));
        DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_MultiplyByLiteral_PreservesDimension()
    {
        var force = DerivedDimension.Force;
        DimensionAnalyzer.Instance.SetVariableDimension("F", force);
        var expr = Expr.Multiply(Expr.Literal(2), Expr.Variable("F"));
        var result = DimensionAnalyzer.Instance.AnalyzeExpression(expr);
        result.Should().Be(force);
    }

    [Fact]
    public void DimensionChecker_CheckAddition_SameDimension()
    {
        var q1 = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        DimensionChecker.Instance.CheckAddition(q1, q2).Should().BeNull();
    }

    [Fact]
    public void DimensionChecker_CheckAddition_DifferentDimension()
    {
        var q1 = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        var diag = DimensionChecker.Instance.CheckAddition(q1, q2);
        diag.Should().NotBeNull();
        diag!.Rule.Should().Be(DimensionRule.IncompatibleOperation);
    }

    [Fact]
    public void DimensionChecker_CheckEquality_SameDimension()
    {
        var q1 = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        DimensionChecker.Instance.CheckEquality(q1, q2).Should().BeNull();
    }

    [Fact]
    public void DimensionChecker_CheckEquality_DifferentDimension()
    {
        var q1 = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Second, Dimension = Second.Dimension };
        var diag = DimensionChecker.Instance.CheckEquality(q1, q2);
        diag.Should().NotBeNull();
    }

    [Fact]
    public void DimensionChecker_CheckAssignment_Compatible()
    {
        var expected = Meter.Dimension;
        var actual = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        DimensionChecker.Instance.CheckAssignment("x", actual, expected).Should().BeNull();
    }

    [Fact]
    public void DimensionChecker_CheckAssignment_Incompatible()
    {
        var expected = Meter.Dimension;
        var actual = new PhysicalQuantity { Value = 5.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        var diag = DimensionChecker.Instance.CheckAssignment("x", actual, expected);
        diag.Should().NotBeNull();
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Addition()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1), Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFromContext("+", dims);
        result.Should().Be(Dimension.FromBaseDimensions(length: 1));
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Multiplication()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1), Dimension.FromBaseDimensions(time: -1) };
        var result = DimensionInferenceEngine.InferFromContext("*", dims);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Division()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1), Dimension.FromBaseDimensions(time: 1) };
        var result = DimensionInferenceEngine.InferFromContext("/", dims);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Sqrt()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 2) };
        var result = DimensionInferenceEngine.InferFromContext("sqrt", dims);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Sin()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFromContext("sin", dims);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_Exp()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFromContext("exp", dims);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFromContext_UnknownFunction()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFromContext("unknown", dims);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferLiteralDimension()
    {
        var result = DimensionInferenceEngine.InferLiteralDimension(42.0, null);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferBinaryDimension_Add()
    {
        var left = Dimension.FromBaseDimensions(length: 1);
        var right = Dimension.FromBaseDimensions(length: 1);
        var op = MathOperator.Add;
        var result = DimensionInferenceEngine.InferBinaryDimension(op, left, right);
        result.Should().Be(left);
    }

    [Fact]
    public void DimensionInferenceEngine_InferBinaryDimension_Multiply()
    {
        var left = Dimension.FromBaseDimensions(length: 1);
        var right = Dimension.FromBaseDimensions(time: -1);
        var op = MathOperator.Multiply;
        var result = DimensionInferenceEngine.InferBinaryDimension(op, left, right);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionInferenceEngine_InferBinaryDimension_Divide()
    {
        var left = Dimension.FromBaseDimensions(length: 1);
        var right = Dimension.FromBaseDimensions(time: 1);
        var op = MathOperator.Divide;
        var result = DimensionInferenceEngine.InferBinaryDimension(op, left, right);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionInferenceEngine_InferBinaryDimension_Power()
    {
        var left = Dimension.FromBaseDimensions(length: 1);
        var right = Dimension.None;
        var op = MathOperator.Power;
        var result = DimensionInferenceEngine.InferBinaryDimension(op, left, right);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFunctionDimension_Sin()
    {
        var args = new[] { Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFunctionDimension("sin", args);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionInferenceEngine_InferFunctionDimension_Exp()
    {
        var args = new[] { Dimension.FromBaseDimensions(length: 1) };
        var result = DimensionInferenceEngine.InferFunctionDimension("exp", args);
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionDiagnostics_AddMultiple()
    {
        var diags = new DimensionDiagnostics();
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Addition, Message = "test1" });
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Subtraction, Message = "test2" });
        diags.Diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DimensionDiagnostics_HasErrors_TrueWhenAny()
    {
        var diags = new DimensionDiagnostics();
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Addition, Message = "test" });
        diags.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void DimensionDiagnostics_HasErrors_FalseWhenEmpty()
    {
        var diags = new DimensionDiagnostics();
        diags.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void DimensionDiagnostics_GetErrors_ReturnsAll()
    {
        var diags = new DimensionDiagnostics();
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Addition, Message = "err1" });
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Multiplication, Message = "err2" });
        diags.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void DimensionDiagnostics_GetWarnings_WhenNoExpectedActual()
    {
        var diags = new DimensionDiagnostics();
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Addition, Message = "warn1" });
        diags.GetWarnings().Should().HaveCount(1);
    }

    [Fact]
    public void DimensionDiagnostics_Clear_RemovesAll()
    {
        var diags = new DimensionDiagnostics();
        diags.Add(new DimensionDiagnostic { Rule = DimensionRule.Addition, Message = "test" });
        diags.Clear();
        diags.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DimensionAnalyzer_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            var expr = Expr.Add(Expr.Literal(1), Expr.Variable("x"));
            DimensionAnalyzer.Instance.AnalyzeExpression(expr).Should().Be(Dimension.None);
        });
    }

    [Fact]
    public void DimensionAnalyzer_SetGetVariableDimension()
    {
        DimensionAnalyzer.Instance.SetVariableDimension("v", Meter.Dimension);
        DimensionAnalyzer.Instance.GetVariableDimension("v").Should().Be(Meter.Dimension);
    }

    [Fact]
    public void DimensionAnalyzer_Clear_Resets()
    {
        DimensionAnalyzer.Instance.SetVariableDimension("x", Meter.Dimension);
        DimensionAnalyzer.Instance.Clear();
        DimensionAnalyzer.Instance.GetVariableDimension("x").Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionChecker_AreDimensionsCompatible_SameDimension()
    {
        var dim = Meter.Dimension;
        DimensionChecker.Instance.AreDimensionsCompatible(dim, dim, "+").Should().BeTrue();
    }

    [Fact]
    public void DimensionChecker_AreDimensionsCompatible_DifferentDimension()
    {
        DimensionChecker.Instance.AreDimensionsCompatible(Meter.Dimension, Kilogram.Dimension, "+").Should().BeFalse();
    }

    [Fact]
    public void DimensionChecker_AreDimensionsCompatible_Multiplication()
    {
        DimensionChecker.Instance.AreDimensionsCompatible(Meter.Dimension, Kilogram.Dimension, "*").Should().BeTrue();
    }

    [Fact]
    public void DimensionChecker_CheckDivision_SameDimension()
    {
        var q1 = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        DimensionChecker.Instance.CheckDivision(q1, q2).Should().BeNull();
    }

    [Fact]
    public void DimensionChecker_CheckDivision_DifferentDimension()
    {
        var q1 = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension };
        var q2 = new PhysicalQuantity { Value = 2.0, Unit = Second, Dimension = Second.Dimension };
        DimensionChecker.Instance.CheckDivision(q1, q2).Should().NotBeNull();
    }
}
