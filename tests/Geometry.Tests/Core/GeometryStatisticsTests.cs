namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryStatistics"/> record.</summary>
public class GeometryStatisticsTests
{
    /// <summary>Verifies that a default instance has all zero values.</summary>
    [Fact]
    public void Default_AllValuesZero()
    {
        var stats = new GeometryStatistics();

        stats.PointsCreated.Should().Be(0);
        stats.LinesCreated.Should().Be(0);
        stats.CirclesCreated.Should().Be(0);
        stats.TrianglesCreated.Should().Be(0);
        stats.MeshesCreated.Should().Be(0);
        stats.TransformationsApplied.Should().Be(0);
        stats.IntersectionsComputed.Should().Be(0);
        stats.CurvesEvaluated.Should().Be(0);
        stats.SurfacesEvaluated.Should().Be(0);
        stats.TotalMemoryAllocated.Should().Be(0);
    }

    /// <summary>Verifies that PointsCreated can be tracked.</summary>
    [Fact]
    public void PointsCreated_CanBeSet()
    {
        var stats = new GeometryStatistics { PointsCreated = 5 };

        stats.PointsCreated.Should().Be(5);
    }

    /// <summary>Verifies that LinesCreated can be tracked.</summary>
    [Fact]
    public void LinesCreated_CanBeSet()
    {
        var stats = new GeometryStatistics { LinesCreated = 3 };

        stats.LinesCreated.Should().Be(3);
    }

    /// <summary>Verifies that CirclesCreated can be tracked.</summary>
    [Fact]
    public void CirclesCreated_CanBeSet()
    {
        var stats = new GeometryStatistics { CirclesCreated = 2 };

        stats.CirclesCreated.Should().Be(2);
    }

    /// <summary>Verifies that TrianglesCreated can be tracked.</summary>
    [Fact]
    public void TrianglesCreated_CanBeSet()
    {
        var stats = new GeometryStatistics { TrianglesCreated = 10 };

        stats.TrianglesCreated.Should().Be(10);
    }

    /// <summary>Verifies that TotalMemoryAllocated can be tracked.</summary>
    [Fact]
    public void TotalMemoryAllocated_CanBeSet()
    {
        var stats = new GeometryStatistics { TotalMemoryAllocated = 1000 };

        stats.TotalMemoryAllocated.Should().Be(1000);
    }
}
