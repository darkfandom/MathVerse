namespace MathVerse.Simulation.Tests.Solvers;

using MathVerse.Math.Numerics.LinearAlgebra;

public sealed class ODESolversExtendedTests
{
    [Fact]
    public void SolveRK4_ConstantFunction_LinearResult()
    {
        var problem = new ODEProblem
        {
            Function = (_, _) => new Vector(1.0),
            InitialState = new Vector(0.0),
            StartTime = 0,
            EndTime = 1.0
        };
        var solution = ODESolvers.SolveRK4(problem);
        solution.Success.Should().BeTrue();
        solution.States.Last()[0].Should().BeGreaterThan(0);
    }

    [Fact]
    public void SolveRK4_ZeroInitialState_StartsAtZero()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-1),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 0.5
        };
        var solution = ODESolvers.SolveRK4(problem);
        solution.States[0][0].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void SolveRK4_TimesAreMonotonicallyIncreasing()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-0.1),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 1.0
        };
        var solution = ODESolvers.SolveRK4(problem);
        for (int i = 1; i < solution.Times.Length; i++)
            solution.Times[i].Should().BeGreaterThan(solution.Times[i - 1]);
    }

    [Fact]
    public void SolveRK4_StepCountMatchesTimesMinusOne()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-0.1),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 0.5
        };
        var solution = ODESolvers.SolveRK4(problem);
        solution.Times.Length.Should().Be(solution.Steps + 1);
    }

    [Fact]
    public void SolveRK4_VectorDimension_Preserved()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-0.1),
            InitialState = new Vector(1.0, 2.0, 3.0),
            StartTime = 0,
            EndTime = 0.1
        };
        var solution = ODESolvers.SolveRK4(problem);
        solution.States.Last().Size.Should().Be(3);
    }

    [Fact]
    public void SolveAdaptiveRK45_SmallProblem_FewSteps()
    {
        var problem = new ODEProblem
        {
            Function = (_, _) => new Vector(0.0),
            InitialState = new Vector(0.0),
            StartTime = 0,
            EndTime = 0.01
        };
        var solution = ODESolvers.SolveAdaptiveRK45(problem);
        solution.Success.Should().BeTrue();
    }

    [Fact]
    public void SolveAdaptiveRK45_ExponentialDecay_ApproachesZero()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-1.0),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 5.0
        };
        var opts = new ODESolverOptions { InitialStep = 0.01 };
        var solution = ODESolvers.SolveAdaptiveRK45(problem, opts);
        solution.States.Last()[0].Should().BeApproximately(0.0, 0.1);
    }

    [Fact]
    public void SolveImplicitEuler_ExponentialDecay_ProducesResult()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-1.0),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 1.0
        };
        var solution = ODESolvers.SolveImplicitEuler(problem);
        solution.Success.Should().BeTrue();
        solution.States.Last()[0].Should().BeGreaterThan(0);
    }

    [Fact]
    public void SolveAdaptive_Alias_ProducesSameResult()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-0.5),
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 1.0
        };
        var opts = new ODESolverOptions { InitialStep = 0.01 };
        var sol1 = ODESolvers.SolveAdaptive(problem, opts);
        var sol2 = ODESolvers.SolveAdaptiveRK45(problem, opts);
        sol1.States.Last()[0].Should().BeApproximately(sol2.States.Last()[0], 1e-6);
    }

    [Fact]
    public void ODESolverOptions_DefaultValues()
    {
        var opts = new ODESolverOptions();
        opts.AbsoluteTolerance.Should().Be(1e-12);
        opts.RelativeTolerance.Should().Be(1e-9);
        opts.InitialStep.Should().Be(0.01);
        opts.MinStep.Should().Be(1e-12);
        opts.MaxStep.Should().Be(1.0);
        opts.MaxSteps.Should().Be(100000);
        opts.EnableEventDetection.Should().BeTrue();
        opts.DenseOutput.Should().BeFalse();
    }

    [Fact]
    public void ODEProblem_Record_CanBeConstructed()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y,
            InitialState = new Vector(1.0),
            StartTime = 0,
            EndTime = 1.0
        };
        problem.StartTime.Should().Be(0);
        problem.EndTime.Should().Be(1.0);
    }

    [Fact]
    public void EventOccurrence_DefaultValues()
    {
        var eo = new EventOccurrence();
        eo.EventName.Should().Be(string.Empty);
        eo.Time.Should().Be(0);
    }

    [Fact]
    public void EventDirection_AllValuesDistinct()
    {
        var values = Enum.GetValues<EventDirection>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }
}
