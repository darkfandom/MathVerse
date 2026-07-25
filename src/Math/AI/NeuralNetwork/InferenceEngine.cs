namespace MathVerse.Math.AI.NeuralNetwork;

/// <summary>High-performance inference engine for trained networks.</summary>
public sealed class InferenceEngine
{
    private readonly SequentialNetwork _network;

    /// <summary>Initializes a new inference engine wrapping the specified network.</summary>
    /// <param name="network">The trained sequential network to use for inference.</param>
    public InferenceEngine(SequentialNetwork network)
    {
        _network = network;
    }

    /// <summary>Gets the underlying neural network.</summary>
    public SequentialNetwork Network => _network;

    /// <summary>Performs a prediction on a single input sample.</summary>
    /// <param name="input">The input feature array.</param>
    /// <returns>The predicted output array.</returns>
    public double[] Predict(double[] input)
    {
        Tensor inputTensor = new Tensor([1, input.Length], input);
        Tensor output = _network.Forward(inputTensor, training: false);
        return ExtractOutput(output);
    }

    /// <summary>Performs batch prediction on multiple input samples.</summary>
    /// <param name="inputs">An array of input feature arrays.</param>
    /// <returns>An array of predicted output arrays.</returns>
    public double[][] PredictBatch(double[][] inputs)
    {
        if (inputs.Length == 0)
        {
            return [];
        }

        double[][] results = new double[inputs.Length][];
        for (int i = 0; i < inputs.Length; i++)
        {
            results[i] = Predict(inputs[i]);
        }
        return results;
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
}
