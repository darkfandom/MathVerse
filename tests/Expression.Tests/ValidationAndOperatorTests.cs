namespace Expression.Tests;

public sealed class ExpressionValidationResultTests
{
    [Fact]
    public void Success_ShouldReturnValidResult()
    {
        var result = ExpressionValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_ShouldReturnInvalidResult()
    {
        var error = new ExpressionValidationError
        {
            Code = "ERR",
            Message = "Test error",
            NodeId = 1,
            ExpressionKind = ExpressionKind.Literal
        };

        var result = ExpressionValidationResult.Failure(error);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Failure_WithErrorList_ShouldReturnInvalidResult()
    {
        var errors = new List<ExpressionValidationError>
        {
            new() { Code = "E1", Message = "Error 1" },
            new() { Code = "E2", Message = "Error 2" }
        };

        var result = ExpressionValidationResult.Failure(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}

public sealed class ExpressionValidatorTests
{
    private readonly ExpressionValidator _validator = new();

    [Fact]
    public void Validate_ValidLiteral_ShouldReturnSuccess()
    {
        var expr = Expr.Literal(42.0);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidBinaryExpression_ShouldReturnSuccess()
    {
        var expr = Expr.Add(Expr.Literal(1), Expr.Literal(2));

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_BinaryExpressionWithNullOperand_ShouldReturnError()
    {
        var expr = new BinaryExpression(MathOperator.Add, Expr.Literal(1), Expr.Null);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXNullOperand");
    }

    [Fact]
    public void Validate_BinaryExpressionWithNullLeftOperand_ShouldReturnError()
    {
        var expr = new BinaryExpression(MathOperator.Add, Expr.Null, Expr.Literal(1));

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXNullOperand");
    }

    [Fact]
    public void Validate_UnaryExpressionWithNullOperand_ShouldReturnError()
    {
        var expr = new UnaryExpression(MathOperator.Negate, Expr.Null);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXNullOperand");
    }

    [Fact]
    public void Validate_FunctionCallWithNoArgs_ShouldReturnError()
    {
        var expr = Expr.Call("sin");

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXEmptyArgs");
    }

    [Fact]
    public void Validate_IdentityFunctionWithNoArgs_ShouldNotReturnError()
    {
        var expr = Expr.Call("identity");

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FunctionCallWithNullArg_ShouldReturnError()
    {
        var expr = Expr.Call("f", Expr.Null);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXNullArg");
    }

    [Fact]
    public void Validate_DefiniteIntegralWithNullBounds_ShouldReturnError()
    {
        var expr = new IntegralExpression(Expr.Literal(1), Expr.Variable("x"), Expr.Null, Expr.Literal(2));

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXNullBound");
    }

    [Fact]
    public void Validate_DefiniteIntegralWithBothNullBounds_ShouldReturnTwoErrors()
    {
        var expr = new IntegralExpression(Expr.Literal(1), Expr.Variable("x"), Expr.Null, Expr.Null);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Where(e => e.Code == "MV_EXNullBound").Should().HaveCount(2);
    }

    [Fact]
    public void Validate_IndefiniteIntegral_ShouldNotCheckBounds()
    {
        var expr = Expr.Integral(Expr.Literal(1), Expr.Variable("x"));

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyMatrix_ShouldReturnError()
    {
        var expr = new MatrixExpression([]);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXEmptyMatrix");
    }

    [Fact]
    public void Validate_MatrixWithInconsistentRowDimensions_ShouldReturnError()
    {
        var row1 = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var row2 = Expr.Vector(Expr.Literal(3));
        var expr = new MatrixExpression([row1, row2]);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXInconsistentDimensions");
    }

    [Fact]
    public void Validate_ValidMatrix_ShouldReturnSuccess()
    {
        var row1 = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var row2 = Expr.Vector(Expr.Literal(3), Expr.Literal(4));
        var expr = new MatrixExpression([row1, row2]);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyVector_ShouldReturnError()
    {
        var expr = new VectorExpression([]);

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "MV_EXEmptyVector");
    }

    [Fact]
    public void Validate_ValidVector_ShouldReturnSuccess()
    {
        var expr = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        var result = _validator.Validate(expr);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ErrorShouldContainNodeIdAndExpressionKind()
    {
        var expr = Expr.Call("sin");

        var result = _validator.Validate(expr);

        var error = result.Errors.Single();
        error.Code.Should().Be("MV_EXEmptyArgs");
        error.Message.Should().NotBeNullOrWhiteSpace();
        error.NodeId.Should().Be(expr.NodeId);
        error.ExpressionKind.Should().Be(ExpressionKind.FunctionCall);
    }
}

public sealed class MathOperatorTests
{
    [Fact]
    public void Add_ShouldHaveCorrectProperties()
    {
        var op = MathOperator.Add;

        op.Symbol.Should().Be("+");
        op.Name.Should().Be("Add");
        op.Category.Should().Be(OperatorCategory.Arithmetic);
        op.Arity.Should().Be(2);
        op.Precedence.Should().Be(1);
        op.Associativity.Should().Be(OperatorAssociativity.Left);
        op.IsUnary.Should().BeFalse();
        op.IsBinary.Should().BeTrue();
    }

    [Fact]
    public void Negate_ShouldBeUnary()
    {
        MathOperator.Negate.IsUnary.Should().BeTrue();
        MathOperator.Negate.IsBinary.Should().BeFalse();
        MathOperator.Negate.Arity.Should().Be(1);
    }

    [Fact]
    public void Power_ShouldBeRightAssociative()
    {
        MathOperator.Power.Associativity.Should().Be(OperatorAssociativity.Right);
        MathOperator.Power.Symbol.Should().Be("^");
    }

    [Fact]
    public void Equality_ShouldBeBasedOnSymbolAndName()
    {
        var a = new MathOperator("+", "Add", OperatorCategory.Arithmetic, 2, 1);
        var b = new MathOperator("+", "Add", OperatorCategory.Logical, 1, 0);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistentWithEquality()
    {
        var a = new MathOperator("+", "Add", OperatorCategory.Arithmetic, 2, 1);
        var b = new MathOperator("+", "Add", OperatorCategory.Logical, 1, 0);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnSymbol()
    {
        MathOperator.Add.ToString().Should().Be("+");
        MathOperator.Power.ToString().Should().Be("^");
        MathOperator.Not.ToString().Should().Be("¬");
    }

    [Fact]
    public void Assign_ShouldBeRightAssociative()
    {
        MathOperator.Assign.Associativity.Should().Be(OperatorAssociativity.Right);
    }

    [Fact]
    public void AllStaticInstances_ShouldHaveExpectedArity()
    {
        MathOperator.Add.Arity.Should().Be(2);
        MathOperator.Subtract.Arity.Should().Be(2);
        MathOperator.Multiply.Arity.Should().Be(2);
        MathOperator.Divide.Arity.Should().Be(2);
        MathOperator.Modulo.Arity.Should().Be(2);
        MathOperator.Power.Arity.Should().Be(2);
        MathOperator.Negate.Arity.Should().Be(1);
        MathOperator.Abs.Arity.Should().Be(1);
        MathOperator.Equal.Arity.Should().Be(2);
        MathOperator.NotEqual.Arity.Should().Be(2);
        MathOperator.LessThan.Arity.Should().Be(2);
        MathOperator.GreaterThan.Arity.Should().Be(2);
        MathOperator.And.Arity.Should().Be(2);
        MathOperator.Or.Arity.Should().Be(2);
        MathOperator.Not.Arity.Should().Be(1);
        MathOperator.Transpose.Arity.Should().Be(1);
    }

    [Fact]
    public void NotEqual_ShouldHaveCorrectSymbol()
    {
        MathOperator.NotEqual.Symbol.Should().Be("!=");
    }
}

public sealed class OperatorRegistryTests
{
    private readonly OperatorRegistry _registry = new();

    [Fact]
    public void GetBySymbol_Plus_ShouldReturnAdd()
    {
        var op = _registry.GetBySymbol("+");

        op.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void GetBySymbol_Unknown_ShouldReturnNull()
    {
        var op = _registry.GetBySymbol("~");

        op.Should().BeNull();
    }

    [Fact]
    public void GetByName_ShouldBeCaseInsensitive()
    {
        _registry.GetByName("add").Should().Be(MathOperator.Add);
        _registry.GetByName("ADD").Should().Be(MathOperator.Add);
        _registry.GetByName("Add").Should().Be(MathOperator.Add);
    }

    [Fact]
    public void GetByName_Unknown_ShouldReturnNull()
    {
        var op = _registry.GetByName("Foo");

        op.Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldContainAllDefaultOperators()
    {
        var all = _registry.GetAll();

        all.Should().Contain(MathOperator.Add);
        all.Should().Contain(MathOperator.Subtract);
        all.Should().Contain(MathOperator.Multiply);
        all.Should().Contain(MathOperator.Divide);
        all.Should().Contain(MathOperator.Power);
        all.Should().Contain(MathOperator.Negate);
        all.Should().Contain(MathOperator.Abs);
        all.Should().Contain(MathOperator.Equal);
        all.Should().Contain(MathOperator.NotEqual);
        all.Should().Contain(MathOperator.LessThan);
        all.Should().Contain(MathOperator.GreaterThan);
        all.Should().Contain(MathOperator.And);
        all.Should().Contain(MathOperator.Or);
        all.Should().Contain(MathOperator.Not);
        all.Should().Contain(MathOperator.Union);
        all.Should().Contain(MathOperator.Transpose);
        all.Should().Contain(MathOperator.Compose);
        all.Should().Contain(MathOperator.Assign);
    }

    [Fact]
    public void GetAll_ShouldReturnDistinctOperators()
    {
        var all = _registry.GetAll();

        all.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetByCategory_Arithmetic_ShouldReturnCorrectOperators()
    {
        var arithmetic = _registry.GetByCategory(OperatorCategory.Arithmetic);

        arithmetic.Should().Contain(MathOperator.Add);
        arithmetic.Should().Contain(MathOperator.Subtract);
        arithmetic.Should().Contain(MathOperator.Multiply);
        arithmetic.Should().Contain(MathOperator.Divide);
        arithmetic.Should().Contain(MathOperator.Modulo);
        arithmetic.Should().Contain(MathOperator.Power);
        arithmetic.Should().Contain(MathOperator.Negate);
        arithmetic.Should().Contain(MathOperator.Abs);
        arithmetic.Should().HaveCount(8);
    }

    [Fact]
    public void GetByCategory_Relational_ShouldReturnCorrectOperators()
    {
        var relational = _registry.GetByCategory(OperatorCategory.Relational);

        relational.Should().Contain(MathOperator.Equal);
        relational.Should().Contain(MathOperator.NotEqual);
        relational.Should().Contain(MathOperator.LessThan);
        relational.Should().Contain(MathOperator.GreaterThan);
        relational.Should().Contain(MathOperator.LessThanOrEqual);
        relational.Should().Contain(MathOperator.GreaterThanOrEqual);
        relational.Should().HaveCount(6);
    }

    [Fact]
    public void TryGet_ExistingSymbol_ShouldReturnTrue()
    {
        var found = _registry.TryGet("*", out var op);

        found.Should().BeTrue();
        op.Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void TryGet_UnknownSymbol_ShouldReturnFalse()
    {
        var found = _registry.TryGet("??", out var op);

        found.Should().BeFalse();
        op.Should().BeNull();
    }
}

public sealed class ExpressionKindTests
{
    [Fact]
    public void Enum_ShouldHaveAllExpectedValues()
    {
        var kinds = Enum.GetValues<ExpressionKind>();

        kinds.Should().Contain(ExpressionKind.Literal);
        kinds.Should().Contain(ExpressionKind.Variable);
        kinds.Should().Contain(ExpressionKind.Constant);
        kinds.Should().Contain(ExpressionKind.Binary);
        kinds.Should().Contain(ExpressionKind.Unary);
        kinds.Should().Contain(ExpressionKind.FunctionCall);
        kinds.Should().Contain(ExpressionKind.Lambda);
        kinds.Should().Contain(ExpressionKind.Parameter);
        kinds.Should().Contain(ExpressionKind.Equation);
        kinds.Should().Contain(ExpressionKind.Piecewise);
        kinds.Should().Contain(ExpressionKind.Conditional);
        kinds.Should().Contain(ExpressionKind.Tuple);
        kinds.Should().Contain(ExpressionKind.Vector);
        kinds.Should().Contain(ExpressionKind.Matrix);
        kinds.Should().Contain(ExpressionKind.Tensor);
        kinds.Should().Contain(ExpressionKind.Index);
        kinds.Should().Contain(ExpressionKind.Slice);
        kinds.Should().Contain(ExpressionKind.Derivative);
        kinds.Should().Contain(ExpressionKind.Integral);
        kinds.Should().Contain(ExpressionKind.Summation);
        kinds.Should().Contain(ExpressionKind.Product);
        kinds.Should().Contain(ExpressionKind.Limit);
        kinds.Should().Contain(ExpressionKind.Factorial);
        kinds.Should().Contain(ExpressionKind.Range);
        kinds.Should().Contain(ExpressionKind.Interval);
        kinds.Should().Contain(ExpressionKind.Set);
        kinds.Should().Contain(ExpressionKind.Complex);
        kinds.Should().Contain(ExpressionKind.Polynomial);
        kinds.Should().Contain(ExpressionKind.Boolean);
        kinds.Should().Contain(ExpressionKind.Relation);
        kinds.Should().Contain(ExpressionKind.Assignment);
        kinds.Should().Contain(ExpressionKind.Composition);
        kinds.Should().Contain(ExpressionKind.Identity);
        kinds.Should().Contain(ExpressionKind.Null);
    }

    [Fact]
    public void Enum_ShouldHaveExactly35Values()
    {
        var count = Enum.GetValues<ExpressionKind>().Length;

        count.Should().Be(34);
    }

    [Fact]
    public void Enum_UnderlyingType_ShouldBeInt()
    {
        typeof(ExpressionKind).GetEnumUnderlyingType()!.Should().Be(typeof(int));
    }
}
