namespace MathVerse.Math.AI.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>Training loop for neural networks.</summary>
public sealed class ModelTrainer
{
    /// <summary>Trains a sequential neural network on the provided data.</summary>
    /// <param name="network">The neural network to train.</param>
    /// <param name="inputs">The training input data, each row is a sample.</param>
    /// <param name="targets">The training target data, each row is a sample.</param>
    /// <param name="options">The training configuration options.</param>
    /// <returns>A TrainingResult containing training metrics and history.</returns>
    public TrainingResult Train(SequentialNetwork network, double[][] inputs, double[][] targets,
        TrainingOptions options)
    {
        if (inputs.Length != targets.Length)
        {
            throw new ArgumentException(
                $"Input count ({inputs.Length}) must match target count ({targets.Length}).");
        }
        if (inputs.Length == 0)
        {
            throw new ArgumentException("Training data must not be empty.");
        }

        int inputSize = inputs[0].Length;
        int outputSize = targets[0].Length;
        var lossHistory = new List<double>();
        var stopwatch = Stopwatch.StartNew();

        var rng = new Random(options.RandomSeed);

        // Create index array for shuffling
        int[] indices = new int[inputs.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        bool converged = false;
        double lastLoss = double.MaxValue;

        for (int epoch = 0; epoch < options.Epochs; epoch++)
        {
            // Shuffle data if requested
            if (options.Shuffle)
            {
                FisherYatesShuffle(indices, rng);
            }

            double epochLoss = 0.0;
            int batchCount = 0;

            // Process mini-batches
            for (int batchStart = 0; batchStart < inputs.Length; batchStart += options.BatchSize)
            {
                int batchEnd = System.Math.Min(batchStart + options.BatchSize, inputs.Length);
                int batchSize = batchEnd - batchStart;

                // Accumulate gradients over the batch
                for (int sampleIdx = batchStart; sampleIdx < batchEnd; sampleIdx++)
                {
                    int idx = indices[sampleIdx];

                    // Forward pass
                    Tensor inputTensor = WrapInput(inputs[idx]);
                    Tensor output = network.Forward(inputTensor, training: true);

                    // Compute loss
                    double[] predicted = ExtractOutput(output);
                    double[] actual = targets[idx];
                    double sampleLoss = LossFunctions.Compute(predicted, actual, options.LossFunction);
                    epochLoss += sampleLoss;

                    // Compute loss gradient
                    double[] lossGrad = LossFunctions.Gradient(predicted, actual, options.LossFunction);
                    Tensor lossGradient = WrapInput(lossGrad);

                    // Backward pass
                    network.Backward(lossGradient, options.LearningRate);
                }

                // Update parameters once per batch
                network.UpdateParameters(options.LearningRate);
                batchCount++;
            }

            double avgLoss = epochLoss / inputs.Length;
            lossHistory.Add(avgLoss);

            // Check convergence (loss change less than threshold)
            if (System.Math.Abs(lastLoss - avgLoss) < 1e-10)
            {
                converged = true;
            }
            lastLoss = avgLoss;
        }

        stopwatch.Stop();

        return new TrainingResult
        {
            LossHistory = lossHistory.ToArray(),
            EpochsCompleted = options.Epochs,
            FinalLoss = lossHistory.Count > 0 ? lossHistory[^1] : 0.0,
            Converged = converged,
            ElapsedTime = stopwatch.Elapsed
        };
    }

    /// <summary>Wraps a double array into a 2D tensor (single sample, shape [1, N]).</summary>
    /// <param name="data">The data array.</param>
    /// <returns>A 1xN tensor.</returns>
    private static Tensor WrapInput(double[] data)
    {
        double[] flat = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            flat[i] = data[i];
        }
        return new Tensor([1, data.Length], flat);
    }

    /// <summary>Extracts a 1D double array from a 2D tensor (single row).</summary>
    /// <param name="tensor">The tensor to extract from.</param>
    /// <returns>A double array containing the values.</returns>
    private static double[] ExtractOutput(Tensor tensor)
    {
        double[] result = new double[tensor.TotalSize];
        double[] data = tensor.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = data[i];
        }
        return result;
    }

    /// <summary>Shuffles an array using the Fisher-Yates algorithm.</summary>
    /// <param name="array">The array to shuffle in place.</param>
    /// <param name="rng">The random number generator.</param>
    private static void FisherYatesShuffle(int[] array, Random rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}

/// <summary>Configuration options for neural network training.</summary>
public sealed class TrainingOptions
{
    /// <summary>Gets the number of training epochs.</summary>
    public int Epochs { get; init; } = 100;

    /// <summary>Gets the learning rate.</summary>
    public double LearningRate { get; init; } = 0.01;

    /// <summary>Gets the loss function type.</summary>
    public LossType LossFunction { get; init; } = LossType.MSE;

    /// <summary>Gets the mini-batch size.</summary>
    public int BatchSize { get; init; } = 32;

    /// <summary>Gets whether to shuffle training data each epoch.</summary>
    public bool Shuffle { get; init; } = true;

    /// <summary>Gets the random seed for reproducibility.</summary>
    public int RandomSeed { get; init; } = 42;

    /// <summary>Gets a default training options instance.</summary>
    public static TrainingOptions Default => new();
}

/// <summary>Contains the results of a training run.</summary>
public sealed class TrainingResult
{
    /// <summary>Gets the loss value at each epoch.</summary>
    public double[] LossHistory { get; init; } = [];

    /// <summary>Gets the number of epochs completed.</summary>
    public int EpochsCompleted { get; init; }

    /// <summary>Gets the final loss value.</summary>
    public double FinalLoss { get; init; }

    /// <summary>Gets whether the training converged.</summary>
    public bool Converged { get; init; }

    /// <summary>Gets the total elapsed training time.</summary>
    public TimeSpan ElapsedTime { get; init; }
}
