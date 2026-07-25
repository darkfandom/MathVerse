namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel inference engine that applies a model to multiple input samples concurrently,
    /// maximizing throughput for batch prediction workloads.
    /// </summary>
    public sealed class ParallelInferenceEngine
    {
        /// <summary>
        /// Generates predictions for a batch of inputs in parallel. Each input sample
        /// is processed independently through the model function.
        /// </summary>
        /// <param name="model">
        /// Model inference function.
        /// Signature: (double[] input) -> double[] prediction.
        /// </param>
        /// <param name="inputs">Array of input samples to predict.</param>
        /// <returns>
        /// Array of predictions, one per input sample, in the same order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="model"/> or <paramref name="inputs"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="inputs"/> is empty.</exception>
        public static double[][] PredictBatch(Func<double[], double[]> model, double[][] inputs)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            if (inputs.Length == 0) throw new ArgumentException("Inputs must not be empty.", nameof(inputs));

            double[][] predictions = new double[inputs.Length][];

            Parallel.For(0, inputs.Length, i =>
            {
                predictions[i] = model(inputs[i]);
            });

            return predictions;
        }
    }
}
