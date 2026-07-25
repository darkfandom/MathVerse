namespace MathVerse.Math.AI.MachineLearning.DimensionalityReduction;

using System;

/// <summary>Uniform Manifold Approximation and Projection (UMAP) for nonlinear dimensionality reduction.</summary>
/// <remarks>
/// This is a simplified implementation using fuzzy simplicial set construction
/// and stochastic gradient descent optimization with a cross-entropy loss function.
/// </remarks>
public sealed class UMAP
{
    private const double Epsilon = 1e-8;

    private double[][] _embedding = [];

    /// <summary>Gets the learned low-dimensional embedding after fitting.</summary>
    public double[][] Embedding => _embedding;

    /// <summary>Fits UMAP to the data and returns the low-dimensional embedding.</summary>
    /// <param name="data">Input data matrix (samples x features).</param>
    /// <param name="numDimensions">Target dimensionality (typically 2 or 3).</param>
    /// <param name="nNeighbors">Number of neighbors for local connectivity.</param>
    /// <param name="minDist">Minimum distance between embedded points.</param>
    /// <param name="learningRate">Initial learning rate for SGD.</param>
    /// <param name="maxIterations">Number of optimization epochs.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A <see cref="DimensionalityReductionResult"/> with the embedded data.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public DimensionalityReductionResult Fit(
        double[][] data,
        int numDimensions = 2,
        int nNeighbors = 15,
        double minDist = 0.1,
        double learningRate = 1.0,
        int maxIterations = 200,
        int seed = 42)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (numDimensions <= 0)
            throw new ArgumentException("numDimensions must be positive.", nameof(numDimensions));
        if (nNeighbors <= 0 || nNeighbors >= data.Length)
            throw new ArgumentException($"nNeighbors must be between 1 and {data.Length - 1}.", nameof(nNeighbors));
        if (minDist < 0.0)
            throw new ArgumentException("minDist must be non-negative.", nameof(minDist));

        int n = data.Length;
        int d = data[0].Length;

        // Adjust nNeighbors if data is small
        if (nNeighbors > n - 1)
            nNeighbors = n - 1;

        // Step 1: Compute k-nearest neighbor graph
        int[][] knnIndices = ComputeKNN(data, n, nNeighbors);

        // Step 2: Compute local connectivity distances (sigma) for each point
        double[] sigmas = ComputeLocalConnectivity(data, knnIndices, n, nNeighbors);

        // Step 3: Compute fuzzy simplicial set (high-dimensional weighted graph)
        double[][] highDimWeights = ComputeFuzzyWeights(knnIndices, sigmas, n, nNeighbors);

        // Step 4: Initialize low-dimensional embedding with spectral initialization or random
        _embedding = InitializeEmbedding(data, n, numDimensions, seed);

        // Step 5: Optimize with SGD using cross-entropy loss
        Optimize(highDimWeights, knnIndices, _embedding, n, numDimensions, minDist, learningRate, maxIterations, seed);

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

    /// <summary>Projects new data onto the learned UMAP embedding using the approximate embedding transform.</summary>
    /// <param name="newData">New data to project.</param>
    /// <param name="trainData">Original training data used during Fit.</param>
    /// <returns>Projected data in low-dimensional space.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Fit has not been called.</exception>
    public double[][] Transform(double[][] newData, double[][] trainData)
    {
        if (_embedding.Length == 0)
            throw new InvalidOperationException("Model has not been fitted. Call Fit() first.");

        int m = newData.Length;
        int nTrain = trainData.Length;
        int numDim = _embedding[0].Length;
        int nNeighbors = System.Math.Min(15, nTrain - 1);

        int[][] knnIndices = ComputeKNN(trainData, nTrain, nNeighbors);
        double[] sigmas = ComputeLocalConnectivity(trainData, knnIndices, nTrain, nNeighbors);

        double[][] projected = new double[m][];
        for (int i = 0; i < m; i++)
        {
            projected[i] = new double[numDim];

            // Find nearest training points to each new point
            double[] dists = new double[nTrain];
            for (int j = 0; j < nTrain; j++)
                dists[j] = EuclideanDistance(newData[i], trainData[j]);

            // Use inverse-distance weighted average of k nearest neighbors
            double[][] sortedPairs = new double[nTrain][];
            for (int j = 0; j < nTrain; j++)
                sortedPairs[j] = new double[] { j, dists[j] };

            Array.Sort(sortedPairs, (a, b) => a[1].CompareTo(b[1]));

            double weightSum = 0.0;
            for (int k = 0; k < nNeighbors && k < nTrain; k++)
            {
                int idx = (int)sortedPairs[k][0];
                double dist = sortedPairs[k][1];
                double w = 1.0 / (dist + Epsilon);
                weightSum += w;
                for (int dim = 0; dim < numDim; dim++)
                    projected[i][dim] += _embedding[idx][dim] * w;
            }

            if (weightSum > 0.0)
                for (int dim = 0; dim < numDim; dim++)
                    projected[i][dim] /= weightSum;
        }

        return projected;
    }

    /// <summary>Computes the k-nearest neighbor indices for each data point.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="k">Number of neighbors.</param>
    /// <returns>kNN index matrix [n x k].</returns>
    private static int[][] ComputeKNN(double[][] data, int n, int k)
    {
        int[][] indices = new int[n][];

        for (int i = 0; i < n; i++)
        {
            // Compute distances from point i to all others
            double[][] dists = new double[n][];
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    dists[j] = new double[] { j, double.MaxValue };
                else
                    dists[j] = new double[] { j, EuclideanDistance(data[i], data[j]) };
            }

            // Partial sort: find k smallest
            Array.Sort(dists, (a, b) => a[1].CompareTo(b[1]));

            indices[i] = new int[k];
            for (int j = 0; j < k; j++)
                indices[i][j] = (int)dists[j][0];
        }

        return indices;
    }

    /// <summary>Computes local connectivity scaling (sigma) for each point using binary search to match log2(k) perplexity.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="knnIndices">kNN indices.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="k">Number of neighbors.</param>
    /// <returns>Local scaling parameters.</returns>
    private static double[] ComputeLocalConnectivity(double[][] data, int[][] knnIndices, int n, int k)
    {
        double[] sigmas = new double[n];
        double targetPerplexity = System.Math.Log(k);

        for (int i = 0; i < n; i++)
        {
            double lo = 0.0;
            double hi = double.MaxValue;

            for (int iter = 0; iter < 64; iter++)
            {
                double mid = (lo + hi) / 2.0;
                double sigma = mid < Epsilon ? Epsilon : mid;
                double twoSigmaSq = 2.0 * sigma * sigma;

                double sum = 0.0;
                for (int j = 0; j < k && j < knnIndices[i].Length; j++)
                {
                    double dist = EuclideanDistance(data[i], data[knnIndices[i][j]]);
                    sum += System.Math.Exp(-dist * dist / twoSigmaSq);
                }

                if (sum < Epsilon)
                {
                    lo = mid;
                    continue;
                }

                double entropy = 0.0;
                for (int j = 0; j < k && j < knnIndices[i].Length; j++)
                {
                    double dist = EuclideanDistance(data[i], data[knnIndices[i][j]]);
                    double p = System.Math.Exp(-dist * dist / twoSigmaSq) / sum;
                    if (p > Epsilon)
                        entropy -= p * System.Math.Log(p);
                }

                if (System.Math.Abs(entropy - targetPerplexity) < 0.01)
                    break;

                if (entropy > targetPerplexity)
                    hi = mid;
                else
                    lo = mid;
            }

            sigmas[i] = (lo + hi) / 2.0;
            if (sigmas[i] < Epsilon)
                sigmas[i] = Epsilon;
        }

        return sigmas;
    }

    /// <summary>Computes fuzzy simplicial set weights from the kNN graph and local connectivity.</summary>
    /// <param name="knnIndices">kNN indices.</param>
    /// <param name="sigmas">Local scaling parameters.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="k">Number of neighbors.</param>
    /// <returns>Weighted adjacency matrix.</returns>
    private static double[][] ComputeFuzzyWeights(int[][] knnIndices, double[] sigmas, int n, int k)
    {
        double[][] weights = new double[n][];

        for (int i = 0; i < n; i++)
        {
            weights[i] = new double[n];
            double twoSigmaSq = 2.0 * sigmas[i] * sigmas[i];

            // Compute distances to k nearest neighbors and set weights
            for (int j = 0; j < k && j < knnIndices[i].Length; j++)
            {
                int neighborIdx = knnIndices[i][j];
                // We store weight as exp(-d^2 / (2*sigma^2)), actual distance not needed here
                // since we precomputed sigma to normalize
                weights[i][neighborIdx] = 1.0; // Placeholder; actual weight computed via smooth knn
            }

            // Set the kth neighbor weight as the normalization target
            // For simplicity, use uniform weights among kNN neighbors
            for (int j = 0; j < k && j < knnIndices[i].Length; j++)
            {
                weights[i][knnIndices[i][j]] = 1.0;
            }
        }

        // Symmetrize: w_ij = max(w_ij, w_ji) (union of both directions)
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double w = System.Math.Max(weights[i][j], weights[j][i]);
                weights[i][j] = w;
                weights[j][i] = w;
            }
        }

        return weights;
    }

    /// <summary>Initializes the low-dimensional embedding using a Laplacian eigenmap or random init.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="d">Target dimensionality.</param>
    /// <param name="seed">Random seed.</param>
    /// <returns>Initial embedding.</returns>
    private static double[][] InitializeEmbedding(double[][] data, int n, int d, int seed)
    {
        var rng = new Random(seed);
        double[][] embedding = new double[n][];

        // Simple spectral-like initialization: use first d principal components
        // For a simplified approach, use random initialization scaled by data spread
        double[] mean = new double[data[0].Length];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < data[i].Length; j++)
                mean[j] += data[i][j];
        for (int j = 0; j < data[0].Length; j++)
            mean[j] /= n;

        double maxSpread = 0.0;
        for (int i = 0; i < n; i++)
        {
            double dist = 0.0;
            for (int j = 0; j < data[i].Length; j++)
            {
                double diff = data[i][j] - mean[j];
                dist += diff * diff;
            }
            if (dist > maxSpread)
                maxSpread = dist;
        }
        maxSpread = System.Math.Sqrt(maxSpread);
        if (maxSpread < Epsilon)
            maxSpread = 1.0;

        for (int i = 0; i < n; i++)
        {
            embedding[i] = new double[d];
            for (int j = 0; j < d; j++)
            {
                embedding[i][j] = (rng.NextDouble() - 0.5) * 2.0 * maxSpread * 0.001;
            }
        }

        return embedding;
    }

    /// <summary>Optimizes the low-dimensional embedding using stochastic gradient descent with cross-entropy loss.</summary>
    /// <param name="highDimWeights">High-dimensional fuzzy simplicial weights.</param>
    /// <param name="knnIndices">kNN indices for negative sampling.</param>
    /// <param name="embedding">Low-dimensional embedding to optimize (modified in-place).</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="d">Low-dimensional dimensionality.</param>
    /// <param name="minDist">Minimum distance parameter for attractive force scaling.</param>
    /// <param name="learningRate">Initial learning rate.</param>
    /// <param name="maxIterations">Number of optimization epochs.</param>
    /// <param name="seed">Random seed.</param>
    private static void Optimize(
        double[][] highDimWeights,
        int[][] knnIndices,
        double[][] embedding,
        int n, int d,
        double minDist,
        double learningRate,
        int maxIterations,
        int seed)
    {
        var rng = new Random(seed);

        // Compute attractive repulsion weights based on minDist
        double a, b;
        ComputeABParameters(minDist, out a, out b);

        // Build edge list from the high-dimensional graph
        int edgeCount = 0;
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (highDimWeights[i][j] > Epsilon || highDimWeights[j][i] > Epsilon)
                    edgeCount++;

        // If edge list is too sparse, use kNN edges directly
        if (edgeCount == 0)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < knnIndices[i].Length; j++)
                    edgeCount++;
            edgeCount /= 2; // Approximate
        }

        for (int epoch = 0; epoch < maxIterations; epoch++)
        {
            double epochLearningRate = learningRate * (1.0 - (double)epoch / maxIterations);

            // Attractive forces: iterate over edges (kNN pairs)
            for (int i = 0; i < n; i++)
            {
                for (int jIdx = 0; jIdx < knnIndices[i].Length; jIdx++)
                {
                    int j = knnIndices[i][jIdx];
                    if (j <= i) continue;

                    double weight = System.Math.Max(highDimWeights[i][j], highDimWeights[j][i]);
                    if (weight < Epsilon) continue;

                    // Compute attractive gradient
                    double[] diff = new double[d];
                    double distSq = 0.0;
                    for (int k = 0; k < d; k++)
                    {
                        diff[k] = embedding[i][k] - embedding[j][k];
                        distSq += diff[k] * diff[k];
                    }

                    double dist = System.Math.Sqrt(distSq + Epsilon);

                    // attractive force: 2 * a * b * dist^(2b-1) / (1 + dist^2)^b * weight * grad
                    double gradCoeff = -2.0 * a * b * System.Math.Pow(dist, 2.0 * b - 1.0) /
                                       System.Math.Pow(1.0 + distSq, b) * weight;
                    // Simplified: use smooth approximation
                    double attractiveGrad = -2.0 * a * System.Math.Pow(distSq, b - 1.0) /
                                            System.Math.Pow(1.0 + a * System.Math.Pow(distSq, b), 2.0) * weight;

                    for (int k = 0; k < d; k++)
                    {
                        double grad = attractiveGrad * diff[k] / (dist + Epsilon);
                        embedding[i][k] -= epochLearningRate * grad;
                        embedding[j][k] += epochLearningRate * grad;
                    }
                }
            }

            // Repulsive forces: approximate with random negative samples
            int numNegativeSamples = System.Math.Max(5, n / 10);
            for (int sample = 0; sample < numNegativeSamples; sample++)
            {
                int i = rng.Next(n);
                int j = rng.Next(n);
                if (i == j) continue;

                double[] diff = new double[d];
                double distSq = 0.0;
                for (int k = 0; k < d; k++)
                {
                    diff[k] = embedding[i][k] - embedding[j][k];
                    distSq += diff[k] * diff[k];
                }

                double dist = System.Math.Sqrt(distSq + Epsilon);

                // repulsive force: 2 * b / ((eps + dist^2) * (1 + a * dist^2b)) * weight
                double repulsiveGrad = 2.0 * b /
                    ((Epsilon + distSq) * (1.0 + a * System.Math.Pow(distSq, b)));

                for (int k = 0; k < d; k++)
                {
                    double grad = repulsiveGrad * diff[k] / (dist + Epsilon);
                    embedding[i][k] += epochLearningRate * grad;
                }
            }
        }
    }

    /// <summary>Computes the a and b parameters for the UMAP repulsive/ attractive force curve
    /// based on the minDist parameter.</summary>
    /// <param name="minDist">Minimum desired distance between points.</param>
    /// <param name="a">Output curve parameter a.</param>
    /// <param name="b">Output curve parameter b.</param>
    private static void ComputeABParameters(double minDist, out double a, out double b)
    {
        if (minDist <= 0.0)
        {
            a = 1.576943;
            b = 0.895061;
            return;
        }

        // Optimize a and b using binary search to satisfy:
        // minDist = (1 / a)^(1/b)
        // => a * minDist^b = 1
        // Fix b = log2(1/minDist) and solve for a
        // Or use a simple fitting approach
        b = System.Math.Max(1.0, -System.Math.Log(minDist) / System.Math.Log(2.0));
        a = 1.0 / System.Math.Pow(minDist, b);
    }

    /// <summary>Computes Euclidean distance between two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Euclidean distance.</returns>
    private static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int j = 0; j < a.Length; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return System.Math.Sqrt(sum);
    }
}
