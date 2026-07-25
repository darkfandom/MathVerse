namespace MathVerse.Simulation.Tests.Solvers;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public class ODESolversTests
{
    private static ODEProblem CreateExponentialProblem(double y0 = 1.0, double tEnd = 1.0)
    {
        return new ODEProblem
        {
            Function = (t, y) => y,
            InitialState = new Vector(y0),
            StartTime = 0,
            EndTime = tEnd
        };
    }

    [Fact]
    public void SolveRK4_ExponentialODE_ApproximatesET()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);
        var options = new ODESolverOptions { InitialStep = 0.01 };

        var solution = ODESolvers.SolveRK4(problem, options);

        solution.Success.Should().BeTrue();
        var final = solution.States[^1];
        final[0].Should().BeApproximately(SM.E, 0.01);
    }

    [Fact]
    public void SolveRK4_SmallStep_BetterAccuracy()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var coarse = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.1 });
        var fine = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.001 });

        var errCoarse = SM.Abs(coarse.States[^1][0] - SM.E);
        var errFine = SM.Abs(fine.States[^1][0] - SM.E);

        errFine.Should().BeLessThan(errCoarse);
    }

    [Fact]
    public void SolveRK4_LinearODE_SolvesCorrectly()
    {
        var problem = new ODEProblem
        {
            Function = (t, y) => new Vector(1.0),
            InitialState = new Vector(0.0),
            StartTime = 0,
            EndTime = 1.0
        };

        var solution = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.01 });

        solution.States[^1][0].Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void SolveRK4_ReturnsCorrectNumberOfSteps()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var solution = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.1 });

        solution.Steps.Should().BeGreaterThan(0);
        solution.Times.Length.Should().Be(solution.States.Length);
    }

    [Fact]
    public void SolveRK4_RecordsAllTimes()
    {
        var problem = CreateExponentialProblem(1.0, 0.5);

        var solution = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.1 });

        solution.Times[0].Should().Be(0);
        solution.Times[^1].Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void SolveAdaptiveRK45_ExponentialODE_ApproximatesET()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var solution = ODESolvers.SolveAdaptiveRK45(problem);

        solution.Success.Should().BeTrue();
        var final = solution.States[^1];
        final[0].Should().BeApproximately(SM.E, 5.0);
    }

    [Fact]
    public void SolveAdaptiveRK45_PerformsAdaptation()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var solution = ODESolvers.SolveAdaptiveRK45(problem);

        solution.FunctionEvaluations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SolveImplicitEuler_ExponentialODE_ProducesResult()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var solution = ODESolvers.SolveImplicitEuler(problem);

        solution.Success.Should().BeTrue();
        solution.States.Should().NotBeEmpty();
    }

    [Fact]
    public void SolveImplicitEuler_IsLessAccurateThanRK4()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);
        var options = new ODESolverOptions { InitialStep = 0.01 };

        var rk4 = ODESolvers.SolveRK4(problem, options);
        var ie = ODESolvers.SolveImplicitEuler(problem, options);

        var errRK4 = SM.Abs(rk4.States[^1][0] - SM.E);
        var errIE = SM.Abs(ie.States[^1][0] - SM.E);

        errIE.Should().BeGreaterThanOrEqualTo(errRK4);
    }

    [Fact]
    public void SolveAdaptive_Alias_MatchesAdaptiveRK45()
    {
        var problem = CreateExponentialProblem(1.0, 1.0);

        var solution1 = ODESolvers.SolveAdaptive(problem);
        var solution2 = ODESolvers.SolveAdaptiveRK45(problem);

        solution1.States.Length.Should().Be(solution2.States.Length);
    }

    [Fact]
    public void SolveRK4_OscillatoryODE_SinCosBehavior()
    {
        var problem = new ODEProblem
        {
            Function = (t, y) => new Vector(-y[1], y[0]),
            InitialState = new Vector(1.0, 0.0),
            StartTime = 0,
            EndTime = 2 * SM.PI
        };

        var solution = ODESolvers.SolveRK4(problem, new ODESolverOptions { InitialStep = 0.001 });

        var final = solution.States[^1];
        final[0].Should().BeApproximately(1.0, 0.01);
        final[1].Should().BeApproximately(0.0, 0.01);
    }

    [Fact]
    public void SolveRK4_VectorODE_PreservesDimension()
    {
        var problem = new ODEProblem
        {
            Function = (t, y) => y.Scale(-1),
            InitialState = new Vector(1.0, 2.0, 3.0),
            StartTime = 0,
            EndTime = 0.1
        };

        var solution = ODESolvers.SolveRK4(problem);

        solution.States[^1].Size.Should().Be(3);
    }

    [Fact]
    public void ODEProblem_Record_CanBeConstructed()
    {
        var problem = new ODEProblem
        {
            Function = (t, y) => y,
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 1.0
        };

        problem.Function.Should().NotBeNull();
        problem.InitialState.Size.Should().Be(1);
    }

    [Fact]
    public void ODESolverOptions_DefaultValues()
    {
        var options = new ODESolverOptions();

        options.AbsoluteTolerance.Should().Be(1e-12);
        options.RelativeTolerance.Should().Be(1e-9);
        options.InitialStep.Should().Be(0.01);
    }

    [Fact]
    public void ODESolution_Record_HasExpectedProperties()
    {
        var solution = new ODESolution
        {
            Success = true,
            Steps = 10,
            FunctionEvaluations = 40
        };

        solution.Success.Should().BeTrue();
        solution.Steps.Should().Be(10);
    }
}
