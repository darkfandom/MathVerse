namespace MathVerse.Math.Simulation.Public;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Numerics.Optimization;
using MathVerse.Math.Numerics.Integration;
using MathVerse.Math.Numerics.RootFinding;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Time;
using MathVerse.Math.Simulation.Events;
using MathVerse.Math.Simulation.Physics;
using MathVerse.Math.Simulation.Thermodynamics;
using MathVerse.Math.Simulation.Electromagnetics;
using MathVerse.Math.Simulation.FluidDynamics;
using MathVerse.Math.Simulation.Chemistry;
using MathVerse.Math.Simulation.Biology;
using MathVerse.Math.Simulation.Finance;
using MathVerse.Math.Simulation.SignalProcessing;
using MathVerse.Math.Simulation.ControlSystems;
using MathVerse.Math.Simulation.MonteCarlo;
using MathVerse.Math.Simulation.Solvers;
using MathVerse.Math.Simulation.Diagnostics;
using MathVerse.Math.Simulation.Configuration;
using MathVerse.Math.Simulation.Visualization;
using MathVerse.Math.Simulation.Models;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class SimulationEngine
{
    private readonly SimulationOptions _options;
    private readonly SimulationConfiguration _config;
    private readonly Core.SimulationEngine _coreEngine;
    private readonly DiagnosticCollector _diagnostics = new();

    public SimulationEngine(SimulationOptions? options = null, SimulationConfiguration? config = null)
    {
        _options = options ?? SimulationOptions.Default;
        _config = config ?? SimulationConfiguration.Default;
        _coreEngine = new Core.SimulationEngine(_options);
    }

    public SimulationOptions Options => _options;
    public SimulationConfiguration Configuration => _config;
    public DiagnosticCollector Diagnostics => _diagnostics;

    public SimulationResult Run(Func<SimulationState, double, SimulationState> stepFunction)
        => _coreEngine.Run(stepFunction);

    public PhysicsState SimulatePhysics(PhysicsState initial, double timeStep, int steps)
    {
        var state = initial;
        for (int i = 0; i < steps; i++)
            state = PhysicsEngine.Step(state, timeStep);
        return state;
    }

    public PhysicsState SimulatePhysics(PhysicsState initial, double time)
    {
        int steps = (int)(time / _config.Physics.DefaultTimeStep);
        return SimulatePhysics(initial, _config.Physics.DefaultTimeStep, steps);
    }

    public ThermodynamicState SimulateThermodynamics(ThermodynamicState initial, double time, ImmutableArray<Thermodynamics.HeatTransfer> transfers)
        => ThermodynamicsEngine.UpdateState(initial, time, transfers);

    public static MVVector CoulombForce(MVVector r, double q1, double q2)
        => ElectromagneticsEngine.CoulombForce(r, q1, q2);

    public static MVVector LorentzForce(MVVector velocity, MVVector eField, MVVector bField, double charge)
        => ElectromagneticsEngine.LorentzForce(velocity, eField, bField, charge);

    public static double ReynoldsNumber(double density, double velocity, double length, double viscosity)
        => FluidDynamicsEngine.ReynoldsNumber(density, velocity, length, viscosity);

    public static double ReactionRate(double A, double Ea, double T)
        => ChemistryEngine.ArrheniusRate(A, 0, T);

    public static double LogisticGrowth(double N, double r, double K, double dt)
        => BiologyEngine.LogisticGrowth(N, r, K, dt);

    public static (double prey, double predator) LotkaVolterra(
        double prey, double predator, double alpha, double beta, double gamma, double delta, double dt)
        => BiologyEngine.LotkaVolterra(prey, predator, alpha, beta, gamma, delta, dt);

    public static EpidemiologicalState SIRModel(EpidemiologicalState state, double beta, double gamma, double mu, double dt)
        => BiologyEngine.SIRModel(state, beta, gamma, mu, dt);

    public static double CompoundInterest(double principal, double rate, double time, int frequency = 12)
        => FinanceEngine.CompoundInterest(principal, rate, time, frequency);

    public static double BlackScholesCall(double S, double K, double T, double r, double sigma)
        => FinanceEngine.BlackScholesCall(S, K, T, r, sigma);

    public static double MonteCarloOptionPrice(Func<double, double> payoff, double S0, double r, double sigma, double T, int paths = 10000)
        => FinanceEngine.MonteCarloOptionPrice(payoff, S0, r, sigma, T, paths);

    public static ImmutableArray<System.Numerics.Complex> FFT(ImmutableArray<System.Numerics.Complex> signal)
        => SignalProcessingEngine.FFT(signal);

    public static double[] Convolution(double[] a, double[] b)
    {
        int n = a.Length + b.Length - 1;
        var result = new double[n];
        for (int i = 0; i < a.Length; i++)
            for (int j = 0; j < b.Length; j++)
                result[i + j] += a[i] * b[j];
        return result;
    }

    public static double PIDControl(double setpoint, double measured, double Kp, double Ki, double Kd, double dt, ref double integral, ref double prevError)
    {
        double error = setpoint - measured;
        integral += error * dt;
        double derivative = (error - prevError) / dt;
        prevError = error;
        return Kp * error + Ki * integral + Kd * derivative;
    }

    public static double MonteCarloIntegrate(Func<double, double> f, double a, double b, int samples = 10000)
    {
        var random = new Random();
        double sum = 0;
        for (int i = 0; i < samples; i++)
            sum += f(a + (b - a) * random.NextDouble());
        return (b - a) * sum / samples;
    }

    public static SolverResult SolveODE(
        Func<double, MVVector, MVVector> f,
        MVVector y0,
        double t0,
        double tf,
        SolverOptions? options = null)
        => ODESolver.Solve(f, y0, t0, tf, options);

    public static IntegrationResult Integrate(
        Func<double, double> f,
        double a,
        double b,
        IntegrationOptions? options = null)
        => Integrator.Instance.Integrate(f, a, b, options);

    public static RootResult FindRoot(Func<double, double> f, double guess, RootOptions? options = null)
        => RootFinderRegistry.Instance.Get("newton").FindRoot(f, guess, options);

    public static OptimizationResult Minimize(
        Func<MVVector, double> f,
        MVVector initial,
        OptimizationOptions? options = null)
        => new GradientDescent().Optimize(f, initial, options);

    public static double Mean(ImmutableArray<double> data)
        => data.Average();

    public static double Variance(ImmutableArray<double> data)
    {
        double mean = data.Average();
        return data.Average(x => (x - mean) * (x - mean));
    }

    public static double Correlation(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        double mx = x.Average(), my = y.Average();
        double num = 0, dx2 = 0, dy2 = 0;
        for (int i = 0; i < x.Length; i++)
        {
            double dx = x[i] - mx, dy = y[i] - my;
            num += dx * dy;
            dx2 += dx * dx;
            dy2 += dy * dy;
        }
        double denom = System.Math.Sqrt(dx2 * dy2);
        return denom > 0 ? num / denom : 0;
    }

    public DiagnosticCollector GetDiagnostics() => _diagnostics;
    public void ClearDiagnostics() => _diagnostics.Clear();
}
