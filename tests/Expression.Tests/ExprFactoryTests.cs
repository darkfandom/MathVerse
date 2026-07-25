namespace MathVerse.Expression.Tests;

public class ExprFactoryTests
{
    [Fact]
    public void Literal_CreatesLiteralExpressionWithValue()
    {
        var expr = Expr.Literal(42.0);

        expr.Should().BeOfType<LiteralExpression>();
        expr.Value.Should().Be(42.0);
        expr.Kind.Should().Be(ExpressionKind.Literal);
        expr.Depth.Should().Be(0);
        expr.NodeCount.Should().Be(1);
        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void Variable_CreatesVariableExpressionWithName()
    {
        var expr = Expr.Variable("x");

        expr.Should().BeOfType<VariableExpression>();
        expr.Name.Should().Be("x");
        expr.Kind.Should().Be(ExpressionKind.Variable);
    }

    [Fact]
    public void Constant_CreatesConstantExpressionWithNameAndValue()
    {
        var expr = Expr.Constant("pi", 3.14159);

        expr.Should().BeOfType<ConstantExpression>();
        expr.Name.Should().Be("pi");
        expr.Value.Should().Be(3.14159);
        expr.Kind.Should().Be(ExpressionKind.Constant);
    }

    [Fact]
    public void Boolean_CreatesBooleanExpressionWithValue()
    {
        var t = Expr.Boolean(true);
        var f = Expr.Boolean(false);

        t.Should().BeOfType<BooleanExpression>();
        t.Value.Should().BeTrue();
        f.Should().BeOfType<BooleanExpression>();
        f.Value.Should().BeFalse();
    }

    [Fact]
    public void Parameter_CreatesParameterExpressionWithName()
    {
        var expr = Expr.Parameter("p");

        expr.Should().BeOfType<ParameterExpression>();
        expr.Name.Should().Be("p");
        expr.Kind.Should().Be(ExpressionKind.Parameter);
    }

    [Fact]
    public void Identity_CreatesIdentityExpressionWithOperation()
    {
        var expr = Expr.Identity("add");

        expr.Should().BeOfType<IdentityExpression>();
        expr.Operation.Should().Be("add");
        expr.Kind.Should().Be(ExpressionKind.Identity);
    }

    [Fact]
    public void Null_ReturnsSingletonInstance()
    {
        var first = Expr.Null;
        var second = Expr.Null;

        first.Should().BeOfType<NullExpression>();
        first.Kind.Should().Be(ExpressionKind.Null);
        second.Should().BeSameAs(first);
        ReferenceEquals(first, second).Should().BeTrue();
    }

    [Fact]
    public void Arithmetic_Operators_CorrectTypes()
    {
        var a = Expr.Literal(1);
        var b = Expr.Literal(2);

        Expr.Add(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Add);
        Expr.Subtract(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Subtract);
        Expr.Multiply(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Multiply);
        Expr.Divide(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Divide);
        Expr.Modulo(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Modulo);
        Expr.Pow(a, b).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Power);
    }

    [Fact]
    public void Add_PreservesOperands()
    {
        var left = Expr.Variable("a");
        var right = Expr.Literal(3);
        var add = Expr.Add(left, right);

        add.Left.Should().BeSameAs(left);
        add.Right.Should().BeSameAs(right);
        add.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Negate_And_Abs_CorrectOperator()
    {
        var x = Expr.Variable("x");

        Expr.Negate(x).Should().BeOfType<UnaryExpression>().Which.Operator.Should().Be(MathOperator.Negate);
        Expr.Abs(x).Should().BeOfType<UnaryExpression>().Which.Operator.Should().Be(MathOperator.Abs);
    }

    [Fact]
    public void Negate_PreservesOperand()
    {
        var x = Expr.Variable("x");
        var neg = Expr.Negate(x);

        neg.Operand.Should().BeSameAs(x);
    }

    [Fact]
    public void Relational_Operators_CorrectTypes()
    {
        var a = Expr.Literal(0);
        var b = Expr.Literal(1);

        Expr.Equal(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.Equal);
        Expr.NotEqual(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.NotEqual);
        Expr.LessThan(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.LessThan);
        Expr.GreaterThan(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.GreaterThan);
        Expr.LessThanOrEqual(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.LessThanOrEqual);
        Expr.GreaterThanOrEqual(a, b).Should().BeOfType<RelationExpression>().Which.Operator.Should().Be(MathOperator.GreaterThanOrEqual);
    }

    [Fact]
    public void Logical_Operators_CorrectTypes()
    {
        var t = Expr.Boolean(true);
        var f = Expr.Boolean(false);

        Expr.And(t, f).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.And);
        Expr.Or(t, f).Should().BeOfType<BinaryExpression>().Which.Operator.Should().Be(MathOperator.Or);
        Expr.Not(t).Should().BeOfType<UnaryExpression>().Which.Operator.Should().Be(MathOperator.Not);
    }

    [Fact]
    public void Function_NamedCall_CorrectNameAndArgs()
    {
        var arg = Expr.Literal(5);
        var call = Expr.Call("custom", arg, Expr.Variable("y"));

        call.Should().BeOfType<FunctionCallExpression>();
        call.Name.Should().Be("custom");
        call.Arguments.Should().HaveCount(2);
        call.Arguments[0].Should().BeSameAs(arg);
    }

    [Fact]
    public void Sin_Cos_Tan_CreatedCorrectly()
    {
        var x = Expr.Variable("x");

        Expr.Sin(x).Name.Should().Be("sin");
        Expr.Cos(x).Name.Should().Be("cos");
        Expr.Tan(x).Name.Should().Be("tan");
    }

    [Fact]
    public void TransitiveFunctions_CorrectNames()
    {
        var x = Expr.Variable("x");

        Expr.Asin(x).Name.Should().Be("asin");
        Expr.Acos(x).Name.Should().Be("acos");
        Expr.Atan(x).Name.Should().Be("atan");
        Expr.Ln(x).Name.Should().Be("ln");
        Expr.Log10(x).Name.Should().Be("log10");
        Expr.Exp(x).Name.Should().Be("exp");
        Expr.Sqrt(x).Name.Should().Be("sqrt");
        Expr.Cbrt(x).Name.Should().Be("cbrt");
        Expr.Sinh(x).Name.Should().Be("sinh");
        Expr.Cosh(x).Name.Should().Be("cosh");
        Expr.Tanh(x).Name.Should().Be("tanh");
    }

    [Fact]
    public void Log_WithBase_CreatesTwoArgFunction()
    {
        var x = Expr.Variable("x");
        var b = Expr.Literal(2);
        var log = Expr.Log(x, b);

        log.Name.Should().Be("log");
        log.Arguments.Should().HaveCount(2);
        log.Arguments[1].Should().BeSameAs(b);
    }

    [Fact]
    public void Lambda_SingleParam_CorrectStructure()
    {
        var p = Expr.Parameter("x");
        var body = Expr.Add(p, Expr.Literal(1));
        var lambda = Expr.Lambda(p, body);

        lambda.Should().BeOfType<LambdaExpression>();
        lambda.Parameters.Should().HaveCount(1);
        lambda.Parameters[0].Should().BeSameAs(p);
        lambda.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void Lambda_MultipleParams_CorrectStructure()
    {
        var p1 = Expr.Parameter("x");
        var p2 = Expr.Parameter("y");
        var body = Expr.Add(p1, p2);
        var lambda = Expr.Lambda(new[] { p1, p2 }, body);

        lambda.Parameters.Should().HaveCount(2);
        lambda.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void Equation_CorrectLeftAndRight()
    {
        var l = Expr.Literal(0);
        var r = Expr.Variable("x");
        var eq = Expr.Equation(l, r);

        eq.Should().BeOfType<EquationExpression>();
        eq.Left.Should().BeSameAs(l);
        eq.Right.Should().BeSameAs(r);
    }

    [Fact]
    public void Conditional_CorrectBranches()
    {
        var cond = Expr.Boolean(true);
        var then = Expr.Literal(1);
        var @else = Expr.Literal(0);
        var ifExpr = Expr.Conditional(cond, then, @else);

        ifExpr.Should().BeOfType<ConditionalExpression>();
        ifExpr.Condition.Should().BeSameAs(cond);
        ifExpr.ThenBranch.Should().BeSameAs(then);
        ifExpr.ElseBranch.Should().BeSameAs(@else);
        ifExpr.Children.Should().HaveCount(3);
    }

    [Fact]
    public void Piecewise_CasesAndDefault()
    {
        var cases = new[]
        {
            new PiecewiseCase(Expr.Literal(1), Expr.Boolean(true)),
            new PiecewiseCase(Expr.Literal(0), Expr.Boolean(false))
        };
        var pw = Expr.Piecewise(cases, Expr.Literal(-1));

        pw.Should().BeOfType<PiecewiseExpression>();
        pw.Cases.Should().HaveCount(2);
        pw.DefaultCase.Should().NotBeNull();
    }

    [Fact]
    public void Tuple_CreatesWithCorrectElements()
    {
        var t = Expr.Tuple(Expr.Literal(1), Expr.Variable("a"), Expr.Boolean(false));

        t.Should().BeOfType<TupleExpression>();
        t.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Vector_CorrectDimensionAndComponents()
    {
        var v = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        v.Should().BeOfType<VectorExpression>();
        v.Dimension.Should().Be(3);
        v.Components.Should().HaveCount(3);
    }

    [Fact]
    public void Matrix_FromRows_CorrectDimensions()
    {
        var row1 = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var row2 = Expr.Vector(Expr.Literal(3), Expr.Literal(4));
        var m = Expr.Matrix(row1, row2);

        m.Should().BeOfType<MatrixExpression>();
        m.RowCount.Should().Be(2);
        m.ColumnCount.Should().Be(2);
    }

    [Fact]
    public void Matrix_From2DArray_CorrectValues()
    {
        var data = new double[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 }
        };
        var m = Expr.Matrix(data);

        m.RowCount.Should().Be(2);
        m.ColumnCount.Should().Be(3);
        m.Rows[0].Should().BeOfType<VectorExpression>();
        m.Rows[0].As<VectorExpression>().Components[1].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(2);
        m.Rows[1].As<VectorExpression>().Components[2].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(6);
    }

    [Fact]
    public void Tensor_CorrectShapeAndComponents()
    {
        var t = Expr.Tensor(new[] { 2, 3 }, Expr.Literal(1), Expr.Literal(2), Expr.Literal(3), Expr.Literal(4), Expr.Literal(5), Expr.Literal(6));

        t.Should().BeOfType<TensorExpression>();
        t.Rank.Should().Be(2);
        t.Shape.Should().Equal(2, 3);
        t.Components.Should().HaveCount(6);
    }

    [Fact]
    public void Index_CorrectTargetAndIndices()
    {
        var target = Expr.Variable("A");
        var idx = Expr.Index(target, Expr.Literal(0), Expr.Literal(1));

        idx.Should().BeOfType<IndexExpression>();
        idx.Target.Should().BeSameAs(target);
        idx.Indices.Should().HaveCount(2);
    }

    [Fact]
    public void Slice_CorrectTargetAndSlices()
    {
        var target = Expr.Variable("A");
        var s = Expr.Slice(target, null, Expr.Literal(0));

        s.Should().BeOfType<SliceExpression>();
        s.Target.Should().BeSameAs(target);
        s.Slices.Should().HaveCount(2);
    }

    [Fact]
    public void Transpose_CreatesUnaryWithTransposeOperator()
    {
        var m = Expr.Variable("A");
        var t = Expr.Transpose(m);

        t.Should().BeOfType<UnaryExpression>();
        t.Operator.Should().Be(MathOperator.Transpose);
        t.Operand.Should().BeSameAs(m);
    }

    [Fact]
    public void Derivative_FirstOrder_CorrectStructure()
    {
        var fn = Expr.Variable("f");
        var v = Expr.Variable("x");
        var d = Expr.Derivative(fn, v);

        d.Should().BeOfType<DerivativeExpression>();
        d.Function.Should().BeSameAs(fn);
        d.Variable.Should().BeSameAs(v);
        d.Order.Should().Be(1);
    }

    [Fact]
    public void Derivative_HigherOrder_CorrectOrder()
    {
        var d = Expr.Derivative(Expr.Variable("f"), Expr.Variable("x"), 3);

        d.Order.Should().Be(3);
    }

    [Fact]
    public void Integral_Indefinite_HasNoBounds()
    {
        var integrand = Expr.Variable("f");
        var v = Expr.Variable("x");
        var integral = Expr.Integral(integrand, v);

        integral.Should().BeOfType<IntegralExpression>();
        integral.Integrand.Should().BeSameAs(integrand);
        integral.Variable.Should().BeSameAs(v);
        integral.IsDefinite.Should().BeFalse();
        integral.LowerBound.Should().BeNull();
        integral.UpperBound.Should().BeNull();
    }

    [Fact]
    public void Integral_Definite_HasBounds()
    {
        var integral = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), Expr.Literal(1));

        integral.IsDefinite.Should().BeTrue();
        integral.LowerBound.Should().NotBeNull();
        integral.UpperBound.Should().NotBeNull();
    }

    [Fact]
    public void Summation_CorrectBoundsAndBody()
    {
        var i = Expr.Parameter("i");
        var body = Expr.Pow(i, Expr.Literal(2));
        var sum = Expr.Summation(i, Expr.Literal(1), Expr.Literal(10), body);

        sum.Should().BeOfType<SummationExpression>();
        sum.Variable.Should().BeSameAs(i);
        sum.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void Product_CorrectBoundsAndBody()
    {
        var i = Expr.Parameter("i");
        var body = Expr.Variable("i");
        var prod = Expr.Product(i, Expr.Literal(1), Expr.Literal(5), body);

        prod.Should().BeOfType<ProductExpression>();
        prod.Variable.Should().BeSameAs(i);
    }

    [Fact]
    public void Limit_CorrectStructure()
    {
        var body = Expr.Divide(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"));
        var v = Expr.Variable("x");
        var target = Expr.Literal(0);
        var lim = Expr.Limit(body, v, target, LimitDirection.Both);

        lim.Should().BeOfType<LimitExpression>();
        lim.Body.Should().BeSameAs(body);
        lim.Direction.Should().Be(LimitDirection.Both);
    }

    [Fact]
    public void Factorial_CorrectOperand()
    {
        var n = Expr.Variable("n");
        var fact = Expr.Factorial(n);

        fact.Should().BeOfType<FactorialExpression>();
        fact.Operand.Should().BeSameAs(n);
    }

    [Fact]
    public void Range_WithAndWithoutStep()
    {
        var r1 = Expr.Range(Expr.Literal(1), Expr.Literal(10));
        r1.Should().BeOfType<RangeExpression>();
        r1.Step.Should().BeNull();

        var r2 = Expr.Range(Expr.Literal(0), Expr.Literal(1), Expr.Literal(0.1));
        r2.Step.Should().NotBeNull();
    }

    [Fact]
    public void Interval_CorrectBoundsAndClosedness()
    {
        var i1 = Expr.Interval(Expr.Literal(0), Expr.Literal(1));
        i1.LowerClosed.Should().BeTrue();
        i1.UpperClosed.Should().BeTrue();

        var i2 = Expr.Interval(Expr.Literal(0), Expr.Literal(1), lowerClosed: false, upperClosed: false);
        i2.LowerClosed.Should().BeFalse();
        i2.UpperClosed.Should().BeFalse();
    }

    [Fact]
    public void Set_CorrectElements()
    {
        var s = Expr.Set(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        s.Should().BeOfType<SetExpression>();
        s.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Complex_CorrectRealAndImaginary()
    {
        var c = Expr.Complex(Expr.Literal(3), Expr.Literal(4));

        c.Should().BeOfType<ComplexExpression>();
        c.Real.As<LiteralExpression>().Value.Should().Be(3);
        c.Imaginary.As<LiteralExpression>().Value.Should().Be(4);
    }

    [Fact]
    public void Polynomial_CorrectVariableAndCoefficients()
    {
        var x = Expr.Variable("x");
        var p = Expr.Polynomial(x, Expr.Literal(1), Expr.Literal(0), Expr.Literal(3));

        p.Should().BeOfType<PolynomialExpression>();
        p.Variable.Should().BeSameAs(x);
        p.Coefficients.Should().HaveCount(3);
        p.Degree.Should().Be(2);
    }

    [Fact]
    public void Assign_CorrectTargetAndValue()
    {
        var target = Expr.Variable("x");
        var value = Expr.Literal(42);
        var a = Expr.Assign(target, value);

        a.Should().BeOfType<AssignmentExpression>();
        a.Target.Should().BeSameAs(target);
        a.Value.Should().BeSameAs(value);
    }

    [Fact]
    public void Compose_CorrectFunctionOrder()
    {
        var f = Expr.Variable("f");
        var g = Expr.Variable("g");
        var comp = Expr.Compose(f, g);

        comp.Should().BeOfType<CompositionExpression>();
        comp.Functions.Should().HaveCount(2);
        comp.Functions[0].Should().BeSameAs(f);
        comp.Functions[1].Should().BeSameAs(g);
    }

    [Fact]
    public void Square_EquivalentToPow2()
    {
        var x = Expr.Variable("x");
        var square = Expr.Square(x);
        var manual = Expr.Pow(x, Expr.Literal(2));

        square.Should().Be(manual);
    }

    [Fact]
    public void Cube_EquivalentToPow3()
    {
        var x = Expr.Variable("x");
        var cube = Expr.Cube(x);
        var manual = Expr.Pow(x, Expr.Literal(3));

        cube.Should().Be(manual);
    }

    [Fact]
    public void Times_EquivalentToMultiply()
    {
        var a = Expr.Variable("a");
        var b = Expr.Variable("b");
        var times = Expr.Times(a, b);
        var manual = Expr.Multiply(a, b);

        times.Should().Be(manual);
    }

    [Fact]
    public void Plus_EquivalentToAdd()
    {
        var a = Expr.Variable("a");
        var b = Expr.Variable("b");
        var plus = Expr.Plus(a, b);
        var manual = Expr.Add(a, b);

        plus.Should().Be(manual);
    }

    [Fact]
    public void Minus_EquivalentToNegate()
    {
        var x = Expr.Variable("x");
        var minus = Expr.Minus(x);
        var manual = Expr.Negate(x);

        minus.Should().Be(manual);
    }

    [Fact]
    public void ComplexComposition_SinOfXSquaredPlusOne()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Sin(Expr.Add(Expr.Square(x), Expr.Literal(1)));

        expr.Should().BeOfType<FunctionCallExpression>();
        expr.As<FunctionCallExpression>().Name.Should().Be("sin");

        var arg = expr.As<FunctionCallExpression>().Arguments[0];
        arg.Should().BeOfType<BinaryExpression>();
        arg.As<BinaryExpression>().Operator.Should().Be(MathOperator.Add);
    }

    [Fact]
    public void NestedExpression_DerivativeOfSinTimesX()
    {
        var x = Expr.Variable("x");
        var fn = Expr.Multiply(Expr.Sin(x), x);
        var d = Expr.Derivative(fn, x);

        d.Function.Should().BeOfType<BinaryExpression>();
        d.Function.As<BinaryExpression>().Left.Should().BeOfType<FunctionCallExpression>();
    }

    [Fact]
    public void TreeDepth_IncreasesWithNesting()
    {
        var x = Expr.Variable("x");
        var shallow = Expr.Add(x, Expr.Literal(1));
        var deep = Expr.Add(Expr.Multiply(x, Expr.Literal(2)), Expr.Literal(1));

        shallow.Depth.Should().Be(1);
        deep.Depth.Should().Be(2);
    }

    [Fact]
    public void NodeCount_AggregatesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(1));

        expr.NodeCount.Should().Be(3);
    }
}
