namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

/// <summary>Main entry point for the MathVerse AI framework. Provides a unified facade for
/// training, prediction, optimization, clustering, dimensionality reduction, symbolic AI,
/// reinforcement learning, graph intelligence, and intelligent integration.</summary>
public sealed class AIEngine
{
    private readonly AIOptions _options;
    private readonly AIConfiguration _configuration;
    private readonly AIContext _context;
    private readonly AIServices _services;
    private readonly AIRegistry _registry;
    private readonly Random _random;

    /// <summary>Initialises a new AI engine.</summary>
    /// <param name="options">Optional engine options; uses <see cref="AIOptions.Default"/> when <c>null</c>.</param>
    /// <param name="configuration">Optional configuration; uses <see cref="AIConfiguration.Default"/> when <c>null</c>.</param>
    public AIEngine(AIOptions? options = null, AIConfiguration? configuration = null)
    {
        _options = options ?? AIOptions.Default;
        _configuration = configuration ?? AIConfiguration.Default;
        _context = new AIContext(_configuration);
        _services = new AIServices(_configuration);
        _registry = AIRegistry.CreateDefault();
        _random = new Random(_options.RandomSeed);
    }

    /// <summary>Engine options.</summary>
    public AIOptions Options => _options;

    /// <summary>Full configuration.</summary>
    public AIConfiguration Configuration => _configuration;

    /// <summary>Execution context carrying session state and cache.</summary>
    public AIContext Context => _context;

    /// <summary>Model and algorithm registry.</summary>
    public AIRegistry Registry => _registry;

    /// <summary>Service locator for subsystem access.</summary>
    public AIServices Services => _services;

    // ───────────────────────────────────────────────────────────────────────
    //  ML - Regression
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Trains an ordinary-least-squares linear regression model.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Target vector (N).</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainLinearRegression(double[][] features, double[] labels)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        if (n == 0 || features[0].Length == 0)
        {
            return AIResult.Fail("Empty feature matrix.");
        }

        int d = features[0].Length;

        // Normal equations: w = (X^T X)^{-1} X^T y
        // Build X^T X (d × d)
        double[,] xtx = new double[d, d];
        double[] xty = new double[d];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < d; j++)
            {
                xty[j] += features[i][j] * labels[i];
                for (int k = 0; k < d; k++)
                {
                    xtx[j, k] += features[i][j] * features[i][k];
                }
            }
        }

        // Solve via Gaussian elimination with partial pivoting
        double[] weights = SolveLinearSystem(xtx, xty);

        sw.Stop();

        // Compute MSE
        double mse = 0.0;
        for (int i = 0; i < n; i++)
        {
            double pred = 0.0;
            for (int j = 0; j < d; j++) pred += features[i][j] * weights[j];
            double diff = pred - labels[i];
            mse += diff * diff;
        }
        mse /= n;

        // Build parameter dictionary
        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++)
        {
            builder[$"w{j}"] = weights[j];
        }

        AIModel model = new(Guid.NewGuid().ToString("N"), "LinearRegression")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("MSE", mse)
                .SetItem("RMSE", System.Math.Sqrt(mse)),
        };

        _context.SetMetric("LinearRegression_MSE", mse);
        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(
            weights,
            mse,
            1,
            sw.Elapsed,
            diagnostics: [$"Trained linear regression on {n} samples, {d} features"],
            message: $"Linear regression trained. MSE={mse:E6}");
    }

    /// <summary>Trains a polynomial regression model of the given degree.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Target vector (N).</param>
    /// <param name="degree">Polynomial degree.</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainPolynomialRegression(double[][] features, double[] labels, int degree)
    {
        if (degree < 1) return AIResult.Fail("Degree must be >= 1.");

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        int n = features.Length;
        int origD = features[0].Length;

        // Expand features to polynomial basis: for each original feature x_j, emit x_j^1, x_j^2, …, x_j^degree
        int expandedD = origD * degree;
        double[][] expanded = new double[n][];
        for (int i = 0; i < n; i++)
        {
            expanded[i] = new double[expandedD];
            for (int j = 0; j < origD; j++)
            {
                double val = features[i][j];
                double power = val;
                for (int deg = 1; deg <= degree; deg++)
                {
                    expanded[i][j * degree + (deg - 1)] = power;
                    power *= val;
                }
            }
        }

        // Delegate to linear regression on expanded features
        AIResult result = TrainLinearRegression(expanded, labels);
        sw.Stop();

        return new AIResult
        {
            Success = result.Success,
            Message = $"Polynomial regression (degree={degree}): {result.Message}",
            OutputValues = result.OutputValues,
            LossValue = result.LossValue,
            EpochsExecuted = result.EpochsExecuted,
            ElapsedTime = sw.Elapsed,
            Metrics = result.Metrics,
            Diagnostics = result.Diagnostics,
        };
    }

    /// <summary>Trains a ridge regression model with L2 regularisation.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Target vector (N).</param>
    /// <param name="alpha">Regularisation strength.</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainRidgeRegression(double[][] features, double[] labels, double alpha)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;

        double[,] xtx = new double[d, d];
        double[] xty = new double[d];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < d; j++)
            {
                xty[j] += features[i][j] * labels[i];
                for (int k = 0; k < d; k++)
                {
                    xtx[j, k] += features[i][j] * features[i][k];
                }
            }
        }

        // Add alpha * I
        for (int j = 0; j < d; j++)
        {
            xtx[j, j] += alpha;
        }

        double[] weights = SolveLinearSystem(xtx, xty);

        sw.Stop();

        double mse = 0.0;
        for (int i = 0; i < n; i++)
        {
            double pred = 0.0;
            for (int j = 0; j < d; j++) pred += features[i][j] * weights[j];
            double diff = pred - labels[i];
            mse += diff * diff;
        }
        mse /= n;

        // Add L2 penalty to loss
        double l2Penalty = 0.0;
        for (int j = 0; j < d; j++) l2Penalty += weights[j] * weights[j];
        double totalLoss = mse + alpha * l2Penalty;

        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++) builder[$"w{j}"] = weights[j];

        AIModel model = new(Guid.NewGuid().ToString("N"), "RidgeRegression")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("MSE", mse)
                .SetItem("L2Penalty", l2Penalty),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(weights, totalLoss, 1, sw.Elapsed,
            diagnostics: [$"Ridge regression (alpha={alpha}): MSE={mse:E6}, L2={l2Penalty:E6}"],
            message: $"Ridge regression trained. Total loss={totalLoss:E6}");
    }

    /// <summary>Trains a Lasso regression model with L1 regularisation via coordinate descent.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Target vector (N).</param>
    /// <param name="alpha">Regularisation strength.</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainLassoRegression(double[][] features, double[] labels, double alpha)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;

        double[] weights = new double[d];
        double[] residuals = new double[n];

        // Compute initial residuals
        for (int i = 0; i < n; i++)
        {
            residuals[i] = labels[i];
        }

        // Precompute column norms
        double[] colNorms = new double[d];
        for (int j = 0; j < d; j++)
        {
            double norm = 0.0;
            for (int i = 0; i < n; i++) norm += features[i][j] * features[i][j];
            colNorms[j] = norm;
        }

        int maxIter = 1000;
        double tolerance = 1e-8;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double maxChange = 0.0;

            for (int j = 0; j < d; j++)
            {
                // Compute partial residual without feature j
                double dotSum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    dotSum += features[i][j] * (residuals[i] + features[i][j] * weights[j]);
                }

                double newWeight = SoftThreshold(dotSum / System.Math.Max(colNorms[j], 1e-15), alpha / System.Math.Max(colNorms[j], 1e-15));
                double change = System.Math.Abs(newWeight - weights[j]);

                // Update residuals
                for (int i = 0; i < n; i++)
                {
                    residuals[i] += features[i][j] * (weights[j] - newWeight);
                }

                weights[j] = newWeight;
                if (change > maxChange) maxChange = change;
            }

            if (maxChange < tolerance) break;
        }

        sw.Stop();

        double mse = 0.0;
        for (int i = 0; i < n; i++)
        {
            double pred = 0.0;
            for (int j = 0; j < d; j++) pred += features[i][j] * weights[j];
            double diff = pred - labels[i];
            mse += diff * diff;
        }
        mse /= n;

        double l1Penalty = 0.0;
        for (int j = 0; j < d; j++) l1Penalty += System.Math.Abs(weights[j]);

        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++) builder[$"w{j}"] = weights[j];

        AIModel model = new(Guid.NewGuid().ToString("N"), "LassoRegression")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("MSE", mse)
                .SetItem("L1Penalty", l1Penalty),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(weights, mse + alpha * l1Penalty, maxIter, sw.Elapsed,
            diagnostics: [$"Lasso regression (alpha={alpha}): MSE={mse:E6}, L1={l1Penalty:E6}"],
            message: $"Lasso regression trained. Total loss={mse + alpha * l1Penalty:E6}");
    }

    /// <summary>Trains an Elastic Net regression model combining L1 and L2 regularisation.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Target vector (N).</param>
    /// <param name="alpha">Overall regularisation strength.</param>
    /// <param name="l1Ratio">Mixing ratio: 1.0 = pure Lasso, 0.0 = pure Ridge.</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainElasticNet(double[][] features, double[] labels, double alpha, double l1Ratio)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;

        double[] weights = new double[d];
        double[] residuals = new double[n];

        for (int i = 0; i < n; i++) residuals[i] = labels[i];

        double[] colNorms = new double[d];
        for (int j = 0; j < d; j++)
        {
            double norm = 0.0;
            for (int i = 0; i < n; i++) norm += features[i][j] * features[i][j];
            colNorms[j] = norm;
        }

        double l1Alpha = alpha * l1Ratio;
        double l2Alpha = alpha * (1.0 - l1Ratio);

        int maxIter = 1000;
        double tolerance = 1e-8;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double maxChange = 0.0;

            for (int j = 0; j < d; j++)
            {
                double dotSum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    dotSum += features[i][j] * (residuals[i] + features[i][j] * weights[j]);
                }

                double normJ = System.Math.Max(colNorms[j], 1e-15);
                double newWeight = SoftThreshold(dotSum / normJ, l1Alpha / normJ) / (1.0 + l2Alpha / normJ);
                double change = System.Math.Abs(newWeight - weights[j]);

                for (int i = 0; i < n; i++)
                {
                    residuals[i] += features[i][j] * (weights[j] - newWeight);
                }

                weights[j] = newWeight;
                if (change > maxChange) maxChange = change;
            }

            if (maxChange < tolerance) break;
        }

        sw.Stop();

        double mse = 0.0;
        for (int i = 0; i < n; i++)
        {
            double pred = 0.0;
            for (int j = 0; j < d; j++) pred += features[i][j] * weights[j];
            double diff = pred - labels[i];
            mse += diff * diff;
        }
        mse /= n;

        double l1Penalty = 0.0;
        double l2Penalty = 0.0;
        for (int j = 0; j < d; j++)
        {
            l1Penalty += System.Math.Abs(weights[j]);
            l2Penalty += weights[j] * weights[j];
        }

        double totalLoss = mse + l1Alpha * l1Penalty + l2Alpha * l2Penalty;

        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++) builder[$"w{j}"] = weights[j];

        AIModel model = new(Guid.NewGuid().ToString("N"), "ElasticNet")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("MSE", mse)
                .SetItem("L1Penalty", l1Penalty)
                .SetItem("L2Penalty", l2Penalty),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(weights, totalLoss, maxIter, sw.Elapsed,
            diagnostics: [$"ElasticNet (alpha={alpha}, l1Ratio={l1Ratio}): MSE={mse:E6}"],
            message: $"ElasticNet trained. Total loss={totalLoss:E6}");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  ML - Classification
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Trains a binary logistic regression model via gradient descent.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Binary labels (0 or 1) for each sample.</param>
    /// <param name="epochs">Number of training epochs.</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainLogisticRegression(double[][] features, int[] labels, int epochs)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;
        double lr = _options.LearningRate;

        double[] weights = new double[d];

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int i = 0; i < n; i++)
            {
                double z = 0.0;
                for (int j = 0; j < d; j++) z += weights[j] * features[i][j];
                double prediction = Sigmoid(z);
                double error = prediction - labels[i];

                for (int j = 0; j < d; j++)
                {
                    weights[j] -= lr * error * features[i][j];
                }
            }
        }

        sw.Stop();

        // Compute accuracy and cross-entropy loss
        int correct = 0;
        double crossEntropy = 0.0;
        for (int i = 0; i < n; i++)
        {
            double z = 0.0;
            for (int j = 0; j < d; j++) z += weights[j] * features[i][j];
            double p = Sigmoid(z);
            if ((p >= 0.5 ? 1 : 0) == labels[i]) correct++;

            double clamped = System.Math.Clamp(p, 1e-15, 1.0 - 1e-15);
            crossEntropy -= labels[i] * System.Math.Log(clamped) + (1.0 - labels[i]) * System.Math.Log(1.0 - clamped);
        }
        crossEntropy /= n;

        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++) builder[$"w{j}"] = weights[j];

        AIModel model = new(Guid.NewGuid().ToString("N"), "LogisticRegression")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("Accuracy", (double)correct / n)
                .SetItem("CrossEntropy", crossEntropy),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(weights, crossEntropy, epochs, sw.Elapsed,
            diagnostics: [$"Logistic regression: accuracy={(double)correct / n:P2}, cross-entropy={crossEntropy:E6}"],
            message: $"Logistic regression trained. Accuracy={(double)correct / n:P2}");
    }

    /// <summary>Trains a decision tree classifier using information gain (entropy).</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Class labels for each sample.</param>
    /// <returns>Result containing a model that stores tree structure in parameters.</returns>
    public AIResult TrainDecisionTree(double[][] features, int[] labels)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;
        int maxDepth = 10;

        // Simple decision tree: for each level, find best feature/split
        Dictionary<string, double> treeParams = new();
        BuildDecisionTree(features, labels, treeParams, 0, maxDepth, 0);

        sw.Stop();

        ImmutableDictionary<string, double> treeImmutable = treeParams.ToImmutableDictionary();
        AIModel model = new(Guid.NewGuid().ToString("N"), "DecisionTree")
        {
            TrainedParameters = treeImmutable,
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("MaxDepth", System.Math.Min(maxDepth, (int)(treeImmutable.Count / 3.0 + 1))),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(Array.Empty<double>(), 0.0, 1, sw.Elapsed,
            diagnostics: [$"Decision tree trained with {treeParams.Count / 3} internal nodes"],
            message: "Decision tree trained.");
    }

    /// <summary>Trains a random forest classifier with the specified number of trees.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Class labels for each sample.</param>
    /// <param name="treeCount">Number of trees in the ensemble.</param>
    /// <returns>Result containing the ensemble model.</returns>
    public AIResult TrainRandomForest(double[][] features, int[] labels, int treeCount)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;
        Dictionary<string, double> ensembleParams = new();

        for (int t = 0; t < treeCount; t++)
        {
            // Bootstrap sample
            double[][] sampleFeatures = new double[n][];
            int[] sampleLabels = new int[n];
            for (int i = 0; i < n; i++)
            {
                int idx = _random.Next(n);
                sampleFeatures[i] = features[idx];
                sampleLabels[i] = labels[idx];
            }

            Dictionary<string, double> treeParams = new();
            BuildDecisionTree(sampleFeatures, sampleLabels, treeParams, 0, 6, t);
            foreach (KeyValuePair<string, double> kv in treeParams)
            {
                ensembleParams[$"tree{t}_{kv.Key}"] = kv.Value;
            }
        }

        sw.Stop();

        AIModel model = new(Guid.NewGuid().ToString("N"), "RandomForest")
        {
            TrainedParameters = ensembleParams.ToImmutableDictionary(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("TreeCount", treeCount),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(Array.Empty<double>(), 0.0, treeCount, sw.Elapsed,
            diagnostics: [$"Random forest trained with {treeCount} trees"],
            message: $"Random forest trained. {treeCount} trees.");
    }

    /// <summary>Trains a K-Nearest Neighbours classifier.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Class labels for each sample.</param>
    /// <param name="k">Number of neighbours.</param>
    /// <returns>Result containing a model that stores training data for lazy prediction.</returns>
    public AIResult TrainKNN(double[][] features, int[] labels, int k)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;

        // Store training data in parameters (flattened)
        Dictionary<string, double> store = new();
        store["k"] = k;
        store["n"] = n;
        store["d"] = d;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < d; j++)
            {
                store[$"x_{i}_{j}"] = features[i][j];
            }
            store[$"y_{i}"] = labels[i];
        }

        sw.Stop();

        AIModel model = new(Guid.NewGuid().ToString("N"), "KNN")
        {
            TrainedParameters = store.ToImmutableDictionary(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("K", k)
                .SetItem("TrainingSamples", n),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(Array.Empty<double>(), 0.0, 1, sw.Elapsed,
            diagnostics: [$"KNN trained with k={k} on {n} samples"],
            message: $"KNN model stored. k={k}, samples={n}.");
    }

    /// <summary>Trains a linear SVM classifier using the perceptron/hinge-loss approach.</summary>
    /// <param name="features">Feature matrix (N × D).</param>
    /// <param name="labels">Binary labels (0 or 1, mapped to -1/+1 internally).</param>
    /// <returns>Result containing trained weight vector.</returns>
    public AIResult TrainSVM(double[][] features, int[] labels)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = features.Length;
        int d = features[0].Length;
        double lr = _options.LearningRate;
        int epochs = _options.MaxEpochs > 0 ? System.Math.Min(_options.MaxEpochs, 1000) : 100;
        double lambda = 1.0 / n;

        double[] weights = new double[d];

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int i = 0; i < n; i++)
            {
                // Map labels: 0 -> -1, 1 -> +1
                int y = labels[i] == 1 ? 1 : -1;

                double z = 0.0;
                for (int j = 0; j < d; j++) z += weights[j] * features[i][j];

                if (y * z < 1.0)
                {
                    // Sub-gradient of hinge loss + L2 reg
                    for (int j = 0; j < d; j++)
                    {
                        weights[j] = (1.0 - lr * lambda) * weights[j] + lr * y * features[i][j];
                    }
                }
                else
                {
                    for (int j = 0; j < d; j++)
                    {
                        weights[j] = (1.0 - lr * lambda) * weights[j];
                    }
                }
            }
        }

        sw.Stop();

        // Compute training accuracy
        int correct = 0;
        for (int i = 0; i < n; i++)
        {
            double z = 0.0;
            for (int j = 0; j < d; j++) z += weights[j] * features[i][j];
            int predicted = z >= 0 ? 1 : 0;
            if (predicted == labels[i]) correct++;
        }

        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();
        for (int j = 0; j < d; j++) builder[$"w{j}"] = weights[j];

        AIModel model = new(Guid.NewGuid().ToString("N"), "SVM")
        {
            TrainedParameters = builder.ToImmutable(),
            TrainedAt = DateTime.UtcNow,
            Metrics = ImmutableDictionary<string, double>.Empty
                .SetItem("Accuracy", (double)correct / n),
        };

        _context.CacheSet($"model_{model.ModelId}", model);

        return AIResult.Ok(weights, 0.0, epochs, sw.Elapsed,
            diagnostics: [$"SVM trained: accuracy={(double)correct / n:P2}"],
            message: $"SVM trained. Accuracy={(double)correct / n:P2}");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  ML - Clustering
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Partitions data into k clusters using the K-Means algorithm.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <param name="k">Number of clusters.</param>
    /// <returns>Result containing cluster assignments and centroids.</returns>
    public AIResult Cluster(double[][] data, int k)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = data.Length;
        int d = data[0].Length;

        if (k <= 0 || k > n) return AIResult.Fail($"k must be between 1 and {n}.");

        // Initialise centroids using k-means++
        double[][] centroids = KMeansPPInit(data, k, _random);
        int[] assignments = new int[n];

        int maxIter = 300;
        for (int iter = 0; iter < maxIter; iter++)
        {
            bool changed = false;

            // Assignment step
            for (int i = 0; i < n; i++)
            {
                int bestCluster = 0;
                double bestDist = double.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    double dist = EuclideanDistanceSquared(data[i], centroids[c]);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestCluster = c;
                    }
                }
                if (assignments[i] != bestCluster)
                {
                    assignments[i] = bestCluster;
                    changed = true;
                }
            }

            if (!changed) break;

            // Update step
            int[] counts = new int[k];
            double[][] newCentroids = new double[k][];
            for (int c = 0; c < k; c++)
            {
                newCentroids[c] = new double[d];
            }

            for (int i = 0; i < n; i++)
            {
                int c = assignments[i];
                counts[c]++;
                for (int j = 0; j < d; j++)
                {
                    newCentroids[c][j] += data[i][j];
                }
            }

            for (int c = 0; c < k; c++)
            {
                if (counts[c] > 0)
                {
                    for (int j = 0; j < d; j++)
                    {
                        newCentroids[c][j] /= counts[c];
                    }
                }
                else
                {
                    newCentroids[c] = centroids[c];
                }
            }

            centroids = newCentroids;
        }

        sw.Stop();

        // Compute inertia (sum of squared distances to nearest centroid)
        double inertia = 0.0;
        for (int i = 0; i < n; i++)
        {
            inertia += EuclideanDistanceSquared(data[i], centroids[assignments[i]]);
        }

        // Flatten results
        double[] output = new double[n + k * d];
        for (int i = 0; i < n; i++) output[i] = assignments[i];
        for (int c = 0; c < k; c++)
        {
            for (int j = 0; j < d; j++)
            {
                output[n + c * d + j] = centroids[c][j];
            }
        }

        ImmutableDictionary<string, double> metrics = ImmutableDictionary<string, double>.Empty
            .SetItem("Inertia", inertia)
            .SetItem("K", k);

        return AIResult.Ok(output, inertia, 0, sw.Elapsed, metrics: metrics,
            diagnostics: [$"K-Means converged with k={k}, inertia={inertia:E2}"],
            message: $"K-Means completed. k={k}, inertia={inertia:E2}");
    }

    /// <summary>Clusters data using the DBSCAN algorithm.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <param name="epsilon">Maximum distance between two points in the same neighbourhood.</param>
    /// <param name="minPoints">Minimum number of points to form a core point.</param>
    /// <returns>Result containing cluster labels (-1 for noise).</returns>
    public AIResult ClusterDBSCAN(double[][] data, double epsilon, int minPoints)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = data.Length;
        int[] labels = new int[n];
        for (int i = 0; i < n; i++) labels[i] = -1;

        int clusterId = 0;
        bool[] visited = new bool[n];

        for (int i = 0; i < n; i++)
        {
            if (visited[i]) continue;
            visited[i] = true;

            List<int> neighbours = GetNeighbours(data, i, epsilon);

            if (neighbours.Count < minPoints)
            {
                labels[i] = -1; // Noise
            }
            else
            {
                ExpandCluster(data, labels, visited, i, neighbours, clusterId, epsilon, minPoints);
                clusterId++;
            }
        }

        sw.Stop();

        double[] output = new double[n];
        for (int i = 0; i < n; i++) output[i] = labels[i];

        return AIResult.Ok(output, 0.0, 0, sw.Elapsed,
            metrics: ImmutableDictionary<string, double>.Empty
                .SetItem("Clusters", clusterId)
                .SetItem("Epsilon", epsilon),
            diagnostics: [$"DBSCAN found {clusterId} clusters, epsilon={epsilon}, minPoints={minPoints}"],
            message: $"DBSCAN completed. {clusterId} clusters found.");
    }

    /// <summary>Performs agglomerative hierarchical clustering.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <param name="numClusters">Desired number of output clusters.</param>
    /// <returns>Result containing cluster assignments.</returns>
    public AIResult ClusterHierarchical(double[][] data, int numClusters)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = data.Length;
        int[] labels = new int[n];
        for (int i = 0; i < n; i++) labels[i] = i;

        // Distance matrix
        double[,] dist = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double d = System.Math.Sqrt(EuclideanDistanceSquared(data[i], data[j]));
                dist[i, j] = d;
                dist[j, i] = d;
            }
        }

        int activeCount = n;

        while (activeCount > numClusters)
        {
            // Find closest pair of clusters
            double minDist = double.MaxValue;
            int minI = -1, minJ = -1;

            for (int i = 0; i < n; i++)
            {
                if (labels[i] < 0) continue;
                for (int j = i + 1; j < n; j++)
                {
                    if (labels[j] < 0) continue;
                    if (labels[i] == labels[j]) continue;
                    if (dist[i, j] < minDist)
                    {
                        minDist = dist[i, j];
                        minI = i;
                        minJ = j;
                    }
                }
            }

            if (minI < 0) break;

            // Merge cluster of minJ into cluster of minI
            int targetLabel = labels[minI];
            int sourceLabel = labels[minJ];
            for (int i = 0; i < n; i++)
            {
                if (labels[i] == sourceLabel)
                {
                    labels[i] = targetLabel;
                }
            }

            activeCount--;
        }

        sw.Stop();

        // Relabel to 0..k-1
        Dictionary<int, int> remap = new();
        int nextLabel = 0;
        double[] output = new double[n];
        for (int i = 0; i < n; i++)
        {
            if (!remap.TryGetValue(labels[i], out int mapped))
            {
                mapped = nextLabel++;
                remap[labels[i]] = mapped;
            }
            output[i] = mapped;
        }

        return AIResult.Ok(output, 0.0, 0, sw.Elapsed,
            metrics: ImmutableDictionary<string, double>.Empty.SetItem("Clusters", nextLabel),
            diagnostics: [$"Hierarchical clustering produced {nextLabel} clusters"],
            message: $"Hierarchical clustering completed. {nextLabel} clusters.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  ML - Dimensionality Reduction
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Reduces dimensionality using Principal Component Analysis.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <param name="components">Number of principal components to retain.</param>
    /// <returns>Result containing the projected data matrix (flattened).</returns>
    public AIResult ReducePCA(double[][] data, int components)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = data.Length;
        int d = data[0].Length;

        if (components > d) components = d;

        // Centre data
        double[] mean = new double[d];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < d; j++)
                mean[j] += data[i][j];
        for (int j = 0; j < d; j++) mean[j] /= n;

        double[,] centred = new double[n, d];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < d; j++)
                centred[i, j] = data[i][j] - mean[j];

        // Covariance matrix (d × d)
        double[,] cov = new double[d, d];
        for (int i = 0; i < d; i++)
        {
            for (int j = i; j < d; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++) sum += centred[k, i] * centred[k, j];
                cov[i, j] = sum / n;
                cov[j, i] = cov[i, j];
            }
        }

        // Power iteration to find top eigenvectors
        double[] eigenvalues = new double[components];
        double[,] eigenvectors = new double[d, components];

        for (int comp = 0; comp < components; comp++)
        {
            double[] v = new double[d];
            for (int j = 0; j < d; j++) v[j] = _random.NextDouble() - 0.5;

            double eigenvalue = 0.0;

            for (int iter = 0; iter < 200; iter++)
            {
                // Matrix-vector multiply
                double[]Av = new double[d];
                for (int i = 0; i < d; i++)
                {
                    for (int j = 0; j < d; j++)
                    {
                        Av[i] += cov[i, j] * v[j];
                    }
                }

                // Orthogonalise against previous eigenvectors
                for (int prev = 0; prev < comp; prev++)
                {
                    double dot = 0.0;
                    for (int i = 0; i < d; i++) dot += Av[i] * eigenvectors[i, prev];
                    for (int i = 0; i < d; i++) Av[i] -= dot * eigenvectors[i, prev];
                }

                // Normalise
                double norm = 0.0;
                for (int i = 0; i < d; i++) norm += Av[i] * Av[i];
                norm = System.Math.Sqrt(norm);

                if (norm < 1e-15) break;
                for (int i = 0; i < d; i++) v[i] = Av[i] / norm;

                eigenvalue = 0.0;
                for (int i = 0; i < d; i++)
                {
                    double Avi = 0.0;
                    for (int j = 0; j < d; j++) Avi += cov[i, j] * v[j];
                    eigenvalue += v[i] * Avi;
                }
            }

            eigenvalues[comp] = eigenvalue;
            for (int i = 0; i < d; i++) eigenvectors[i, comp] = v[i];
        }

        // Project data
        double[] output = new double[n * components];
        for (int i = 0; i < n; i++)
        {
            for (int c = 0; c < components; c++)
            {
                double proj = 0.0;
                for (int j = 0; j < d; j++) proj += centred[i, j] * eigenvectors[j, c];
                output[i * components + c] = proj;
            }
        }

        // Total variance explained
        double totalVariance = 0.0;
        for (int j = 0; j < d; j++) totalVariance += cov[j, j];
        double explainedVariance = 0.0;
        for (int c = 0; c < components; c++) explainedVariance += eigenvalues[c];

        sw.Stop();

        return AIResult.Ok(output, 0.0, 0, sw.Elapsed,
            metrics: ImmutableDictionary<string, double>.Empty
                .SetItem("ExplainedVarianceRatio", totalVariance > 0 ? explainedVariance / totalVariance : 0.0)
                .SetItem("Components", components),
            diagnostics: [$"PCA reduced {d}D → {components}D, variance explained={explainedVariance / System.Math.Max(totalVariance, 1e-15):P2}"],
            message: $"PCA completed. {components} components, variance explained={explainedVariance / System.Math.Max(totalVariance, 1e-15):P2}");
    }

    /// <summary>Reduces dimensionality using a simplified t-SNE implementation.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <param name="components">Number of output dimensions (typically 2 or 3).</param>
    /// <returns>Result containing the low-dimensional embedding (flattened).</returns>
    public AIResult ReduceTSNE(double[][] data, int components)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = data.Length;
        int d = data[0].Length;

        if (components > d) components = d;
        if (n <= components) components = System.Math.Max(1, n - 1);

        double perplexity = System.Math.Min(30.0, (double)(n - 1) / 3.0);
        int iterations = 1000;
        double learningRate = 200.0;
        double momentum = 0.8;

        // Compute pairwise distances
        double[,] distances = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double dist = System.Math.Sqrt(EuclideanDistanceSquared(data[i], data[j]));
                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        // Compute joint probabilities p_{j|i} using Gaussian kernel
        double targetEntropy = System.Math.Log(perplexity);
        double[,] p = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            // Binary search for sigma_i
            double sigmaMin = 1e-10, sigmaMax = 1e10;
            double[] pi = new double[n];

            for (int search = 0; search < 50; search++)
            {
                double sigma = (sigmaMin + sigmaMax) / 2.0;
                double twoSigmaSq = 2.0 * sigma * sigma;

                double sumP = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    pi[j] = System.Math.Exp(-distances[i, j] * distances[i, j] / twoSigmaSq);
                    sumP += pi[j];
                }

                if (sumP < 1e-15) { sigmaMax = sigma; continue; }

                double entropy = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    pi[j] /= sumP;
                    if (pi[j] > 1e-15) entropy -= pi[j] * System.Math.Log(pi[j]);
                }

                if (entropy > targetEntropy) sigmaMax = sigma;
                else sigmaMin = sigma;
            }

            for (int j = 0; j < n; j++) p[i, j] = pi[j];
        }

        // Symmetrise: p_{ij} = (p_{j|i} + p_{i|j}) / 2n
        double[,] P = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                P[i, j] = (p[i, j] + p[j, i]) / (2.0 * n);
            }
        }

        // Initialise low-dimensional embedding randomly
        double[,] y = new double[n, components];
        double[,] yPrev = new double[n, components];
        for (int i = 0; i < n; i++)
        {
            for (int c = 0; c < components; c++)
            {
                y[i, c] = _random.NextDouble() * 0.01 - 0.005;
                yPrev[i, c] = y[i, c];
            }
        }

        double earlyExaggeration = 4.0;

        for (int iter = 0; iter < iterations; iter++)
        {
            double useMomentum = iter < 250 ? 0.5 : momentum;
            double useExaggeration = iter < 250 ? earlyExaggeration : 1.0;

            // Compute low-dimensional similarities q_{ij} using Student-t
            double[,] Q = new double[n, n];
            double sumQ = 0.0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double distSq = 0.0;
                    for (int c = 0; c < components; c++)
                    {
                        double diff = y[i, c] - y[j, c];
                        distSq += diff * diff;
                    }
                    double qij = 1.0 / (1.0 + distSq);
                    Q[i, j] = qij;
                    Q[j, i] = qij;
                    sumQ += 2.0 * qij;
                }
            }

            // Normalise Q
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    Q[i, j] = System.Math.Max(Q[i, j] / System.Math.Max(sumQ, 1e-15), 1e-15);

            // Compute gradients
            double[,] grads = new double[n, components];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    double diff = useExaggeration * P[i, j] - Q[i, j];
                    double factor = diff * Q[i, j] * (1.0 + EuclideanDistanceSquared(
                        new[] { y[i, 0] }, new[] { y[j, 0] }) == 0 ? 1.0 : 1.0);

                    // Recompute distance properly
                    double distSq = 0.0;
                    for (int c = 0; c < components; c++)
                    {
                        double dd = y[i, c] - y[j, c];
                        distSq += dd * dd;
                    }
                    factor = (useExaggeration * P[i, j] - Q[i, j]) * Q[i, j] / System.Math.Max(1.0 + distSq, 1e-15);

                    for (int c = 0; c < components; c++)
                    {
                        grads[i, c] += factor * (y[i, c] - y[j, c]);
                    }
                }
            }

            // Update embedding
            for (int i = 0; i < n; i++)
            {
                for (int c = 0; c < components; c++)
                {
                    double update = learningRate * grads[i, c] + useMomentum * (y[i, c] - yPrev[i, c]);
                    yPrev[i, c] = y[i, c];
                    y[i, c] += update;

                    // Clip to prevent NaN
                    y[i, c] = System.Math.Clamp(y[i, c], -1e10, 1e10);
                }
            }
        }

        sw.Stop();

        // Flatten output
        double[] output = new double[n * components];
        for (int i = 0; i < n; i++)
        {
            for (int c = 0; c < components; c++)
            {
                output[i * components + c] = y[i, c];
            }
        }

        return AIResult.Ok(output, 0.0, iterations, sw.Elapsed,
            metrics: ImmutableDictionary<string, double>.Empty.SetItem("Components", components),
            diagnostics: [$"t-SNE reduced {d}D → {components}D over {iterations} iterations"],
            message: $"t-SNE completed. {components}D embedding for {n} points.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Neural Network
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Creates a new sequential neural network builder.</summary>
    /// <returns>A new <see cref="NeuralNetworkBuilder"/>.</returns>
    public NeuralNetworkBuilder BuildNetwork() => new(_options.RandomSeed);

    /// <summary>Trains a sequential neural network on the given data.</summary>
    /// <param name="network">The network to train.</param>
    /// <param name="inputs">Input data matrix (N × inputSize).</param>
    /// <param name="targets">Target data matrix (N × outputSize).</param>
    /// <param name="epochs">Number of training epochs.</param>
    /// <returns>Result containing final loss and trained network.</returns>
    public AIResult TrainNetwork(SequentialNetwork network, double[][] inputs, double[][] targets, int epochs)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = inputs.Length;
        double lr = _options.LearningRate;
        double bestLoss = double.MaxValue;
        List<string> diagnostics = [];

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            double epochLoss = 0.0;

            for (int i = 0; i < n; i++)
            {
                // Forward pass
                double[] output = network.Forward(inputs[i]);

                // Compute MSE loss
                double sampleLoss = 0.0;
                for (int j = 0; j < targets[i].Length; j++)
                {
                    double diff = output[j] - targets[i][j];
                    sampleLoss += diff * diff;
                }
                sampleLoss /= targets[i].Length;
                epochLoss += sampleLoss;

                // Backward pass
                network.Backward(targets[i], lr);
            }

            epochLoss /= n;

            if (_options.EnableDiagnostics && epoch % System.Math.Max(1, epochs / 10) == 0)
            {
                diagnostics.Add($"Epoch {epoch}/{epochs}: loss={epochLoss:E6}");
            }

            if (epochLoss < bestLoss) bestLoss = epochLoss;

            if (epochLoss < _options.ConvergenceTolerance)
            {
                diagnostics.Add($"Converged at epoch {epoch}: loss={epochLoss:E6}");
                break;
            }
        }

        sw.Stop();

        return AIResult.Ok(Array.Empty<double>(), bestLoss, epochs, sw.Elapsed,
            diagnostics: diagnostics,
            message: $"Network trained. Final loss={bestLoss:E6}");
    }

    /// <summary>Runs forward inference on a trained network.</summary>
    /// <param name="network">Trained network.</param>
    /// <param name="inputs">Input data matrix.</param>
    /// <returns>Output data matrix (flattened).</returns>
    public double[][] PredictNetwork(SequentialNetwork network, double[][] inputs)
    {
        double[][] results = new double[inputs.Length][];
        for (int i = 0; i < inputs.Length; i++)
        {
            results[i] = network.Forward(inputs[i]);
        }
        return results;
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Optimization
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Minimises an objective function using the specified gradient-free or gradient-based method.</summary>
    /// <param name="objective">Objective function to minimise.</param>
    /// <param name="initial">Initial parameter vector.</param>
    /// <param name="method">Optimizer name: "Adam", "SGD", "RMSProp", "NelderMead", or "RandomSearch".</param>
    /// <returns>Result containing the optimal parameter vector.</returns>
    public AIResult Optimize(Func<double[], double> objective, double[] initial, string method = "Adam")
    {
        return method.ToUpperInvariant() switch
        {
            "NELDERMEAD" => OptimizeNelderMead(objective, initial),
            "RANDOMSEARCH" => OptimizeRandomSearch(objective, initial),
            _ => _registry.RunOptimizer(method, initial, _configuration),
        };
    }

    /// <summary>Minimises an objective function using the Nelder-Mead simplex method.</summary>
    /// <param name="objective">Objective function to minimise.</param>
    /// <param name="initial">Initial parameter vector.</param>
    /// <returns>Result containing the optimal parameter vector.</returns>
    public AIResult OptimizeNelderMead(Func<double[], double> objective, double[] initial)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = initial.Length;
        int maxIter = _options.MaxEpochs > 0 ? _options.MaxEpochs : 5000;
        double tolerance = _options.ConvergenceTolerance;

        // Build simplex: n+1 vertices
        double[][] simplex = new double[n + 1][];
        double[] fValues = new double[n + 1];

        simplex[0] = (double[])initial.Clone();
        fValues[0] = objective(initial);

        for (int i = 0; i < n; i++)
        {
            simplex[i + 1] = (double[])initial.Clone();
            simplex[i + 1][i] += (simplex[i + 1][i] != 0) ? 0.05 * simplex[i + 1][i] : 0.05;
            fValues[i + 1] = objective(simplex[i + 1]);
        }

        double alpha = 1.0, gamma = 2.0, rho = 0.5, sigma = 0.5;

        for (int iter = 0; iter < maxIter; iter++)
        {
            // Sort by function value
            int[] indices = new int[n + 1];
            for (int i = 0; i <= n; i++) indices[i] = i;
            Array.Sort(indices, (a, b) => fValues[a].CompareTo(fValues[b]));

            int best = indices[0], worst = indices[n], secondWorst = indices[n - 1];

            // Check convergence
            double range = System.Math.Abs(fValues[worst] - fValues[best]);
            if (range < tolerance) break;

            // Centroid of all except worst
            double[] centroid = new double[n];
            for (int i = 0; i <= n; i++)
            {
                if (i == worst) continue;
                for (int j = 0; j < n; j++) centroid[j] += simplex[i][j];
            }
            for (int j = 0; j < n; j++) centroid[j] /= n;

            // Reflection
            double[] reflected = new double[n];
            for (int j = 0; j < n; j++) reflected[j] = centroid[j] + alpha * (centroid[j] - simplex[worst][j]);
            double fReflected = objective(reflected);

            if (fReflected < fValues[secondWorst] && fReflected >= fValues[best])
            {
                simplex[worst] = reflected;
                fValues[worst] = fReflected;
            }
            else if (fReflected < fValues[best])
            {
                // Expansion
                double[] expanded = new double[n];
                for (int j = 0; j < n; j++) expanded[j] = centroid[j] + gamma * (reflected[j] - centroid[j]);
                double fExpanded = objective(expanded);

                if (fExpanded < fReflected)
                {
                    simplex[worst] = expanded;
                    fValues[worst] = fExpanded;
                }
                else
                {
                    simplex[worst] = reflected;
                    fValues[worst] = fReflected;
                }
            }
            else
            {
                // Contraction
                double[] contracted = new double[n];
                for (int j = 0; j < n; j++) contracted[j] = centroid[j] + rho * (simplex[worst][j] - centroid[j]);
                double fContracted = objective(contracted);

                if (fContracted < fValues[worst])
                {
                    simplex[worst] = contracted;
                    fValues[worst] = fContracted;
                }
                else
                {
                    // Shrink
                    for (int i = 0; i <= n; i++)
                    {
                        if (i == best) continue;
                        for (int j = 0; j < n; j++)
                            simplex[i][j] = simplex[best][j] + sigma * (simplex[i][j] - simplex[best][j]);
                        fValues[i] = objective(simplex[i]);
                    }
                }
            }
        }

        sw.Stop();

        // Find best
        int bestIdx = 0;
        for (int i = 1; i <= n; i++)
        {
            if (fValues[i] < fValues[bestIdx]) bestIdx = i;
        }

        return AIResult.Ok(simplex[bestIdx], fValues[bestIdx], maxIter, sw.Elapsed,
            message: $"Nelder-Mead completed. f*={fValues[bestIdx]:E6}");
    }

    /// <summary>Minimises an objective function using random search.</summary>
    /// <param name="objective">Objective function to minimise.</param>
    /// <param name="initial">Initial parameter vector (defines scale).</param>
    /// <returns>Result containing the best parameter vector found.</returns>
    public AIResult OptimizeRandomSearch(Func<double[], double> objective, double[] initial)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = initial.Length;
        int maxEval = _options.MaxEpochs > 0 ? _options.MaxEpochs * n : 10000;
        double[] best = (double[])initial.Clone();
        double bestVal = objective(initial);

        for (int eval = 0; eval < maxEval; eval++)
        {
            double[] candidate = new double[n];
            for (int j = 0; j < n; j++)
            {
                candidate[j] = best[j] + (_random.NextDouble() - 0.5) * 2.0 * System.Math.Max(System.Math.Abs(best[j]), 1.0);
            }

            double val = objective(candidate);
            if (val < bestVal)
            {
                bestVal = val;
                Array.Copy(candidate, best, n);
            }
        }

        sw.Stop();

        return AIResult.Ok(best, bestVal, maxEval, sw.Elapsed,
            message: $"Random search completed. f*={bestVal:E6}");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Probabilistic
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Runs a particle filter for state estimation.</summary>
    /// <param name="measurementModel">Measurement model: state → predicted observation.</param>
    /// <param name="initialState">Initial state estimate.</param>
    /// <param name="observations">Sequence of observed measurements.</param>
    /// <returns>Result containing the estimated state trajectory.</returns>
    public AIResult RunParticleFilter(Func<double[], double> measurementModel, double[] initialState, double[][] observations)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int stateDim = initialState.Length;
        int numParticles = 1000;
        int numObs = observations.Length;

        // Initialise particles
        double[][] particles = new double[numParticles][];
        double[] weights = new double[numParticles];

        for (int p = 0; p < numParticles; p++)
        {
            particles[p] = new double[stateDim];
            for (int j = 0; j < stateDim; j++)
            {
                particles[p][j] = initialState[j] + (_random.NextDouble() - 0.5) * 2.0;
            }
            weights[p] = 1.0 / numParticles;
        }

        double[][] trajectory = new double[numObs][];

        for (int t = 0; t < numObs; t++)
        {
            // Predict: add process noise
            for (int p = 0; p < numParticles; p++)
            {
                for (int j = 0; j < stateDim; j++)
                {
                    particles[p][j] += (_random.NextDouble() - 0.5) * 0.1;
                }
            }

            // Update weights based on measurement likelihood
            double maxLogWeight = double.MinValue;
            double[] logWeights = new double[numParticles];

            for (int p = 0; p < numParticles; p++)
            {
                double predicted = measurementModel(particles[p]);
                double innovation = observations[t][0] - predicted;
                logWeights[p] = -0.5 * innovation * innovation / 1.0; // Gaussian likelihood, sigma=1
                if (logWeights[p] > maxLogWeight) maxLogWeight = logWeights[p];
            }

            // Normalize weights in log-space then convert
            double sumW = 0.0;
            for (int p = 0; p < numParticles; p++)
            {
                weights[p] = System.Math.Exp(logWeights[p] - maxLogWeight);
                sumW += weights[p];
            }
            for (int p = 0; p < numParticles; p++) weights[p] /= sumW;

            // Estimate state
            double[] stateEstimate = new double[stateDim];
            for (int p = 0; p < numParticles; p++)
            {
                for (int j = 0; j < stateDim; j++)
                {
                    stateEstimate[j] += weights[p] * particles[p][j];
                }
            }
            trajectory[t] = stateEstimate;

            // Resample (systematic resampling)
            double[] newParticlesFlat = new double[numParticles * stateDim];
            double r = _random.NextDouble() / numParticles;
            double cumWeight = 0.0;
            int idx = 0;
            for (int p = 0; p < numParticles; p++)
            {
                cumWeight += weights[p];
                while (r + (double)p / numParticles < cumWeight && idx < numParticles)
                {
                    for (int j = 0; j < stateDim; j++)
                        newParticlesFlat[idx * stateDim + j] = particles[p][j];
                    idx++;
                }
            }
            // Fill remaining
            while (idx < numParticles)
            {
                for (int j = 0; j < stateDim; j++)
                    newParticlesFlat[idx * stateDim + j] = particles[numParticles - 1][j];
                idx++;
            }
            for (int p = 0; p < numParticles; p++)
            {
                for (int j = 0; j < stateDim; j++)
                    particles[p][j] = newParticlesFlat[p * stateDim + j];
                weights[p] = 1.0 / numParticles;
            }
        }

        sw.Stop();

        // Flatten trajectory
        double[] output = new double[numObs * stateDim];
        for (int t = 0; t < numObs; t++)
            for (int j = 0; j < stateDim; j++)
                output[t * stateDim + j] = trajectory[t][j];

        return AIResult.Ok(output, 0.0, numObs, sw.Elapsed,
            diagnostics: [$"Particle filter: {numParticles} particles, {numObs} observations"],
            message: $"Particle filter completed. {numObs} time steps estimated.");
    }

    /// <summary>Runs a linear Kalman filter for state estimation.</summary>
    /// <param name="initial">Initial state estimate.</param>
    /// <param name="observations">Sequence of observed measurements.</param>
    /// <param name="measurementNoise">Measurement noise variance.</param>
    /// <returns>Result containing the estimated state trajectory.</returns>
    public AIResult RunKalmanFilter(double[] initial, double[][] observations, double measurementNoise)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        int n = initial.Length;
        int numObs = observations.Length;
        double processNoise = 0.01;

        // State estimate and covariance
        double[] x = (double[])initial.Clone();
        double[,] P = new double[n, n];
        for (int i = 0; i < n; i++) P[i, i] = 1.0;

        // State transition (identity for simple random walk)
        // Measurement matrix (identity: we observe full state)
        double[,] H = new double[n, n];
        for (int i = 0; i < n; i++) H[i, i] = 1.0;

        double[][] trajectory = new double[numObs][];

        for (int t = 0; t < numObs; t++)
        {
            // Predict
            // x = F * x (identity: x stays same)
            // P = F * P * F^T + Q
            double[,] PNew = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    PNew[i, j] = P[i, j];
                    if (i == j) PNew[i, j] += processNoise;
                }
            }
            P = PNew;

            // Measurement residual
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                y[i] = observations[t][i] - x[i];
            }

            // Innovation covariance: S = H * P * H^T + R
            double[,] S = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    S[i, j] = P[i, j];
                    if (i == j) S[i, j] += measurementNoise;
                }
            }

            // Kalman gain: K = P * H^T * S^{-1}
            // Since H = I, K = P * S^{-1}
            double[,] K = SolveMatrixSystem(P, S);

            // Update state
            for (int i = 0; i < n; i++)
            {
                double update = 0.0;
                for (int j = 0; j < n; j++) update += K[i, j] * y[j];
                x[i] += update;
            }

            // Update covariance: P = (I - K * H) * P
            double[,] PHat = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double kh = 0.0;
                    for (int k = 0; k < n; k++) kh += K[i, k] * H[k, j];
                    PHat[i, j] = (i == j ? 1.0 : 0.0) - kh;
                }
            }

            double[,] PUpdated = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                        PUpdated[i, j] += PHat[i, k] * P[k, j];

            P = PUpdated;

            trajectory[t] = (double[])x.Clone();
        }

        sw.Stop();

        double[] output = new double[numObs * n];
        for (int t = 0; t < numObs; t++)
            for (int j = 0; j < n; j++)
                output[t * n + j] = trajectory[t][j];

        return AIResult.Ok(output, 0.0, numObs, sw.Elapsed,
            diagnostics: [$"Kalman filter: {n}D state, {numObs} observations"],
            message: $"Kalman filter completed. {numObs} time steps estimated.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Symbolic AI
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Extracts mathematical patterns from an expression string.</summary>
    /// <param name="expression">Mathematical expression to analyse.</param>
    /// <returns>List of discovered patterns.</returns>
    public List<string> ExtractPatterns(string expression)
    {
        List<string> patterns = [];
        string expr = expression.Trim();

        if (expr.Contains('+')) patterns.Add("AdditiveStructure");
        if (expr.Contains('-')) patterns.Add("SubtractiveStructure");
        if (expr.Contains('*') || expr.Contains("·")) patterns.Add("MultiplicativeStructure");
        if (expr.Contains('/') || expr.Contains('÷')) patterns.Add("DivisiveStructure");
        if (expr.Contains('^') || expr.Contains("**")) patterns.Add("PowerStructure");
        if (expr.Contains("sin") || expr.Contains("cos") || expr.Contains("tan")) patterns.Add("TrigonometricStructure");
        if (expr.Contains("log") || expr.Contains("ln") || expr.Contains("exp")) patterns.Add("ExponentialLogStructure");
        if (expr.Contains("sqrt")) patterns.Add("RadicalStructure");

        // Check for polynomial structure: ax^n + bx^(n-1) + …
        if (!expr.Contains("sin") && !expr.Contains("cos") && !expr.Contains("log") && (expr.Contains('^') || IsSimplePolynomial(expr)))
        {
            patterns.Add("PolynomialStructure");
        }

        // Check for rational function
        if (expr.Contains('/'))
        {
            patterns.Add("RationalStructure");
        }

        // Check for nested composition
        int depth = 0;
        int maxDepth = 0;
        foreach (char c in expr)
        {
            if (c == '(') { depth++; if (depth > maxDepth) maxDepth = depth; }
            if (c == ')') depth--;
        }
        if (maxDepth >= 2) patterns.Add("NestedComposition");

        // Check for symmetry
        string reversed = new string(expr.Reverse().ToArray());
        if (string.Equals(expr, reversed, StringComparison.OrdinalIgnoreCase))
        {
            patterns.Add("SymmetricExpression");
        }

        return patterns;
    }

    /// <summary>Suggests algebraic simplifications for an expression string.</summary>
    /// <param name="expression">Mathematical expression to simplify.</param>
    /// <returns>List of suggested simplification rules.</returns>
    public List<string> SuggestSimplifications(string expression)
    {
        List<string> suggestions = [];
        string expr = expression.Trim();

        if (expr.Contains("0 + ") || expr.Contains(" + 0"))
            suggestions.Add("AdditiveIdentity: x + 0 = x");

        if (expr.Contains("1 * ") || expr.Contains(" * 1"))
            suggestions.Add("MultiplicativeIdentity: x · 1 = x");

        if (expr.Contains("0 * ") || expr.Contains(" * 0"))
            suggestions.Add("ZeroMultiplication: x · 0 = 0");

        if (expr.Contains("x^1"))
            suggestions.Add("PowerOfOne: x^1 = x");

        if (expr.Contains("x^0"))
            suggestions.Add("PowerOfZero: x^0 = 1");

        if (expr.Contains("sin") && expr.Contains("cos") && expr.Contains('^'))
            suggestions.Add("PythagoreanIdentity: sin²θ + cos²θ = 1");

        if (expr.Contains("2 * ") || expr.Contains(" * 2"))
            suggestions.Add("DoubleAngle: Consider using double-angle formulas");

        if (expr.Contains("ln") && expr.Contains("exp"))
            suggestions.Add("InverseFunctions: ln(exp(x)) = x");

        if (expr.Contains("sqrt") && expr.Contains('^'))
            suggestions.Add("RadicalToPower: sqrt(x) = x^(1/2)");

        // Check for common factor
        string[] terms = expr.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length > 1)
        {
            suggestions.Add("FactorCommonTerms: Look for GCF in all terms");
        }

        // Check for difference of squares pattern
        if (expr.Contains("a^2") && expr.Contains("b^2") && expr.Contains('-'))
            suggestions.Add("DifferenceOfSquares: a² - b² = (a+b)(a-b)");

        return suggestions;
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Mathematical Learning
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Learns a symbolic expression from data points by trying common function forms.</summary>
    /// <param name="dataPoints">Data points (x, y) pairs stored as (x_i, y_i) in a flat array or 2D layout.</param>
    /// <returns>The best-fitting expression string.</returns>
    public string LearnExpression(double[][] dataPoints)
    {
        if (dataPoints.Length < 2) return "InsufficientData";

        int n = dataPoints.Length;

        // Try linear fit: y = mx + b
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            double x = dataPoints[i][0];
            double y = dataPoints[i][1];
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double denom = n * sumX2 - sumX * sumX;
        double m = 0, b = 0;
        if (System.Math.Abs(denom) > 1e-15)
        {
            m = (n * sumXY - sumX * sumY) / denom;
            b = (sumY - m * sumX) / n;
        }

        double linearMSE = ComputeRegressionMSE(dataPoints, x => m * x + b);

        // Try quadratic: y = ax^2 + bx + c (simplified via least squares)
        double quadMSE = ComputePolynomialMSE(dataPoints, 2);

        // Try exponential: y = a * exp(b * x) — linearised via ln(y)
        double expMSE = double.MaxValue;
        bool allPositive = true;
        for (int i = 0; i < n; i++) { if (dataPoints[i][1] <= 0) allPositive = false; }
        if (allPositive)
        {
            double[] logY = new double[n];
            for (int i = 0; i < n; i++) logY[i] = System.Math.Log(dataPoints[i][1]);
            double sumLogY = 0;
            for (int i = 0; i < n; i++) sumLogY += logY[i];
            double sumXLogY = 0;
            for (int i = 0; i < n; i++) sumXLogY += dataPoints[i][0] * logY[i];

            double bExp = 0;
            if (System.Math.Abs(denom) > 1e-15)
                bExp = (n * sumXLogY - sumX * sumLogY) / denom;
            double logA = (sumLogY - bExp * sumX) / n;
            double aExp = System.Math.Exp(logA);

            expMSE = ComputeRegressionMSE(dataPoints, x => aExp * System.Math.Exp(bExp * x));
        }

        // Try power: y = a * x^b — linearised via ln-ln
        double powerMSE = double.MaxValue;
        bool allPositiveXY = true;
        for (int i = 0; i < n; i++)
        {
            if (dataPoints[i][0] <= 0 || dataPoints[i][1] <= 0) allPositiveXY = false;
        }
        if (allPositiveXY && n >= 2)
        {
            double[] logX = new double[n];
            double[] logY2 = new double[n];
            for (int i = 0; i < n; i++)
            {
                logX[i] = System.Math.Log(dataPoints[i][0]);
                logY2[i] = System.Math.Log(dataPoints[i][1]);
            }
            double sx = 0, sy = 0, sxy = 0, sx2 = 0;
            for (int i = 0; i < n; i++)
            {
                sx += logX[i];
                sy += logY2[i];
                sxy += logX[i] * logY2[i];
                sx2 += logX[i] * logX[i];
            }
            double denomP = n * sx2 - sx * sx;
            if (System.Math.Abs(denomP) > 1e-15)
            {
                double bPow = (n * sxy - sx * sy) / denomP;
                double aPow = System.Math.Exp((sy - bPow * sx) / n);
                powerMSE = ComputeRegressionMSE(dataPoints, x => aPow * System.Math.Pow(x, bPow));
            }
        }

        // Select best model
        string bestExpr = $"y = {m:F6}x + {b:F6}";
        double bestMSE = linearMSE;

        if (quadMSE < bestMSE)
        {
            bestExpr = "y = quadratic(…)";
            bestMSE = quadMSE;
        }
        if (expMSE < bestMSE)
        {
            bestExpr = "y = a·exp(b·x)";
            bestMSE = expMSE;
        }
        if (powerMSE < bestMSE)
        {
            bestExpr = "y = a·x^b";
            bestMSE = powerMSE;
        }

        return $"{bestExpr} [MSE={bestMSE:E6}]";
    }

    /// <summary>Compares two expression strings for structural similarity.</summary>
    /// <param name="expr1">First expression.</param>
    /// <param name="expr2">Second expression.</param>
    /// <returns>Similarity score between 0 (completely different) and 1 (identical).</returns>
    public double CompareExpressions(string expr1, string expr2)
    {
        if (string.Equals(expr1, expr2, StringComparison.OrdinalIgnoreCase)) return 1.0;

        // Character-level similarity using longest common subsequence ratio
        int lcsLen = LongestCommonSubsequenceLength(expr1, expr2);
        int maxLen = System.Math.Max(expr1.Length, expr2.Length);

        if (maxLen == 0) return 1.0;
        return (double)lcsLen / maxLen;
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Graph Intelligence
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Computes PageRank scores for a directed graph.</summary>
    /// <param name="graph">Adjacency list: node → list of outgoing neighbours.</param>
    /// <param name="iterations">Number of power-iteration steps.</param>
    /// <param name="dampingFactor">Damping factor (typically 0.85).</param>
    /// <returns>Map of node → PageRank score.</returns>
    public Dictionary<int, double> ComputePageRank(Dictionary<int, List<int>> graph, int iterations, double dampingFactor)
    {
        int n = graph.Count;
        if (n == 0) return new Dictionary<int, double>();

        int[] nodeIds = [.. graph.Keys];
        Dictionary<int, int> nodeIndex = new();
        for (int i = 0; i < n; i++) nodeIndex[nodeIds[i]] = i;

        double[] rank = new double[n];
        double[] newRank = new double[n];
        for (int i = 0; i < n; i++) rank[i] = 1.0 / n;

        for (int iter = 0; iter < iterations; iter++)
        {
            // Dangling node contribution
            double danglingSum = 0.0;
            for (int i = 0; i < n; i++)
            {
                List<int> neighbours = graph[nodeIds[i]];
                if (neighbours.Count == 0)
                {
                    danglingSum += rank[i];
                }
            }

            for (int i = 0; i < n; i++) newRank[i] = (1.0 - dampingFactor) / n + dampingFactor * danglingSum / n;

            // Contribution from links
            for (int i = 0; i < n; i++)
            {
                List<int> neighbours = graph[nodeIds[i]];
                if (neighbours.Count == 0) continue;

                double share = rank[i] / neighbours.Count;
                foreach (int neighbour in neighbours)
                {
                    if (nodeIndex.TryGetValue(neighbour, out int j))
                    {
                        newRank[j] += dampingFactor * share;
                    }
                }
            }

            Array.Copy(newRank, rank, n);
        }

        Dictionary<int, double> result = new();
        for (int i = 0; i < n; i++) result[nodeIds[i]] = rank[i];

        return result;
    }

    /// <summary>Detects communities in an undirected graph using label propagation.</summary>
    /// <param name="graph">Adjacency list (treated as undirected).</param>
    /// <returns>List of communities, each being a list of node IDs.</returns>
    public List<List<int>> DetectCommunities(Dictionary<int, List<int>> graph)
    {
        int maxIter = 100;
        Dictionary<int, int> labels = new();

        // Initialise: each node in its own community
        foreach (int node in graph.Keys) labels[node] = node;

        for (int iter = 0; iter < maxIter; iter++)
        {
            bool changed = false;

            foreach (int node in graph.Keys)
            {
                List<int> neighbours = graph[node];
                if (neighbours.Count == 0) continue;

                // Count neighbour labels
                Dictionary<int, int> labelCounts = new();
                foreach (int nb in neighbours)
                {
                    if (!labels.TryGetValue(nb, out int lbl)) continue;
                    labelCounts.TryGetValue(lbl, out int cnt);
                    labelCounts[lbl] = cnt + 1;
                }

                if (labelCounts.Count == 0) continue;

                // Find most frequent label (break ties by smallest label)
                int bestLabel = labels[node];
                int bestCount = -1;
                foreach (KeyValuePair<int, int> kv in labelCounts)
                {
                    if (kv.Value > bestCount || (kv.Value == bestCount && kv.Key < bestLabel))
                    {
                        bestCount = kv.Value;
                        bestLabel = kv.Key;
                    }
                }

                if (labels[node] != bestLabel)
                {
                    labels[node] = bestLabel;
                    changed = true;
                }
            }

            if (!changed) break;
        }

        // Group nodes by final label
        Dictionary<int, List<int>> communities = new();
        foreach (KeyValuePair<int, int> kv in labels)
        {
            if (!communities.TryGetValue(kv.Value, out List<int>? list))
            {
                list = [];
                communities[kv.Value] = list;
            }
            list.Add(kv.Key);
        }

        return [.. communities.Values];
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Recommendation
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Recommends a solver strategy for the given problem type.</summary>
    /// <param name="problemType">Problem type string (e.g. "ODE", "linear_system", "optimization").</param>
    /// <returns>Recommended solver name and configuration hint.</returns>
    public string RecommendSolver(string problemType)
    {
        return problemType.ToLowerInvariant() switch
        {
            "ode" or "differential_equation" => "RK45 (Runge-Kutta 4(5) adaptive step) — best for non-stiff ODEs",
            "stiff_ode" => "BDF (Backward Differentiation Formula) — best for stiff systems",
            "linear_system" => "LU decomposition for small systems, GMRES for large sparse systems",
            "optimization" or "minimize" => "Adam for gradient-based; Nelder-Mead for derivative-free",
            "nonlinear_optimization" => "L-BFGS for large-scale; Levenberg-Marquardt for least-squares",
            "root_finding" => "Newton-Raphson for smooth functions; Brent's method for bracketed roots",
            "integration" => "Gauss-Legendre quadrature for smooth functions; adaptive Simpson for oscillatory",
            "eigenvalue" => "QR algorithm for dense; Lanczos for large sparse symmetric",
            "interpolation" => "Cubic spline for smooth data; linear for noisy data",
            "classification" => "Random Forest for tabular; logistic regression for interpretable",
            "regression" => "Ridge regression for multicollinear features; Lasso for feature selection",
            "clustering" => "K-Means for spherical clusters; DBSCAN for arbitrary shapes",
            _ => $"No specific recommendation for '{problemType}'. Consider trying multiple solvers.",
        };
    }

    /// <summary>Recommends an algorithm based on a free-text problem description.</summary>
    /// <param name="problemDescription">Description of the problem to solve.</param>
    /// <returns>Recommended algorithm name and brief rationale.</returns>
    public string RecommendAlgorithm(string problemDescription)
    {
        string desc = problemDescription.ToLowerInvariant();

        if (desc.Contains("classify") || desc.Contains("class") || desc.Contains("label"))
        {
            if (desc.Contains("image") || desc.Contains("pixel"))
                return "Convolutional Neural Network (CNN) — state-of-the-art for image classification";
            if (desc.Contains("text") || desc.Contains("nlp"))
                return "Transformer-based classifier — state-of-the-art for text classification";
            return "Random Forest — robust default for tabular classification";
        }

        if (desc.Contains("regress") || desc.Contains("predict") || desc.Contains("forecast"))
        {
            if (desc.Contains("time series") || desc.Contains("temporal"))
                return "LSTM / Temporal Fusion Transformer — best for time series forecasting";
            return "Gradient Boosted Trees (XGBoost) — best default for tabular regression";
        }

        if (desc.Contains("cluster") || desc.Contains("group") || desc.Contains("segment"))
        {
            if (desc.Contains("density") || desc.Contains("noise"))
                return "DBSCAN — handles arbitrary shapes and noise";
            return "K-Means — fast and effective for spherical clusters";
        }

        if (desc.Contains("anomal") || desc.Contains("outlier") || desc.Contains("detect"))
            return "Isolation Forest or Autoencoder — effective for anomaly detection";

        if (desc.Contains("generat") || desc.Contains("synth"))
            return "Variational Autoencoder (VAE) or Diffusion Model — state-of-the-art generation";

        if (desc.Contains("optimi") || desc.Contains("minimi"))
            return "Adam optimizer with learning rate scheduling — fast and reliable";

        if (desc.Contains("reinforcement") || desc.Contains("rl") || desc.Contains("policy"))
            return "Proximal Policy Optimization (PPO) — stable and sample-efficient";

        if (desc.Contains("graph") || desc.Contains("network") || desc.Contains("node"))
            return "Graph Neural Network (GNN) — designed for graph-structured data";

        return "Random Forest — safe default for unknown problem structure";
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Integration helpers
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Analyses a simulation engine's current configuration and suggests AI-driven improvements.</summary>
    /// <param name="engine">The simulation engine to analyse.</param>
    /// <returns>Analysis report string.</returns>
    public string AnalyzeSimulation(MathVerse.Math.Simulation.Public.SimulationEngine engine)
    {
        StringBuilder sb = new();
        _ = sb.AppendLine("=== AI-Driven Simulation Analysis ===");
        _ = sb.AppendLine($"Options: MaxSteps={engine.Options.MaxSteps}, Mode={engine.Options.Mode}");
        _ = sb.AppendLine($"Configuration: PhysicsTimeStep={engine.Configuration.Physics.DefaultTimeStep}");

        List<string> suggestions =
        [
            "Consider using adaptive time-stepping for stiff systems",
            "Enable Monte Carlo uncertainty quantification for probabilistic inputs",
            "Use surrogate models (Gaussian Process) to accelerate expensive evaluations",
            "Apply sensitivity analysis to identify most influential parameters",
        ];

        _ = sb.AppendLine("Suggestions:");
        foreach (string s in suggestions)
        {
            _ = sb.AppendLine($"  - {s}");
        }

        return sb.ToString();
    }

    /// <summary>Analyses numerical data and recommends appropriate AI methods.</summary>
    /// <param name="data">Data matrix (N × D).</param>
    /// <returns>Analysis report string.</returns>
    public string AnalyzeNumerics(double[][] data)
    {
        if (data.Length == 0) return "Empty dataset.";

        int n = data.Length;
        int d = data[0].Length;

        // Compute basic statistics
        double[] means = new double[d];
        double[] stds = new double[d];

        for (int j = 0; j < d; j++)
        {
            double sum = 0;
            for (int i = 0; i < n; i++) sum += data[i][j];
            means[j] = sum / n;

            double sumSq = 0;
            for (int i = 0; i < n; i++) sumSq += (data[i][j] - means[j]) * (data[i][j] - means[j]);
            stds[j] = System.Math.Sqrt(sumSq / n);
        }

        StringBuilder sb = new();
        _ = sb.AppendLine("=== Numerical Data Analysis ===");
        _ = sb.AppendLine($"Samples: {n}, Features: {d}");

        for (int j = 0; j < d; j++)
        {
            _ = sb.AppendLine($"  Feature {j}: mean={means[j]:E4}, std={stds[j]:E4}");
        }

        if (n < 100) _ = sb.AppendLine("Recommendation: Small dataset — use simple models (linear/logistic regression, KNN)");
        else if (n < 10000) _ = sb.AppendLine("Recommendation: Medium dataset — use Random Forest or Gradient Boosting");
        else _ = sb.AppendLine("Recommendation: Large dataset — consider neural networks");

        if (d > n) _ = sb.AppendLine("Warning: High dimensionality. Apply PCA or feature selection first.");

        return sb.ToString();
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Import / Export
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>Serialises a model to a JSON string.</summary>
    /// <param name="model">Model to serialise.</param>
    /// <returns>JSON string representation.</returns>
    public string ExportModel(AIModel model)
    {
        StringBuilder sb = new();
        _ = sb.Append('{');
        _ = sb.Append($"\"ModelId\":\"{model.ModelId}\",");
        _ = sb.Append($"\"ModelType\":\"{model.ModelType}\",");
        _ = sb.Append($"\"TrainedAt\":\"{model.TrainedAt:O}\",");

        _ = sb.Append("\"HyperParameters\":{");
        bool first = true;
        foreach (KeyValuePair<string, double> kv in model.HyperParameters)
        {
            if (!first) _ = sb.Append(',');
            _ = sb.Append($"\"{kv.Key}\":{kv.Value}");
            first = false;
        }
        _ = sb.Append("},");

        _ = sb.Append("\"TrainedParameters\":{");
        first = true;
        foreach (KeyValuePair<string, double> kv in model.TrainedParameters)
        {
            if (!first) _ = sb.Append(',');
            _ = sb.Append($"\"{kv.Key}\":{kv.Value}");
            first = false;
        }
        _ = sb.Append("},");

        _ = sb.Append("\"Metrics\":{");
        first = true;
        foreach (KeyValuePair<string, double> kv in model.Metrics)
        {
            if (!first) _ = sb.Append(',');
            _ = sb.Append($"\"{kv.Key}\":{kv.Value}");
            first = false;
        }
        _ = sb.Append('}');

        _ = sb.Append('}');
        return sb.ToString();
    }

    /// <summary>Deserialises a model from a JSON string.</summary>
    /// <param name="serializedData">JSON string representation of the model.</param>
    /// <returns>The deserialised model, or <c>null</c> if parsing fails.</returns>
    public AIModel? ImportModel(string serializedData)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(serializedData);
            JsonElement root = doc.RootElement;

            string modelId = root.GetProperty("ModelId").GetString() ?? Guid.NewGuid().ToString("N");
            string modelType = root.GetProperty("ModelType").GetString() ?? "Unknown";

            AIModel model = new(modelId, modelType);

            if (root.TryGetProperty("TrainedAt", out JsonElement trainedAtEl))
            {
                string? dateStr = trainedAtEl.GetString();
                if (dateStr != null && DateTime.TryParse(dateStr, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dt))
                {
                    model = new AIModel(modelId, modelType)
                    {
                        TrainedAt = dt,
                        HyperParameters = ParseJsonDoubleDict(root, "HyperParameters"),
                        TrainedParameters = ParseJsonDoubleDict(root, "TrainedParameters"),
                        Metrics = ParseJsonDoubleDict(root, "Metrics"),
                    };
                }
            }

            if (root.TryGetProperty("HyperParameters", out _))
            {
                model = new AIModel(model.ModelId, model.ModelType)
                {
                    TrainedAt = model.TrainedAt,
                    HyperParameters = ParseJsonDoubleDict(root, "HyperParameters"),
                    TrainedParameters = model.TrainedParameters,
                    Metrics = model.Metrics,
                };
            }

            if (root.TryGetProperty("TrainedParameters", out _))
            {
                model = new AIModel(model.ModelId, model.ModelType)
                {
                    TrainedAt = model.TrainedAt,
                    HyperParameters = model.HyperParameters,
                    TrainedParameters = ParseJsonDoubleDict(root, "TrainedParameters"),
                    Metrics = model.Metrics,
                };
            }

            if (root.TryGetProperty("Metrics", out _))
            {
                model = new AIModel(model.ModelId, model.ModelType)
                {
                    TrainedAt = model.TrainedAt,
                    HyperParameters = model.HyperParameters,
                    TrainedParameters = model.TrainedParameters,
                    Metrics = ParseJsonDoubleDict(root, "Metrics"),
                };
            }

            return model;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Clears all cached data in the execution context.</summary>
    public void ClearCaches() => _context.ClearCache();

    // ───────────────────────────────────────────────────────────────────────
    //  Private helpers
    // ───────────────────────────────────────────────────────────────────────

    private static double Sigmoid(double x)
    {
        if (x >= 0)
        {
            double ex = System.Math.Exp(-x);
            return 1.0 / (1.0 + ex);
        }
        double exPos = System.Math.Exp(x);
        return exPos / (1.0 + exPos);
    }

    private static double SoftThreshold(double value, double threshold)
    {
        if (value > threshold) return value - threshold;
        if (value < -threshold) return value + threshold;
        return 0.0;
    }

    private static double EuclideanDistanceSquared(double[] a, double[] b)
    {
        double sum = 0.0;
        int len = System.Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            double diff = a[i] - b[i];
            sum += diff * diff;
        }
        return sum;
    }

    private double[][] KMeansPPInit(double[][] data, int k, Random rng)
    {
        int n = data.Length;
        int d = data[0].Length;
        double[][] centroids = new double[k][];

        // First centroid: random
        centroids[0] = (double[])data[rng.Next(n)].Clone();

        double[] distances = new double[n];

        for (int c = 1; c < k; c++)
        {
            // Compute distance to nearest centroid
            for (int i = 0; i < n; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < c; j++)
                {
                    double dist = EuclideanDistanceSquared(data[i], centroids[j]);
                    if (dist < minDist) minDist = dist;
                }
                distances[i] = minDist;
            }

            // Weighted random selection
            double totalDist = 0.0;
            for (int i = 0; i < n; i++) totalDist += distances[i];

            double r = rng.NextDouble() * totalDist;
            double cumSum = 0.0;
            int selected = 0;
            for (int i = 0; i < n; i++)
            {
                cumSum += distances[i];
                if (cumSum >= r)
                {
                    selected = i;
                    break;
                }
            }

            centroids[c] = (double[])data[selected].Clone();
        }

        return centroids;
    }

    private List<int> GetNeighbours(double[][] data, int pointIndex, double epsilon)
    {
        List<int> neighbours = [];
        for (int i = 0; i < data.Length; i++)
        {
            if (System.Math.Sqrt(EuclideanDistanceSquared(data[pointIndex], data[i])) <= epsilon)
            {
                neighbours.Add(i);
            }
        }
        return neighbours;
    }

    private void ExpandCluster(double[][] data, int[] labels, bool[] visited, int pointIndex,
        List<int> neighbours, int clusterId, double epsilon, int minPoints)
    {
        labels[pointIndex] = clusterId;

        Queue<int> queue = new(neighbours);
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            if (!visited[current])
            {
                visited[current] = true;
                List<int> currentNeighbours = GetNeighbours(data, current, epsilon);
                if (currentNeighbours.Count >= minPoints)
                {
                    foreach (int nb in currentNeighbours)
                    {
                        if (labels[nb] < 0 || !visited[nb])
                        {
                            queue.Enqueue(nb);
                        }
                    }
                }
            }
            if (labels[current] < 0)
            {
                labels[current] = clusterId;
            }
        }
    }

    private void BuildDecisionTree(double[][] features, int[] labels, Dictionary<string, double> treeParams, int depth, int maxDepth, int treeIndex)
    {
        if (depth >= maxDepth || features.Length <= 1)
        {
            // Leaf: majority class
            Dictionary<int, int> classCounts = new();
            foreach (int lbl in labels)
            {
                classCounts.TryGetValue(lbl, out int cnt);
                classCounts[lbl] = cnt + 1;
            }
            int majorityClass = 0;
            int maxCount = 0;
            foreach (KeyValuePair<int, int> kv in classCounts)
            {
                if (kv.Value > maxCount) { maxCount = kv.Value; majorityClass = kv.Key; }
            }
            string prefix = $"t{treeIndex}_d{depth}";
            treeParams[$"{prefix}_leaf"] = 1;
            treeParams[$"{prefix}_class"] = majorityClass;
            return;
        }

        // Find best split: for each feature, try the mean as threshold
        int bestFeature = 0;
        double bestThreshold = 0;
        double bestGain = -1;

        int n = features.Length;
        int d = features[0].Length;

        for (int j = 0; j < d; j++)
        {
            double[] vals = new double[n];
            for (int i = 0; i < n; i++) vals[i] = features[i][j];
            Array.Sort(vals);
            double median = vals[n / 2];

            double gain = ComputeInfoGain(features, labels, j, median);
            if (gain > bestGain)
            {
                bestGain = gain;
                bestFeature = j;
                bestThreshold = median;
            }
        }

        string pfx = $"t{treeIndex}_d{depth}";
        treeParams[$"{pfx}_feature"] = bestFeature;
        treeParams[$"{pfx}_threshold"] = bestThreshold;
        treeParams[$"{pfx}_leaf"] = 0;

        // Split
        List<double[]> leftFeatures = [], rightFeatures = [];
        List<int> leftLabels = [], rightLabels = [];

        for (int i = 0; i < n; i++)
        {
            if (features[i][bestFeature] <= bestThreshold)
            {
                leftFeatures.Add(features[i]);
                leftLabels.Add(labels[i]);
            }
            else
            {
                rightFeatures.Add(features[i]);
                rightLabels.Add(labels[i]);
            }
        }

        if (leftFeatures.Count > 0)
            BuildDecisionTree(leftFeatures.ToArray(), leftLabels.ToArray(), treeParams, depth + 1, maxDepth, treeIndex);
        if (rightFeatures.Count > 0)
            BuildDecisionTree(rightFeatures.ToArray(), rightLabels.ToArray(), treeParams, depth + 1, maxDepth, treeIndex);
    }

    private static double ComputeInfoGain(double[][] features, int[] labels, int featureIndex, double threshold)
    {
        int n = labels.Length;
        if (n == 0) return 0;

        double parentEntropy = ComputeEntropy(labels);

        List<int> leftLabels = [], rightLabels = [];
        for (int i = 0; i < n; i++)
        {
            if (features[i][featureIndex] <= threshold) leftLabels.Add(labels[i]);
            else rightLabels.Add(labels[i]);
        }

        if (leftLabels.Count == 0 || rightLabels.Count == 0) return 0;

        double leftEntropy = ComputeEntropy(leftLabels.ToArray());
        double rightEntropy = ComputeEntropy(rightLabels.ToArray());

        double leftWeight = (double)leftLabels.Count / n;
        double rightWeight = (double)rightLabels.Count / n;

        return parentEntropy - leftWeight * leftEntropy - rightWeight * rightEntropy;
    }

    private static double ComputeEntropy(int[] labels)
    {
        int n = labels.Length;
        if (n == 0) return 0;

        Dictionary<int, int> counts = new();
        foreach (int lbl in labels)
        {
            counts.TryGetValue(lbl, out int cnt);
            counts[lbl] = cnt + 1;
        }

        double entropy = 0.0;
        foreach (KeyValuePair<int, int> kv in counts)
        {
            double p = (double)kv.Value / n;
            if (p > 0) entropy -= p * System.Math.Log2(p);
        }
        return entropy;
    }

    private static double ComputeRegressionMSE(double[][] data, Func<double, double> func)
    {
        double mse = 0.0;
        int n = data.Length;
        for (int i = 0; i < n; i++)
        {
            double predicted = func(data[i][0]);
            double diff = predicted - data[i][1];
            mse += diff * diff;
        }
        return mse / n;
    }

    private static double ComputePolynomialMSE(double[][] data, int degree)
    {
        int n = data.Length;
        if (n <= degree) return double.MaxValue;

        // Build Vandermonde-like system for least squares
        int cols = degree + 1;
        double[,] A = new double[n, cols];
        for (int i = 0; i < n; i++)
        {
            double x = data[i][0];
            double xPow = 1.0;
            for (int j = 0; j < cols; j++)
            {
                A[i, j] = xPow;
                xPow *= x;
            }
        }

        // Normal equations: (A^T A) c = A^T b
        double[,] AtA = new double[cols, cols];
        double[] Atb = new double[cols];
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                double sum = 0;
                for (int k = 0; k < n; k++) sum += A[k, i] * A[k, j];
                AtA[i, j] = sum;
            }
            double bsum = 0;
            for (int k = 0; k < n; k++) bsum += A[k, i] * data[k][1];
            Atb[i] = bsum;
        }

        double[] coeffs = SolveLinearSystem(AtA, Atb);

        // Compute MSE
        double mse = 0.0;
        for (int i = 0; i < n; i++)
        {
            double x = data[i][0];
            double predicted = 0;
            double xPow = 1.0;
            for (int j = 0; j < cols; j++)
            {
                predicted += coeffs[j] * xPow;
                xPow *= x;
            }
            double diff = predicted - data[i][1];
            mse += diff * diff;
        }
        return mse / n;
    }

    private static double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        double[,] augmented = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++) augmented[i, j] = A[i, j];
            augmented[i, n] = b[i];
        }

        // Gaussian elimination with partial pivoting
        for (int col = 0; col < n; col++)
        {
            // Find pivot
            int maxRow = col;
            double maxVal = System.Math.Abs(augmented[col, col]);
            for (int row = col + 1; row < n; row++)
            {
                double absVal = System.Math.Abs(augmented[row, col]);
                if (absVal > maxVal) { maxVal = absVal; maxRow = row; }
            }

            // Swap
            if (maxRow != col)
            {
                for (int j = 0; j <= n; j++)
                {
                    (augmented[col, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[col, j]);
                }
            }

            double pivot = augmented[col, col];
            if (System.Math.Abs(pivot) < 1e-15) continue;

            for (int row = col + 1; row < n; row++)
            {
                double factor = augmented[row, col] / pivot;
                for (int j = col; j <= n; j++)
                {
                    augmented[row, j] -= factor * augmented[col, j];
                }
            }
        }

        // Back substitution
        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = augmented[i, n];
            for (int j = i + 1; j < n; j++)
            {
                sum -= augmented[i, j] * x[j];
            }
            double diag = augmented[i, i];
            x[i] = System.Math.Abs(diag) > 1e-15 ? sum / diag : 0.0;
        }

        return x;
    }

    private static double[,] SolveMatrixSystem(double[,] A, double[,] B)
    {
        int n = A.GetLength(0);
        // Solve A * X = B column by column
        double[,] X = new double[n, n];

        for (int col = 0; col < n; col++)
        {
            double[] bCol = new double[n];
            for (int i = 0; i < n; i++) bCol[i] = B[i, col];

            // Augment A with bCol and solve
            double[] solution = SolveLinearSystem(A, bCol);
            for (int i = 0; i < n; i++) X[i, col] = solution[i];
        }

        return X;
    }

    private static int LongestCommonSubsequenceLength(string a, string b)
    {
        int m = a.Length;
        int n = b.Length;
        int[] prev = new int[n + 1];
        int[] curr = new int[n + 1];

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (a[i - 1] == b[j - 1])
                    curr[j] = prev[j - 1] + 1;
                else
                    curr[j] = System.Math.Max(prev[j], curr[j - 1]);
            }
            (prev, curr) = (curr, prev);
            Array.Clear(curr, 0, curr.Length);
        }

        return prev[n];
    }

    private static bool IsSimplePolynomial(string expr)
    {
        foreach (char c in expr)
        {
            if (!char.IsDigit(c) && c != '.' && c != 'x' && c != 'X' && c != '+' && c != '-' && c != '^' && c != ' ')
            {
                return false;
            }
        }
        return true;
    }

    private static ImmutableDictionary<string, double> ParseJsonDoubleDict(JsonElement root, string propertyName)
    {
        ImmutableDictionary<string, double>.Builder builder = ImmutableDictionary.CreateBuilder<string, double>();

        if (root.TryGetProperty(propertyName, out JsonElement element) && element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Number)
                {
                    builder[prop.Name] = prop.Value.GetDouble();
                }
            }
        }

        return builder.ToImmutable();
    }
}
