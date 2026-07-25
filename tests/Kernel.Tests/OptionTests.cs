using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class OptionTests
{
    [Fact]
    public void Some_ContainsValue()
    {
        var option = Option<int>.Some(42);

        option.IsSome.Should().BeTrue();
        option.IsNone.Should().BeFalse();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void None_IsEmpty()
    {
        var option = Option<int>.None;

        option.IsNone.Should().BeTrue();
        option.IsSome.Should().BeFalse();
    }

    [Fact]
    public void None_Value_Throws()
    {
        var option = Option<int>.None;

        var act = () => option.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSome()
    {
        Option<int> option = 42;

        option.IsSome.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromNull_CreatesNone()
    {
        string? nullValue = null;
        Option<string> option = nullValue;

        option.IsNone.Should().BeTrue();
    }

    [Fact]
    public void Map_TransformsValue()
    {
        var option = Option<int>.Some(5);
        var mapped = option.Map(x => x * 2);

        mapped.IsSome.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_PreservesNone()
    {
        var option = Option<int>.None;
        var mapped = option.Map(x => x * 2);

        mapped.IsNone.Should().BeTrue();
    }

    [Fact]
    public void Bind_ChainsOperations()
    {
        var option = Option<int>.Some(10);
        var bound = option.Bind(x => Option<string>.Some(x.ToString()));

        bound.IsSome.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void Or_ReturnsDefault_WhenNone()
    {
        var option = Option<int>.None;

        option.Or(99).Should().Be(99);
    }

    [Fact]
    public void Or_ReturnsValue_WhenSome()
    {
        var option = Option<int>.Some(42);

        option.Or(99).Should().Be(42);
    }

    [Fact]
    public void OrGet_ReturnsComputedDefault_WhenNone()
    {
        var option = Option<int>.None;

        option.OrGet(() => 99).Should().Be(99);
    }

    [Fact]
    public void Match_ExecutesCorrectBranch()
    {
        var some = Option<int>.Some(5);
        var none = Option<int>.None;

        some.Match(x => x * 2, () => 0).Should().Be(10);
        none.Match(x => x * 2, () => 0).Should().Be(0);
    }

    [Fact]
    public void Where_PredicateTrue_KeepsSome()
    {
        var option = Option<int>.Some(5);
        var filtered = option.Where(x => x > 0);

        filtered.IsSome.Should().BeTrue();
        filtered.Value.Should().Be(5);
    }

    [Fact]
    public void Where_PredicateFalse_ReturnsNone()
    {
        var option = Option<int>.Some(-1);
        var filtered = option.Where(x => x > 0);

        filtered.IsNone.Should().BeTrue();
    }

    [Fact]
    public void ToResult_Success_WhenSome()
    {
        var option = Option<int>.Some(42);
        var result = option.ToResult(new Error("E", "err", ErrorKind.Internal));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_Failure_WhenNone()
    {
        var error = new Error("E", "err", ErrorKind.Internal);
        var option = Option<int>.None;
        var result = option.ToResult(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void FromNullable_NonNull_ReturnsSome()
    {
        var option = Option<string>.FromNullable("hello");

        option.IsSome.Should().BeTrue();
        option.Value.Should().Be("hello");
    }

    [Fact]
    public void FromNullable_Null_ReturnsNone()
    {
        var option = Option<string>.FromNullable(null);

        option.IsNone.Should().BeTrue();
    }
}
