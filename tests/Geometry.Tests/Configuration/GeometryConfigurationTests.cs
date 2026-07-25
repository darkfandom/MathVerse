namespace MathVerse.Geometry.Tests.Configuration;

/// <summary>Tests for the <see cref="GeometryConfiguration"/> class.</summary>
public class GeometryConfigurationTests
{
    /// <summary>Verifies that Build produces a GeometryOptions with default tolerance.</summary>
    [Fact]
    public void DefaultBuild_HasDefaultTolerance()
    {
        var config = new GeometryConfiguration();

        var options = config.Build();

        options.Tolerance.Should().BeApproximately(1e-10, 1e-20);
    }

    /// <summary>Verifies that WithTolerance sets the tolerance on the built options.</summary>
    [Fact]
    public void WithTolerance_SetsTolerance()
    {
        var config = new GeometryConfiguration().WithTolerance(1e-6);

        var options = config.Build();

        options.Tolerance.Should().BeApproximately(1e-6, 1e-20);
    }

    /// <summary>Verifies that WithCaching sets caching on the built options.</summary>
    [Fact]
    public void WithCaching_SetsCaching()
    {
        var config = new GeometryConfiguration().WithCaching(false);

        var options = config.Build();

        options.EnableCaching.Should().BeFalse();
    }

    /// <summary>Verifies that WithParallelProcessing sets parallel processing on the built options.</summary>
    [Fact]
    public void WithParallelProcessing_SetsParallelProcessing()
    {
        var config = new GeometryConfiguration().WithParallelProcessing(false);

        var options = config.Build();

        options.EnableParallelProcessing.Should().BeFalse();
    }

    /// <summary>Verifies that WithMaxParallelism sets the max parallelism on the built options.</summary>
    [Fact]
    public void WithMaxParallelism_SetsMaxParallelism()
    {
        var config = new GeometryConfiguration().WithMaxParallelism(4);

        var options = config.Build();

        options.MaxParallelism.Should().Be(4);
    }

    /// <summary>Verifies that WithDiagnostics sets diagnostics on the built options.</summary>
    [Fact]
    public void WithDiagnostics_SetsDiagnostics()
    {
        var config = new GeometryConfiguration().WithDiagnostics(true);

        var options = config.Build();

        options.EnableDiagnostics.Should().BeTrue();
    }

    /// <summary>Verifies that WithValidation sets validation on the built options.</summary>
    [Fact]
    public void WithValidation_SetsValidation()
    {
        var config = new GeometryConfiguration().WithValidation(true);

        var options = config.Build();

        options.ValidateOnCreate.Should().BeTrue();
    }

    /// <summary>Verifies that fluent chaining produces the correct final state.</summary>
    [Fact]
    public void FluentChaining_CorrectFinalState()
    {
        var options = new GeometryConfiguration()
            .WithTolerance(1e-8)
            .WithCaching(false)
            .WithParallelProcessing(true)
            .WithMaxParallelism(8)
            .WithDiagnostics(true)
            .WithValidation(true)
            .Build();

        options.Tolerance.Should().BeApproximately(1e-8, 1e-20);
        options.EnableCaching.Should().BeFalse();
        options.EnableParallelProcessing.Should().BeTrue();
        options.MaxParallelism.Should().Be(8);
        options.EnableDiagnostics.Should().BeTrue();
        options.ValidateOnCreate.Should().BeTrue();
    }
}
