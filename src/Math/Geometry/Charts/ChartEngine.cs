using System.Collections.Immutable;
using MathVerse.Math.Geometry.Colors;

namespace MathVerse.Math.Geometry.Charts;

/// <summary>Provides methods for generating various types of charts.</summary>
public sealed class ChartEngine
{
    /// <summary>Creates a line chart from the given data series.</summary>
    /// <param name="series">The data series to plot.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateLineChart(IReadOnlyList<Series> series, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;
            var validatedSeries = ValidateAndNormalizeSeries(series);
            return new ChartResult(cfg, true, null, validatedSeries, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateLineChart failed: {ex.Message}");
        }
    }

    /// <summary>Creates an area chart from the given data series.</summary>
    /// <param name="series">The data series to plot.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateAreaChart(IReadOnlyList<Series> series, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;
            var validatedSeries = ValidateAndNormalizeSeries(series);
            return new ChartResult(cfg, true, null, validatedSeries, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateAreaChart failed: {ex.Message}");
        }
    }

    /// <summary>Creates a bar chart from the given data series.</summary>
    /// <param name="series">The data series to plot.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateBarChart(IReadOnlyList<Series> series, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;
            var validatedSeries = ValidateAndNormalizeSeries(series);
            return new ChartResult(cfg, true, null, validatedSeries, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateBarChart failed: {ex.Message}");
        }
    }

    /// <summary>Creates a pie chart from the given slices.</summary>
    /// <param name="slices">The pie chart slices.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreatePieChart(IReadOnlyList<PieSlice> slices, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;

            if (slices.Count == 0)
            {
                return ChartResult.Failed("No slices provided for pie chart.");
            }

            double total = 0.0;
            for (int i = 0; i < slices.Count; i++)
            {
                total += slices[i].Value;
            }

            if (total <= 0.0)
            {
                return ChartResult.Failed("Total slice value must be positive.");
            }

            var normalizedSlices = new List<PieSlice>();
            for (int i = 0; i < slices.Count; i++)
            {
                normalizedSlices.Add(new PieSlice(slices[i].Label, slices[i].Value / total, slices[i].Color));
            }

            return new ChartResult(cfg, true, null, ImmutableArray<Series>.Empty, normalizedSlices);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreatePieChart failed: {ex.Message}");
        }
    }

    /// <summary>Creates a histogram chart from the given values.</summary>
    /// <param name="values">The values to bin.</param>
    /// <param name="bins">The number of bins.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateHistogramChart(IReadOnlyList<double> values, int bins, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;

            if (values.Count == 0)
            {
                return ChartResult.Failed("No values provided for histogram.");
            }

            double min = values[0];
            double max = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] < min) min = values[i];
                if (values[i] > max) max = values[i];
            }

            if (System.Math.Abs(max - min) < 1e-15)
            {
                max = min + 1.0;
            }

            double binWidth = (max - min) / bins;
            var counts = new int[bins];
            var points = ImmutableArray<(double X, double Y)>.Empty;

            for (int i = 0; i < values.Count; i++)
            {
                int binIndex = (int)((values[i] - min) / binWidth);
                if (binIndex >= bins) binIndex = bins - 1;
                if (binIndex < 0) binIndex = 0;
                counts[binIndex]++;
            }

            for (int i = 0; i < bins; i++)
            {
                double x = min + (i + 0.5) * binWidth;
                points = points.Add((x, counts[i]));
            }

            var histogramSeries = new Series("histogram", Color.Blue, points);
            return new ChartResult(cfg, true, null, new List<Series> { histogramSeries }, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateHistogramChart failed: {ex.Message}");
        }
    }

    /// <summary>Creates a box plot from the given data.</summary>
    /// <param name="data">The box plot data.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateBoxPlot(IReadOnlyList<BoxPlotData> data, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;

            if (data.Count == 0)
            {
                return ChartResult.Failed("No data provided for box plot.");
            }

            var seriesList = new List<Series>();
            ColorPalette palette = ColorPalette.Default;

            for (int i = 0; i < data.Count; i++)
            {
                BoxPlotData box = data[i];
                Color color = palette.GetColor(i);

                var points = ImmutableArray.Create(
                    ((double)i, box.Min),
                    ((double)i, box.Q1),
                    ((double)i, box.Median),
                    ((double)i, box.Q3),
                    ((double)i, box.Max));

                seriesList.Add(new Series(box.Label, color, points));
            }

            return new ChartResult(cfg, true, null, seriesList, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateBoxPlot failed: {ex.Message}");
        }
    }

    /// <summary>Creates a candlestick chart from the given data.</summary>
    /// <param name="data">The candlestick data.</param>
    /// <param name="config">Optional chart configuration.</param>
    /// <returns>A <see cref="ChartResult"/> containing the chart data.</returns>
    public ChartResult CreateCandlestickChart(IReadOnlyList<CandlestickData> data, ChartConfiguration? config = null)
    {
        try
        {
            ChartConfiguration cfg = config ?? ChartConfiguration.Default;

            if (data.Count == 0)
            {
                return ChartResult.Failed("No data provided for candlestick chart.");
            }

            var points = ImmutableArray<(double X, double Y)>.Empty;
            var colors = ImmutableArray<Color>.Empty;

            for (int i = 0; i < data.Count; i++)
            {
                CandlestickData candle = data[i];
                double mid = (candle.Open + candle.Close) / 2.0;
                points = points.Add(((double)i, mid));
                colors = colors.Add(candle.Close >= candle.Open ? Color.Green : Color.Red);
            }

            Color primaryColor = colors.Length > 0 ? colors[0] : Color.Green;
            var candleSeries = new Series("candlestick", primaryColor, points);
            return new ChartResult(cfg, true, null, new List<Series> { candleSeries }, ImmutableArray<PieSlice>.Empty);
        }
        catch (Exception ex)
        {
            return ChartResult.Failed($"CreateCandlestickChart failed: {ex.Message}");
        }
    }

    /// <summary>Validates and normalizes the input series.</summary>
    /// <param name="series">The input series.</param>
    /// <returns>A validated list of <see cref="Series"/>.</returns>
    private static IReadOnlyList<Series> ValidateAndNormalizeSeries(IReadOnlyList<Series> series)
    {
        var result = new List<Series>();
        ColorPalette palette = ColorPalette.Default;

        for (int i = 0; i < series.Count; i++)
        {
            Series s = series[i];
            Color color = s.Color.A < 0.01 ? palette.GetColor(i) : s.Color;
            result.Add(new Series(s.Name, color, s.Points));
        }

        return result;
    }
}
