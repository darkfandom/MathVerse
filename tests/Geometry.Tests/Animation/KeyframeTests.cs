namespace MathVerse.Geometry.Tests.Animation;

/// <summary>Tests for the <see cref="Keyframe"/> struct.</summary>
public class KeyframeTests
{
    /// <summary>Verifies that Keyframe stores the correct time value.</summary>
    [Fact]
    public void Construction_StoresTime()
    {
        var kf = new Keyframe(1.5, 10.0);

        kf.Time.Should().BeApproximately(1.5, 1e-10);
    }

    /// <summary>Verifies that Keyframe stores the correct value.</summary>
    [Fact]
    public void Construction_StoresValue()
    {
        var kf = new Keyframe(1.5, 10.0);

        kf.Value.Should().BeApproximately(10.0, 1e-10);
    }

    /// <summary>Verifies that two keyframes with same time and value are equal.</summary>
    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var a = new Keyframe(2.0, 5.0);
        var b = new Keyframe(2.0, 5.0);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    /// <summary>Verifies that two keyframes with different time are not equal.</summary>
    [Fact]
    public void Equality_DifferentTime_ReturnsFalse()
    {
        var a = new Keyframe(1.0, 5.0);
        var b = new Keyframe(2.0, 5.0);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>Verifies that two keyframes with different value are not equal.</summary>
    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        var a = new Keyframe(1.0, 5.0);
        var b = new Keyframe(1.0, 6.0);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>Verifies that equal keyframes have the same hash code.</summary>
    [Fact]
    public void Equality_SameValues_SameHashCode()
    {
        var a = new Keyframe(3.0, 7.0);
        var b = new Keyframe(3.0, 7.0);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
