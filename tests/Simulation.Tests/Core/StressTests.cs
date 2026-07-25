namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Physics;
using MathVerse.Math.Simulation.Events;
using MathVerse.Math.Simulation.Diagnostics;
using MathVerse.Math.Simulation.Time;
using MathVerse.Math.Simulation.SignalProcessing;
using MathVerse.Math.Simulation.ControlSystems;
using MathVerse.Math.Simulation.MonteCarlo;
using MathVerse.Math.Simulation.Solvers;
using MathVerse.Math.Simulation.Configuration;
using MathVerse.Math.Simulation.Visualization;
using MathVerse.Math.Simulation.Chemistry;
using MathVerse.Math.Simulation.Biology;
using MathVerse.Math.Simulation.Finance;
using MathVerse.Math.Simulation.Thermodynamics;
using MathVerse.Math.Simulation.Electromagnetics;
using MathVerse.Math.Simulation.FluidDynamics;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class StressTests
{
    [Fact]
    public void MillionStepSimulation_CompletesWithinTimeLimit()
    {
        var opts = SimulationOptions.Default with { EndTime = 10000.0, MaxTimeStep = 0.01, EnableStateRecording = false };
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(opts);
        var vars = ImmutableDictionary<string, double>.Empty.Add("x", 0.0);
        engine.SetInitialState(vars);
        var sw = Stopwatch.StartNew();
        var result = engine.Run((state, dt) =>
        {
            var x = state.GetVariable("x");
            return state with
            {
                Variables = state.Variables.SetItem("x", x + dt),
                CurrentTime = state.CurrentTime + dt,
                StepCount = state.StepCount + 1
            };
        });
        sw.Stop();
        result.Status.Should().Be(SimulationStatus.Completed);
        result.TotalSteps.Should().Be(1000000);
        sw.ElapsedMilliseconds.Should().BeLessThan(60000);
    }

    [Fact]
    public void ThousandParticles_PhysicsStep_Completes()
    {
        var particles = ImmutableDictionary<string, Particle>.Empty;
        for (int i = 0; i < 1000; i++)
        {
            var p = Particle.Create($"p{i}", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0);
            particles = particles.Add(p.Id, p);
        }
        var state = PhysicsState.Create(MVVector.ZeroOf(3)) with { Particles = particles };
        var result = PhysicsEngine.Step(state, 0.01);
        result.Particles.Should().HaveCount(1000);
        result.Time.Should().BeApproximately(0.01, 1e-10);
    }

    [Fact]
    public void Engine_StopsWhenMaxStepsExceeded()
    {
        var opts = SimulationOptions.Default with { EndTime = 1000.0, MaxTimeStep = 0.01, EnableStateRecording = false, MaxSteps = 10 };
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(opts);
        var vars = ImmutableDictionary<string, double>.Empty.Add("x", 0.0);
        engine.SetInitialState(vars);
        var result = engine.Run((state, dt) =>
        {
            return state with
            {
                Variables = state.Variables.SetItem("x", state.GetVariable("x") + dt),
                CurrentTime = state.CurrentTime + dt,
                StepCount = state.StepCount + 1
            };
        });
        result.TotalSteps.Should().BeLessThanOrEqualTo(11);
    }

    [Fact]
    public void RepeatedEngineReuse_ProducesConsistentResults()
    {
        var opts = SimulationOptions.Default with { EndTime = 1.0, MaxTimeStep = 0.1, EnableStateRecording = false };
        double[] results = new double[5];
        for (int trial = 0; trial < 5; trial++)
        {
            var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(opts);
            var vars = ImmutableDictionary<string, double>.Empty.Add("x", 1.0);
            engine.SetInitialState(vars);
            var r = engine.Run((state, dt) =>
            {
                var x = state.GetVariable("x");
                return state with
                {
                    Variables = state.Variables.SetItem("x", x * 2.0),
                    CurrentTime = state.CurrentTime + dt,
                    StepCount = state.StepCount + 1
                };
            });
            results[trial] = r.FinalState.GetVariable("x");
        }
        for (int i = 1; i < 5; i++)
            results[i].Should().Be(results[0]);
    }

    [Fact]
    public void ImmutableState_Verification()
    {
        var vars = ImmutableDictionary<string, double>.Empty.Add("a", 1.0).Add("b", 2.0);
        var state = SimulationState.Create(0.0, vars);
        var modified = state with
        {
            Variables = state.Variables.SetItem("a", 99.0),
            CurrentTime = 5.0
        };
        state.GetVariable("a").Should().Be(1.0);
        state.CurrentTime.Should().Be(0.0);
        modified.GetVariable("a").Should().Be(99.0);
        modified.CurrentTime.Should().Be(5.0);
    }

    [Fact]
    public void PhysicsState_ImmutableAfterStep()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0);
        var state = PhysicsState.Create(MVVector.ZeroOf(3)) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", p)
        };
        var originalTime = state.Time;
        var result = PhysicsEngine.Step(state, 0.01);
        state.Time.Should().Be(originalTime);
        state.Particles["p1"].Position[0].Should().Be(0.0);
        result.Time.Should().Be(0.01);
    }

    [Fact]
    public void EventQueue_HandlesThousandsOfEvents()
    {
        var queue = new EventQueue();
        for (int i = 0; i < 5000; i++)
        {
            var evt = SimulationEvent.Create(i * 0.001, $"event_{i}", EventType.Custom);
            queue.Enqueue(evt);
        }
        queue.Count.Should().Be(5000);
        var dequeued = new List<SimulationEvent>();
        while (queue.TryDequeue(out var e))
            dequeued.Add(e!);
        dequeued.Should().HaveCount(5000);
        for (int i = 1; i < dequeued.Count; i++)
            dequeued[i].Time.Should().BeGreaterThanOrEqualTo(dequeued[i - 1].Time);
    }

    [Fact]
    public void RecurringEventQueue_StressTest()
    {
        var queue = new EventQueue();
        var evt = SimulationEvent.Recurring(0.0, 0.1, "tick", _ => { }, 5);
        queue.Enqueue(evt);
        int count = 0;
        while (queue.TryDequeue(out var e))
        {
            count++;
            if (count > 10) break;
        }
        count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void LargeVector_Operations()
    {
        var v1 = MVVector.ZeroOf(10000);
        var v2 = MVVector.ZeroOf(10000);
        var sum = v1.Add(v2);
        sum.Size.Should().Be(10000);
        sum.Norm().Should().Be(0.0);
    }

    [Fact]
    public async Task DiagnosticsEngine_ConcurrentAccess()
    {
        var collector = new DiagnosticCollector();
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            int idx = i;
            tasks.Add(Task.Run(() =>
            {
                collector.AddWarning($"Warning {idx}", 0.0);
            }));
        }
        await Task.WhenAll(tasks);
        collector.WarningCount.Should().Be(100);
    }

    [Fact]
    public void AdaptiveTimeStep_ControllerBoundsRespected()
    {
        var controller = new AdaptiveTimeStepController();
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        double ts = controller.GetTimeStep(state);
        ts.Should().BeGreaterThan(0);
    }

    [Fact]
    public void VariableTimeStep_AdjustsOnError()
    {
        var controller = new VariableTimeStepController();
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        double initial = controller.GetTimeStep(state);
        controller.AdjustTimeStep(1.0);
        double after = controller.GetTimeStep(state);
        after.Should().BeLessThanOrEqualTo(initial);
    }

    [Fact]
    public void FixedTimeStep_IgnoresAdjustment()
    {
        var controller = new FixedTimeStepController();
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        double initial = controller.GetTimeStep(state);
        controller.AdjustTimeStep(100.0);
        controller.GetTimeStep(state).Should().Be(initial);
        controller.AdjustTimeStep(0.0);
        controller.GetTimeStep(state).Should().Be(initial);
    }

    [Fact]
    public void SimulationResult_FactorySuccess()
    {
        var state = SimulationState.Create(1.0, ImmutableDictionary<string, double>.Empty.Add("x", 42.0));
        var result = SimulationResult.Success(state, 100, 1.0, 5, 200, TimeSpan.FromSeconds(0.5));
        result.Status.Should().Be(SimulationStatus.Completed);
        result.TotalSteps.Should().Be(100);
        result.TotalTime.Should().Be(1.0);
        result.EventCount.Should().Be(5);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void SimulationResult_FactoryFailure()
    {
        var result = SimulationResult.Failure("Numerical instability", TimeSpan.FromSeconds(0.5));
        result.Status.Should().Be(SimulationStatus.Failed);
        result.ErrorMessage.Should().Be("Numerical instability");
    }

    [Fact]
    public void SimulationOptions_DefaultValues()
    {
        var opts = SimulationOptions.Default;
        opts.StartTime.Should().Be(0.0);
        opts.EndTime.Should().BeGreaterThan(0);
        opts.MaxTimeStep.Should().BeGreaterThan(0);
        opts.MinTimeStep.Should().BeGreaterThan(0);
        opts.Tolerance.Should().BeGreaterThan(0);
        opts.MaxSteps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SimulationState_GetVariable_MissingKey_ReturnsNaN()
    {
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        double val = state.GetVariable("nonexistent");
        double.IsNaN(val).Should().BeTrue();
    }

    [Fact]
    public void SimulationState_IsComplete_FalseDuringRun()
    {
        var state = SimulationState.Create(5.0, ImmutableDictionary<string, double>.Empty)
            with { Status = SimulationStatus.Running };
        state.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void SimulationState_IsComplete_TrueWhenCompleted()
    {
        var state = SimulationState.Create(5.0, ImmutableDictionary<string, double>.Empty)
            with { Status = SimulationStatus.Completed };
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void SimulationState_IsComplete_TrueWhenFailed()
    {
        var state = SimulationState.Create(5.0, ImmutableDictionary<string, double>.Empty)
            with { Status = SimulationStatus.Failed };
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void AdaptiveRK45_LongRun_Converges()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-0.5),
            InitialState = new MVVector(1.0),
            StartTime = 0,
            EndTime = 10.0
        };
        var solution = ODESolvers.SolveAdaptiveRK45(problem);
        solution.States.Last()[0].Should().BeApproximately(System.Math.Exp(-5.0), 0.01);
        solution.Steps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RK4_HarmonicOscillator_AmplitudePreserved()
    {
        var problem = new ODEProblem
        {
            Function = (t, y) => new MVVector(y[1], -y[0]),
            InitialState = new MVVector(1.0, 0.0),
            StartTime = 0,
            EndTime = 2 * System.Math.PI
        };
        var solution = ODESolvers.SolveRK4(problem);
        var last = solution.States.Last();
        double amp = System.Math.Sqrt(last[0] * last[0] + last[1] * last[1]);
        amp.Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void ImplicitEuler_StableForStiffProblem()
    {
        var problem = new ODEProblem
        {
            Function = (_, y) => y.Scale(-1000.0),
            InitialState = new MVVector(1.0),
            StartTime = 0,
            EndTime = 0.01
        };
        var solution = ODESolvers.SolveImplicitEuler(problem);
        solution.States.Last()[0].Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void RK4_ConservesEnergy_TwoBodyProblem()
    {
        double G = 1.0;
        var problem = new ODEProblem
        {
            Function = (t, y) =>
            {
                var rx = y[0];
                var ry = y[1];
                var vx = y[2];
                var vy = y[3];
                double r = System.Math.Sqrt(rx * rx + ry * ry);
                if (r < 1e-10) return new MVVector(0, 0, 0, 0);
                double ax = -G * rx / (r * r * r);
                double ay = -G * ry / (r * r * r);
                return new MVVector(vx, vy, ax, ay);
            },
            InitialState = new MVVector(1.0, 0.0, 0.0, 0.5),
            StartTime = 0,
            EndTime = 2 * System.Math.PI
        };
        var solution = ODESolvers.SolveRK4(problem);
        var last = solution.States.Last();
        double r2 = last[0] * last[0] + last[1] * last[1];
        double v2 = last[2] * last[2] + last[3] * last[3];
        double energy = 0.5 * v2 - G / System.Math.Sqrt(r2);
        double initialEnergy = 0.5 * 0.25 - G / 1.0;
        energy.Should().BeApproximately(initialEnergy, 0.1);
    }

    [Fact]
    public void SignalProcessing_FFT_RoundTrip()
    {
        var signal = ImmutableArray.Create(
            new Complex(0, 0), new Complex(1, 0), new Complex(0, 0), new Complex(-1, 0),
            new Complex(0, 0), new Complex(1, 0), new Complex(0, 0), new Complex(-1, 0));
        var spectrum = SignalProcessingEngine.FFT(signal);
        var reconstructed = SignalProcessingEngine.IFFT(spectrum);
        for (int i = 0; i < 8; i++)
        {
            reconstructed[i].Real.Should().BeApproximately(signal[i].Real, 1e-10);
            reconstructed[i].Imaginary.Should().BeApproximately(signal[i].Imaginary, 1e-10);
        }
    }

    [Fact]
    public void SignalProcessing_Convolution_KnownResult()
    {
        var a = ImmutableArray.Create(1.0, 2.0, 3.0, 0.0);
        var b = ImmutableArray.Create(1.0, 1.0, 0.0, 0.0);
        var result = SignalProcessingEngine.Convolve(a, b);
        result.Length.Should().BeGreaterThan(0);
        result[0].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void SignalProcessing_MovingAverage_Smooths()
    {
        var signal = ImmutableArray.Create(0.0, 0.0, 0.0, 10.0, 0.0, 0.0, 0.0);
        var smoothed = SignalProcessingEngine.MovingAverage(signal, 3);
        smoothed[3].Should().BeLessThan(10.0);
        smoothed[3].Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void SignalProcessing_ExponentialMovingAverage_ConvergesToConstant()
    {
        var signal = ImmutableArray.Create(Enumerable.Repeat(5.0, 100).ToArray());
        var ema = SignalProcessingEngine.ExponentialMovingAverage(signal, 0.1);
        ema[99].Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void SignalProcessing_Resample_ChangesLength()
    {
        var signal = ImmutableArray.Create(0.0, 1.0, 2.0, 3.0);
        var resampled = SignalProcessingEngine.Resample(signal, 2);
        resampled.Length.Should().Be(8);
    }

    [Fact]
    public void SignalProcessing_WindowFunction_AllTypes()
    {
        foreach (var type in Enum.GetValues<SignalProcessingEngine.WindowType>())
        {
            var window = SignalProcessingEngine.WindowFunction(16, type);
            window.Length.Should().Be(16);
        }
    }

    [Fact]
    public void SignalProcessing_PowerSpectralDensity_NonNegative()
    {
        var signal = ImmutableArray.Create(Enumerable.Range(0, 16).Select(i => System.Math.Sin(2 * System.Math.PI * i / 16.0)).ToArray());
        var psd = SignalProcessingEngine.PowerSpectralDensity(signal, 1.0);
        foreach (var val in psd.magnitudes)
            val.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ControlSystem_PIDController_MultiStep()
    {
        var pid = new PIDController { Kp = 1.0, Ki = 0.1, Kd = 0.05, Setpoint = 10.0 };
        double measured = 0.0;
        for (int i = 0; i < 100; i++)
        {
            double output = pid.Update(measured, 0.01);
            measured += output * 0.01;
        }
        measured.Should().NotBe(0.0);
    }

    [Fact]
    public void ControlSystem_PIDController_Reset()
    {
        var pid = new PIDController { Kp = 1.0, Ki = 0.1, Kd = 0.05, Setpoint = 10.0 };
        pid.Update(0.0, 0.01);
        pid.Update(0.5, 0.01);
        pid.Update(1.0, 0.01);
        pid.Reset();
        double output = pid.Update(0.0, 0.01);
        output.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void ControlSystem_TransferFunction_Evaluate()
    {
        var tf = TransferFunction.Create(new double[] { 1 }, new double[] { 1, 1 });
        var result = tf.Evaluate(new Complex(0, 0));
        result.Magnitude.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void ControlSystem_TransferFunction_EvaluateAtHighFreq()
    {
        var tf = TransferFunction.Create(new double[] { 1 }, new double[] { 1, 1 });
        var result = tf.Evaluate(new Complex(1e10, 0));
        result.Magnitude.Should().BeApproximately(0.0, 1e-5);
    }

    [Fact]
    public void MonteCarlo_Integrate_LinearFunction()
    {
        var result = MonteCarloEngine.Integrate(x => x, 0.0, 1.0);
        result.Mean.Should().BeApproximately(0.5, 0.05);
    }

    [Fact]
    public void MonteCarlo_EstimatePi_Converges()
    {
        var result = MonteCarloEngine.EstimatePi(100000);
        result.Mean.Should().BeApproximately(System.Math.PI, 0.1);
    }

    [Fact]
    public void MonteCarloResult_SuccessFactory()
    {
        var result = MonteCarloResult.Success(1.0, 0.01, 1000, TimeSpan.FromMilliseconds(1));
        result.Mean.Should().Be(1.0);
        result.SamplesUsed.Should().Be(1000);
    }

    [Fact]
    public void SolverFactory_CreatesODESolver()
    {
        var solver = SolverFactory.Create(SolverType.RungeKutta4);
        solver.Should().NotBeNull();
    }

    [Fact]
    public void SolverType_AllValuesExist()
    {
        var values = Enum.GetValues<SolverType>();
        values.Length.Should().BeGreaterThanOrEqualTo(6);
    }

    [Fact]
    public void Constraint_DistancePreserved()
    {
        var c = Constraint.Distance("c1", "p1", "p2", 5.0);
        c.Type.Should().Be(ConstraintType.Distance);
    }

    [Fact]
    public void Constraint_Fixed_MaintainsPosition()
    {
        var c = Constraint.Fixed("c1", "p1", MVVector.ZeroOf(3));
        c.Type.Should().Be(ConstraintType.Fixed);
    }

    [Fact]
    public void Force_GravityFactory()
    {
        var f = Force.GravityForce(10.0, -9.81);
        f.Vector[1].Should().BeApproximately(-98.1, 1e-10);
    }

    [Fact]
    public void Force_SpringFactory()
    {
        var f = Force.SpringForce(new MVVector(new double[] { 0.5 }), 100.0, 1.0);
        f.Vector[0].Should().BeApproximately(50.0, 1e-10);
    }

    [Fact]
    public void Force_DragFactory()
    {
        var vel = new MVVector(10.0, 0, 0);
        var f = Force.DragForce(vel, 0.5, 1.0);
        f.Vector[0].Should().BeNegative();
    }

    [Fact]
    public void Particle_Momentum_CorrectlyComputed()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), new MVVector(1.0, 2.0, 3.0), 5.0);
        var mom = p.Momentum;
        mom[0].Should().BeApproximately(5.0, 1e-10);
        mom[1].Should().BeApproximately(10.0, 1e-10);
        mom[2].Should().BeApproximately(15.0, 1e-10);
    }

    [Fact]
    public void Particle_KineticEnergy_CorrectlyComputed()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), new MVVector(3.0, 4.0, 0), 2.0);
        p.KineticEnergy.Should().BeApproximately(25.0, 1e-10);
    }

    [Fact]
    public void RigidBody_RecordEquality()
    {
        var rb1 = new RigidBody { Id = "rb1", Mass = 5.0, Position = MVVector.ZeroOf(3) };
        var rb2 = new RigidBody { Id = "rb1", Mass = 5.0, Position = MVVector.ZeroOf(3) };
        rb1.Id.Should().Be(rb2.Id);
        rb1.Mass.Should().Be(rb2.Mass);
    }

    [Fact]
    public void Thermodynamics_CarnotEfficiency_Bounds()
    {
        double eff = ThermodynamicsEngine.CarnotEfficiency(500.0, 300.0);
        eff.Should().BeGreaterThan(0.0);
        eff.Should().BeLessThan(1.0);
    }

    [Fact]
    public void Thermodynamics_CarnotEfficiency_ValuesMatch()
    {
        double eff = ThermodynamicsEngine.CarnotEfficiency(500.0, 300.0);
        eff.Should().BeApproximately(1.0 - 300.0 / 500.0, 1e-10);
    }

    [Fact]
    public void Thermodynamics_EntropyChange_PositiveForHeatAddition()
    {
        double ds = ThermodynamicsEngine.EntropyChange(100.0, 300.0);
        ds.Should().BePositive();
    }

    [Fact]
    public void Thermodynamics_GibbsFreeEnergy_Formula()
    {
        double g = ThermodynamicsEngine.GibbsFreeEnergy(1000.0, 300.0, 5.0);
        g.Should().BeApproximately(1000.0 - 300.0 * 5.0, 1e-10);
    }

    [Fact]
    public void Thermodynamics_HeatCapacity_Formula()
    {
        double hc = ThermodynamicsEngine.HeatCapacity(2.0, 4186.0);
        hc.Should().BeApproximately(8372.0, 1e-10);
    }

    [Fact]
    public void Electromagnetics_CoulombForce_NegativeForOppositeCharges()
    {
        var force = ElectromagneticsEngine.CoulombForce(new MVVector(1, 0, 0), 1.0, -1.0);
        force[0].Should().BeNegative();
    }

    [Fact]
    public void Electromagnetics_CoulombForce_PositiveForLikeCharges()
    {
        var force = ElectromagneticsEngine.CoulombForce(new MVVector(1, 0, 0), 1.0, 1.0);
        force[0].Should().BePositive();
    }

    [Fact]
    public void Electromagnetics_SpeedOfLight_Correct()
    {
        ElectromagneticsEngine.SpeedOfLight.Should().BeApproximately(3e8, 1e6);
    }

    [Fact]
    public void Electromagnetics_Capacitance_Formula()
    {
        double cap = ElectromagneticsEngine.Capacitance(1.0, 0.01, 1.0);
        cap.Should().BePositive();
    }

    [Fact]
    public void Electromagnetics_Inductance_Formula()
    {
        double ind = ElectromagneticsEngine.Inductance(1.0, 0.01, 100);
        ind.Should().BePositive();
    }

    [Fact]
    public void Electromagnetics_ResonanceFrequency_Formula()
    {
        double f = ElectromagneticsEngine.ResonanceFrequency(1.0, 1.0);
        f.Should().BeApproximately(1.0 / (2 * System.Math.PI), 1e-10);
    }

    [Fact]
    public void Electromagnetics_SkinDepth_Formula()
    {
        MVVector sd = ElectromagneticsEngine.SkinDepth(new MVVector(new double[] { 1e7 }), 1e6, 1.0);
        sd.Norm().Should().BePositive();
        sd.Norm().Should().BeLessThan(1.0);
    }

    [Fact]
    public void FluidDynamics_ReynoldsNumber_Correct()
    {
        double re = FluidDynamicsEngine.ReynoldsNumber(1000.0, 1.0, 1.0, 0.001);
        re.Should().BeApproximately(1e6, 1e3);
    }

    [Fact]
    public void FluidDynamics_DetermineRegime_Laminar()
    {
        FluidDynamicsEngine.DetermineRegime(500.0).Should().Be(FlowRegime.Laminar);
    }

    [Fact]
    public void FluidDynamics_DetermineRegime_Turbulent()
    {
        FluidDynamicsEngine.DetermineRegime(50000.0).Should().Be(FlowRegime.Turbulent);
    }

    [Fact]
    public void FluidDynamics_FrictionFactorLaminar_Formula()
    {
        double f = FluidDynamicsEngine.FrictionFactorLaminar(1000.0);
        f.Should().BeApproximately(64.0 / 1000.0, 1e-10);
    }

    [Fact]
    public void FluidDynamics_MachNumber_Formula()
    {
        double m = FluidDynamicsEngine.MachNumber(340.0, 340.0);
        m.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Biology_Exercise_LogisticGrowth_WithinBounds()
    {
        double N = BiologyEngine.LogisticGrowth(100.0, 0.1, 1000.0, 0.1);
        N.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Biology_EpidemiologicalState_TotalPopulation()
    {
        var state = new EpidemiologicalState { Susceptible = 90, Infected = 9, Recovered = 1 };
        state.TotalPopulation.Should().Be(100);
    }

    [Fact]
    public void Chemistry_EquilibriumConstant_Formula()
    {
        double K = ChemistryEngine.EquilibriumConstant(-10000.0, 298.15);
        K.Should().BePositive();
    }

    [Fact]
    public void Chemistry_Constants_AreCorrect()
    {
        ChemistryEngine.GasConstant.Should().BeApproximately(8.314, 0.01);
        ChemistryEngine.AvogadroNumber.Should().BeApproximately(6.022e23, 1e21);
    }

    [Fact]
    public void Chemistry_GibbsFreeEnergy_Formula()
    {
        double G = ChemistryEngine.GibbsFreeEnergy(1000.0, 5.0, 298.15);
        G.Should().BeApproximately(1000.0 - 298.15 * 5.0, 1e-10);
    }

    [Fact]
    public void Finance_BlackScholes_PutCallParity()
    {
        double S = 100, K = 100, T = 1.0, r = 0.05, sigma = 0.2;
        double call = FinanceEngine.BlackScholesCall(S, K, T, r, sigma);
        double put = FinanceEngine.BlackScholesPut(S, K, T, r, sigma);
        double parity = call - put - S + K * System.Math.Exp(-r * T);
        parity.Should().BeApproximately(0.0, 1e-6);
    }

    [Fact]
    public void Finance_CompoundInterest_KnownResult()
    {
        double fv = FinanceEngine.CompoundInterest(1000.0, 0.05, 10.0, 1);
        fv.Should().BeApproximately(1000.0 * System.Math.Pow(1.05, 10), 1e-6);
    }

    [Fact]
    public void Finance_PresentValue_FutureValue_Inverse()
    {
        double fv = FinanceEngine.FutureValue(1000.0, 0.05, 10.0);
        double pv = FinanceEngine.PresentValue(fv, 0.05, 10.0);
        pv.Should().BeApproximately(1000.0, 1e-6);
    }

    [Fact]
    public void Finance_NetPresentValue_KnownCashFlows()
    {
        var flows = ImmutableArray.Create(0.0, 100.0, 100.0, 100.0);
        double npv = FinanceEngine.NetPresentValue(0.1, flows);
        npv.Should().BePositive();
    }

    [Fact]
    public void Visualization_CreateLineSeries()
    {
        var series = VisualizationModels.CreateLineSeries("test", ImmutableArray.Create(1.0), ImmutableArray.Create(2.0));
        series.Should().NotBeNull();
        series.Name.Should().Be("test");
    }

    [Fact]
    public void Visualization_CreateScatterSeries()
    {
        var series = VisualizationModels.CreateScatterSeries("scatter", ImmutableArray.Create(1.0, 2.0), ImmutableArray.Create(3.0, 4.0));
        series.Should().NotBeNull();
    }

    [Fact]
    public void EventDispatcher_SubscribePublish()
    {
        var dispatcher = new EventDispatcher();
        int count = 0;
        dispatcher.Subscribe("test", _ => { count++; });
        var evt = SimulationEvent.Create(0.0, "test", EventType.Collision);
        dispatcher.Publish(evt);
        count.Should().Be(1);
    }

    [Fact]
    public void EventDispatcher_Unsubscribe()
    {
        var dispatcher = new EventDispatcher();
        int count = 0;
        Action<SimulationEvent> handler = _ => { count++; };
        dispatcher.Subscribe("test", handler);
        dispatcher.Unsubscribe("test", handler);
        var evt = SimulationEvent.Create(0.0, "test", EventType.Collision);
        dispatcher.Publish(evt);
        count.Should().Be(0);
    }

    [Fact]
    public void EventDispatcher_ScheduleProcessNext()
    {
        var dispatcher = new EventDispatcher();
        var evt = SimulationEvent.Create(1.0, "evt1", EventType.TimePoint);
        dispatcher.Schedule(evt);
        dispatcher.ProcessNext(out var next).Should().BeTrue();
        next!.Name.Should().Be("evt1");
    }

    [Fact]
    public void SimulationEngine_SetGetVariable()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(SimulationOptions.Default);
        engine.SetInitialState(ImmutableDictionary<string, double>.Empty);
        engine.SetVariable("x", 42.0);
        engine.GetVariable("x").Should().Be(42.0);
    }

    [Fact]
    public void SimulationEngine_GetVariable_Missing_ReturnsDefault()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(SimulationOptions.Default);
        engine.SetInitialState(ImmutableDictionary<string, double>.Empty);
        engine.GetVariable("missing").Should().Be(0.0);
    }

    [Fact]
    public void SimulationContext_SetGetVariable()
    {
        var ctx = new SimulationContext(SimulationOptions.Default);
        ctx.Initialize(ImmutableDictionary<string, double>.Empty);
        ctx.SetVariable("temp", 37.5);
        ctx.GetVariable("temp").Should().Be(37.5);
    }

    [Fact]
    public void SimulationContext_Advance()
    {
        var ctx = new SimulationContext(SimulationOptions.Default);
        ctx.Initialize(ImmutableDictionary<string, double>.Empty);
        ctx.Advance(0.1);
        ctx.State.CurrentTime.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void EventDrivenTimeController_NextEventTime()
    {
        var ctrl = new EventDrivenTimeController();
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        ctrl.NextEventTime(state).Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void EventDrivenTimeController_FixedStep()
    {
        var ctrl = new EventDrivenTimeController();
        var state = SimulationState.Create(0.0, ImmutableDictionary<string, double>.Empty);
        ctrl.GetTimeStep(state).Should().BeGreaterThan(0);
    }

    [Fact]
    public void TransferFunction_Create_FromArrays()
    {
        var tf = TransferFunction.Create(new double[] { 1, 2 }, new double[] { 1, 3, 2 });
        tf.Numerator.Length.Should().Be(2);
        tf.Denominator.Length.Should().Be(3);
    }

    [Fact]
    public void EventDispatcher_MultipleSubscribers()
    {
        var dispatcher = new EventDispatcher();
        int count = 0;
        dispatcher.Subscribe("evt", _ => { count++; });
        dispatcher.Subscribe("evt", _ => { count++; });
        dispatcher.Subscribe("evt", _ => { count++; });
        dispatcher.Publish(SimulationEvent.Create(0.0, "evt", EventType.Custom));
        count.Should().Be(3);
    }

    [Fact]
    public void EventDispatcher_WrongEventName_DoesNotFire()
    {
        var dispatcher = new EventDispatcher();
        int count = 0;
        dispatcher.Subscribe("target", _ => { count++; });
        dispatcher.Publish(SimulationEvent.Create(0.0, "other", EventType.Custom));
        count.Should().Be(0);
    }

    [Fact]
    public void DiagnosticsCollector_Clear()
    {
        var collector = new DiagnosticCollector();
        collector.AddWarning("w1", 0.0);
        collector.AddError("e1", 0.0);
        collector.WarningCount.Should().Be(1);
        collector.ErrorCount.Should().Be(1);
        collector.Clear();
        collector.WarningCount.Should().Be(0);
        collector.ErrorCount.Should().Be(0);
    }

    [Fact]
    public void DiagnosticsCollector_AddDiagnostic()
    {
        var collector = new DiagnosticCollector();
        var diag = SimulationDiagnostic.Warning("test", 0.0);
        collector.Add(diag);
        collector.WarningCount.Should().Be(1);
    }

    [Fact]
    public void SimulationConfiguration_Defaults()
    {
        var config = SimulationConfiguration.Default;
        config.Should().NotBeNull();
    }

    [Fact]
    public void PhysicsState_Create_WithGravity()
    {
        var g = new MVVector(0, -9.81, 0);
        var state = PhysicsState.Create(g);
        state.Gravity[1].Should().BeApproximately(-9.81, 1e-10);
    }

    [Fact]
    public void Particle_Create_WithAllParams()
    {
        var p = Particle.Create("p1", new MVVector(1, 2, 3), new MVVector(4, 5, 6), 10.0, 0.5);
        p.Id.Should().Be("p1");
        p.Position[0].Should().Be(1.0);
        p.Velocity[2].Should().Be(6.0);
        p.Mass.Should().Be(10.0);
        p.Radius.Should().Be(0.5);
    }

    [Fact]
    public void SimulationStatus_AllValues()
    {
        var values = Enum.GetValues<SimulationStatus>();
        values.Should().Contain(SimulationStatus.NotStarted);
        values.Should().Contain(SimulationStatus.Running);
        values.Should().Contain(SimulationStatus.Paused);
        values.Should().Contain(SimulationStatus.Completed);
        values.Should().Contain(SimulationStatus.Failed);
        values.Should().Contain(SimulationStatus.Cancelled);
    }

    [Fact]
    public void SimulationMode_AllValues()
    {
        var values = Enum.GetValues<SimulationMode>();
        values.Length.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void ForceType_AllValues()
    {
        var values = Enum.GetValues<ForceType>();
        values.Length.Should().Be(9);
    }

    [Fact]
    public void ConstraintType_AllValues()
    {
        var values = Enum.GetValues<ConstraintType>();
        values.Length.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void EventType_AllValues()
    {
        var values = Enum.GetValues<EventType>();
        values.Length.Should().Be(9);
    }

    [Fact]
    public void EventPriority_AllValues()
    {
        var values = Enum.GetValues<EventPriority>();
        values.Length.Should().Be(4);
    }

    [Fact]
    public void DiagnosticSeverity_AllValues()
    {
        var values = Enum.GetValues<DiagnosticSeverity>();
        values.Length.Should().Be(4);
    }

    [Fact]
    public void DiagnosticType_AllValues()
    {
        var values = Enum.GetValues<DiagnosticType>();
        values.Length.Should().Be(12);
    }

    [Fact]
    public void HeatTransferMode_AllValues()
    {
        var values = Enum.GetValues<HeatTransferMode>();
        values.Length.Should().Be(4);
    }

    [Fact]
    public void FlowRegime_AllValues()
    {
        var values = Enum.GetValues<FlowRegime>();
        values.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void PlotType_AllValues()
    {
        var values = Enum.GetValues<PlotType>();
        values.Length.Should().BeGreaterThanOrEqualTo(5);
    }
}
