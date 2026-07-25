namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Batch normalization layer that normalizes inputs across the batch dimension.</summary>
public sealed class BatchNormalizationLayer : ILayer
{
    private readonly int _inputSize;
    private readonly Tensor _gamma;
    private readonly Tensor _beta;
    private readonly Tensor _gammaGradients;
    private readonly Tensor _betaGradients;
    private double _runningMean;
    private double _runningVariance;
    private Tensor? _lastInput;
    private Tensor? _normalizedInput;
    private double _currentMean;
    private double _currentVariance;
    private const double Epsilon = 1e-5;
    private const double Momentum = 0.1;

    /// <summary>Gets the name of this layer.</summary>
    public string Name => "BatchNormalization";

    /// <summary>Gets the expected input shape.</summary>
    public TensorShape InputShape { get; }

    /// <summary>Gets the output shape (same as input).</summary>
    public TensorShape OutputShape { get; }

    /// <summary>Gets the learnable scale parameter (gamma).</summary>
    public Tensor Gamma => _gamma;

    /// <summary>Gets the learnable shift parameter (beta).</summary>
    public Tensor Beta => _beta;

    /// <summary>Initializes a new batch normalization layer.</summary>
    /// <param name="inputSize">The number of features to normalize.</param>
    public BatchNormalizationLayer(int inputSize)
    {
        _inputSize = inputSize;
        InputShape = TensorShape.Vector(inputSize);
        OutputShape = TensorShape.Vector(inputSize);

        _gamma = Tensor.Ones([1, inputSize]);
        _beta = Tensor.Zeros([1, inputSize]);
        _gammaGradients = Tensor.Zeros([1, inputSize]);
        _betaGradients = Tensor.Zeros([1, inputSize]);
        _runningMean = 0.0;
        _runningVariance = 1.0;
        _currentMean = 0.0;
        _currentVariance = 1.0;
    }

    /// <summary>Normalizes the input across the batch, then applies scale and shift.</summary>
    /// <param name="input">The input tensor of shape [batch, inputSize].</param>
    /// <param name="training">Whether the layer is in training mode.</param>
    /// <returns>The normalized, scaled, and shifted output tensor.</returns>
    public Tensor Forward(Tensor input, bool training = true)
    {
        if (input.Rank != 2)
        {
            throw new ArgumentException($"BatchNorm expects 2D input [batch, features]. Got rank {input.Rank}.");
        }

        _lastInput = input;
        int batchSize = input.Shape[0];
        int features = input.Shape[1];
        double[] result = new double[input.TotalSize];

        if (training)
        {
            // Compute batch mean and variance
            double mean = 0.0;
            for (int i = 0; i < input.TotalSize; i++)
            {
                mean += input.Data[i];
            }
            mean /= input.TotalSize;
            _currentMean = mean;

            double variance = 0.0;
            for (int i = 0; i < input.TotalSize; i++)
            {
                double diff = input.Data[i] - mean;
                variance += diff * diff;
            }
            variance /= input.TotalSize;
            _currentVariance = variance;

            // Update running statistics
            _runningMean = (1.0 - Momentum) * _runningMean + Momentum * mean;
            _runningVariance = (1.0 - Momentum) * _runningVariance + Momentum * variance;

            // Normalize
            double stdInv = 1.0 / System.Math.Sqrt(variance + Epsilon);
            double[] normData = new double[input.TotalSize];
            for (int i = 0; i < input.TotalSize; i++)
            {
                normData[i] = (input.Data[i] - mean) * stdInv;
            }
            _normalizedInput = new Tensor(input.Shape, normData);

            // Apply gamma and beta: output = gamma * normalized + beta
            double[] gammaData = _gamma.Data;
            double[] betaData = _beta.Data;
            for (int b = 0; b < batchSize; b++)
            {
                for (int f = 0; f < features; f++)
                {
                    result[b * features + f] =
                        gammaData[f] * normData[b * features + f] + betaData[f];
                }
            }
        }
        else
        {
            // Use running statistics for inference
            double stdInv = 1.0 / System.Math.Sqrt(_runningVariance + Epsilon);
            double[] gammaData = _gamma.Data;
            double[] betaData = _beta.Data;
            for (int b = 0; b < batchSize; b++)
            {
                for (int f = 0; f < features; f++)
                {
                    result[b * features + f] =
                        gammaData[f] * (input.Data[b * features + f] - _runningMean) * stdInv
                        + betaData[f];
                }
            }
        }

        return new Tensor(input.Shape, result);
    }

    /// <summary>Computes gradients for gamma, beta, and the input.</summary>
    /// <param name="outputGradient">The gradient of the loss with respect to the output.</param>
    /// <param name="learningRate">The learning rate (unused during gradient computation).</param>
    /// <returns>The gradient of the loss with respect to the input.</returns>
    public Tensor Backward(Tensor outputGradient, double learningRate)
    {
        if (_lastInput == null || _normalizedInput == null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        int batchSize = outputGradient.Shape[0];
        int features = outputGradient.Shape[1];
        double m = batchSize;

        // dGamma = sum over batch of (dout * normalizedInput)
        double[] dGamma = new double[features];
        double[] gradNormData = _normalizedInput.Data;
        double[] doutData = outputGradient.Data;
        for (int b = 0; b < batchSize; b++)
        {
            for (int f = 0; f < features; f++)
            {
                dGamma[f] += doutData[b * features + f] * gradNormData[b * features + f];
            }
        }

        // dBeta = sum over batch of dout
        double[] dBeta = new double[features];
        for (int b = 0; b < batchSize; b++)
        {
            for (int f = 0; f < features; f++)
            {
                dBeta[f] += doutData[b * features + f];
            }
        }

        // Store gradients
        double[] gradGData = _gammaGradients.Data;
        for (int f = 0; f < features; f++)
        {
            gradGData[f] = dGamma[f];
        }
        double[] gradBData = _betaGradients.Data;
        for (int f = 0; f < features; f++)
        {
            gradBData[f] = dBeta[f];
        }

        // Compute dInput
        double stdInv = 1.0 / System.Math.Sqrt(_currentVariance + Epsilon);
        double[] gammaData = _gamma.Data;
        double[] inputData = _lastInput.Data;
        double[] dInputData = new double[_lastInput.TotalSize];

        // dInput = (1/m) * gamma * stdInv * (m * dout - sum(dout) - normalizedInput * sum(dout * normalizedInput))
        double[] sumDout = new double[features];
        double[] sumDoutNorm = new double[features];
        for (int b = 0; b < batchSize; b++)
        {
            for (int f = 0; f < features; f++)
            {
                sumDout[f] += doutData[b * features + f];
                sumDoutNorm[f] += doutData[b * features + f] * gradNormData[b * features + f];
            }
        }

        for (int b = 0; b < batchSize; b++)
        {
            for (int f = 0; f < features; f++)
            {
                double normVal = gradNormData[b * features + f];
                dInputData[b * features + f] =
                    gammaData[f] * stdInv / m
                    * (m * doutData[b * features + f]
                       - sumDout[f]
                       - normVal * sumDoutNorm[f]);
            }
        }

        return new Tensor(_lastInput.Shape, dInputData);
    }

    /// <summary>Updates gamma and beta using the stored gradients.</summary>
    /// <param name="learningRate">The learning rate for the parameter update.</param>
    public void UpdateParameters(double learningRate)
    {
        double[] gData = _gamma.Data;
        double[] gradGData = _gammaGradients.Data;
        for (int i = 0; i < gData.Length; i++)
        {
            gData[i] -= learningRate * gradGData[i];
        }
        double[] bData = _beta.Data;
        double[] gradBData = _betaGradients.Data;
        for (int i = 0; i < bData.Length; i++)
        {
            bData[i] -= learningRate * gradBData[i];
        }
    }
}
