using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class ValidationTests
{
    [Fact]
    public void Valid_ReturnsValidResult()
    {
        var validation = Validation<int>.Valid(42);

        validation.IsValid.Should().BeTrue();
        validation.Value.Should().Be(42);
        validation.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Invalid_ReturnsInvalidResult()
    {
        var error = Error.Validation("E", "err");
        var validation = Validation<int>.Invalid(error);

        validation.IsInvalid.Should().BeTrue();
        validation.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Invalid_WithMultipleErrors()
    {
        var errors = new List<Error>
        {
            Error.Validation("E1", "err1"),
            Error.Validation("E2", "err2")
        };
        var validation = Validation<int>.Invalid(errors);

        validation.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Map_TransformsValidValue()
    {
        var validation = Validation<int>.Valid(5);
        var mapped = validation.Map(x => x * 2);

        mapped.IsValid.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_PreservesInvalid()
    {
        var error = Error.Validation("E", "err");
        var validation = Validation<int>.Invalid(error);
        var mapped = validation.Map(x => x * 2);

        mapped.IsInvalid.Should().BeTrue();
        mapped.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Bind_ChainsOperations()
    {
        var validation = Validation<int>.Valid(10);
        var bound = validation.Bind(x => Validation<string>.Valid(x.ToString()));

        bound.IsValid.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void Combine_AllValid_ReturnsValid()
    {
        var v1 = Validation<int>.Valid(1);
        var v2 = Validation<int>.Valid(2);
        var combined = Validation<int>.Combine(v1, v2);

        combined.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Combine_SomeInvalid_ReturnsInvalid()
    {
        var v1 = Validation<int>.Valid(1);
        var v2 = Validation<int>.Invalid(Error.Validation("E", "err"));
        var combined = Validation<int>.Combine(v1, v2);

        combined.IsInvalid.Should().BeTrue();
        combined.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void ToResult_WhenValid_ReturnsSuccess()
    {
        var validation = Validation<int>.Valid(42);
        var result = validation.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_WhenInvalid_ReturnsFailure()
    {
        var validation = Validation<int>.Invalid(Error.Validation("E", "err"));
        var result = validation.ToResult();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Match_ExecutesCorrectBranch()
    {
        var valid = Validation<int>.Valid(5);
        var invalid = Validation<int>.Invalid(Error.Validation("E", "err"));

        valid.Match(x => x * 2, _ => 0).Should().Be(10);
        invalid.Match(x => x * 2, _ => 0).Should().Be(0);
    }

    [Fact]
    public void ValidationHelper_ValidatesCorrectly()
    {
        var valid = Validation.Validate(10, x => x > 0, "E", "err");
        var invalid = Validation.Validate(-1, x => x > 0, "E", "err");

        valid.IsValid.Should().BeTrue();
        invalid.IsInvalid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAll_PassesAllRules()
    {
        var rules = new List<ValidationRule<int>>
        {
            new() { Condition = x => x > 0, Error = Error.Validation("E", "must be positive") },
            new() { Condition = x => x < 100, Error = Error.Validation("E", "must be less than 100") }
        };

        var valid = Validation.ValidateAll(50, rules);
        var invalid = Validation.ValidateAll(150, rules);

        valid.IsValid.Should().BeTrue();
        invalid.IsInvalid.Should().BeTrue();
        invalid.Errors.Should().HaveCount(1);
    }
}
