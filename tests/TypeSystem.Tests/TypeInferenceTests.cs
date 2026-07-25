namespace MathVerse.TypeSystem.Tests;

public class TypeInferenceTests
{
    [Fact]
    public void Infer_LiteralInteger()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Literal(42.0);
        var result = engine.Infer(expr);
        result.Type.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Infer_LiteralReal()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Literal(3.14);
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_LiteralZero_IsInteger()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Literal(0.0);
        var result = engine.Infer(expr);
        result.Type.Kind.Should().Be(TypeKind.Integer);
    }

    [Fact]
    public void Infer_BinaryAdd_SameType()
    {
        var engine = new TypeInferenceEngine();
        var left = Expr.Literal(1.0);
        var right = Expr.Literal(2.0);
        var expr = Expr.Binary(left, "+", right);
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinarySubtract()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(5.0), "-", Expr.Literal(3.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinaryMultiply()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(4.0), "*", Expr.Literal(5.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinaryDivide()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(10.0), "/", Expr.Literal(2.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinaryPower()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(2.0), "^", Expr.Literal(10.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinaryModulo()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(10.0), "%", Expr.Literal(3.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BinaryComparison_ReturnsBoolean()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(1.0), "<", Expr.Literal(2.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_BinaryEquality_ReturnsBoolean()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(1.0), "==", Expr.Literal(1.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_BinaryGreaterThan()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(5.0), ">", Expr.Literal(3.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_BinaryLessEqual()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Binary(Expr.Literal(2.0), "<=", Expr.Literal(2.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_BinaryLogicalAnd()
    {
        var engine = new TypeInferenceEngine();
        var left = Expr.Binary(Expr.Literal(1.0), "<", Expr.Literal(2.0));
        var right = Expr.Binary(Expr.Literal(3.0), ">", Expr.Literal(1.0));
        var expr = Expr.Binary(left, "&&", right);
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_BinaryLogicalOr()
    {
        var engine = new TypeInferenceEngine();
        var left = Expr.Binary(Expr.Literal(1.0), "==", Expr.Literal(1.0));
        var right = Expr.Binary(Expr.Literal(2.0), "==", Expr.Literal(3.0));
        var expr = Expr.Binary(left, "||", right);
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_UnaryNegate()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Unary("-", Expr.Literal(5.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_UnaryNot()
    {
        var engine = new TypeInferenceEngine();
        var inner = Expr.Binary(Expr.Literal(1.0), "<", Expr.Literal(2.0));
        var expr = Expr.Unary("!", inner);
        var result = engine.Infer(expr);
        result.Type.Should().Be(BooleanType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Sin()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("sin", Expr.Literal(0.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Cos()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("cos", Expr.Literal(0.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Sqrt()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("sqrt", Expr.Literal(144.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Abs()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("abs", Expr.Literal(-5.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Exp()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("exp", Expr.Literal(1.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Ln()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("ln", Expr.Literal(1.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Tan()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("tan", Expr.Literal(0.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Floor()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("floor", Expr.Literal(3.7));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Ceil()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("ceil", Expr.Literal(3.2));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Round()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("round", Expr.Literal(3.5));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Pow_TwoArgs()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("pow", Expr.Literal(2.0), Expr.Literal(3.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Atan2()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("atan2", Expr.Literal(1.0), Expr.Literal(1.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Min()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("min", Expr.Literal(1.0), Expr.Literal(2.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Max()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Call("max", Expr.Literal(1.0), Expr.Literal(2.0));
        var result = engine.Infer(expr);
        result.Type.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Dot()
    {
        var engine = new TypeInferenceEngine();
        var v1 = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var v2 = Expr.Vector(Expr.Literal(3.0), Expr.Literal(4.0));
        var expr = Expr.Call("dot", v1, v2);
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_FunctionCall_Norm()
    {
        var engine = new TypeInferenceEngine();
        var v = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var expr = Expr.Call("norm", v);
        var result = engine.Infer(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_Assignment()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Assign("x", Expr.Literal(42.0));
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_Conditional()
    {
        var engine = new TypeInferenceEngine();
        var cond = Expr.Binary(Expr.Literal(1.0), "<", Expr.Literal(2.0));
        var then = Expr.Literal(1.0);
        var else_ = Expr.Literal(2.0);
        var expr = Expr.Conditional(cond, then, else_);
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_Lambda()
    {
        var engine = new TypeInferenceEngine();
        var param = new MathVerse.Math.Expressions.ParameterExpression("x");
        var body = Expr.Binary(param, "+", Expr.Literal(1.0));
        var expr = Expr.Lambda(new[] { param }, body);
        var result = engine.Infer(expr);
        result.Type.Should().BeOfType<FunctionType>();
    }

    [Fact]
    public void Infer_Vector()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0), Expr.Literal(3.0));
        var result = engine.Infer(expr);
        result.Type.Should().BeOfType<VectorType>();
    }

    [Fact]
    public void Infer_VectorDimension()
    {
        var engine = new TypeInferenceEngine();
        var expr = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var result = engine.Infer(expr);
        var vt = (VectorType)result.Type;
        vt.Dimension.Should().Be(2);
    }

    [Fact]
    public void Infer_Matrix()
    {
        var engine = new TypeInferenceEngine();
        var row1 = new[] { Expr.Literal(1.0), Expr.Literal(2.0) };
        var row2 = new[] { Expr.Literal(3.0), Expr.Literal(4.0) };
        var expr = Expr.Matrix(new[] { row1, row2 });
        var result = engine.Infer(expr);
        result.Type.Should().BeOfType<MatrixType>();
    }

    [Fact]
    public void Infer_MatrixDimensions()
    {
        var engine = new TypeInferenceEngine();
        var row1 = new[] { Expr.Literal(1.0), Expr.Literal(2.0), Expr.Literal(3.0) };
        var row2 = new[] { Expr.Literal(4.0), Expr.Literal(5.0), Expr.Literal(6.0) };
        var expr = Expr.Matrix(new[] { row1, row2 });
        var result = engine.Infer(expr);
        var mt = (MatrixType)result.Type;
        mt.Rows.Should().Be(2);
        mt.Columns.Should().Be(3);
    }

    [Fact]
    public void Infer_BoundLiteral()
    {
        var engine = new TypeInferenceEngine();
        var expr = new BoundLiteralExpression(42.0);
        var result = engine.InferBound(expr);
        result.Type.Kind.Should().Be(TypeKind.Integer);
    }

    [Fact]
    public void Infer_BoundVariable()
    {
        var engine = new TypeInferenceEngine();
        var expr = new BoundVariableExpression(new VariableSymbol("x"));
        var result = engine.InferBound(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BoundBinary()
    {
        var engine = new TypeInferenceEngine();
        var left = new BoundLiteralExpression(1.0);
        var right = new BoundLiteralExpression(2.0);
        var expr = new BoundBinaryExpression(left, MathOperator.Add, right);
        var result = engine.InferBound(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BoundFunction()
    {
        var engine = new TypeInferenceEngine();
        var arg = new BoundLiteralExpression(0.0);
        var sinSymbol = new FunctionSymbol("sin", Array.Empty<ParameterSymbol>());
        var expr = new BoundFunctionCallExpression(sinSymbol, new BoundExpression[] { arg });
        var result = engine.InferBound(expr);
        result.Type.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Infer_BoundUnary()
    {
        var engine = new TypeInferenceEngine();
        var inner = new BoundLiteralExpression(5.0);
        var expr = new BoundUnaryExpression(MathOperator.Negate, inner);
        var result = engine.InferBound(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_BoundAssignment()
    {
        var engine = new TypeInferenceEngine();
        var target = new VariableSymbol("x");
        var value = new BoundLiteralExpression(42.0);
        var expr = new BoundAssignmentExpression(target, value);
        var result = engine.InferBound(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void InferFunction_ReturnsFunctionType()
    {
        var engine = new TypeInferenceEngine();
        var body = Expr.Binary(
            new MathVerse.Math.Expressions.VariableExpression("x"),
            "+",
            Expr.Literal(1.0));
        var result = engine.InferFunction(
            new[] { ("x", (MathType)RealType.Instance) }, body);
        result.Type.Should().BeOfType<FunctionType>();
        var ft = (FunctionType)result.Type;
        ft.Arity.Should().Be(1);
        ft.ReturnType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void InferFunction_TwoParams()
    {
        var engine = new TypeInferenceEngine();
        var body = Expr.Binary(
            new MathVerse.Math.Expressions.VariableExpression("x"),
            "+",
            new MathVerse.Math.Expressions.VariableExpression("y"));
        var result = engine.InferFunction(
            new[] { ("x", (MathType)RealType.Instance), ("y", (MathType)RealType.Instance) }, body);
        var ft = (FunctionType)result.Type;
        ft.Arity.Should().Be(2);
    }

    [Fact]
    public void Infer_LambdaTwoParams()
    {
        var engine = new TypeInferenceEngine();
        var body = Expr.Binary(
            new MathVerse.Math.Expressions.VariableExpression("x"),
            "*",
            new MathVerse.Math.Expressions.VariableExpression("y"));
        var result = engine.InferLambda(new[] { "x", "y" }, body);
        var ft = (FunctionType)result.Type;
        ft.Arity.Should().Be(2);
    }

    [Fact]
    public void Infer_NestedBinary()
    {
        var engine = new TypeInferenceEngine();
        var inner = Expr.Binary(Expr.Literal(2.0), "*", Expr.Literal(3.0));
        var expr = Expr.Binary(Expr.Literal(1.0), "+", inner);
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Infer_ComplexExpression()
    {
        var engine = new TypeInferenceEngine();
        var sinExpr = Expr.Call("sin", Expr.Literal(0.0));
        var cosExpr = Expr.Call("cos", Expr.Literal(0.0));
        var expr = Expr.Binary(sinExpr, "+", cosExpr);
        var result = engine.Infer(expr);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void InferenceContext_FreshVariable()
    {
        var ctx = new InferenceContext();
        var v1 = ctx.FreshVariable("a");
        var v2 = ctx.FreshVariable("b");
        v1.Id.Should().NotBe(v2.Id);
    }

    [Fact]
    public void InferenceContext_AddConstraint()
    {
        var ctx = new InferenceContext();
        var constraint = new TypeConstraint(TypeConstraintKind.Equality,
            RealType.Instance, IntegerType.Instance);
        ctx.AddConstraint(constraint);
        ctx.Constraints.Should().HaveCount(1);
    }

    [Fact]
    public void InferenceContext_AddEquality()
    {
        var ctx = new InferenceContext();
        ctx.AddEquality(RealType.Instance, IntegerType.Instance);
        ctx.Constraints.Should().HaveCount(1);
    }

    [Fact]
    public void InferenceContext_AddNumericConstraint()
    {
        var ctx = new InferenceContext();
        ctx.AddNumericConstraint(RealType.Instance);
        ctx.Constraints.Should().HaveCount(1);
    }

    [Fact]
    public void TypeVariable_Id()
    {
        var tv = new TypeVariable(42, "x");
        tv.Id.Should().Be(42);
    }

    [Fact]
    public void TypeVariable_SourceName()
    {
        var tv = new TypeVariable(1, "myVar");
        tv.SourceName.Should().Be("myVar");
    }

    [Fact]
    public void TypeVariable_Name_WithSource()
    {
        var tv = new TypeVariable(1, "x");
        tv.Name.Should().Be("x");
    }

    [Fact]
    public void TypeVariable_Name_WithoutSource()
    {
        var tv = new TypeVariable(1);
        tv.Name.Should().Be("?1");
    }

    [Fact]
    public void TypeVariable_IsGenericParameter()
    {
        var tv = new TypeVariable(1);
        tv.IsGenericParameter.Should().BeTrue();
    }

    [Fact]
    public void TypeVariable_Equals()
    {
        var tv1 = new TypeVariable(1);
        var tv2 = new TypeVariable(1);
        tv1.Equals(tv2).Should().BeTrue();
    }

    [Fact]
    public void TypeVariable_NotEquals_DifferentId()
    {
        var tv1 = new TypeVariable(1);
        var tv2 = new TypeVariable(2);
        tv1.Equals(tv2).Should().BeFalse();
    }

    [Fact]
    public void TypeVariable_GetHashCode()
    {
        var tv = new TypeVariable(1);
        tv.GetHashCode().Should().Be(tv.GetHashCode());
    }
}
