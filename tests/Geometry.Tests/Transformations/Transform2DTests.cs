namespace MathVerse.Geometry.Tests.Transformations;

/// <summary>Tests for Transform2D affine 2D transformation struct.</summary>
public class Transform2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Identity transformation should leave points unchanged.</summary>
    [Fact]
    public void Identity_TransformPoint_ShouldReturnSamePoint()
    {
        var t = Transform2D.Identity;
        var p = new Point2D(3, 7);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Identity determinant should be 1.</summary>
    [Fact]
    public void Identity_Determinant_ShouldBeOne()
    {
        Transform2D.Identity.Determinant().Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Identity matrix elements should match diagonal of ones.</summary>
    [Fact]
    public void Identity_MatrixElements_ShouldBeDiagonal()
    {
        var t = Transform2D.Identity;
        t[0, 0].Should().BeApproximately(1.0, Precision);
        t[1, 1].Should().BeApproximately(1.0, Precision);
        t[2, 2].Should().BeApproximately(1.0, Precision);
        t[0, 1].Should().BeApproximately(0.0, Precision);
        t[1, 0].Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Translation should offset point by (dx, dy).</summary>
    [Fact]
    public void Translation_TransformPoint_ShouldOffset()
    {
        var t = Transform2D.Translation(5, 10);
        var p = new Point2D(1, 2);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(6.0, Precision);
        result.Y.Should().BeApproximately(12.0, Precision);
    }

    /// <summary>Translation inverse should restore original point.</summary>
    [Fact]
    public void Translation_Inverse_ShouldRestorePoint()
    {
        var t = Transform2D.Translation(5, 10);
        var ti = t.Inverse();
        var p = new Point2D(1, 2);
        Point2D result = ti.TransformPoint(t.TransformPoint(p));
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Translation should not affect vectors.</summary>
    [Fact]
    public void Translation_TransformVector_ShouldNotAffect()
    {
        var t = Transform2D.Translation(100, 200);
        var v = new Vector2D(1, 0);
        Vector2D result = t.TransformVector(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>90-degree rotation should map (1,0) to (0,1).</summary>
    [Fact]
    public void Rotation90_PointOnXAxis_ShouldMapToYAxis()
    {
        var t = Transform2D.Rotation(System.Math.PI / 2);
        var p = new Point2D(1, 0);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>180-degree rotation should negate coordinates.</summary>
    [Fact]
    public void Rotation180_ShouldNegateCoordinates()
    {
        var t = Transform2D.Rotation(System.Math.PI);
        var p = new Point2D(3, 4);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(-3.0, Precision);
        result.Y.Should().BeApproximately(-4.0, Precision);
    }

    /// <summary>360-degree rotation should return to original position.</summary>
    [Fact]
    public void Rotation360_ShouldReturnToOriginal()
    {
        var t = Transform2D.Rotation(2.0 * System.Math.PI);
        var p = new Point2D(5, 7);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Rotation determinant should be 1.</summary>
    [Fact]
    public void Rotation_Determinant_ShouldBeOne()
    {
        var t = Transform2D.Rotation(System.Math.PI / 4);
        t.Determinant().Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Uniform scaling should scale both axes equally.</summary>
    [Fact]
    public void ScalingUniform_ShouldScaleBothAxes()
    {
        var t = Transform2D.Scaling(3.0);
        var p = new Point2D(2, 4);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(6.0, Precision);
        result.Y.Should().BeApproximately(12.0, Precision);
    }

    /// <summary>Non-uniform scaling should scale axes independently.</summary>
    [Fact]
    public void ScalingNonUniform_ShouldScaleAxesIndependently()
    {
        var t = Transform2D.Scaling(2.0, 3.0);
        var p = new Point2D(4, 5);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(8.0, Precision);
        result.Y.Should().BeApproximately(15.0, Precision);
    }

    /// <summary>Scaling determinant should equal the product of scale factors.</summary>
    [Fact]
    public void Scaling_Determinant_ShouldBeProductOfScales()
    {
        var t = Transform2D.Scaling(2.0, 5.0);
        t.Determinant().Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Shearing should offset x based on y.</summary>
    [Fact]
    public void Shearing_ShouldOffsetCoordinates()
    {
        var t = Transform2D.Shearing(1.0, 0.0);
        var p = new Point2D(1, 2);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Shearing determinant should be 1 - shx*shy.</summary>
    [Fact]
    public void Shearing_Determinant_ShouldBeOneMinusShxShy()
    {
        var t = Transform2D.Shearing(0.5, 0.3);
        t.Determinant().Should().BeApproximately(1.0 - 0.5 * 0.3, Precision);
    }

    /// <summary>Multiply identity by any transform should yield the same transform.</summary>
    [Fact]
    public void Multiply_IdentityLeft_ShouldReturnOther()
    {
        var t = Transform2D.Translation(5, 10);
        var result = Transform2D.Identity.Multiply(t);
        var p = new Point2D(0, 0);
        Point2D pt = result.TransformPoint(p);
        pt.X.Should().BeApproximately(5.0, Precision);
        pt.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Multiply should compose two transformations correctly.</summary>
    [Fact]
    public void Multiply_TwoTranslations_ShouldCombine()
    {
        var t1 = Transform2D.Translation(1, 2);
        var t2 = Transform2D.Translation(3, 4);
        var combined = t1.Multiply(t2);
        var p = new Point2D(0, 0);
        Point2D result = combined.TransformPoint(p);
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Multiply operator should produce same result as method.</summary>
    [Fact]
    public void Multiply_Operator_ShouldMatchMethod()
    {
        var a = Transform2D.Translation(1, 2);
        var b = Transform2D.Rotation(System.Math.PI / 6);
        var p = new Point2D(3, 4);
        Point2D viaMethod = a.Multiply(b).TransformPoint(p);
        Point2D viaOperator = (a * b).TransformPoint(p);
        viaMethod.X.Should().BeApproximately(viaOperator.X, Precision);
        viaMethod.Y.Should().BeApproximately(viaOperator.Y, Precision);
    }

    /// <summary>T * T^-1 should equal identity.</summary>
    [Fact]
    public void Inverse_TimesOriginal_ShouldBeIdentity()
    {
        var t = Transform2D.Translation(3, 7).Multiply(
            Transform2D.Rotation(System.Math.PI / 5)).Multiply(
            Transform2D.Scaling(2, 3));
        var inv = t.Inverse();
        var product = t.Multiply(inv);
        product[0, 0].Should().BeApproximately(1.0, Precision);
        product[1, 1].Should().BeApproximately(1.0, Precision);
        product[2, 2].Should().BeApproximately(1.0, Precision);
        product[0, 1].Should().BeApproximately(0.0, Precision);
        product[1, 0].Should().BeApproximately(0.0, Precision);
        product[0, 2].Should().BeApproximately(0.0, Precision);
        product[1, 2].Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Inverse of translation should negate translation values.</summary>
    [Fact]
    public void Inverse_Translation_ShouldNegateTranslation()
    {
        var t = Transform2D.Translation(10, -5);
        var inv = t.Inverse();
        var p = new Point2D(0, 0);
        Point2D result = inv.TransformPoint(p);
        result.X.Should().BeApproximately(-10.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>TransformPoint via operator should match method call.</summary>
    [Fact]
    public void TransformPoint_Operator_ShouldMatchMethod()
    {
        var t = Transform2D.Rotation(System.Math.PI / 3);
        var p = new Point2D(2, 5);
        Point2D viaMethod = t.TransformPoint(p);
        Point2D viaOperator = t * p;
        viaMethod.X.Should().BeApproximately(viaOperator.X, Precision);
        viaMethod.Y.Should().BeApproximately(viaOperator.Y, Precision);
    }

    /// <summary>TransformVector should apply rotation to vector.</summary>
    [Fact]
    public void TransformVector_Rotation_ShouldRotateVector()
    {
        var t = Transform2D.Rotation(System.Math.PI / 2);
        var v = new Vector2D(1, 0);
        Vector2D result = t.TransformVector(v);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Compose should produce same result as Multiply.</summary>
    [Fact]
    public void Compose_ShouldMatchMultiply()
    {
        var a = Transform2D.Scaling(2, 3);
        var b = Transform2D.Translation(1, 1);
        var p = new Point2D(1, 1);
        Point2D viaCompose = a.Compose(b).TransformPoint(p);
        Point2D viaMultiply = a.Multiply(b).TransformPoint(p);
        viaCompose.X.Should().BeApproximately(viaMultiply.X, Precision);
        viaCompose.Y.Should().BeApproximately(viaMultiply.Y, Precision);
    }

    /// <summary>Reflection across X axis should negate Y coordinate.</summary>
    [Fact]
    public void Reflection_XAxis_ShouldNegateY()
    {
        var t = Transform2D.Reflection(Vector2D.UnitX);
        var p = new Point2D(3, 5);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(-5.0, Precision);
    }

    /// <summary>Reflection across Y axis should negate X coordinate.</summary>
    [Fact]
    public void Reflection_YAxis_ShouldNegateX()
    {
        var t = Transform2D.Reflection(Vector2D.UnitY);
        var p = new Point2D(3, 5);
        Point2D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(-3.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Double reflection should restore original point.</summary>
    [Fact]
    public void Reflection_Double_ShouldRestoreOriginal()
    {
        var t = Transform2D.Reflection(new Vector2D(1, 1).Normalize());
        var p = new Point2D(4, 7);
        Point2D result = t.TransformPoint(t.TransformPoint(p));
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Reflection determinant should be -1.</summary>
    [Fact]
    public void Reflection_Determinant_ShouldBeNegativeOne()
    {
        var t = Transform2D.Reflection(Vector2D.UnitX);
        t.Determinant().Should().BeApproximately(-1.0, Precision);
    }

    /// <summary>Indexer should throw for out-of-range indices.</summary>
    [Fact]
    public void Indexer_Invalid_ShouldThrow()
    {
        var t = Transform2D.Identity;
        Action act = () => _ = t[3, 0];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    /// <summary>Operator* for point should match TransformPoint.</summary>
    [Fact]
    public void OperatorStar_Point_ShouldMatchTransformPoint()
    {
        var t = Transform2D.Translation(2, 3);
        var p = new Point2D(1, 1);
        Point2D result = t * p;
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(4.0, Precision);
    }
}
