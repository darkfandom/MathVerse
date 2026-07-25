namespace MathVerse.Math.Visualization.Export;
using System.Collections.Generic;

/// <summary>Represents a render command for rasterization.</summary>
public sealed class RenderCommand
{
    /// <summary>Gets the command type.</summary>
    public RenderCommandType CommandType { get; init; }

    /// <summary>Gets the command parameters.</summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}

/// <summary>Defines types of render commands.</summary>
public enum RenderCommandType
{
    /// <summary>Clear the buffer.</summary>
    Clear,

    /// <summary>Draw a line.</summary>
    DrawLine,

    /// <summary>Draw a circle outline.</summary>
    DrawCircle,

    /// <summary>Fill a circle.</summary>
    FillCircle,

    /// <summary>Fill a rectangle.</summary>
    FillRect,

    /// <summary>Draw a triangle.</summary>
    DrawTriangle,

    /// <summary>Draw text.</summary>
    DrawText,

    /// <summary>Apply a transform.</summary>
    Transform
}

/// <summary>Creates pixel buffers and render command lists for PNG export.</summary>
public sealed class PNGExporter
{
    private const int DefaultWidth = 1920;
    private const int DefaultHeight = 1080;

    /// <summary>Creates a render command list for rasterizing a scene.</summary>
    /// <param name="scene">The scene to render.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <returns>A list of render commands.</returns>
    public static List<RenderCommand> CreateRenderCommands(Core.VisualizationScene scene, int width, int height)
    {
        var commands = new List<RenderCommand>();

        commands.Add(new RenderCommand
        {
            CommandType = RenderCommandType.Clear,
            Parameters = new Dictionary<string, object>
            {
                ["r"] = (byte)255,
                ["g"] = (byte)255,
                ["b"] = (byte)255,
                ["a"] = (byte)255
            }
        });

        if (scene?.Objects == null)
            return commands;

        foreach (var obj in scene.Objects)
        {
            AppendRenderCommands(commands, obj, width, height);
        }

        return commands;
    }

    /// <summary>Creates a pixel buffer from a scene.</summary>
    /// <param name="scene">The scene to rasterize.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <returns>A pixel buffer containing the rendered scene.</returns>
    public static PixelBuffer CreatePixelBuffer(Core.VisualizationScene scene, int width, int height)
    {
        var buffer = new PixelBuffer(width, height);
        buffer.Clear(255, 255, 255, 255);

        if (scene?.Objects == null)
            return buffer;

        foreach (var obj in scene.Objects)
        {
            RasterizeObject(buffer, obj);
        }

        return buffer;
    }

    /// <summary>Executes render commands on a pixel buffer.</summary>
    /// <param name="buffer">The target pixel buffer.</param>
    /// <param name="commands">The render commands to execute.</param>
    public static void ExecuteCommands(PixelBuffer buffer, List<RenderCommand> commands)
    {
        foreach (var cmd in commands)
        {
            switch (cmd.CommandType)
            {
                case RenderCommandType.Clear:
                    ExecuteClear(buffer, cmd);
                    break;
                case RenderCommandType.DrawLine:
                    ExecuteDrawLine(buffer, cmd);
                    break;
                case RenderCommandType.DrawCircle:
                    ExecuteDrawCircle(buffer, cmd);
                    break;
                case RenderCommandType.FillCircle:
                    ExecuteFillCircle(buffer, cmd);
                    break;
                case RenderCommandType.FillRect:
                    ExecuteFillRect(buffer, cmd);
                    break;
                case RenderCommandType.DrawTriangle:
                    ExecuteDrawTriangle(buffer, cmd);
                    break;
            }
        }
    }

    /// <summary>Converts a pixel buffer to PNG byte array.</summary>
    /// <param name="buffer">The pixel buffer to convert.</param>
    /// <returns>PNG file bytes.</returns>
    public static byte[] ToPNG(PixelBuffer buffer)
    {
        var png = new List<byte>();

        png.AddRange(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        png.AddRange(WriteChunk("IHDR", CreateIHDR(buffer.Width, buffer.Height)));
        png.AddRange(WriteChunk("IDAT", CreateIDAT(buffer)));
        png.AddRange(WriteChunk("IEND", new byte[0]));

        return png.ToArray();
    }

    private static byte[] CreateIHDR(int width, int height)
    {
        var data = new byte[13];
        WriteInt32BigEndian(data, 0, width);
        WriteInt32BigEndian(data, 4, height);
        data[8] = 8;  // bit depth
        data[9] = 6;  // color type RGBA
        data[10] = 0; // compression
        data[11] = 0; // filter
        data[12] = 0; // interlace
        return data;
    }

    private static byte[] CreateIDAT(PixelBuffer buffer)
    {
        int rawSize = buffer.Height * (1 + buffer.Stride);
        var rawData = new byte[rawSize];

        int offset = 0;
        for (int y = 0; y < buffer.Height; y++)
        {
            rawData[offset] = 0; // filter none
            offset++;

            System.Buffer.BlockCopy(buffer.Data, y * buffer.Stride, rawData, offset, buffer.Stride);
            offset += buffer.Stride;
        }

        return CompressDeflate(rawData);
    }

    private static byte[] CompressDeflate(byte[] data)
    {
        var compressed = new List<byte>();

        compressed.Add(0x78);
        compressed.Add(0x01);

        int i = 0;
        while (i < data.Length)
        {
            int blockLength = System.Math.Min(65535, data.Length - i);

            compressed.Add((byte)(i + blockLength == data.Length ? 0x01 : 0x00));
            compressed.Add((byte)(blockLength & 0xFF));
            compressed.Add((byte)((blockLength >> 8) & 0xFF));
            compressed.Add((byte)(~blockLength & 0xFF));
            compressed.Add((byte)((~blockLength >> 8) & 0xFF));

            for (int j = 0; j < blockLength; j++)
            {
                compressed.Add(data[i + j]);
            }

            i += blockLength;
        }

        uint crc = 0xFFFFFFFF;
        foreach (byte b in compressed)
        {
            crc = Crc32Update(crc, b);
        }
        crc ^= 0xFFFFFFFF;

        compressed.AddRange(System.BitConverter.GetBytes(crc));

        return compressed.ToArray();
    }

    private static List<byte> WriteChunk(string type, byte[] data)
    {
        var chunk = new List<byte>();

        chunk.AddRange(System.BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data.Length)));

        foreach (char c in type)
            chunk.Add((byte)c);

        chunk.AddRange(data);

        uint crc = Crc32Calculate(type, data);
        chunk.AddRange(System.BitConverter.GetBytes(crc));

        return chunk;
    }

    private static uint Crc32Calculate(string type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;

        foreach (char c in type)
            crc = Crc32Update(crc, (byte)c);

        foreach (byte b in data)
            crc = Crc32Update(crc, b);

        return crc ^ 0xFFFFFFFF;
    }

    private static uint Crc32Update(uint crc, byte b)
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

    private static void WriteInt32BigEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }

    private static void AppendRenderCommands(List<RenderCommand> commands, Core.VisualizationObject obj, int width, int height)
    {
        if (obj == null)
            return;

        switch (obj)
        {
            case Core.LinePlot linePlot:
                AppendLinePlotCommands(commands, linePlot);
                break;
            case Core.PointCloud pointCloud:
                AppendPointCloudCommands(commands, pointCloud);
                break;
            default:
                break;
        }
    }

    private static void AppendLinePlotCommands(List<RenderCommand> commands, Core.LinePlot linePlot)
    {
        if (linePlot.Points == null || linePlot.Points.Count < 2)
            return;

        ParseHexColor(linePlot.Color ?? "#0000FF", out byte r, out byte g, out byte b);

        for (int i = 0; i < linePlot.Points.Count - 1; i++)
        {
            var p0 = linePlot.Points[i];
            var p1 = linePlot.Points[i + 1];

            commands.Add(new RenderCommand
            {
                CommandType = RenderCommandType.DrawLine,
                Parameters = new Dictionary<string, object>
                {
                    ["x0"] = (int)p0.X,
                    ["y0"] = (int)p0.Y,
                    ["x1"] = (int)p1.X,
                    ["y1"] = (int)p1.Y,
                    ["r"] = r,
                    ["g"] = g,
                    ["b"] = b,
                    ["a"] = (byte)255
                }
            });
        }
    }

    private static void AppendPointCloudCommands(List<RenderCommand> commands, Core.PointCloud pointCloud)
    {
        if (pointCloud.Points == null)
            return;

        ParseHexColor(pointCloud.Color ?? "#FF0000", out byte r, out byte g, out byte b);
        int radius = (int)(pointCloud.PointSize > 0 ? pointCloud.PointSize / 2.0 : 2);

        foreach (var pt in pointCloud.Points)
        {
            commands.Add(new RenderCommand
            {
                CommandType = RenderCommandType.FillCircle,
                Parameters = new Dictionary<string, object>
                {
                    ["cx"] = (int)pt.X,
                    ["cy"] = (int)pt.Y,
                    ["radius"] = radius,
                    ["r"] = r,
                    ["g"] = g,
                    ["b"] = b,
                    ["a"] = (byte)255
                }
            });
        }
    }

    private static void RasterizeObject(PixelBuffer buffer, Core.VisualizationObject obj)
    {
        if (obj is Core.LinePlot linePlot && linePlot.Points != null)
        {
            ParseHexColor(linePlot.Color ?? "#0000FF", out byte r, out byte g, out byte b);

            for (int i = 0; i < linePlot.Points.Count - 1; i++)
            {
                var p0 = linePlot.Points[i];
                var p1 = linePlot.Points[i + 1];
                buffer.DrawLine((int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, r, g, b);
            }
        }
        else if (obj is Core.PointCloud pointCloud && pointCloud.Points != null)
        {
            ParseHexColor(pointCloud.Color ?? "#FF0000", out byte r, out byte g, out byte b);
            int radius = (int)(pointCloud.PointSize > 0 ? pointCloud.PointSize / 2.0 : 2);

            foreach (var pt in pointCloud.Points)
            {
                buffer.FillCircle((int)pt.X, (int)pt.Y, radius, r, g, b);
            }
        }
    }

    private static void ExecuteClear(PixelBuffer buffer, RenderCommand cmd)
    {
        byte r = GetParameter<byte>(cmd, "r", 255);
        byte g = GetParameter<byte>(cmd, "g", 255);
        byte b = GetParameter<byte>(cmd, "b", 255);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.Clear(r, g, b, a);
    }

    private static void ExecuteDrawLine(PixelBuffer buffer, RenderCommand cmd)
    {
        int x0 = GetParameter<int>(cmd, "x0", 0);
        int y0 = GetParameter<int>(cmd, "y0", 0);
        int x1 = GetParameter<int>(cmd, "x1", 0);
        int y1 = GetParameter<int>(cmd, "y1", 0);
        byte r = GetParameter<byte>(cmd, "r", 0);
        byte g = GetParameter<byte>(cmd, "g", 0);
        byte b = GetParameter<byte>(cmd, "b", 0);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.DrawLine(x0, y0, x1, y1, r, g, b, a);
    }

    private static void ExecuteDrawCircle(PixelBuffer buffer, RenderCommand cmd)
    {
        int cx = GetParameter<int>(cmd, "cx", 0);
        int cy = GetParameter<int>(cmd, "cy", 0);
        int radius = GetParameter<int>(cmd, "radius", 0);
        byte r = GetParameter<byte>(cmd, "r", 0);
        byte g = GetParameter<byte>(cmd, "g", 0);
        byte b = GetParameter<byte>(cmd, "b", 0);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.DrawCircle(cx, cy, radius, r, g, b, a);
    }

    private static void ExecuteFillCircle(PixelBuffer buffer, RenderCommand cmd)
    {
        int cx = GetParameter<int>(cmd, "cx", 0);
        int cy = GetParameter<int>(cmd, "cy", 0);
        int radius = GetParameter<int>(cmd, "radius", 0);
        byte r = GetParameter<byte>(cmd, "r", 0);
        byte g = GetParameter<byte>(cmd, "g", 0);
        byte b = GetParameter<byte>(cmd, "b", 0);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.FillCircle(cx, cy, radius, r, g, b, a);
    }

    private static void ExecuteFillRect(PixelBuffer buffer, RenderCommand cmd)
    {
        int x = GetParameter<int>(cmd, "x", 0);
        int y = GetParameter<int>(cmd, "y", 0);
        int w = GetParameter<int>(cmd, "width", 0);
        int h = GetParameter<int>(cmd, "height", 0);
        byte r = GetParameter<byte>(cmd, "r", 0);
        byte g = GetParameter<byte>(cmd, "g", 0);
        byte b = GetParameter<byte>(cmd, "b", 0);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.FillRect(x, y, w, h, r, g, b, a);
    }

    private static void ExecuteDrawTriangle(PixelBuffer buffer, RenderCommand cmd)
    {
        int x0 = GetParameter<int>(cmd, "x0", 0);
        int y0 = GetParameter<int>(cmd, "y0", 0);
        int x1 = GetParameter<int>(cmd, "x1", 0);
        int y1 = GetParameter<int>(cmd, "y1", 0);
        int x2 = GetParameter<int>(cmd, "x2", 0);
        int y2 = GetParameter<int>(cmd, "y2", 0);
        byte r = GetParameter<byte>(cmd, "r", 0);
        byte g = GetParameter<byte>(cmd, "g", 0);
        byte b = GetParameter<byte>(cmd, "b", 0);
        byte a = GetParameter<byte>(cmd, "a", 255);
        buffer.DrawTriangle(x0, y0, x1, y1, x2, y2, r, g, b, a);
    }

    private static T GetParameter<T>(RenderCommand cmd, string key, T defaultValue)
    {
        if (cmd.Parameters.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    private static void ParseHexColor(string hex, out byte r, out byte g, out byte b)
    {
        r = 0;
        g = 0;
        b = 0;

        if (string.IsNullOrEmpty(hex))
            return;

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            b = System.Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else if (hex.Length == 3)
        {
            r = System.Convert.ToByte(new string(hex[0], 2), 16);
            g = System.Convert.ToByte(new string(hex[1], 2), 16);
            b = System.Convert.ToByte(new string(hex[2], 2), 16);
        }
    }
}
