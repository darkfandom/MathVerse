namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel model trainer that trains multiple models concurrently, each with its own
    /// training function, enabling ensemble or hyperparameter search workflows.
    /// </summary>
    public sealed class ParallelModelTrainer
    {
        /// <summary>
        /// Trains multiple models in parallel. Each model is defined by a training function
        /// that receives inputs and outputs and returns a loss/accuracy metric.
        /// All models share the same training data.
        /// </summary>
        /// <param name="trainers">
        /// 2D array of trainer functions. The outer dimension selects the model variant,
        /// and the inner dimension provides alternative trainers (e.g., for cross-validation).
        /// Signature for each trainer: (double[][] inputs, double[][] outputs) -> double metric.
        /// </param>
        /// <param name="inputs">Training input data.</param>
        /// <param name="outputs">Training output/target data.</param>
        /// <returns>
        /// 2D array of metrics with the same shape as <paramref name="trainers"/>.
        /// Each element is the metric returned by the corresponding trainer.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when trainers or data arrays are empty.</exception>
        public static double[][] TrainModels(
            Func<double[][], double[][], double>[][] trainers,
            double[][] inputs,
            double[][] outputs)
        {
            if (trainers == null) throw new ArgumentNullException(nameof(trainers));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            if (outputs == null) throw new ArgumentNullException(nameof(outputs));
            if (trainers.Length == 0) throw new ArgumentException("Trainers array must not be empty.", nameof(trainers));
            if (inputs.Length == 0) throw new ArgumentException("Inputs must not be empty.", nameof(inputs));

            int outerCount = trainers.Length;
            double[][] results = new double[outerCount][];

            Parallel.For(0, outerCount, i =>
            {
                int innerCount = trainers[i].Length;
                results[i] = new double[innerCount];

                // Train each variant within the outer model in parallel
                Parallel.For(0, innerCount, j =>
                {
                    if (trainers[i][j] != null)
                    {
                        results[i][j] = trainers[i][j](inputs, outputs);
                    }
                });
            });

            return results;
        }
    }
}
