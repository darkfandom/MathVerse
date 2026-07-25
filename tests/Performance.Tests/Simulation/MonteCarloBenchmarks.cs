namespace MathVerse.Performance.Tests.Simulation;

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Simulation.MonteCarlo;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

[MemoryDiagnoser]
public class MonteCarloBenchmarks
{
    private MonteCarloOptions _defaultOpts = null!;
    private MonteCarloOptions _highSampleOpts = null!;

    [GlobalSetup]
    public void Setup()
    {
        _defaultOpts = new MonteCarloOptions { Samples = 10000 };
        _highSampleOpts = new MonteCarloOptions { Samples = 100000 };
    }

    [Benchmark] public MonteCarloResult Integrate_LinearFunction() => MonteCarloEngine.Integrate(x => x, 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_QuadraticFunction() => MonteCarloEngine.Integrate(x => x * x, 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_SinusoidalFunction() => MonteCarloEngine.Integrate(x => System.Math.Sin(x), 0.0, System.Math.PI, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_HighSample() => MonteCarloEngine.Integrate(x => x, 0.0, 1.0, _highSampleOpts);
    [Benchmark] public MonteCarloResult IntegrateMultiDim_2D() => MonteCarloEngine.IntegrateMultiDim(v => v[0] * v[1], new MVVector(new double[] { 0.0, 0.0 }), new MVVector(new double[] { 1.0, 1.0 }), _defaultOpts);
    [Benchmark] public MonteCarloResult EstimatePi_Small() => MonteCarloEngine.EstimatePi(1000);
    [Benchmark] public MonteCarloResult EstimatePi_Medium() => MonteCarloEngine.EstimatePi(10000);
    [Benchmark] public MonteCarloResult EstimatePi_Large() => MonteCarloEngine.EstimatePi(100000);
    [Benchmark] public MonteCarloResult Integrate_ConstantFunction() => MonteCarloEngine.Integrate(x => 5.0, 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_GaussianFunction() => MonteCarloEngine.Integrate(x => System.Math.Exp(-x * x), -3.0, 3.0, _defaultOpts);
    [Benchmark] public MonteCarloResult MonteCarloResult_Creation() => MonteCarloResult.Success(1.0, 0.01, 1000, System.TimeSpan.FromMilliseconds(1));
    [Benchmark] public double MonteCarloResult_ConfidenceInterval() => MonteCarloResult.Success(1.0, 0.01, 1000, System.TimeSpan.FromMilliseconds(1)).ConfidenceIntervalUpper - MonteCarloResult.Success(1.0, 0.01, 1000, System.TimeSpan.FromMilliseconds(1)).ConfidenceIntervalLower;
    [Benchmark] public double MonteCarloResult_StandardError() => MonteCarloResult.Success(1.0, 0.01, 1000, System.TimeSpan.FromMilliseconds(1)).StandardError;
    [Benchmark] public MonteCarloOptions MonteCarloOptions_Defaults() => new MonteCarloOptions();
    [Benchmark] public MonteCarloResult Integrate_NegativeFunction() => MonteCarloEngine.Integrate(x => -x * x, 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_StepFunction() => MonteCarloEngine.Integrate(x => x < 0.5 ? 0.0 : 1.0, 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_AbsFunction() => MonteCarloEngine.Integrate(x => System.Math.Abs(x - 0.5), 0.0, 1.0, _defaultOpts);
    [Benchmark] public MonteCarloResult EstimatePi_VeryLarge() => MonteCarloEngine.EstimatePi(1000000);
    [Benchmark] public MonteCarloResult Integrate_PiecewiseFunction() => MonteCarloEngine.Integrate(x => x < 0.3 ? x * 2 : (1.0 - x) * 3, 0.0, 1.0, _defaultOpts);
    [Benchmark] public bool MonteCarloResult_ConvergedCheck() => MonteCarloResult.Success(1.0, 0.0001, 10000, System.TimeSpan.FromMilliseconds(1)).Converged;
    [Benchmark] public MonteCarloResult Integrate_SmallInterval() => MonteCarloEngine.Integrate(x => x * x, 0.0, 0.1, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_LargeInterval() => MonteCarloEngine.Integrate(x => x * x, 0.0, 100.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_ExpFunction() => MonteCarloEngine.Integrate(x => System.Math.Exp(-x), 0.0, 5.0, _defaultOpts);
    [Benchmark] public MonteCarloResult Integrate_LogFunction() => MonteCarloEngine.Integrate(x => System.Math.Log(x + 1.0), 0.0, 3.0, _defaultOpts);
    [Benchmark] public MonteCarloResult EstimatePi_ConvergenceCheck() { var r = MonteCarloEngine.EstimatePi(50000); return r; }
}
