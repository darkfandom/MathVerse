namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Exports visualizations to PNG using raw pixel manipulation with a minimal PNG encoder.
/// </summary>
public sealed class PNGExporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Exports a scene to a PNG byte array.
    /// </summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="width">The output image width in pixels.</param>
    /// <param name="height">The output image height in pixels.</param>
    /// <returns>A byte array containing the PNG image data.</returns>
    public byte[] Export(Scene scene, int width, int height)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        byte[] pixels = RasterizeScene(scene, width, height);
        return EncodePng(pixels, width, height);
    }

    /// <summary>
    /// Exports a scene to a PNG stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="scene">The scene to export.</param>
    /// <param name="width">The output image width in pixels.</param>
    /// <param name="height">The output image height in pixels.</param>
    public void ExportToStream(Stream stream, Scene scene, int width, int height)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        byte[] png = Export(scene, width, height);
        stream.Write(png, 0, png.Length);
    }

    private static byte[] RasterizeScene(Scene scene, int width, int height)
    {
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        ParseColor(scene.BackgroundColor, out byte bgR, out byte bgG, out byte bgB);
        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * stride;
            for (int x = 0; x < width; x++)
            {
                int idx = rowOffset + x * 4;
                pixels[idx] = bgR;
                pixels[idx + 1] = bgG;
                pixels[idx + 2] = bgB;
                pixels[idx + 3] = 255;
            }
        }

        foreach (var elem in scene.Elements)
        {
            switch (elem)
            {
                case CircleElement circle:
                    DrawFilledCircle(pixels, width, height, circle);
                    break;
                case LineElement line:
                    DrawLine(pixels, width, height, line);
                    break;
                case PathElement path:
                    DrawPathStroke(pixels, width, height, path);
                    break;
                case TextElement text:
                    DrawTextPlaceholder(pixels, width, height, text);
                    break;
            }
        }

        return pixels;
    }

    private static void DrawFilledCircle(byte[] pixels, int w, int h, CircleElement c)
    {
        ParseColor(c.FillColor, out byte r, out byte g, out byte b);
        int x0 = (int)(c.CX - c.Radius);
        int x1 = (int)(c.CX + c.Radius);
        int y0 = (int)(c.CY - c.Radius);
        int y1 = (int)(c.CY + c.Radius);
        double radSq = c.Radius * c.Radius;

        for (int y = System.Math.Max(0, y0); y < System.Math.Min(h, y1); y++)
        {
            for (int x = System.Math.Max(0, x0); x < System.Math.Min(w, x1); x++)
            {
                double dx = x - c.CX;
                double dy = y - c.CY;
                if (dx * dx + dy * dy <= radSq)
                {
                    int idx = (y * w + x) * 4;
                    BlendPixel(pixels, idx, r, g, b, c.Opacity);
                }
            }
        }
    }

    private static void DrawLine(byte[] pixels, int w, int h, LineElement line)
    {
        ParseColor(line.StrokeColor, out byte r, out byte g, out byte b);
        int thick = System.Math.Max(1, (int)line.StrokeWidth);

        double dx = line.X2 - line.X1;
        double dy = line.Y2 - line.Y1;
        double len = System.Math.Sqrt(dx * dx + dy * dy);
        if (len < 1e-10) return;

        int steps = (int)System.Math.Ceiling(len);
        for (int i = 0; i <= steps; i++)
        {
            double t = steps > 0 ? (double)i / steps : 0;
            double px = line.X1 + dx * t;
            double py = line.Y1 + dy * t;

            for (int sy = -thick / 2; sy <= thick / 2; sy++)
            {
                for (int sx = -thick / 2; sx <= thick / 2; sx++)
                {
                    int pxI = (int)px + sx;
                    int pyI = (int)py + sy;
                    if (pxI >= 0 && pxI < w && pyI >= 0 && pyI < h)
                    {
                        int idx = (pyI * w + pxI) * 4;
                        BlendPixel(pixels, idx, r, g, b, line.Opacity);
                    }
                }
            }
        }
    }

    private static void DrawPathStroke(byte[] pixels, int w, int h, PathElement path)
    {
        ParseColor(path.StrokeColor, out byte r, out byte g, out byte b);
        int thick = System.Math.Max(1, (int)path.StrokeWidth);
        string d = path.PathData ?? string.Empty;

        double cx = 0, cy = 0;
        int idx = 0;
        while (idx < d.Length)
        {
            while (idx < d.Length && (d[idx] == ' ' || d[idx] == ',')) idx++;
            if (idx >= d.Length) break;

            char cmd = d[idx];
            idx++;

            if (cmd is 'M' or 'm')
            {
                double nx = ParseNum(d, ref idx);
                double ny = ParseNum(d, ref idx);
                cx = cmd == 'm' ? cx + nx : nx;
                cy = cmd == 'm' ? cy + ny : ny;
            }
            else if (cmd is 'L' or 'l')
            {
                double nx = ParseNum(d, ref idx);
                double ny = ParseNum(d, ref idx);
                double tx = cmd == 'l' ? cx + nx : nx;
                double ty = cmd == 'l' ? cy + ny : ny;
                DrawLineOnPixels(pixels, w, h, cx, cy, tx, ty, r, g, b, thick, path.Opacity);
                cx = tx;
                cy = ty;
            }
        }
    }

    private static void DrawTextPlaceholder(byte[] pixels, int w, int h, TextElement text)
    {
        ParseColor(text.FillColor, out byte r, out byte g, out byte b);
        int boxW = System.Math.Max(10, (int)(text.Text.Length * text.FontSize * 0.6));
        int boxH = (int)(text.FontSize * 1.2);

        for (int y = (int)text.Y - boxH; y < (int)text.Y && y >= 0 && y < h; y++)
        {
            for (int x = (int)text.X; x < (int)text.X + boxW && x >= 0 && x < w; x++)
            {
                int idx = (y * w + x) * 4;
                BlendPixel(pixels, idx, r, g, b, text.Opacity);
            }
        }
    }

    private static void DrawLineOnPixels(byte[] pixels, int w, int h, double x1, double y1, double x2, double y2,
        byte r, byte g, byte b, int thick, double opacity)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        double len = System.Math.Sqrt(dx * dx + dy * dy);
        if (len < 1e-10) return;
        int steps = (int)System.Math.Ceiling(len);

        for (int i = 0; i <= steps; i++)
        {
            double t = steps > 0 ? (double)i / steps : 0;
            int px = (int)(x1 + dx * t);
            int py = (int)(y1 + dy * t);
            for (int sy = -thick / 2; sy <= thick / 2; sy++)
            {
                for (int sx = -thick / 2; sx <= thick / 2; sx++)
                {
                    int pxI = px + sx;
                    int pyI = py + sy;
                    if (pxI >= 0 && pxI < w && pyI >= 0 && pyI < h)
                    {
                        int idx = (pyI * w + pxI) * 4;
                        BlendPixel(pixels, idx, r, g, b, opacity);
                    }
                }
            }
        }
    }

    private static void BlendPixel(byte[] pixels, int idx, byte r, byte g, byte b, double opacity)
    {
        byte a = (byte)(opacity * 255);
        if (a == 0) return;
        double factor = a / 255.0;
        pixels[idx] = (byte)(r * factor + pixels[idx] * (1.0 - factor));
        pixels[idx + 1] = (byte)(g * factor + pixels[idx + 1] * (1.0 - factor));
        pixels[idx + 2] = (byte)(b * factor + pixels[idx + 2] * (1.0 - factor));
        pixels[idx + 3] = 255;
    }

    private static byte[] EncodePng(byte[] pixels, int width, int height)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        WriteChunk(bw, "IHDR", CreateIHDR(width, height));
        WriteChunk(bw, "IDAT", CompressIDAT(pixels, width, height));
        WriteChunk(bw, "IEND", Array.Empty<byte>());

        return ms.ToArray();
    }

    private static byte[] CreateIHDR(int width, int height)
    {
        byte[] data = new byte[13];
        data[0] = (byte)(width >> 24);
        data[1] = (byte)(width >> 16);
        data[2] = (byte)(width >> 8);
        data[3] = (byte)width;
        data[4] = (byte)(height >> 24);
        data[5] = (byte)(height >> 16);
        data[6] = (byte)(height >> 8);
        data[7] = (byte)height;
        data[8] = 8; // bit depth
        data[9] = 6; // color type RGBA
        data[10] = 0; // compression
        data[11] = 0; // filter
        data[12] = 0; // interlace
        return data;
    }

    private static byte[] CompressIDAT(byte[] pixels, int width, int height)
    {
        int stride = width * 4;
        var rawData = new byte[height * (1 + stride)];
        for (int y = 0; y < height; y++)
        {
            int rawOffset = y * (1 + stride);
            rawData[rawOffset] = 0; // filter: none
            Array.Copy(pixels, y * stride, rawData, rawOffset + 1, stride);
        }

        using var output = new MemoryStream();
        using (var ds = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            ds.Write(rawData, 0, rawData.Length);
        }
        return output.ToArray();
    }

    private static void WriteChunk(BinaryWriter bw, string type, byte[] data)
    {
        byte[] length = new byte[4];
        length[0] = (byte)(data.Length >> 24);
        length[1] = (byte)(data.Length >> 16);
        length[2] = (byte)(data.Length >> 8);
        length[3] = (byte)data.Length;
        bw.Write(length);

        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        bw.Write(typeBytes);

        if (data.Length > 0)
            bw.Write(data);

        uint crc = Crc32(typeBytes, data);
        byte[] crcBytes = new byte[4];
        crcBytes[0] = (byte)(crc >> 24);
        crcBytes[1] = (byte)(crc >> 16);
        crcBytes[2] = (byte)(crc >> 8);
        crcBytes[3] = (byte)crc;
        bw.Write(crcBytes);
    }

    private static uint Crc32(byte[] type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in type)
            crc = UpdateCrc(crc, b);
        foreach (byte b in data)
            crc = UpdateCrc(crc, b);
        return crc ^ 0xFFFFFFFF;
    }

    private static uint UpdateCrc(uint crc, byte b)
    {
        crc ^= b;
        for (int i = 0; i < 8; i++)
        {
            if ((crc & 1) != 0)
                crc = (crc >> 1) ^ 0xEDB88320;
            else
                crc >>= 1;
        }
        return crc;
    }

    private static void ParseColor(string color, out byte r, out byte g, out byte b)
    {
        r = 0; g = 0; b = 0;
        if (string.IsNullOrEmpty(color)) return;

        string c = color.TrimStart('#');
        if (c.Length == 6)
        {
            byte.TryParse(c.Substring(0, 2), NumberStyles.HexNumber, Inv, out r);
            byte.TryParse(c.Substring(2, 2), NumberStyles.HexNumber, Inv, out g);
            byte.TryParse(c.Substring(4, 2), NumberStyles.HexNumber, Inv, out b);
        }
    }

    private static double ParseNum(string s, ref int idx)
    {
        while (idx < s.Length && (s[idx] == ' ' || s[idx] == ',')) idx++;
        int start = idx;
        while (idx < s.Length && (char.IsDigit(s[idx]) || s[idx] == '.' || s[idx] == '-' || s[idx] == '+'))
            idx++;
        if (idx > start && double.TryParse(s.Substring(start, idx - start), NumberStyles.Float, Inv, out double val))
            return val;
        return 0;
    }
}
