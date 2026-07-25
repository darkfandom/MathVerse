namespace MathVerse.Simulation.Tests.Visualization;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class VisualizationModelsTests
{
    [Fact]
    public void PlotSeries_DefaultValues()
    {
        var ps = new PlotSeries();
        ps.Name.Should().Be(string.Empty);
        ps.Color.Should().Be(string.Empty);
        ps.LineWidth.Should().Be(0);
        ps.Marker.Should().Be(string.Empty);
    }

    [Fact]
    public void PlotType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<PlotType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void PlotType_ContainsExpectedTypes()
    {
        Enum.GetValues<PlotType>().Should().HaveCount(9);
    }

    [Fact]
    public void CreateLineSeries_SetsProperties()
    {
        var x = ImmutableArray.Create(1.0, 2.0, 3.0);
        var y = ImmutableArray.Create(4.0, 5.0, 6.0);
        var series = VisualizationModels.CreateLineSeries("sin", x, y, "red", 2.0);
        series.Name.Should().Be("sin");
        series.Type.Should().Be(PlotType.Line);
        series.X.Should().HaveCount(3);
        series.Y.Should().HaveCount(3);
        series.Color.Should().Be("red");
        series.LineWidth.Should().Be(2.0);
    }

    [Fact]
    public void CreateScatterSeries_SetsProperties()
    {
        var x = ImmutableArray.Create(1.0, 2.0);
        var y = ImmutableArray.Create(3.0, 4.0);
        var series = VisualizationModels.CreateScatterSeries("pts", x, y, "blue", "square");
        series.Type.Should().Be(PlotType.Scatter);
        series.Marker.Should().Be("square");
        series.Color.Should().Be("blue");
    }

    [Fact]
    public void CreateHeatmapSeries_SetsType()
    {
        var hm = VisualizationModels.CreateHeatmap(
            ImmutableArray.Create(1.0, 2.0),
            ImmutableArray.Create(3.0, 4.0),
            ImmutableArray.Create(0.0, 1.0, 2.0, 3.0));
        var series = VisualizationModels.CreateHeatmapSeries("heat", hm);
        series.Type.Should().Be(PlotType.Heatmap);
    }

    [Fact]
    public void Frame_DefaultValues()
    {
        var frame = new Frame();
        frame.Time.Should().Be(0);
        frame.Series.IsDefaultOrEmpty.Should().BeTrue();
    }

    [Fact]
    public void CreateFrame_SetsProperties()
    {
        var series = ImmutableArray.Create(
            VisualizationModels.CreateLineSeries("a",
                ImmutableArray.Create(1.0), ImmutableArray.Create(2.0)));
        var frame = VisualizationModels.CreateFrame(0.5, series);
        frame.Time.Should().Be(0.5);
        frame.Series.Should().HaveCount(1);
    }

    [Fact]
    public void HeatmapData_DefaultValues()
    {
        var hd = new HeatmapData();
        hd.MinValue.Should().Be(0);
        hd.MaxValue.Should().Be(0);
        hd.ColorScale.Should().Be("viridis");
    }

    [Fact]
    public void CreateHeatmap_ComputesMinMax()
    {
        var x = ImmutableArray.Create(0.0, 1.0);
        var y = ImmutableArray.Create(0.0, 1.0);
        var vals = ImmutableArray.Create(-5.0, 3.0, 0.0, 10.0);
        var hm = VisualizationModels.CreateHeatmap(x, y, vals);
        hm.MinValue.Should().Be(-5.0);
        hm.MaxValue.Should().Be(10.0);
    }

    [Fact]
    public void Vector2D_Zero()
    {
        Vector2D.Zero.X.Should().Be(0);
        Vector2D.Zero.Y.Should().Be(0);
    }

    [Fact]
    public void VectorFieldData_SetsProperties()
    {
        var positions = ImmutableArray.Create(new Vector2D { X = 0, Y = 0 });
        var vectors = ImmutableArray.Create(new Vector2D { X = 1, Y = 0 });
        var data = VisualizationModels.CreateVectorField(positions, vectors, 2.0);
        data.Scale.Should().Be(2.0);
        data.Positions.Should().HaveCount(1);
    }

    [Fact]
    public void CreateVectorFieldSeries_SetsType()
    {
        var data = new VectorFieldData
        {
            Positions = ImmutableArray.Create(new Vector2D { X = 0, Y = 0 }),
            Vectors = ImmutableArray.Create(new Vector2D { X = 1, Y = 1 })
        };
        var series = VisualizationModels.CreateVectorFieldSeries("vf", data);
        series.Type.Should().Be(PlotType.VectorField);
    }

    [Fact]
    public void CreateTimeline_SetsProperties()
    {
        var frames = ImmutableArray.Create(
            VisualizationModels.CreateFrame(0, ImmutableArray<PlotSeries>.Empty),
            VisualizationModels.CreateFrame(1, ImmutableArray<PlotSeries>.Empty));
        var tl = VisualizationModels.CreateTimeline(frames, 0, 1, 2);
        tl.StartTime.Should().Be(0);
        tl.EndTime.Should().Be(1);
        tl.FrameRate.Should().Be(2);
        tl.Frames.Should().HaveCount(2);
    }

    [Fact]
    public void ContourData_DefaultValues()
    {
        var cd = new ContourData();
        cd.X.IsDefaultOrEmpty.Should().BeTrue();
        cd.Y.IsDefaultOrEmpty.Should().BeTrue();
        cd.Z.IsDefaultOrEmpty.Should().BeTrue();
        cd.Levels.IsDefaultOrEmpty.Should().BeTrue();
    }

    [Fact]
    public void StreamlineData_DefaultValues()
    {
        var sd = new StreamlineData();
        sd.Path.IsDefaultOrEmpty.Should().BeTrue();
        sd.Times.IsDefaultOrEmpty.Should().BeTrue();
        sd.StartTime.Should().Be(0);
        sd.EndTime.Should().Be(0);
    }
}
