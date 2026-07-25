namespace MathVerse.Math.Visualization.Export;

/// <summary>In-memory pixel buffer for rasterization.</summary>
public sealed class PixelBuffer
{
    private readonly byte[] _data;

    /// <summary>Gets the width of the buffer in pixels.</summary>
    public int Width { get; }

    /// <summary>Gets the height of the buffer in pixels.</summary>
    public int Height { get; }

    /// <summary>Gets the stride (bytes per row) of the buffer.</summary>
    public int Stride => Width * 4;

    /// <summary>Gets the raw pixel data in RGBA format.</summary>
    public byte[] Data => _data;

    /// <summary>Initializes a new pixel buffer with the specified dimensions.</summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    public PixelBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        _data = new byte[width * height * 4];
    }

    /// <summary>Sets the color of a pixel at the specified coordinates.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        int index = (y * Width + x) * 4;
        _data[index] = r;
        _data[index + 1] = g;
        _data[index + 2] = b;
        _data[index + 3] = a;
    }

    /// <summary>Gets the color of a pixel at the specified coordinates.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The RGBA color values.</returns>
    public (byte r, byte g, byte b, byte a) GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return (0, 0, 0, 0);

        int index = (y * Width + x) * 4;
        return (_data[index], _data[index + 1], _data[index + 2], _data[index + 3]);
    }

    /// <summary>Fills the entire buffer with the specified color.</summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void Clear(byte r = 255, byte g = 255, byte b = 255, byte a = 255)
    {
        int pixelCount = Width * Height;
        for (int i = 0; i < pixelCount; i++)
        {
            int index = i * 4;
            _data[index] = r;
            _data[index + 1] = g;
            _data[index + 2] = b;
            _data[index + 3] = a;
        }
    }

    /// <summary>Draws a line using Bresenham's algorithm.</summary>
    /// <param name="x0">Start x coordinate.</param>
    /// <param name="y0">Start y coordinate.</param>
    /// <param name="x1">End x coordinate.</param>
    /// <param name="y1">End y coordinate.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void DrawLine(int x0, int y0, int x1, int y1, byte r, byte g, byte b, byte a = 255)
    {
        int dx = System.Math.Abs(x1 - x0);
        int dy = -System.Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            SetPixel(x, y, r, g, b, a);

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;

            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    /// <summary>Draws a circle using the midpoint circle algorithm.</summary>
    /// <param name="cx">Center x coordinate.</param>
    /// <param name="cy">Center y coordinate.</param>
    /// <param name="radius">Circle radius.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void DrawCircle(int cx, int cy, int radius, byte r, byte g, byte b, byte a = 255)
    {
        int x = radius;
        int y = 0;
        int err = 1 - radius;

        while (x >= y)
        {
            SetPixel(cx + x, cy + y, r, g, b, a);
            SetPixel(cx + y, cy + x, r, g, b, a);
            SetPixel(cx - y, cy + x, r, g, b, a);
            SetPixel(cx - x, cy + y, r, g, b, a);
            SetPixel(cx - x, cy - y, r, g, b, a);
            SetPixel(cx - y, cy - x, r, g, b, a);
            SetPixel(cx + y, cy - x, r, g, b, a);
            SetPixel(cx + x, cy - y, r, g, b, a);

            y++;

            if (err < 0)
            {
                err += 2 * y + 1;
            }
            else
            {
                x--;
                err += 2 * (y - x) + 1;
            }
        }
    }

    /// <summary>Fills a rectangle with the specified color.</summary>
    /// <param name="x">The x coordinate of the top-left corner.</param>
    /// <param name="y">The y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void FillRect(int x, int y, int width, int height, byte r, byte g, byte b, byte a = 255)
    {
        int startX = System.Math.Max(0, x);
        int startY = System.Math.Max(0, y);
        int endX = System.Math.Min(Width, x + width);
        int endY = System.Math.Min(Height, y + height);

        for (int py = startY; py < endY; py++)
        {
            int rowStart = (py * Width + startX) * 4;
            int rowEnd = (py * Width + endX) * 4;

            for (int idx = rowStart; idx < rowEnd; idx += 4)
            {
                _data[idx] = r;
                _data[idx + 1] = g;
                _data[idx + 2] = b;
                _data[idx + 3] = a;
            }
        }
    }

    /// <summary>Draws a filled circle using scanline fill.</summary>
    /// <param name="cx">Center x coordinate.</param>
    /// <param name="cy">Center y coordinate.</param>
    /// <param name="radius">Circle radius.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void FillCircle(int cx, int cy, int radius, byte r, byte g, byte b, byte a = 255)
    {
        int rSquared = radius * radius;

        for (int y = -radius; y <= radius; y++)
        {
            int xExtent = (int)System.Math.Sqrt(rSquared - y * y);

            for (int x = -xExtent; x <= xExtent; x++)
            {
                SetPixel(cx + x, cy + y, r, g, b, a);
            }
        }
    }

    /// <summary>Draws a triangle outline.</summary>
    /// <param name="x0">First vertex x.</param>
    /// <param name="y0">First vertex y.</param>
    /// <param name="x1">Second vertex x.</param>
    /// <param name="y1">Second vertex y.</param>
    /// <param name="x2">Third vertex x.</param>
    /// <param name="y2">Third vertex y.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2,
        byte r, byte g, byte b, byte a = 255)
    {
        DrawLine(x0, y0, x1, y1, r, g, b, a);
        DrawLine(x1, y1, x2, y2, r, g, b, a);
        DrawLine(x2, y2, x0, y0, r, g, b, a);
    }

    /// <summary>Blends a pixel with alpha compositing.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha component (0-255).</param>
    public void BlendPixel(int x, int y, byte r, byte g, byte b, byte a)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        if (a == 0)
            return;

        if (a == 255)
        {
            SetPixel(x, y, r, g, b, 255);
            return;
        }

        var (srcR, srcG, srcB, srcA) = GetPixel(x, y);
        double alpha = a / 255.0;
        double invAlpha = 1.0 - alpha;

        byte finalR = (byte)(r * alpha + srcR * invAlpha);
        byte finalG = (byte)(g * alpha + srcG * invAlpha);
        byte finalB = (byte)(b * alpha + srcB * invAlpha);
        byte finalA = (byte)System.Math.Min(255, srcA + a);

        SetPixel(x, y, finalR, finalG, finalB, finalA);
    }
}
