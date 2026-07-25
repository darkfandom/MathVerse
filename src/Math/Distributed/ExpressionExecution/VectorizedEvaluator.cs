namespace MathVerse.Math.Distributed.ExpressionExecution
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    /// <summary>
    /// SIMD-accelerated expression evaluator using Vector for vectorized operations.
    /// </summary>
    public sealed class VectorizedEvaluator
    {
        /// <summary>
        /// Evaluates a function over inputs using SIMD acceleration when available.
        /// </summary>
        /// <param name="func">The function to evaluate.</param>
        /// <param name="inputs">Input values.</param>
        /// <returns>Array of results.</returns>
        public double[] EvaluateVectorized(Func<double, double> func, double[] inputs)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            var results = new double[inputs.Length];
            int vectorSize = Vector<double>.Count;
            int i = 0;

            if (vectorSize > 1 && inputs.Length >= vectorSize)
            {
                for (; i <= inputs.Length - vectorSize; i += vectorSize)
                {
                    var vector = new Vector<double>(inputs, i);
                    for (int j = 0; j < vectorSize; j++)
                    {
                        results[i + j] = func(vector[j]);
                    }
                }
            }

            for (; i < inputs.Length; i++)
            {
                results[i] = func(inputs[i]);
            }

            return results;
        }

        /// <summary>
        /// Evaluates a function over inputs in parallel with SIMD.
        /// </summary>
        /// <param name="func">The function to evaluate.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="parallelThreshold">Minimum size for parallel execution.</param>
        /// <returns>Array of results.</returns>
        public double[] EvaluateVectorizedParallel(Func<double, double> func, double[] inputs, int parallelThreshold = 10000)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            var results = new double[inputs.Length];

            if (inputs.Length < parallelThreshold)
            {
                return EvaluateVectorized(func, inputs);
            }

            int vectorSize = Vector<double>.Count;
            int chunkSize = System.Math.Max(vectorSize, inputs.Length / Environment.ProcessorCount);

            Parallel.For(0, (inputs.Length + chunkSize - 1) / chunkSize, chunkIndex =>
            {
                int start = chunkIndex * chunkSize;
                int end = System.Math.Min(start + chunkSize, inputs.Length);
                int i = start;

                if (vectorSize > 1 && (end - start) >= vectorSize)
                {
                    for (; i <= end - vectorSize; i += vectorSize)
                    {
                        for (int j = 0; j < vectorSize; j++)
                        {
                            results[i + j] = func(inputs[i + j]);
                        }
                    }
                }

                for (; i < end; i++)
                {
                    results[i] = func(inputs[i]);
                }
            });

            return results;
        }
    }
}
