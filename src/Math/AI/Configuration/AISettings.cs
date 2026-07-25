namespace MathVerse.Math.AI.Configuration;

using System.Collections.Immutable;

/// <summary>Persistent AI settings for the MathVerse platform.</summary>
public sealed class AISettings
{
    /// <summary>Gets the default optimizer name used when none is specified.</summary>
    public string DefaultOptimizer { get; init; } = "Adam";

    /// <summary>Gets the default maximum number of training epochs.</summary>
    public int DefaultMaxEpochs { get; init; } = 1000;

    /// <summary>Gets the default learning rate for gradient-based optimizers.</summary>
    public double DefaultLearningRate { get; init; } = 0.01;

    /// <summary>Gets the default random seed for reproducibility.</summary>
    public int DefaultRandomSeed { get; init; } = 42;

    /// <summary>Gets whether GPU acceleration is enabled.</summary>
    public bool EnableGPU { get; init; } = false;

    /// <summary>Gets the maximum number of models that can be trained concurrently.</summary>
    public int MaxConcurrentModels { get; init; } = 4;

    /// <summary>Gets the default batch size for mini-batch training.</summary>
    public int DefaultBatchSize { get; init; } = 32;

    /// <summary>Gets the default regularization lambda parameter.</summary>
    public double DefaultRegularizationLambda { get; init; } = 0.001;

    /// <summary>Gets the default gradient clipping threshold.</summary>
    public double DefaultGradientClipThreshold { get; init; } = 5.0;

    /// <summary>Gets the default convergence tolerance.</summary>
    public double DefaultConvergenceTolerance { get; init; } = 1e-6;

    /// <summary>Gets the default dropout rate for neural networks.</summary>
    public double DefaultDropoutRate { get; init; } = 0.1;

    /// <summary>Gets custom key-value settings for extensibility.</summary>
    public ImmutableDictionary<string, string> CustomSettings { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>Gets a default <see cref="AISettings"/> instance.</summary>
    public static AISettings Default => new();

    /// <summary>Creates a copy of these settings with the specified overrides.</summary>
    /// <param name="defaultOptimizer">New optimizer name, or null to keep current.</param>
    /// <param name="defaultMaxEpochs">New max epochs, or null to keep current.</param>
    /// <param name="defaultLearningRate">New learning rate, or null to keep current.</param>
    /// <param name="enableGPU">New GPU flag, or null to keep current.</param>
    /// <returns>A new <see cref="AISettings"/> instance with the specified overrides.</returns>
    public AISettings WithOverrides(
        string? defaultOptimizer = null,
        int? defaultMaxEpochs = null,
        double? defaultLearningRate = null,
        bool? enableGPU = null) =>
        new()
        {
            DefaultOptimizer = defaultOptimizer ?? DefaultOptimizer,
            DefaultMaxEpochs = defaultMaxEpochs ?? DefaultMaxEpochs,
            DefaultLearningRate = defaultLearningRate ?? DefaultLearningRate,
            DefaultRandomSeed = DefaultRandomSeed,
            EnableGPU = enableGPU ?? EnableGPU,
            MaxConcurrentModels = MaxConcurrentModels,
            DefaultBatchSize = DefaultBatchSize,
            DefaultRegularizationLambda = DefaultRegularizationLambda,
            DefaultGradientClipThreshold = DefaultGradientClipThreshold,
            DefaultConvergenceTolerance = DefaultConvergenceTolerance,
            DefaultDropoutRate = DefaultDropoutRate,
            CustomSettings = CustomSettings
        };

    /// <summary>Retrieves a custom setting value by key, or the default if not found.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>The setting value, or the default.</returns>
    public string GetCustomSetting(string key, string defaultValue = "")
    {
        if (CustomSettings.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>Retrieves a custom setting value parsed as a double.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value if not found or unparseable.</param>
    /// <returns>The parsed value, or the default.</returns>
    public double GetCustomSettingDouble(string key, double defaultValue = 0.0)
    {
        if (CustomSettings.TryGetValue(key, out var value) && double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return defaultValue;
    }
}
