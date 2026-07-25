namespace MathVerse.Geometry.Tests.Picking;

/// <summary>Tests for the <see cref="HitTestResult"/> record.</summary>
public class HitTestResultTests
{
    /// <summary>Verifies that Miss() returns Hit=false.</summary>
    [Fact]
    public void Miss_ReturnsHitFalse()
    {
        var result = HitTestResult.Miss();

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that Miss() returns Distance=MaxValue.</summary>
    [Fact]
    public void Miss_ReturnsMaxDistance()
    {
        var result = HitTestResult.Miss();

        result.Distance.Should().Be(double.MaxValue);
    }

    /// <summary>Verifies that Miss() returns default HitPoint.</summary>
    [Fact]
    public void Miss_ReturnsDefaultHitPoint()
    {
        var result = HitTestResult.Miss();

        result.HitPoint.Should().Be(default(Point3D));
    }

    /// <summary>Verifies that Miss() returns default Normal.</summary>
    [Fact]
    public void Miss_ReturnsDefaultNormal()
    {
        var result = HitTestResult.Miss();

        result.Normal.Should().Be(default(Vector3D));
    }

    /// <summary>Verifies that a hit result has Hit=true.</summary>
    [Fact]
    public void Hit_HasHitTrue()
    {
        var result = new HitTestResult { Hit = true, Distance = 5.0 };

        result.Hit.Should().BeTrue();
    }

    /// <summary>Verifies that a hit result stores the correct distance.</summary>
    [Fact]
    public void Hit_StoresDistance()
    {
        var result = new HitTestResult { Hit = true, Distance = 3.14 };

        result.Distance.Should().BeApproximately(3.14, 1e-10);
    }

    /// <summary>Verifies that a hit result stores the correct hit point.</summary>
    [Fact]
    public void Hit_StoresHitPoint()
    {
        var point = new Point3D(1, 2, 3);
        var result = new HitTestResult { Hit = true, HitPoint = point };

        result.HitPoint.Should().Be(point);
    }

    /// <summary>Verifies that a hit result stores the correct triangle index.</summary>
    [Fact]
    public void Hit_StoresTriangleIndex()
    {
        var result = new HitTestResult { Hit = true, TriangleIndex = 42 };

        result.TriangleIndex.Should().Be(42);
    }
}
