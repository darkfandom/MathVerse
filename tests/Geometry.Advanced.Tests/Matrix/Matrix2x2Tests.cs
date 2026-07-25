namespace MathVerse.Geometry.Advanced.Tests.Matrix;

public class Matrix2x2Tests
{
    [Fact]
    public void Identity_M00_IsOne()
    {
        Matrix2x2.Identity.M00.Should().Be(1);
    }

    [Fact]
    public void Identity_M01_IsZero()
    {
        Matrix2x2.Identity.M01.Should().Be(0);
    }

    [Fact]
    public void Identity_M10_IsZero()
    {
        Matrix2x2.Identity.M10.Should().Be(0);
    }

    [Fact]
    public void Identity_M11_IsOne()
    {
        Matrix2x2.Identity.M11.Should().Be(1);
    }

    [Fact]
    public void IdentityTimesIdentity_EqualsIdentity()
    {
        var result = Matrix2x2.Identity * Matrix2x2.Identity;
        result.Should().Be(Matrix2x2.Identity);
    }

    [Fact]
    public void IdentityTimesMatrix_EqualsMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        (Matrix2x2.Identity * m).Should().Be(m);
    }

    [Fact]
    public void MatrixTimesIdentity_EqualsMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        (m * Matrix2x2.Identity).Should().Be(m);
    }

    [Fact]
    public void Determinant_Identity_IsOne()
    {
        Matrix2x2.Identity.Determinant.Should().Be(1);
    }

    [Fact]
    public void Determinant_KnownMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Determinant.Should().Be(-2);
    }

    [Fact]
    public void Determinant_Diagonal()
    {
        var m = new Matrix2x2(2, 0, 0, 3);
        m.Determinant.Should().Be(6);
    }

    [Fact]
    public void Determinant_ZeroMatrix_IsZero()
    {
        Matrix2x2.Zero.Determinant.Should().Be(0);
    }

    [Fact]
    public void Determinant_SingleRow()
    {
        var m = new Matrix2x2(5, 6, 7, 8);
        m.Determinant.Should().Be(5 * 8 - 6 * 7);
    }

    [Fact]
    public void Transpose_Identity()
    {
        Matrix2x2.Identity.Transpose().Should().Be(Matrix2x2.Identity);
    }

    [Fact]
    public void Transpose_KnownMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var t = m.Transpose();
        t.M00.Should().Be(1);
        t.M01.Should().Be(3);
        t.M10.Should().Be(2);
        t.M11.Should().Be(4);
    }

    [Fact]
    public void Transpose_Twice_EqualsOriginal()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Transpose().Transpose().Should().Be(m);
    }

    [Fact]
    public void Transpose_SymmetricMatrix()
    {
        var m = new Matrix2x2(1, 2, 2, 1);
        m.Transpose().Should().Be(m);
    }

    [Fact]
    public void Inverse_Identity()
    {
        Matrix2x2.Identity.Inverse().Should().Be(Matrix2x2.Identity);
    }

    [Fact]
    public void Inverse_Twice_EqualsOriginal()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Inverse().Inverse().Should().Be(m);
    }

    [Fact]
    public void Inverse_TimesOriginal_EqualsIdentity()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var result = m * m.Inverse();
        result.M00.Should().BeApproximately(1, 1e-10);
        result.M01.Should().BeApproximately(0, 1e-10);
        result.M10.Should().BeApproximately(0, 1e-10);
        result.M11.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Inverse_OriginalTimesInverse_EqualsIdentity()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var result = m.Inverse() * m;
        result.M00.Should().BeApproximately(1, 1e-10);
        result.M01.Should().BeApproximately(0, 1e-10);
        result.M10.Should().BeApproximately(0, 1e-10);
        result.M11.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Inverse_KnownMatrix()
    {
        var m = new Matrix2x2(4, 7, 2, 6);
        var inv = m.Inverse();
        inv.M00.Should().BeApproximately(0.6, 1e-10);
        inv.M01.Should().BeApproximately(-0.7, 1e-10);
        inv.M10.Should().BeApproximately(-0.2, 1e-10);
        inv.M11.Should().BeApproximately(0.4, 1e-10);
    }

    [Fact]
    public void Inverse_SingularMatrix_Throws()
    {
        var m = new Matrix2x2(1, 2, 2, 4);
        Action act = () => m.Inverse();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inverse_NearlySingular_Throws()
    {
        var m = new Matrix2x2(1, 2, 2, 4 + 1e-16);
        Action act = () => m.Inverse();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cofactor_KnownMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var c = m.Cofactor();
        c.M00.Should().Be(4);
        c.M01.Should().Be(-3);
        c.M10.Should().Be(-2);
        c.M11.Should().Be(1);
    }

    [Fact]
    public void Cofactor_Identity()
    {
        var c = Matrix2x2.Identity.Cofactor();
        c.Should().Be(Matrix2x2.Identity);
    }

    [Fact]
    public void Adjugate_KnownMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var adj = m.Adjugate();
        adj.M00.Should().Be(4);
        adj.M01.Should().Be(-2);
        adj.M10.Should().Be(-3);
        adj.M11.Should().Be(1);
    }

    [Fact]
    public void Adjugate_Identity()
    {
        Matrix2x2.Identity.Adjugate().Should().Be(Matrix2x2.Identity);
    }

    [Fact]
    public void Adjugate_TimesMatrix_EqualsDeterminantTimesIdentity()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var adjTimesM = m.Adjugate() * m;
        double det = m.Determinant;
        adjTimesM.M00.Should().BeApproximately(det, 1e-10);
        adjTimesM.M01.Should().BeApproximately(0, 1e-10);
        adjTimesM.M10.Should().BeApproximately(0, 1e-10);
        adjTimesM.M11.Should().BeApproximately(det, 1e-10);
    }

    [Fact]
    public void Scale_ByTwo()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var s = m.Scale(2);
        s.M00.Should().Be(2);
        s.M01.Should().Be(4);
        s.M10.Should().Be(6);
        s.M11.Should().Be(8);
    }

    [Fact]
    public void Scale_ByZero_ReturnsZero()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Scale(0).Should().Be(Matrix2x2.Zero);
    }

    [Fact]
    public void Scale_ByOne_ReturnsOriginal()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Scale(1).Should().Be(m);
    }

    [Fact]
    public void Scale_Determinant()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Scale(3).Determinant.Should().BeApproximately(m.Determinant * 9, 1e-10);
    }

    [Fact]
    public void Multiply_KnownMatrices()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(5, 6, 7, 8);
        var result = a.Multiply(b);
        result.M00.Should().Be(19);
        result.M01.Should().Be(22);
        result.M10.Should().Be(43);
        result.M11.Should().Be(50);
    }

    [Fact]
    public void Multiply_Determinant()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(5, 6, 7, 8);
        (a * b).Determinant.Should().BeApproximately(a.Determinant * b.Determinant, 1e-10);
    }

    [Fact]
    public void Multiply_Associative()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(5, 6, 7, 8);
        var c = new Matrix2x2(9, 10, 11, 12);
        ((a * b) * c).Should().Be(a * (b * c));
    }

    [Fact]
    public void Transform_UnitX()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var v = Vector2D.UnitX;
        var result = m.Transform(v);
        result.X.Should().Be(1);
        result.Y.Should().Be(3);
    }

    [Fact]
    public void Transform_UnitY()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var v = Vector2D.UnitY;
        var result = m.Transform(v);
        result.X.Should().Be(2);
        result.Y.Should().Be(4);
    }

    [Fact]
    public void Transform_Identity_ReturnsOriginal()
    {
        var v = new Vector2D(3, 5);
        Matrix2x2.Identity.Transform(v).Should().Be(v);
    }

    [Fact]
    public void Transform_ZeroVector()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Transform(Vector2D.Zero).Should().Be(Vector2D.Zero);
    }

    [Fact]
    public void Solve_KnownSystem()
    {
        var m = new Matrix2x2(2, 1, 1, 3);
        var (x, y) = m.Solve(5, 7);
        x.Should().BeApproximately(8.0 / 5.0, 1e-10);
        y.Should().BeApproximately(9.0 / 5.0, 1e-10);
    }

    [Fact]
    public void Solve_IdentitySystem()
    {
        var (x, y) = Matrix2x2.Identity.Solve(3, 4);
        x.Should().BeApproximately(3, 1e-10);
        y.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Solve_SingularSystem_Throws()
    {
        var m = new Matrix2x2(1, 2, 2, 4);
        Action act = () => m.Solve(1, 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Solve_ATimesResultEqualsB()
    {
        var m = new Matrix2x2(3, 1, 1, 2);
        double bx = 5, by = 7;
        var (x, y) = m.Solve(bx, by);
        (m.M00 * x + m.M01 * y).Should().BeApproximately(bx, 1e-10);
        (m.M10 * x + m.M11 * y).Should().BeApproximately(by, 1e-10);
    }

    [Fact]
    public void Trace_Identity_IsTwo()
    {
        Matrix2x2.Identity.Trace.Should().Be(2);
    }

    [Fact]
    public void Trace_KnownMatrix()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Trace.Should().Be(5);
    }

    [Fact]
    public void Trace_ZeroMatrix_IsZero()
    {
        Matrix2x2.Zero.Trace.Should().Be(0);
    }

    [Fact]
    public void Trace_Diagonal()
    {
        var m = new Matrix2x2(5, 0, 0, 7);
        m.Trace.Should().Be(12);
    }

    [Fact]
    public void Trace_Transpose_EqualsTrace()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m.Trace.Should().Be(m.Transpose().Trace);
    }

    [Fact]
    public void OperatorScalarMultiply_Right()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var result = m * 2.0;
        result.M00.Should().Be(2);
        result.M01.Should().Be(4);
        result.M10.Should().Be(6);
        result.M11.Should().Be(8);
    }

    [Fact]
    public void OperatorScalarMultiply_Left()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        var result = 2.0 * m;
        result.M00.Should().Be(2);
        result.M01.Should().Be(4);
        result.M10.Should().Be(6);
        result.M11.Should().Be(8);
    }

    [Fact]
    public void OperatorMatrixMultiply()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(5, 6, 7, 8);
        (a * b).Should().Be(a.Multiply(b));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        string s = m.ToString();
        s.Should().Contain("1");
        s.Should().Contain("2");
        s.Should().Contain("3");
        s.Should().Contain("4");
    }

    [Fact]
    public void Indexer_AllElements()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        m[0, 0].Should().Be(1);
        m[0, 1].Should().Be(2);
        m[1, 0].Should().Be(3);
        m[1, 1].Should().Be(4);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var m = new Matrix2x2(1, 2, 3, 4);
        Action act = () => _ = m[2, 0];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void ZeroMatrix_AllElementsZero()
    {
        Matrix2x2.Zero.M00.Should().Be(0);
        Matrix2x2.Zero.M01.Should().Be(0);
        Matrix2x2.Zero.M10.Should().Be(0);
        Matrix2x2.Zero.M11.Should().Be(0);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(1, 2, 3, 4);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(1, 2, 3, 5);
        a.Should().NotBe(b);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new Matrix2x2(1, 2, 3, 4);
        var b = new Matrix2x2(1, 2, 3, 4);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Inverse_SolveConsistency()
    {
        var m = new Matrix2x2(2, 1, 1, 3);
        var inv = m.Inverse();
        var (x, y) = m.Solve(5, 7);
        var v = inv.Transform(new Vector2D(5, 7));
        v.X.Should().BeApproximately(x, 1e-10);
        v.Y.Should().BeApproximately(y, 1e-10);
    }
}
