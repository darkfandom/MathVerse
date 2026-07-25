namespace MathVerse.Math.Core.Tests;

public class IntervalTests
{
    [Fact]
    public void FromBounds_NormalBounds_CreatesCorrectInterval()
    {
        var interval = Interval.FromBounds(2.0, 5.0);
        interval.Lower.Should().Be(2.0);
        interval.Upper.Should().Be(5.0);
        interval.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void FromBounds_ReversedBounds_CreatesEmptyInterval()
    {
        var interval = Interval.FromBounds(5.0, 2.0);
        interval.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromPoint_CreatesPointInterval()
    {
        var interval = Interval.FromPoint(3.0);
        interval.Lower.Should().Be(3.0);
        interval.Upper.Should().Be(3.0);
        interval.IsPoint.Should().BeTrue();
    }

    [Fact]
    public void Empty_IsEmpty()
    {
        var empty = Interval.Empty;
        empty.IsEmpty.Should().BeTrue();
        empty.Length.Should().Be(0.0);
    }

    [Fact]
    public void Length_NormalInterval_ReturnsCorrectValue()
    {
        var interval = Interval.FromBounds(2.0, 7.0);
        interval.Length.Should().Be(5.0);
    }

    [Fact]
    public void Length_EmptyInterval_ReturnsZero()
    {
        Interval.Empty.Length.Should().Be(0.0);
    }

    [Fact]
    public void Mid_NormalInterval_ReturnsMidpoint()
    {
        var interval = Interval.FromBounds(2.0, 8.0);
        interval.Mid.Should().Be(5.0);
    }

    [Fact]
    public void Mid_EmptyInterval_ReturnsNaN()
    {
        double.IsNaN(Interval.Empty.Mid).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var interval = Interval.FromBounds(1.0, 5.0);
        interval.Contains(3.0).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOnLowerBoundary_ReturnsTrue()
    {
        var interval = Interval.FromBounds(1.0, 5.0);
        interval.Contains(1.0).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOnUpperBoundary_ReturnsTrue()
    {
        var interval = Interval.FromBounds(1.0, 5.0);
        interval.Contains(5.0).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var interval = Interval.FromBounds(1.0, 5.0);
        interval.Contains(6.0).Should().BeFalse();
    }

    [Fact]
    public void Contains_EmptyInterval_ReturnsFalse()
    {
        Interval.Empty.Contains(3.0).Should().BeFalse();
    }

    [Fact]
    public void Contains_IntervalFullyContained_ReturnsTrue()
    {
        var outer = Interval.FromBounds(0.0, 10.0);
        var inner = Interval.FromBounds(2.0, 5.0);
        outer.Contains(inner).Should().BeTrue();
    }

    [Fact]
    public void Contains_IntervalNotContained_ReturnsFalse()
    {
        var outer = Interval.FromBounds(0.0, 10.0);
        var other = Interval.FromBounds(5.0, 15.0);
        outer.Contains(other).Should().BeFalse();
    }

    [Fact]
    public void Intersects_Overlapping_ReturnsTrue()
    {
        var a = Interval.FromBounds(1.0, 5.0);
        var b = Interval.FromBounds(3.0, 7.0);
        a.Intersects(b).Should().BeTrue();
    }

    [Fact]
    public void Intersects_Disjoint_ReturnsFalse()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        var b = Interval.FromBounds(5.0, 7.0);
        a.Intersects(b).Should().BeFalse();
    }

    [Fact]
    public void Intersects_TouchingAtBoundary_ReturnsTrue()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        var b = Interval.FromBounds(3.0, 5.0);
        a.Intersects(b).Should().BeTrue();
    }

    [Fact]
    public void Intersects_EmptyInterval_ReturnsFalse()
    {
        var a = Interval.FromBounds(1.0, 5.0);
        a.Intersects(Interval.Empty).Should().BeFalse();
    }

    [Fact]
    public void Add_NormalIntervals_ReturnsCorrectResult()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        var b = Interval.FromBounds(2.0, 4.0);
        var result = a.Add(b);
        result.Lower.Should().Be(3.0);
        result.Upper.Should().Be(7.0);
    }

    [Fact]
    public void Add_EmptyInterval_ReturnsEmpty()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        a.Add(Interval.Empty).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Subtract_NormalIntervals_ReturnsCorrectResult()
    {
        var a = Interval.FromBounds(5.0, 10.0);
        var b = Interval.FromBounds(1.0, 3.0);
        var result = a.Subtract(b);
        result.Lower.Should().Be(2.0);
        result.Upper.Should().Be(9.0);
    }

    [Fact]
    public void Subtract_EmptyInterval_ReturnsEmpty()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        a.Subtract(Interval.Empty).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Multiply_PositiveIntervals_ReturnsCorrectResult()
    {
        var a = Interval.FromBounds(2.0, 3.0);
        var b = Interval.FromBounds(4.0, 5.0);
        var result = a.Multiply(b);
        result.Lower.Should().Be(8.0);
        result.Upper.Should().Be(15.0);
    }

    [Fact]
    public void Multiply_IntervalsContainingZero_ExpandsCorrectly()
    {
        var a = Interval.FromBounds(-2.0, 3.0);
        var b = Interval.FromBounds(1.0, 4.0);
        var result = a.Multiply(b);
        result.Lower.Should().Be(-8.0);
        result.Upper.Should().Be(12.0);
    }

    [Fact]
    public void Divide_PositiveIntervals_ReturnsCorrectResult()
    {
        var a = Interval.FromBounds(4.0, 10.0);
        var b = Interval.FromBounds(2.0, 5.0);
        var result = a.Divide(b);
        result.Lower.Should().BeApproximately(0.8, 1e-10);
        result.Upper.Should().Be(5.0);
    }

    [Fact]
    public void Divide_DivisorContainsZero_ReturnsEmpty()
    {
        var a = Interval.FromBounds(1.0, 5.0);
        var b = Interval.FromBounds(-1.0, 1.0);
        a.Divide(b).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Negate_NormalInterval_ReturnsCorrectResult()
    {
        var interval = Interval.FromBounds(2.0, 5.0);
        var negated = interval.Negate();
        negated.Lower.Should().Be(-5.0);
        negated.Upper.Should().Be(-2.0);
    }

    [Fact]
    public void Union_OverlappingIntervals_ReturnsHull()
    {
        var a = Interval.FromBounds(1.0, 4.0);
        var b = Interval.FromBounds(3.0, 6.0);
        var result = a.Union(b);
        result.Lower.Should().Be(1.0);
        result.Upper.Should().Be(6.0);
    }

    [Fact]
    public void Union_DisjointIntervals_ReturnsHull()
    {
        var a = Interval.FromBounds(1.0, 2.0);
        var b = Interval.FromBounds(5.0, 7.0);
        var result = a.Union(b);
        result.Lower.Should().Be(1.0);
        result.Upper.Should().Be(7.0);
    }

    [Fact]
    public void Union_EmptyInterval_ReturnsOther()
    {
        var a = Interval.FromBounds(1.0, 5.0);
        var result = a.Union(Interval.Empty);
        result.Lower.Should().Be(1.0);
        result.Upper.Should().Be(5.0);
    }

    [Fact]
    public void Intersection_OverlappingIntervals_ReturnsOverlap()
    {
        var a = Interval.FromBounds(1.0, 4.0);
        var b = Interval.FromBounds(3.0, 6.0);
        var result = a.Intersection(b);
        result.Lower.Should().Be(3.0);
        result.Upper.Should().Be(4.0);
    }

    [Fact]
    public void Intersection_DisjointIntervals_ReturnsEmpty()
    {
        var a = Interval.FromBounds(1.0, 2.0);
        var b = Interval.FromBounds(5.0, 7.0);
        a.Intersection(b).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Intersection_OneContained_ReturnsSmaller()
    {
        var a = Interval.FromBounds(1.0, 10.0);
        var b = Interval.FromBounds(3.0, 5.0);
        var result = a.Intersection(b);
        result.Lower.Should().Be(3.0);
        result.Upper.Should().Be(5.0);
    }

    [Fact]
    public void OperatorPlus_AddsCorrectly()
    {
        var a = Interval.FromBounds(1.0, 3.0);
        var b = Interval.FromBounds(2.0, 4.0);
        var result = a + b;
        result.Lower.Should().Be(3.0);
        result.Upper.Should().Be(7.0);
    }

    [Fact]
    public void OperatorMinus_SubtractsCorrectly()
    {
        var a = Interval.FromBounds(5.0, 10.0);
        var b = Interval.FromBounds(1.0, 3.0);
        var result = a - b;
        result.Lower.Should().Be(2.0);
        result.Upper.Should().Be(9.0);
    }

    [Fact]
    public void OperatorMultiply_MultipliesCorrectly()
    {
        var a = Interval.FromBounds(2.0, 3.0);
        var b = Interval.FromBounds(4.0, 5.0);
        var result = a * b;
        result.Lower.Should().Be(8.0);
        result.Upper.Should().Be(15.0);
    }

    [Fact]
    public void OperatorDivide_DividesCorrectly()
    {
        var a = Interval.FromBounds(4.0, 10.0);
        var b = Interval.FromBounds(2.0, 5.0);
        var result = a / b;
        result.Lower.Should().BeApproximately(0.8, 1e-10);
        result.Upper.Should().Be(5.0);
    }

    [Fact]
    public void OperatorUnaryMinus_NegatesCorrectly()
    {
        var interval = Interval.FromBounds(2.0, 5.0);
        var result = -interval;
        result.Lower.Should().Be(-5.0);
        result.Upper.Should().Be(-2.0);
    }

    [Fact]
    public void ToString_NormalInterval_FormatsCorrectly()
    {
        var interval = Interval.FromBounds(1.5, 3.5);
        interval.ToString().Should().Be("[1.5, 3.5]");
    }

    [Fact]
    public void ToString_EmptyInterval_FormatsCorrectly()
    {
        Interval.Empty.ToString().Should().Be("∅");
    }

    [Fact]
    public void RealLine_IsRealLine()
    {
        Interval.RealLine.IsRealLine.Should().BeTrue();
        Interval.RealLine.Contains(0.0).Should().BeTrue();
        Interval.RealLine.Contains(1e10).Should().BeTrue();
    }
}
