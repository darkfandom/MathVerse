namespace MathVerse.Geometry.Advanced.Tests.Matrix;

public class Matrix3x3Tests
{
    private const double PI = 3.14159265358979323846;
    [Fact]
    public void Identity_DiagonalElements()
    {
        Matrix3x3.Identity.M00.Should().Be(1);
        Matrix3x3.Identity.M11.Should().Be(1);
        Matrix3x3.Identity.M22.Should().Be(1);
    }

    [Fact]
    public void Identity_OffDiagonalElements()
    {
        Matrix3x3.Identity.M01.Should().Be(0);
        Matrix3x3.Identity.M02.Should().Be(0);
        Matrix3x3.Identity.M10.Should().Be(0);
        Matrix3x3.Identity.M12.Should().Be(0);
        Matrix3x3.Identity.M20.Should().Be(0);
        Matrix3x3.Identity.M21.Should().Be(0);
    }

    [Fact]
    public void IdentityTimesIdentity_EqualsIdentity()
    {
        var result = Matrix3x3.Identity * Matrix3x3.Identity;
        result.Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void IdentityTimesMatrix_EqualsMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        (Matrix3x3.Identity * m).Should().Be(m);
    }

    [Fact]
    public void MatrixTimesIdentity_EqualsMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        (m * Matrix3x3.Identity).Should().Be(m);
    }

    [Fact]
    public void Determinant_Identity_IsOne()
    {
        Matrix3x3.Identity.Determinant.Should().Be(1);
    }

    [Fact]
    public void Determinant_KnownMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        m.Determinant.Should().Be(-3);
    }

    [Fact]
    public void Determinant_ZeroMatrix_IsZero()
    {
        Matrix3x3.Zero.Determinant.Should().Be(0);
    }

    [Fact]
    public void Determinant_Diagonal()
    {
        var m = new Matrix3x3(2, 0, 0, 0, 3, 0, 0, 0, 4);
        m.Determinant.Should().Be(24);
    }

    [Fact]
    public void Determinant_SingularMatrix_IsZero()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Determinant.Should().Be(0);
    }

    [Fact]
    public void Determinant_WithNegativeElements()
    {
        var m = new Matrix3x3(1, -2, 3, -4, 5, -6, 7, -8, 9);
        m.Determinant.Should().Be(0);
    }

    [Fact]
    public void Transpose_Identity()
    {
        Matrix3x3.Identity.Transpose().Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void Transpose_KnownMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var t = m.Transpose();
        t.M00.Should().Be(1);
        t.M01.Should().Be(4);
        t.M02.Should().Be(7);
        t.M10.Should().Be(2);
        t.M11.Should().Be(5);
        t.M12.Should().Be(8);
        t.M20.Should().Be(3);
        t.M21.Should().Be(6);
        t.M22.Should().Be(9);
    }

    [Fact]
    public void Transpose_Twice_EqualsOriginal()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Transpose().Transpose().Should().Be(m);
    }

    [Fact]
    public void Transpose_SymmetricMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 2, 5, 6, 3, 6, 9);
        m.Transpose().Should().Be(m);
    }

    [Fact]
    public void Inverse_Identity()
    {
        Matrix3x3.Identity.Inverse().Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void Inverse_Twice_EqualsOriginal()
    {
        var m = new Matrix3x3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        m.Inverse().Inverse().Should().Be(m);
    }

    [Fact]
    public void Inverse_TimesOriginal_EqualsIdentity()
    {
        var m = new Matrix3x3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var result = m * m.Inverse();
        result.M00.Should().BeApproximately(1, 1e-10);
        result.M01.Should().BeApproximately(0, 1e-10);
        result.M02.Should().BeApproximately(0, 1e-10);
        result.M10.Should().BeApproximately(0, 1e-10);
        result.M11.Should().BeApproximately(1, 1e-10);
        result.M12.Should().BeApproximately(0, 1e-10);
        result.M20.Should().BeApproximately(0, 1e-10);
        result.M21.Should().BeApproximately(0, 1e-10);
        result.M22.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Inverse_OriginalTimesInverse_EqualsIdentity()
    {
        var m = new Matrix3x3(2, 1, 0, 1, 3, 1, 0, 1, 4);
        var result = m.Inverse() * m;
        result.M00.Should().BeApproximately(1, 1e-10);
        result.M01.Should().BeApproximately(0, 1e-10);
        result.M02.Should().BeApproximately(0, 1e-10);
        result.M10.Should().BeApproximately(0, 1e-10);
        result.M11.Should().BeApproximately(1, 1e-10);
        result.M12.Should().BeApproximately(0, 1e-10);
        result.M20.Should().BeApproximately(0, 1e-10);
        result.M21.Should().BeApproximately(0, 1e-10);
        result.M22.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Inverse_SingularMatrix_Throws()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Action act = () => m.Inverse();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cofactor_Identity()
    {
        var c = Matrix3x3.Identity.Cofactor();
        c.Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void Cofactor_KnownMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        var c = m.Cofactor();
        c.M00.Should().Be(50 - 48);
        c.M01.Should().Be(-(40 - 42));
        c.M02.Should().Be(32 - 35);
    }

    [Fact]
    public void Adjugate_KnownMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var adj = m.Adjugate();
        var result = adj * m;
        double det = m.Determinant;
        result.M00.Should().BeApproximately(det, 1e-10);
        result.M11.Should().BeApproximately(det, 1e-10);
        result.M22.Should().BeApproximately(det, 1e-10);
        result.M01.Should().BeApproximately(0, 1e-10);
        result.M02.Should().BeApproximately(0, 1e-10);
        result.M10.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Adjugate_Identity()
    {
        Matrix3x3.Identity.Adjugate().Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void Scale_ByTwo()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var s = m.Scale(2);
        s.M00.Should().Be(2);
        s.M01.Should().Be(4);
        s.M02.Should().Be(6);
        s.M10.Should().Be(8);
        s.M11.Should().Be(10);
        s.M12.Should().Be(12);
        s.M20.Should().Be(14);
        s.M21.Should().Be(16);
        s.M22.Should().Be(18);
    }

    [Fact]
    public void Scale_ByZero_ReturnsZero()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Scale(0).Should().Be(Matrix3x3.Zero);
    }

    [Fact]
    public void Scale_ByOne_ReturnsOriginal()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Scale(1).Should().Be(m);
    }

    [Fact]
    public void Scale_Determinant()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        m.Scale(3).Determinant.Should().BeApproximately(m.Determinant * 27, 1e-10);
    }

    [Fact]
    public void Multiply_KnownMatrices()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(9, 8, 7, 6, 5, 4, 3, 2, 1);
        var result = a.Multiply(b);
        result.M00.Should().Be(1 * 9 + 2 * 6 + 3 * 3);
        result.M01.Should().Be(1 * 8 + 2 * 5 + 3 * 2);
        result.M02.Should().Be(1 * 7 + 2 * 4 + 3 * 1);
        result.M10.Should().Be(4 * 9 + 5 * 6 + 6 * 3);
        result.M11.Should().Be(4 * 8 + 5 * 5 + 6 * 2);
        result.M12.Should().Be(4 * 7 + 5 * 4 + 6 * 1);
        result.M20.Should().Be(7 * 9 + 8 * 6 + 9 * 3);
        result.M21.Should().Be(7 * 8 + 8 * 5 + 9 * 2);
        result.M22.Should().Be(7 * 7 + 8 * 4 + 9 * 1);
    }

    [Fact]
    public void Multiply_Determinant()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        var b = new Matrix3x3(2, 0, 1, 3, 1, 0, 1, 2, 3);
        (a * b).Determinant.Should().BeApproximately(a.Determinant * b.Determinant, 1e-10);
    }

    [Fact]
    public void Multiply_Associative()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(9, 8, 7, 6, 5, 4, 3, 2, 1);
        var c = new Matrix3x3(1, 0, 2, 0, 1, 0, 3, 0, 1);
        ((a * b) * c).Should().Be(a * (b * c));
    }

    [Fact]
    public void Transform_UnitX()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var v = Vector3D.UnitX;
        var result = m.Transform(v);
        result.X.Should().Be(1);
        result.Y.Should().Be(4);
        result.Z.Should().Be(7);
    }

    [Fact]
    public void Transform_UnitY()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var v = Vector3D.UnitY;
        var result = m.Transform(v);
        result.X.Should().Be(2);
        result.Y.Should().Be(5);
        result.Z.Should().Be(8);
    }

    [Fact]
    public void Transform_UnitZ()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var v = Vector3D.UnitZ;
        var result = m.Transform(v);
        result.X.Should().Be(3);
        result.Y.Should().Be(6);
        result.Z.Should().Be(9);
    }

    [Fact]
    public void Transform_Identity_ReturnsOriginal()
    {
        var v = new Vector3D(3, 5, 7);
        Matrix3x3.Identity.Transform(v).Should().Be(v);
    }

    [Fact]
    public void Transform_ZeroVector()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Transform(Vector3D.Zero).Should().Be(Vector3D.Zero);
    }

    [Fact]
    public void TransformPoint_UnitX()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var p = new Point3D(1, 0, 0);
        var result = m.TransformPoint(p);
        result.X.Should().Be(1);
        result.Y.Should().Be(4);
        result.Z.Should().Be(7);
    }

    [Fact]
    public void TransformPoint_Identity()
    {
        var p = new Point3D(3, 5, 7);
        Matrix3x3.Identity.TransformPoint(p).Should().Be(p);
    }

    [Fact]
    public void TransformPoint_Origin()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = m.TransformPoint(Point3D.Origin);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
        result.Z.Should().Be(0);
    }

    [Fact]
    public void FromTransform_Identity()
    {
        var m3 = Matrix3x3.FromTransform(Transform3D.Identity);
        m3.Should().Be(Matrix3x3.Identity);
    }

    [Fact]
    public void FromTransform_RotationX()
    {
        var t = Transform3D.RotationX(PI / 2);
        var m3 = Matrix3x3.FromTransform(t);
        m3.M00.Should().BeApproximately(1, 1e-10);
        m3.M01.Should().BeApproximately(0, 1e-10);
        m3.M02.Should().BeApproximately(0, 1e-10);
        m3.M10.Should().BeApproximately(0, 1e-10);
        m3.M11.Should().BeApproximately(0, 1e-10);
        m3.M12.Should().BeApproximately(-1, 1e-10);
        m3.M20.Should().BeApproximately(0, 1e-10);
        m3.M21.Should().BeApproximately(1, 1e-10);
        m3.M22.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void FromTransform_Scaling()
    {
        var t = Transform3D.Scaling(2, 3, 4);
        var m3 = Matrix3x3.FromTransform(t);
        m3.M00.Should().Be(2);
        m3.M11.Should().Be(3);
        m3.M22.Should().Be(4);
        m3.M01.Should().Be(0);
        m3.M02.Should().Be(0);
        m3.M10.Should().Be(0);
        m3.M12.Should().Be(0);
        m3.M20.Should().Be(0);
        m3.M21.Should().Be(0);
    }

    [Fact]
    public void RotationAxis_ZAxis_90Degrees()
    {
        var m = Matrix3x3.RotationAxis(Vector3D.UnitZ, PI / 2);
        var v = Vector3D.UnitX;
        var result = m.Transform(v);
        result.X.Should().BeApproximately(0, 1e-10);
        result.Y.Should().BeApproximately(1, 1e-10);
        result.Z.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void RotationAxis_XAxis_90Degrees()
    {
        var m = Matrix3x3.RotationAxis(Vector3D.UnitX, PI / 2);
        var v = Vector3D.UnitY;
        var result = m.Transform(v);
        result.X.Should().BeApproximately(0, 1e-10);
        result.Y.Should().BeApproximately(0, 1e-10);
        result.Z.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void RotationAxis_Determinant_IsOne()
    {
        var m = Matrix3x3.RotationAxis(new Vector3D(1, 1, 1), PI / 4);
        m.Determinant.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void RotationAxis_360Degrees_ReturnsIdentity()
    {
        var m = Matrix3x3.RotationAxis(Vector3D.UnitZ, 2 * PI);
        m.M00.Should().BeApproximately(1, 1e-10);
        m.M01.Should().BeApproximately(0, 1e-10);
        m.M02.Should().BeApproximately(0, 1e-10);
        m.M10.Should().BeApproximately(0, 1e-10);
        m.M11.Should().BeApproximately(1, 1e-10);
        m.M12.Should().BeApproximately(0, 1e-10);
        m.M20.Should().BeApproximately(0, 1e-10);
        m.M21.Should().BeApproximately(0, 1e-10);
        m.M22.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void RotationAxis_PreservesLength()
    {
        var v = new Vector3D(1, 2, 3);
        var m = Matrix3x3.RotationAxis(new Vector3D(1, 1, 0), PI / 3);
        var rotated = m.Transform(v);
        rotated.Length.Should().BeApproximately(v.Length, 1e-10);
    }

    [Fact]
    public void Scaling_DiagonalMatrix()
    {
        var m = Matrix3x3.Scaling(2, 3, 4);
        m.M00.Should().Be(2);
        m.M11.Should().Be(3);
        m.M22.Should().Be(4);
        m.M01.Should().Be(0);
        m.M02.Should().Be(0);
        m.M10.Should().Be(0);
        m.M12.Should().Be(0);
        m.M20.Should().Be(0);
        m.M21.Should().Be(0);
    }

    [Fact]
    public void Scaling_Determinant()
    {
        var m = Matrix3x3.Scaling(2, 3, 4);
        m.Determinant.Should().Be(24);
    }

    [Fact]
    public void Scaling_TransformVector()
    {
        var m = Matrix3x3.Scaling(2, 3, 4);
        var v = new Vector3D(1, 1, 1);
        var result = m.Transform(v);
        result.X.Should().Be(2);
        result.Y.Should().Be(3);
        result.Z.Should().Be(4);
    }

    [Fact]
    public void Solve_KnownSystem()
    {
        var m = new Matrix3x3(2, 1, 0, 1, 3, 1, 0, 1, 4);
        var b = new Vector3D(5, 7, 6);
        var x = m.Solve(b);
        var result = m.Transform(x);
        result.X.Should().BeApproximately(b.X, 1e-10);
        result.Y.Should().BeApproximately(b.Y, 1e-10);
        result.Z.Should().BeApproximately(b.Z, 1e-10);
    }

    [Fact]
    public void Solve_IdentitySystem()
    {
        var b = new Vector3D(3, 4, 5);
        var x = Matrix3x3.Identity.Solve(b);
        x.Should().Be(b);
    }

    [Fact]
    public void Solve_SingularSystem_Throws()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Action act = () => m.Solve(new Vector3D(1, 2, 3));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Solve_ATimesXEqualsB()
    {
        var m = new Matrix3x3(1, 0, 2, 0, 1, 0, 3, 0, 1);
        var b = new Vector3D(8, 5, 10);
        var x = m.Solve(b);
        m.Transform(x).X.Should().BeApproximately(8, 1e-10);
        m.Transform(x).Y.Should().BeApproximately(5, 1e-10);
        m.Transform(x).Z.Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void Trace_Identity_IsThree()
    {
        Matrix3x3.Identity.Trace.Should().Be(3);
    }

    [Fact]
    public void Trace_KnownMatrix()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Trace.Should().Be(15);
    }

    [Fact]
    public void Trace_ZeroMatrix_IsZero()
    {
        Matrix3x3.Zero.Trace.Should().Be(0);
    }

    [Fact]
    public void Trace_Diagonal()
    {
        var m = new Matrix3x3(5, 0, 0, 0, 7, 0, 0, 0, 11);
        m.Trace.Should().Be(23);
    }

    [Fact]
    public void Trace_Transpose_EqualsTrace()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m.Trace.Should().Be(m.Transpose().Trace);
    }

    [Fact]
    public void OperatorScalarMultiply_Right()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = m * 2.0;
        result.M00.Should().Be(2);
        result.M01.Should().Be(4);
        result.M02.Should().Be(6);
        result.M10.Should().Be(8);
        result.M11.Should().Be(10);
        result.M12.Should().Be(12);
        result.M20.Should().Be(14);
        result.M21.Should().Be(16);
        result.M22.Should().Be(18);
    }

    [Fact]
    public void OperatorScalarMultiply_Left()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = 2.0 * m;
        result.M00.Should().Be(2);
        result.M01.Should().Be(4);
        result.M02.Should().Be(6);
        result.M10.Should().Be(8);
        result.M11.Should().Be(10);
        result.M12.Should().Be(12);
        result.M20.Should().Be(14);
        result.M21.Should().Be(16);
        result.M22.Should().Be(18);
    }

    [Fact]
    public void OperatorMatrixMultiply()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(9, 8, 7, 6, 5, 4, 3, 2, 1);
        (a * b).Should().Be(a.Multiply(b));
    }

    [Fact]
    public void Indexer_AllElements()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        m[0, 0].Should().Be(1);
        m[0, 1].Should().Be(2);
        m[0, 2].Should().Be(3);
        m[1, 0].Should().Be(4);
        m[1, 1].Should().Be(5);
        m[1, 2].Should().Be(6);
        m[2, 0].Should().Be(7);
        m[2, 1].Should().Be(8);
        m[2, 2].Should().Be(9);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Action act = () => _ = m[3, 0];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void ZeroMatrix_AllElementsZero()
    {
        Matrix3x3.Zero.M00.Should().Be(0);
        Matrix3x3.Zero.M01.Should().Be(0);
        Matrix3x3.Zero.M02.Should().Be(0);
        Matrix3x3.Zero.M10.Should().Be(0);
        Matrix3x3.Zero.M11.Should().Be(0);
        Matrix3x3.Zero.M12.Should().Be(0);
        Matrix3x3.Zero.M20.Should().Be(0);
        Matrix3x3.Zero.M21.Should().Be(0);
        Matrix3x3.Zero.M22.Should().Be(0);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        a.Should().NotBe(b);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var m = new Matrix3x3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        string s = m.ToString();
        s.Should().Contain("1");
        s.Should().Contain("5");
        s.Should().Contain("9");
    }

    [Fact]
    public void Solve_ConsistentWithInverse()
    {
        var m = new Matrix3x3(2, 1, 0, 1, 3, 1, 0, 1, 4);
        var b = new Vector3D(5, 7, 6);
        var x = m.Solve(b);
        var x2 = m.Inverse().Transform(b);
        x.X.Should().BeApproximately(x2.X, 1e-10);
        x.Y.Should().BeApproximately(x2.Y, 1e-10);
        x.Z.Should().BeApproximately(x2.Z, 1e-10);
    }
}
