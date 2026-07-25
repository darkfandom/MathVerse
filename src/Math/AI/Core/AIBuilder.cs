namespace MathVerse.Math.AI.Core;

/// <summary>Fluent builder for constructing configured AI engine instances.</summary>
public sealed class AIBuilder
{
    private AIOptions _options = new();
    private AIConfiguration _configuration = new();

    /// <summary>Sets the full options instance.</summary>
    /// <param name="options">Options to use.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithOptions(AIOptions options)
    {
        _options = options;
        return this;
    }

    /// <summary>Sets the full configuration instance.</summary>
    /// <param name="config">Configuration to use.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithConfiguration(AIConfiguration config)
    {
        _configuration = config;
        return this;
    }

    /// <summary>Sets the learning rate.</summary>
    /// <param name="learningRate">Learning rate value.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithLearningRate(double learningRate)
    {
        _options = _options.WithOverrides(learningRate: learningRate);
        return this;
    }

    /// <summary>Sets the maximum number of training epochs.</summary>
    /// <param name="maxEpochs">Maximum epoch count.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithMaxEpochs(int maxEpochs)
    {
        _options = _options.WithOverrides(maxEpochs: maxEpochs);
        return this;
    }

    /// <summary>Sets the convergence tolerance.</summary>
    /// <param name="convergenceTolerance">Tolerance below which training stops.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithConvergenceTolerance(double convergenceTolerance)
    {
        _options = _options.WithOverrides(convergenceTolerance: convergenceTolerance);
        return this;
    }

    /// <summary>Sets the random seed for reproducibility.</summary>
    /// <param name="seed">Random seed value.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithSeed(int seed)
    {
        _options = _options.WithOverrides(randomSeed: seed);
        return this;
    }

    /// <summary>Sets the default optimizer name in the configuration.</summary>
    /// <param name="optimizerName">Optimizer name (e.g. "Adam", "SGD").</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithDefaultOptimizer(string optimizerName)
    {
        _configuration = _configuration.WithOverrides(defaultOptimizer: optimizerName);
        return this;
    }

    /// <summary>Sets the default loss function name in the configuration.</summary>
    /// <param name="lossFunctionName">Loss function name (e.g. "MSE", "CrossEntropy").</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithDefaultLossFunction(string lossFunctionName)
    {
        _configuration = _configuration.WithOverrides(defaultLossFunction: lossFunctionName);
        return this;
    }

    /// <summary>Sets the default hidden-layer width in the configuration.</summary>
    /// <param name="hiddenSize">Hidden layer width.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithHiddenSize(int hiddenSize)
    {
        _configuration = _configuration.WithOverrides(defaultHiddenSize: hiddenSize);
        return this;
    }

    /// <summary>Sets the default dropout rate in the configuration.</summary>
    /// <param name="dropoutRate">Dropout rate between 0 and 1.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithDropoutRate(double dropoutRate)
    {
        _configuration = _configuration.WithOverrides(defaultDropoutRate: dropoutRate);
        return this;
    }

    /// <summary>Enables or disables GPU acceleration.</summary>
    /// <param name="enable">Whether to prefer GPU kernels.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithGPUAcceleration(bool enable)
    {
        _configuration = new AIConfiguration
        {
            Options = _configuration.Options,
            ModelDefaults = _configuration.ModelDefaults,
            DefaultOptimizer = _configuration.DefaultOptimizer,
            DefaultLossFunction = _configuration.DefaultLossFunction,
            DefaultHiddenSize = _configuration.DefaultHiddenSize,
            DefaultDropoutRate = _configuration.DefaultDropoutRate,
            EnableGPUAcceleration = enable,
        };
        return this;
    }

    /// <summary>Enables or disables diagnostic collection.</summary>
    /// <param name="enable">Whether to collect diagnostics.</param>
    /// <returns>This builder for chaining.</returns>
    public AIBuilder WithDiagnostics(bool enable)
    {
        _options = new AIOptions
        {
            MaxEpochs = _options.MaxEpochs,
            LearningRate = _options.LearningRate,
            ConvergenceTolerance = _options.ConvergenceTolerance,
            RandomSeed = _options.RandomSeed,
            MaxConcurrency = _options.MaxConcurrency,
            EnableCaching = _options.EnableCaching,
            EnableDiagnostics = enable,
            MaxCacheSize = _options.MaxCacheSize,
            Metadata = _options.Metadata,
        };
        return this;
    }

    /// <summary>Builds and returns a new <see cref="AIEngine"/> with the configured options.</summary>
    /// <returns>A configured <see cref="AIEngine"/>.</returns>
    public AIEngine Build() => new(_options, _configuration);
}
