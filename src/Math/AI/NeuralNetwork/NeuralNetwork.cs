namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;

/// <summary>A layer in a sequential neural network.</summary>
public sealed class NeuralLayer
{
    /// <summary>Number of inputs to this layer.</summary>
    public int InputSize { get; init; }

    /// <summary>Number of outputs from this layer.</summary>
    public int OutputSize { get; init; }

    /// <summary>Weight matrix (OutputSize × InputSize) stored row-major.</summary>
    public double[] Weights { get; init; }

    /// <summary>Bias vector (OutputSize).</summary>
    public double[] Biases { get; init; }

    /// <summary>Layer type discriminator.</summary>
    public string ActivationType { get; init; } = "Linear";

    /// <summary>Creates a new layer with Xavier-initialised weights.</summary>
    /// <param name="inputSize">Number of inputs.</param>
    /// <param name="outputSize">Number of outputs.</param>
    /// <param name="activationType">Activation function name.</param>
    /// <param name="rng">Random source for weight initialisation.</param>
    public NeuralLayer(int inputSize, int outputSize, string activationType, Random rng)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        ActivationType = activationType;
        Weights = new double[outputSize * inputSize];
        Biases = new double[outputSize];

        // Xavier initialisation
        double limit = System.Math.Sqrt(6.0 / (inputSize + outputSize));
        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = (rng.NextDouble() * 2.0 - 1.0) * limit;
        }
    }

    private NeuralLayer(int inputSize, int outputSize, double[] weights, double[] biases, string activationType)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        Weights = weights;
        Biases = biases;
        ActivationType = activationType;
    }

    /// <summary>Creates a copy of this layer with updated weights.</summary>
    /// <param name="newWeights">New weight array.</param>
    /// <returns>A new <see cref="NeuralLayer"/>.</returns>
    public NeuralLayer WithWeights(double[] newWeights) =>
        new(InputSize, OutputSize, (double[])newWeights.Clone(), (double[])Biases.Clone(), ActivationType);
}

/// <summary>A sequential neural network composed of stacked layers.</summary>
public sealed class SequentialNetwork
{
    private readonly List<NeuralLayer> _layers = [];
    private readonly List<double[]> _activations = [];
    private readonly List<double[]> _preActivations = [];

    /// <summary>Number of layers in the network.</summary>
    public int LayerCount => _layers.Count;

    /// <summary>Read-only access to the layers.</summary>
    public IReadOnlyList<NeuralLayer> Layers => _layers;

    /// <summary>Creates an empty sequential network.</summary>
    public SequentialNetwork() { }

    /// <summary>Adds a layer to the network.</summary>
    /// <param name="layer">Layer to add.</param>
    public void AddLayer(NeuralLayer layer) => _layers.Add(layer);

    /// <summary>Runs a forward pass through the network.</summary>
    /// <param name="input">Input vector.</param>
    /// <returns>Output vector.</returns>
    public double[] Forward(double[] input)
    {
        _activations.Clear();
        _preActivations.Clear();

        double[] current = input;
        _activations.Add(current);

        for (int l = 0; l < _layers.Count; l++)
        {
            NeuralLayer layer = _layers[l];
            double[] output = new double[layer.OutputSize];

            for (int j = 0; j < layer.OutputSize; j++)
            {
                double sum = layer.Biases[j];
                for (int i = 0; i < layer.InputSize; i++)
                {
                    sum += layer.Weights[j * layer.InputSize + i] * current[i];
                }
                output[j] = sum;
            }

            _preActivations.Add(output);

            // Apply activation
            double[] activated = new double[layer.OutputSize];
            for (int j = 0; j < layer.OutputSize; j++)
            {
                activated[j] = ApplyActivation(output[j], layer.ActivationType);
            }

            _activations.Add(activated);
            current = activated;
        }

        return current;
    }

    /// <summary>Runs backpropagation and updates weights.</summary>
    /// <param name="target">Target output vector.</param>
    /// <param name="learningRate">Learning rate.</param>
    public void Backward(double[] target, double learningRate)
    {
        if (_layers.Count == 0) return;

        int lastLayerIdx = _layers.Count - 1;

        // Compute output layer delta
        double[] outputAct = _activations[lastLayerIdx + 1];
        double[] delta = new double[outputAct.Length];

        for (int j = 0; j < delta.Length; j++)
        {
            double error = outputAct[j] - target[j];
            delta[j] = error * ApplyActivationDerivative(_preActivations[lastLayerIdx][j],
                _layers[lastLayerIdx].ActivationType);
        }

        // Backpropagate through layers
        for (int l = lastLayerIdx; l >= 0; l--)
        {
            NeuralLayer layer = _layers[l];
            double[] inputAct = _activations[l];

            // Update weights and biases
            for (int j = 0; j < layer.OutputSize; j++)
            {
                for (int i = 0; i < layer.InputSize; i++)
                {
                    layer.Weights[j * layer.InputSize + i] -= learningRate * delta[j] * inputAct[i];
                }
                layer.Biases[j] -= learningRate * delta[j];
            }

            // Compute delta for previous layer
            if (l > 0)
            {
                double[] prevDelta = new double[layer.InputSize];
                for (int i = 0; i < layer.InputSize; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < layer.OutputSize; j++)
                    {
                        sum += layer.Weights[j * layer.InputSize + i] * delta[j];
                    }
                    prevDelta[i] = sum * ApplyActivationDerivative(_preActivations[l - 1][i],
                        _layers[l - 1].ActivationType);
                }
                delta = prevDelta;
            }
        }
    }

    private static double ApplyActivation(double x, string type)
    {
        return type.ToUpperInvariant() switch
        {
            "RELU" => x > 0 ? x : 0.0,
            "SIGMOID" => 1.0 / (1.0 + System.Math.Exp(-x)),
            "TANH" => System.Math.Tanh(x),
            "LEAKYRELU" => x > 0 ? x : 0.01 * x,
            "SWISH" => x / (1.0 + System.Math.Exp(-x)),
            "SOFTPLUS" => System.Math.Log(1.0 + System.Math.Exp(x)),
            _ => x, // Linear
        };
    }

    private static double ApplyActivationDerivative(double x, string type)
    {
        return type.ToUpperInvariant() switch
        {
            "RELU" => x > 0 ? 1.0 : 0.0,
            "SIGMOID" => SigmoidDerivative(x),
            "TANH" => TanhDerivative(x),
            "LEAKYRELU" => x > 0 ? 1.0 : 0.01,
            "SWISH" => SwishDerivative(x),
            "SOFTPLUS" => SigmoidRaw(x),
            _ => 1.0,
        };
    }

    private static double SigmoidDerivative(double x)
    {
        double s = SigmoidRaw(x);
        return s * (1.0 - s);
    }

    private static double TanhDerivative(double x)
    {
        double t = System.Math.Tanh(x);
        return 1.0 - t * t;
    }

    private static double SwishDerivative(double x)
    {
        double s = SigmoidRaw(x);
        return s + x * s * (1.0 - s);
    }

    private static double SigmoidRaw(double x)
    {
        return 1.0 / (1.0 + System.Math.Exp(-x));
    }
}

/// <summary>Fluent builder for constructing <see cref="SequentialNetwork"/> instances.</summary>
public sealed class NeuralNetworkBuilder
{
    private readonly Random _rng;
    private readonly List<(int Size, string Activation)> _layers = [];
    private int _inputSize;

    /// <summary>Creates a new builder.</summary>
    /// <param name="seed">Random seed for weight initialisation.</param>
    public NeuralNetworkBuilder(int seed = 42)
    {
        _rng = new Random(seed);
    }

    /// <summary>Sets the input dimension.</summary>
    /// <param name="size">Input size.</param>
    /// <returns>This builder for chaining.</returns>
    public NeuralNetworkBuilder Input(int size)
    {
        _inputSize = size;
        return this;
    }

    /// <summary>Adds a dense (fully-connected) layer.</summary>
    /// <param name="size">Number of neurons.</param>
    /// <param name="activation">Activation function (e.g. "ReLU", "Sigmoid", "Tanh", "Linear").</param>
    /// <returns>This builder for chaining.</returns>
    public NeuralNetworkBuilder Dense(int size, string activation = "ReLU")
    {
        _layers.Add((size, activation));
        return this;
    }

    /// <summary>Adds a dropout layer (no-op in forward pass, stored for completeness).</summary>
    /// <param name="rate">Dropout rate (0–1).</param>
    /// <returns>This builder for chaining.</returns>
    public NeuralNetworkBuilder Dropout(double rate)
    {
        _layers.Add(((int)(rate * 100), "Dropout"));
        return this;
    }

    /// <summary>Builds and returns the configured <see cref="SequentialNetwork"/>.</summary>
    /// <returns>A new sequential network.</returns>
    public SequentialNetwork Build()
    {
        SequentialNetwork network = new();
        int prevSize = _inputSize;

        foreach ((int size, string activation) in _layers)
        {
            if (activation == "Dropout")
            {
                // Dropout is ignored in inference; skip layer creation
                continue;
            }

            NeuralLayer layer = new(prevSize, size, activation, _rng);
            network.AddLayer(layer);
            prevSize = size;
        }

        return network;
    }
}
