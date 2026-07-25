namespace Math.Parsing.Tests;

public class ConversionTests
{
    private static Expression Convert(string source) =>
        ParsingFacade.ParseExpression(source);

    private static Expression ConvertWithAssignMode(string source) =>
        ParsingFacade.ParseExpression(source, new ParserOptions
        {
            AllowEquations = false,
            AllowAssignments = true
        });

    // ─────────────────────────────────────────────────────────
    //  Literal Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void IntegerLiteral_42_ConvertsTo_LiteralExpressionWithValue42()
    {
        var expr = Convert("42");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(42.0);
    }

    [Fact]
    public void RealLiteral_3Point14_ConvertsTo_LiteralExpressionWithValue3Point14()
    {
        var expr = Convert("3.14");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(3.14);
    }

    [Fact]
    public void BooleanTrue_ConvertsTo_BooleanExpressionWithTrue()
    {
        var expr = Convert("true");
        expr.Should().BeOfType<BooleanExpression>();
        ((BooleanExpression)expr).Value.Should().BeTrue();
    }

    [Fact]
    public void BooleanFalse_ConvertsTo_BooleanExpressionWithFalse()
    {
        var expr = Convert("false");
        expr.Should().BeOfType<BooleanExpression>();
        ((BooleanExpression)expr).Value.Should().BeFalse();
    }

    [Fact]
    public void Pi_Constant_ConvertsTo_ConstantExpression()
    {
        var expr = Convert("pi");
        expr.Should().BeOfType<ConstantExpression>();
        ((ConstantExpression)expr).Name.Should().Be("pi");
    }

    [Fact]
    public void EulerConstant_ConvertsTo_ConstantExpression()
    {
        var expr = Convert("e");
        expr.Should().BeOfType<ConstantExpression>();
        ((ConstantExpression)expr).Name.Should().Be("e");
    }

    [Fact]
    public void ImaginaryUnit_ConvertsTo_ConstantExpression()
    {
        var expr = Convert("i");
        expr.Should().BeOfType<ConstantExpression>();
        ((ConstantExpression)expr).Name.Should().Be("i");
    }

    [Fact]
    public void Zero_ConvertsTo_LiteralExpressionWithValue0()
    {
        var expr = Convert("0");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(0.0);
    }

    [Fact]
    public void LargeInteger_ConvertsTo_LiteralExpression()
    {
        var expr = Convert("999999");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(999999.0);
    }

    [Fact]
    public void NegativeInteger_ConvertsTo_UnaryExpressionWithNegate()
    {
        var expr = Convert("-5");
        expr.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)expr).Operator.Should().Be(MathOperator.Negate);
    }

    [Fact]
    public void RealLiteral_0Point5_ConvertsTo_LiteralExpression()
    {
        var expr = Convert("0.5");
        expr.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)expr).Value.Should().Be(0.5);
    }

    // ─────────────────────────────────────────────────────────
    //  Identifier Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Identifier_x_ConvertsTo_VariableExpressionWithNameX()
    {
        var expr = Convert("x");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("x");
    }

    [Fact]
    public void Identifier_alpha_ConvertsTo_VariableExpressionWithNameAlpha()
    {
        var expr = Convert("alpha");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("alpha");
    }

    [Fact]
    public void Identifier_xyz_ConvertsTo_VariableExpressionWithNameXyz()
    {
        var expr = Convert("xyz");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("xyz");
    }

    [Fact]
    public void Identifier_underscore_ConvertsTo_VariableExpression()
    {
        var expr = Convert("_a");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("_a");
    }

    [Fact]
    public void Identifier_abc123_ConvertsTo_VariableExpression()
    {
        var expr = Convert("abc123");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("abc123");
    }

    // ─────────────────────────────────────────────────────────
    //  Binary Expression Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Addition_ConvertsTo_BinaryExpressionWithAdd()
    {
        var expr = Convert("1 + 2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void Subtraction_ConvertsTo_BinaryExpressionWithSubtract()
    {
        var expr = Convert("3 - 4");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Subtract);
    }

    [Fact]
    public void Multiplication_ConvertsTo_BinaryExpressionWithMultiply()
    {
        var expr = Convert("2 * 5");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void Division_ConvertsTo_BinaryExpressionWithDivide()
    {
        var expr = Convert("10 / 2");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Divide);
    }

    [Fact]
    public void Modulo_ConvertsTo_BinaryExpressionWithModulo()
    {
        var expr = Convert("7 % 3");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Modulo);
    }

    [Fact]
    public void Power_ConvertsTo_BinaryExpressionWithPower()
    {
        var expr = Convert("2 ^ 3");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Power);
    }

    [Fact]
    public void Addition_LeftOperandIsCorrect()
    {
        var expr = Convert("1 + 2");
        var bin = (BinaryExpression)expr;
        bin.Left.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)bin.Left).Value.Should().Be(1.0);
    }

    [Fact]
    public void Addition_RightOperandIsCorrect()
    {
        var expr = Convert("1 + 2");
        var bin = (BinaryExpression)expr;
        bin.Right.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)bin.Right).Value.Should().Be(2.0);
    }

    [Fact]
    public void Power_HasCorrectOperands()
    {
        var expr = Convert("2 ^ 3");
        var bin = (BinaryExpression)expr;
        ((LiteralExpression)bin.Left).Value.Should().Be(2.0);
        ((LiteralExpression)bin.Right).Value.Should().Be(3.0);
    }

    [Fact]
    public void ChainedAddition_ConvertsTo_NestedBinaryExpression()
    {
        var expr = Convert("1 + 2 + 3");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        bin.Left.Should().BeOfType<BinaryExpression>();
        bin.Right.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void OperatorPrecedence_MultiplyBeforeAdd()
    {
        var expr = Convert("1 + 2 * 3");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
        bin.Right.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)bin.Right).Operator.Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void OperatorPrecedence_PowerBeforeMultiply()
    {
        var expr = Convert("2 * 3 ^ 4");
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Multiply);
        bin.Right.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)bin.Right).Operator.Should().Be(MathOperator.Power);
    }

    // ─────────────────────────────────────────────────────────
    //  Unary Expression Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void UnaryMinus_ConvertsTo_UnaryExpressionWithNegate()
    {
        var expr = Convert("-x");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Negate);
    }

    [Fact]
    public void UnaryMinus_OperandIsVariable()
    {
        var expr = Convert("-x");
        var unary = (UnaryExpression)expr;
        unary.Operand.Should().BeOfType<VariableExpression>();
        ((VariableExpression)unary.Operand).Name.Should().Be("x");
    }

    [Fact]
    public void LogicalNot_ConvertsTo_UnaryExpressionWithNot()
    {
        var expr = Convert("!x");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Not);
    }

    [Fact]
    public void DoubleNegate_ConvertsTo_NestedUnaryNegate()
    {
        var expr = Convert("--x");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Negate);
        unary.Operand.Should().BeOfType<UnaryExpression>();
    }

    [Fact]
    public void NegateLiteral_ConvertsTo_UnaryNegate()
    {
        var expr = Convert("-42");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Negate);
        unary.Operand.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)unary.Operand).Value.Should().Be(42.0);
    }

    [Fact]
    public void PositivePlus_IsStrippedAway()
    {
        var expr = Convert("+x");
        expr.Should().BeOfType<VariableExpression>();
        ((VariableExpression)expr).Name.Should().Be("x");
    }

    // ─────────────────────────────────────────────────────────
    //  Postfix Expression Conversion (Factorial, Transpose)
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void PostfixFactorial_ConvertsTo_FactorialExpression()
    {
        var expr = Convert("n!");
        expr.Should().BeOfType<FactorialExpression>();
    }

    [Fact]
    public void PostfixFactorial_OperandIsCorrect()
    {
        var expr = Convert("n!");
        var fact = (FactorialExpression)expr;
        fact.Operand.Should().BeOfType<VariableExpression>();
        ((VariableExpression)fact.Operand).Name.Should().Be("n");
    }

    [Fact]
    public void PostfixTranspose_ConvertsTo_UnaryTranspose()
    {
        var expr = Convert("A\u1D40");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Transpose);
    }

    [Fact]
    public void PostfixInverse_ConvertsTo_UnaryInverse()
    {
        var expr = Convert("A\u207B\u00B9");
        expr.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)expr;
        unary.Operator.Should().Be(MathOperator.Inverse);
    }

    [Fact]
    public void FactorialOfExpression_ConvertsCorrectly()
    {
        var expr = Convert("(1 + 2)!");
        expr.Should().BeOfType<FactorialExpression>();
        var fact = (FactorialExpression)expr;
        fact.Operand.Should().BeOfType<BinaryExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Function Call Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SinFunction_ConvertsTo_FunctionCallWithNameSin()
    {
        var expr = Convert("sin(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("sin");
    }

    [Fact]
    public void CosFunction_ConvertsTo_FunctionCallWithNameCos()
    {
        var expr = Convert("cos(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("cos");
    }

    [Fact]
    public void SqrtFunction_ConvertsTo_FunctionCallWithNameSqrt()
    {
        var expr = Convert("sqrt(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("sqrt");
    }

    [Fact]
    public void LogWithTwoArgs_ConvertsTo_FunctionCallWithTwoArguments()
    {
        var expr = Convert("log(x, 10)");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("log");
        func.Arguments.Count.Should().Be(2);
    }

    [Fact]
    public void UnknownFunction_ConvertsTo_FunctionCallWithName()
    {
        var expr = Convert("f(x)");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("f");
        func.Arguments.Count.Should().Be(1);
    }

    [Fact]
    public void SinFunction_ArgumentIsCorrect()
    {
        var expr = Convert("sin(x)");
        var func = (FunctionCallExpression)expr;
        func.Arguments[0].Should().BeOfType<VariableExpression>();
        ((VariableExpression)func.Arguments[0]).Name.Should().Be("x");
    }

    [Fact]
    public void TanFunction_ConvertsTo_FunctionCallWithNameTan()
    {
        var expr = Convert("tan(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("tan");
    }

    [Fact]
    public void ExpFunction_ConvertsTo_FunctionCallWithNameExp()
    {
        var expr = Convert("exp(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("exp");
    }

    [Fact]
    public void LnFunction_ConvertsTo_FunctionCallWithNameLn()
    {
        var expr = Convert("ln(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("ln");
    }

    [Fact]
    public void AbsFunction_ConvertsTo_FunctionCallWithNameAbs()
    {
        var expr = Convert("abs(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("abs");
    }

    [Fact]
    public void Log10Function_ConvertsTo_FunctionCallWithNameLog10()
    {
        var expr = Convert("log10(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("log10");
    }

    [Fact]
    public void CbrtFunction_ConvertsTo_FunctionCallWithNameCbrt()
    {
        var expr = Convert("cbrt(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("cbrt");
    }

    [Fact]
    public void SinhFunction_ConvertsTo_FunctionCallWithNameSinh()
    {
        var expr = Convert("sinh(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("sinh");
    }

    [Fact]
    public void CoshFunction_ConvertsTo_FunctionCallWithNameCosh()
    {
        var expr = Convert("cosh(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("cosh");
    }

    [Fact]
    public void TanhFunction_ConvertsTo_FunctionCallWithNameTanh()
    {
        var expr = Convert("tanh(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("tanh");
    }

    [Fact]
    public void AsinFunction_ConvertsTo_FunctionCallWithNameAsin()
    {
        var expr = Convert("asin(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("asin");
    }

    [Fact]
    public void AcosFunction_ConvertsTo_FunctionCallWithNameAcos()
    {
        var expr = Convert("acos(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("acos");
    }

    [Fact]
    public void AtanFunction_ConvertsTo_FunctionCallWithNameAtan()
    {
        var expr = Convert("atan(x)");
        expr.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)expr).Name.Should().Be("atan");
    }

    [Fact]
    public void MinFunction_ConvertsTo_FunctionCallWithNameMin()
    {
        var expr = Convert("min(1, 2)");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("min");
        func.Arguments.Count.Should().Be(2);
    }

    [Fact]
    public void MaxFunction_ConvertsTo_FunctionCallWithNameMax()
    {
        var expr = Convert("max(1, 2)");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("max");
        func.Arguments.Count.Should().Be(2);
    }

    // ─────────────────────────────────────────────────────────
    //  Equation Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Equation_xEquals5_ConvertsTo_EquationExpression()
    {
        var expr = Convert("x = 5");
        expr.Should().BeOfType<EquationExpression>();
    }

    [Fact]
    public void Equation_xPlus1EqualsY_ConvertsTo_EquationExpression()
    {
        var expr = Convert("x + 1 = y");
        expr.Should().BeOfType<EquationExpression>();
    }

    [Fact]
    public void Equation_LeftSideIsCorrect()
    {
        var expr = Convert("x = 5");
        var eq = (EquationExpression)expr;
        eq.Left.Should().BeOfType<VariableExpression>();
        ((VariableExpression)eq.Left).Name.Should().Be("x");
    }

    [Fact]
    public void Equation_RightSideIsCorrect()
    {
        var expr = Convert("x = 5");
        var eq = (EquationExpression)expr;
        eq.Right.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)eq.Right).Value.Should().Be(5.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Conditional Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Conditional_IfThenElse_ConvertsTo_ConditionalExpression()
    {
        var expr = Convert("if x > 0 then x else -x");
        expr.Should().BeOfType<ConditionalExpression>();
    }

    [Fact]
    public void Conditional_ConditionIsRelation()
    {
        var expr = Convert("if x > 0 then x else -x");
        var cond = (ConditionalExpression)expr;
        cond.Condition.Should().BeOfType<RelationExpression>();
    }

    [Fact]
    public void Conditional_ThenBranchIsCorrect()
    {
        var expr = Convert("if x > 0 then x else -x");
        var cond = (ConditionalExpression)expr;
        cond.ThenBranch.Should().BeOfType<VariableExpression>();
        ((VariableExpression)cond.ThenBranch).Name.Should().Be("x");
    }

    [Fact]
    public void Conditional_ElseBranchIsCorrect()
    {
        var expr = Convert("if x > 0 then x else -x");
        var cond = (ConditionalExpression)expr;
        cond.ElseBranch.Should().BeOfType<UnaryExpression>();
        ((UnaryExpression)cond.ElseBranch).Operator.Should().Be(MathOperator.Negate);
    }

    // ─────────────────────────────────────────────────────────
    //  Lambda Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Lambda_SingleParam_ConvertsTo_LambdaExpression()
    {
        var expr = Convert("fn(x) => x + 1");
        expr.Should().BeOfType<LambdaExpression>();
    }

    [Fact]
    public void Lambda_SingleParam_ParametersCountIsOne()
    {
        var expr = Convert("fn(x) => x + 1");
        var lam = (LambdaExpression)expr;
        lam.Parameters.Count.Should().Be(1);
        lam.Parameters[0].Name.Should().Be("x");
    }

    [Fact]
    public void Lambda_TwoParams_ParametersCountIsTwo()
    {
        var expr = Convert("fn(x, y) => x + y");
        var lam = (LambdaExpression)expr;
        lam.Parameters.Count.Should().Be(2);
        lam.Parameters[0].Name.Should().Be("x");
        lam.Parameters[1].Name.Should().Be("y");
    }

    [Fact]
    public void Lambda_BodyIsBinaryExpression()
    {
        var expr = Convert("fn(x) => x + 1");
        var lam = (LambdaExpression)expr;
        lam.Body.Should().BeOfType<BinaryExpression>();
        ((BinaryExpression)lam.Body).Operator.Should().Be(MathOperator.Add);
    }

    // ─────────────────────────────────────────────────────────
    //  Vector Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Vector_ThreeElements_ConvertsTo_VectorExpression()
    {
        var expr = Convert("[1, 2, 3]");
        expr.Should().BeOfType<VectorExpression>();
    }

    [Fact]
    public void Vector_ThreeElements_DimensionIsThree()
    {
        var expr = Convert("[1, 2, 3]");
        var vec = (VectorExpression)expr;
        vec.Dimension.Should().Be(3);
    }

    [Fact]
    public void Vector_ComponentsAreCorrect()
    {
        var expr = Convert("[1, 2, 3]");
        var vec = (VectorExpression)expr;
        vec.Components[0].Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)vec.Components[0]).Value.Should().Be(1.0);
        ((LiteralExpression)vec.Components[1]).Value.Should().Be(2.0);
        ((LiteralExpression)vec.Components[2]).Value.Should().Be(3.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Set Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Set_ThreeElements_ConvertsTo_SetExpression()
    {
        var expr = Convert("{1, 2, 3}");
        expr.Should().BeOfType<SetExpression>();
    }

    [Fact]
    public void Set_ThreeElements_ElementCountIsThree()
    {
        var expr = Convert("{1, 2, 3}");
        var set = (SetExpression)expr;
        set.Elements.Count.Should().Be(3);
    }

    [Fact]
    public void Set_ElementsAreCorrectValues()
    {
        var expr = Convert("{1, 2, 3}");
        var set = (SetExpression)expr;
        ((LiteralExpression)set.Elements[0]).Value.Should().Be(1.0);
        ((LiteralExpression)set.Elements[1]).Value.Should().Be(2.0);
        ((LiteralExpression)set.Elements[2]).Value.Should().Be(3.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Tuple Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Tuple_ThreeElements_ConvertsTo_TupleExpression()
    {
        var expr = Convert("(1, 2, 3)");
        expr.Should().BeOfType<TupleExpression>();
    }

    [Fact]
    public void Tuple_ThreeElements_ElementCountIsThree()
    {
        var expr = Convert("(1, 2, 3)");
        var tuple = (TupleExpression)expr;
        tuple.Elements.Count.Should().Be(3);
    }

    [Fact]
    public void Tuple_ElementsAreCorrectValues()
    {
        var expr = Convert("(1, 2, 3)");
        var tuple = (TupleExpression)expr;
        ((LiteralExpression)tuple.Elements[0]).Value.Should().Be(1.0);
        ((LiteralExpression)tuple.Elements[1]).Value.Should().Be(2.0);
        ((LiteralExpression)tuple.Elements[2]).Value.Should().Be(3.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Assignment Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Assignment_xEquals1_ConvertsTo_AssignmentExpression()
    {
        var expr = ConvertWithAssignMode("x = 1");
        expr.Should().BeOfType<AssignmentExpression>();
    }

    [Fact]
    public void Assignment_TargetIsVariable()
    {
        var expr = ConvertWithAssignMode("x = 1");
        var assign = (AssignmentExpression)expr;
        assign.Target.Should().BeOfType<VariableExpression>();
        ((VariableExpression)assign.Target).Name.Should().Be("x");
    }

    [Fact]
    public void Assignment_ValueIsLiteral()
    {
        var expr = ConvertWithAssignMode("x = 1");
        var assign = (AssignmentExpression)expr;
        assign.Value.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)assign.Value).Value.Should().Be(1.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Summation Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Summation_ConvertsTo_SummationExpression()
    {
        var expr = Convert("\u2211 i=1,10 i");
        expr.Should().BeOfType<SummationExpression>();
    }

    [Fact]
    public void Summation_VariableIsCorrect()
    {
        var expr = Convert("\u2211 i=1,10 i");
        var sum = (SummationExpression)expr;
        sum.Variable.Should().BeOfType<VariableExpression>();
        ((VariableExpression)sum.Variable).Name.Should().Be("i");
    }

    [Fact]
    public void Summation_LowerBoundIsCorrect()
    {
        var expr = Convert("\u2211 i=1,10 i");
        var sum = (SummationExpression)expr;
        sum.LowerBound.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)sum.LowerBound).Value.Should().Be(1.0);
    }

    [Fact]
    public void Summation_UpperBoundIsCorrect()
    {
        var expr = Convert("\u2211 i=1,10 i");
        var sum = (SummationExpression)expr;
        sum.UpperBound.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)sum.UpperBound).Value.Should().Be(10.0);
    }

    [Fact]
    public void Summation_BodyIsCorrect()
    {
        var expr = Convert("\u2211 i=1,10 i");
        var sum = (SummationExpression)expr;
        sum.Body.Should().BeOfType<VariableExpression>();
        ((VariableExpression)sum.Body).Name.Should().Be("i");
    }

    // ─────────────────────────────────────────────────────────
    //  Derivative Conversion (via direct converter)
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Derivative_ConvertsTo_DerivativeExpression()
    {
        var expr = Convert("\u2202x sin(x)");
        expr.Should().BeOfType<DerivativeExpression>();
    }

    [Fact]
    public void Derivative_VariableIsCorrect()
    {
        var expr = Convert("\u2202x sin(x)");
        var deriv = (DerivativeExpression)expr;
        deriv.Variable.Should().BeOfType<VariableExpression>();
        ((VariableExpression)deriv.Variable).Name.Should().Be("x");
    }

    [Fact]
    public void Derivative_FunctionIsSin()
    {
        var expr = Convert("\u2202x sin(x)");
        var deriv = (DerivativeExpression)expr;
        deriv.Function.Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)deriv.Function).Name.Should().Be("sin");
    }

    // ─────────────────────────────────────────────────────────
    //  Limit Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Limit_ConvertsTo_LimitExpression()
    {
        var expr = Convert("lim sin(x)/x \u2192 x 0");
        expr.Should().BeOfType<LimitExpression>();
    }

    [Fact]
    public void Limit_BodyIsCorrect()
    {
        var expr = Convert("lim sin(x)/x \u2192 x 0");
        var lim = (LimitExpression)expr;
        lim.Body.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Limit_VariableIsCorrect()
    {
        var expr = Convert("lim sin(x)/x \u2192 x 0");
        var lim = (LimitExpression)expr;
        lim.Variable.Should().BeOfType<VariableExpression>();
        ((VariableExpression)lim.Variable).Name.Should().Be("x");
    }

    [Fact]
    public void Limit_TargetIsCorrect()
    {
        var expr = Convert("lim sin(x)/x \u2192 x 0");
        var lim = (LimitExpression)expr;
        lim.Target.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)lim.Target).Value.Should().Be(0.0);
    }

    // ─────────────────────────────────────────────────────────
    //  Product Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Product_ConvertsTo_ProductExpression()
    {
        var expr = Convert("\u220F i=1,5 i");
        expr.Should().BeOfType<ProductExpression>();
    }

    [Fact]
    public void Product_VariableIsCorrect()
    {
        var expr = Convert("\u220F i=1,5 i");
        var prod = (ProductExpression)expr;
        prod.Variable.Should().BeOfType<VariableExpression>();
        ((VariableExpression)prod.Variable).Name.Should().Be("i");
    }

    // ─────────────────────────────────────────────────────────
    //  Operator Mapping Verification
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OperatorMapping_PlusToken_MapsToAdd()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.PlusToken)
            .Should().Be(MathOperator.Add);
    }

    [Fact]
    public void OperatorMapping_MinusToken_MapsToSubtract()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.MinusToken)
            .Should().Be(MathOperator.Subtract);
    }

    [Fact]
    public void OperatorMapping_StarToken_MapsToMultiply()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.StarToken)
            .Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void OperatorMapping_SlashToken_MapsToDivide()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.SlashToken)
            .Should().Be(MathOperator.Divide);
    }

    [Fact]
    public void OperatorMapping_PercentToken_MapsToModulo()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.PercentToken)
            .Should().Be(MathOperator.Modulo);
    }

    [Fact]
    public void OperatorMapping_CaretToken_MapsToPower()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.CaretToken)
            .Should().Be(MathOperator.Power);
    }

    [Fact]
    public void OperatorMapping_EqualsEqualsToken_MapsToEqual()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.EqualsEqualsToken)
            .Should().Be(MathOperator.Equal);
    }

    [Fact]
    public void OperatorMapping_NotEqualsToken_MapsToNotEqual()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.NotEqualsToken)
            .Should().Be(MathOperator.NotEqual);
    }

    [Fact]
    public void OperatorMapping_LessThanToken_MapsToLessThan()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.LessThanToken)
            .Should().Be(MathOperator.LessThan);
    }

    [Fact]
    public void OperatorMapping_GreaterThanToken_MapsToGreaterThan()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.GreaterThanToken)
            .Should().Be(MathOperator.GreaterThan);
    }

    [Fact]
    public void OperatorMapping_LessThanOrEqualToken_MapsToLessThanOrEqual()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.LessThanOrEqualToken)
            .Should().Be(MathOperator.LessThanOrEqual);
    }

    [Fact]
    public void OperatorMapping_GreaterThanOrEqualToken_MapsToGreaterThanOrEqual()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.GreaterThanOrEqualToken)
            .Should().Be(MathOperator.GreaterThanOrEqual);
    }

    [Fact]
    public void OperatorMapping_UnionToken_MapsToUnion()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.UnionToken)
            .Should().Be(MathOperator.Union);
    }

    [Fact]
    public void OperatorMapping_IntersectionToken_MapsToIntersection()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.IntersectionToken)
            .Should().Be(MathOperator.Intersection);
    }

    [Fact]
    public void OperatorMapping_DotProductToken_MapsToDot()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.DotProductToken)
            .Should().Be(MathOperator.Dot);
    }

    [Fact]
    public void OperatorMapping_CrossProductToken_MapsToCross()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.CrossProductToken)
            .Should().Be(MathOperator.Cross);
    }

    [Fact]
    public void OperatorMapping_ComposeToken_MapsToCompose()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.ComposeToken)
            .Should().Be(MathOperator.Compose);
    }

    [Fact]
    public void OperatorMapping_WedgeToken_MapsToAnd()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.WedgeToken)
            .Should().Be(MathOperator.And);
    }

    [Fact]
    public void OperatorMapping_VeeToken_MapsToOr()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.VeeToken)
            .Should().Be(MathOperator.Or);
    }

    [Fact]
    public void OperatorMapping_ElementOfToken_MapsToElementOf()
    {
        SyntaxToExpressionConverter.MapSyntaxKindToOperator(SyntaxKind.ElementOfToken)
            .Should().Be(MathOperator.ElementOf);
    }

    // ─────────────────────────────────────────────────────────
    //  TokenType Mapping Verification
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void TokenTypeMapping_Plus_MapsToAdd()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Plus)
            .Should().Be(MathOperator.Add);
    }

    [Fact]
    public void TokenTypeMapping_Minus_MapsToSubtract()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Minus)
            .Should().Be(MathOperator.Subtract);
    }

    [Fact]
    public void TokenTypeMapping_Star_MapsToMultiply()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Star)
            .Should().Be(MathOperator.Multiply);
    }

    [Fact]
    public void TokenTypeMapping_Slash_MapsToDivide()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Slash)
            .Should().Be(MathOperator.Divide);
    }

    [Fact]
    public void TokenTypeMapping_Caret_MapsToPower()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Caret)
            .Should().Be(MathOperator.Power);
    }

    [Fact]
    public void TokenTypeMapping_Percent_MapsToModulo()
    {
        SyntaxToExpressionConverter.MapTokenTypeToOperator(TokenType.Percent)
            .Should().Be(MathOperator.Modulo);
    }

    // ─────────────────────────────────────────────────────────
    //  Relation Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void EqualityExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x == 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.Equal);
    }

    [Fact]
    public void NotEqualExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x != 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.NotEqual);
    }

    [Fact]
    public void LessThanExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x < 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.LessThan);
    }

    [Fact]
    public void GreaterThanExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x > 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.GreaterThan);
    }

    [Fact]
    public void LessThanOrEqualExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x <= 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.LessThanOrEqual);
    }

    [Fact]
    public void GreaterThanOrEqualExpression_ConvertsTo_RelationExpression()
    {
        var expr = Convert("x >= 5");
        expr.Should().BeOfType<RelationExpression>();
        var rel = (RelationExpression)expr;
        rel.Operator.Should().Be(MathOperator.GreaterThanOrEqual);
    }

    // ─────────────────────────────────────────────────────────
    //  Parenthesized Conversion
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ParenthesizedExpression_ConvertsTo_InnerExpression()
    {
        var expr = Convert("(1 + 2)");
        expr.Should().BeOfType<BinaryExpression>();
        var bin = (BinaryExpression)expr;
        bin.Operator.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void NestedParentheses_ConvertCorrectly()
    {
        var expr = Convert("((1 + 2))");
        expr.Should().BeOfType<BinaryExpression>();
    }

    // ─────────────────────────────────────────────────────────
    //  Additional Conversion Coverage
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void FunctionCallWithLiteralArg_ConvertsCorrectly()
    {
        var expr = Convert("sin(0)");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("sin");
        func.Arguments.Count.Should().Be(1);
        func.Arguments[0].Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void NestedFunctionCall_ConvertsCorrectly()
    {
        var expr = Convert("sin(cos(x))");
        var func = (FunctionCallExpression)expr;
        func.Name.Should().Be("sin");
        func.Arguments[0].Should().BeOfType<FunctionCallExpression>();
        ((FunctionCallExpression)func.Arguments[0]).Name.Should().Be("cos");
    }

    [Fact]
    public void ExpressionKind_IsCorrect()
    {
        var expr = Convert("1 + 2");
        expr.Kind.Should().Be(ExpressionKind.Binary);
    }

    [Fact]
    public void LiteralExpressionKind_IsCorrect()
    {
        var expr = Convert("42");
        expr.Kind.Should().Be(ExpressionKind.Literal);
    }

    [Fact]
    public void VariableExpressionKind_IsCorrect()
    {
        var expr = Convert("x");
        expr.Kind.Should().Be(ExpressionKind.Variable);
    }

    [Fact]
    public void EquationExpressionKind_IsCorrect()
    {
        var expr = Convert("x = 5");
        expr.Kind.Should().Be(ExpressionKind.Equation);
    }

    [Fact]
    public void LambdaExpressionKind_IsCorrect()
    {
        var expr = Convert("fn(x) => x");
        expr.Kind.Should().Be(ExpressionKind.Lambda);
    }

    [Fact]
    public void VectorExpressionKind_IsCorrect()
    {
        var expr = Convert("[1, 2]");
        expr.Kind.Should().Be(ExpressionKind.Vector);
    }

    [Fact]
    public void SetExpressionKind_IsCorrect()
    {
        var expr = Convert("{1, 2}");
        expr.Kind.Should().Be(ExpressionKind.Set);
    }

    [Fact]
    public void TupleExpressionKind_IsCorrect()
    {
        var expr = Convert("(1, 2)");
        expr.Kind.Should().Be(ExpressionKind.Tuple);
    }

    [Fact]
    public void ConditionalExpressionKind_IsCorrect()
    {
        var expr = Convert("if x > 0 then x else 0");
        expr.Kind.Should().Be(ExpressionKind.Conditional);
    }

    [Fact]
    public void SummationExpressionKind_IsCorrect()
    {
        var expr = Convert("\u2211 i=1,5 i");
        expr.Kind.Should().Be(ExpressionKind.Summation);
    }

    [Fact]
    public void ProductExpressionKind_IsCorrect()
    {
        var expr = Convert("\u220F i=1,5 i");
        expr.Kind.Should().Be(ExpressionKind.Product);
    }

    [Fact]
    public void LimitExpressionKind_IsCorrect()
    {
        var expr = Convert("lim x \u2192 x 0");
        expr.Kind.Should().Be(ExpressionKind.Limit);
    }

    [Fact]
    public void DerivativeExpressionKind_IsCorrect()
    {
        var expr = Convert("\u2202x sin(x)");
        expr.Kind.Should().Be(ExpressionKind.Derivative);
    }

    [Fact]
    public void FactorialExpressionKind_IsCorrect()
    {
        var expr = Convert("5!");
        expr.Kind.Should().Be(ExpressionKind.Factorial);
    }
}
