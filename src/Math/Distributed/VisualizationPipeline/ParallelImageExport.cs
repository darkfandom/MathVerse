namespace MathVerse.Math.Distributed.VisualizationPipeline
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel image exporter that rasterizes triangle meshes into pixel buffers
    /// by partitioning scanlines across multiple threads.
    /// </summary>
    public sealed class ParallelImageExport
    {
        /// <summary>
        /// Rasterizes a set of triangles into a pixel buffer using parallel scanline partitioning.
        /// Each scanline row is processed independently, with triangles tested for overlap.
        /// Uses a simple flat-shading model with face normals for lighting.
        /// </summary>
        /// <param name="triangles">
        /// Array of triangles, where each triangle is represented by 9 doubles:
        /// [x0, y0, z0, x1, y1, z1, x2, y2, z2].
        /// </param>
        /// <param name="width">Output image width in pixels.</param>
        /// <param name="height">Output image height in pixels.</param>
        /// <returns>
        /// Pixel buffer of shape [height, width, 3] (RGB, values 0-255).
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="triangles"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when width or height is non-positive, or triangles have invalid data.
        /// </exception>
        public static byte[,,] RasterizeParallel(double[][] triangles, int width, int height)
        {
            if (triangles == null) throw new ArgumentNullException(nameof(triangles));
            if (width <= 0) throw new ArgumentException("Width must be positive.", nameof(width));
            if (height <= 0) throw new ArgumentException("Height must be positive.", nameof(height));

            // Initialize framebuffer with background color (dark gray)
            byte[,,] framebuffer = new byte[height, width, 3];
            double[,] depthBuffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    framebuffer[y, x, 0] = 30;
                    framebuffer[y, x, 1] = 30;
                    framebuffer[y, x, 2] = 40;
                    depthBuffer[y, x] = double.MaxValue;
                }
            }

            // Precompute bounding boxes and face normals
            int triCount = triangles.Length;
            double[][] normals = new double[triCount][];
            int[][] boundingBoxes = new int[triCount][]; // [minY, maxY, minX, maxX]

            Parallel.For(0, triCount, t =>
            {
                double[] tri = triangles[t];
                if (tri == null || tri.Length < 9) return;

                double[] v0 = { tri[0], tri[1], tri[2] };
                double[] v1 = { tri[3], tri[4], tri[5] };
                double[] v2 = { tri[6], tri[7], tri[8] };

                // Compute face normal
                double[] e1 = { v1[0] - v0[0], v1[1] - v0[1], v1[2] - v0[2] };
                double[] e2 = { v2[0] - v0[0], v2[1] - v0[1], v2[2] - v0[2] };
                double[] n = Cross(e1, e2);
                double len = System.Math.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
                if (len > 1e-10)
                {
                    n[0] /= len;
                    n[1] /= len;
                    n[2] /= len;
                }
                normals[t] = n;

                // Compute screen-space bounding box
                double minX = System.Math.Min(v0[0], System.Math.Min(v1[0], v2[0]));
                double maxX = System.Math.Max(v0[0], System.Math.Max(v1[0], v2[0]));
                double minY = System.Math.Min(v0[1], System.Math.Min(v1[1], v2[1]));
                double maxY = System.Math.Max(v0[1], System.Math.Max(v1[1], v2[1]));

                int pixMinX = System.Math.Max(0, (int)System.Math.Floor(minX));
                int pixMaxX = System.Math.Min(width - 1, (int)System.Math.Ceiling(maxX));
                int pixMinY = System.Math.Max(0, (int)System.Math.Floor(minY));
                int pixMaxY = System.Math.Min(height - 1, (int)System.Math.Ceiling(maxY));

                boundingBoxes[t] = new int[] { pixMinY, pixMaxY, pixMinX, pixMaxX };
            });

            // Light direction for simple shading (normalized)
            double lightX = 0.4, lightY = 0.6, lightZ = 0.7;
            double lightLen = System.Math.Sqrt(lightX * lightX + lightY * lightY + lightZ * lightZ);
            lightX /= lightLen;
            lightY /= lightLen;
            lightZ /= lightLen;

            // Rasterize by partitioning scanlines
            Parallel.For(0, height, scanline =>
            {
                for (int t = 0; t < triCount; t++)
                {
                    if (triangles[t] == null || triangles[t].Length < 9) continue;
                    if (boundingBoxes[t] == null) continue;

                    int triMinY = boundingBoxes[t][0];
                    int triMaxY = boundingBoxes[t][1];
                    int triMinX = boundingBoxes[t][2];
                    int triMaxX = boundingBoxes[t][3];

                    if (scanline < triMinY || scanline > triMaxY) continue;

                    double[] tri = triangles[t];
                    double x0 = tri[0], y0 = tri[1], z0 = tri[2];
                    double x1 = tri[3], y1 = tri[4], z1 = tri[5];
                    double x2 = tri[6], y2 = tri[7], z2 = tri[8];

                    double py = scanline + 0.5;

                    for (int px = triMinX; px <= triMaxX; px++)
                    {
                        double pxf = px + 0.5;

                        // Barycentric coordinates
                        double denom = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2);
                        if (System.Math.Abs(denom) < 1e-10) continue;

                        double w0 = ((y1 - y2) * (pxf - x2) + (x2 - x1) * (py - y2)) / denom;
                        double w1 = ((y2 - y0) * (pxf - x2) + (x0 - x2) * (py - y2)) / denom;
                        double w2 = 1.0 - w0 - w1;

                        if (w0 < 0.0 || w1 < 0.0 || w2 < 0.0) continue;

                        // Interpolate depth
                        double z = w0 * z0 + w1 * z1 + w2 * z2;

                        if (z < depthBuffer[scanline, px])
                        {
                            depthBuffer[scanline, px] = z;

                            // Simple directional lighting
                            double shade = System.Math.Abs(normals[t][0] * lightX
                                + normals[t][1] * lightY
                                + normals[t][2] * lightZ);
                            shade = System.Math.Max(0.2, System.Math.Min(1.0, shade));

                            byte r = (byte)(shade * 180.0);
                            byte g = (byte)(shade * 160.0);
                            byte b = (byte)(shade * 220.0);

                            framebuffer[scanline, px, 0] = r;
                            framebuffer[scanline, px, 1] = g;
                            framebuffer[scanline, px, 2] = b;
                        }
                    }
                }
            });

            return framebuffer;
        }

        /// <summary>
        /// Converts a rasterized framebuffer to a flat RGB byte array (row-major, 3 bytes per pixel).
        /// </summary>
        /// <param name="framebuffer">Rasterized framebuffer from <see cref="RasterizeParallel"/>.</param>
        /// <returns>Flat byte array of length height * width * 3.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="framebuffer"/> is null.</exception>
        public static byte[] ToFlatRgb(byte[,,] framebuffer)
        {
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));

            int height = framebuffer.GetLength(0);
            int width = framebuffer.GetLength(1);
            byte[] flat = new byte[height * width * 3];

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = (y * width + x) * 3;
                    flat[idx] = framebuffer[y, x, 0];
                    flat[idx + 1] = framebuffer[y, x, 1];
                    flat[idx + 2] = framebuffer[y, x, 2];
                }
            });

            return flat;
        }

        /// <summary>
        /// Generates a simple PPM (Portable Pixmap) header string for the given dimensions.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>PPM header string.</returns>
        public static string GeneratePpmHeader(int width, int height)
        {
            return $"P6\n{width} {height}\n255\n";
        }

        private static double[] Cross(double[] a, double[] b)
        {
            return new double[]
            {
                a[1] * b[2] - a[2] * b[1],
                a[2] * b[0] - a[0] * b[2],
                a[0] * b[1] - a[1] * b[0]
            };
        }
    }
}
