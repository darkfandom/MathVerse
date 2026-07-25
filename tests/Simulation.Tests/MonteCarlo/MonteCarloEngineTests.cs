namespace MathVerse.Simulation.Tests.MonteCarlo;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public class MonteCarloEngineTests
{
    [Fact]
    public void Integrate_xSquared_FromZeroToOne_ApproximatesOneThird()
    {
        Func<double, double> f = x => x * x;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 100000 });

        result.Mean.Should().BeApproximately(1.0 / 3.0, 0.02);
    }

    [Fact]
    public void Integrate_ConstantFunction_CorrectValue()
    {
        Func<double, double> f = x => 5.0;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 10000 });

        result.Mean.Should().BeApproximately(5.0, 0.1);
    }

    [Fact]
    public void Integrate_LinearFunction_FromZeroToOne()
    {
        Func<double, double> f = x => x;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 100000 });

        result.Mean.Should().BeApproximately(0.5, 0.02);
    }

    [Fact]
    public void Integrate_MoreSamples_BetterAccuracy()
    {
        Func<double, double> f = x => x * x;

        var coarse = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 100 });
        var fine = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 100000 });

        var errCoarse = SM.Abs(coarse.Mean - 1.0 / 3.0);
        var errFine = SM.Abs(fine.Mean - 1.0 / 3.0);

        errFine.Should().BeLessThan(errCoarse);
    }

    [Fact]
    public void Integrate_ProducesConfidenceInterval()
    {
        Func<double, double> f = x => x;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 10000 });

        result.ConfidenceIntervalLower.Should().BeLessThan(result.Mean);
        result.ConfidenceIntervalUpper.Should().BeGreaterThan(result.Mean);
    }

    [Fact]
    public void Integrate_SamplesUsed_MatchesOption()
    {
        Func<double, double> f = x => x;
        var options = new MonteCarloOptions { Samples = 5000 };

        var result = MonteCarloEngine.Integrate(f, 0, 1, options);

        result.SamplesUsed.Should().Be(5000);
    }

    [Fact]
    public void EstimatePi_ApproximatesPi()
    {
        var result = MonteCarloEngine.EstimatePi(100000);

        result.Mean.Should().BeApproximately(SM.PI, 0.1);
    }

    [Fact]
    public void EstimatePi_MoreSamples_CloserToPi()
    {
        var coarse = MonteCarloEngine.EstimatePi(100);
        var fine = MonteCarloEngine.EstimatePi(100000);

        var errCoarse = SM.Abs(coarse.Mean - SM.PI);
        var errFine = SM.Abs(fine.Mean - SM.PI);

        errFine.Should().BeLessThan(errCoarse);
    }

    [Fact]
    public void IntegrateZeroFunction_ReturnsZero()
    {
        Func<double, double> f = x => 0.0;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 1000 });

        result.Mean.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void Integrate_NegativeFunction_NegativeResult()
    {
        Func<double, double> f = x => -x;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 10000 });

        result.Mean.Should().BeApproximately(-0.5, 0.02);
    }

    [Fact]
    public void ImportanceSampling_ProducesResult()
    {
        Func<double, double> f = x => x * x;
        var random = new System.Random(42);
        Func<double> proposalSampler = () => random.NextDouble();
        Func<double, double> proposalPdf = x => 1.0;

        var (mean, error) = MonteCarloEngine.ImportanceSampling(f, proposalSampler, proposalPdf, 10000);

        mean.Should().BeApproximately(1.0 / 3.0, 0.05);
    }

    [Fact]
    public void MetropolisHastings_ProducesVector()
    {
        Func<Vector, double> logTarget = v => -v.Dot(v);
        var initial = new Vector(0.0, 0.0);
        var proposalCov = Matrix.Identity(2);

        var result = MonteCarloEngine.MetropolisHastings(logTarget, initial, proposalCov, 100, 10);

        result.Should().NotBeNull();
    }

    [Fact]
    public void MonteCarloResult_Success_CreatesResult()
    {
        var result = MonteCarloResult.Success(1.0, 0.01, 1000, TimeSpan.FromMilliseconds(5));

        result.Mean.Should().Be(1.0);
        result.SamplesUsed.Should().Be(1000);
        result.Converged.Should().BeTrue();
    }

    [Fact]
    public void MonteCarloResult_StandardError_CorrectFormula()
    {
        double variance = 0.25;
        int samples = 10000;

        var result = MonteCarloResult.Success(variance, variance, samples, TimeSpan.Zero);

        result.StandardError.Should().BeApproximately(SM.Sqrt(variance / samples), 1e-10);
    }

    [Fact]
    public void MonteCarloOptions_DefaultValues()
    {
        var options = new MonteCarloOptions();

        options.Samples.Should().Be(10000);
        options.Tolerance.Should().Be(1e-6);
        options.UseAntitheticVariates.Should().BeTrue();
    }

    [Fact]
    public void Integrate_Cosine_FromZeroToPi()
    {
        Func<double, double> f = SM.Cos;

        var result = MonteCarloEngine.Integrate(f, 0, SM.PI, new MonteCarloOptions { Samples = 100000 });

        result.Mean.Should().BeApproximately(0.0, 0.05);
    }

    [Fact]
    public void Integrate_EulerNumber_ApproximatesEMinusOne()
    {
        Func<double, double> f = SM.Exp;

        var result = MonteCarloEngine.Integrate(f, 0, 1, new MonteCarloOptions { Samples = 100000 });

        result.Mean.Should().BeApproximately(SM.E - 1.0, 0.02);
    }

    [Fact]
    public void Integrate_MultiDim_ConstantFunction()
    {
        Func<Vector, double> f = v => 1.0;
        var lower = new Vector(0.0, 0.0);
        var upper = new Vector(1.0, 1.0);

        var result = MonteCarloEngine.IntegrateMultiDim(f, lower, upper, new MonteCarloOptions { Samples = 10000 });

        result.Mean.Should().BeApproximately(1.0, 0.1);
    }
}
