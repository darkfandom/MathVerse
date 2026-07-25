namespace MathVerse.Math.AI.Core;

using System.Collections.Concurrent;

/// <summary>Registry of AI model types, optimizers, and algorithms available for use.</summary>
public sealed class AIRegistry
{
    private readonly ConcurrentDictionary<string, Func<AIModel>> _modelFactories = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Func<double[], AIConfiguration, AIResult>> _optimizerFactories = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers a model factory under the specified name.</summary>
    /// <param name="name">Case-insensitive name for the model type.</param>
    /// <param name="factory">Factory delegate that creates a new <see cref="AIModel"/>.</param>
    public void RegisterModel(string name, Func<AIModel> factory) => _modelFactories[name] = factory;

    /// <summary>Registers an optimizer factory under the specified name.</summary>
    /// <param name="name">Case-insensitive name for the optimizer.</param>
    /// <param name="factory">Factory delegate that runs the optimizer and returns a result.</param>
    public void RegisterOptimizer(string name, Func<double[], AIConfiguration, AIResult> factory) => _optimizerFactories[name] = factory;

    /// <summary>Creates a new model by name.</summary>
    /// <param name="name">Registered model name.</param>
    /// <returns>A new <see cref="AIModel"/>, or <c>null</c> if the name is not registered.</returns>
    public AIModel? CreateModel(string name)
    {
        return _modelFactories.TryGetValue(name, out Func<AIModel>? factory) ? factory() : null;
    }

    /// <summary>Runs an optimizer by name on the given initial parameters.</summary>
    /// <param name="name">Registered optimizer name.</param>
    /// <param name="initial">Initial parameter vector.</param>
    /// <param name="configuration">AI configuration.</param>
    /// <returns>The optimization result, or a failure if the name is not registered.</returns>
    public AIResult RunOptimizer(string name, double[] initial, AIConfiguration configuration)
    {
        if (_optimizerFactories.TryGetValue(name, out Func<double[], AIConfiguration, AIResult>? factory))
        {
            return factory(initial, configuration);
        }

        return AIResult.Fail($"Optimizer '{name}' is not registered.");
    }

    /// <summary>Returns <c>true</c> when a model factory with the given name exists.</summary>
    /// <param name="name">Model name to check.</param>
    /// <returns><c>true</c> if registered.</returns>
    public bool HasModel(string name) => _modelFactories.ContainsKey(name);

    /// <summary>Returns <c>true</c> when an optimizer with the given name exists.</summary>
    /// <param name="name">Optimizer name to check.</param>
    /// <returns><c>true</c> if registered.</returns>
    public bool HasOptimizer(string name) => _optimizerFactories.ContainsKey(name);

    /// <summary>Gets the names of all registered model factories.</summary>
    public IReadOnlyCollection<string> ModelNames => (IReadOnlyCollection<string>)_modelFactories.Keys;

    /// <summary>Gets the names of all registered optimizer factories.</summary>
    public IReadOnlyCollection<string> OptimizerNames => (IReadOnlyCollection<string>)_optimizerFactories.Keys;

    /// <summary>Removes a model factory by name.</summary>
    /// <param name="name">Model name.</param>
    /// <returns><c>true</c> if the factory was present and removed.</returns>
    public bool UnregisterModel(string name) => _modelFactories.TryRemove(name, out _);

    /// <summary>Removes an optimizer factory by name.</summary>
    /// <param name="name">Optimizer name.</param>
    /// <returns><c>true</c> if the factory was present and removed.</returns>
    public bool UnregisterOptimizer(string name) => _optimizerFactories.TryRemove(name, out _);

    /// <summary>Creates a new registry pre-loaded with all built-in models and optimizers.</summary>
    /// <returns>A populated <see cref="AIRegistry"/>.</returns>
    public static AIRegistry CreateDefault()
    {
        AIRegistry registry = new();

        // Built-in model factories
        registry.RegisterModel("LinearRegression", () => new AIModel(Guid.NewGuid().ToString("N"), "LinearRegression"));
        registry.RegisterModel("PolynomialRegression", () => new AIModel(Guid.NewGuid().ToString("N"), "PolynomialRegression"));
        registry.RegisterModel("RidgeRegression", () => new AIModel(Guid.NewGuid().ToString("N"), "RidgeRegression"));
        registry.RegisterModel("LassoRegression", () => new AIModel(Guid.NewGuid().ToString("N"), "LassoRegression"));
        registry.RegisterModel("ElasticNet", () => new AIModel(Guid.NewGuid().ToString("N"), "ElasticNet"));
        registry.RegisterModel("LogisticRegression", () => new AIModel(Guid.NewGuid().ToString("N"), "LogisticRegression"));
        registry.RegisterModel("DecisionTree", () => new AIModel(Guid.NewGuid().ToString("N"), "DecisionTree"));
        registry.RegisterModel("RandomForest", () => new AIModel(Guid.NewGuid().ToString("N"), "RandomForest"));
        registry.RegisterModel("KNN", () => new AIModel(Guid.NewGuid().ToString("N"), "KNN"));
        registry.RegisterModel("SVM", () => new AIModel(Guid.NewGuid().ToString("N"), "SVM"));
        registry.RegisterModel("MLP", () => new AIModel(Guid.NewGuid().ToString("N"), "MLP"));

        // Built-in optimizer factories
        registry.RegisterOptimizer("SGD", (initial, config) => RunGradientDescent(initial, config, 0.01, 100));
        registry.RegisterOptimizer("Adam", (initial, config) => RunAdam(initial, config));
        registry.RegisterOptimizer("RMSProp", (initial, config) => RunRMSProp(initial, config));

        return registry;
    }

    private static AIResult RunGradientDescent(double[] initial, AIConfiguration config, double learningRate, int iterations)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        double[] parameters = (double[])initial.Clone();
        double lr = config.Options.LearningRate > 0 ? config.Options.LearningRate : learningRate;

        for (int iter = 0; iter < iterations; iter++)
        {
            // Numerical gradient estimation: f(x+h) - f(x-h) / 2h
            for (int i = 0; i < parameters.Length; i++)
            {
                double h = 1e-7;
                double original = parameters[i];

                // Simple quadratic objective: sum of squares
                parameters[i] = original + h;
                double fxh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fxh += parameters[j] * parameters[j];

                parameters[i] = original - h;
                double fmh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fmh += parameters[j] * parameters[j];

                double gradient = (fxh - fmh) / (2.0 * h);
                parameters[i] = original - lr * gradient;
            }
        }

        sw.Stop();

        double finalLoss = 0.0;
        for (int i = 0; i < parameters.Length; i++) finalLoss += parameters[i] * parameters[i];

        return AIResult.Ok(parameters, finalLoss, iterations, sw.Elapsed, message: "SGD completed");
    }

    private static AIResult RunAdam(double[] initial, AIConfiguration config)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        int maxEpochs = config.Options.MaxEpochs > 0 ? config.Options.MaxEpochs : 100;
        double lr = config.Options.LearningRate;
        double beta1 = 0.9;
        double beta2 = 0.999;
        double epsilon = 1e-8;

        double[] parameters = (double[])initial.Clone();
        double[] m = new double[parameters.Length];
        double[] v = new double[parameters.Length];

        for (int t = 1; t <= maxEpochs; t++)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                double h = 1e-7;
                double original = parameters[i];

                parameters[i] = original + h;
                double fxh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fxh += parameters[j] * parameters[j];

                parameters[i] = original - h;
                double fmh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fmh += parameters[j] * parameters[j];

                double grad = (fxh - fmh) / (2.0 * h);
                parameters[i] = original;

                m[i] = beta1 * m[i] + (1.0 - beta1) * grad;
                v[i] = beta2 * v[i] + (1.0 - beta2) * grad * grad;

                double mHat = m[i] / (1.0 - System.Math.Pow(beta1, t));
                double vHat = v[i] / (1.0 - System.Math.Pow(beta2, t));

                parameters[i] = original - lr * mHat / (System.Math.Sqrt(vHat) + epsilon);
            }
        }

        sw.Stop();

        double finalLoss = 0.0;
        for (int i = 0; i < parameters.Length; i++) finalLoss += parameters[i] * parameters[i];

        return AIResult.Ok(parameters, finalLoss, maxEpochs, sw.Elapsed, message: "Adam completed");
    }

    private static AIResult RunRMSProp(double[] initial, AIConfiguration config)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        int maxEpochs = config.Options.MaxEpochs > 0 ? config.Options.MaxEpochs : 100;
        double lr = config.Options.LearningRate;
        double decayRate = 0.9;
        double epsilon = 1e-8;

        double[] parameters = (double[])initial.Clone();
        double[] cache = new double[parameters.Length];

        for (int t = 0; t < maxEpochs; t++)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                double h = 1e-7;
                double original = parameters[i];

                parameters[i] = original + h;
                double fxh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fxh += parameters[j] * parameters[j];

                parameters[i] = original - h;
                double fmh = 0.0;
                for (int j = 0; j < parameters.Length; j++) fmh += parameters[j] * parameters[j];

                double grad = (fxh - fmh) / (2.0 * h);
                parameters[i] = original;

                cache[i] = decayRate * cache[i] + (1.0 - decayRate) * grad * grad;
                parameters[i] = original - lr * grad / (System.Math.Sqrt(cache[i]) + epsilon);
            }
        }

        sw.Stop();

        double finalLoss = 0.0;
        for (int i = 0; i < parameters.Length; i++) finalLoss += parameters[i] * parameters[i];

        return AIResult.Ok(parameters, finalLoss, maxEpochs, sw.Elapsed, message: "RMSProp completed");
    }
}
