namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;
using CoreSimulationEngine = MathVerse.Math.Simulation.Core.SimulationEngine;

public sealed class SimulationEngineTests
{
    [Fact]
    public void Constructor_WithDefaultOptions_CreatesSuccessfully()
    {
        var engine = new CoreSimulationEngine();
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomOptions_UsesProvidedOptions()
    {
        var options = new SimulationOptions { EndTime = 5.0 };
        var engine = new CoreSimulationEngine(options);
        engine.Should().NotBeNull();
    }

    [Fact]
    public void SetVariable_And_GetVariable_RoundTrips()
    {
        var engine = new CoreSimulationEngine();
        engine.SetVariable("x", 42.0);
        engine.GetVariable("x").Should().Be(42.0);
    }

    [Fact]
    public void GetVariable_NonExisting_ReturnsZero()
    {
        var engine = new CoreSimulationEngine();
        engine.GetVariable("missing").Should().Be(0.0);
    }

    [Fact]
    public void SetVariable_OverwritesPreviousValue()
    {
        var engine = new CoreSimulationEngine();
        engine.SetVariable("x", 1.0);
        engine.SetVariable("x", 2.0);
        engine.GetVariable("x").Should().Be(2.0);
    }

    [Fact]
    public void Run_SimpleStepFunction_CompletesSuccessfully()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.1,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) =>
        {
            var newTime = state.CurrentTime + dt;
            return state with { CurrentTime = newTime };
        });

        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_SimpleStepFunction_ReturnsPositiveStepCount()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.1,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.TotalSteps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_SimpleStepFunction_RecordsHistory()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 0.5,
            MaxTimeStep = 0.1,
            EnableEventDetection = false,
            EnableStateRecording = true,
            RecordingInterval = 1
        });

        engine.SetVariable("x", 0.0);
        var result = engine.Run((state, dt) =>
        {
            var x = state.Variables["x"] + 1.0;
            return state with
            {
                CurrentTime = state.CurrentTime + dt,
                Variables = state.Variables.SetItem("x", x)
            };
        });

        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_MaxStepsLimitsExecution()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1000.0,
            MaxSteps = 5,
            MaxTimeStep = 1.0,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.TotalSteps.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public void Run_WithException_ReturnsFailure()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.1,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => throw new InvalidOperationException("test error"));
        result.Status.Should().Be(SimulationStatus.Failed);
        result.ErrorMessage.Should().Be("test error");
    }

    [Fact]
    public void Run_WithException_ReturnsZeroSteps()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.1,
            EnableEventDetection = false
        });

        var result = engine.Run((state, dt) => throw new InvalidOperationException("boom"));
        result.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void Run_FunctionEvaluations_EqualsStepCount()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.25,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.FunctionEvaluations.Should().Be(result.TotalSteps);
    }

    [Fact]
    public void Run_TimeAdvances()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 2.0,
            MaxTimeStep = 0.5,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.TotalTime.Should().BeGreaterOrEqualTo(2.0);
    }

    [Fact]
    public void Run_AccumulatesVariable()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            MaxTimeStep = 0.25,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        engine.SetVariable("counter", 0.0);
        engine.Run((state, dt) =>
        {
            var counter = state.Variables["counter"] + 1.0;
            return state with
            {
                CurrentTime = state.CurrentTime + dt,
                Variables = state.Variables.SetItem("counter", counter)
            };
        });

        engine.GetVariable("counter").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_VariableTimeStep_Completes()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            Mode = SimulationMode.VariableTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_AdaptiveTimeStep_Completes()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            Mode = SimulationMode.AdaptiveTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_EventDriven_Completes()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 1.0,
            Mode = SimulationMode.EventDriven,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_EmptyStepFunction_Completes()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 0.5,
            MaxTimeStep = 0.1,
            EnableEventDetection = false,
            EnableStateRecording = false
        });

        var result = engine.Run((state, dt) => state);
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_EndTimeZero_CompletesImmediately()
    {
        var engine = new CoreSimulationEngine(new SimulationOptions
        {
            EndTime = 0.0,
            MaxTimeStep = 0.1,
            EnableEventDetection = false
        });

        var result = engine.Run((state, dt) => state with { CurrentTime = state.CurrentTime + dt });
        result.Status.Should().Be(SimulationStatus.Completed);
        result.TotalSteps.Should().Be(0);
    }
}
