namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Supported loss function types.</summary>
public enum LossType
{
    /// <summary>Mean Squared Error loss.</summary>
    MSE,
    /// <summary>Cross-entropy loss for multi-class classification.</summary>
    CrossEntropy,
    /// <summary>Binary cross-entropy loss for binary classification.</summary>
    BinaryCrossEntropy,
    /// <summary>Mean Absolute Error loss.</summary>
    MAE,
    /// <summary>Huber loss (smooth L1).</summary>
    Huber
}

/// <summary>Provides loss computation and gradient functions for neural network training.</summary>
public static class LossFunctions
{
    private const double Epsilon = 1e-15;
    private const double HuberDelta = 1.0;

    /// <summary>Computes the loss value between predicted and actual values.</summary>
    /// <param name="predicted">The predicted output array.</param>
    /// <param name="actual">The target values array.</param>
    /// <param name="type">The type of loss function to use.</param>
    /// <returns>The computed loss value.</returns>
    public static double Compute(double[] predicted, double[] actual, LossType type)
    {
        if (predicted.Length != actual.Length)
        {
            throw new ArgumentException(
                $"Predicted ({predicted.Length}) and actual ({actual.Length}) arrays must have the same length.");
        }
        return type switch
        {
            LossType.MSE => ComputeMSE(predicted, actual),
            LossType.CrossEntropy => ComputeCrossEntropy(predicted, actual),
            LossType.BinaryCrossEntropy => ComputeBinaryCrossEntropy(predicted, actual),
            LossType.MAE => ComputeMAE(predicted, actual),
            LossType.Huber => ComputeHuber(predicted, actual),
            _ => throw new ArgumentException($"Unsupported loss type: {type}")
        };
    }

    /// <summary>Computes the gradient of the loss with respect to the predicted values.</summary>
    /// <param name="predicted">The predicted output array.</param>
    /// <param name="actual">The target values array.</param>
    /// <param name="type">The type of loss function to use.</param>
    /// <returns>The gradient array with respect to predicted.</returns>
    public static double[] Gradient(double[] predicted, double[] actual, LossType type)
    {
        if (predicted.Length != actual.Length)
        {
            throw new ArgumentException(
                $"Predicted ({predicted.Length}) and actual ({actual.Length}) arrays must have the same length.");
        }
        return type switch
        {
            LossType.MSE => GradientMSE(predicted, actual),
            LossType.CrossEntropy => GradientCrossEntropy(predicted, actual),
            LossType.BinaryCrossEntropy => GradientBinaryCrossEntropy(predicted, actual),
            LossType.MAE => GradientMAE(predicted, actual),
            LossType.Huber => GradientHuber(predicted, actual),
            _ => throw new ArgumentException($"Unsupported loss type: {type}")
        };
    }

    private static double ComputeMSE(double[] predicted, double[] actual)
    {
        double sum = 0.0;
        for (int i = 0; i < predicted.Length; i++)
        {
            double diff = predicted[i] - actual[i];
            sum += diff * diff;
        }
        return sum / predicted.Length;
    }

    private static double[] GradientMSE(double[] predicted, double[] actual)
    {
        double[] grad = new double[predicted.Length];
        for (int i = 0; i < predicted.Length; i++)
        {
            grad[i] = 2.0 * (predicted[i] - actual[i]) / predicted.Length;
        }
        return grad;
    }

    private static double ComputeCrossEntropy(double[] predicted, double[] actual)
    {
        double sum = 0.0;
        for (int i = 0; i < predicted.Length; i++)
        {
            double p = System.Math.Max(Epsilon, System.Math.Min(1.0 - Epsilon, predicted[i]));
            if (actual[i] > 0.0)
            {
                sum -= actual[i] * System.Math.Log(p);
            }
        }
        return sum;
    }

    private static double[] GradientCrossEntropy(double[] predicted, double[] actual)
    {
        double[] grad = new double[predicted.Length];
        for (int i = 0; i < predicted.Length; i++)
        {
            double p = System.Math.Max(Epsilon, System.Math.Min(1.0 - Epsilon, predicted[i]));
            if (actual[i] > 0.0)
            {
                grad[i] = -actual[i] / p;
            }
            else
            {
                grad[i] = 0.0;
            }
        }
        return grad;
    }

    private static double ComputeBinaryCrossEntropy(double[] predicted, double[] actual)
    {
        double sum = 0.0;
        for (int i = 0; i < predicted.Length; i++)
        {
            double p = System.Math.Max(Epsilon, System.Math.Min(1.0 - Epsilon, predicted[i]));
            sum -= actual[i] * System.Math.Log(p) + (1.0 - actual[i]) * System.Math.Log(1.0 - p);
        }
        return sum / predicted.Length;
    }

    private static double[] GradientBinaryCrossEntropy(double[] predicted, double[] actual)
    {
        double[] grad = new double[predicted.Length];
        for (int i = 0; i < predicted.Length; i++)
        {
            double p = System.Math.Max(Epsilon, System.Math.Min(1.0 - Epsilon, predicted.Length > 0 ? predicted[i] : 0.5));
            grad[i] = (-actual[i] / p + (1.0 - actual[i]) / (1.0 - p)) / predicted.Length;
        }
        return grad;
    }

    private static double ComputeMAE(double[] predicted, double[] actual)
    {
        double sum = 0.0;
        for (int i = 0; i < predicted.Length; i++)
        {
            sum += System.Math.Abs(predicted[i] - actual[i]);
        }
        return sum / predicted.Length;
    }

    private static double[] GradientMAE(double[] predicted, double[] actual)
    {
        double[] grad = new double[predicted.Length];
        for (int i = 0; i < predicted.Length; i++)
        {
            double diff = predicted[i] - actual[i];
            if (diff > 0.0)
                grad[i] = 1.0 / predicted.Length;
            else if (diff < 0.0)
                grad[i] = -1.0 / predicted.Length;
            else
                grad[i] = 0.0;
        }
        return grad;
    }

    private static double ComputeHuber(double[] predicted, double[] actual)
    {
        double sum = 0.0;
        for (int i = 0; i < predicted.Length; i++)
        {
            double diff = System.Math.Abs(predicted[i] - actual[i]);
            if (diff <= HuberDelta)
            {
                sum += 0.5 * diff * diff;
            }
            else
            {
                sum += HuberDelta * (diff - 0.5 * HuberDelta);
            }
        }
        return sum / predicted.Length;
    }

    private static double[] GradientHuber(double[] predicted, double[] actual)
    {
        double[] grad = new double[predicted.Length];
        for (int i = 0; i < predicted.Length; i++)
        {
            double diff = predicted[i] - actual[i];
            double absDiff = System.Math.Abs(diff);
            if (absDiff <= HuberDelta)
            {
                grad[i] = diff / predicted.Length;
            }
            else
            {
                grad[i] = HuberDelta * (diff > 0.0 ? 1.0 : -1.0) / predicted.Length;
            }
        }
        return grad;
    }
}
