using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.Core;
using MathVerse.Math.Expressions;
using MathVerse.Math.Parsing;
using MathVerse.Math.Visualization.Export;

namespace MathVerse.Desktop.ViewModels;

public sealed class GraphRenderer
{
    private readonly PixelBuffer _buffer;
    private readonly int _width;
    private readonly int _height;

    private double _viewCenterX;
    private double _viewCenterY;
    private double _viewScale;

    private const byte BgR = 11, BgG = 11, BgB = 18;
    private const byte GridR = 30, GridG = 30, GridB = 50;
    private const byte AxisR = 90, AxisG = 90, AxisB = 120;
    private const byte LabelR = 140, LabelG = 140, LabelB = 160;

    public int Width => _width;
    public int Height => _height;
    public PixelBuffer Buffer => _buffer;

    public GraphRenderer(int width, int height)
    {
        _width = width;
        _height = height;
        _buffer = new PixelBuffer(width, height);
        _viewScale = System.Math.Min(width, height) / 20.0;
    }

    public void SetViewport(double cx, double cy, double scale)
    {
        _viewCenterX = cx; _viewCenterY = cy; _viewScale = scale;
    }

    public void Clear() => _buffer.Clear(BgR, BgG, BgB, 255);

    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height)
            _buffer.SetPixel(x, y, r, g, b, a);
    }

    public void DrawGrid(bool showLabels = true)
    {
        if (_viewScale <= 0) return;
        double viewWidth = _width / _viewScale;
        double viewHeight = _height / _viewScale;
        double step = NiceStep(viewWidth / 10.0);
        if (step <= 0) return;

        double xStart = System.Math.Floor((_viewCenterX - viewWidth / 2) / step) * step;
        double xEnd = System.Math.Ceiling((_viewCenterX + viewWidth / 2) / step) * step;
        double yStart = System.Math.Floor((_viewCenterY - viewHeight / 2) / step) * step;
        double yEnd = System.Math.Ceiling((_viewCenterY + viewHeight / 2) / step) * step;

        for (double x = xStart; x <= xEnd; x += step)
        {
            int sx = M2SX(x);
            if (sx < 0 || sx >= _width) continue;
            bool isO = System.Math.Abs(x) < step * 0.01;
            DrawVLine(sx, 0, _height - 1, isO ? AxisR : GridR, isO ? AxisG : GridG, isO ? AxisB : GridB, 255);
            if (showLabels && !isO)
                DrawTickLabel(sx, _height - 16, FormatTick(x, step));
        }
        for (double y = yStart; y <= yEnd; y += step)
        {
            int sy = M2SY(y);
            if (sy < 0 || sy >= _height) continue;
            bool isO = System.Math.Abs(y) < step * 0.01;
            DrawHLine(0, _width - 1, sy, isO ? AxisR : GridR, isO ? AxisG : GridG, isO ? AxisB : GridB, 255);
            if (showLabels && !isO)
                DrawTickLabel(4, sy - 4, FormatTick(y, step));
        }
    }

    public void DrawCurve(Func<double, double> func, string hexColor, double lineWidth, bool fill = false)
    {
        var (r, g, b) = ParseHex(hexColor);
        double xStart = _viewCenterX - _width / _viewScale / 2;
        double xEnd = _viewCenterX + _width / _viewScale / 2;
        int steps = System.Math.Max(_width * 2, 1000);
        double dx = (xEnd - xStart) / steps;
        int? px = null, py = null;
        int baseY = M2SY(0);

        for (int i = 0; i <= steps; i++)
        {
            double x = xStart + i * dx;
            try
            {
                double y = func(x);
                if (double.IsNaN(y) || double.IsInfinity(y)) { px = null; continue; }
                int sx = M2SX(x), sy = M2SY(y);
                if (px.HasValue)
                {
                    _buffer.DrawLine(px.Value, py!.Value, sx, sy, r, g, b, 255);
                    if (fill && System.Math.Abs(sy - baseY) < _height)
                    {
                        int yMin = System.Math.Min(sy, baseY);
                        int yMax = System.Math.Max(sy, baseY);
                        for (int yy = yMin; yy <= yMax; yy++)
                            _buffer.BlendPixel(sx, yy, r, g, b, 40);
                    }
                }
                px = sx; py = sy;
            }
            catch { px = null; }
        }
    }

    public void DrawPolarCurve(Func<double, double> rFunc, string hexColor, double lineWidth,
        double thetaMin, double thetaMax)
    {
        var (r, g, b) = ParseHex(hexColor);
        int steps = 1000;
        double dTheta = (thetaMax - thetaMin) / steps;
        int? prevSx = null, prevSy = null;
        for (int i = 0; i <= steps; i++)
        {
            double theta = thetaMin + i * dTheta;
            try
            {
                double radius = rFunc(theta);
                if (double.IsNaN(radius) || double.IsInfinity(radius)) { prevSx = null; continue; }
                int sx = M2SX(radius * System.Math.Cos(theta));
                int sy = M2SY(radius * System.Math.Sin(theta));
                if (prevSx.HasValue)
                    _buffer.DrawLine(prevSx.Value, prevSy!.Value, sx, sy, r, g, b, 255);
                prevSx = sx; prevSy = sy;
            }
            catch { prevSx = null; }
        }
    }

    public void DrawParametric(Func<double, (double x, double y)> func, string hexColor,
        double lineWidth, double tMin, double tMax)
    {
        var (r, g, b) = ParseHex(hexColor);
        int steps = 1000;
        double dt = (tMax - tMin) / steps;
        int? prevSx = null, prevSy = null;
        for (int i = 0; i <= steps; i++)
        {
            double t = tMin + i * dt;
            try
            {
                var (x, y) = func(t);
                if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                { prevSx = null; continue; }
                int sx = M2SX(x), sy = M2SY(y);
                if (prevSx.HasValue)
                    _buffer.DrawLine(prevSx.Value, prevSy!.Value, sx, sy, r, g, b, 255);
                prevSx = sx; prevSy = sy;
            }
            catch { prevSx = null; }
        }
    }

    public void DrawScatterPlot(double[] xData, double[] yData, string hexColor, double markerSize)
    {
        var (r, g, b) = ParseHex(hexColor);
        int radius = System.Math.Max(2, (int)(markerSize * _viewScale / 5.0));
        for (int i = 0; i < xData.Length && i < yData.Length; i++)
        {
            if (double.IsNaN(xData[i]) || double.IsNaN(yData[i])) continue;
            _buffer.FillCircle(M2SX(xData[i]), M2SY(yData[i]), radius, r, g, b, 255);
        }
    }

    public void DrawVectorField(Func<double, double, (double vx, double vy)> field,
        string hexColor, int resolution, bool normalize)
    {
        var (r, g, b) = ParseHex(hexColor);
        double vw = _width / _viewScale, vh = _height / _viewScale;
        double stepX = vw / resolution, stepY = vh / resolution;
        double xS = _viewCenterX - vw / 2, yS = _viewCenterY - vh / 2;
        double arrowLen = stepX * 0.4;

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                double x = xS + (i + 0.5) * stepX, y = yS + (j + 0.5) * stepY;
                try
                {
                    var (vx, vy) = field(x, y);
                    if (double.IsNaN(vx) || double.IsInfinity(vx) || double.IsNaN(vy) || double.IsInfinity(vy)) continue;
                    double mag = System.Math.Sqrt(vx * vx + vy * vy);
                    if (mag < 1e-10) continue;
                    if (normalize) { vx /= mag; vy /= mag; }
                    double ex = x + vx / mag * arrowLen, ey = y + vy / mag * arrowLen;
                    _buffer.DrawLine(M2SX(x), M2SY(y), M2SX(ex), M2SY(ey), r, g, b, 255);
                }
                catch { }
            }
        }
    }

    public void DrawHistogram(double[] values, int bins, string hexColor, double barOpacity = 0.7)
    {
        if (values.Length == 0 || bins <= 0) return;
        var (r, g, b) = ParseHex(hexColor);
        double min = values.Min(), max = values.Max();
        double binWidth = (max - min) / bins;
        if (binWidth <= 0) return;
        var counts = new int[bins];
        foreach (double v in values)
        {
            int idx = System.Math.Clamp((int)((v - min) / binWidth), 0, bins - 1);
            counts[idx]++;
        }
        int maxCount = counts.Max();
        if (maxCount == 0) return;
        double vw = _width / _viewScale, vh = _height / _viewScale;
        double barW = vw / bins;
        byte alpha = (byte)(barOpacity * 255);

        for (int i = 0; i < bins; i++)
        {
            double barH = (double)counts[i] / maxCount * vh * 0.8;
            double x0 = _viewCenterX - vw / 2 + i * barW;
            int sx = M2SX(x0);
            int sw = System.Math.Max(1, (int)(barW * _viewScale));
            int sh = System.Math.Max(1, (int)(barH * _viewScale));
            int sy = _height - (int)(((_viewCenterY - vh / 2 + vh) - _viewCenterY + vh / 2) * _viewScale) - sh;
            _buffer.FillRect(sx, _height - sh, sw, sh, r, g, b, alpha);
        }
    }

    public void DrawHeatmap(Func<double, double, double> func, int resolution, double minVal, double maxVal)
    {
        double vw = _width / _viewScale, vh = _height / _viewScale;
        double xS = _viewCenterX - vw / 2, yS = _viewCenterY - vh / 2;
        double stepX = vw / resolution, stepY = vh / resolution;
        double range = maxVal - minVal;
        if (range <= 0) range = 1;

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                double x = xS + (i + 0.5) * stepX, y = yS + (j + 0.5) * stepY;
                try
                {
                    double val = func(x, y);
                    if (double.IsNaN(val)) continue;
                    double t = System.Math.Clamp((val - minVal) / range, 0, 1);
                    var (cr, cg, cb) = HeatmapColor(t);
                    int sx = M2SX(x - stepX / 2);
                    int sy = M2SY(y + stepY / 2);
                    int sw = System.Math.Max(1, (int)(stepX * _viewScale) + 1);
                    int sh = System.Math.Max(1, (int)(stepY * _viewScale) + 1);
                    _buffer.FillRect(sx, sy - sh, sw, sh, cr, cg, cb, 255);
                }
                catch { }
            }
        }
    }

    public void DrawContour(Func<double, double, double> func, int levels, string hexColor, double lineWidth)
    {
        var (r, g, b) = ParseHex(hexColor);
        double vw = _width / _viewScale, vh = _height / _viewScale;
        double xS = _viewCenterX - vw / 2, yS = _viewCenterY - vh / 2;
        int res = System.Math.Max(_width / 2, 200);
        double dx = vw / res, dy = vh / res;

        double fMin = double.MaxValue, fMax = double.MinValue;
        var grid = new double[res + 1, res + 1];
        for (int i = 0; i <= res; i++)
            for (int j = 0; j <= res; j++)
            {
                try
                {
                    grid[i, j] = func(xS + i * dx, yS + j * dy);
                    if (!double.IsNaN(grid[i, j]))
                    { fMin = System.Math.Min(fMin, grid[i, j]); fMax = System.Math.Max(fMax, grid[i, j]); }
                }
                catch { grid[i, j] = double.NaN; }
            }

        if (fMin >= fMax) return;
        for (int l = 0; l < levels; l++)
        {
            double level = fMin + (fMax - fMin) * (l + 0.5) / levels;
            for (int i = 0; i < res; i++)
                for (int j = 0; j < res; j++)
                    MarchSquare(i, j, dx, dy, xS, yS, level,
                        grid[i, j], grid[i + 1, j], grid[i, j + 1], grid[i + 1, j + 1], r, g, b);
        }
    }

    private void MarchSquare(int i, int j, double dx, double dy, double xOff, double yOff,
        double level, double v00, double v10, double v01, double v11, byte r, byte g, byte b)
    {
        int cfg = 0;
        if (!double.IsNaN(v00) && v00 >= level) cfg |= 1;
        if (!double.IsNaN(v10) && v10 >= level) cfg |= 2;
        if (!double.IsNaN(v11) && v11 >= level) cfg |= 4;
        if (!double.IsNaN(v01) && v01 >= level) cfg |= 8;
        if (cfg == 0 || cfg == 15) return;

        double x0 = xOff + i * dx, y0 = yOff + j * dy;
        double x1 = x0 + dx, y1 = y0 + dy;
        double bottom = Lerp(x0, x1, v00, v10, level);
        double right = Lerp(y0, y1, v10, v11, level);
        double top = Lerp(x0, x1, v01, v11, level);
        double left = Lerp(y0, y1, v00, v01, level);

        (int sx, int sy) T(double x, double y) => (M2SX(x), M2SY(y));

        switch (cfg)
        {
            case 1: case 14: DrawSeg(T(x0, left), T(bottom, y0), r, g, b); break;
            case 2: case 13: DrawSeg(T(bottom, y0), T(x1, right), r, g, b); break;
            case 3: case 12: DrawSeg(T(x0, left), T(x1, right), r, g, b); break;
            case 4: case 11: DrawSeg(T(x1, right), T(top, y1), r, g, b); break;
            case 5: DrawSeg(T(x0, left), T(top, y1), r, g, b); DrawSeg(T(bottom, y0), T(x1, right), r, g, b); break;
            case 6: case 9: DrawSeg(T(bottom, y0), T(top, y1), r, g, b); break;
            case 7: case 8: DrawSeg(T(x0, left), T(top, y1), r, g, b); break;
            case 10: DrawSeg(T(x0, left), T(bottom, y0), r, g, b); DrawSeg(T(top, y1), T(x1, right), r, g, b); break;
        }
    }

    private static double Lerp(double a, double b, double va, double vb, double level)
    {
        double d = vb - va;
        return System.Math.Abs(d) < 1e-15 ? (a + b) / 2 : a + (level - va) / d * (b - a);
    }

    private void DrawSeg((int x, int y) a, (int x, int y) b, byte r, byte g, byte b2)
        => _buffer.DrawLine(a.x, a.y, b.x, b.y, r, g, b2, 255);

    private void DrawVLine(int x, int y0, int y1, byte r, byte g, byte b2, byte alpha)
    {
        for (int y = System.Math.Min(y0, y1); y <= System.Math.Max(y0, y1); y++)
            _buffer.SetPixel(x, y, r, g, b2, alpha);
    }

    private void DrawHLine(int x0, int x1, int y, byte r, byte g, byte b2, byte alpha)
    {
        for (int x = System.Math.Min(x0, x1); x <= System.Math.Max(x0, x1); x++)
            _buffer.SetPixel(x, y, r, g, b2, alpha);
    }

    private void DrawTickLabel(int sx, int sy, string text)
    {
        int x = sx + 3;
        foreach (char c in text)
        {
            if (x >= 0 && x + 5 < _width && sy >= 0 && sy + 7 < _height)
                DrawChar5x7(x, sy, c, LabelR, LabelG, LabelB, 200);
            x += 6;
        }
    }

    private void DrawChar5x7(int ox, int oy, char ch, byte r, byte g, byte b2, byte alpha)
    {
        ReadOnlySpan<byte> glyph = ch switch
        {
            '-' => [0x00, 0x00, 0x1F, 0x00, 0x00],
            '.' => [0x00, 0x00, 0x00, 0x00, 0x04],
            '0' => [0x0E, 0x11, 0x13, 0x15, 0x0E], '1' => [0x04, 0x0C, 0x04, 0x04, 0x0E],
            '2' => [0x0E, 0x11, 0x06, 0x08, 0x1F], '3' => [0x0E, 0x11, 0x06, 0x11, 0x0E],
            '4' => [0x06, 0x0A, 0x12, 0x1F, 0x02], '5' => [0x1F, 0x10, 0x1E, 0x11, 0x0E],
            '6' => [0x06, 0x08, 0x0E, 0x11, 0x0E], '7' => [0x1F, 0x01, 0x02, 0x04, 0x08],
            '8' => [0x0E, 0x11, 0x0E, 0x11, 0x0E], '9' => [0x0E, 0x11, 0x0F, 0x01, 0x0E],
            _ => [0x00, 0x00, 0x00, 0x00, 0x00]
        };
        for (int row = 0; row < 5; row++)
        {
            byte bits = row < glyph.Length ? glyph[row] : (byte)0;
            for (int col = 0; col < 5; col++)
            {
                if ((bits & (1 << col)) != 0)
                {
                    int px = ox + col, py = oy + row;
                    if (px >= 0 && px < _width && py >= 0 && py < _height)
                        _buffer.SetPixel(px, py, r, g, b2, alpha);
                }
            }
        }
    }

    private int M2SX(double x) => (int)((x - _viewCenterX) * _viewScale + _width / 2.0);
    private int M2SY(double y) => (int)(_height / 2.0 - (y - _viewCenterY) * _viewScale);

    private static double NiceStep(double raw)
    {
        double exp = System.Math.Floor(System.Math.Log10(raw));
        double frac = raw / System.Math.Pow(10, exp);
        double nice = frac switch { < 1.5 => 1, < 3.5 => 2, < 7.5 => 5, _ => 10 };
        return nice * System.Math.Pow(10, exp);
    }

    private static string FormatTick(double val, double step)
    {
        if (System.Math.Abs(val) < step * 0.01) return "0";
        int decimals = System.Math.Max(0, -((int)System.Math.Floor(System.Math.Log10(step * 0.1))));
        return val.ToString($"F{decimals}", CultureInfo.InvariantCulture);
    }

    public static (byte r, byte g, byte b) ParseHex(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return (74, 158, 255);
        hex = hex.TrimStart('#');
        if (hex.Length >= 6)
            return (Convert.ToByte(hex[..2], 16), Convert.ToByte(hex[2..4], 16), Convert.ToByte(hex[4..6], 16));
        return (74, 158, 255);
    }

    private static (byte r, byte g, byte b) HeatmapColor(double t)
    {
        t = System.Math.Clamp(t, 0, 1);
        if (t < 0.25) { double s = t / 0.25; return (0, (byte)(s * 255), 255); }
        if (t < 0.5) { double s = (t - 0.25) / 0.25; return (0, 255, (byte)((1 - s) * 255)); }
        if (t < 0.75) { double s = (t - 0.5) / 0.25; return ((byte)(s * 255), 255, 0); }
        { double s = (t - 0.75) / 0.25; return (255, (byte)((1 - s) * 255), 0); }
    }

    public static Func<double, double> BuildFunctionFromExpression(
        string expression, string variable, IDictionary<string, double> parameters)
    {
        Expression? parsed = null;
        try { parsed = ParsingFacade.ParseExpression(expression); }
        catch { return _ => double.NaN; }
        return x =>
        {
            try
            {
                var vars = ImmutableDictionary<string, double>.Empty.Add(variable, x);
                foreach (var kv in parameters)
                    if (!vars.ContainsKey(kv.Key)) vars = vars.Add(kv.Key, kv.Value);
                var result = Evaluator.Instance.Evaluate(parsed, vars);
                if (result.Result is LiteralExpression lit) return lit.Value;
                return Evaluator.Instance.EvaluateToDouble(parsed, vars);
            }
            catch { return double.NaN; }
        };
    }

    public static Func<double, double, double> Build2DFunctionFromExpression(
        string expression, string varX, string varY, IDictionary<string, double> parameters)
    {
        Expression? parsed = null;
        try { parsed = ParsingFacade.ParseExpression(expression); }
        catch { return (_, _) => double.NaN; }
        return (x, y) =>
        {
            try
            {
                var vars = ImmutableDictionary<string, double>.Empty.Add(varX, x).Add(varY, y);
                foreach (var kv in parameters)
                    if (!vars.ContainsKey(kv.Key)) vars = vars.Add(kv.Key, kv.Value);
                var result = Evaluator.Instance.Evaluate(parsed, vars);
                if (result.Result is LiteralExpression lit) return lit.Value;
                return Evaluator.Instance.EvaluateToDouble(parsed, vars);
            }
            catch { return double.NaN; }
        };
    }

    public static List<string> DetectParameters(string expression)
    {
        try
        {
            var parsed = ParsingFacade.ParseExpression(expression);
            var vars = parsed.Variables();
            return vars.Where(v => v != "x" && v != "y" && v != "t" && v != "theta"
                                   && v != "r" && v != "z" && v != "i" && v != "j").ToList();
        }
        catch { return new(); }
    }
}
