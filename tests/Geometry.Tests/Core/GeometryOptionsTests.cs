namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryOptions"/> record.</summary>
public class GeometryOptionsTests
{
    /// <summary>Verifies that default Tolerance is 1e-10.</summary>
    [Fact]
    public void Default_Tolerance()
    {
        var opts = new GeometryOptions();

        opts.Tolerance.Should().BeApproximately(1e-10, 1e-20);
    }

    /// <summary>Verifies that default EnableCaching is true.</summary>
    [Fact]
    public void Default_EnableCaching()
    {
        var opts = new GeometryOptions();

        opts.EnableCaching.Should().BeTrue();
    }

    /// <summary>Verifies that default EnableParallelProcessing is true.</summary>
    [Fact]
    public void Default_EnableParallelProcessing()
    {
        var opts = new GeometryOptions();

        opts.EnableParallelProcessing.Should().BeTrue();
    }

    /// <summary>Verifies that default MaxParallelism is ProcessorCount.</summary>
    [Fact]
    public void Default_MaxParallelism()
    {
        var opts = new GeometryOptions();

        opts.MaxParallelism.Should().Be(Environment.ProcessorCount);
    }

    /// <summary>Verifies that default EnableDiagnostics is false.</summary>
    [Fact]
    public void Default_EnableDiagnostics()
    {
        var opts = new GeometryOptions();

        opts.EnableDiagnostics.Should().BeFalse();
    }

    /// <summary>Verifies that default ValidateOnCreate is false.</summary>
    [Fact]
    public void Default_ValidateOnCreate()
    {
        var opts = new GeometryOptions();

        opts.ValidateOnCreate.Should().BeFalse();
    }

    /// <summary>Verifies that Tolerance can be set via init accessor.</summary>
    [Fact]
    public void Tolerance_CanBeSet()
    {
        var opts = new GeometryOptions { Tolerance = 1e-6 };

        opts.Tolerance.Should().BeApproximately(1e-6, 1e-20);
    }

    /// <summary>Verifies that EnableCaching can be set via init accessor.</summary>
    [Fact]
    public void EnableCaching_CanBeSet()
    {
        var opts = new GeometryOptions { EnableCaching = false };

        opts.EnableCaching.Should().BeFalse();
    }
}
