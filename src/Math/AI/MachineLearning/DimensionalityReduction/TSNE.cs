namespace MathVerse.Math.AI.MachineLearning.DimensionalityReduction;

using System;

/// <summary>t-Distributed Stochastic Neighbor Embedding for nonlinear dimensionality reduction.</summary>
public sealed class TSNE
{
    private const double MinGain = 0.01;

    private double[][] _embedding = [];

    /// <summary>Gets the learned low-dimensional embedding after fitting.</summary>
    public double[][] Embedding => _embedding;

    /// <summary>Fits t-SNE to the data and returns the low-dimensional embedding.</summary>
    /// <param name="data">High-dimensional data matrix (samples x features).</param>
    /// <param name="numDimensions">Target dimensionality (typically 2 or 3).</param>
    /// <param name="perplexity">Perplexity parameter controlling the effective number of neighbors.</param>
    /// <param name="maxIterations">Maximum number of gradient descent iterations.</param>
    /// <param name="learningRate">Learning rate for gradient descent.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A <see cref="DimensionalityReductionResult"/> with the embedded data.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public DimensionalityReductionResult Fit(
        double[][] data,
        int numDimensions = 2,
        double perplexity = 30.0,
        int maxIterations = 1000,
        double learningRate = 200.0,
        int seed = 42)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (numDimensions <= 0)
            throw new ArgumentException("numDimensions must be positive.", nameof(numDimensions));
        if (perplexity <= 0.0 || perplexity >= data.Length)
            throw new ArgumentException($"perplexity must be between 0 and {data.Length}.", nameof(perplexity));

        int n = data.Length;
        int d = data[0].Length;

        // Step 1: Compute pairwise affinities in high-dimensional space (Student t-distribution)
        double[][] pMatrix = ComputeHighDimAffinities(data, n, perplexity);

        // Symmetrize: p_ij = (p_ij + p_ji) / 2n
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double avg = (pMatrix[i][j] + pMatrix[j][i]) / 2.0;
                pMatrix[i][j] = avg;
                pMatrix[j][i] = avg;
            }
        }

        // Scale by 1/n
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                pMatrix[i][j] /= n;

        // Step 2: Initialize low-dimensional embedding randomly
        var rng = new Random(seed);
        _embedding = new double[n][];
        for (int i = 0; i < n; i++)
        {
            _embedding[i] = new double[numDimensions];
            for (int j = 0; j < numDimensions; j++)
                _embedding[i][j] = rng.NextDouble() * 0.01 - 0.005;
        }

        // Momentum-based optimization
        double[][] velocity = new double[n][];
        for (int i = 0; i < n; i++)
            velocity[i] = new double[numDimensions];

        double[] gains = new double[n * numDimensions];
        for (int g = 0; g < gains.Length; g++)
            gains[g] = 1.0;

        double momentum = 0.5;
        double earlyExaggeration = 4.0;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Compute pairwise affinities in low-dimensional space (Student t-distribution with df=1)
            double[][] qMatrix = ComputeLowDimAffinities(_embedding, n, numDimensions);

            // Compute gradients
            double[][] grad = ComputeGradients(pMatrix, qMatrix, _embedding, n, numDimensions, earlyExaggeration);

            // Update embedding with adaptive gains and momentum
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < numDimensions; j++)
                {
                    int idx = i * numDimensions + j;
                    double g = grad[i][j];

                    // Adaptive gain: increase gain where gradient direction matches velocity, decrease otherwise
                    bool signMatch = (g > 0 && velocity[i][j] > 0) || (g < 0 && velocity[i][j] < 0);
                    if (signMatch)
                        gains[idx] += 0.2;
                    else
                        gains[idx] *= 0.8;

                    if (gains[idx] < MinGain)
                        gains[idx] = MinGain;

                    velocity[i][j] = momentum * velocity[i][j] - learningRate * gains[idx] * g;
                    _embedding[i][j] += velocity[i][j];
                }
            }

            // Learning rate scheduling: switch momentum at early exaggeration end
            if (iter == 250)
                momentum = 0.8;

            // Fade out early exaggeration
            if (iter == 250)
                earlyExaggeration = 1.0;
        }

        double totalVar = 0.0;
        for (int j = 0; j < numDimensions; j++)
        {
            double mean = 0.0;
            for (int i = 0; i < n; i++)
                mean += _embedding[i][j];
            mean /= n;

            double variance = 0.0;
            for (int i = 0; i < n; i++)
            {
                double diff = _embedding[i][j] - mean;
                variance += diff * diff;
            }
            totalVar += variance / n;
        }

        double[] explainedRatio = new double[numDimensions];
        for (int j = 0; j < numDimensions; j++)
        {
            double mean = 0.0;
            for (int i = 0; i < n; i++)
                mean += _embedding[i][j];
            mean /= n;

            double variance = 0.0;
            for (int i = 0; i < n; i++)
            {
                double diff = _embedding[i][j] - mean;
                variance += diff * diff;
            }
            explainedRatio[j] = totalVar > 0.0 ? variance / n / totalVar : 1.0 / numDimensions;
        }

        return new DimensionalityReductionResult
        {
            TransformedData = _embedding,
            ExplainedVarianceRatio = explainedRatio,
            Components = [],
            OriginalDimensions = d,
            ReducedDimensions = numDimensions
        };
    }

    /// <summary>Computes pairwise affinities in high-dimensional space using Gaussian kernel
    /// with binary search for the appropriate sigma to match target perplexity.</summary>
    /// <param name="data">High-dimensional data.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="targetPerplexity">Target perplexity value.</param>
    /// <returns>Conditional probability matrix (row-normalized affinities).</returns>
    private static double[][] ComputeHighDimAffinities(double[][] data, int n, double targetPerplexity)
    {
        double[][] pMatrix = new double[n][];
        double targetEntropy = System.Math.Log(targetPerplexity);

        for (int i = 0; i < n; i++)
        {
            pMatrix[i] = new double[n];

            // Binary search for sigma that gives desired perplexity
            double sigmaMin = 1e-10;
            double sigmaMax = double.MaxValue;
            double sigma = 1.0;

            for (int search = 0; search < 50; search++)
            {
                // Compute affinities with current sigma
                double[] distSq = new double[n];
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distSq[j] = 0.0;
                        continue;
                    }
                    distSq[j] = SquaredDistance(data[i], data[j]);
                }

                double twoSigmaSq = 2.0 * sigma * sigma;
                double maxDistSq = 0.0;
                for (int j = 0; j < n; j++)
                    if (distSq[j] > maxDistSq)
                        maxDistSq = distSq[j];

                double[] conditional = new double[n];
                double sumCond = 0.0;

                // Use log-sum-exp trick for numerical stability
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    conditional[j] = System.Math.Exp(-distSq[j] / twoSigmaSq);
                    sumCond += conditional[j];
                }

                if (sumCond < 1e-300)
                {
                    // sigma too small, increase it
                    sigmaMin = sigma;
                    sigma = sigmaMax == double.MaxValue ? sigma * 2.0 : (sigma + sigmaMax) / 2.0;
                    continue;
                }

                // Normalize and compute entropy
                double entropy = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    conditional[j] /= sumCond;
                    if (conditional[j] > 1e-20)
                        entropy -= conditional[j] * System.Math.Log(conditional[j]);
                }

                // Binary search update
                if (System.Math.Abs(entropy - targetEntropy) < 1e-4)
                    break;

                if (entropy > targetEntropy)
                {
                    // Need smaller perplexity (less spread) => larger sigma? No, larger entropy means sigma too large
                    sigmaMax = sigma;
                }
                else
                {
                    sigmaMin = sigma;
                }

                if (sigmaMax == double.MaxValue || sigmaMax < 0)
                    sigma *= 2.0;
                else
                    sigma = (sigma + sigmaMax) / 2.0;

                if (sigma < 1e-10)
                    sigma = 1e-10;
            }

            // Final computation with converged sigma
            double twoSigmaSqFinal = 2.0 * sigma * sigma;
            double sumFinal = 0.0;
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;
                double dSq = SquaredDistance(data[i], data[j]);
                pMatrix[i][j] = System.Math.Exp(-dSq / twoSigmaSqFinal);
                sumFinal += pMatrix[i][j];
            }

            if (sumFinal > 0.0)
            {
                for (int j = 0; j < n; j++)
                    pMatrix[i][j] /= sumFinal;
            }
        }

        return pMatrix;
    }

    /// <summary>Computes pairwise affinities in low-dimensional space using the Student t-distribution with one degree of freedom.</summary>
    /// <param name="embedding">Current low-dimensional embedding.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="d">Dimensionality of the embedding.</param>
    /// <returns>Affinity matrix (row-normalized).</returns>
    private static double[][] ComputeLowDimAffinities(double[][] embedding, int n, int d)
    {
        double[][] qMatrix = new double[n][];
        double[] numeratorMatrix = new double[n * n];

        // Compute pairwise distances and numerators
        for (int i = 0; i < n; i++)
        {
            qMatrix[i] = new double[n];
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    numeratorMatrix[i * n + j] = 0.0;
                    continue;
                }
                double distSq = SquaredDistanceLow(embedding[i], embedding[j], d);
                numeratorMatrix[i * n + j] = 1.0 / (1.0 + distSq);
            }
        }

        // Normalize each row
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
                sum += numeratorMatrix[i * n + j];

            if (sum > 0.0)
            {
                for (int j = 0; j < n; j++)
                    qMatrix[i][j] = numeratorMatrix[i * n + j] / sum;
            }
        }

        return qMatrix;
    }

    /// <summary>Computes the KL-divergence gradient between the high-d affinities P and low-d affinities Q.</summary>
    /// <param name="pMatrix">High-dimensional affinity matrix.</param>
    /// <param name="qMatrix">Low-dimensional affinity matrix.</param>
    /// <param name="embedding">Current embedding.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="d">Low-dimensional dimensionality.</param>
    /// <param name="exaggeration">Early exaggeration factor for P.</param>
    /// <returns>Gradient matrix for each point and dimension.</returns>
    private static double[][] ComputeGradients(
        double[][] pMatrix, double[][] qMatrix,
        double[][] embedding, int n, int d, double exaggeration)
    {
        double[][] grad = new double[n][];
        for (int i = 0; i < n; i++)
            grad[i] = new double[d];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;

                double pij = pMatrix[i][j] * exaggeration;
                double qij = System.Math.Max(qMatrix[i][j], 1e-12);
                double diff = pij - qij;

                // Student t-distribution gradient factor: (1 + ||y_i - y_j||^2)^{-1} * 4
                double distSq = SquaredDistanceLow(embedding[i], embedding[j], d);
                double factor = diff * 4.0 / (1.0 + distSq);

                for (int k = 0; k < d; k++)
                {
                    double component = (embedding[i][k] - embedding[j][k]) * factor;
                    grad[i][k] += component;
                }
            }
        }

        return grad;
    }

    /// <summary>Computes squared Euclidean distance between two high-dimensional vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Squared distance.</returns>
    private static double SquaredDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int j = 0; j < a.Length; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return sum;
    }

    /// <summary>Computes squared Euclidean distance between two low-dimensional vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="d">Dimensionality.</param>
    /// <returns>Squared distance.</returns>
    private static double SquaredDistanceLow(double[] a, double[] b, int d)
    {
        double sum = 0.0;
        for (int j = 0; j < d; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return sum;
    }
}
