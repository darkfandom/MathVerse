using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class GuardTests
{
    [Fact]
    public void NotNull_WithValue_ReturnsValue()
    {
        var result = Guard.NotNull("hello", "param");

        result.Should().Be("hello");
    }

    [Fact]
    public void NotNull_WithNull_ThrowsArgumentNullException()
    {
        var act = () => Guard.NotNull<string>(null, "param");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotNullOrEmpty_WithValue_ReturnsValue()
    {
        var result = Guard.NotNullOrEmpty("hello", "param");

        result.Should().Be("hello");
    }

    [Fact]
    public void NotNullOrEmpty_WithNull_Throws()
    {
        var act = () => Guard.NotNullOrEmpty(null, "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotNullOrEmpty_WithEmpty_Throws()
    {
        var act = () => Guard.NotNullOrEmpty("", "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotNullOrWhiteSpace_WithValue_ReturnsValue()
    {
        var result = Guard.NotNullOrWhiteSpace("hello", "param");

        result.Should().Be("hello");
    }

    [Fact]
    public void NotNullOrWhiteSpace_WithWhitespace_Throws()
    {
        var act = () => Guard.NotNullOrWhiteSpace("   ", "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotDefault_WithNonDefault_ReturnsValue()
    {
        var result = Guard.NotDefault(42, "param");

        result.Should().Be(42);
    }

    [Fact]
    public void NotDefault_WithDefault_Throws()
    {
        var act = () => Guard.NotDefault(0, "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotNullOrEmpty_Collection_WithItems_ReturnsValue()
    {
        IReadOnlyCollection<int> list = new List<int> { 1, 2, 3 };
        var result = Guard.NotNullOrEmpty(list, "param");

        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public void NotNullOrEmpty_Collection_Empty_Throws()
    {
        IReadOnlyCollection<int> list = new List<int>();
        var act = () => Guard.NotNullOrEmpty(list, "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Satisfies_WhenConditionMet_ReturnsValue()
    {
        var result = Guard.Satisfies(10, x => x > 0, "param");

        result.Should().Be(10);
    }

    [Fact]
    public void Satisfies_WhenConditionFails_Throws()
    {
        var act = () => Guard.Satisfies(-1, x => x > 0, "param");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GreaterThan_WhenGreaterThan_ReturnsValue()
    {
        var result = Guard.GreaterThan(10, 5, "param");

        result.Should().Be(10);
    }

    [Fact]
    public void GreaterThan_WhenEqualTo_Throws()
    {
        var act = () => Guard.GreaterThan(5, 5, "param");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GreaterThanOrEqualTo_WhenEqual_ReturnsValue()
    {
        var result = Guard.GreaterThanOrEqualTo(5, 5, "param");

        result.Should().Be(5);
    }

    [Fact]
    public void LessThan_WhenLessThan_ReturnsValue()
    {
        var result = Guard.LessThan(3, 5, "param");

        result.Should().Be(3);
    }

    [Fact]
    public void LessThan_WhenEqualTo_Throws()
    {
        var act = () => Guard.LessThan(5, 5, "param");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LessThanOrEqualTo_WhenEqual_ReturnsValue()
    {
        var result = Guard.LessThanOrEqualTo(5, 5, "param");

        result.Should().Be(5);
    }

    [Fact]
    public void Between_InRange_ReturnsValue()
    {
        var result = Guard.Between(5, 1, 10, "param");

        result.Should().Be(5);
    }

    [Fact]
    public void Between_OutOfRange_Throws()
    {
        var act = () => Guard.Between(15, 1, 10, "param");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void OneOf_WhenValid_ReturnsValue()
    {
        var result = Guard.OneOf("a", new[] { "a", "b", "c" }, "param");

        result.Should().Be("a");
    }

    [Fact]
    public void OneOf_WhenInvalid_Throws()
    {
        var act = () => Guard.OneOf("d", new[] { "a", "b", "c" }, "param");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
