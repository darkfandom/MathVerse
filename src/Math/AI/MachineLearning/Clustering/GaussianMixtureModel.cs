namespace MathVerse.Math.AI.MachineLearning.Clustering;

using System;

/// <summary>Gaussian Mixture Model clustering using the Expectation-Maximization algorithm.</summary>
public sealed class GaussianMixtureModel
{
    private const double Tolerance = 1e-6;
    private const double MinVariance = 1e-6;

    private double[][] _means = [];
    private double[][] _variances = [];
    private double[] _weights = [];
    private int _numComponents;
    private double _logLikelihood;

    /// <summary>Gets the estimated mean vectors for each Gaussian component.</summary>
    public double[][] Means => _means;

    /// <summary>Gets the estimated diagonal variance vectors for each Gaussian component.</summary>
    public double[][] Variances => _variances;

    /// <summary>Gets the estimated mixing coefficients (prior probabilities) for each component.</summary>
    public double[] Weights => _weights;

    /// <summary>Gets the final log-likelihood of the data given the model.</summary>
    public double LogLikelihood => _logLikelihood;

    /// <summary>Fits a Gaussian Mixture Model to the data using the EM algorithm.</summary>
    /// <param name="data">Array of data points.</param>
    /// <param name="numComponents">Number of Gaussian components.</param>
    /// <param name="maxIterations">Maximum number of EM iterations.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A <see cref="ClusteringResult"/> with hard cluster labels and component parameters.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public ClusteringResult Fit(double[][] data, int numComponents, int maxIterations = 100, int seed = 42)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (numComponents <= 0 || numComponents > data.Length)
            throw new ArgumentException($"numComponents must be between 1 and {data.Length}.", nameof(numComponents));

        int n = data.Length;
        int d = data[0].Length;
        _numComponents = numComponents;

        InitializeParameters(data, seed);

        double prevLogLikelihood = double.MinValue;
        _logLikelihood = double.MinValue;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // E-step: compute responsibilities
            double[][] responsibilities = EStep(data, n, numComponents);

            // M-step: update parameters
            MStep(data, responsibilities, n, d, numComponents);

            // Compute log-likelihood
            _logLikelihood = ComputeLogLikelihood(data, n, numComponents);

            // Check convergence
            if (System.Math.Abs(_logLikelihood - prevLogLikelihood) < Tolerance)
            {
                break;
            }

            prevLogLikelihood = _logLikelihood;
        }

        // Assign hard labels
        int[] labels = new int[n];
        for (int i = 0; i < n; i++)
        {
            double maxResp = 0.0;
            int bestComponent = 0;
            for (int k = 0; k < numComponents; k++)
            {
                double resp = ComputeResponsibility(data[i], k);
                if (resp > maxResp)
                {
                    maxResp = resp;
                    bestComponent = k;
                }
            }
            labels[i] = bestComponent;
        }

        // Compute centroids as component means
        double[][] centroids = new double[numComponents][];
        for (int k = 0; k < numComponents; k++)
        {
            centroids[k] = (double[])_means[k].Clone();
        }

        return new ClusteringResult
        {
            Labels = labels,
            Centroids = centroids,
            NumClusters = numComponents,
            Inertia = ComputeInertia(data, labels, centroids),
            IterationsExecuted = maxIterations,
            Converged = System.Math.Abs(_logLikelihood - prevLogLikelihood) < Tolerance
        };
    }

    /// <summary>Computes the posterior probability of each component given each data point (E-step).</summary>
    /// <param name="data">The data points.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="k">Number of components.</param>
    /// <returns>Responsibility matrix [n x k].</returns>
    private double[][] EStep(double[][] data, int n, int k)
    {
        double[][] responsibilities = new double[n][];
        for (int i = 0; i < n; i++)
        {
            responsibilities[i] = new double[k];
            double totalResp = 0.0;
            for (int c = 0; c < k; c++)
            {
                responsibilities[i][c] = _weights[c] * GaussPDF(data[i], _means[c], _variances[c]);
                totalResp += responsibilities[i][c];
            }
            for (int c = 0; c < k; c++)
            {
                responsibilities[i][c] = totalResp > 0.0 ? responsibilities[i][c] / totalResp : 1.0 / k;
            }
        }
        return responsibilities;
    }

    /// <summary>Updates model parameters using the current responsibilities (M-step).</summary>
    /// <param name="data">The data points.</param>
    /// <param name="responsibilities">Current responsibility matrix.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="d">Dimensionality.</param>
    /// <param name="k">Number of components.</param>
    private void MStep(double[][] data, double[][] responsibilities, int n, int d, int k)
    {
        for (int c = 0; c < k; c++)
        {
            // Effective number of points assigned to this component
            double Nk = 0.0;
            for (int i = 0; i < n; i++)
                Nk += responsibilities[i][c];

            if (Nk < 1e-12)
                continue;

            // Update mixing coefficient
            _weights[c] = Nk / n;

            // Update mean
            for (int j = 0; j < d; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++)
                    sum += responsibilities[i][c] * data[i][j];
                _means[c][j] = sum / Nk;
            }

            // Update diagonal variance
            for (int j = 0; j < d; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    double diff = data[i][j] - _means[c][j];
                    sum += responsibilities[i][c] * diff * diff;
                }
                _variances[c][j] = sum / Nk;
                if (_variances[c][j] < MinVariance)
                    _variances[c][j] = MinVariance;
            }
        }
    }

    /// <summary>Computes the diagonal multivariate Gaussian probability density function.</summary>
    /// <param name="x">Data point.</param>
    /// <param name="mean">Mean vector.</param>
    /// <param name="variance">Diagonal variance vector.</param>
    /// <returns>Probability density value.</returns>
    private static double GaussPDF(double[] x, double[] mean, double[] variance)
    {
        int d = x.Length;
        double logDet = 0.0;
        double mahalDist = 0.0;

        for (int j = 0; j < d; j++)
        {
            logDet += System.Math.Log(variance[j]);
            double diff = x[j] - mean[j];
            mahalDist += (diff * diff) / variance[j];
        }

        double logPdf = -0.5 * (d * System.Math.Log(2.0 * System.Math.PI) + logDet + mahalDist);
        return System.Math.Exp(logPdf);
    }

    /// <summary>Computes the responsibility of a specific component for a single data point.</summary>
    /// <param name="x">Data point.</param>
    /// <param name="component">Component index.</param>
    /// <returns>Posterior probability (responsibility).</returns>
    private double ComputeResponsibility(double[] x, int component)
    {
        double totalResp = 0.0;
        for (int c = 0; c < _numComponents; c++)
        {
            totalResp += _weights[c] * GaussPDF(x, _means[c], _variances[c]);
        }
        double componentResp = _weights[component] * GaussPDF(x, _means[component], _variances[component]);
        return totalResp > 0.0 ? componentResp / totalResp : 1.0 / _numComponents;
    }

    /// <summary>Computes the log-likelihood of the data under the current model.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="k">Number of components.</param>
    /// <returns>Total log-likelihood.</returns>
    private double ComputeLogLikelihood(double[][] data, int n, int k)
    {
        double logLik = 0.0;
        for (int i = 0; i < n; i++)
        {
            double pdf = 0.0;
            for (int c = 0; c < k; c++)
                pdf += _weights[c] * GaussPDF(data[i], _means[c], _variances[c]);

            if (pdf > 0.0)
                logLik += System.Math.Log(pdf);
            else
                logLik += -700.0; // Approximation for log of very small number
        }
        return logLik;
    }

    /// <summary>Initializes means, variances, and weights using random data selection and uniform priors.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="seed">Random seed.</param>
    private void InitializeParameters(double[][] data, int seed)
    {
        int n = data.Length;
        int d = data[0].Length;
        var rng = new Random(seed);

        _means = new double[_numComponents][];
        _variances = new double[_numComponents][];
        _weights = new double[_numComponents];

        // Select random data points as initial means
        bool[] selected = new bool[n];
        for (int c = 0; c < _numComponents; c++)
        {
            int idx;
            do { idx = rng.Next(n); } while (selected[idx]);
            selected[idx] = true;
            _means[c] = (double[])data[idx].Clone();
        }

        // Compute data variance for initial component variances
        double[] dataMean = new double[d];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < d; j++)
                dataMean[j] += data[i][j];
        for (int j = 0; j < d; j++)
            dataMean[j] /= n;

        double[] dataVar = new double[d];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < d; j++)
            {
                double diff = data[i][j] - dataMean[j];
                dataVar[j] += diff * diff;
            }

        for (int j = 0; j < d; j++)
            dataVar[j] /= n;

        // Initialize all variances to data variance and uniform weights
        for (int c = 0; c < _numComponents; c++)
        {
            _variances[c] = new double[d];
            for (int j = 0; j < d; j++)
                _variances[c][j] = dataVar[j] < MinVariance ? 1.0 : dataVar[j];
            _weights[c] = 1.0 / _numComponents;
        }
    }

    /// <summary>Computes within-cluster sum of squares.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="labels">Cluster labels.</param>
    /// <param name="centroids">Centroid positions.</param>
    /// <returns>Total inertia.</returns>
    private static double ComputeInertia(double[][] data, int[] labels, double[][] centroids)
    {
        double inertia = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            double sum = 0.0;
            double[] c = centroids[labels[i]];
            for (int j = 0; j < data[i].Length; j++)
            {
                double diff = data[i][j] - c[j];
                sum += diff * diff;
            }
            inertia += sum;
        }
        return inertia;
    }
}
