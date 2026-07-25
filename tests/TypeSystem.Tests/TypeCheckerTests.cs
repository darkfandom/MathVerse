namespace MathVerse.TypeSystem.Tests;

public class TypeCheckerTests
{
    [Fact]
    public void Check_BinaryAdd()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "+", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_BinarySubtract()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(IntegerType.Instance, "-", IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_BinaryMultiply()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "*", IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_BinaryDivide()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "/", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_BinaryPower()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "^", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_BinaryComparison_LessThan()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "<", RealType.Instance);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_BinaryEquality()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(RealType.Instance, "==", RealType.Instance);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_BinaryLogicalAnd()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(BooleanType.Instance, "&&", BooleanType.Instance);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_BinaryLogicalOr()
    {
        var checker = new TypeChecker();
        var result = checker.CheckBinary(BooleanType.Instance, "||", BooleanType.Instance);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_UnaryNegate()
    {
        var checker = new TypeChecker();
        var result = checker.CheckUnary("-", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Check_UnaryNot()
    {
        var checker = new TypeChecker();
        var result = checker.CheckUnary("!", BooleanType.Instance);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_UnaryNot_OnNonBool_Fails()
    {
        var checker = new TypeChecker();
        var result = checker.CheckUnary("!", RealType.Instance);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_UnaryIncrement_OnInteger()
    {
        var checker = new TypeChecker();
        var result = checker.CheckUnary("++", IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_UnaryIncrement_OnReal_Fails()
    {
        var checker = new TypeChecker();
        var result = checker.CheckUnary("++", RealType.Instance);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_Assignment_Compatible()
    {
        var checker = new TypeChecker();
        var result = checker.CheckAssignment(RealType.Instance, IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_Assignment_Incompatible()
    {
        var checker = new TypeChecker();
        var result = checker.CheckAssignment(IntegerType.Instance, RealType.Instance);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_MatrixMultiply_Compatible()
    {
        var checker = new TypeChecker();
        var left = new MatrixType(RealType.Instance, 2, 3);
        var right = new MatrixType(RealType.Instance, 3, 4);
        var result = checker.CheckMatrixMultiply(left, right);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_MatrixMultiply_Incompatible()
    {
        var checker = new TypeChecker();
        var left = new MatrixType(RealType.Instance, 2, 3);
        var right = new MatrixType(RealType.Instance, 2, 4);
        var result = checker.CheckMatrixMultiply(left, right);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_MatrixMultiply_ResultDimensions()
    {
        var checker = new TypeChecker();
        var left = new MatrixType(RealType.Instance, 2, 3);
        var right = new MatrixType(RealType.Instance, 3, 5);
        var result = checker.CheckMatrixMultiply(left, right);
        var mt = (MatrixType)result.Type;
        mt.Rows.Should().Be(2);
        mt.Columns.Should().Be(5);
    }

    [Fact]
    public void Check_VectorOp_Add()
    {
        var checker = new TypeChecker();
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(RealType.Instance, 3);
        var result = checker.CheckVectorOp("+", left, right);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_VectorOp_Subtract()
    {
        var checker = new TypeChecker();
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(RealType.Instance, 3);
        var result = checker.CheckVectorOp("-", left, right);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_VectorOp_DimensionMismatch()
    {
        var checker = new TypeChecker();
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(RealType.Instance, 4);
        var result = checker.CheckVectorOp("+", left, right);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_VectorOp_Equality()
    {
        var checker = new TypeChecker();
        var left = new VectorType(RealType.Instance, 3);
        var right = new VectorType(RealType.Instance, 3);
        var result = checker.CheckVectorOp("==", left, right);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Check_Lambda()
    {
        var checker = new TypeChecker();
        var result = checker.CheckLambda(
            new MathType[] { RealType.Instance }, RealType.Instance);
        result.IsSuccess.Should().BeTrue();
        result.Type.Should().BeOfType<FunctionType>();
    }

    [Fact]
    public void Check_Lambda_ErrorParam()
    {
        var checker = new TypeChecker();
        var result = checker.CheckLambda(
            new MathType[] { ErrorType.Instance }, RealType.Instance);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_Equation()
    {
        var checker = new TypeChecker();
        var result = checker.CheckEquation(RealType.Instance, "=", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
        result.Type.Should().BeOfType<EquationType>();
    }

    [Fact]
    public void Check_Equation_Inequality()
    {
        var checker = new TypeChecker();
        var result = checker.CheckEquation(RealType.Instance, "!=", IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_Equation_LessThan_Scalars()
    {
        var checker = new TypeChecker();
        var result = checker.CheckEquation(RealType.Instance, "<", RealType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_Equation_LessThan_NonScalar()
    {
        var checker = new TypeChecker();
        var lt = new VectorType(RealType.Instance, 3);
        var rt = new VectorType(RealType.Instance, 3);
        var result = checker.CheckEquation(lt, "<", rt);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Check_Derivative()
    {
        var checker = new TypeChecker();
        var result = checker.CheckDerivative(RealType.Instance, "x");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_IterationBounds()
    {
        var checker = new TypeChecker();
        var result = checker.CheckIterationBounds(IntegerType.Instance, IntegerType.Instance);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_IterationBounds_NonInteger_Warning()
    {
        var checker = new TypeChecker();
        var result = checker.CheckIterationBounds(RealType.Instance, RealType.Instance);
        result.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void Check_SetOp()
    {
        var checker = new TypeChecker();
        var left = new SetType(RealType.Instance);
        var right = new SetType(RealType.Instance);
        var result = checker.CheckSetOp("∪", left, right);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_SetOp_TypeMismatch()
    {
        var checker = new TypeChecker();
        var left = new SetType(RealType.Instance);
        var right = new SetType(IntegerType.Instance);
        var result = checker.CheckSetOp("∪", left, right);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void CheckFunctionCall_Valid()
    {
        var checker = new TypeChecker();
        var funcType = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var result = checker.CheckFunctionCall("sin", funcType, new[] { RealType.Instance });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CheckFunctionCall_WrongArity()
    {
        var checker = new TypeChecker();
        var funcType = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var result = checker.CheckFunctionCall("sin", funcType,
            new MathType[] { RealType.Instance, IntegerType.Instance });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void CheckFunctionCall_WrongArgType()
    {
        var checker = new TypeChecker();
        var funcType = new FunctionType(new[] { RealType.Instance }, RealType.Instance);
        var result = checker.CheckFunctionCall("sin", funcType, new[] { BooleanType.Instance });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void CheckFunctionCall_NotCallable()
    {
        var checker = new TypeChecker();
        var result = checker.CheckFunctionCall("x", RealType.Instance, new[] { RealType.Instance });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void TypeCheckResult_IsSuccess()
    {
        var result = new TypeCheckResult(RealType.Instance, true, Array.Empty<TypeCheckDiagnostic>());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void TypeCheckDiagnostic_Severity()
    {
        var d = new TypeCheckDiagnostic(TypeCheckSeverity.Error,
            TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.Severity.Should().Be(TypeCheckSeverity.Error);
    }

    [Fact]
    public void TypeCheckDiagnostic_ToString()
    {
        var d = new TypeCheckDiagnostic(TypeCheckSeverity.Error,
            TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.ToString().Should().Be("[Error] [IncompatibleTypes] msg");
    }

    [Fact]
    public void TypeCheckDiagnostic_Equals()
    {
        var d1 = new TypeCheckDiagnostic(TypeCheckSeverity.Error,
            TypeDiagnosticCode.IncompatibleTypes, "msg");
        var d2 = new TypeCheckDiagnostic(TypeCheckSeverity.Error,
            TypeDiagnosticCode.IncompatibleTypes, "msg");
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void TypeCheckDiagnostic_GetHashCode()
    {
        var d = new TypeCheckDiagnostic(TypeCheckSeverity.Error,
            TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.GetHashCode().Should().Be(d.GetHashCode());
    }
}
