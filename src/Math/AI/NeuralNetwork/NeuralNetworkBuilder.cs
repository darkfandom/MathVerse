namespace MathVerse.Math.AI.NeuralNetwork;

/// <summary>Fluent builder for constructing neural networks.</summary>
public sealed class NeuralNetworkBuilder
{
    private readonly SequentialNetwork _network = new();
    private int _lastOutputSize;

    /// <summary>Gets the current number of layers in the network being built.</summary>
    public int LayerCount => _network.LayerCount;

    /// <summary>Adds a dense (fully connected) layer followed by an optional activation layer.</summary>
    /// <param name="inputSize">The number of input features.</param>
    /// <param name="outputSize">The number of output features.</param>
    /// <param name="activation">The activation function to apply after the dense layer.</param>
    /// <returns>This builder for method chaining.</returns>
    public NeuralNetworkBuilder Dense(int inputSize, int outputSize,
        ActivationType activation = ActivationType.ReLU)
    {
        _network.AddLayer(new DenseLayer(inputSize, outputSize));
        _lastOutputSize = outputSize;
        if (activation != ActivationType.None)
        {
            _network.AddLayer(new ActivationLayer(activation, TensorShape.Vector(outputSize)));
        }
        return this;
    }

    /// <summary>Adds a dropout layer.</summary>
    /// <param name="rate">The fraction of units to drop (0.0 to 1.0).</param>
    /// <returns>This builder for method chaining.</returns>
    public NeuralNetworkBuilder Dropout(double rate)
    {
        _network.AddLayer(new DropoutLayer(rate, TensorShape.Vector(_lastOutputSize)));
        return this;
    }

    /// <summary>Adds a batch normalization layer.</summary>
    /// <param name="inputSize">The number of features to normalize.</param>
    /// <returns>This builder for method chaining.</returns>
    public NeuralNetworkBuilder BatchNormalization(int inputSize)
    {
        _network.AddLayer(new BatchNormalizationLayer(inputSize));
        return this;
    }

    /// <summary>Adds an activation layer.</summary>
    /// <param name="type">The activation function type.</param>
    /// <returns>This builder for method chaining.</returns>
    public NeuralNetworkBuilder Activation(ActivationType type)
    {
        _network.AddLayer(new ActivationLayer(type, TensorShape.Vector(_lastOutputSize)));
        return this;
    }

    /// <summary>Builds and returns the completed sequential network.</summary>
    /// <returns>The constructed SequentialNetwork.</returns>
    public SequentialNetwork Build()
    {
        return _network;
    }
}
