namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Fully connected (dense) layer: output = input * weights + bias.</summary>
public sealed class DenseLayer : ILayer
{
    private readonly Tensor _weights;
    private readonly Tensor _bias;
    private readonly Tensor _weightGradients;
    private readonly Tensor _biasGradients;
    private Tensor? _lastInput;

    /// <summary>Gets the name of this layer.</summary>
    public string Name => "Dense";

    /// <summary>Gets the expected input shape.</summary>
    public TensorShape InputShape { get; }

    /// <summary>Gets the output shape.</summary>
    public TensorShape OutputShape { get; }

    /// <summary>Gets the weight matrix of this layer.</summary>
    public Tensor Weights => _weights;

    /// <summary>Gets the bias vector of this layer.</summary>
    public Tensor Bias => _bias;

    /// <summary>Initializes a new dense layer with Xavier/Glorot weight initialization.</summary>
    /// <param name="inputSize">The number of input features.</param>
    /// <param name="outputSize">The number of output features.</param>
    public DenseLayer(int inputSize, int outputSize)
    {
        InputShape = TensorShape.Vector(inputSize);
        OutputShape = TensorShape.Vector(outputSize);

        double limit = System.Math.Sqrt(6.0 / (inputSize + outputSize));
        var rng = new Random(42);
        double[] weightData = new double[inputSize * outputSize];
        for (int i = 0; i < weightData.Length; i++)
        {
            weightData[i] = (rng.NextDouble() * 2.0 - 1.0) * limit;
        }
        _weights = new Tensor([inputSize, outputSize], weightData);
        _bias = Tensor.Zeros([1, outputSize]);
        _weightGradients = Tensor.Zeros([inputSize, outputSize]);
        _biasGradients = Tensor.Zeros([1, outputSize]);
    }

    /// <summary>Performs the forward pass: computes output = input * weights + bias.</summary>
    /// <param name="input">The input tensor of shape [batch, inputSize].</param>
    /// <param name="training">Whether the layer is in training mode.</param>
    /// <returns>The output tensor of shape [batch, outputSize].</returns>
    public Tensor Forward(Tensor input, bool training = true)
    {
        if (training)
        {
            _lastInput = input;
        }
        var ops = new TensorOperations();
        Tensor matmulResult = ops.MatMul(input, _weights);
        return ops.Add(matmulResult, _bias);
    }

    /// <summary>Performs the backward pass, computing weight, bias, and input gradients.</summary>
    /// <param name="outputGradient">The gradient of the loss with respect to the output, shape [batch, outputSize].</param>
    /// <param name="learningRate">The learning rate (unused during gradient computation).</param>
    /// <returns>The gradient of the loss with respect to the input, shape [batch, inputSize].</returns>
    public Tensor Backward(Tensor outputGradient, double learningRate)
    {
        if (_lastInput == null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        int batchSize = outputGradient.Shape[0];
        int inputSize = _lastInput.Shape[1];
        int outputSize = outputGradient.Shape[1];

        // dW = input^T * outputGradient
        Tensor inputT = _lastInput.Transpose();
        var ops = new TensorOperations();
        Tensor dW = ops.MatMul(inputT, outputGradient);

        // db = sum of outputGradient along batch axis
        Tensor dB = ops.Sum(outputGradient, 0);

        // Store gradients
        double[] dWData = dW.Data;
        double[] gradWData = _weightGradients.Data;
        for (int i = 0; i < gradWData.Length; i++)
        {
            gradWData[i] = dWData[i];
        }
        double[] dbData = dB.Data;
        double[] gradBData = _biasGradients.Data;
        for (int i = 0; i < gradBData.Length; i++)
        {
            gradBData[i] = dbData[i];
        }

        // dInput = outputGradient * weights^T
        Tensor weightsT = _weights.Transpose();
        Tensor dInput = ops.MatMul(outputGradient, weightsT);

        return dInput;
    }

    /// <summary>Updates the weights and biases using the stored gradients.</summary>
    /// <param name="learningRate">The learning rate for the parameter update.</param>
    public void UpdateParameters(double learningRate)
    {
        double[] wData = _weights.Data;
        double[] gradWData = _weightGradients.Data;
        for (int i = 0; i < wData.Length; i++)
        {
            wData[i] -= learningRate * gradWData[i];
        }
        double[] bData = _bias.Data;
        double[] gradBData = _biasGradients.Data;
        for (int i = 0; i < bData.Length; i++)
        {
            bData[i] -= learningRate * gradBData[i];
        }
    }
}
