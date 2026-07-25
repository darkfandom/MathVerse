namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single cell in the heatmap grid.</summary>
public sealed record HeatmapCell
{
    /// <summary>Row index.</summary>
    public required int Row { get; init; }

    /// <summary>Column index.</summary>
    public required int Col { get; init; }

    /// <summary>Raw value at this cell.</summary>
    public required double Value { get; init; }

    /// <summary>Normalized value (0-1).</summary>
    public required double NormalizedValue { get; init; }

    /// <summary>Red component (0-1).</summary>
    public double R { get; init; }

    /// <summary>Green component (0-1).</summary>
    public double G { get; init; }

    /// <summary>Blue component (0-1).</summary>
    public double B { get; init; }
}

/// <summary>Complete data for a tensor heatmap visualization.</summary>
public sealed record TensorHeatmapData
{
    /// <summary>Grid cells with values and colors.</summary>
    public required IReadOnlyList<IReadOnlyList<HeatmapCell>> Grid { get; init; }

    /// <summary>Number of rows.</summary>
    public required int Rows { get; init; }

    /// <summary>Number of columns.</summary>
    public required int Cols { get; init; }

    /// <summary>Minimum value in the data.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum value in the data.</summary>
    public required double MaxValue { get; init; }

    /// <summary>Name of the color map used.</summary>
    public required string ColorMapName { get; init; }
}

/// <summary>Visualizes a 2D tensor as a color-coded heatmap.</summary>
public sealed class TensorHeatmap
{
    /// <summary>
    /// Creates a heatmap from 2D tensor data with specified color map.
    /// </summary>
    /// <param name="data">Flat array of values (row-major order).</param>
    /// <param name="rows">Number of rows.</param>
    /// <param name="cols">Number of columns.</param>
    /// <param name="colorMap">Color map name: "Viridis", "Plasma", "Inferno", "Magma", "Turbo", "Grayscale", "Jet", "Rainbow", "Coolwarm", "RdBu".</param>
    /// <returns>Complete heatmap data with colored cells.</returns>
    public TensorHeatmapData Create(double[] data, int rows, int cols, string colorMap = "Viridis")
    {
        if (data == null || data.Length == 0)
        {
            return new TensorHeatmapData
            {
                Grid = [],
                Rows = 0,
                Cols = 0,
                MinValue = 0.0,
                MaxValue = 0.0,
                ColorMapName = colorMap
            };
        }

        double min = data[0];
        double max = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            if (data[i] < min) min = data[i];
            if (data[i] > max) max = data[i];
        }

        var grid = new List<IReadOnlyList<HeatmapCell>>();
        double range = max - min;

        for (int r = 0; r < rows; r++)
        {
            var row = new List<HeatmapCell>();
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                double value = idx < data.Length ? data[idx] : 0.0;
                double normalized = range > 1e-15 ? (value - min) / range : 0.0;

                (double rv, double gv, double bv) = ApplyColorMap(normalized, colorMap);

                row.Add(new HeatmapCell
                {
                    Row = r,
                    Col = c,
                    Value = value,
                    NormalizedValue = normalized,
                    R = rv,
                    G = gv,
                    B = bv
                });
            }
            grid.Add(row);
        }

        return new TensorHeatmapData
        {
            Grid = grid,
            Rows = rows,
            Cols = cols,
            MinValue = min,
            MaxValue = max,
            ColorMapName = colorMap
        };
    }

    private static (double r, double g, double b) ApplyColorMap(double t, string mapName)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        return mapName.ToLowerInvariant() switch
        {
            "viridis" => ViridisColor(t),
            "plasma" => PlasmaColor(t),
            "inferno" => InfernoColor(t),
            "magma" => MagmaColor(t),
            "turbo" => TurboColor(t),
            "grayscale" => (t, t, t),
            "jet" => JetColor(t),
            "rainbow" => RainbowColor(t),
            "coolwarm" => CoolwarmColor(t),
            "rdbu" => RdBuColor(t),
            _ => ViridisColor(t)
        };
    }

    private static (double r, double g, double b) ViridisColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.267004, 0.004874, 0.329415],
            [0.07, 0.282327, 0.140926, 0.457517],
            [0.14, 0.253935, 0.265254, 0.529983],
            [0.21, 0.194026, 0.407061, 0.557050],
            [0.28, 0.127568, 0.566949, 0.551229],
            [0.35, 0.070600, 0.694293, 0.499732],
            [0.42, 0.119208, 0.786923, 0.397928],
            [0.49, 0.283008, 0.859415, 0.265007],
            [0.56, 0.468049, 0.897475, 0.157388],
            [0.63, 0.647009, 0.888069, 0.091574],
            [0.70, 0.816658, 0.847280, 0.078447],
            [0.77, 0.934850, 0.788888, 0.147713],
            [0.84, 0.971237, 0.669400, 0.181784],
            [0.91, 0.940015, 0.499239, 0.209724],
            [1.0, 0.988362, 0.305861, 0.317971]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) PlasmaColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.050383, 0.029803, 0.527975],
            [0.07, 0.167139, 0.021675, 0.559794],
            [0.14, 0.284939, 0.013218, 0.553439],
            [0.21, 0.385377, 0.004353, 0.516706],
            [0.28, 0.473062, 0.015023, 0.457958],
            [0.35, 0.548495, 0.071881, 0.388273],
            [0.42, 0.610617, 0.157349, 0.319777],
            [0.49, 0.661322, 0.241729, 0.247390],
            [0.56, 0.703234, 0.324343, 0.184576],
            [0.63, 0.736036, 0.406015, 0.130124],
            [0.70, 0.760638, 0.488352, 0.090915],
            [0.77, 0.776728, 0.572635, 0.065525],
            [0.84, 0.785105, 0.659710, 0.047982],
            [0.91, 0.786889, 0.749648, 0.044228],
            [1.0, 0.781178, 0.841941, 0.026693]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) InfernoColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.001462, 0.000466, 0.013866],
            [0.07, 0.064492, 0.019437, 0.136646],
            [0.14, 0.206364, 0.029396, 0.273293],
            [0.21, 0.341393, 0.025752, 0.335300],
            [0.28, 0.474024, 0.023244, 0.325859],
            [0.35, 0.597613, 0.058546, 0.253344],
            [0.42, 0.703983, 0.147086, 0.135018],
            [0.49, 0.791270, 0.278557, 0.061907],
            [0.56, 0.856387, 0.425350, 0.024355],
            [0.63, 0.898393, 0.572706, 0.042368],
            [0.70, 0.922470, 0.715256, 0.083681],
            [0.77, 0.933478, 0.835552, 0.176423],
            [0.84, 0.927224, 0.923315, 0.338965],
            [0.91, 0.950532, 0.965310, 0.562702],
            [1.0, 0.988362, 0.998364, 0.644924]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) MagmaColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.001462, 0.000466, 0.013866],
            [0.07, 0.080558, 0.020758, 0.162255],
            [0.14, 0.214800, 0.026149, 0.327413],
            [0.21, 0.342230, 0.022012, 0.387288],
            [0.28, 0.465223, 0.015833, 0.385818],
            [0.35, 0.580741, 0.018889, 0.337129],
            [0.42, 0.691484, 0.058861, 0.281088],
            [0.49, 0.790670, 0.148782, 0.233602],
            [0.56, 0.872336, 0.270714, 0.192110],
            [0.63, 0.931130, 0.409796, 0.182323],
            [0.70, 0.962831, 0.563285, 0.212297],
            [0.77, 0.974022, 0.714282, 0.290478],
            [0.84, 0.973125, 0.850485, 0.440363],
            [0.91, 0.977609, 0.938990, 0.645829],
            [1.0, 0.987002, 0.991443, 0.749837]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) TurboColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.18995, 0.07176, 0.23299],
            [0.07, 0.25107, 0.25237, 0.54029],
            [0.14, 0.17653, 0.42643, 0.59743],
            [0.21, 0.11934, 0.57580, 0.54942],
            [0.28, 0.21280, 0.70663, 0.38492],
            [0.35, 0.42481, 0.79335, 0.21461],
            [0.42, 0.64763, 0.83055, 0.07922],
            [0.49, 0.83563, 0.82455, 0.04835],
            [0.56, 0.95920, 0.75023, 0.04798],
            [0.63, 0.98714, 0.62751, 0.06330],
            [0.70, 0.97006, 0.47171, 0.09835],
            [0.77, 0.90380, 0.31735, 0.15702],
            [0.84, 0.79654, 0.18045, 0.21894],
            [0.91, 0.65521, 0.07367, 0.27411],
            [1.0, 0.48306, 0.01387, 0.26014]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) JetColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.0, 0.0, 0.5],
            [0.11, 0.0, 0.0, 1.0],
            [0.35, 0.0, 1.0, 1.0],
            [0.5, 0.0, 1.0, 0.0],
            [0.65, 1.0, 1.0, 0.0],
            [0.89, 1.0, 0.0, 0.0],
            [1.0, 0.5, 0.0, 0.0]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) RainbowColor(double t)
    {
        double hue = t * 300.0;
        return HsvToRgb(hue / 360.0, 1.0, 1.0);
    }

    private static (double r, double g, double b) CoolwarmColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.230, 0.299, 0.754],
            [0.1, 0.291, 0.434, 0.816],
            [0.2, 0.404, 0.561, 0.863],
            [0.3, 0.565, 0.675, 0.890],
            [0.4, 0.745, 0.788, 0.902],
            [0.5, 0.865, 0.865, 0.865],
            [0.6, 0.918, 0.722, 0.647],
            [0.7, 0.902, 0.569, 0.463],
            [0.8, 0.839, 0.392, 0.310],
            [0.9, 0.745, 0.216, 0.173],
            [1.0, 0.612, 0.090, 0.098]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) RdBuColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.045, 0.055, 0.388],
            [0.1, 0.128, 0.173, 0.518],
            [0.2, 0.255, 0.337, 0.635],
            [0.3, 0.435, 0.506, 0.737],
            [0.4, 0.639, 0.675, 0.808],
            [0.5, 0.839, 0.839, 0.839],
            [0.6, 0.855, 0.663, 0.592],
            [0.7, 0.843, 0.478, 0.400],
            [0.8, 0.776, 0.294, 0.243],
            [0.9, 0.671, 0.141, 0.122],
            [1.0, 0.404, 0.020, 0.047]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double r, double g, double b) InterpolateStops(double[][] stops, double t)
    {
        if (t <= stops[0][0])
            return (stops[0][1], stops[0][2], stops[0][3]);
        if (t >= stops[^1][0])
            return (stops[^1][1], stops[^1][2], stops[^1][3]);

        for (int i = 0; i < stops.Length - 1; i++)
        {
            if (t >= stops[i][0] && t <= stops[i + 1][0])
            {
                double span = stops[i + 1][0] - stops[i][0];
                double localT = span > 1e-15 ? (t - stops[i][0]) / span : 0.0;

                double r = stops[i][1] + (stops[i + 1][1] - stops[i][1]) * localT;
                double g = stops[i][2] + (stops[i + 1][2] - stops[i][2]) * localT;
                double b = stops[i][3] + (stops[i + 1][3] - stops[i][3]) * localT;
                return (r, g, b);
            }
        }

        return (stops[^1][1], stops[^1][2], stops[^1][3]);
    }

    private static (double r, double g, double b) HsvToRgb(double h, double s, double v)
    {
        h = h - System.Math.Floor(h);
        double c = v * s;
        double x = c * (1.0 - System.Math.Abs((h * 6.0) % 2.0 - 1.0));
        double m = v - c;

        double r1, g1, b1;
        int sector = (int)(h * 6.0);

        switch (sector)
        {
            case 0: r1 = c; g1 = x; b1 = 0.0; break;
            case 1: r1 = x; g1 = c; b1 = 0.0; break;
            case 2: r1 = 0.0; g1 = c; b1 = x; break;
            case 3: r1 = 0.0; g1 = x; b1 = c; break;
            case 4: r1 = x; g1 = 0.0; b1 = c; break;
            default: r1 = c; g1 = 0.0; b1 = x; break;
        }

        return (r1 + m, g1 + m, b1 + m);
    }
}
