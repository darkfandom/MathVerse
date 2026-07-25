namespace MathVerse.Math.Parsing.Tests;

public sealed class ParserFunctionTests
{
    private static ParserResult Parse(string source) => ParsingFacade.Parse(source);

    // ───────────────────────────────────────────────────────
    //  Built-in Trigonometric Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Sin_ReturnsFunctionCallExpression()
    {
        var result = Parse("sin(x)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<FunctionCallExpressionSyntax>();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("sin");
        fn.Arguments.Should().HaveCount(1);
        fn.Arguments[0].Should().BeOfType<IdentifierNameSyntax>();
    }

    [Fact]
    public void Parse_Cos_ReturnsFunctionCallExpression()
    {
        var result = Parse("cos(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("cos");
        fn.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_Tan_ReturnsFunctionCallExpression()
    {
        var result = Parse("tan(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("tan");
    }

    [Fact]
    public void Parse_Asin_ReturnsFunctionCallExpression()
    {
        var result = Parse("asin(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("asin");
    }

    [Fact]
    public void Parse_Acos_ReturnsFunctionCallExpression()
    {
        var result = Parse("acos(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("acos");
    }

    [Fact]
    public void Parse_Atan_ReturnsFunctionCallExpression()
    {
        var result = Parse("atan(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("atan");
    }

    // ───────────────────────────────────────────────────────
    //  Hyperbolic Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Sinh_ReturnsFunctionCallExpression()
    {
        var result = Parse("sinh(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("sinh");
    }

    [Fact]
    public void Parse_Cosh_ReturnsFunctionCallExpression()
    {
        var result = Parse("cosh(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("cosh");
    }

    [Fact]
    public void Parse_Tanh_ReturnsFunctionCallExpression()
    {
        var result = Parse("tanh(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("tanh");
    }

    // ───────────────────────────────────────────────────────
    //  Logarithmic & Exponential Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Ln_ReturnsFunctionCallExpression()
    {
        var result = Parse("ln(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("ln");
    }

    [Fact]
    public void Parse_Log_ReturnsFunctionCallExpression()
    {
        var result = Parse("log(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("log");
    }

    [Fact]
    public void Parse_Log10_ReturnsFunctionCallExpression()
    {
        var result = Parse("log10(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("log10");
    }

    [Fact]
    public void Parse_Exp_ReturnsFunctionCallExpression()
    {
        var result = Parse("exp(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("exp");
    }

    // ───────────────────────────────────────────────────────
    //  Root & Absolute Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Sqrt_ReturnsFunctionCallExpression()
    {
        var result = Parse("sqrt(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("sqrt");
    }

    [Fact]
    public void Parse_Cbrt_ReturnsFunctionCallExpression()
    {
        var result = Parse("cbrt(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("cbrt");
    }

    [Fact]
    public void Parse_Abs_ReturnsFunctionCallExpression()
    {
        var result = Parse("abs(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("abs");
    }

    [Fact]
    public void Parse_Floor_ReturnsFunctionCallExpression()
    {
        var result = Parse("floor(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("floor");
    }

    [Fact]
    public void Parse_Ceil_ReturnsFunctionCallExpression()
    {
        var result = Parse("ceil(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("ceil");
    }

    [Fact]
    public void Parse_Round_ReturnsFunctionCallExpression()
    {
        var result = Parse("round(x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("round");
    }

    // ───────────────────────────────────────────────────────
    //  Multi-Argument Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_LogTwoArgs_ReturnsFunctionCallExpression()
    {
        var result = Parse("log(x, 10)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("log");
        fn.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_Mod_ReturnsFunctionCallExpression()
    {
        var result = Parse("mod(7, 3)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("mod");
        fn.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_Min_ReturnsFunctionCallExpression()
    {
        var result = Parse("min(1, 2)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("min");
        fn.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_Max_ReturnsFunctionCallExpression()
    {
        var result = Parse("max(3, 4)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("max");
        fn.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_Det_ReturnsFunctionCallExpression()
    {
        var result = Parse("det(A)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("det");
        fn.Arguments.Should().HaveCount(1);
    }

    // ───────────────────────────────────────────────────────
    //  Empty Function Call
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyParensAfterBuiltin_ReturnsFunctionCallExpression()
    {
        var result = Parse("sin()");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("sin");
        fn.Arguments.Should().BeEmpty();
    }

    // ───────────────────────────────────────────────────────
    //  Nested Functions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_NestedFunction_sin_cos()
    {
        var result = Parse("sin(cos(x))");
        result.Success.Should().BeTrue();
        var outer = (FunctionCallExpressionSyntax)result.Root!;
        outer.FunctionName.Should().Be("sin");
        outer.Arguments.Should().HaveCount(1);
        outer.Arguments[0].Should().BeOfType<FunctionCallExpressionSyntax>();
        var inner = (FunctionCallExpressionSyntax)outer.Arguments[0];
        inner.FunctionName.Should().Be("cos");
    }

    [Fact]
    public void Parse_TripleNestedFunction()
    {
        var result = Parse("ln(exp(sqrt(x)))");
        result.Success.Should().BeTrue();
        var outer = (FunctionCallExpressionSyntax)result.Root!;
        outer.FunctionName.Should().Be("ln");
        var mid = (FunctionCallExpressionSyntax)outer.Arguments[0];
        mid.FunctionName.Should().Be("exp");
        var inner = (FunctionCallExpressionSyntax)mid.Arguments[0];
        inner.FunctionName.Should().Be("sqrt");
    }

    // ───────────────────────────────────────────────────────
    //  Function in Binary Expression
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_FunctionPlusFunction()
    {
        var result = Parse("sin(x) + cos(x)");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        bin.Left.Should().BeOfType<FunctionCallExpressionSyntax>();
        bin.Right.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    [Fact]
    public void Parse_FunctionTimesConstant()
    {
        var result = Parse("2 * sin(x)");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        ((LiteralExpressionSyntax)bin.Left).Token.Value.Should().Be(2);
        bin.Right.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    [Fact]
    public void Parse_FunctionTimesVariable()
    {
        var result = Parse("x * cos(y)");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<IdentifierNameSyntax>();
        bin.Right.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Function with Complex Arguments
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_FunctionWithAdditionArg()
    {
        var result = Parse("sin(x + 1)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Arguments.Should().HaveCount(1);
        fn.Arguments[0].Should().BeOfType<BinaryExpressionSyntax>();
        var arg = (BinaryExpressionSyntax)fn.Arguments[0];
        arg.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
    }

    [Fact]
    public void Parse_FunctionWithProductArg()
    {
        var result = Parse("sin(x * y)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Arguments[0].Should().BeOfType<BinaryExpressionSyntax>();
        var arg = (BinaryExpressionSyntax)fn.Arguments[0];
        arg.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
    }

    [Fact]
    public void Parse_FunctionWithPowerArg()
    {
        var result = Parse("sin(x ^ 2)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Arguments[0].Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_FunctionWithUnaryArg()
    {
        var result = Parse("sin(-x)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Arguments[0].Should().BeOfType<UnaryExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Postfix Operators: Factorial
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_IntegerFactorial_ReturnsPostfixExpression()
    {
        var result = Parse("5!");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<PostfixExpressionSyntax>();
        var postfix = (PostfixExpressionSyntax)result.Root!;
        postfix.OperatorToken.Kind.Should().Be(SyntaxKind.ExclamationToken);
        postfix.Operand.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_VariableFactorial_ReturnsPostfixExpression()
    {
        var result = Parse("n!");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<PostfixExpressionSyntax>();
        var postfix = (PostfixExpressionSyntax)result.Root!;
        postfix.OperatorToken.Kind.Should().Be(SyntaxKind.ExclamationToken);
        postfix.Operand.Should().BeOfType<IdentifierNameSyntax>();
    }

    [Fact]
    public void Parse_FactorialInExpression()
    {
        var result = Parse("n! + 1");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<PostfixExpressionSyntax>();
        bin.Right.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_ChainedFactorial()
    {
        var result = Parse("5!!");
        result.Success.Should().BeTrue();
        var outer = (PostfixExpressionSyntax)result.Root!;
        outer.Operand.Should().BeOfType<PostfixExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Postfix Operators: Inverse (⁻¹ works; ᵀ is caught as letter)
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Inverse_ReturnsPostfixExpression()
    {
        var result = Parse("A\u207B\u00B9");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<PostfixExpressionSyntax>();
        var postfix = (PostfixExpressionSyntax)result.Root!;
        postfix.OperatorToken.Kind.Should().Be(SyntaxKind.InverseToken);
    }

    [Fact]
    public void Parse_InverseInAddition()
    {
        var result = Parse("A\u207B\u00B9 + B");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<PostfixExpressionSyntax>();
        bin.Right.Should().BeOfType<IdentifierNameSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Vectors
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_VectorThreeElements_ReturnsVectorLiteral()
    {
        var result = Parse("[1, 2, 3]");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<VectorLiteralExpressionSyntax>();
        var vec = (VectorLiteralExpressionSyntax)result.Root!;
        vec.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_VectorSingleElement_ReturnsVectorLiteral()
    {
        var result = Parse("[1]");
        result.Success.Should().BeTrue();
        var vec = (VectorLiteralExpressionSyntax)result.Root!;
        vec.Elements.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_VectorWithExpressions()
    {
        var result = Parse("[x + 1, y * 2, z ^ 3]");
        result.Success.Should().BeTrue();
        var vec = (VectorLiteralExpressionSyntax)result.Root!;
        vec.Elements.Should().HaveCount(3);
        vec.Elements[0].Should().BeOfType<BinaryExpressionSyntax>();
        vec.Elements[1].Should().BeOfType<BinaryExpressionSyntax>();
        vec.Elements[2].Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_EmptyVector_ReturnsVectorLiteral()
    {
        var result = Parse("[]");
        result.Success.Should().BeTrue();
        var vec = (VectorLiteralExpressionSyntax)result.Root!;
        vec.Elements.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MatrixAsNestedVector_ReturnsVectorLiteral()
    {
        var result = Parse("[[1, 2], [3, 4]]");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<VectorLiteralExpressionSyntax>();
        var outer = (VectorLiteralExpressionSyntax)result.Root!;
        outer.Elements.Should().HaveCount(2);
        outer.Elements[0].Should().BeOfType<VectorLiteralExpressionSyntax>();
        outer.Elements[1].Should().BeOfType<VectorLiteralExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Sets
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SetThreeElements_ReturnsSetLiteral()
    {
        var result = Parse("{1, 2, 3}");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<SetLiteralExpressionSyntax>();
        var set = (SetLiteralExpressionSyntax)result.Root!;
        set.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_EmptySet_ReturnsSetLiteral()
    {
        var result = Parse("{}");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<SetLiteralExpressionSyntax>();
        var set = (SetLiteralExpressionSyntax)result.Root!;
        set.Elements.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SetSingleElement()
    {
        var result = Parse("{42}");
        result.Success.Should().BeTrue();
        var set = (SetLiteralExpressionSyntax)result.Root!;
        set.Elements.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_SetWithIdentifiers()
    {
        var result = Parse("{x, y, z}");
        result.Success.Should().BeTrue();
        var set = (SetLiteralExpressionSyntax)result.Root!;
        set.Elements.Should().HaveCount(3);
        set.Elements[0].Should().BeOfType<IdentifierNameSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Tuples
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_TupleThreeElements_ReturnsTupleExpression()
    {
        var result = Parse("(1, 2, 3)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<TupleExpressionSyntax>();
        var tuple = (TupleExpressionSyntax)result.Root!;
        tuple.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_TupleTwoElements_ReturnsTupleExpression()
    {
        var result = Parse("(x, y)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<TupleExpressionSyntax>();
        var tuple = (TupleExpressionSyntax)result.Root!;
        tuple.Elements.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_IsNotTuple()
    {
        var result = Parse("(1 + 2)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        result.Root.Should().NotBeOfType<TupleExpressionSyntax>();
    }

    [Fact]
    public void Parse_EmptyTuple_ReturnsTupleExpression()
    {
        var result = Parse("()");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<TupleExpressionSyntax>();
        var tuple = (TupleExpressionSyntax)result.Root!;
        tuple.Elements.Should().BeEmpty();
    }

    // ───────────────────────────────────────────────────────
    //  Lambda Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_LambdaWithUnicodeArrow_ReturnsLambdaExpression()
    {
        var result = Parse("fn(x) \u2192 x + 1");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LambdaExpressionSyntax>();
        var lambda = (LambdaExpressionSyntax)result.Root!;
        lambda.Parameters.Should().HaveCount(1);
        lambda.Parameters[0].Name.Should().Be("x");
        lambda.Body.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_LambdaMultiParam_ReturnsLambdaExpression()
    {
        var result = Parse("fn(x, y) \u2192 x + y");
        result.Success.Should().BeTrue();
        var lambda = (LambdaExpressionSyntax)result.Root!;
        lambda.Parameters.Should().HaveCount(2);
        lambda.Parameters[0].Name.Should().Be("x");
        lambda.Parameters[1].Name.Should().Be("y");
    }

    [Fact]
    public void Parse_LambdaBodyIsFunction()
    {
        var result = Parse("fn(x) \u2192 sin(x)");
        result.Success.Should().BeTrue();
        var lambda = (LambdaExpressionSyntax)result.Root!;
        lambda.Body.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    [Fact]
    public void Parse_LambdaBodyIsLiteral()
    {
        var result = Parse("fn() \u2192 42");
        result.Success.Should().BeTrue();
        var lambda = (LambdaExpressionSyntax)result.Root!;
        lambda.Parameters.Should().BeEmpty();
        lambda.Body.Should().BeOfType<LiteralExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Conditional Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_IfThenElse_ReturnsConditionalExpression()
    {
        var result = Parse("if x > 0 then x else -x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ConditionalExpressionSyntax>();
        var cond = (ConditionalExpressionSyntax)result.Root!;
        cond.Condition.Should().BeOfType<BinaryExpressionSyntax>();
        cond.ThenBranch.Should().BeOfType<IdentifierNameSyntax>();
        cond.ElseBranch.Should().BeOfType<UnaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_IfThenElseWithComparison()
    {
        var result = Parse("if x == 0 then 0 else 1");
        result.Success.Should().BeTrue();
        var cond = (ConditionalExpressionSyntax)result.Root!;
        var condition = (BinaryExpressionSyntax)cond.Condition;
        condition.OperatorToken.Kind.Should().Be(SyntaxKind.EqualsEqualsToken);
    }

    [Fact]
    public void Parse_IfThenElseWithFunctionBody()
    {
        var result = Parse("if x > 0 then sqrt(x) else 0");
        result.Success.Should().BeTrue();
        var cond = (ConditionalExpressionSyntax)result.Root!;
        cond.ThenBranch.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Piecewise Expressions (parser maps "when" keyword text → KeywordWhere)
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_PiecewiseTwoCases_ReturnsPiecewiseExpression()
    {
        var result = Parse("piecewise { x where x > 0, -x where x < 0 }");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<PiecewiseExpressionSyntax>();
        var pw = (PiecewiseExpressionSyntax)result.Root!;
        pw.Cases.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_PiecewiseSingleCase()
    {
        var result = Parse("piecewise { x where x > 0 }");
        result.Success.Should().BeTrue();
        var pw = (PiecewiseExpressionSyntax)result.Root!;
        pw.Cases.Should().HaveCount(1);
        pw.Cases[0].Value.Should().BeOfType<IdentifierNameSyntax>();
        pw.Cases[0].Condition.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_PiecewiseThreeCases()
    {
        var result = Parse("piecewise { x where x > 0, 0 where x == 0, -x where x < 0 }");
        result.Success.Should().BeTrue();
        var pw = (PiecewiseExpressionSyntax)result.Root!;
        pw.Cases.Should().HaveCount(3);
    }

    // ───────────────────────────────────────────────────────
    //  Summation & Product (use j instead of i since i is ConstantI)
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Summation_ReturnsSummationExpression()
    {
        var result = Parse("\u2211 j=1,10 j ^ 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<SummationExpressionSyntax>();
        var sum = (SummationExpressionSyntax)result.Root!;
        sum.VariableToken.Text.Should().Be("j");
        sum.LowerBound.Should().BeOfType<LiteralExpressionSyntax>();
        sum.UpperBound.Should().BeOfType<LiteralExpressionSyntax>();
        sum.Body.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_Product_ReturnsProductExpression()
    {
        var result = Parse("\u220F j=1,5 j");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ProductExpressionSyntax>();
        var prod = (ProductExpressionSyntax)result.Root!;
        prod.VariableToken.Text.Should().Be("j");
    }

    // ───────────────────────────────────────────────────────
    //  Derivative (partial derivative with Unicode)
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_PartialDerivative_ReturnsDerivativeExpression()
    {
        var result = Parse("\u2202x sin(x)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<DerivativeExpressionSyntax>();
        var deriv = (DerivativeExpressionSyntax)result.Root!;
        deriv.VariableToken.Text.Should().Be("x");
        deriv.Function.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Limit Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Limit_ReturnsLimitExpression()
    {
        var result = Parse("lim sin(x) \u2192 x 0");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LimitExpressionSyntax>();
        var lim = (LimitExpressionSyntax)result.Root!;
        lim.Body.Should().BeOfType<FunctionCallExpressionSyntax>();
        lim.VariableToken.Text.Should().Be("x");
        lim.Target.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_LimitWithProductBody()
    {
        var result = Parse("lim x * x \u2192 x 0");
        result.Success.Should().BeTrue();
        var lim = (LimitExpressionSyntax)result.Root!;
        lim.Body.Should().BeOfType<BinaryExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Function Call Properties
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_FunctionCallSyntaxKind_IsFunctionCallExpression()
    {
        var result = Parse("sin(x)");
        result.Root!.Kind.Should().Be(SyntaxKind.FunctionCallExpression);
    }

    [Fact]
    public void Parse_FunctionCall_HasCorrectChildren()
    {
        var result = Parse("sin(x)");
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Children.Should().Contain(n => n is SyntaxToken && ((SyntaxToken)n).Text == "sin");
        fn.Children.Should().Contain(n => n is SyntaxToken && ((SyntaxToken)n).Text == "(");
        fn.Children.Should().Contain(n => n is SyntaxToken && ((SyntaxToken)n).Text == ")");
    }

    [Fact]
    public void Parse_FunctionCallWithThreeArgs()
    {
        var result = Parse("max(1, max(2, 3))");
        result.Success.Should().BeTrue();
        var outer = (FunctionCallExpressionSyntax)result.Root!;
        outer.FunctionName.Should().Be("max");
        outer.Arguments.Should().HaveCount(2);
        outer.Arguments[1].Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Unicode Symbols in Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnionOperator()
    {
        var result = Parse("A \u222A B");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.UnionToken);
    }

    [Fact]
    public void Parse_IntersectionOperator()
    {
        var result = Parse("A \u2229 B");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.IntersectionToken);
    }

    [Fact]
    public void Parse_ElementOfOperator()
    {
        var result = Parse("x \u2208 S");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.ElementOfToken);
    }

    [Fact]
    public void Parse_CrossProductOperator()
    {
        var result = Parse("A \u00D7 B");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.CrossProductToken);
    }

    [Fact]
    public void Parse_DotProductOperator()
    {
        var result = Parse("A \u00B7 B");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.DotProductToken);
    }

    // ───────────────────────────────────────────────────────
    //  Function with Multiple Comma-Separated Args
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_MinWithThreeArgs()
    {
        var result = Parse("min(1, 2, 3)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.FunctionName.Should().Be("min");
        fn.Arguments.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_MaxWithExpressions()
    {
        var result = Parse("max(x + 1, y - 2)");
        result.Success.Should().BeTrue();
        var fn = (FunctionCallExpressionSyntax)result.Root!;
        fn.Arguments.Should().HaveCount(2);
        fn.Arguments[0].Should().BeOfType<BinaryExpressionSyntax>();
        fn.Arguments[1].Should().BeOfType<BinaryExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Vector Arithmetic
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_VectorPlusVector()
    {
        var result = Parse("[1, 2] + [3, 4]");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<VectorLiteralExpressionSyntax>();
        bin.Right.Should().BeOfType<VectorLiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_ScalarTimesVector()
    {
        var result = Parse("2 * [1, 2, 3]");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Right.Should().BeOfType<VectorLiteralExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Unknown Function Names (parsed as identifiers)
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnknownFunctionName_ParsesAsIdentifier()
    {
        var result = Parse("f(x)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
    }
}
