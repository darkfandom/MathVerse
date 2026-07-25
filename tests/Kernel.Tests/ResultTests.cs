using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

/// <summary>
/// Tests for the Result type.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_ReturnsFailedResult()
    {
        var error = new Error("TEST", "Test error", ErrorKind.Internal);
        var result = Result<int>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Map_TransformsSuccessValue()
    {
        var result = Result<int>.Success(2);
        var mapped = result.Map(x => x * 3);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(6);
    }

    [Fact]
    public void Map_PreservesFailure()
    {
        var error = new Error("TEST", "Test error", ErrorKind.Internal);
        var result = Result<int>.Failure(error);
        var mapped = result.Map(x => x * 3);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_ChainsOperations()
    {
        var result = Result<int>.Success(10);
        var chained = result.Bind(x => Result<string>.Success(x.ToString()));

        chained.IsSuccess.Should().BeTrue();
        chained.Value.Should().Be("10");
    }

    [Fact]
    public void Match_ExecutesCorrectBranch()
    {
        var success = Result<int>.Success(5);
        var failure = Result<int>.Failure(new Error("E", "err", ErrorKind.Internal));

        var successResult = success.Match(
            onSuccess: x => x * 2,
            onFailure: _ => 0);

        var failureResult = failure.Match(
            onSuccess: x => x * 2,
            onFailure: _ => 0);

        successResult.Should().Be(10);
        failureResult.Should().Be(0);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        var error = new Error("TEST", "Test", ErrorKind.Internal);
        Result<int> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
