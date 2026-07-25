namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates pie charts from labeled proportional data.</summary>
public static class PiePlot
{
    private const int ArcSegments = 64;
    private const double Radius = 1.0;

    private static readonly string[] DefaultPalette =
    [
        "#4E79A7", "#F28E2B", "#E15759", "#76B7B2",
        "#59A14F", "#EDC948", "#B07AA1", "#FF9DA7",
        "#9C755F", "#BAB0AC"
    ];

    /// <summary>Creates a pie chart from labels and values.</summary>
    /// <param name="labels">Labels for each slice.</param>
    /// <param name="values">Numeric values for each slice (proportional to area).</param>
    /// <returns>A <see cref="Plot2DResult"/> with filled wedge series for each slice.</returns>
    public static Plot2DResult Create(string[] labels, double[] values)
    {
        if (labels is null) throw new System.ArgumentNullException(nameof(labels));
        if (values is null) throw new System.ArgumentNullException(nameof(values));
        if (labels.Length != values.Length)
            throw new System.ArgumentException("labels and values arrays must have the same length.");
        if (labels.Length == 0)
            throw new System.ArgumentException("Input arrays must not be empty.");

        double total = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < 0)
                throw new System.ArgumentException("All values must be non-negative.");
            total += values[i];
        }

        if (total <= 0)
            throw new System.ArgumentException("Sum of values must be positive.");

        var seriesList = new List<PlotSeries>(labels.Length);
        double currentAngle = -System.Math.PI / 2.0;

        for (int i = 0; i < labels.Length; i++)
        {
            double proportion = values[i] / total;
            double sweepAngle = proportion * 2.0 * System.Math.PI;

            if (sweepAngle < 1e-10)
                continue;

            string fillColor = DefaultPalette[i % DefaultPalette.Length];

            var points = new List<Point2D>(ArcSegments + 2);
            points.Add(new Point2D(0, 0));

            for (int j = 0; j <= ArcSegments; j++)
            {
                double angle = currentAngle + sweepAngle * j / ArcSegments;
                points.Add(new Point2D(
                    Radius * System.Math.Cos(angle),
                    Radius * System.Math.Sin(angle)));
            }

            points.Add(new Point2D(0, 0));

            double pct = proportion * 100.0;
            string sliceLabel = $"{labels[i]} ({pct:G3}%)";

            seriesList.Add(new PlotSeries
            {
                Label = sliceLabel,
                Points = points,
                Color = fillColor,
                LineWidth = 1.0,
                LineStyle = LineStyle.Solid,
                Marker = MarkerStyle.None,
                IsFilled = true,
                FillColor = fillColor
            });

            currentAngle += sweepAngle;
        }

        double extent = 1.2;

        return new Plot2DResult
        {
            Series = seriesList,
            XAxis = new PlotAxis
            {
                Label = "",
                Min = -extent,
                Max = extent,
                Ticks = []
            },
            YAxis = new PlotAxis
            {
                Label = "",
                Min = -extent,
                Max = extent,
                Ticks = []
            },
            Title = "",
            ShowGrid = false,
            ShowLegend = true,
            BackgroundColor = "#FFFFFF",
            Bounds = new BoundingBox2D(-extent, -extent, extent, extent)
        };
    }
}
