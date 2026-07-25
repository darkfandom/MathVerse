namespace MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates polar coordinate plots with radial and angular grid lines.</summary>
public static class PolarPlot
{
    private const int CircleSegments = 64;

    /// <summary>Creates a polar plot from angle and radius arrays.</summary>
    /// <param name="angles">Angle values in radians.</param>
    /// <param name="radii">Radius values corresponding to each angle.</param>
    /// <param name="options">Optional plot configuration.</param>
    /// <returns>A <see cref="Plot2DResult"/> with Cartesian-converted data and grid lines.</returns>
    public static Plot2DResult Create(double[] angles, double[] radii, Plot2DOptions? options = null)
    {
        if (angles is null) throw new System.ArgumentNullException(nameof(angles));
        if (radii is null) throw new System.ArgumentNullException(nameof(radii));
        if (angles.Length != radii.Length)
            throw new System.ArgumentException("angles and radii arrays must have the same length.");
        if (angles.Length == 0)
            throw new System.ArgumentException("Input arrays must not be empty.");

        options ??= new Plot2DOptions();

        var dataPoints = new List<Point2D>(angles.Length);
        double maxRadius = 0.0;

        for (int i = 0; i < angles.Length; i++)
        {
            double x = radii[i] * System.Math.Cos(angles[i]);
            double y = radii[i] * System.Math.Sin(angles[i]);
            dataPoints.Add(new Point2D(x, y));

            double absR = System.Math.Abs(radii[i]);
            if (absR > maxRadius) maxRadius = absR;
        }

        if (maxRadius <= 0) maxRadius = 1.0;

        var radialTicks = AxisGenerator.GenerateTicks(0, maxRadius, 5);
        double gridRadius = radialTicks.Count > 0 ? radialTicks[^1].Value : maxRadius;

        var series = new List<PlotSeries>();

        series.Add(new PlotSeries
        {
            Label = "Data",
            Points = dataPoints,
            Color = "#007ACC",
            LineWidth = 2.0,
            LineStyle = LineStyle.Solid,
            Marker = MarkerStyle.Circle,
            MarkerSize = 3.0
        });

        if (options.ShowGrid)
        {
            foreach (var tick in radialTicks)
            {
                if (tick.Value <= 0) continue;

                var circle = new List<Point2D>(CircleSegments + 1);
                for (int i = 0; i <= CircleSegments; i++)
                {
                    double angle = 2.0 * System.Math.PI * i / CircleSegments;
                    circle.Add(new Point2D(
                        tick.Value * System.Math.Cos(angle),
                        tick.Value * System.Math.Sin(angle)));
                }

                series.Add(new PlotSeries
                {
                    Label = $"r={tick.Label}",
                    Points = circle,
                    Color = "#E0E0E0",
                    LineWidth = 0.5,
                    LineStyle = LineStyle.Dotted,
                    Marker = MarkerStyle.None
                });
            }

            for (int a = 0; a < 12; a++)
            {
                double angle = System.Math.PI * a / 6.0;
                var line = new List<Point2D>(2)
                {
                    new Point2D(0, 0),
                    new Point2D(
                        gridRadius * System.Math.Cos(angle),
                        gridRadius * System.Math.Sin(angle))
                };

                series.Add(new PlotSeries
                {
                    Label = $"{a * 30}\u00B0",
                    Points = line,
                    Color = "#E0E0E0",
                    LineWidth = 0.5,
                    LineStyle = LineStyle.Dotted,
                    Marker = MarkerStyle.None
                });
            }
        }

        return new Plot2DResult
        {
            Series = series,
            XAxis = new PlotAxis
            {
                Label = options.XAxisLabel ?? "",
                Min = -gridRadius,
                Max = gridRadius,
                Ticks = AxisGenerator.GenerateTicks(-gridRadius, gridRadius, options.MaxTicks)
            },
            YAxis = new PlotAxis
            {
                Label = options.YAxisLabel ?? "",
                Min = -gridRadius,
                Max = gridRadius,
                Ticks = AxisGenerator.GenerateTicks(-gridRadius, gridRadius, options.MaxTicks)
            },
            Title = options.Title ?? "",
            ShowGrid = options.ShowGrid,
            ShowLegend = options.ShowLegend,
            BackgroundColor = options.BackgroundColor,
            Bounds = new BoundingBox2D(-gridRadius, -gridRadius, gridRadius, gridRadius)
        };
    }
}
