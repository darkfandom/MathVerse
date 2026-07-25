namespace MathVerse.Math.AI.Probability;

using System;

/// <summary>Sequential Monte Carlo particle filter for non-linear/non-Gaussian state estimation.</summary>
public sealed class ParticleFilter
{
    private readonly int _stateDimension;
    private readonly int _observationDimension;
    private readonly Random _rng;

    /// <summary>Initializes a new particle filter.</summary>
    /// <param name="stateDimension">Dimension of the state space.</param>
    /// <param name="observationDimension">Dimension of the observation space.</param>
    /// <param name="seed">Random seed for reproducibility. Use -1 for non-deterministic.</param>
    public ParticleFilter(int stateDimension, int observationDimension, int seed = -1)
    {
        if (stateDimension <= 0)
            throw new ArgumentException("State dimension must be positive.", nameof(stateDimension));
        if (observationDimension <= 0)
            throw new ArgumentException("Observation dimension must be positive.", nameof(observationDimension));

        _stateDimension = stateDimension;
        _observationDimension = observationDimension;
        _rng = seed >= 0 ? new Random(seed) : new Random();
    }

    /// <summary>Runs the particle filter over a sequence of observations.</summary>
    /// <param name="initialState">Initial state estimate (mean).</param>
    /// <param name="observations">Sequence of observation vectors.</param>
    /// <param name="numParticles">Number of particles to use.</param>
    /// <returns>Array of filtered state estimates (weighted mean of particles at each step).</returns>
    public double[][] Filter(double[] initialState, double[][] observations, int numParticles = 1000)
    {
        if (initialState == null)
            throw new ArgumentNullException(nameof(initialState));
        if (observations == null)
            throw new ArgumentNullException(nameof(observations));
        if (numParticles <= 0)
            throw new ArgumentException("Number of particles must be positive.", nameof(numParticles));

        double[][] particles = InitializeParticles(initialState, numParticles, 0.5);
        double[] weights = new double[numParticles];
        double weightPerParticle = 1.0 / numParticles;
        for (int i = 0; i < numParticles; i++)
            weights[i] = weightPerParticle;

        double[][] results = new double[observations.Length + 1][];
        results[0] = WeightedMean(particles, weights);

        for (int t = 0; t < observations.Length; t++)
        {
            particles = PropagateParticles(particles, 1.0);

            weights = UpdateWeights(particles, observations[t]);
            double weightSum = 0.0;
            for (int i = 0; i < numParticles; i++)
                weightSum += weights[i];

            if (weightSum > 0.0)
            {
                for (int i = 0; i < numParticles; i++)
                    weights[i] /= weightSum;
            }
            else
            {
                for (int i = 0; i < numParticles; i++)
                    weights[i] = weightPerParticle;
            }

            double ess = EffectiveSampleSize(weights);
            if (ess < numParticles / 2.0)
            {
                particles = SystematicResample(particles, weights);
                for (int i = 0; i < numParticles; i++)
                    weights[i] = weightPerParticle;
            }

            results[t + 1] = WeightedMean(particles, weights);
        }

        return results;
    }

    /// <summary>Computes the effective sample size to determine when resampling is needed.</summary>
    /// <param name="weights">Normalized weight vector.</param>
    /// <returns>Effective sample size value.</returns>
    public double EffectiveSampleSize(double[] weights)
    {
        if (weights == null || weights.Length == 0)
            return 0.0;

        double sumSq = 0.0;
        double sum = 0.0;
        for (int i = 0; i < weights.Length; i++)
        {
            sumSq += weights[i] * weights[i];
            sum += weights[i];
        }

        if (System.Math.Abs(sum) < 1e-15)
            return 0.0;

        double normalizedSumSq = sumSq / (sum * sum);
        return 1.0 / normalizedSumSq;
    }

    /// <summary>Performs systematic resampling of particles.</summary>
    /// <param name="particles">Current particle states.</param>
    /// <param name="weights">Particle weights (normalized).</param>
    /// <returns>Resampled particles.</returns>
    public double[][] SystematicResample(double[][] particles, double[] weights)
    {
        int n = particles.Length;
        double[][] resampled = new double[n][];

        double[] cumulative = new double[n];
        cumulative[0] = weights[0];
        for (int i = 1; i < n; i++)
            cumulative[i] = cumulative[i - 1] + weights[i];

        double u = _rng.NextDouble() / n;
        int idx = 0;

        for (int i = 0; i < n; i++)
        {
            double target = u + (double)i / n;
            while (idx < n - 1 && cumulative[idx] < target)
                idx++;
            resampled[i] = (double[])particles[idx].Clone();
        }

        return resampled;
    }

    /// <summary>Computes the particle cloud covariance matrix.</summary>
    /// <param name="particles">Particle states.</param>
    /// <param name="weights">Particle weights.</param>
    /// <returns>Covariance matrix.</returns>
    public double[][] ComputeCovariance(double[][] particles, double[] weights)
    {
        int n = particles.Length;
        int d = _stateDimension;

        double[] mean = WeightedMean(particles, weights);
        double[][] cov = new double[d][];
        for (int i = 0; i < d; i++)
            cov[i] = new double[d];

        for (int p = 0; p < n; p++)
        {
            for (int i = 0; i < d; i++)
            {
                for (int j = 0; j < d; j++)
                {
                    cov[i][j] += weights[p] * (particles[p][i] - mean[i]) * (particles[p][j] - mean[j]);
                }
            }
        }

        return cov;
    }

    private double[][] InitializeParticles(double[] mean, int numParticles, double spread)
    {
        double[][] particles = new double[numParticles][];
        for (int i = 0; i < numParticles; i++)
        {
            particles[i] = new double[_stateDimension];
            for (int d = 0; d < _stateDimension; d++)
            {
                double u1 = _rng.NextDouble();
                double u2 = _rng.NextDouble();
                double z = System.Math.Sqrt(-2.0 * System.Math.Log(u1 + 1e-300)) * System.Math.Cos(2.0 * System.Math.PI * u2);
                particles[i][d] = mean[d] + spread * z;
            }
        }
        return particles;
    }

    private double[][] PropagateParticles(double[][] particles, double noiseStd)
    {
        int n = particles.Length;
        double[][] propagated = new double[n][];

        for (int i = 0; i < n; i++)
        {
            propagated[i] = new double[_stateDimension];
            for (int d = 0; d < _stateDimension; d++)
            {
                double u1 = _rng.NextDouble();
                double u2 = _rng.NextDouble();
                double z = System.Math.Sqrt(-2.0 * System.Math.Log(u1 + 1e-300)) * System.Math.Cos(2.0 * System.Math.PI * u2);
                propagated[i][d] = particles[i][d] + noiseStd * z;
            }
        }

        return propagated;
    }

    private double[] UpdateWeights(double[][] particles, double[] observation)
    {
        int n = particles.Length;
        double[] weights = new double[n];

        for (int i = 0; i < n; i++)
        {
            double logLikelihood = 0.0;
            for (int d = 0; d < _observationDimension; d++)
            {
                double diff = (d < particles[i].Length ? particles[i][d] : 0.0) - (d < observation.Length ? observation[d] : 0.0);
                logLikelihood -= 0.5 * diff * diff;
            }
            weights[i] = System.Math.Exp(logLikelihood);
        }

        return weights;
    }

    private static double[] WeightedMean(double[][] particles, double[] weights)
    {
        int n = particles.Length;
        int d = particles[0].Length;
        double[] mean = new double[d];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < d; j++)
                mean[j] += weights[i] * particles[i][j];
        }

        return mean;
    }
}
