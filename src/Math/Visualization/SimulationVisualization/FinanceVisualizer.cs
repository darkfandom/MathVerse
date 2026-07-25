namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a single candlestick bar.</summary>
public sealed record CandlestickBar
{
    /// <summary>Index or date label.</summary>
    public string? Date { get; init; }

    /// <summary>Open price.</summary>
    public required double Open { get; init; }

    /// <summary>High price.</summary>
    public required double High { get; init; }

    /// <summary>Low price.</summary>
    public required double Low { get; init; }

    /// <summary>Close price.</summary>
    public required double Close { get; init; }

    /// <summary>Body height (|Close - Open|).</summary>
    public required double BodyHeight { get; init; }

    /// <summary>True if bullish (Close > Open).</summary>
    public required bool IsBullish { get; init; }
}

/// <summary>Complete data for candlestick chart visualization.</summary>
public sealed record CandlestickData
{
    /// <summary>Candlestick bars.</summary>
    public required IReadOnlyList<CandlestickBar> Bars { get; init; }

    /// <summary>Overall price minimum (Low).</summary>
    public required double MinPrice { get; init; }

    /// <summary>Overall price maximum (High).</summary>
    public required double MaxPrice { get; init; }
}

/// <summary>Represents a point on the equity curve.</summary>
public sealed record EquityPoint
{
    /// <summary>Index or time step.</summary>
    public required int Index { get; init; }

    /// <summary>Portfolio value.</summary>
    public required double Value { get; init; }

    /// <summary>Normalized value (0-1 range relative to min/max).</summary>
    public required double NormalizedValue { get; init; }
}

/// <summary>Complete data for equity curve visualization.</summary>
public sealed record EquityCurveData
{
    /// <summary>Equity curve points.</summary>
    public required IReadOnlyList<EquityPoint> Points { get; init; }

    /// <summary>Minimum portfolio value.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum portfolio value.</summary>
    public required double MaxValue { get; init; }

    /// <summary>Total return percentage.</summary>
    public required double TotalReturn { get; init; }

    /// <summary>Maximum drawdown percentage.</summary>
    public required double MaxDrawdown { get; init; }
}

/// <summary>Visualizes financial data as candlestick charts and equity curves.</summary>
public sealed class FinanceVisualizer
{
    /// <summary>
    /// Creates a candlestick chart from OHLC data.
    /// </summary>
    /// <param name="open">Opening prices.</param>
    /// <param name="high">High prices.</param>
    /// <param name="low">Low prices.</param>
    /// <param name="close">Closing prices.</param>
    /// <param name="dates">Optional date labels for each bar.</param>
    /// <returns>Candlestick chart data with price range.</returns>
    public CandlestickData CreateCandlestick(
        double[] open,
        double[] high,
        double[] low,
        double[] close,
        string[]? dates = null)
    {
        if (open == null || high == null || low == null || close == null)
        {
            return new CandlestickData
            {
                Bars = [],
                MinPrice = 0.0,
                MaxPrice = 0.0
            };
        }

        int count = System.Math.Min(
            System.Math.Min(open.Length, high.Length),
            System.Math.Min(low.Length, close.Length));

        var bars = new List<CandlestickBar>();
        double minPrice = double.MaxValue;
        double maxPrice = double.MinValue;

        for (int i = 0; i < count; i++)
        {
            bool bullish = close[i] >= open[i];
            double bodyHeight = System.Math.Abs(close[i] - open[i]);

            if (low[i] < minPrice) minPrice = low[i];
            if (high[i] > maxPrice) maxPrice = high[i];

            bars.Add(new CandlestickBar
            {
                Date = dates != null && i < dates.Length ? dates[i] : null,
                Open = open[i],
                High = high[i],
                Low = low[i],
                Close = close[i],
                BodyHeight = bodyHeight,
                IsBullish = bullish
            });
        }

        return new CandlestickData
        {
            Bars = bars,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
    }

    /// <summary>
    /// Creates an equity curve visualization with performance metrics.
    /// </summary>
    /// <param name="values">Portfolio values over time.</param>
    /// <returns>Equity curve data with return and drawdown metrics.</returns>
    public EquityCurveData CreateEquityCurve(double[] values)
    {
        if (values == null || values.Length == 0)
        {
            return new EquityCurveData
            {
                Points = [],
                MinValue = 0.0,
                MaxValue = 0.0,
                TotalReturn = 0.0,
                MaxDrawdown = 0.0
            };
        }

        double min = values[0];
        double max = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
        }

        double range = max - min;
        var points = new List<EquityPoint>();

        for (int i = 0; i < values.Length; i++)
        {
            double normalized = range > 1e-15 ? (values[i] - min) / range : 0.0;
            points.Add(new EquityPoint
            {
                Index = i,
                Value = values[i],
                NormalizedValue = normalized
            });
        }

        double totalReturn = values[0] > 1e-15
            ? (values[^1] - values[0]) / values[0] * 100.0
            : 0.0;

        double peak = values[0];
        double maxDrawdown = 0.0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > peak) peak = values[i];
            double drawdown = peak > 1e-15 ? (peak - values[i]) / peak : 0.0;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }

        return new EquityCurveData
        {
            Points = points,
            MinValue = min,
            MaxValue = max,
            TotalReturn = totalReturn,
            MaxDrawdown = maxDrawdown * 100.0
        };
    }
}
