namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;
using System.Text;

/// <summary>Represents a trained AI model with metadata, parameters, and inference capability.</summary>
public sealed class AIModel
{
    /// <summary>Unique identifier for this model instance.</summary>
    public string ModelId { get; }

    /// <summary>Discriminator indicating the model family (e.g. "LinearRegression", "MLP").</summary>
    public string ModelType { get; }

    /// <summary>UTC timestamp when the model was trained.</summary>
    public DateTime TrainedAt { get; init; }

    /// <summary>Hyper-parameters used during training.</summary>
    public ImmutableDictionary<string, double> HyperParameters { get; init; }

    /// <summary>Learned parameters (weights, biases, …) of the trained model.</summary>
    public ImmutableDictionary<string, double> TrainedParameters { get; init; }

    /// <summary>Evaluation metrics collected during training.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; }

    /// <summary>Initialises a new model.</summary>
    /// <param name="modelId">Unique identifier.</param>
    /// <param name="modelType">Model family discriminator.</param>
    public AIModel(string modelId, string modelType)
    {
        ModelId = modelId;
        ModelType = modelType;
        TrainedAt = DateTime.UtcNow;
        HyperParameters = ImmutableDictionary<string, double>.Empty;
        TrainedParameters = ImmutableDictionary<string, double>.Empty;
        Metrics = ImmutableDictionary<string, double>.Empty;
    }

    private AIModel(
        string modelId,
        string modelType,
        DateTime trainedAt,
        ImmutableDictionary<string, double> hyperParameters,
        ImmutableDictionary<string, double> trainedParameters,
        ImmutableDictionary<string, double> metrics)
    {
        ModelId = modelId;
        ModelType = modelType;
        TrainedAt = trainedAt;
        HyperParameters = hyperParameters;
        TrainedParameters = trainedParameters;
        Metrics = metrics;
    }

    /// <summary>Runs inference on a single input vector using a dot-product of the first
    /// <paramref name="input"/> elements against corresponding trained parameter values
    /// keyed by index (e.g. "w0", "w1", …). Returns <c>0</c> when no matching weights exist.</summary>
    /// <param name="input">Input feature vector.</param>
    /// <returns>Predicted output value.</returns>
    public double Predict(double[] input)
    {
        if (input.Length == 0)
        {
            return 0.0;
        }

        double result = 0.0;

        // Check for bias term
        if (TrainedParameters.TryGetValue("b0", out double bias))
        {
            result = bias;
        }

        for (int i = 0; i < input.Length; i++)
        {
            string weightKey = string.Concat("w", i.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (TrainedParameters.TryGetValue(weightKey, out double weight))
            {
                result += input[i] * weight;
            }
        }

        return result;
    }

    /// <summary>Runs batch inference on multiple input vectors.</summary>
    /// <param name="inputs">Array of input feature vectors.</param>
    /// <returns>Array of predicted output values, one per input.</returns>
    public double[] PredictBatch(double[][] inputs)
    {
        double[] results = new double[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            results[i] = Predict(inputs[i]);
        }
        return results;
    }

    /// <summary>Creates a copy of this model with updated trained parameters.</summary>
    /// <param name="newParameters">New trained parameter dictionary.</param>
    /// <returns>A new <see cref="AIModel"/> with the updated parameters.</returns>
    public AIModel WithUpdatedParameters(ImmutableDictionary<string, double> newParameters) =>
        new(ModelId, ModelType, TrainedAt, HyperParameters, newParameters, Metrics);

    /// <summary>Creates a copy of this model with updated hyper-parameters.</summary>
    /// <param name="newHyperParameters">New hyper-parameter dictionary.</param>
    /// <returns>A new <see cref="AIModel"/> with the updated hyper-parameters.</returns>
    public AIModel WithUpdatedHyperParameters(ImmutableDictionary<string, double> newHyperParameters) =>
        new(ModelId, ModelType, TrainedAt, newHyperParameters, TrainedParameters, Metrics);

    /// <summary>Creates a copy of this model with updated metrics.</summary>
    /// <param name="newMetrics">New metrics dictionary.</param>
    /// <returns>A new <see cref="AIModel"/> with the updated metrics.</returns>
    public AIModel WithUpdatedMetrics(ImmutableDictionary<string, double> newMetrics) =>
        new(ModelId, ModelType, TrainedAt, HyperParameters, TrainedParameters, newMetrics);

    /// <summary>Serialises the model to a human-readable string.</summary>
    /// <returns>A multi-line description of the model.</returns>
    public string Serialize()
    {
        StringBuilder sb = new();
        _ = sb.AppendLine($"ModelId: {ModelId}");
        _ = sb.AppendLine($"ModelType: {ModelType}");
        _ = sb.AppendLine($"TrainedAt: {TrainedAt:O}");
        _ = sb.AppendLine($"HyperParameters: {HyperParameters.Count}");
        foreach (KeyValuePair<string, double> kv in HyperParameters)
        {
            _ = sb.AppendLine($"  {kv.Key} = {kv.Value}");
        }
        _ = sb.AppendLine($"TrainedParameters: {TrainedParameters.Count}");
        foreach (KeyValuePair<string, double> kv in TrainedParameters)
        {
            _ = sb.AppendLine($"  {kv.Key} = {kv.Value}");
        }
        _ = sb.AppendLine($"Metrics: {Metrics.Count}");
        foreach (KeyValuePair<string, double> kv in Metrics)
        {
            _ = sb.AppendLine($"  {kv.Key} = {kv.Value}");
        }
        return sb.ToString();
    }
}
