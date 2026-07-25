namespace MathVerse.Math.Distributed.VisualizationPipeline
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel plot data generator that transforms multiple data series concurrently
    /// for visualization pipelines.
    /// </summary>
    public sealed class ParallelPlotGen
    {
        /// <summary>
        /// Generates plot data in parallel by applying a transform function to each
        /// data series independently. Each series is processed on a separate thread.
        /// </summary>
        /// <param name="dataSeries">
        /// Array of data series, where each series is an array of data points.
        /// </param>
        /// <param name="transform">
        /// Transform function applied to each data series.
        /// Signature: (double[] series) -> double[] transformedSeries.
        /// The output array length may differ from the input.
        /// </param>
        /// <returns>
        /// Array of transformed data series, one per input series, in the same order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataSeries"/> or <paramref name="transform"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="dataSeries"/> is empty.</exception>
        public static double[][] GenerateDataParallel(
            double[][] dataSeries,
            Func<double[], double[]> transform)
        {
            if (dataSeries == null) throw new ArgumentNullException(nameof(dataSeries));
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            if (dataSeries.Length == 0) throw new ArgumentException("Data series must not be empty.", nameof(dataSeries));

            double[][] results = new double[dataSeries.Length][];

            Parallel.For(0, dataSeries.Length, i =>
            {
                results[i] = transform(dataSeries[i]);
            });

            return results;
        }

        /// <summary>
        /// Generates multi-channel plot data by applying independent transforms to each
        /// channel of a multi-dimensional dataset in parallel.
        /// </summary>
        /// <param name="data">
        /// 2D data array of shape [numSamples, numChannels].
        /// </param>
        /// <param name="channelTransforms">
        /// Array of transform functions, one per channel.
        /// Signature: (double[] channelData) -> double[] transformedChannel.
        /// </param>
        /// <returns>
        /// 2D result array of shape [numSamples, numChannels] with each channel transformed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when data is empty or transform count doesn't match channel count.
        /// </exception>
        public static double[,] GenerateMultiChannel(
            double[,] data,
            Func<double[], double[]>[] channelTransforms)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (channelTransforms == null) throw new ArgumentNullException(nameof(channelTransforms));

            int numRows = data.GetLength(0);
            int numChannels = data.GetLength(1);

            if (numRows == 0 || numChannels == 0)
                throw new ArgumentException("Data must not be empty.", nameof(data));
            if (channelTransforms.Length != numChannels)
                throw new ArgumentException(
                    $"Transform count ({channelTransforms.Length}) must match channel count ({numChannels}).",
                    nameof(channelTransforms));

            double[,] result = new double[numRows, numChannels];

            Parallel.For(0, numChannels, ch =>
            {
                // Extract channel data
                double[] channelData = new double[numRows];
                for (int r = 0; r < numRows; r++)
                {
                    channelData[r] = data[r, ch];
                }

                // Transform
                double[] transformed = channelTransforms[ch](channelData);

                // Write back (clip to original length if different)
                int copyLen = System.Math.Min(transformed.Length, numRows);
                for (int r = 0; r < copyLen; r++)
                {
                    result[r, ch] = transformed[r];
                }
            });

            return result;
        }

        /// <summary>
        /// Generates interpolated plot data in parallel. For each data series, missing
        /// values are filled using linear interpolation.
        /// </summary>
        /// <param name="dataSeries">Array of data series with potential NaN gaps.</param>
        /// <param name="xValues">Array of x-axis values corresponding to each data point.</param>
        /// <returns>Array of interpolated data series.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static double[][] InterpolateParallel(double[][] dataSeries, double[] xValues)
        {
            if (dataSeries == null) throw new ArgumentNullException(nameof(dataSeries));
            if (xValues == null) throw new ArgumentNullException(nameof(xValues));

            double[][] results = new double[dataSeries.Length][];

            Parallel.For(0, dataSeries.Length, i =>
            {
                results[i] = LinearInterpolate(dataSeries[i], xValues);
            });

            return results;
        }

        /// <summary>
        /// Performs linear interpolation to fill NaN gaps in a data series.
        /// </summary>
        private static double[] LinearInterpolate(double[] data, double[] xValues)
        {
            int n = data.Length;
            double[] result = new double[n];
            System.Array.Copy(data, result, n);

            for (int i = 0; i < n; i++)
            {
                if (double.IsNaN(result[i]))
                {
                    // Find nearest valid neighbors
                    int prev = -1, next = -1;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (!double.IsNaN(result[j])) { prev = j; break; }
                    }
                    for (int j = i + 1; j < n; j++)
                    {
                        if (!double.IsNaN(result[j])) { next = j; break; }
                    }

                    if (prev >= 0 && next >= 0)
                    {
                        double t = (xValues[i] - xValues[prev]) / (xValues[next] - xValues[prev]);
                        result[i] = result[prev] + t * (result[next] - result[prev]);
                    }
                    else if (prev >= 0)
                    {
                        result[i] = result[prev];
                    }
                    else if (next >= 0)
                    {
                        result[i] = result[next];
                    }
                }
            }

            return result;
        }
    }
}
