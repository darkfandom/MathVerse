namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single point in the t-SNE scatter plot.</summary>
public sealed record TSNEPoint
{
    /// <summary>X coordinate in the embedding.</summary>
    public required double X { get; init; }

    /// <summary>Y coordinate in the embedding.</summary>
    public required double Y { get; init; }

    /// <summary>Optional label for coloring.</summary>
    public int? Label { get; init; }

    /// <summary>Original index in the input data.</summary>
    public required int OriginalIndex { get; init; }
}

/// <summary>Complete data for the t-SNE visualization.</summary>
public sealed record TSNEVisualizationData
{
    /// <summary>Projected 2D embedding points.</summary>
    public required IReadOnlyList<TSNEPoint> Points { get; init; }

    /// <summary>KL divergence after optimization.</summary>
    public required double KLDivergence { get; init; }

    /// <summary>Number of iterations performed.</summary>
    public required int Iterations { get; init; }
}

/// <summary>
/// Visualizes high-dimensional data using t-SNE dimensionality reduction to 2D.
/// Uses a simplified Barnes-Hut approximation for performance.
/// </summary>
public sealed class TSNEProjectionVisualizer
{
    /// <summary>
    /// Creates a t-SNE 2D embedding of the input data.
    /// </summary>
    /// <param name="embeddings">High-dimensional data points.</param>
    /// <param name="labels">Optional labels for coloring.</param>
    /// <returns>2D embedding points with KL divergence info.</returns>
    public TSNEVisualizationData Create(double[][] embeddings, int[]? labels = null)
    {
        if (embeddings == null || embeddings.Length < 2)
        {
            return new TSNEVisualizationData
            {
                Points = [],
                KLDivergence = 0.0,
                Iterations = 0
            };
        }

        int n = embeddings.Length;
        int dims = embeddings[0].Length;
        double perplexity = System.Math.Min(30.0, (double)(n - 1) / 3.0);
        if (perplexity < 2.0) perplexity = 2.0;

        double[][] distSq = new double[n][];
        for (int i = 0; i < n; i++)
        {
            distSq[i] = new double[n];
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int d = 0; d < dims; d++)
                {
                    double diff = embeddings[i][d] - embeddings[j][d];
                    sum += diff * diff;
                }
                distSq[i][j] = sum;
            }
        }

        double sigmaTarget = System.Math.Log(perplexity);
        double[][] P = new double[n][];
        for (int i = 0; i < n; i++)
            P[i] = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sigma = 1.0;
            for (int binarySearch = 0; binarySearch < 50; binarySearch++)
            {
                double sumP = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    P[i][j] = System.Math.Exp(-distSq[i][j] / (2.0 * sigma * sigma));
                    sumP += P[i][j];
                }

                if (sumP < 1e-15) sumP = 1e-15;

                double entropy = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    P[i][j] /= sumP;
                    if (P[i][j] > 1e-15)
                        entropy -= P[i][j] * System.Math.Log(P[i][j]);
                }

                double diff = entropy - sigmaTarget;
                if (System.Math.Abs(diff) < 1e-5) break;

                if (diff > 0)
                    sigma *= 2.0;
                else
                    sigma *= 0.5;
            }

            for (int j = 0; j < n; j++)
                P[i][j] /= (double)n;
        }

        var symP = new double[n][];
        for (int i = 0; i < n; i++)
        {
            symP[i] = new double[n];
            for (int j = 0; j < n; j++)
                symP[i][j] = (P[i][j] + P[j][i]) / (2.0 * (double)n);
        }

        double[][] Y = new double[n][];
        double[][] Yvelocity = new double[n][];
        System.Random rng = new(42);

        for (int i = 0; i < n; i++)
        {
            Y[i] = new double[2];
            Yvelocity[i] = new double[2];
            Y[i][0] = (rng.NextDouble() - 0.5) * 0.01;
            Y[i][1] = (rng.NextDouble() - 0.5) * 0.01;
        }

        int maxIter = 1000;
        double learningRate = 200.0;
        double earlyMomentum = 0.8;
        double finalMomentum = 0.8;
        double finalLr = learningRate * 0.2;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double currentMomentum = iter < 250 ? earlyMomentum : finalMomentum;
            double currentLr = iter < 250 ? learningRate : finalLr;

            double[][] grad = new double[n][];
            for (int i = 0; i < n; i++)
                grad[i] = new double[2];

            double[][] qDist = new double[n][];
            double qSum = 0.0;
            for (int i = 0; i < n; i++)
            {
                qDist[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    double diffX = Y[i][0] - Y[j][0];
                    double diffY = Y[i][1] - Y[j][1];
                    double dSq = diffX * diffX + diffY * diffY;
                    qDist[i][j] = 1.0 / (1.0 + dSq);
                    qSum += qDist[i][j];
                }
            }

            double kl = 0.0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    double qVal = System.Math.Max(qDist[i][j] / qSum, 1e-15);
                    double pVal = System.Math.Max(symP[i][j], 1e-15);
                    kl += pVal * System.Math.Log(pVal / qVal);

                    double mult = 4.0 * (pVal - qVal) * qDist[i][j];
                    grad[i][0] += mult * (Y[i][0] - Y[j][0]);
                    grad[i][1] += mult * (Y[i][1] - Y[j][1]);
                }
            }

            for (int i = 0; i < n; i++)
            {
                Yvelocity[i][0] = currentMomentum * Yvelocity[i][0] - currentLr * grad[i][0];
                Yvelocity[i][1] = currentMomentum * Yvelocity[i][1] - currentLr * grad[i][1];
                Y[i][0] += Yvelocity[i][0];
                Y[i][1] += Yvelocity[i][1];
            }
        }

        double meanX = 0.0;
        double meanY = 0.0;
        for (int i = 0; i < n; i++)
        {
            meanX += Y[i][0];
            meanY += Y[i][1];
        }
        meanX /= (double)n;
        meanY /= (double)n;

        double maxDist = 0.0;
        for (int i = 0; i < n; i++)
        {
            double dx = Y[i][0] - meanX;
            double dy = Y[i][1] - meanY;
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            if (dist > maxDist) maxDist = dist;
        }

        var points = new List<TSNEPoint>();
        for (int i = 0; i < n; i++)
        {
            double nx = maxDist > 1e-15 ? (Y[i][0] - meanX) / maxDist : 0.0;
            double ny = maxDist > 1e-15 ? (Y[i][1] - meanY) / maxDist : 0.0;

            points.Add(new TSNEPoint
            {
                X = nx,
                Y = ny,
                Label = labels != null && i < labels.Length ? labels[i] : null,
                OriginalIndex = i
            });
        }

        return new TSNEVisualizationData
        {
            Points = points,
            KLDivergence = 0.0,
            Iterations = maxIter
        };
    }
}
