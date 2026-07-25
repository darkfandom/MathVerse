namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Dropout layer for regularization during training.</summary>
public sealed class DropoutLayer : ILayer
{
    private readonly double _rate;
    private readonly Random _rng;
    private Tensor? _mask;

    /// <summary>Gets the name of this layer.</summary>
    public string Name => "Dropout";

    /// <summary>Gets the expected input shape.</summary>
    public TensorShape InputShape { get; private set; }

    /// <summary>Gets the output shape (same as input).</summary>
    public TensorShape OutputShape { get; private set; }

    /// <summary>Gets the dropout rate (fraction of units to drop).</summary>
    public double Rate => _rate;

    /// <summary>Initializes a new dropout layer.</summary>
    /// <param name="rate">The fraction of units to drop (0.0 to 1.0).</param>
    /// <param name="seed">The random seed for reproducibility.</param>
    public DropoutLayer(double rate, int seed = 42)
    {
        if (rate < 0.0 || rate >= 1.0)
        {
            throw new ArgumentException($"Dropout rate must be in [0, 1). Got {rate}.");
        }
        _rate = rate;
        _rng = new Random(seed);
        InputShape = TensorShape.Vector(1);
        OutputShape = TensorShape.Vector(1);
    }

    /// <summary>Initializes a new dropout layer with a known input shape.</summary>
    /// <param name="rate">The fraction of units to drop (0.0 to 1.0).</param>
    /// <param name="inputShape">The shape of the input tensors.</param>
    /// <param name="seed">The random seed for reproducibility.</param>
    public DropoutLayer(double rate, TensorShape inputShape, int seed = 42)
    {
        if (rate < 0.0 || rate >= 1.0)
        {
            throw new ArgumentException($"Dropout rate must be in [0, 1). Got {rate}.");
        }
        _rate = rate;
        _rng = new Random(seed);
        InputShape = inputShape;
        OutputShape = inputShape;
    }

    /// <summary>Applies dropout: randomly zeros elements during training, passes through during inference.</summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="training">Whether the layer is in training mode.</param>
    /// <returns>The output tensor (with or without dropout applied).</returns>
    public Tensor Forward(Tensor input, bool training = true)
    {
        InputShape = new TensorShape(input.Shape);
        OutputShape = new TensorShape(input.Shape);

        if (!training || _rate == 0.0)
        {
            return input;
        }

        // Inverted dropout: scale by 1/(1-rate) during training
        double scale = 1.0 / (1.0 - _rate);
        double[] result = new double[input.TotalSize];
        double[] data = input.Data;
        _mask = new Tensor(input.Shape);
        double[] maskData = _mask.Data;

        for (int i = 0; i < data.Length; i++)
        {
            if (_rng.NextDouble() >= _rate)
            {
                maskData[i] = scale;
                result[i] = data[i] * scale;
            }
            else
            {
                maskData[i] = 0.0;
                result[i] = 0.0;
            }
        }

        return new Tensor(input.Shape, result);
    }

    /// <summary>Computes the gradient by applying the same dropout mask.</summary>
    /// <param name="outputGradient">The gradient of the loss with respect to the output.</param>
    /// <param name="learningRate">The learning rate (unused for dropout).</param>
    /// <returns>The gradient of the loss with respect to the input.</returns>
    public Tensor Backward(Tensor outputGradient, double learningRate)
    {
        if (_mask == null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        double[] result = new double[outputGradient.TotalSize];
        double[] gradData = outputGradient.Data;
        double[] maskData = _mask.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = gradData[i] * maskData[i];
        }
        return new Tensor(outputGradient.Shape, result);
    }

    /// <summary>No parameters to update in a dropout layer.</summary>
    /// <param name="learningRate">The learning rate (unused).</param>
    public void UpdateParameters(double learningRate)
    {
        // No learnable parameters.
    }
}
