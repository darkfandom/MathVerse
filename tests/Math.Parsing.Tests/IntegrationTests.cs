namespace Math.Parsing.Tests;

public class IntegrationTests
{
    // ─────────────────────────────────────────────────────────
    //  Basic Arithmetic Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SimpleAddition_1Plus2_ConvertsToBinaryAdd()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        ((LiteralExpression)bin.Left).Value.Should().Be(1.0);
        ((LiteralExpression)bin.Right).Value.Should().Be(2.0);
    }

    [Fact]
    public void SimpleSubtraction_ConvertsToBinarySubtract()
    {
        var expr = ParsingFacade.ParseExpression("10 - 3");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Subtract);
    }

    [Fact]
    public void SimpleMultiplication_ConvertsToBinaryMultiply()
    {
        var expr = ParsingFacade.ParseExpression("4 * 5");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void SimpleDivision_ConvertsToBinaryDivide()
    {
        var expr = ParsingFacade.ParseExpression("20 / 4");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Divide);
    }

    [Fact]
    public void ModuloExpression_ConvertsToBinaryModulo()
    {
        var expr = ParsingFacade.ParseExpression("7 % 3");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Modulo);
    }

    [Fact]
    public void PowerExpression_ConvertsToBinaryPower()
    {
        var expr = ParsingFacade.ParseExpression("2 ^ 10");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Power);
        ((LiteralExpression)bin.Left).Value.Should().Be(2.0);
        ((LiteralExpression)bin.Right).Value.Should().Be(10.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Function Call Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SinPiOver2_ProducesFunctionCallWithConstantArg()
    {
        var expr = ParsingFacade.ParseExpression("sin(pi/2)");
        expr.Should().BeOfType<FunctionCallExpression>();
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("sin");
        func.Arguments.Count.Should().Be(1);
        func.Arguments[0].Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CosFunction_ProducesFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("cos(0)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("cos");
    }

    [Fact]
    public void TanFunction_ProducesFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("tan(x)");
        ((FunctionCallExpression)expr).Name.Should().Be("tan");
    }

    [Fact]
    public void SqrtFunction_ProducesFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("sqrt(4)");
        ((FunctionCallExpression)expr).Name.Should().Be("sqrt");
    }

    [Fact]
    public void ExpFunction_ProducesFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("exp(1)");
        ((FunctionCallExpression)expr).Name.Should().Be("exp");
    }

    [Fact]
    public void LnFunction_ProducesFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("ln(1)");
        ((FunctionCallExpression)expr).Name.Should().Be("ln");
    }

    [Fact]
    public void AbsFunction_ProducesUnaryAbs()
    {
        var expr = ParsingFacade.ParseExpression("abs(-5)");
        expr.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)expr).Operator.Should().Be(MathOperator.Abs);
    }

    // ─────────────────────────────────────────────────────────
    //  Complex Expression Structures
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void QuadraticExpression_xSquaredPlus2xPlus1_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("x^2 + 2*x + 1");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        bin.Left.Should().BeOfType<BinaryExpression>();
        bin.Right.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void TrigIdentity_SinSquaredPlusCosSquared_ProducesNestedTree()
    {
        var expr = ParsingFacade.ParseExpression("sin(x)^2 + cos(x)^2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        bin.Left.Should().BeOfType<BinaryExpression>();
        bin.Right.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void NestedFunctions_SinCosTan_ProducesDeepTree()
    {
        var expr = ParsingFacade.ParseExpression("sin(cos(tan(x)))");
        expr.Should().BeOfType<FunctionCallExpression>();
        var outer = (FunctionCallExpression)expr;
        outer.Name.Should().Be("sin");
        var mid = (FunctionCallExpression)outer.Arguments[0];
        mid.Name.Should().Be("cos");
        var inner = (FunctionCallExpression)mid.Arguments[0];
        inner.Name.Should().Be("tan");
    }

    [Fact]
    public void ParenthesizedExpression_PreservedInStructure()
    {
        var expr = ParsingFacade.ParseExpression("(1 + 2) * 3");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Multiply);
        bin.Left.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)bin.Left).Operator.Should().Be(MathOperator.Add);
        bin.Right.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void ComplexNestedArithmetic_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("((1 + 2) * (3 - 4)) / (5 ^ 2)");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Divide);
        bin.Left.Should().BeOfType<BinaryExpression>();
        bin.Right.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void OperatorPrecedence_MultiplicationBindsTighterThanAddition()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2 * 3");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        var rightBin = (BinaryExpression)bin.Right;
        rightBin.Operator.Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void OperatorPrecedence_PowerBindsTighterThanMultiplication()
    {
        var expr = ParsingFacade.ParseExpression("2 * 3 ^ 4");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Multiply);
        var rightBin = (BinaryExpression)bin.Right;
        rightBin.Operator.Should().Be(MathOperator.Power);
    }

    // ─────────────────────────────────────────────────────────
    //  Equation Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Equation_xSquaredEquals1()
    {
        var expr = ParsingFacade.ParseExpression("x^2 = 1");
        expr.Should().BeOfType<EquationExpression>();
        var eq = (EquationExpression)expr;
        eq.Left.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)eq.Left).Operator.Should().Be(MathOperator.Power);
        eq.Right.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void Equation_SimpleLinearEquation()
    {
        var expr = ParsingFacade.ParseExpression("2*x = 10");
        var eq = (EquationExpression)expr;
        eq.Left.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)eq.Left).Operator.Should().Be(MathOperator.Multiply);
    }

    // ─────────────────────────────────────────────────────────
    //  Conditional Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Conditional_IfThenElse_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("if x > 0 then sqrt(x) else 0");
        expr.Should().BeOfType<ConditionalExpression>();
        var cond = (ConditionalExpression)expr;
        cond.Condition.Should().BeOfType<RelationExpression>();
        cond.ThenBranch.Should().BeOfType<FunctionCallExpression>();
        cond.ElseBranch.Should().BeOfType<LiteralExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Lambda Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Lambda_SingleParam_xSquared_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("fn(x) \u2192 x^2");
        expr.Should().BeOfType<LambdaExpression>();
        var lam = (LambdaExpression)expr;
        lam.Parameters.Count.Should().Be(1);
        lam.Parameters[0].Name.Should().Be("x");
        lam.Body.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)lam.Body).Operator.Should().Be(MathOperator.Power);
    }

    [Fact]
    public void Lambda_TwoParams_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("fn(x, y) \u2192 x + y");
        var lam = (LambdaExpression)expr;
        lam.Parameters.Count.Should().Be(2);
        lam.Body.Should().BeOfType<BinaryExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Vector, Set, Tuple Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Vector_ThreeElements_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("[1, 2, 3]");
        expr.Should().BeOfType<VectorExpression>();
        var vec = (VectorExpression)expr;
        vec.Dimension.Should().Be(3);
    }

    [Fact]
    public void Vector_ExpressionElements_ProducesCorrectComponents()
    {
        var expr = ParsingFacade.ParseExpression("[1 + 1, 2 * 3, 4]");
        var vec = (VectorExpression)expr;
        vec.Dimension.Should().Be(3);
        vec.Components[0].Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Set_ThreeElements_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("{1, 2, 3}");
        expr.Should().BeOfType<SetExpression>();
        ((SetExpression)expr).Elements.Count.Should().Be(3);
    }

    [Fact]
    public void Tuple_ThreeElements_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("(1, 2, 3)");
        expr.Should().BeOfType<TupleExpression>();
        ((TupleExpression)expr).Elements.Count.Should().Be(3);
    }

    [Fact]
    public void SingleElementTuple_ParenthesizedExpression()
    {
        var expr = ParsingFacade.ParseExpression("(1 + 2)");
        expr.Should().BeOfType<BinaryExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Summation and Product Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Summation_nEquals1To10_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("\u2211 n=1,10 n");
        expr.Should().BeOfType<SummationExpression>();
        var sum = (SummationExpression)expr;
        ((VariableExpression)sum.Variable).Name.Should().Be("n");
        ((LiteralExpression)sum.LowerBound).Value.Should().Be(1.0);
        ((LiteralExpression)sum.UpperBound).Value.Should().Be(10.0);
    }

    [Fact]
    public void Product_nEquals1To5_FullPipeline()
    {
        var expr = ParsingFacade.ParseExpression("\u220F n=1,5 n");
        expr.Should().BeOfType<ProductExpression>();
        var prod = (ProductExpression)expr;
        ((VariableExpression)prod.Variable).Name.Should().Be("n");
        ((LiteralExpression)prod.LowerBound).Value.Should().Be(1.0);
        ((LiteralExpression)prod.UpperBound).Value.Should().Be(5.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Derivative Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Derivative_PartialXSinX_ProducesDerivativeExpression()
    {
        var expr = ParsingFacade.ParseExpression("\u2202x sin(x)");
        expr.Should().BeOfType<DerivativeExpression>();
        var deriv = (DerivativeExpression)expr;
        ((VariableExpression)deriv.Variable).Name.Should().Be("x");
        deriv.Function.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)deriv.Function).Name.Should().Be("sin");
    }

    [Fact]
    public void Derivative_PartialXsinX_ProducesDerivativeWithFunctionCall()
    {
        var expr = ParsingFacade.ParseExpression("\u2202x sin(x)");
        expr.Should().BeOfType<DerivativeExpression>();
        var deriv = (DerivativeExpression)expr;
        ((VariableExpression)deriv.Variable).Name.Should().Be("x");
        deriv.Function.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)deriv.Function).Arguments.Count.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────
    //  Limit Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Limit_SinXOverX_AsXApproaches0_ProducesLimitExpression()
    {
        var expr = ParsingFacade.ParseExpression("lim sin(x)/x \u2192 x 0");
        expr.Should().BeOfType<LimitExpression>();
        var lim = (LimitExpression)expr;
        lim.Body.Should().BeOfType<BinaryExpression>();
        ((VariableExpression)lim.Variable).Name.Should().Be("x");
        ((LiteralExpression)lim.Target).Value.Should().Be(0.0);
    }

    [Fact]
    public void Limit_SimpleVariable_AsTargetApproaches1()
    {
        var expr = ParsingFacade.ParseExpression("lim x \u2192 x 1");
        var lim = (LimitExpression)expr;
        lim.Body.Should().BeOfType<VariableExpression>();
        ((LiteralExpression)lim.Target).Value.Should().Be(1.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Factorial and Postfix Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Factorial_n_ProducesFactorialExpression()
    {
        var expr = ParsingFacade.ParseExpression("n!");
        expr.Should().BeOfType<FactorialExpression>();
        var fact = (FactorialExpression)expr;
        fact.Operand.Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void Factorial_NumericLiteral_ProducesFactorialExpression()
    {
        var expr = ParsingFacade.ParseExpression("5!");
        expr.Should().BeOfType<FactorialExpression>();
        var fact = (FactorialExpression)expr;
        ((LiteralExpression)fact.Operand).Value.Should().Be(5.0);
    }

    [Fact]
    public void Transpose_AT_ProducesPostfixTranspose()
    {
        var tree = ParsingFacade.ParseSyntaxTree("A");
        tree.Root.Should().BeOfType<IdentifierNameSyntax>();
        var unary = new PostfixExpressionSyntax(
            new IdentifierNameSyntax(new SyntaxToken(SyntaxKind.IdentifierToken, 0, "A", null)),
            new SyntaxToken(SyntaxKind.TransposeToken, 1, "\u1D40", null));
        var converter = new SyntaxToExpressionConverter();
        var expr = converter.Convert(unary);
        expr.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)expr).Operator.Should().Be(MathOperator.Transpose);
    }

    // ─────────────────────────────────────────────────────────
    //  Unary Expression Pipeline
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Negation_x_ProducesUnaryNegate()
    {
        var expr = ParsingFacade.ParseExpression("-x");
        expr.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)expr).Operator.Should().Be(MathOperator.Negate);
    }

    [Fact]
    public void LogicalNot_x_ProducesUnaryNot()
    {
        var expr = ParsingFacade.ParseExpression("!x");
        expr.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)expr).Operator.Should().Be(MathOperator.Not);
    }

    // ─────────────────────────────────────────────────────────
    //  Lexer Tokenize Verification
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Tokenize_SimpleExpression_ReturnsCorrectTokenCount()
    {
        var tokens = ParsingFacade.Tokenize("1 + 2");
        tokens.Length.Should().BeGreaterThan(3);
    }

    [Fact]
    public void Tokenize_FunctionCall_ReturnsFunctionTokens()
    {
        var tokens = ParsingFacade.Tokenize("sin(x)");
        tokens.Should().Contain(t => t.Kind == TokenType.FuncSin);
    }

    [Fact]
    public void Tokenize_EmptyString_ContainsOnlyEof()
    {
        var tokens = ParsingFacade.Tokenize("");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void Tokenize_MultipleTokens_ProducesCorrectSequence()
    {
        var tokens = ParsingFacade.Tokenize("2 * 3");
        tokens.Should().Contain(t => t.Kind == TokenType.IntegerLiteral);
        tokens.Should().Contain(t => t.Kind == TokenType.Star);
    }

    // ─────────────────────────────────────────────────────────
    //  SyntaxTree Verification
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ParseSyntaxTree_ReturnsNonEmptyTree()
    {
        var tree = ParsingFacade.ParseSyntaxTree("1 + 2");
        tree.Should().NotBeNull();
        tree.Root.Should().NotBeNull();
    }

    [Fact]
    public void ParseSyntaxTree_RootIsBinaryExpression()
    {
        var tree = ParsingFacade.ParseSyntaxTree("1 + 2");
        tree.Root.Kind.Should().Be(SyntaxKind.BinaryExpression);
    }

    [Fact]
    public void ParseSyntaxTree_NoErrorsForValidInput()
    {
        var tree = ParsingFacade.ParseSyntaxTree("1 + 2");
        tree.HasErrors.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    //  ParserResult Verification
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ParseResult_SuccessIsTrueForValidInput()
    {
        var result = ParsingFacade.Parse("1 + 2");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ParseResult_RootIsNonNullForValidInput()
    {
        var result = ParsingFacade.Parse("1 + 2");
        result.Root.Should().NotBeNull();
    }

    [Fact]
    public void ParseExpression_ReturnsNonNullForValidInput()
    {
        var expr = ParsingFacade.ParseExpression("1");
        expr.Should().NotBeNull();
    }

    // ─────────────────────────────────────────────────────────
    //  Roundtrip Tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_SingleLiteral_ProducesCorrectKind()
    {
        var expr = ParsingFacade.ParseExpression("42");
        expr.Kind.Should().Be(ExpressionKind.Literal);
    }

    [Fact]
    public void Roundtrip_Addition_ProducesBinaryKind()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.Kind.Should().Be(ExpressionKind.Binary);
    }

    [Fact]
    public void Roundtrip_Equation_ProducesEquationKind()
    {
        var expr = ParsingFacade.ParseExpression("x = 5");
        expr.Kind.Should().Be(ExpressionKind.Equation);
    }

    [Fact]
    public void Roundtrip_Lambda_ProducesLambdaKind()
    {
        var expr = ParsingFacade.ParseExpression("fn(x) \u2192 x");
        expr.Kind.Should().Be(ExpressionKind.Lambda);
    }

    [Fact]
    public void Roundtrip_Conditional_ProducesConditionalKind()
    {
        var expr = ParsingFacade.ParseExpression("if x > 0 then x else 0");
        expr.Kind.Should().Be(ExpressionKind.Conditional);
    }

    [Fact]
    public void Roundtrip_Vector_ProducesVectorKind()
    {
        var expr = ParsingFacade.ParseExpression("[1, 2]");
        expr.Kind.Should().Be(ExpressionKind.Vector);
    }

    [Fact]
    public void Roundtrip_Set_ProducesSetKind()
    {
        var expr = ParsingFacade.ParseExpression("{1, 2}");
        expr.Kind.Should().Be(ExpressionKind.Set);
    }

    [Fact]
    public void Roundtrip_Tuple_ProducesTupleKind()
    {
        var expr = ParsingFacade.ParseExpression("(1, 2)");
        expr.Kind.Should().Be(ExpressionKind.Tuple);
    }

    [Fact]
    public void Roundtrip_Summation_ProducesSummationKind()
    {
        var expr = ParsingFacade.ParseExpression("\u2211 n=1,5 n");
        expr.Kind.Should().Be(ExpressionKind.Summation);
    }

    [Fact]
    public void Roundtrip_Product_ProducesProductKind()
    {
        var expr = ParsingFacade.ParseExpression("\u220F n=1,5 n");
        expr.Kind.Should().Be(ExpressionKind.Product);
    }

    [Fact]
    public void Roundtrip_Derivative_ProducesDerivativeKind()
    {
        var expr = ParsingFacade.ParseExpression("\u2202x sin(x)");
        expr.Kind.Should().Be(ExpressionKind.Derivative);
    }

    [Fact]
    public void Roundtrip_Limit_ProducesLimitKind()
    {
        var expr = ParsingFacade.ParseExpression("lim x \u2192 x 0");
        expr.Kind.Should().Be(ExpressionKind.Limit);
    }

    [Fact]
    public void Roundtrip_Factorial_ProducesFactorialKind()
    {
        var expr = ParsingFacade.ParseExpression("5!");
        expr.Kind.Should().Be(ExpressionKind.Factorial);
    }

    [Fact]
    public void Roundtrip_UnknownFunction_ProducesVariableKind()
    {
        var expr = ParsingFacade.ParseExpression("f");
        expr.Kind.Should().Be(ExpressionKind.Variable);
    }

    [Fact]
    public void Roundtrip_Transpose_ProducesUnaryKind()
    {
        var unary = new PostfixExpressionSyntax(
            new IdentifierNameSyntax(new SyntaxToken(SyntaxKind.IdentifierToken, 0, "A", null)),
            new SyntaxToken(SyntaxKind.TransposeToken, 1, "\u1D40", null));
        var converter = new SyntaxToExpressionConverter();
        var expr = converter.Convert(unary);
        expr.Kind.Should().Be(ExpressionKind.Unary);
    }

    // ─────────────────────────────────────────────────────────
    //  Multiple Sequential Expressions
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void MultipleExpressions_DifferentSources_ProduceDifferentKinds()
    {
        var e1 = ParsingFacade.ParseExpression("1");
        var e2 = ParsingFacade.ParseExpression("x");
        var e3 = ParsingFacade.ParseExpression("1 + 2");
        e1.Kind.Should().Be(ExpressionKind.Literal);
        e2.Kind.Should().Be(ExpressionKind.Variable);
        e3.Kind.Should().Be(ExpressionKind.Binary);
    }

    // ─────────────────────────────────────────────────────────
    //  Large and Complex Expressions
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LargeExpression_ManyOperators_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2 + 3 + 4 + 5");
        expr.Should().BeOfType<BinaryExpression>();
        expr.Kind.Should().Be(ExpressionKind.Binary);
    }

    [Fact]
    public void DeeplyNestedFunctions_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("sin(cos(tan(exp(ln(x)))))");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("sin");
    }

    [Fact]
    public void MixedArithmeticAndFunctions_ProducesCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("2 * sin(x) + cos(x)^2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void MinMaxFunctions_ProduceCorrectTree()
    {
        var expr = ParsingFacade.ParseExpression("min(1, max(2, 3))");
        expr.Should().BeOfType<FunctionCallExpression>();
        var outer = (FunctionCallExpression)expr;
        outer.Name.Should().Be("min");
        outer.Arguments.Count.Should().Be(2);
        outer.Arguments[1].Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)outer.Arguments[1]).Name.Should().Be("max");
    }

    // ─────────────────────────────────────────────────────────
    //  Unicode and Greek Letters
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Unicode_GreekLetters_AlphaPlusBeta()
    {
        var expr = ParsingFacade.ParseExpression("\u03B1 + \u03B2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Left.Should().BeOfType<VariableExpression>();
        bin.Right.Should().BeOfType<VariableExpression>();
        ((VariableExpression)bin.Left).Name.Should().Be("\u03B1");
        ((VariableExpression)bin.Right).Name.Should().Be("\u03B2");
    }

    // ─────────────────────────────────────────────────────────
    //  Error Handling
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void InvalidExpression_ThrowsOnParseExpression()
    {
        Action act = () => ParsingFacade.ParseExpression("1 +");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ParseResult_ForInvalidInput_HasErrors()
    {
        var result = ParsingFacade.Parse("1 +");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    //  ConvertToExpression from SyntaxTree
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ConvertToExpression_FromSyntaxTree_ProducesCorrectExpression()
    {
        var tree = ParsingFacade.ParseSyntaxTree("1 + 2");
        var expr = ParsingFacade.ConvertToExpression(tree);
        expr.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)expr).Operator.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void ConvertToExpression_FromSyntaxTree_FunctionCall()
    {
        var tree = ParsingFacade.ParseSyntaxTree("sin(x)");
        var expr = ParsingFacade.ConvertToExpression(tree);
        expr.Should().BeOfType<FunctionCallExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Assignment via ParserOptions
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void AssignmentMode_xEquals1_ProducesAssignmentExpression()
    {
        var opts = new ParserOptions { AllowEquations = false, AllowAssignments = true };
        var expr = ParsingFacade.ParseExpression("x = 1", opts);
        expr.Should().BeOfType<AssignmentExpression>();
        var assign = (AssignmentExpression)expr;
        ((VariableExpression)assign.Target).Name.Should().Be("x");
        ((LiteralExpression)assign.Value).Value.Should().Be(1.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Edge Cases
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SingleNumber_ProducesLiteralExpression()
    {
        var expr = ParsingFacade.ParseExpression("1");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(1.0);
    }

    [Fact]
    public void SingleIdentifier_ProducesVariableExpression()
    {
        var expr = ParsingFacade.ParseExpression("x");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("x");
    }

    [Fact]
    public void NegativeNumber_ProducesUnaryNegate()
    {
        var expr = ParsingFacade.ParseExpression("-1");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Negate);
        unary.Operand.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void ChainedPower_IsLeftAssociative()
    {
        var expr = ParsingFacade.ParseExpression("2 ^ 3 ^ 4");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Power);
        bin.Left.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)bin.Left).Operator.Should().Be(MathOperator.Power);
    }

    [Fact]
    public void EmptyParentheses_ProducesTupleExpression()
    {
        var expr = ParsingFacade.ParseExpression("()");
        expr.Should().BeOfType<TupleExpression>();
        ((TupleExpression)expr).Elements.Count.Should().Be(0);
    }

    [Fact]
    public void EmptyBrackets_ProducesVectorExpression()
    {
        var expr = ParsingFacade.ParseExpression("[]");
        expr.Should().BeOfType<VectorExpression>();
        ((VectorExpression)expr).Dimension.Should().Be(0);
    }

    [Fact]
    public void EmptyBraces_ProducesSetExpression()
    {
        var expr = ParsingFacade.ParseExpression("{}");
        expr.Should().BeOfType<SetExpression>();
        ((SetExpression)expr).Elements.Count.Should().Be(0);
    }

    [Fact]
    public void ExpressionDepth_SingleLiteral_IsZero()
    {
        var expr = ParsingFacade.ParseExpression("1");
        expr.Depth.Should().Be(0);
    }

    [Fact]
    public void ExpressionNodeCount_SingleLiteral_IsOne()
    {
        var expr = ParsingFacade.ParseExpression("1");
        expr.NodeCount.Should().Be(1);
    }

    [Fact]
    public void ExpressionNodeCount_BinaryExpression_IsThree()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void ExpressionToString_ReturnsNonEmptyString()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NodeCount_DeepExpression_IsCorrect()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2 + 3");
        expr.NodeCount.Should().Be(5);
    }

    [Fact]
    public void Depth_BinaryExpression_IsOne()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.Depth.Should().Be(1);
    }

    [Fact]
    public void Children_BinaryExpression_HasTwoChildren()
    {
        var expr = ParsingFacade.ParseExpression("1 + 2");
        expr.Children.Count.Should().Be(2);
    }

    [Fact]
    public void Children_LiteralExpression_HasNoChildren()
    {
        var expr = ParsingFacade.ParseExpression("1");
        expr.Children.Count.Should().Be(0);
    }
}
