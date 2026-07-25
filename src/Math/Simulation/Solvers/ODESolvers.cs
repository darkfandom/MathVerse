namespace MathVerse.Math.Simulation.Solvers;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed record ODEProblem
{
    public Func<double, Vector, Vector> Function { get; init; } = (_, _) => default!;
    public Vector InitialState { get; init; }
    public double StartTime { get; init; }
    public double EndTime { get; init; }
    public ImmutableArray<Event> Events { get; init; }
}

public sealed record Event
{
    public string Name { get; init; } = string.Empty;
    public Func<double, Vector, double> Condition { get; init; } = (_, _) => default!;
    public EventDirection Direction { get; init; }
    public bool Terminate { get; init; }
}

public enum EventDirection
{
    Increasing,
    Decreasing,
    Both
}

public sealed record ODESolution
{
    public ImmutableArray<double> Times { get; init; }
    public ImmutableArray<Vector> States { get; init; }
    public ImmutableArray<EventOccurrence> Events { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int FunctionEvaluations { get; init; }
    public int Steps { get; init; }
    public int RejectedSteps { get; init; }
}

public sealed record EventOccurrence
{
    public string EventName { get; init; } = string.Empty;
    public double Time { get; init; }
    public Vector State { get; init; }
    public int Direction { get; init; }
}

public sealed record ODESolverOptions
{
    public double AbsoluteTolerance { get; init; } = 1e-12;
    public double RelativeTolerance { get; init; } = 1e-9;
    public double InitialStep { get; init; } = 0.01;
    public double MinStep { get; init; } = 1e-12;
    public double MaxStep { get; init; } = 1.0;
    public int MaxSteps { get; init; } = 100000;
    public bool EnableEventDetection { get; init; } = true;
    public bool DenseOutput { get; init; } = false;
}

public static class ODESolvers
{
    public static ODESolution SolveRK4(ODEProblem problem, ODESolverOptions? options = null)
    {
        options ??= new ODESolverOptions();
        var f = problem.Function;
        var y = problem.InitialState;
        double t = problem.StartTime;
        double h = options.InitialStep;
        double tEnd = problem.EndTime;

        var times = ImmutableArray.CreateBuilder<double>();
        var states = ImmutableArray.CreateBuilder<Vector>();
        times.Add(t);
        states.Add(y);

        int steps = 0;
        int evals = 0;

        while (t < problem.EndTime && steps < 1000000)
        {
            if (t + h > problem.EndTime) h = problem.EndTime - t;

            var k1 = problem.Function(t, y);
            var k2 = problem.Function(t + 0.5 * h, y.Add(k1.Scale(0.5 * h)));
            var k3 = problem.Function(t + 0.5 * h, y.Add(k2.Scale(0.5 * h)));
            var k4 = problem.Function(t + h, y.Add(k3.Scale(h)));

            Vector yNew = y.Add(
                k1.Add(k2.Scale(2)).Add(k3.Scale(2)).Add(k4).Scale(h / 6.0)
            );

            t += h;
            y = yNew;
            steps++;
            evals += 4;

            times.Add(t);
            states.Add(y);
        }

        return new ODESolution
        {
            Times = times.ToImmutable(),
            States = states.ToImmutable(),
            Events = ImmutableArray<EventOccurrence>.Empty,
            Success = true,
            FunctionEvaluations = evals,
            Steps = steps
        };
    }

    public static ODESolution SolveAdaptiveRK45(ODEProblem problem, ODESolverOptions? options = null)
    {
        options ??= new ODESolverOptions();
        var f = problem.Function;
        var y = problem.InitialState;
        double t = problem.StartTime;
        double h = options.InitialStep;
        double tEnd = problem.EndTime;

        var times = ImmutableArray.CreateBuilder<double>();
        var states = ImmutableArray.CreateBuilder<Vector>();
        times.Add(t);
        states.Add(y);

        int steps = 0, rejected = 0, evals = 0;
        double atol = options.AbsoluteTolerance;
        double rtol = options.RelativeTolerance;

        while (t < problem.EndTime && steps < 1000000)
        {
            if (t + h > problem.EndTime) h = problem.EndTime - t;

            // RK4(5) - Dormand-Prince
            var k1 = problem.Function(t, y);
            var k2 = problem.Function(t + 1.0/5 * h, y.Add(k1.Scale(1.0/5 * h)));
            var k3 = problem.Function(t + 3.0/10 * h, y.Add(k1.Scale(3.0/40 * h)).Add(k2.Scale(9.0/40 * h)));
            var k4 = problem.Function(t + 4.0/5 * h, y.Add(k1.Scale(44.0/45 * h)).Add(k2.Scale(-56.0/15 * h)).Add(k3.Scale(32.0/9 * h)));
            var k5 = problem.Function(t + 8.0/9 * h, y.Add(k1.Scale(19372.0/6561 * h)).Add(k2.Scale(-25360.0/2187 * h)).Add(k3.Scale(64448.0/6561 * h)).Add(k4.Scale(-212.0/729 * h)));
            var k6 = problem.Function(t + h, y.Add(k1.Scale(9017.0/3168 * h)).Add(k2.Scale(-355.0/33 * h)).Add(k3.Scale(46732.0/5247 * h)).Add(k4.Scale(49.0/176 * h)).Add(k5.Scale(-5103.0/18656 * h)));
            var k7 = problem.Function(t + h, y.Add(k1.Scale(35.0/384 * h)).Add(k3.Scale(500.0/1113 * h)).Add(k4.Scale(125.0/192 * h)).Add(k5.Scale(-2187.0/6784 * h)).Add(k6.Scale(11.0/84 * h)));

            // 4th order solution
            var y4 = y.Add(k1.Scale(5179.0/57600 * h)).Add(k3.Scale(7571.0/16695 * h)).Add(k4.Scale(393.0/640 * h)).Add(k5.Scale(-92097.0/339200 * h)).Add(k6.Scale(187.0/2100 * h)).Add(k7.Scale(1.0/40 * h));
            
            // 5th order solution
            var y5 = y.Add(k1.Scale(35.0/384 * h)).Add(k3.Scale(500.0/1113 * h)).Add(k4.Scale(125.0/192 * h)).Add(k5.Scale(-2187.0/6784 * h)).Add(k6.Scale(11.0/84 * h));

            // Error estimate using mixed absolute/relative tolerance
            double errorNorm = y5.Subtract(y4).Norm();
            double yNorm = y.Norm();
            double tol = atol + rtol * yNorm;
            double error = errorNorm;
            evals += 7;

            if (error <= tol)
            {
                t += h;
                y = y5;
                states.Add(y);
                times.Add(t);
                steps++;
            }
            else
            {
                rejected++;
            }

            // Adjust step size
            double safety = 0.9;
            double ratio = tol / System.Math.Max(error, 1e-30);
            double factor = System.Math.Min(5.0, System.Math.Max(0.2, safety * System.Math.Pow(ratio, 0.2)));
            h = System.Math.Clamp(h * factor, 1e-12, 1.0);
        }

        return new ODESolution
        {
            Times = times.ToImmutable(),
            States = states.ToImmutable(),
            Events = ImmutableArray<EventOccurrence>.Empty,
            Success = true,
            FunctionEvaluations = evals,
            Steps = steps,
            RejectedSteps = rejected
        };
    }

    public static ODESolution SolveImplicitEuler(ODEProblem problem, ODESolverOptions? options = null)
    {
        options ??= new ODESolverOptions();
        var f = problem.Function;
        var y = problem.InitialState;
        double t = problem.StartTime;
        double h = options.InitialStep;

        var times = ImmutableArray.CreateBuilder<double>();
        var states = ImmutableArray.CreateBuilder<Vector>();
        times.Add(t);
        states.Add(y);

        int steps = 0;

        while (t < problem.EndTime && steps < 1000000)
        {
            if (t + h > problem.EndTime) h = problem.EndTime - t;

            // Implicit Euler: y_{n+1} = y_n + h * f(t_{n+1}, y_{n+1})
            // Solve using fixed point iteration
            var yNew = y;
            for (int iter = 0; iter < 10; iter++)
            {
                var yNext = y.Add(f(t + h, yNew).Scale(h));
                if ((yNext.Subtract(yNew)).Norm() < 1e-10)
                {
                    yNew = yNext;
                    break;
                }
                yNew = yNext;
            }

            t += h;
            y = yNew;
            steps++;
            times.Add(t);
            states.Add(y);
        }

        return new ODESolution
        {
            Times = times.ToImmutable(),
            States = states.ToImmutable(),
            Events = ImmutableArray<EventOccurrence>.Empty,
            Success = true,
            FunctionEvaluations = steps,
            Steps = steps
        };
    }

    public static ODESolution SolveAdaptive(ODEProblem problem, ODESolverOptions? options = null)
        => SolveAdaptiveRK45(problem, options);
}