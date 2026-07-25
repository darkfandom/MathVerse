namespace MathVerse.Geometry.Tests.Plotting;

/// <summary>Tests for the <see cref="PlotConfiguration"/> record.</summary>
public class PlotConfigurationTests
{
    /// <summary>Verifies that default configuration has empty title.</summary>
    [Fact]
    public void Default_HasEmptyTitle()
    {
        var cfg = PlotConfiguration.Default;

        cfg.Title.Should().BeEmpty();
    }

    /// <summary>Verifies that the Title property can be set.</summary>
    [Fact]
    public void Title_CanBeSet()
    {
        var cfg = new PlotConfiguration { Title = "My Plot" };

        cfg.Title.Should().Be("My Plot");
    }

    /// <summary>Verifies that the XLabel property can be set.</summary>
    [Fact]
    public void XLabel_CanBeSet()
    {
        var cfg = new PlotConfiguration { XLabel = "X Axis" };

        cfg.XLabel.Should().Be("X Axis");
    }

    /// <summary>Verifies that the YLabel property can be set.</summary>
    [Fact]
    public void YLabel_CanBeSet()
    {
        var cfg = new PlotConfiguration { YLabel = "Y Axis" };

        cfg.YLabel.Should().Be("Y Axis");
    }

    /// <summary>Verifies that the Width property can be set.</summary>
    [Fact]
    public void Width_CanBeSet()
    {
        var cfg = new PlotConfiguration { Width = 1024 };

        cfg.Width.Should().Be(1024);
    }

    /// <summary>Verifies that the Height property can be set.</summary>
    [Fact]
    public void Height_CanBeSet()
    {
        var cfg = new PlotConfiguration { Height = 768 };

        cfg.Height.Should().Be(768);
    }

    /// <summary>Verifies that the ShowGrid property can be set to false.</summary>
    [Fact]
    public void ShowGrid_CanBeSetFalse()
    {
        var cfg = new PlotConfiguration { ShowGrid = false };

        cfg.ShowGrid.Should().BeFalse();
    }

    /// <summary>Verifies that the ShowLegend property can be set to false.</summary>
    [Fact]
    public void ShowLegend_CanBeSetFalse()
    {
        var cfg = new PlotConfiguration { ShowLegend = false };

        cfg.ShowLegend.Should().BeFalse();
    }
}
