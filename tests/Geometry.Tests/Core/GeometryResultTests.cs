namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryResult"/> record.</summary>
public class GeometryResultTests
{
    /// <summary>Verifies that Ok() returns a result with Success=true.</summary>
    [Fact]
    public void Ok_ReturnsSuccess()
    {
        var result = GeometryResult.Ok();

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Ok() returns null ErrorMessage.</summary>
    [Fact]
    public void Ok_ErrorMessageIsNull()
    {
        var result = GeometryResult.Ok();

        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>Verifies that Failure returns Success=false.</summary>
    [Fact]
    public void Failure_ReturnsSuccessFalse()
    {
        var result = GeometryResult.Failure("error");

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Failure stores the error message.</summary>
    [Fact]
    public void Failure_StoresErrorMessage()
    {
        var result = GeometryResult.Failure("something went wrong");

        result.ErrorMessage.Should().Be("something went wrong");
    }

    /// <summary>Verifies that Failure with diagnostic type stores the type.</summary>
    [Fact]
    public void Failure_StoresDiagnosticType()
    {
        var result = GeometryResult.Failure("error", GeometryDiagnosticType.DegenerateGeometry);

        result.DiagnosticType.Should().Be(GeometryDiagnosticType.DegenerateGeometry);
    }

    /// <summary>Verifies that Failure defaults to General diagnostic type.</summary>
    [Fact]
    public void Failure_DefaultsToGeneralDiagnosticType()
    {
        var result = GeometryResult.Failure("error");

        result.DiagnosticType.Should().Be(GeometryDiagnosticType.General);
    }

    /// <summary>Verifies that Ok() can be created as a new record with Success=true.</summary>
    [Fact]
    public void Ok_CreatedDirectly_HasSuccessTrue()
    {
        var result = new GeometryResult { Success = true };

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Failure with NullInput diagnostic type stores correctly.</summary>
    [Fact]
    public void Failure_NullInput_StoresDiagnosticType()
    {
        var result = GeometryResult.Failure("null", GeometryDiagnosticType.NullInput);

        result.DiagnosticType.Should().Be(GeometryDiagnosticType.NullInput);
    }
}
