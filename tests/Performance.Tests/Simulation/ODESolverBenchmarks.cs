namespace MathVerse.Performance.Tests.Simulation;

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Simulation.Solvers;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

[MemoryDiagnoser]
public class ODESolverBenchmarks
{
    private ODEProblem _expDecay = null!;
    private ODEProblem _harmonic = null!;
    private ODEProblem _stiff = null!;
    private ODEProblem _twoBody = null!;
    private ODESolverOptions _defaultOpts = null!;
    private ODESolverOptions _tightTol = null!;

    [GlobalSetup]
    public void Setup()
    {
        _expDecay = new ODEProblem { Function = (_, y) => y.Scale(-1.0), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 1.0 };
        _harmonic = new ODEProblem { Function = (t, y) => new MVVector(y[1], -y[0]), InitialState = new MVVector(1.0, 0.0), StartTime = 0, EndTime = 2 * System.Math.PI };
        _stiff = new ODEProblem { Function = (_, y) => y.Scale(-1000.0), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 0.01 };
        _twoBody = new ODEProblem
        {
            Function = (t, y) =>
            {
                double r = System.Math.Sqrt(y[0] * y[0] + y[1] * y[1]);
                if (r < 1e-10) return new MVVector(0, 0, 0, 0);
                return new MVVector(y[2], y[3], -y[0] / (r * r * r), -y[1] / (r * r * r));
            },
            InitialState = new MVVector(1.0, 0.0, 0.0, 0.5),
            StartTime = 0,
            EndTime = 2 * System.Math.PI
        };
        _defaultOpts = new ODESolverOptions();
        _tightTol = new ODESolverOptions { AbsoluteTolerance = 1e-15, RelativeTolerance = 1e-12 };
    }

    [Benchmark] public ODESolution SolveRK4_ExpDecay() => ODESolvers.SolveRK4(_expDecay, _defaultOpts);
    [Benchmark] public ODESolution SolveRK4_Harmonic() => ODESolvers.SolveRK4(_harmonic, _defaultOpts);
    [Benchmark] public ODESolution SolveRK4_StiffProblem() => ODESolvers.SolveRK4(_stiff, _defaultOpts);
    [Benchmark] public ODESolution SolveRK4_TwoBody() => ODESolvers.SolveRK4(_twoBody, _defaultOpts);
    [Benchmark] public ODESolution SolveRK4_LargeTimeSpan()
    {
        var p = new ODEProblem { Function = (_, y) => y.Scale(-0.1), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 100.0 };
        return ODESolvers.SolveRK4(p);
    }
    [Benchmark] public ODESolution SolveRK4_ManySteps()
    {
        var p = new ODEProblem { Function = (_, y) => y.Scale(-1.0), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 10.0 };
        return ODESolvers.SolveRK4(p, new ODESolverOptions { InitialStep = 0.001 });
    }
    [Benchmark] public ODESolution SolveAdaptiveRK45_ExpDecay() => ODESolvers.SolveAdaptiveRK45(_expDecay);
    [Benchmark] public ODESolution SolveAdaptiveRK45_Harmonic() => ODESolvers.SolveAdaptiveRK45(_harmonic);
    [Benchmark] public ODESolution SolveAdaptiveRK45_TwoBody() => ODESolvers.SolveAdaptiveRK45(_twoBody);
    [Benchmark] public ODESolution SolveAdaptiveRK45_TightTolerance() => ODESolvers.SolveAdaptiveRK45(_expDecay, _tightTol);
    [Benchmark] public ODESolution SolveAdaptiveRK45_RelaxedTolerance()
    {
        var opts = new ODESolverOptions { AbsoluteTolerance = 1e-4, RelativeTolerance = 1e-3 };
        return ODESolvers.SolveAdaptiveRK45(_expDecay, opts);
    }
    [Benchmark] public ODESolution SolveImplicitEuler_ExpDecay() => ODESolvers.SolveImplicitEuler(_expDecay);
    [Benchmark] public ODESolution SolveImplicitEuler_Stiff() => ODESolvers.SolveImplicitEuler(_stiff);
    [Benchmark] public ODESolution SolveImplicitEuler_Harmonic() => ODESolvers.SolveImplicitEuler(_harmonic);
    [Benchmark] public ODESolution SolveAdaptive_DelegatesToRK45() => ODESolvers.SolveAdaptive(_expDecay);
    [Benchmark] public ODEProblem ODEProblem_Creation() => new ODEProblem { Function = (_, y) => y.Scale(-1.0), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 1.0 };
    [Benchmark] public ODESolverOptions ODESolverOptions_Defaults() => new ODESolverOptions();
    [Benchmark] public ImmutableArray<double> ODESolution_TimesAccess() => ODESolvers.SolveRK4(_expDecay).Times;
    [Benchmark] public ImmutableArray<MVVector> ODESolution_StatesAccess() => ODESolvers.SolveRK4(_expDecay).States;
    [Benchmark] public ODESolution SolveRK4_QuadraticFunction()
    {
        var p = new ODEProblem { Function = (_, y) => new MVVector(y[0] * y[0]), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 0.1 };
        return ODESolvers.SolveRK4(p);
    }
    [Benchmark] public ODESolution SolveRK4_CubicFunction()
    {
        var p = new ODEProblem { Function = (_, y) => new MVVector(y[0] * y[0] * y[0]), InitialState = new MVVector(0.5), StartTime = 0, EndTime = 0.1 };
        return ODESolvers.SolveRK4(p);
    }
    [Benchmark] public ODESolution SolveRK4_SinusoidalFunction()
    {
        var p = new ODEProblem { Function = (t, _) => new MVVector(System.Math.Sin(t)), InitialState = new MVVector(0.0), StartTime = 0, EndTime = 2 * System.Math.PI };
        return ODESolvers.SolveRK4(p);
    }
    [Benchmark] public ODESolution SolveAdaptiveRK45_ExponentialGrowth()
    {
        var p = new ODEProblem { Function = (_, y) => y.Scale(1.0), InitialState = new MVVector(1.0), StartTime = 0, EndTime = 1.0 };
        return ODESolvers.SolveAdaptiveRK45(p);
    }
    [Benchmark] public ODESolution SolveRK4_VectorDimension2() => ODESolvers.SolveRK4(_harmonic);
    [Benchmark] public ODESolution SolveRK4_VectorDimension4() => ODESolvers.SolveRK4(_twoBody);
    [Benchmark] public ODESolution SolveRK4_VectorDimension10()
    {
        var p = new ODEProblem
        {
            Function = (t, y) =>
            {
                var result = new double[10];
                for (int i = 0; i < 10; i++) result[i] = -y[i];
                return new MVVector(result);
            },
            InitialState = new MVVector(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }),
            StartTime = 0, EndTime = 1.0
        };
        return ODESolvers.SolveRK4(p);
    }
    [Benchmark] public ODESolution SolveImplicitEuler_ConvergeCheck()
    {
        var result = ODESolvers.SolveImplicitEuler(_expDecay);
        return result;
    }
    [Benchmark] public int ODESolveRK4_FunctionEvaluations() => ODESolvers.SolveRK4(_expDecay).FunctionEvaluations;
}
