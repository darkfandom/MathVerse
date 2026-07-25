namespace MathVerse.Geometry.Advanced.Tests.Geometry3D_New;

public class Direction3DTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Forward_IsCorrect()
    {
        Direction3D.Forward.X.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Forward.Y.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Forward.Z.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Backward_IsCorrect()
    {
        Direction3D.Backward.X.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Backward.Y.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Backward.Z.Should().BeApproximately(-1.0, Tolerance);
    }

    [Fact]
    public void Up_IsCorrect()
    {
        Direction3D.Up.X.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Up.Y.Should().BeApproximately(1.0, Tolerance);
        Direction3D.Up.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Down_IsCorrect()
    {
        Direction3D.Down.X.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Down.Y.Should().BeApproximately(-1.0, Tolerance);
        Direction3D.Down.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Right_IsCorrect()
    {
        Direction3D.Right.X.Should().BeApproximately(1.0, Tolerance);
        Direction3D.Right.Y.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Right.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Left_IsCorrect()
    {
        Direction3D.Left.X.Should().BeApproximately(-1.0, Tolerance);
        Direction3D.Left.Y.Should().BeApproximately(0.0, Tolerance);
        Direction3D.Left.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromVector_Normalizes()
    {
        var v = new Vector3D(3, 0, 0);
        var dir = Direction3D.FromVector(v);

        dir.X.Should().BeApproximately(1.0, Tolerance);
        dir.Y.Should().BeApproximately(0.0, Tolerance);
        dir.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromVector_AlreadyNormalized()
    {
        var v = new Vector3D(0, 1, 0);
        var dir = Direction3D.FromVector(v);

        dir.Should().Be(Direction3D.Up);
    }

    [Fact]
    public void FromVector_NonUnitVector()
    {
        var v = new Vector3D(0, 5, 0);
        var dir = Direction3D.FromVector(v);

        dir.X.Should().BeApproximately(0.0, Tolerance);
        dir.Y.Should().BeApproximately(1.0, Tolerance);
        dir.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromPoints_GivesCorrectDirection()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(1, 0, 0);
        var dir = Direction3D.FromPoints(a, b);

        dir.Should().Be(Direction3D.Right);
    }

    [Fact]
    public void FromPoints_Reversed()
    {
        var a = new Point3D(1, 0, 0);
        var b = new Point3D(0, 0, 0);
        var dir = Direction3D.FromPoints(a, b);

        dir.Should().Be(Direction3D.Left);
    }

    [Fact]
    public void FromPoints_Diagonal()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(1, 1, 0);
        var dir = Direction3D.FromPoints(a, b);

        double expected = 1.0 / System.Math.Sqrt(2.0);
        dir.X.Should().BeApproximately(expected, Tolerance);
        dir.Y.Should().BeApproximately(expected, Tolerance);
        dir.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void AngleTo_SameDirection_IsZero()
    {
        double angle = Direction3D.Forward.AngleTo(Direction3D.Forward);

        angle.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void AngleTo_OppositeDirection_IsPI()
    {
        double angle = Direction3D.Forward.AngleTo(Direction3D.Backward);

        angle.Should().BeApproximately(System.Math.PI, Tolerance);
    }

    [Fact]
    public void AngleTo_Perpendicular_IsHalfPI()
    {
        double angle = Direction3D.Forward.AngleTo(Direction3D.Right);

        angle.Should().BeApproximately(System.Math.PI / 2.0, Tolerance);
    }

    [Fact]
    public void Dot_SameDirection_IsOne()
    {
        Direction3D.Forward.Dot(Direction3D.Forward).Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Dot_OppositeDirection_IsMinusOne()
    {
        Direction3D.Forward.Dot(Direction3D.Backward).Should().BeApproximately(-1.0, Tolerance);
    }

    [Fact]
    public void Dot_Perpendicular_IsZero()
    {
        Direction3D.Forward.Dot(Direction3D.Right).Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Dot_UpRight_IsZero()
    {
        Direction3D.Up.Dot(Direction3D.Right).Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Cross_ForwardRight_IsUp()
    {
        var cross = Direction3D.Forward.Cross(Direction3D.Right);

        cross.X.Should().BeApproximately(0.0, Tolerance);
        cross.Y.Should().BeApproximately(1.0, Tolerance);
        cross.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Cross_Parallel_IsZero()
    {
        var cross = Direction3D.Forward.Cross(Direction3D.Forward);

        cross.Length.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Negate_ForwardBecomesBackward()
    {
        Direction3D.Forward.Negate().Should().Be(Direction3D.Backward);
    }

    [Fact]
    public void Negate_RightBecomesLeft()
    {
        Direction3D.Right.Negate().Should().Be(Direction3D.Left);
    }

    [Fact]
    public void Negate_UpBecomesDown()
    {
        Direction3D.Up.Negate().Should().Be(Direction3D.Down);
    }

    [Fact]
    public void Negate_TwiceReturnsOriginal()
    {
        Direction3D.Forward.Negate().Negate().Should().Be(Direction3D.Forward);
    }

    [Fact]
    public void Lerp_AtZero_ReturnsFirst()
    {
        var result = Direction3D.Forward.Lerp(Direction3D.Right, 0.0);

        result.Should().Be(Direction3D.Forward);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsSecond()
    {
        var result = Direction3D.Forward.Lerp(Direction3D.Right, 1.0);

        result.X.Should().BeApproximately(Direction3D.Right.X, Tolerance);
        result.Y.Should().BeApproximately(Direction3D.Right.Y, Tolerance);
        result.Z.Should().BeApproximately(Direction3D.Right.Z, Tolerance);
    }

    [Fact]
    public void Lerp_AtHalf_IsMidpoint()
    {
        var result = Direction3D.Forward.Lerp(Direction3D.Up, 0.5);

        double expected = 1.0 / System.Math.Sqrt(2.0);
        result.Z.Should().BeApproximately(expected, Tolerance);
        result.Y.Should().BeApproximately(expected, Tolerance);
    }

    [Fact]
    public void Lerp_ResultIsNormalized()
    {
        var result = Direction3D.Forward.Lerp(Direction3D.Up, 0.3);

        double len = System.Math.Sqrt(result.X * result.X + result.Y * result.Y + result.Z * result.Z);
        len.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void ToVector3D_ReturnsCorrectVector()
    {
        var dir = Direction3D.Forward;
        var v = dir.ToVector3D();

        v.X.Should().BeApproximately(0.0, Tolerance);
        v.Y.Should().BeApproximately(0.0, Tolerance);
        v.Z.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void ToVector3D_LengthIsOne()
    {
        var v = Direction3D.Up.ToVector3D();

        v.Length.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Indexer_Zero_IsX()
    {
        Direction3D.Right[0].Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Indexer_One_IsY()
    {
        Direction3D.Up[1].Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Indexer_Two_IsZ()
    {
        Direction3D.Forward[2].Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Indexer_InvalidIndex_Throws()
    {
        Action act = () => { var _ = Direction3D.Forward[3]; };

        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void AngleTo_IsSymmetric()
    {
        double a1 = Direction3D.Forward.AngleTo(Direction3D.Up);
        double a2 = Direction3D.Up.AngleTo(Direction3D.Forward);

        a1.Should().BeApproximately(a2, Tolerance);
    }

    [Fact]
    public void Dot_IsCommutative()
    {
        double d1 = Direction3D.Forward.Dot(Direction3D.Up);
        double d2 = Direction3D.Up.Dot(Direction3D.Forward);

        d1.Should().BeApproximately(d2, Tolerance);
    }
}
