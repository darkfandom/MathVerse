namespace MathVerse.Math.AI.Optimization;

/// <summary>Particle swarm optimizer with inertia weight and cognitive/social coefficients.</summary>
public sealed class ParticleSwarmOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "ParticleSwarm";

    /// <summary>Runs particle swarm optimization.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector (used for dimensionality; swarm is initialized randomly around this).</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var rng = new Random(opts.RandomSeed);
        int n = initial.Length;
        int swarmSize = System.Math.Max(20, n * 4);
        double w = 0.7;
        double c1 = 1.5;
        double c2 = 1.5;
        var positions = new double[swarmSize][];
        var velocities = new double[swarmSize][];
        var personalBest = new double[swarmSize][];
        var personalBestVal = new double[swarmSize];
        double[] globalBest = (double[])initial.Clone();
        double globalBestVal = objective(initial);

        for (int p = 0; p < swarmSize; p++)
        {
            positions[p] = new double[n];
            velocities[p] = new double[n];
            personalBest[p] = new double[n];
            for (int i = 0; i < n; i++)
            {
                double range = System.Math.Max(System.Math.Abs(initial[i]) * 0.5, 1.0);
                positions[p][i] = initial[i] + (rng.NextDouble() * 2.0 - 1.0) * range;
                velocities[p][i] = (rng.NextDouble() * 2.0 - 1.0) * range * 0.1;
                personalBest[p][i] = positions[p][i];
            }
            personalBestVal[p] = objective(positions[p]);
            if (personalBestVal[p] < globalBestVal)
            {
                globalBestVal = personalBestVal[p];
                for (int i = 0; i < n; i++) globalBest[i] = positions[p][i];
            }
        }

        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            for (int p = 0; p < swarmSize; p++)
            {
                for (int i = 0; i < n; i++)
                {
                    double r1 = rng.NextDouble();
                    double r2 = rng.NextDouble();
                    velocities[p][i] = w * velocities[p][i]
                        + c1 * r1 * (personalBest[p][i] - positions[p][i])
                        + c2 * r2 * (globalBest[i] - positions[p][i]);
                    double vMax = System.Math.Max(System.Math.Abs(globalBest[i]) * 0.5, 1.0);
                    velocities[p][i] = System.Math.Max(-vMax, System.Math.Min(vMax, velocities[p][i]));
                    positions[p][i] += velocities[p][i];
                }

                GradientDescentOptimizer.ClampToBounds(positions[p], opts.LowerBounds, opts.UpperBounds);

                double val = objective(positions[p]);
                if (val < personalBestVal[p])
                {
                    personalBestVal[p] = val;
                    for (int i = 0; i < n; i++) personalBest[p][i] = positions[p][i];
                }
                if (val < globalBestVal)
                {
                    globalBestVal = val;
                    for (int i = 0; i < n; i++) globalBest[i] = positions[p][i];
                }
            }

            double spread = 0.0;
            for (int p = 0; p < swarmSize; p++)
            {
                for (int i = 0; i < n; i++)
                {
                    double diff = positions[p][i] - globalBest[i];
                    spread += diff * diff;
                }
            }
            spread = System.Math.Sqrt(spread / swarmSize);
            if (spread < opts.Tolerance)
            {
                converged = true;
                break;
            }
        }

        sw.Stop();
        return new OptimizationResult
        {
            Success = true,
            BestParameters = globalBest,
            BestValue = globalBestVal,
            IterationsExecuted = iter,
            Converged = converged,
            ElapsedTime = sw.Elapsed
        };
    }
}
