namespace MathVerse.Geometry.Tests.Transformations;

/// <summary>Tests for QuaternionRotation struct.</summary>
public class QuaternionRotationTests
{
    private const double Precision = 1e-8;

    /// <summary>Identity quaternion should have W=1 and XYZ=0.</summary>
    [Fact]
    public void Identity_ShouldHaveCorrectComponents()
    {
        var q = QuaternionRotation.Identity;
        q.X.Should().BeApproximately(0.0, Precision);
        q.Y.Should().BeApproximately(0.0, Precision);
        q.Z.Should().BeApproximately(0.0, Precision);
        q.W.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>FromAxisAngle on X axis should produce correct quaternion.</summary>
    [Fact]
    public void FromAxisAngle_XAxis_ShouldHaveCorrectComponents()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitX, System.Math.PI);
        q.X.Should().BeApproximately(1.0, Precision);
        q.Y.Should().BeApproximately(0.0, Precision);
        q.Z.Should().BeApproximately(0.0, Precision);
        q.W.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>FromAxisAngle on Y axis should produce correct quaternion.</summary>
    [Fact]
    public void FromAxisAngle_YAxis_ShouldHaveCorrectComponents()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitY, System.Math.PI);
        q.X.Should().BeApproximately(0.0, Precision);
        q.Y.Should().BeApproximately(1.0, Precision);
        q.Z.Should().BeApproximately(0.0, Precision);
        q.W.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>FromAxisAngle on Z axis should produce correct quaternion.</summary>
    [Fact]
    public void FromAxisAngle_ZAxis_ShouldHaveCorrectComponents()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI);
        q.X.Should().BeApproximately(0.0, Precision);
        q.Y.Should().BeApproximately(0.0, Precision);
        q.Z.Should().BeApproximately(1.0, Precision);
        q.W.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>FromEuler with zero angles should return identity.</summary>
    [Fact]
    public void FromEuler_ZeroAngles_ShouldReturnIdentity()
    {
        var q = QuaternionRotation.FromEuler(0, 0, 0);
        q.Length.Should().BeApproximately(1.0, Precision);
        System.Math.Abs(q.W).Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Normalize should produce unit quaternion.</summary>
    [Fact]
    public void Normalize_ShouldProduceUnitQuaternion()
    {
        var q = new QuaternionRotation(1, 2, 3, 4);
        var n = q.Normalize();
        n.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Conjugate should negate XYZ and keep W.</summary>
    [Fact]
    public void Conjugate_ShouldNegateVectorPart()
    {
        var q = new QuaternionRotation(1, 2, 3, 4);
        var c = q.Conjugate();
        c.X.Should().BeApproximately(-1.0, Precision);
        c.Y.Should().BeApproximately(-2.0, Precision);
        c.Z.Should().BeApproximately(-3.0, Precision);
        c.W.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Inverse times original should produce identity quaternion.</summary>
    [Fact]
    public void Inverse_TimesOriginal_ShouldBeIdentity()
    {
        var q = QuaternionRotation.FromAxisAngle(new Vector3D(1, 1, 0).Normalize(), System.Math.PI / 4);
        var inv = q.Inverse();
        var product = q.Multiply(inv);
        product.X.Should().BeApproximately(0.0, Precision);
        product.Y.Should().BeApproximately(0.0, Precision);
        product.Z.Should().BeApproximately(0.0, Precision);
        product.W.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Length of identity quaternion should be 1.</summary>
    [Fact]
    public void Length_Identity_ShouldBeOne()
    {
        QuaternionRotation.Identity.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Length of unit quaternion should be 1.</summary>
    [Fact]
    public void Length_UnitQuaternion_ShouldBeOne()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI / 3);
        q.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Rotate 90 degrees around Z should map X to Y.</summary>
    [Fact]
    public void Rotate_90DegreesZ_ShouldMapXToY()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI / 2);
        var v = Vector3D.UnitX;
        Vector3D result = q.Rotate(v);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Rotate 180 degrees around X should negate Y and Z.</summary>
    [Fact]
    public void Rotate_180DegreesX_ShouldNegateYAndZ()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitX, System.Math.PI);
        var v = new Vector3D(0, 1, 1);
        Vector3D result = q.Rotate(v);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(-1.0, Precision);
        result.Z.Should().BeApproximately(-1.0, Precision);
    }

    /// <summary>ToTransform should produce a valid rotation matrix.</summary>
    [Fact]
    public void ToTransform_ShouldProduceRotationMatrix()
    {
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI / 4);
        var t = q.ToTransform();
        double det = t.Determinant();
        det.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>ToTransform matrix diagonal should sum to approximately 1 + 2*cos(angle).</summary>
    [Fact]
    public void ToTransform_MatrixTrace_ShouldBeCorrect()
    {
        double angle = System.Math.PI / 3;
        var q = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, angle);
        var t = q.ToTransform();
        double trace = t[0, 0] + t[1, 1] + t[2, 2] + t[3, 3];
        trace.Should().BeApproximately(2.0 + 2.0 * System.Math.Cos(angle), 1e-6);
    }

    /// <summary>Multiply two identity quaternions should yield identity.</summary>
    [Fact]
    public void Multiply_TwoIdentities_ShouldYieldIdentity()
    {
        var result = QuaternionRotation.Identity.Multiply(QuaternionRotation.Identity);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
        result.W.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Operator* should match Multiply method.</summary>
    [Fact]
    public void OperatorStar_ShouldMatchMultiplyMethod()
    {
        var a = QuaternionRotation.FromAxisAngle(Vector3D.UnitX, 0.5);
        var b = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, 0.7);
        var viaMethod = a.Multiply(b);
        var viaOperator = a * b;
        viaMethod.X.Should().BeApproximately(viaOperator.X, Precision);
        viaMethod.Y.Should().BeApproximately(viaOperator.Y, Precision);
        viaMethod.Z.Should().BeApproximately(viaOperator.Z, Precision);
        viaMethod.W.Should().BeApproximately(viaOperator.W, Precision);
    }

    /// <summary>Slerp at t=0 should return the start quaternion.</summary>
    [Fact]
    public void Slerp_AtZero_ShouldReturnStart()
    {
        var start = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, 0);
        var end = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI);
        var result = start.Slerp(end, 0);
        result.X.Should().BeApproximately(start.X, Precision);
        result.Y.Should().BeApproximately(start.Y, Precision);
        result.Z.Should().BeApproximately(start.Z, Precision);
        result.W.Should().BeApproximately(start.W, Precision);
    }

    /// <summary>Slerp at t=1 should return the target quaternion.</summary>
    [Fact]
    public void Slerp_AtOne_ShouldReturnTarget()
    {
        var start = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, 0);
        var end = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI / 2);
        var result = start.Slerp(end, 1);
        result.X.Should().BeApproximately(end.X, Precision);
        result.Y.Should().BeApproximately(end.Y, Precision);
        result.Z.Should().BeApproximately(end.Z, Precision);
        result.W.Should().BeApproximately(end.W, Precision);
    }

    /// <summary>Slerp at t=0.5 should be midpoint rotation.</summary>
    [Fact]
    public void Slerp_AtHalf_ShouldBeMidpoint()
    {
        var start = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, 0);
        var end = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI);
        var result = start.Slerp(end, 0.5);
        result.Length.Should().BeApproximately(1.0, Precision);
        var rotated = result.Rotate(Vector3D.UnitX);
        rotated.X.Should().BeApproximately(0.0, Precision);
        rotated.Y.Should().BeApproximately(1.0, Precision);
        rotated.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Double rotation by 180 degrees should equal 360 degrees.</summary>
    [Fact]
    public void DoubleRotation_180Twice_ShouldEqual360()
    {
        var half = QuaternionRotation.FromAxisAngle(Vector3D.UnitZ, System.Math.PI);
        var full = half.Multiply(half);
        var v = Vector3D.UnitX;
        Vector3D result = full.Rotate(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>360-degree rotation should return vector to original direction.</summary>
    [Fact]
    public void Rotation360_ShouldReturnToOriginal()
    {
        var q = QuaternionRotation.FromAxisAngle(new Vector3D(1, 1, 1).Normalize(), 2.0 * System.Math.PI);
        var v = new Vector3D(1, 2, 3);
        Vector3D result = q.Rotate(v);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Length squared should equal length squared.</summary>
    [Fact]
    public void LengthSquared_ShouldEqualLengthSquared()
    {
        var q = new QuaternionRotation(1, 2, 3, 4);
        q.LengthSquared.Should().BeApproximately(q.Length * q.Length, Precision);
    }

    /// <summary>Normalized quaternion should have unit length.</summary>
    [Fact]
    public void Normalize_RandomQuaternion_ShouldHaveUnitLength()
    {
        var q = new QuaternionRotation(3, 4, 5, 6);
        var n = q.Normalize();
        n.Length.Should().BeApproximately(1.0, Precision);
    }
}
