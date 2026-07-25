namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Distributed optimizer that coordinates multiple worker threads computing partial
    /// gradients on data partitions and combines them for synchronized gradient descent.
    /// </summary>
    public sealed class DistributedOptimizer
    {
        /// <summary>
        /// Performs distributed gradient descent optimization. The data is partitioned
        /// across workers, each computes a partial gradient, and the combined gradient
        /// is applied to update the parameters.
        /// </summary>
        /// <param name="gradientFunc">
        /// Gradient computation function for a data partition.
        /// Signature: (double[] parameters, double[][] partition) -> double[] partialGradient.
        /// </param>
        /// <param name="initialParams">Initial parameter vector.</param>
        /// <param name="data">Full training data to partition across workers.</param>
        /// <param name="learningRate">Step size for gradient descent.</param>
        /// <param name="epochs">Number of optimization epochs.</param>
        /// <returns>The optimized parameter vector.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when parameters are invalid or data is empty.
        /// </exception>
        public static double[] Optimize(
            Func<double[], double[][], double[]> gradientFunc,
            double[] initialParams,
            double[][] data,
            double learningRate,
            int epochs)
        {
            if (gradientFunc == null) throw new ArgumentNullException(nameof(gradientFunc));
            if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.", nameof(data));
            if (epochs < 0) throw new ArgumentException("Epochs must be non-negative.", nameof(epochs));

            int paramDim = initialParams.Length;
            double[] parameters = new double[paramDim];
            System.Array.Copy(initialParams, parameters, paramDim);

            int workerCount = System.Environment.ProcessorCount;
            int partitionSize = System.Math.Max(1, data.Length / workerCount);
            int partitionCount = (data.Length + partitionSize - 1) / partitionSize;

            // Pre-partition data
            double[][][] partitions = new double[partitionCount][][];
            for (int p = 0; p < partitionCount; p++)
            {
                int start = p * partitionSize;
                int end = System.Math.Min(start + partitionSize, data.Length);
                int count = end - start;

                partitions[p] = new double[count][];
                System.Array.Copy(data, start, partitions[p], 0, count);
            }

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double[][] partialGradients = new double[partitionCount][];

                // Compute partial gradients in parallel
                Parallel.For(0, partitionCount, p =>
                {
                    partialGradients[p] = gradientFunc(parameters, partitions[p]);
                });

                // Combine gradients by averaging
                double[] combinedGradient = new double[paramDim];

                for (int p = 0; p < partitionCount; p++)
                {
                    for (int d = 0; d < paramDim && d < partialGradients[p].Length; d++)
                    {
                        combinedGradient[d] += partialGradients[p][d];
                    }
                }

                double normalizer = 1.0 / partitionCount;
                for (int d = 0; d < paramDim; d++)
                {
                    combinedGradient[d] *= normalizer;
                }

                // Update parameters: theta = theta - lr * gradient
                for (int d = 0; d < paramDim; d++)
                {
                    parameters[d] -= learningRate * combinedGradient[d];
                }
            }

            return parameters;
        }

        /// <summary>
        /// Performs distributed optimization with adaptive learning rate (Adam-like).
        /// Maintains per-parameter first and second moment estimates across workers.
        /// </summary>
        /// <param name="gradientFunc">
        /// Gradient computation function for a data partition.
        /// Signature: (double[] parameters, double[][] partition) -> double[] partialGradient.
        /// </param>
        /// <param name="initialParams">Initial parameter vector.</param>
        /// <param name="data">Full training data to partition across workers.</param>
        /// <param name="learningRate">Base step size (default: 0.001).</param>
        /// <param name="beta1">Exponential decay rate for first moment (default: 0.9).</param>
        /// <param name="beta2">Exponential decay rate for second moment (default: 0.999).</param>
        /// <param name="epochs">Number of optimization epochs.</param>
        /// <returns>The optimized parameter vector.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static double[] OptimizeAdam(
            Func<double[], double[][], double[]> gradientFunc,
            double[] initialParams,
            double[][] data,
            double learningRate = 0.001,
            double beta1 = 0.9,
            double beta2 = 0.999,
            int epochs = 100)
        {
            if (gradientFunc == null) throw new ArgumentNullException(nameof(gradientFunc));
            if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.", nameof(data));
            if (epochs < 0) throw new ArgumentException("Epochs must be non-negative.", nameof(epochs));

            int paramDim = initialParams.Length;
            double[] parameters = new double[paramDim];
            System.Array.Copy(initialParams, parameters, paramDim);

            double[] m = new double[paramDim]; // First moment
            double[] v = new double[paramDim]; // Second moment
            double epsilon = 1e-8;

            int workerCount = System.Environment.ProcessorCount;
            int partitionSize = System.Math.Max(1, data.Length / workerCount);
            int partitionCount = (data.Length + partitionSize - 1) / partitionSize;

            double[][][] partitions = new double[partitionCount][][];
            for (int p = 0; p < partitionCount; p++)
            {
                int start = p * partitionSize;
                int end = System.Math.Min(start + partitionSize, data.Length);
                int count = end - start;

                partitions[p] = new double[count][];
                System.Array.Copy(data, start, partitions[p], 0, count);
            }

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                double[][] partialGradients = new double[partitionCount][];

                Parallel.For(0, partitionCount, p =>
                {
                    partialGradients[p] = gradientFunc(parameters, partitions[p]);
                });

                double[] combinedGradient = new double[paramDim];
                for (int p = 0; p < partitionCount; p++)
                {
                    for (int d = 0; d < paramDim && d < partialGradients[p].Length; d++)
                    {
                        combinedGradient[d] += partialGradients[p][d];
                    }
                }

                double normalizer = 1.0 / partitionCount;

                // Adam update
                for (int d = 0; d < paramDim; d++)
                {
                    double g = combinedGradient[d] * normalizer;
                    m[d] = beta1 * m[d] + (1.0 - beta1) * g;
                    v[d] = beta2 * v[d] + (1.0 - beta2) * g * g;

                    double mHat = m[d] / (1.0 - System.Math.Pow(beta1, epoch));
                    double vHat = v[d] / (1.0 - System.Math.Pow(beta2, epoch));

                    parameters[d] -= learningRate * mHat / (System.Math.Sqrt(vHat) + epsilon);
                }
            }

            return parameters;
        }
    }
}
