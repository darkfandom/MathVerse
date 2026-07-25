namespace MathVerse.Simulation.Tests.Solvers;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public class SolverEngineTests
{
    [Theory]
    [InlineData(SolverType.ExplicitEuler)]
    [InlineData(SolverType.ImplicitEuler)]
    [InlineData(SolverType.RungeKutta4)]
    [InlineData(SolverType.AdaptiveRungeKutta45)]
    [InlineData(SolverType.DormandPrince)]
    [InlineData(SolverType.AdamsBashforth)]
    [InlineData(SolverType.AdamsMoulton)]
    [InlineData(SolverType.BackwardDifferentiation)]
    public void SolverFactory_Create_AllSolverTypes_ProducesNonNull(SolverType type)
    {
        var solver = SolverFactory.Create(type);

        solver.Should().NotBeNull();
    }

    [Fact]
    public void SolverFactory_Create_ReturnsODESolver()
    {
        var solver = SolverFactory.Create(SolverType.RungeKutta4);

        solver.Should().BeOfType<ODESolver>();
    }

    [Fact]
    public void ODESolver_Solve_SimpleODE_ProducesResult()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 1.0);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ODESolver_Solve_FinalTime_ReachesTarget()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 1.0);

        result.FinalTime.Should().BeApproximately(1.0, 0.1);
    }

    [Fact]
    public void ODESolver_Solve_LinearODE_CorrectValue()
    {
        Func<double, Vector, Vector> f = (t, y) => new Vector(1.0);
        var y0 = new Vector(0.0);

        var result = ODESolver.Solve(f, y0, 0, 1.0);

        result.FinalState[0].Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public void ODESolver_Solve_ExponentialODE_ApproximatesET()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 1.0);

        result.FinalState[0].Should().BeApproximately(SM.E, 5.0);
    }

    [Fact]
    public void ODESolver_Solve_SuccessFlag()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 0.1);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ODESolver_Solve_HasTrajectory()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 1.0);

        result.Trajectory.Should().NotBeEmpty();
    }

    [Fact]
    public void ODESolver_Solve_RecordsExecutionTime()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);

        var result = ODESolver.Solve(f, y0, 0, 0.1);

        result.ExecutionTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void SolverResult_Record_DefaultValues()
    {
        var result = new SolverResult
        {
            Success = true,
            Steps = 100,
            FunctionEvaluations = 400,
            FinalState = new Vector(1.0),
            FinalTime = 1.0
        };

        result.Success.Should().BeTrue();
        result.Steps.Should().Be(100);
    }

    [Fact]
    public void SolverOptions_Record_DefaultValues()
    {
        var options = new SolverOptions();

        options.Method.Should().Be(SolverType.RungeKutta4);
        options.InitialStep.Should().Be(0.01);
        options.AbsoluteTolerance.Should().Be(1e-12);
    }

    [Fact]
    public void SolutionPoint_Record_CanBeConstructed()
    {
        var point = new SolutionPoint
        {
            Time = 0.5,
            State = new Vector(1.0, 2.0)
        };

        point.Time.Should().Be(0.5);
        point.State.Size.Should().Be(2);
    }

    [Fact]
    public void ODESolver_Solve_WithCustomOptions()
    {
        Func<double, Vector, Vector> f = (t, y) => y;
        var y0 = new Vector(1.0);
        var options = new SolverOptions
        {
            InitialStep = 0.01,
            MaxSteps = 100000
        };

        var result = ODESolver.Solve(f, y0, 0, 1.0, options);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void RungeKutta45Solver_Solve_ProducesResult()
    {
        var result = RungeKutta45Solver.Solve(
            (t, y) => y,
            new Vector(1.0),
            0, 0.1);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ImplicitEulerSolver_Solve_ProducesResult()
    {
        var result = ImplicitEulerSolver.Solve(
            (t, y) => y,
            (t, y) => Matrix.Identity(1),
            new Vector(1.0),
            0, 0.1);

        result.Should().NotBeNull();
    }
}
