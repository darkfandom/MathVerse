using BenchmarkDotNet.Attributes;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Time;
using MathVerse.Math.Simulation.Configuration;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class EngineAndConfigBenchmarks
{
    private SimulationOptions _options = null!;
    private SimulationOptions _customOptions = null!;
    private SimulationConfiguration _config = null!;
    private SimulationState _state = null!;
    private ImmutableDictionary<string, double> _variables = null!;
    private FixedTimeStepController _fixedController = null!;
    private VariableTimeStepController _variableController = null!;
    private AdaptiveTimeStepController _adaptiveController = null!;
    private EventDrivenTimeController _eventController = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = SimulationOptions.Default;
        _customOptions = new SimulationOptions
        {
            StartTime = 0.0,
            EndTime = 1.0,
            MaxTimeStep = 0.01,
            MinTimeStep = 1e-8,
            Tolerance = 1e-8,
            Mode = SimulationMode.AdaptiveTimeStep,
            EnableEventDetection = true,
            EnableStateRecording = true,
            RecordingInterval = 5,
            MaxSteps = 100000
        };
        _config = SimulationConfiguration.Default;
        _variables = ImmutableDictionary<string, double>.Empty
            .Add("x", 1.0).Add("y", 2.0).Add("z", 3.0)
            .Add("vx", 0.5).Add("vy", -0.3).Add("vz", 0.1);
        _state = SimulationState.Create(0.0, _variables);
        _fixedController = new FixedTimeStepController();
        _variableController = new VariableTimeStepController();
        _adaptiveController = new AdaptiveTimeStepController();
        _eventController = new EventDrivenTimeController();
    }

    [Benchmark]
    public MathVerse.Math.Simulation.Core.SimulationEngine SimulationEngine_Create()
    {
        return new MathVerse.Math.Simulation.Core.SimulationEngine();
    }

    [Benchmark]
    public void SimulationEngine_SetInitialState()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine();
        engine.SetInitialState(_variables);
    }

    [Benchmark]
    public double SimulationEngine_SetGetVariable()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine();
        engine.SetVariable("x", 42.0);
        return engine.GetVariable("x");
    }

    [Benchmark]
    public SimulationResult SimulationEngine_Run_TinySimulation()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(new SimulationOptions
        {
            StartTime = 0.0,
            EndTime = 0.1,
            MaxTimeStep = 0.01,
            MaxSteps = 10,
            Mode = SimulationMode.FixedTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = false
        });
        engine.SetInitialState(_variables);
        return engine.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            Variables = state.Variables.SetItem("x", state.Variables["x"] + dt),
            LastTimeStep = dt
        });
    }

    [Benchmark]
    public SimulationResult SimulationEngine_Run_MediumSimulation()
    {
        var engine = new MathVerse.Math.Simulation.Core.SimulationEngine(new SimulationOptions
        {
            StartTime = 0.0,
            EndTime = 10.0,
            MaxTimeStep = 0.1,
            MaxSteps = 100,
            Mode = SimulationMode.FixedTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = false
        });
        engine.SetInitialState(_variables);
        return engine.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            Variables = state.Variables.SetItem("x", state.Variables["x"] + dt * state.Variables["vx"]),
            LastTimeStep = dt
        });
    }

    [Benchmark]
    public SimulationOptions SimulationOptions_Defaults()
    {
        return SimulationOptions.Default;
    }

    [Benchmark]
    public SimulationOptions SimulationOptions_CreateCustom()
    {
        return new SimulationOptions
        {
            StartTime = 0.5,
            EndTime = 5.0,
            MaxTimeStep = 0.05,
            MinTimeStep = 1e-7,
            Tolerance = 1e-7,
            Mode = SimulationMode.VariableTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = true,
            RecordingInterval = 10,
            MaxSteps = 500000,
            EnableParallelExecution = true
        };
    }

    [Benchmark]
    public SimulationOptions SimulationOptions_RecordCopy()
    {
        return _customOptions with { EndTime = 20.0 };
    }

    [Benchmark]
    public SimulationState SimulationState_Create()
    {
        return SimulationState.Create(0.0, _variables);
    }

    [Benchmark]
    public double SimulationState_GetVariable()
    {
        return _state.GetVariable("x");
    }

    [Benchmark]
    public SimulationState SimulationState_SetVariable()
    {
        return _state with { Variables = _state.Variables.SetItem("x", 99.0) };
    }

    [Benchmark]
    public bool SimulationState_IsComplete()
    {
        return _state.IsComplete;
    }

    [Benchmark]
    public SimulationState SimulationState_RecordWith()
    {
        return _state with
        {
            CurrentTime = 1.0,
            StepCount = 100,
            Status = SimulationStatus.Running
        };
    }

    [Benchmark]
    public SimulationContext SimulationContext_Create()
    {
        return new SimulationContext();
    }

    [Benchmark]
    public double SimulationContext_SetGetVariable()
    {
        var ctx = new SimulationContext();
        ctx.SetVariable("x", 5.0);
        return ctx.GetVariable("x");
    }

    [Benchmark]
    public void SimulationContext_Advance()
    {
        var ctx = new SimulationContext();
        ctx.Initialize(_variables);
        ctx.Advance(0.01);
    }

    [Benchmark]
    public SimulationResult SimulationResult_SuccessFactory()
    {
        return SimulationResult.Success(_state, 100, 1.0, 5, 200, TimeSpan.FromMilliseconds(42));
    }

    [Benchmark]
    public SimulationResult SimulationResult_FailureFactory()
    {
        return SimulationResult.Failure("Test failure", TimeSpan.FromMilliseconds(10));
    }

    [Benchmark]
    public double FixedTimeStep_GetTimeStep()
    {
        return _fixedController.GetTimeStep(_state);
    }

    [Benchmark]
    public void FixedTimeStep_Advance()
    {
        var state = _state;
        _fixedController.Advance(ref state, 0.01);
    }

    [Benchmark]
    public double FixedTimeStep_Adjust()
    {
        return _fixedController.EstimateError(_state);
    }

    [Benchmark]
    public double VariableTimeStep_GetTimeStep()
    {
        return _variableController.GetTimeStep(_state);
    }

    [Benchmark]
    public void VariableTimeStep_Adjust_10Steps()
    {
        for (int i = 0; i < 10; i++)
            _variableController.AdjustTimeStep(1e-7 * (i + 1));
    }

    [Benchmark]
    public double AdaptiveTimeStep_GetTimeStep()
    {
        return _adaptiveController.GetTimeStep(_state);
    }

    [Benchmark]
    public void AdaptiveTimeStep_Adjust_10Steps()
    {
        for (int i = 0; i < 10; i++)
            _adaptiveController.AdjustTimeStep(1e-7 * (i + 1));
    }

    [Benchmark]
    public double EventDrivenTimeController_GetTimeStep()
    {
        return _eventController.GetTimeStep(_state);
    }

    [Benchmark]
    public double EventDrivenTimeController_NextEventTime()
    {
        return _eventController.NextEventTime(_state);
    }

    [Benchmark]
    public SimulationConfiguration SimulationConfiguration_Default()
    {
        return SimulationConfiguration.Default;
    }

    [Benchmark]
    public PhysicsConfiguration PhysicsConfiguration_Default()
    {
        return PhysicsConfiguration.Default;
    }
}
