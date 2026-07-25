using System.Collections.Immutable;

namespace MathVerse.Geometry.Tests.Charts;

/// <summary>Tests for the <see cref="ChartEngine"/> class.</summary>
public class ChartEngineTests
{
    /// <summary>Verifies that CreateLineChart returns a successful result.</summary>
    [Fact]
    public void CreateLineChart_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("Series1", Color.Blue, ImmutableArray.Create((1.0, 2.0), (3.0, 4.0)))
        };

        var result = engine.CreateLineChart(series);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateAreaChart returns a successful result.</summary>
    [Fact]
    public void CreateAreaChart_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("Area1", Color.Green, ImmutableArray.Create((1.0, 3.0)))
        };

        var result = engine.CreateAreaChart(series);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateBarChart returns a successful result.</summary>
    [Fact]
    public void CreateBarChart_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("Bar1", Color.Red, ImmutableArray.Create((1.0, 5.0)))
        };

        var result = engine.CreateBarChart(series);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreatePieChart normalizes slice values.</summary>
    [Fact]
    public void CreatePieChart_NormalizesSlices()
    {
        var engine = new ChartEngine();
        var slices = new List<PieSlice>
        {
            new("A", 30, Color.Red),
            new("B", 70, Color.Blue)
        };

        var result = engine.CreatePieChart(slices);

        result.Success.Should().BeTrue();
        result.PieSlices[0].Value.Should().BeApproximately(0.3, 1e-10);
        result.PieSlices[1].Value.Should().BeApproximately(0.7, 1e-10);
    }

    /// <summary>Verifies that CreateHistogramChart returns a successful result.</summary>
    [Fact]
    public void CreateHistogramChart_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var values = new List<double> { 1, 2, 3, 4, 5 };

        var result = engine.CreateHistogramChart(values, 3);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateBoxPlot returns a successful result.</summary>
    [Fact]
    public void CreateBoxPlot_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var data = new List<BoxPlotData>
        {
            new("Box1", 1, 3, 5, 7, 9, ImmutableArray<double>.Empty)
        };

        var result = engine.CreateBoxPlot(data);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateCandlestickChart returns a successful result.</summary>
    [Fact]
    public void CreateCandlestickChart_ReturnsSuccess()
    {
        var engine = new ChartEngine();
        var data = new List<CandlestickData>
        {
            new(DateTime.Now, 100, 110, 90, 105)
        };

        var result = engine.CreateCandlestickChart(data);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateLineChart with empty series returns success.</summary>
    [Fact]
    public void CreateLineChart_EmptySeries_ReturnsSuccess()
    {
        var engine = new ChartEngine();

        var result = engine.CreateLineChart(new List<Series>());

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreateBarChart with empty series returns success.</summary>
    [Fact]
    public void CreateBarChart_EmptySeries_ReturnsSuccess()
    {
        var engine = new ChartEngine();

        var result = engine.CreateBarChart(new List<Series>());

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that CreatePieChart with empty slices returns failure.</summary>
    [Fact]
    public void CreatePieChart_EmptySlices_ReturnsFailure()
    {
        var engine = new ChartEngine();

        var result = engine.CreatePieChart(new List<PieSlice>());

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that CreateHistogramChart with empty values returns failure.</summary>
    [Fact]
    public void CreateHistogramChart_EmptyValues_ReturnsFailure()
    {
        var engine = new ChartEngine();

        var result = engine.CreateHistogramChart(new List<double>(), 5);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that CreateBoxPlot with empty data returns failure.</summary>
    [Fact]
    public void CreateBoxPlot_EmptyData_ReturnsFailure()
    {
        var engine = new ChartEngine();

        var result = engine.CreateBoxPlot(new List<BoxPlotData>());

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that CreateCandlestickChart with empty data returns failure.</summary>
    [Fact]
    public void CreateCandlestickChart_EmptyData_ReturnsFailure()
    {
        var engine = new ChartEngine();

        var result = engine.CreateCandlestickChart(new List<CandlestickData>());

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that CreateLineChart with single series returns that series.</summary>
    [Fact]
    public void CreateLineChart_SingleSeries_ReturnsSingleSeries()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("Only", Color.Red, ImmutableArray.Create((0.0, 0.0)))
        };

        var result = engine.CreateLineChart(series);

        result.Series.Should().HaveCount(1);
    }

    /// <summary>Verifies that CreateLineChart with multiple series returns all series.</summary>
    [Fact]
    public void CreateLineChart_MultipleSeries_ReturnsAll()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("A", Color.Red, ImmutableArray.Create((0.0, 0.0))),
            new("B", Color.Blue, ImmutableArray.Create((1.0, 1.0))),
            new("C", Color.Green, ImmutableArray.Create((2.0, 2.0)))
        };

        var result = engine.CreateLineChart(series);

        result.Series.Should().HaveCount(3);
    }

    /// <summary>Verifies that CreateBarChart with single series returns that series.</summary>
    [Fact]
    public void CreateBarChart_SingleSeries_ReturnsSingleSeries()
    {
        var engine = new ChartEngine();
        var series = new List<Series>
        {
            new("Bar", Color.Cyan, ImmutableArray.Create((1.0, 10.0)))
        };

        var result = engine.CreateBarChart(series);

        result.Series.Should().HaveCount(1);
    }

    /// <summary>Verifies that CreatePieChart with single slice returns that slice.</summary>
    [Fact]
    public void CreatePieChart_SingleSlice_ReturnsSingleSlice()
    {
        var engine = new ChartEngine();
        var slices = new List<PieSlice>
        {
            new("Only", 100, Color.Yellow)
        };

        var result = engine.CreatePieChart(slices);

        result.PieSlices.Should().HaveCount(1);
    }

    /// <summary>Verifies that CreateHistogramChart generates series data.</summary>
    [Fact]
    public void CreateHistogramChart_GeneratesSeries()
    {
        var engine = new ChartEngine();
        var values = Enumerable.Range(0, 50).Select(i => (double)i).ToList();

        var result = engine.CreateHistogramChart(values, 5);

        result.Series.Should().HaveCount(1);
    }

    /// <summary>Verifies that CreateBoxPlot generates series for each box.</summary>
    [Fact]
    public void CreateBoxPlot_GeneratesMultipleSeries()
    {
        var engine = new ChartEngine();
        var data = new List<BoxPlotData>
        {
            new("A", 1, 2, 3, 4, 5, ImmutableArray<double>.Empty),
            new("B", 6, 7, 8, 9, 10, ImmutableArray<double>.Empty)
        };

        var result = engine.CreateBoxPlot(data);

        result.Series.Should().HaveCount(2);
    }

    /// <summary>Verifies that CreateCandlestickChart with bullish candle uses green.</summary>
    [Fact]
    public void CreateCandlestickChart_Bullish_UsesGreen()
    {
        var engine = new ChartEngine();
        var data = new List<CandlestickData>
        {
            new(DateTime.Now, 100, 110, 90, 105)
        };

        var result = engine.CreateCandlestickChart(data);

        result.Series[0].Color.Should().Be(Color.Green);
    }

    /// <summary>Verifies that CreateCandlestickChart with bearish candle uses red.</summary>
    [Fact]
    public void CreateCandlestickChart_Bearish_UsesRed()
    {
        var engine = new ChartEngine();
        var data = new List<CandlestickData>
        {
            new(DateTime.Now, 105, 110, 90, 100)
        };

        var result = engine.CreateCandlestickChart(data);

        result.Series[0].Color.Should().Be(Color.Red);
    }
}
