namespace MathVerse.Geometry.Tests.Plotting;

/// <summary>Tests for the <see cref="PlotEngine"/> class.</summary>
public class PlotEngineTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that PlotFunction generates line data.</summary>
    [Fact]
    public void PlotFunction_GeneratesLines()
    {
        var engine = new PlotEngine();

        var result = engine.PlotFunction(x => x * x, -1.0, 1.0);

        result.Success.Should().BeTrue();
        result.Lines.Should().HaveCount(1);
    }

    /// <summary>Verifies that PlotFunction generates correct number of points.</summary>
    [Fact]
    public void PlotFunction_CorrectPointCount()
    {
        var engine = new PlotEngine();

        var result = engine.PlotFunction(x => x, 0.0, 10.0);

        result.Lines[0].Points.Should().HaveCount(101);
    }

    /// <summary>Verifies that PlotParametric for a circle generates a closed curve.</summary>
    [Fact]
    public void PlotParametric_Circle_ClosedCurve()
    {
        var engine = new PlotEngine();

        var result = engine.PlotParametric(
            t => (System.Math.Cos(t), System.Math.Sin(t)),
            0.0, 2.0 * System.Math.PI);

        result.Success.Should().BeTrue();
        result.Lines.Should().HaveCount(1);
        var first = result.Lines[0].Points[0];
        var last = result.Lines[0].Points[^1];
        first.X.Should().BeApproximately(last.X, 0.01);
        first.Y.Should().BeApproximately(last.Y, 0.01);
    }

    /// <summary>Verifies that PlotPolar generates line data.</summary>
    [Fact]
    public void PlotPolar_GeneratesLines()
    {
        var engine = new PlotEngine();

        var result = engine.PlotPolar(theta => 1.0, 0.0, 2.0 * System.Math.PI);

        result.Success.Should().BeTrue();
        result.Lines.Should().HaveCount(1);
    }

    /// <summary>Verifies that PlotScatter generates scatter data.</summary>
    [Fact]
    public void PlotScatter_GeneratesScatterData()
    {
        var engine = new PlotEngine();
        var points = new List<(double X, double Y)> { (1, 2), (3, 4), (5, 6) };

        var result = engine.PlotScatter(points);

        result.Success.Should().BeTrue();
        result.ScatterPlots.Should().HaveCount(1);
    }

    /// <summary>Verifies that PlotScatter preserves the input points.</summary>
    [Fact]
    public void PlotScatter_PreservesPoints()
    {
        var engine = new PlotEngine();
        var points = new List<(double X, double Y)> { (1, 2), (3, 4) };

        var result = engine.PlotScatter(points);

        result.ScatterPlots[0].Points.Should().HaveCount(2);
    }

    /// <summary>Verifies that PlotHistogram generates bar data.</summary>
    [Fact]
    public void PlotHistogram_GeneratesBarData()
    {
        var engine = new PlotEngine();
        var values = new List<double> { 1, 2, 3, 4, 5 };

        var result = engine.PlotHistogram(values, 3);

        result.Success.Should().BeTrue();
        result.Bars.Should().HaveCount(1);
    }

    /// <summary>Verifies that PlotHistogram with empty values returns failure.</summary>
    [Fact]
    public void PlotHistogram_EmptyValues_ReturnsFailure()
    {
        var engine = new PlotEngine();

        var result = engine.PlotHistogram(new List<double>(), 5);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that PlotLine generates line data.</summary>
    [Fact]
    public void PlotLine_GeneratesLines()
    {
        var engine = new PlotEngine();
        var points = new List<(double X, double Y)> { (0, 0), (1, 1), (2, 2) };

        var result = engine.PlotLine(points);

        result.Success.Should().BeTrue();
        result.Lines.Should().HaveCount(1);
    }

    /// <summary>Verifies that PlotContour generates contour lines.</summary>
    [Fact]
    public void PlotContour_GeneratesLines()
    {
        var engine = new PlotEngine();

        var result = engine.PlotContour(
            (x, y) => x * x + y * y,
            -1.0, 1.0, -1.0, 1.0, 3);

        result.Success.Should().BeTrue();
        result.Lines.Should().NotBeEmpty();
    }

    /// <summary>Verifies that PlotVectorField generates arrow lines.</summary>
    [Fact]
    public void PlotVectorField_GeneratesLines()
    {
        var engine = new PlotEngine();

        var result = engine.PlotVectorField(
            (x, y) => (1.0, 0.0),
            0.0, 1.0, 0.0, 1.0, 2);

        result.Success.Should().BeTrue();
        result.Lines.Should().NotBeEmpty();
    }

    /// <summary>Verifies that PlotSurface generates a mesh.</summary>
    [Fact]
    public void PlotSurface_GeneratesMesh()
    {
        var engine = new PlotEngine();

        var result = engine.PlotSurface(
            (x, y) => x * y,
            -1.0, 1.0, -1.0, 1.0, 5);

        result.Success.Should().BeTrue();
        result.SurfaceMesh.Should().NotBeNull();
    }

    /// <summary>Verifies that PlotSurface mesh has correct vertex count.</summary>
    [Fact]
    public void PlotSurface_MeshHasCorrectVertices()
    {
        var engine = new PlotEngine();

        var result = engine.PlotSurface(
            (x, y) => 0.0,
            -1.0, 1.0, -1.0, 1.0, 4);

        result.SurfaceMesh!.VertexCount.Should().Be(25);
    }

    /// <summary>Verifies that PlotFunction uses default config when none is provided.</summary>
    [Fact]
    public void PlotFunction_DefaultConfig()
    {
        var engine = new PlotEngine();

        var result = engine.PlotFunction(x => x, 0.0, 1.0);

        result.Configuration.Should().Be(PlotConfiguration.Default);
    }

    /// <summary>Verifies that PlotFunction uses custom config when provided.</summary>
    [Fact]
    public void PlotFunction_CustomConfig()
    {
        var engine = new PlotEngine();
        var config = new PlotConfiguration { Title = "Test" };

        var result = engine.PlotFunction(x => x, 0.0, 1.0, config);

        result.Configuration.Title.Should().Be("Test");
    }

    /// <summary>Verifies that PlotScatter with many points succeeds.</summary>
    [Fact]
    public void PlotScatter_ManyPoints_Succeeds()
    {
        var engine = new PlotEngine();
        var points = Enumerable.Range(0, 1000).Select(i => ((double)i, (double)i * 2)).ToList();

        var result = engine.PlotScatter(points);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that PlotFunction uses custom config via parameter.</summary>
    [Fact]
    public void PlotLine_CustomConfig()
    {
        var engine = new PlotEngine();
        var config = new PlotConfiguration { Title = "LinePlot" };
        var points = new List<(double X, double Y)> { (0, 0), (1, 1) };

        var result = engine.PlotLine(points, config);

        result.Configuration.Title.Should().Be("LinePlot");
    }

    /// <summary>Verifies that PlotHistogram generates correct number of bins.</summary>
    [Fact]
    public void PlotHistogram_CorrectBinCount()
    {
        var engine = new PlotEngine();
        var values = Enumerable.Range(0, 100).Select(i => (double)i).ToList();

        var result = engine.PlotHistogram(values, 10);

        result.Bars[0].Bars.Should().HaveCount(10);
    }

    /// <summary>Verifies that PlotParametric with custom config passes through.</summary>
    [Fact]
    public void PlotParametric_CustomConfig()
    {
        var engine = new PlotEngine();
        var config = new PlotConfiguration { Title = "Parametric" };

        var result = engine.PlotParametric(t => (t, t * t), 0.0, 1.0, config);

        result.Configuration.Title.Should().Be("Parametric");
    }

    /// <summary>Verifies that PlotPolar with custom config passes through.</summary>
    [Fact]
    public void PlotPolar_CustomConfig()
    {
        var engine = new PlotEngine();
        var config = new PlotConfiguration { Title = "Polar" };

        var result = engine.PlotPolar(theta => 1.0, 0.0, 2.0 * System.Math.PI, config);

        result.Configuration.Title.Should().Be("Polar");
    }

    /// <summary>Verifies that PlotSurface with custom config passes through.</summary>
    [Fact]
    public void PlotSurface_CustomConfig()
    {
        var engine = new PlotEngine();
        var config = new PlotConfiguration { Title = "Surface" };

        var result = engine.PlotSurface((x, y) => x + y, 0.0, 1.0, 0.0, 1.0, 2, config);

        result.Configuration.Title.Should().Be("Surface");
    }
}
