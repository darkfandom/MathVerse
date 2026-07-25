namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;
using CoreEngine = MathVerse.Math.Simulation.Core.SimulationEngine;

public sealed class SimulationEngineAdvancedTests
{
    [Fact]
    public void Constructor_WithOptions_CreatesSuccessfully()
    {
        var opts = new SimulationOptions { StartTime = 0, EndTime = 1 };
        var engine = new CoreEngine(opts);
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_DefaultOptions_CreatesSuccessfully()
    {
        var engine = new CoreEngine();
        engine.Should().NotBeNull();
    }

    [Fact]
    public void SetVariable_StoresValue()
    {
        var engine = new CoreEngine();
        engine.SetVariable("x", 42.0);
        engine.GetVariable("x").Should().Be(42.0);
    }

    [Fact]
    public void SetVariable_OverwritesValue()
    {
        var engine = new CoreEngine();
        engine.SetVariable("x", 1.0);
        engine.SetVariable("x", 2.0);
        engine.GetVariable("x").Should().Be(2.0);
    }

    [Fact]
    public void GetVariable_NonExisting_ReturnsZero()
    {
        var engine = new CoreEngine();
        engine.GetVariable("nonexistent").Should().Be(0.0);
    }

    [Fact]
    public void Run_CompletesWithCorrectStatus()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_ReturnsPositiveSteps()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.TotalSteps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_ExceptionReturnsFailure()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 1.0, MaxTimeStep = 0.1, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => throw new InvalidOperationException("boom"));
        result.Status.Should().Be(SimulationStatus.Failed);
        result.ErrorMessage.Should().Be("boom");
    }

    [Fact]
    public void Run_MaxStepsLimitsExecution()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 1000, MaxTimeStep = 0.01, MaxSteps = 5
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.TotalSteps.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void Run_EndTimeZero_CompletesImmediately()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0, MaxTimeStep = 0.1
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void Run_AccumulatesVariable()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1, MaxSteps = 100
        });
        engine.SetVariable("x", 0);
        var result = engine.Run((s, dt) =>
        {
            var newX = s.Variables["x"] + 1.0;
            return s with
            {
                CurrentTime = s.CurrentTime + dt,
                StepCount = s.StepCount + 1,
                Variables = s.Variables.SetItem("x", newX)
            };
        });
        result.FinalState.Variables["x"].Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_RecordsHistory_WhenEnabled()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1,
            EnableStateRecording = true, RecordingInterval = 1,
            MaxSteps = 100, EnableEventDetection = false
        });
        engine.SetVariable("x", 1.0);
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1,
            Variables = s.Variables.SetItem("x", s.Variables["x"] + 1)
        });
        result.FinalState.History.Should().NotBeEmpty();
    }

    [Fact]
    public void Run_EventDriven_Completes()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1,
            Mode = SimulationMode.EventDriven, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_AdaptiveTimeStep_Completes()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1,
            Mode = SimulationMode.AdaptiveTimeStep, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_VariableTimeStep_Completes()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1,
            Mode = SimulationMode.VariableTimeStep, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_TimeAdvances()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.TotalTime.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_ExecutionTime_IsNonNegative()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.1, MaxTimeStep = 0.05, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.ExecutionTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void Run_FunctionEvaluations_Positive()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 0.5, MaxTimeStep = 0.1, MaxSteps = 100
        });
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1
        });
        result.FunctionEvaluations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_WithStateRecording_RecordingInterval()
    {
        var engine = new CoreEngine(new SimulationOptions
        {
            StartTime = 0, EndTime = 1.0, MaxTimeStep = 0.1,
            EnableStateRecording = true, RecordingInterval = 5,
            MaxSteps = 1000, EnableEventDetection = false
        });
        engine.SetVariable("v", 1.0);
        var result = engine.Run((s, dt) => s with
        {
            CurrentTime = s.CurrentTime + dt,
            StepCount = s.StepCount + 1,
            Variables = s.Variables.SetItem("v", s.Variables["v"] + 1)
        });
        result.Status.Should().Be(SimulationStatus.Completed);
    }
}
