namespace MathVerse.Expression.Tests;

public class PiecewiseExpressionTests
{
    [Fact]
    public void Piecewise_WithCasesAndDefault_HasCorrectKind()
    {
        var pw = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
            new PiecewiseCase(Expr.Literal(2), Expr.Literal(2)),
        ], Expr.Literal(0));

        pw.Kind.Should().Be(ExpressionKind.Piecewise);
    }

    [Fact]
    public void Piecewise_StoresAllCasesAndDefault()
    {
        var v = Expr.Variable("x");
        var case1Value = Expr.Literal(1);
        var case1Cond = Expr.GreaterThan(v, Expr.Literal(0));
        var case2Value = Expr.Literal(-1);
        var case2Cond = Expr.LessThan(v, Expr.Literal(0));
        var def = Expr.Literal(0);

        var pw = Expr.Piecewise(
        [
            new PiecewiseCase(case1Value, case1Cond),
            new PiecewiseCase(case2Value, case2Cond),
        ], def);

        pw.Cases.Should().HaveCount(2);
        pw.Cases[0].Value.Should().BeSameAs(case1Value);
        pw.Cases[0].Condition.Should().BeSameAs(case1Cond);
        pw.Cases[1].Value.Should().BeSameAs(case2Value);
        pw.Cases[1].Condition.Should().BeSameAs(case2Cond);
        pw.DefaultCase.Should().BeSameAs(def);
    }

    [Fact]
    public void Piecewise_WithoutDefault_DefaultCaseIsNull()
    {
        var pw = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Boolean(true)),
        ]);

        pw.DefaultCase.Should().BeNull();
    }

    [Fact]
    public void Piecewise_Equal_WhenSameStructure()
    {
        var a = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
        ], Expr.Literal(0));
        var b = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
        ], Expr.Literal(0));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Piecewise_NotEqual_WhenDifferentDefault()
    {
        var a = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
        ], Expr.Literal(0));
        var b = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
        ], Expr.Literal(99));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Piecewise_ChildrenCount_MatchesCasesTimesTwoPlusDefault()
    {
        var pw = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
            new PiecewiseCase(Expr.Literal(2), Expr.Literal(2)),
            new PiecewiseCase(Expr.Literal(3), Expr.Literal(3)),
        ], Expr.Literal(0));

        pw.Children.Count.Should().Be(7);
    }

    [Fact]
    public void Piecewise_DepthAndNodeCount_AreCorrect()
    {
        var pw = Expr.Piecewise(
        [
            new PiecewiseCase(Expr.Literal(1), Expr.Literal(1)),
        ], Expr.Literal(0));

        pw.Depth.Should().Be(1);
        pw.NodeCount.Should().Be(4);
    }
}

public class ConditionalExpressionTests
{
    [Fact]
    public void Conditional_Creation_StoresBranches()
    {
        var cond = Expr.Literal(1);
        var then = Expr.Literal(2);
        var els = Expr.Literal(3);

        var c = Expr.Conditional(cond, then, els);

        c.Kind.Should().Be(ExpressionKind.Conditional);
        c.Condition.Should().BeSameAs(cond);
        c.ThenBranch.Should().BeSameAs(then);
        c.ElseBranch.Should().BeSameAs(els);
    }

    [Fact]
    public void Conditional_Children_ReturnsThree()
    {
        var c = Expr.Conditional(
            Expr.GreaterThan(Expr.Variable("x"), Expr.Literal(0)),
            Expr.Literal(1),
            Expr.Literal(-1));

        c.Children.Should().HaveCount(3);
        c.Children[0].Should().BeSameAs(c.Condition);
        c.Children[1].Should().BeSameAs(c.ThenBranch);
        c.Children[2].Should().BeSameAs(c.ElseBranch);
    }

    [Fact]
    public void Conditional_Equal_WhenSameBranches()
    {
        var a = Expr.Conditional(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        var b = Expr.Conditional(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Conditional_NotEqual_WhenDifferentCondition()
    {
        var a = Expr.Conditional(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        var b = Expr.Conditional(Expr.Literal(9), Expr.Literal(2), Expr.Literal(3));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Conditional_ToString_ContainsIfThenElse()
    {
        var c = Expr.Conditional(
            Expr.GreaterThan(Expr.Variable("x"), Expr.Literal(0)),
            Expr.Literal(1),
            Expr.Literal(-1));

        var s = c.ToString();
        s.Should().NotBeNullOrWhiteSpace();
    }
}

public class TupleExpressionTests
{
    [Fact]
    public void Tuple_Creation_StoresElements()
    {
        var t = Expr.Tuple(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        t.Kind.Should().Be(ExpressionKind.Tuple);
        t.Elements.Should().HaveCount(3);
        ((LiteralExpression)t.Elements[0]).Value.Should().Be(1);
        ((LiteralExpression)t.Elements[1]).Value.Should().Be(2);
        ((LiteralExpression)t.Elements[2]).Value.Should().Be(3);
    }

    [Fact]
    public void Tuple_Children_IsSameAsElements()
    {
        var e1 = Expr.Literal(1);
        var e2 = Expr.Variable("y");
        var t = Expr.Tuple(e1, e2);

        t.Children.Should().HaveCount(2);
        t.Children[0].Should().BeSameAs(e1);
        t.Children[1].Should().BeSameAs(e2);
    }

    [Fact]
    public void Tuple_Equal_WhenSameElements()
    {
        var a = Expr.Tuple(Expr.Literal(1), Expr.Variable("x"));
        var b = Expr.Tuple(Expr.Literal(1), Expr.Variable("x"));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Tuple_NotEqual_WhenDifferentLength()
    {
        var a = Expr.Tuple(Expr.Literal(1));
        var b = Expr.Tuple(Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }
}

public class VectorExpressionTests
{
    [Fact]
    public void Vector_Creation_StoresComponents()
    {
        var v = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        v.Kind.Should().Be(ExpressionKind.Vector);
        v.Components.Should().HaveCount(3);
        v.Dimension.Should().Be(3);
    }

    [Fact]
    public void Vector_Empty_HasZeroDimension()
    {
        var v = Expr.Vector();

        v.Dimension.Should().Be(0);
        v.Components.Should().BeEmpty();
    }

    [Fact]
    public void Vector_Equal_WhenSameComponents()
    {
        var a = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Vector(Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Vector_NotEqual_WhenDifferentDimension()
    {
        var a = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        a.Equals(b).Should().BeFalse();
    }
}

public class MatrixExpressionTests
{
    [Fact]
    public void Matrix_FromVectors_CorrectDimensions()
    {
        var r1 = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var r2 = Expr.Vector(Expr.Literal(3), Expr.Literal(4));

        var m = Expr.Matrix(r1, r2);

        m.Kind.Should().Be(ExpressionKind.Matrix);
        m.RowCount.Should().Be(2);
        m.ColumnCount.Should().Be(2);
    }

    [Fact]
    public void Matrix_From2DArray_CorrectDimensions()
    {
        var arr = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        var m = Expr.Matrix(arr);

        m.RowCount.Should().Be(2);
        m.ColumnCount.Should().Be(3);
    }

    [Fact]
    public void Matrix_From2DArray_PopulatesRows()
    {
        var arr = new double[,] { { 10, 20 }, { 30, 40 } };

        var m = Expr.Matrix(arr);

        m.Rows.Should().HaveCount(2);
        m.Rows[0].Should().BeOfType<VectorExpression>();
        m.Rows[1].Should().BeOfType<VectorExpression>();
    }

    [Fact]
    public void Matrix_Equal_WhenSameRows()
    {
        var a = Expr.Matrix(Expr.Vector(Expr.Literal(1)), Expr.Vector(Expr.Literal(2)));
        var b = Expr.Matrix(Expr.Vector(Expr.Literal(1)), Expr.Vector(Expr.Literal(2)));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Matrix_NotEqual_WhenDifferentRowCount()
    {
        var a = Expr.Matrix(Expr.Vector(Expr.Literal(1)));
        var b = Expr.Matrix(Expr.Vector(Expr.Literal(1)), Expr.Vector(Expr.Literal(2)));

        a.Equals(b).Should().BeFalse();
    }
}

public class TensorExpressionTests
{
    [Fact]
    public void Tensor_Creation_StoresShapeAndComponents()
    {
        var t = Expr.Tensor([2, 3],
            Expr.Literal(1), Expr.Literal(2), Expr.Literal(3),
            Expr.Literal(4), Expr.Literal(5), Expr.Literal(6));

        t.Kind.Should().Be(ExpressionKind.Tensor);
        t.Shape.Should().BeEquivalentTo(new[] { 2, 3 });
        t.Rank.Should().Be(2);
        t.Components.Should().HaveCount(6);
    }

    [Fact]
    public void Tensor_Equal_WhenSameShapeAndComponents()
    {
        var a = Expr.Tensor([2], Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Tensor([2], Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Tensor_NotEqual_WhenDifferentShape()
    {
        var a = Expr.Tensor([2], Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Tensor([1, 2], Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }
}

public class IndexExpressionTests
{
    [Fact]
    public void Index_Creation_StoresTargetAndIndices()
    {
        var target = Expr.Variable("A");
        var idx = Expr.Index(target, Expr.Literal(0), Expr.Literal(1));

        idx.Kind.Should().Be(ExpressionKind.Index);
        idx.Target.Should().BeSameAs(target);
        idx.Indices.Should().HaveCount(2);
    }

    [Fact]
    public void Index_Children_IncludesTargetThenIndices()
    {
        var target = Expr.Variable("M");
        var i = Expr.Literal(0);
        var j = Expr.Literal(1);
        var idx = Expr.Index(target, i, j);

        idx.Children.Should().HaveCount(3);
        idx.Children[0].Should().BeSameAs(target);
        idx.Children[1].Should().BeSameAs(i);
        idx.Children[2].Should().BeSameAs(j);
    }

    [Fact]
    public void Index_Equal_WhenSameTargetAndIndices()
    {
        var a = Expr.Index(Expr.Variable("A"), Expr.Literal(1));
        var b = Expr.Index(Expr.Variable("A"), Expr.Literal(1));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Index_NotEqual_WhenDifferentTarget()
    {
        var a = Expr.Index(Expr.Variable("A"), Expr.Literal(1));
        var b = Expr.Index(Expr.Variable("B"), Expr.Literal(1));

        a.Equals(b).Should().BeFalse();
    }
}

public class SliceExpressionTests
{
    [Fact]
    public void Slice_Creation_StoresTargetAndSlices()
    {
        var target = Expr.Variable("A");
        var s = Expr.Slice(target, Expr.Literal(1), null);

        s.Kind.Should().Be(ExpressionKind.Slice);
        s.Target.Should().BeSameAs(target);
        s.Slices.Should().HaveCount(2);
        s.Slices[1].Should().BeNull();
    }

    [Fact]
    public void Slice_Equal_WhenSameTargetAndSliceCount()
    {
        var t = Expr.Variable("A");
        var a = Expr.Slice(t, Expr.Literal(0), null);
        var b = Expr.Slice(t, Expr.Literal(99), null);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Slice_NotEqual_WhenDifferentSliceCount()
    {
        var t = Expr.Variable("A");
        var a = Expr.Slice(t, Expr.Literal(0));
        var b = Expr.Slice(t, Expr.Literal(0), null);

        a.Equals(b).Should().BeFalse();
    }
}
