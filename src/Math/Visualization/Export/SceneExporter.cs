namespace MathVerse.Math.Visualization.Export;

/// <summary>Unified scene exporter that dispatches to format-specific exporters.</summary>
public sealed class SceneExporter
{
    /// <summary>Exports a visualization scene to the specified format.</summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="format">The export format.</param>
    public static void Export(Core.VisualizationScene scene, string filePath, ExportFormat format)
    {
        switch (format)
        {
            case ExportFormat.SVG:
                SVGExporter.Export(scene, filePath);
                break;

            case ExportFormat.PNG:
                ExportPNG(scene, filePath);
                break;

            case ExportFormat.JPEG:
                ExportJPEG(scene, filePath);
                break;

            case ExportFormat.JSON:
                JSONExporter.Export(scene, filePath);
                break;

            case ExportFormat.MathVerseScene:
                var sceneData = MathVerseSceneFormat.Serialize(scene);
                System.IO.File.WriteAllText(filePath, sceneData, System.Text.Encoding.UTF8);
                break;

            default:
                throw new System.ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format.");
        }
    }

    /// <summary>Exports a visualization scene to PNG format.</summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    public static void ExportPNG(Core.VisualizationScene scene, string filePath, int width = 1920, int height = 1080)
    {
        var buffer = PNGExporter.CreatePixelBuffer(scene, width, height);
        byte[] pngData = PNGExporter.ToPNG(buffer);
        System.IO.File.WriteAllBytes(filePath, pngData);
    }

    /// <summary>Exports a visualization scene to JPEG format.</summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="quality">JPEG quality (1-100).</param>
    public static void ExportJPEG(Core.VisualizationScene scene, string filePath, int width = 1920, int height = 1080, int quality = 85)
    {
        var buffer = PNGExporter.CreatePixelBuffer(scene, width, height);
        byte[] jpegData = EncodeJPEG(buffer, quality);
        System.IO.File.WriteAllBytes(filePath, jpegData);
    }

    /// <summary>Gets the file extension for a given export format.</summary>
    /// <param name="format">The export format.</param>
    /// <returns>The file extension including the dot.</returns>
    public static string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.SVG => ".svg",
            ExportFormat.PNG => ".png",
            ExportFormat.JPEG => ".jpg",
            ExportFormat.JSON => ".json",
            ExportFormat.MathVerseScene => ".mvscene",
            _ => ".bin"
        };
    }

    /// <summary>Gets the MIME type for a given export format.</summary>
    /// <param name="format">The export format.</param>
    /// <returns>The MIME type string.</returns>
    public static string GetMimeType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.SVG => "image/svg+xml",
            ExportFormat.PNG => "image/png",
            ExportFormat.JPEG => "image/jpeg",
            ExportFormat.JSON => "application/json",
            ExportFormat.MathVerseScene => "application/x-mathverse-scene",
            _ => "application/octet-stream"
        };
    }

    private static byte[] EncodeJPEG(PixelBuffer buffer, int quality)
    {
        quality = System.Math.Max(1, System.Math.Min(100, quality));

        var jpeg = new System.Collections.Generic.List<byte>();

        jpeg.Add(0xFF);
        jpeg.Add(0xD8);

        jpeg.AddRange(WriteJPEGMarker(0xE0, CreateJFIFHeader()));
        jpeg.AddRange(WriteJPEGMarker(0xDB, CreateQuantizationTable(quality)));
        jpeg.AddRange(WriteJPEGMarker(0xC0, CreateSOFHeader(buffer.Width, buffer.Height)));
        jpeg.AddRange(WriteJPEGMarker(0xC4, CreateHuffmanTable()));
        jpeg.AddRange(WriteJPEGMarker(0xDA, CreateScanHeader()));
        jpeg.AddRange(EncodeJPEGScan(buffer));
        jpeg.AddRange(new byte[] { 0xFF, 0xD9 });

        return jpeg.ToArray();
    }

    private static byte[] CreateJFIFHeader()
    {
        var data = new System.Collections.Generic.List<byte>();
        data.AddRange(new byte[] { 0x4A, 0x46, 0x49, 0x46, 0x00 }); // JFIF\0
        data.AddRange(new byte[] { 0x01, 0x01 }); // version
        data.Add(0x00); // units
        data.AddRange(new byte[] { 0x00, 0x01 }); // X density
        data.AddRange(new byte[] { 0x00, 0x01 }); // Y density
        data.Add(0x00); // thumbnail
        return data.ToArray();
    }

    private static byte[] CreateQuantizationTable(int quality)
    {
        double scale = quality < 50 ? 5000.0 / quality : 200.0 - 2.0 * quality;

        var table = new byte[65];
        table[0] = 0x00; // table ID

        int[] zigzagOrder = {
            0, 1, 8, 16, 9, 2, 3, 10,
            17, 24, 32, 25, 18, 11, 4, 5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13, 6, 7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63
        };

        byte[] baseValues = {
            16, 11, 10, 16, 24, 40, 51, 61,
            12, 12, 14, 19, 26, 58, 60, 55,
            14, 13, 16, 24, 40, 57, 69, 56,
            14, 17, 22, 29, 51, 87, 80, 62,
            18, 22, 37, 56, 68, 109, 103, 77,
            24, 35, 55, 64, 81, 104, 113, 92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103, 99
        };

        for (int i = 0; i < 64; i++)
        {
            int val = (int)(baseValues[i] * scale / 100.0);
            val = System.Math.Max(1, System.Math.Min(255, val));
            table[zigzagOrder[i] + 1] = (byte)val;
        }

        return table.ToArray();
    }

    private static byte[] CreateSOFHeader(int width, int height)
    {
        var data = new System.Collections.Generic.List<byte>();
        data.Add(0x08); // precision
        data.AddRange(new byte[] { (byte)((height >> 8) & 0xFF), (byte)(height & 0xFF) });
        data.AddRange(new byte[] { (byte)((width >> 8) & 0xFF), (byte)(width & 0xFF) });
        data.Add(0x01); // component count
        data.Add(0x01); // component ID
        data.Add(0x11); // sampling
        data.Add(0x00); // quantization table
        return data.ToArray();
    }

    private static byte[] CreateHuffmanTable()
    {
        var data = new System.Collections.Generic.List<byte>();
        data.Add(0x00); // DC table, ID 0

        byte[] bits = new byte[16];
        bits[0] = 0;
        for (int i = 1; i < 12; i++)
            bits[i] = 1;
        data.AddRange(bits);

        for (int i = 0; i < 12; i++)
            data.Add((byte)i);

        return data.ToArray();
    }

    private static byte[] CreateScanHeader()
    {
        var data = new System.Collections.Generic.List<byte>();
        data.Add(0x01); // component count
        data.Add(0x01); // component ID
        data.Add(0x00); // DC/AC table
        data.Add(0x00); // Ss
        data.Add(0x3F); // Se
        data.Add(0x00); // Ah/Al
        return data.ToArray();
    }

    private static System.Collections.Generic.List<byte> EncodeJPEGScan(PixelBuffer buffer)
    {
        var data = new System.Collections.Generic.List<byte>();

        for (int y = 0; y < buffer.Height; y++)
        {
            for (int x = 0; x < buffer.Width; x++)
            {
                var (r, g, b, _) = buffer.GetPixel(x, y);
                int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                gray = System.Math.Max(0, System.Math.Min(255, gray));
                int dc = gray - 128;

                if (dc == 0)
                {
                    data.Add(0x00);
                }
                else
                {
                    int bits = 0;
                    int temp = System.Math.Abs(dc);
                    while (temp > 0)
                    {
                        bits++;
                        temp >>= 1;
                    }

                    data.Add((byte)bits);

                    if (dc > 0)
                    {
                        data.Add((byte)dc);
                    }
                    else
                    {
                        data.Add((byte)(dc + (1 << bits) - 1));
                    }
                }
            }
        }

        return data;
    }

    private static System.Collections.Generic.List<byte> WriteJPEGMarker(byte marker, byte[] data)
    {
        var result = new System.Collections.Generic.List<byte>();
        result.Add(0xFF);
        result.Add(marker);
        result.AddRange(new byte[] { (byte)((data.Length + 2) >> 8), (byte)((data.Length + 2) & 0xFF) });
        result.AddRange(data);
        return result;
    }
}
