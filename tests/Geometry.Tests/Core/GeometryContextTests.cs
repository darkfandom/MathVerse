namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryContext"/> class.</summary>
public class GeometryContextTests
{
    /// <summary>Verifies that Construction with default options succeeds.</summary>
    [Fact]
    public void Construction_DefaultOptions_Succeeds()
    {
        var ctx = new GeometryContext();

        ctx.Should().NotBeNull();
    }

    /// <summary>Verifies that Options returns the provided options.</summary>
    [Fact]
    public void Options_ReturnsProvidedOptions()
    {
        var opts = new GeometryOptions { Tolerance = 1e-6 };
        var ctx = new GeometryContext(opts);

        ctx.Options.Tolerance.Should().BeApproximately(1e-6, 1e-20);
    }

    /// <summary>Verifies that Statistics returns a non-null instance on construction.</summary>
    [Fact]
    public void Statistics_ReturnsNonNullOnConstruction()
    {
        var ctx = new GeometryContext();

        ctx.Statistics.Should().NotBeNull();
    }

    /// <summary>Verifies that TrackCreation increments PointsCreated.</summary>
    [Fact]
    public void TrackCreation_IncrementsPointsCreated()
    {
        var ctx = new GeometryContext();

        ctx.TrackCreation();

        ctx.Statistics.PointsCreated.Should().Be(1);
    }

    /// <summary>Verifies that TrackCreation can be called multiple times.</summary>
    [Fact]
    public void TrackCreation_MultipleCalls_Accumulates()
    {
        var ctx = new GeometryContext();

        ctx.TrackCreation();
        ctx.TrackCreation();
        ctx.TrackCreation();

        ctx.Statistics.PointsCreated.Should().Be(3);
    }

    /// <summary>Verifies that Reset clears accumulated statistics.</summary>
    [Fact]
    public void Reset_ClearsStatistics()
    {
        var ctx = new GeometryContext();
        ctx.TrackCreation();
        ctx.TrackCreation();

        ctx.Reset();

        ctx.Statistics.PointsCreated.Should().Be(0);
    }

    /// <summary>Verifies that Construction with custom options stores them.</summary>
    [Fact]
    public void Construction_CustomOptions_StoresOptions()
    {
        var opts = new GeometryOptions { EnableDiagnostics = true };
        var ctx = new GeometryContext(opts);

        ctx.Options.EnableDiagnostics.Should().BeTrue();
    }

    /// <summary>Verifies that Construction with null options throws.</summary>
    [Fact]
    public void Construction_NullOptions_Throws()
    {
        var act = () => new GeometryContext(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
