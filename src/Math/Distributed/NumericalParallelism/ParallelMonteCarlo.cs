namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Monte Carlo integration.
    /// </summary>
    public sealed class ParallelMonteCarlo
    {
        /// <summary>
        /// Integrates a function over a multidimensional domain using Monte Carlo sampling.
        /// </summary>
        /// <param name="func">Function to integrate.</param>
        /// <param name="mins">Minimum bounds for each dimension.</param>
        /// <param name="maxs">Maximum bounds for each dimension.</param>
        /// <param name="samples">Total number of samples.</param>
        /// <returns>Approximate integral value.</returns>
        public double Integrate(Func<double[], double> func, double[] mins, double[] maxs, int samples = 100000)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (mins == null)
                throw new ArgumentNullException(nameof(mins));
            if (maxs == null)
                throw new ArgumentNullException(nameof(maxs));
            if (mins.Length != maxs.Length)
                throw new ArgumentException("Bounds arrays must have the same length.");

            int dimensions = mins.Length;
            double volume = 1;
            for (int d = 0; d < dimensions; d++)
                volume *= maxs[d] - mins[d];

            int threadCount = Environment.ProcessorCount;
            int samplesPerThread = samples / threadCount;
            double[] threadSums = new double[threadCount];

            Parallel.For(0, threadCount, t =>
            {
                var rng = new Random(Thread.CurrentThread.ManagedThreadId);
                double localSum = 0;
                int count = (t == threadCount - 1) ? samples - samplesPerThread * (threadCount - 1) : samplesPerThread;

                for (int i = 0; i < count; i++)
                {
                    var point = new double[dimensions];
                    for (int d = 0; d < dimensions; d++)
                    {
                        double range = maxs[d] - mins[d];
                        point[d] = mins[d] + rng.NextDouble() * range;
                    }
                    localSum += func(point);
                }

                threadSums[t] = localSum;
            });

            double totalSumDouble = 0;
            for (int t = 0; t < threadCount; t++)
                totalSumDouble += threadSums[t];

            return (totalSumDouble / samples) * volume;
        }

        /// <summary>
        /// Estimates the value of Pi using Monte Carlo method.
        /// </summary>
        /// <param name="samples">Number of samples.</param>
        /// <returns>Estimated value of Pi.</returns>
        public double EstimatePi(int samples = 1000000)
        {
            long insideCircle = 0;
            int threadCount = Environment.ProcessorCount;
            int samplesPerThread = samples / threadCount;
            long[] threadCounts = new long[threadCount];

            Parallel.For(0, threadCount, t =>
            {
                var rng = new Random(Thread.CurrentThread.ManagedThreadId);
                long localCount = 0;
                int count = (t == threadCount - 1) ? samples - samplesPerThread * (threadCount - 1) : samplesPerThread;

                for (int i = 0; i < count; i++)
                {
                    double x = rng.NextDouble() * 2 - 1;
                    double y = rng.NextDouble() * 2 - 1;
                    if (x * x + y * y <= 1)
                        localCount++;
                }

                threadCounts[t] = localCount;
            });

            for (int t = 0; t < threadCount; t++)
                insideCircle += threadCounts[t];

            return 4.0 * insideCircle / samples;
        }
    }
}
