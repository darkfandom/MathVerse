namespace MathVerse.Math.Visualization.Color;

/// <summary>Scientific color maps using piecewise linear interpolation through control points.</summary>
public sealed class ColorMap
{
    /// <summary>
    /// Viridis perceptually uniform color map.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Viridis(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.267004, 0.004874, 0.329415],
            [0.0500, 0.275701, 0.092173, 0.419040],
            [0.1000, 0.253935, 0.265254, 0.529983],
            [0.1500, 0.221940, 0.347435, 0.553710],
            [0.2000, 0.194026, 0.407061, 0.557050],
            [0.2500, 0.163520, 0.471221, 0.556058],
            [0.3000, 0.127568, 0.566949, 0.551229],
            [0.3500, 0.098747, 0.634154, 0.528230],
            [0.4000, 0.082621, 0.694293, 0.499732],
            [0.4500, 0.096422, 0.743533, 0.463039],
            [0.5000, 0.150473, 0.782725, 0.416354],
            [0.5500, 0.233947, 0.817568, 0.366973],
            [0.6000, 0.335044, 0.843108, 0.313150],
            [0.6500, 0.450723, 0.864128, 0.252805],
            [0.7000, 0.575481, 0.872835, 0.183370],
            [0.7500, 0.701568, 0.872983, 0.122134],
            [0.8000, 0.815402, 0.853742, 0.098855],
            [0.8500, 0.900583, 0.816735, 0.109427],
            [0.9000, 0.949217, 0.753042, 0.154362],
            [0.9500, 0.972862, 0.663402, 0.204919],
            [1.0000, 0.988362, 0.553068, 0.277795]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Plasma perceptually uniform color map.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Plasma(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.050383, 0.029803, 0.527975],
            [0.0500, 0.113647, 0.024464, 0.546915],
            [0.1000, 0.180444, 0.017518, 0.556272],
            [0.1500, 0.249526, 0.008916, 0.555414],
            [0.2000, 0.320453, 0.000462, 0.543575],
            [0.2500, 0.392036, 0.005732, 0.520982],
            [0.3000, 0.462178, 0.036839, 0.488641],
            [0.3500, 0.529682, 0.089273, 0.449337],
            [0.4000, 0.594244, 0.157493, 0.405744],
            [0.4500, 0.655338, 0.233381, 0.358235],
            [0.5000, 0.712076, 0.310488, 0.308261],
            [0.5500, 0.764172, 0.386069, 0.255718],
            [0.6000, 0.810604, 0.463804, 0.200716],
            [0.6500, 0.850981, 0.543999, 0.144639],
            [0.7000, 0.885181, 0.626620, 0.091696],
            [0.7500, 0.912859, 0.710952, 0.050014],
            [0.8000, 0.933776, 0.796603, 0.026830],
            [0.8500, 0.947659, 0.882430, 0.025843],
            [0.9000, 0.954486, 0.967380, 0.057715],
            [0.9500, 0.946390, 0.981920, 0.147171],
            [1.0000, 0.940015, 0.979829, 0.266613]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Inferno perceptually uniform color map.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Inferno(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.001462, 0.000466, 0.013866],
            [0.0500, 0.038673, 0.011480, 0.095608],
            [0.1000, 0.107350, 0.021974, 0.229871],
            [0.1500, 0.206364, 0.029396, 0.273293],
            [0.2000, 0.309411, 0.024317, 0.325859],
            [0.2500, 0.409417, 0.019723, 0.345589],
            [0.3000, 0.506532, 0.037843, 0.315075],
            [0.3500, 0.594850, 0.083159, 0.257394],
            [0.4000, 0.672163, 0.149441, 0.190063],
            [0.4500, 0.741590, 0.230495, 0.122801],
            [0.5000, 0.802008, 0.319005, 0.073243],
            [0.5500, 0.853741, 0.408726, 0.043547],
            [0.6000, 0.896372, 0.500725, 0.033961],
            [0.6500, 0.928165, 0.590417, 0.052183],
            [0.7000, 0.949764, 0.677000, 0.089443],
            [0.7500, 0.961315, 0.760222, 0.147272],
            [0.8000, 0.963580, 0.840037, 0.227033],
            [0.8500, 0.957928, 0.915970, 0.334103],
            [0.9000, 0.945302, 0.963426, 0.459500],
            [0.9500, 0.960218, 0.984243, 0.571928],
            [1.0000, 0.988362, 0.998364, 0.644924]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Magma perceptually uniform color map.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Magma(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.001462, 0.000466, 0.013866],
            [0.0500, 0.045478, 0.013451, 0.113712],
            [0.1000, 0.126308, 0.023506, 0.239668],
            [0.1500, 0.214800, 0.026149, 0.327413],
            [0.2000, 0.310831, 0.025204, 0.371240],
            [0.2500, 0.404743, 0.021432, 0.375243],
            [0.3000, 0.496098, 0.017001, 0.354346],
            [0.3500, 0.584267, 0.021610, 0.323041],
            [0.4000, 0.668476, 0.048141, 0.284219],
            [0.4500, 0.746743, 0.099998, 0.246082],
            [0.5000, 0.818387, 0.176575, 0.211147],
            [0.5500, 0.881362, 0.267494, 0.184027],
            [0.6000, 0.931130, 0.368241, 0.178225],
            [0.6500, 0.964290, 0.472356, 0.194431],
            [0.7000, 0.977004, 0.574094, 0.225714],
            [0.7500, 0.976922, 0.674182, 0.282204],
            [0.8000, 0.973542, 0.770800, 0.361284],
            [0.8500, 0.971629, 0.854870, 0.457318],
            [0.9000, 0.973015, 0.922997, 0.571633],
            [0.9500, 0.978957, 0.965314, 0.687753],
            [1.0000, 0.987002, 0.991443, 0.749837]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Turbo fast rainbow color map with smooth transitions.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Turbo(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.18995, 0.07176, 0.23299],
            [0.0500, 0.21742, 0.15108, 0.38400],
            [0.1000, 0.23935, 0.24056, 0.50046],
            [0.1500, 0.22937, 0.33606, 0.56189],
            [0.2000, 0.18221, 0.42736, 0.57311],
            [0.2500, 0.13582, 0.51044, 0.55335],
            [0.3000, 0.10550, 0.59036, 0.51417],
            [0.3500, 0.11077, 0.66322, 0.46401],
            [0.4000, 0.19168, 0.72934, 0.40408],
            [0.4500, 0.32668, 0.78705, 0.33912],
            [0.5000, 0.48310, 0.83481, 0.27324],
            [0.5500, 0.63006, 0.86638, 0.20353],
            [0.6000, 0.76264, 0.88156, 0.13531],
            [0.6500, 0.86004, 0.87718, 0.08286],
            [0.7000, 0.93204, 0.85163, 0.06220],
            [0.7500, 0.96936, 0.80144, 0.07766],
            [0.8000, 0.97157, 0.73256, 0.11659],
            [0.8500, 0.94892, 0.64733, 0.14693],
            [0.9000, 0.90246, 0.54205, 0.16135],
            [0.9500, 0.84047, 0.42399, 0.16392],
            [1.0000, 0.76963, 0.30037, 0.15063]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Simple grayscale color map.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Grayscale(double t)
    {
        double v = System.Math.Clamp(t, 0.0, 1.0);
        return (v, v, v);
    }

    /// <summary>
    /// Classic Jet color map (blue to red).
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Jet(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.00000, 0.00000, 0.50000],
            [0.0700, 0.00000, 0.00000, 0.70000],
            [0.1100, 0.00000, 0.00000, 1.00000],
            [0.2200, 0.00000, 0.50000, 1.00000],
            [0.3300, 0.00000, 0.75000, 1.00000],
            [0.4400, 0.00000, 1.00000, 1.00000],
            [0.5000, 0.00000, 1.00000, 0.50000],
            [0.5600, 0.50000, 1.00000, 0.00000],
            [0.6700, 1.00000, 1.00000, 0.00000],
            [0.7800, 1.00000, 0.50000, 0.00000],
            [0.8900, 1.00000, 0.00000, 0.00000],
            [1.0000, 0.50000, 0.00000, 0.00000]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Rainbow color map (HSV hue-based).
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Rainbow(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        double h = t * 300.0 / 360.0;
        double s = 1.0;
        double v = 1.0;
        return HsvToRgb(h, s, v);
    }

    /// <summary>
    /// Coolwarm diverging color map (blue-white-red).
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) Coolwarm(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.230, 0.299, 0.754],
            [0.0500, 0.260, 0.366, 0.785],
            [0.1000, 0.291, 0.434, 0.816],
            [0.1500, 0.325, 0.498, 0.840],
            [0.2000, 0.364, 0.561, 0.863],
            [0.2500, 0.410, 0.619, 0.877],
            [0.3000, 0.465, 0.675, 0.890],
            [0.3500, 0.528, 0.727, 0.897],
            [0.4000, 0.597, 0.774, 0.902],
            [0.4500, 0.670, 0.817, 0.902],
            [0.5000, 0.865, 0.865, 0.865],
            [0.5500, 0.880, 0.780, 0.750],
            [0.6000, 0.892, 0.722, 0.647],
            [0.6500, 0.888, 0.657, 0.554],
            [0.7000, 0.870, 0.584, 0.463],
            [0.7500, 0.843, 0.506, 0.380],
            [0.8000, 0.810, 0.420, 0.310],
            [0.8500, 0.769, 0.335, 0.246],
            [0.9000, 0.721, 0.245, 0.173],
            [0.9500, 0.663, 0.158, 0.130],
            [1.0000, 0.612, 0.090, 0.098]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// RdBu diverging color map (red-blue, suitable for positive/negative data).
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) RdBu(double t)
    {
        double[][] stops =
        [
            [0.0000, 0.404, 0.020, 0.047],
            [0.0500, 0.537, 0.080, 0.083],
            [0.1000, 0.671, 0.141, 0.122],
            [0.1500, 0.776, 0.213, 0.163],
            [0.2000, 0.843, 0.294, 0.210],
            [0.2500, 0.883, 0.380, 0.278],
            [0.3000, 0.855, 0.478, 0.348],
            [0.3500, 0.815, 0.563, 0.417],
            [0.4000, 0.765, 0.640, 0.490],
            [0.4500, 0.808, 0.722, 0.570],
            [0.5000, 0.839, 0.839, 0.839],
            [0.5500, 0.643, 0.737, 0.843],
            [0.6000, 0.530, 0.651, 0.835],
            [0.6500, 0.415, 0.562, 0.810],
            [0.7000, 0.335, 0.506, 0.737],
            [0.7500, 0.291, 0.434, 0.816],
            [0.8000, 0.255, 0.337, 0.635],
            [0.8500, 0.188, 0.243, 0.560],
            [0.9000, 0.128, 0.173, 0.518],
            [0.9500, 0.045, 0.055, 0.388],
            [1.0000, 0.045, 0.055, 0.388]
        ];
        return InterpolateStops(stops, t);
    }

    /// <summary>
    /// Applies a named color map to a value within a data range.
    /// </summary>
    /// <param name="value">Raw value to map.</param>
    /// <param name="min">Minimum of the data range.</param>
    /// <param name="max">Maximum of the data range.</param>
    /// <param name="mapName">Color map name.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) ApplyColorMap(double value, double min, double max, string mapName)
    {
        double range = max - min;
        double t = range > 1e-15 ? (value - min) / range : 0.0;
        t = System.Math.Clamp(t, 0.0, 1.0);

        return mapName.ToLowerInvariant() switch
        {
            "viridis" => Viridis(t),
            "plasma" => Plasma(t),
            "inferno" => Inferno(t),
            "magma" => Magma(t),
            "turbo" => Turbo(t),
            "grayscale" => Grayscale(t),
            "jet" => Jet(t),
            "rainbow" => Rainbow(t),
            "coolwarm" => Coolwarm(t),
            "rdbu" => RdBu(t),
            _ => Viridis(t)
        };
    }

    /// <summary>
    /// Returns all available color map names.
    /// </summary>
    /// <returns>Array of color map name strings.</returns>
    public static string[] GetAvailableMaps()
    {
        return ["Viridis", "Plasma", "Inferno", "Magma", "Turbo", "Grayscale", "Jet", "Rainbow", "Coolwarm", "RdBu"];
    }

    private static (double R, double G, double B) InterpolateStops(double[][] stops, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

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

                return (
                    stops[i][1] + (stops[i + 1][1] - stops[i][1]) * localT,
                    stops[i][2] + (stops[i + 1][2] - stops[i][2]) * localT,
                    stops[i][3] + (stops[i + 1][3] - stops[i][3]) * localT
                );
            }
        }

        return (stops[^1][1], stops[^1][2], stops[^1][3]);
    }

    private static (double R, double G, double B) HsvToRgb(double h, double s, double v)
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
