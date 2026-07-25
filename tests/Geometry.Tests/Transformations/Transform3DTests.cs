namespace MathVerse.Geometry.Tests.Transformations;

/// <summary>Tests for Transform3D affine 4x4 transformation struct.</summary>
public class Transform3DTests
{
    private const double Precision = 1e-10;

    private static void AssertMatrixElements(Transform3D t, double[,] expected)
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                t[i, j].Should().BeApproximately(expected[i, j], Precision,
                    because: $"element [{i},{j}] should match");
    }

    /// <summary>Identity transformation should leave points unchanged.</summary>
    [Fact]
    public void Identity_TransformPoint_ShouldReturnSamePoint()
    {
        var t = Transform3D.Identity;
        var p = new Point3D(3, 7, 11);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
        result.Z.Should().BeApproximately(11.0, Precision);
    }

    /// <summary>Identity determinant should be 1.</summary>
    [Fact]
    public void Identity_Determinant_ShouldBeOne()
    {
        Transform3D.Identity.Determinant().Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Identity matrix should have ones on diagonal.</summary>
    [Fact]
    public void Identity_DiagonalElements_ShouldBeOne()
    {
        var t = Transform3D.Identity;
        for (int i = 0; i < 4; i++)
            t[i, i].Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Translation should offset point by (dx, dy, dz).</summary>
    [Fact]
    public void Translation_TransformPoint_ShouldOffset()
    {
        var t = Transform3D.Translation(5, 10, 15);
        var p = new Point3D(1, 2, 3);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(6.0, Precision);
        result.Y.Should().BeApproximately(12.0, Precision);
        result.Z.Should().BeApproximately(18.0, Precision);
    }

    /// <summary>Translation inverse should restore original point.</summary>
    [Fact]
    public void Translation_Inverse_ShouldRestorePoint()
    {
        var t = Transform3D.Translation(5, 10, 15);
        var inv = t.Inverse();
        var p = new Point3D(1, 2, 3);
        Point3D result = inv.TransformPoint(t.TransformPoint(p));
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Translation should not affect vectors.</summary>
    [Fact]
    public void Translation_TransformVector_ShouldNotAffect()
    {
        var t = Transform3D.Translation(100, 200, 300);
        var v = new Vector3D(1, 0, 0);
        Vector3D result = t.TransformVector(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>RotationX 90 degrees should map Y axis to Z axis.</summary>
    [Fact]
    public void RotationX90_YAxis_ShouldMapToZAxis()
    {
        var t = Transform3D.RotationX(System.Math.PI / 2);
        var v = new Vector3D(0, 1, 0);
        Vector3D result = t.TransformVector(v);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>RotationX 180 degrees should negate Y and Z.</summary>
    [Fact]
    public void RotationX180_ShouldNegateYAndZ()
    {
        var t = Transform3D.RotationX(System.Math.PI);
        var p = new Point3D(1, 2, 3);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(-2.0, Precision);
        result.Z.Should().BeApproximately(-3.0, Precision);
    }

    /// <summary>RotationY 90 degrees should map Z axis to X axis.</summary>
    [Fact]
    public void RotationY90_ZAxis_ShouldMapToXAxis()
    {
        var t = Transform3D.RotationY(System.Math.PI / 2);
        var v = new Vector3D(0, 0, 1);
        Vector3D result = t.TransformVector(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>RotationY 180 degrees should negate X and Z.</summary>
    [Fact]
    public void RotationY180_ShouldNegateXAndZ()
    {
        var t = Transform3D.RotationY(System.Math.PI);
        var p = new Point3D(1, 2, 3);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(-1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(-3.0, Precision);
    }

    /// <summary>RotationZ 90 degrees should map X axis to Y axis.</summary>
    [Fact]
    public void RotationZ90_XAxis_ShouldMapToYAxis()
    {
        var t = Transform3D.RotationZ(System.Math.PI / 2);
        var v = new Vector3D(1, 0, 0);
        Vector3D result = t.TransformVector(v);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>RotationZ 180 degrees should negate X and Y.</summary>
    [Fact]
    public void RotationZ180_ShouldNegateXAndY()
    {
        var t = Transform3D.RotationZ(System.Math.PI);
        var p = new Point3D(1, 2, 3);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(-1.0, Precision);
        result.Y.Should().BeApproximately(-2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>RotationAxis about Z should match RotationZ.</summary>
    [Fact]
    public void RotationAxis_ZAxis_ShouldMatchRotationZ()
    {
        double angle = System.Math.PI / 4;
        var tAxis = Transform3D.RotationAxis(Vector3D.UnitZ, angle);
        var tZ = Transform3D.RotationZ(angle);
        var p = new Point3D(1, 2, 3);
        Point3D fromAxis = tAxis.TransformPoint(p);
        Point3D fromZ = tZ.TransformPoint(p);
        fromAxis.X.Should().BeApproximately(fromZ.X, Precision);
        fromAxis.Y.Should().BeApproximately(fromZ.Y, Precision);
        fromAxis.Z.Should().BeApproximately(fromZ.Z, Precision);
    }

    /// <summary>Uniform scaling should scale all axes equally.</summary>
    [Fact]
    public void ScalingUniform_ShouldScaleAllAxes()
    {
        var t = Transform3D.Scaling(3.0);
        var p = new Point3D(1, 2, 3);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
        result.Z.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>Non-uniform scaling should scale axes independently.</summary>
    [Fact]
    public void ScalingNonUniform_ShouldScaleAxesIndependently()
    {
        var t = Transform3D.Scaling(2.0, 3.0, 4.0);
        var p = new Point3D(1, 1, 1);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(2.0, Precision);
        result.Y.Should().BeApproximately(3.0, Precision);
        result.Z.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Scaling determinant should equal the product of scale factors.</summary>
    [Fact]
    public void Scaling_Determinant_ShouldBeProductOfScales()
    {
        var t = Transform3D.Scaling(2.0, 3.0, 4.0);
        t.Determinant().Should().BeApproximately(24.0, Precision);
    }

    /// <summary>Shearing should offset coordinates.</summary>
    [Fact]
    public void Shearing_ShouldOffsetCoordinates()
    {
        var t = Transform3D.Shearing(1.0, 0.0, 0.0, 0.0, 0.0, 0.0);
        var p = new Point3D(1, 0, 0);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Shearing determinant should match computed value.</summary>
    [Fact]
    public void Shearing_Determinant_ShouldMatchComputed()
    {
        var t = Transform3D.Shearing(0.5, 0.3, 0.2, 0.1, 0.4, 0.6);
        // det = 1 - zy*yz - yx*xy + yx*zy*xz + zx*xy*yz - zx*xz
        // For (xy=0.5, xz=0.3, yx=0.2, yz=0.1, zx=0.4, zy=0.6):
        // = 1 - 0.6*0.1 - 0.2*0.5 + 0.2*0.6*0.3 + 0.4*0.5*0.1 - 0.4*0.3
        // = 1 - 0.06 - 0.1 + 0.036 + 0.02 - 0.12 = 0.776
        t.Determinant().Should().BeApproximately(0.776, Precision);
    }

    /// <summary>Multiply identity by any transform should yield the same transform.</summary>
    [Fact]
    public void Multiply_IdentityLeft_ShouldReturnOther()
    {
        var t = Transform3D.Translation(5, 10, 15);
        var result = Transform3D.Identity.Multiply(t);
        var p = new Point3D(0, 0, 0);
        Point3D pt = result.TransformPoint(p);
        pt.X.Should().BeApproximately(5.0, Precision);
        pt.Y.Should().BeApproximately(10.0, Precision);
        pt.Z.Should().BeApproximately(15.0, Precision);
    }

    /// <summary>Multiply should compose two translations correctly.</summary>
    [Fact]
    public void Multiply_TwoTranslations_ShouldCombine()
    {
        var t1 = Transform3D.Translation(1, 2, 3);
        var t2 = Transform3D.Translation(4, 5, 6);
        var combined = t1.Multiply(t2);
        var p = new Point3D(0, 0, 0);
        Point3D result = combined.TransformPoint(p);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
        result.Z.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>T * T^-1 should approximate identity.</summary>
    [Fact]
    public void Inverse_TimesOriginal_ShouldBeIdentity()
    {
        var t = Transform3D.Translation(3, 7, 2)
            .Multiply(Transform3D.RotationY(System.Math.PI / 5))
            .Multiply(Transform3D.Scaling(2, 3, 4));
        var inv = t.Inverse();
        var product = t.Multiply(inv);
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                product[i, j].Should().BeApproximately(i == j ? 1.0 : 0.0, 1e-6,
                    because: $"T * T^-1 element [{i},{j}] should be {(i == j ? 1 : 0)}");
    }

    /// <summary>Inverse of translation should negate translation values.</summary>
    [Fact]
    public void Inverse_Translation_ShouldNegateTranslation()
    {
        var t = Transform3D.Translation(10, -5, 20);
        var inv = t.Inverse();
        var p = new Point3D(0, 0, 0);
        Point3D result = inv.TransformPoint(p);
        result.X.Should().BeApproximately(-10.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
        result.Z.Should().BeApproximately(-20.0, Precision);
    }

    /// <summary>TransformPoint uses w=1 and includes translation.</summary>
    [Fact]
    public void TransformPoint_IncludesTranslation()
    {
        var t = Transform3D.Translation(1, 2, 3);
        var p = new Point3D(0, 0, 0);
        Point3D result = t.TransformPoint(p);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>TransformVector uses w=0 and ignores translation.</summary>
    [Fact]
    public void TransformVector_IgnoresTranslation()
    {
        var t = Transform3D.Translation(100, 200, 300);
        var v = new Vector3D(1, 1, 1);
        Vector3D result = t.TransformVector(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>LookAt should produce a valid view matrix.</summary>
    [Fact]
    public void LookAt_ShouldProduceValidViewMatrix()
    {
        var eye = new Point3D(0, 0, 5);
        var target = new Point3D(0, 0, 0);
        var up = Vector3D.UnitY;
        var view = Transform3D.LookAt(eye, target, up);
        view[3, 3].Should().BeApproximately(1.0, Precision);
    }

    /// <summary>LookAt determinant should be non-zero.</summary>
    [Fact]
    public void LookAt_Determinant_ShouldBeNonZero()
    {
        var eye = new Point3D(0, 0, 5);
        var target = new Point3D(0, 0, 0);
        var up = Vector3D.UnitY;
        var view = Transform3D.LookAt(eye, target, up);
        System.Math.Abs(view.Determinant()).Should().BeGreaterThan(1e-10);
    }

    /// <summary>FromRowMajor should create correct matrix.</summary>
    [Fact]
    public void FromRowMajor_ShouldCreateCorrectMatrix()
    {
        var m = new double[][] {
            new double[] {1, 2, 3, 4},
            new double[] {5, 6, 7, 8},
            new double[] {9, 10, 11, 12},
            new double[] {13, 14, 15, 16}
        };
        var t = Transform3D.FromRowMajor(m);
        t[0, 0].Should().BeApproximately(1.0, Precision);
        t[1, 2].Should().BeApproximately(7.0, Precision);
        t[2, 3].Should().BeApproximately(12.0, Precision);
        t[3, 0].Should().BeApproximately(13.0, Precision);
    }

    /// <summary>Operator* for Transform3D should match Multiply method.</summary>
    [Fact]
    public void OperatorStar_ShouldMatchMultiplyMethod()
    {
        var a = Transform3D.Translation(1, 2, 3);
        var b = Transform3D.RotationZ(System.Math.PI / 6);
        var p = new Point3D(1, 1, 1);
        Point3D viaMethod = a.Multiply(b).TransformPoint(p);
        Point3D viaOperator = (a * b).TransformPoint(p);
        viaMethod.X.Should().BeApproximately(viaOperator.X, Precision);
        viaMethod.Y.Should().BeApproximately(viaOperator.Y, Precision);
        viaMethod.Z.Should().BeApproximately(viaOperator.Z, Precision);
    }

    /// <summary>Operator* for point should match TransformPoint.</summary>
    [Fact]
    public void OperatorStar_Point_ShouldMatchTransformPoint()
    {
        var t = Transform3D.Translation(1, 2, 3);
        var p = new Point3D(4, 5, 6);
        Point3D result = t * p;
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(7.0, Precision);
        result.Z.Should().BeApproximately(9.0, Precision);
    }
}
