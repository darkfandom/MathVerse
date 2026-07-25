using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class MaybeTests
{
    [Fact]
    public void Defined_ContainsValue()
    {
        var maybe = Maybe<int>.Defined(42);

        maybe.IsDefined.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void Undefined_IsEmpty()
    {
        var maybe = Maybe<int>.DivisionByZero;

        maybe.IsUndefined.Should().BeTrue();
        maybe.Reason.Should().Be(MaybeReason.DivisionByZero);
    }

    [Fact]
    public void Undefined_Value_Throws()
    {
        var maybe = Maybe<int>.Overflow;

        var act = () => maybe.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesDefined()
    {
        Maybe<int> maybe = 42;

        maybe.IsDefined.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromNull_CreatesUndefined()
    {
        string? nullValue = null;
        Maybe<string> maybe = nullValue;

        maybe.IsUndefined.Should().BeTrue();
        maybe.Reason.Should().Be(MaybeReason.NullValue);
    }

    [Fact]
    public void Map_TransformsValue()
    {
        var maybe = Maybe<int>.Defined(5);
        var mapped = maybe.Map(x => x * 2);

        mapped.IsDefined.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_PreservesUndefined()
    {
        var maybe = Maybe<int>.DivisionByZero;
        var mapped = maybe.Map(x => x * 2);

        mapped.IsUndefined.Should().BeTrue();
        mapped.Reason.Should().Be(MaybeReason.DivisionByZero);
    }

    [Fact]
    public void Bind_ChainsOperations()
    {
        var maybe = Maybe<int>.Defined(10);
        var bound = maybe.Bind(x => Maybe<string>.Defined(x.ToString()));

        bound.IsDefined.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void Or_ReturnsDefault_WhenUndefined()
    {
        var maybe = Maybe<int>.DivisionByZero;

        maybe.Or(99).Should().Be(99);
    }

    [Fact]
    public void Or_ReturnsValue_WhenDefined()
    {
        var maybe = Maybe<int>.Defined(42);

        maybe.Or(99).Should().Be(42);
    }

    [Fact]
    public void Match_ExecutesCorrectBranch()
    {
        var defined = Maybe<int>.Defined(5);
        var undefined = Maybe<int>.Overflow;

        defined.Match(x => x * 2, _ => 0).Should().Be(10);
        undefined.Match(x => x * 2, _ => 0).Should().Be(0);
    }

    [Fact]
    public void ToResult_Success_WhenDefined()
    {
        var maybe = Maybe<int>.Defined(42);
        var result = maybe.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_Failure_WhenUndefined()
    {
        var maybe = Maybe<int>.DivisionByZero;
        var result = maybe.ToResult();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToOption_WhenDefined_ReturnsSome()
    {
        var maybe = Maybe<int>.Defined(42);
        var option = maybe.ToOption();

        option.IsSome.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void ToOption_WhenUndefined_ReturnsNone()
    {
        var maybe = Maybe<int>.DomainError;
        var option = maybe.ToOption();

        option.IsNone.Should().BeTrue();
    }

    [Theory]
    [InlineData(MaybeReason.DivisionByZero)]
    [InlineData(MaybeReason.Overflow)]
    [InlineData(MaybeReason.DomainError)]
    [InlineData(MaybeReason.OutOfRange)]
    [InlineData(MaybeReason.DidNotConverge)]
    public void Undefined_WithVariousReasons_PreservesReason(MaybeReason reason)
    {
        var maybe = Maybe<int>.Undefined(reason);

        maybe.IsUndefined.Should().BeTrue();
        maybe.Reason.Should().Be(reason);
    }
}
