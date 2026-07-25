namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;

public sealed class SimulationContextTests
{
    [Fact]
    public void Constructor_DefaultOptions_CreatesContext()
    {
        var ctx = new SimulationContext();
        ctx.Options.Should().Be(SimulationOptions.Default);
    }

    [Fact]
    public void Constructor_NullOptions_UsesDefault()
    {
        var ctx = new SimulationContext(null);
        ctx.Options.Should().Be(SimulationOptions.Default);
    }

    [Fact]
    public void Constructor_CustomOptions_StoresOptions()
    {
        var opts = new SimulationOptions { StartTime = 1.0, EndTime = 5.0 };
        var ctx = new SimulationContext(opts);
        ctx.Options.StartTime.Should().Be(1.0);
        ctx.Options.EndTime.Should().Be(5.0);
    }

    [Fact]
    public void Constructor_CreatesEngine()
    {
        var ctx = new SimulationContext();
        ctx.Engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CreatesTimeController()
    {
        var ctx = new SimulationContext();
        ctx.TimeController.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitialState_IsNotStarted()
    {
        var ctx = new SimulationContext();
        ctx.State.Status.Should().Be(SimulationStatus.NotStarted);
    }

    [Fact]
    public void Constructor_InitialState_HasZeroTime()
    {
        var ctx = new SimulationContext();
        ctx.State.CurrentTime.Should().Be(0);
    }

    [Fact]
    public void Initialize_SetsVariables()
    {
        var ctx = new SimulationContext();
        var vars = ImmutableDictionary<string, double>.Empty.Add("x", 1.0).Add("v", 2.0);
        ctx.Initialize(vars);
        ctx.State.Variables.Should().HaveCount(2);
        ctx.State.Variables["x"].Should().Be(1.0);
    }

    [Fact]
    public void Initialize_ResetsStatusToNotStarted()
    {
        var ctx = new SimulationContext();
        ctx.Initialize(ImmutableDictionary<string, double>.Empty);
        ctx.State.Status.Should().Be(SimulationStatus.NotStarted);
    }

    [Fact]
    public void Initialize_ResetsStepCount()
    {
        var ctx = new SimulationContext();
        ctx.Initialize(ImmutableDictionary<string, double>.Empty);
        ctx.State.StepCount.Should().Be(0);
    }

    [Fact]
    public void Initialize_UpdatesState()
    {
        var ctx = new SimulationContext();
        var before = ctx.State;
        ctx.Initialize(ImmutableDictionary<string, double>.Empty.Add("y", 42.0));
        ctx.State.Should().NotBeSameAs(before);
    }

    [Fact]
    public void SetVariable_SetsOnEngine()
    {
        var ctx = new SimulationContext();
        ctx.SetVariable("temperature", 300.0);
        ctx.GetVariable("temperature").Should().Be(300.0);
    }

    [Fact]
    public void SetVariable_MultipleVariables()
    {
        var ctx = new SimulationContext();
        ctx.SetVariable("x", 1.0);
        ctx.SetVariable("y", 2.0);
        ctx.GetVariable("x").Should().Be(1.0);
        ctx.GetVariable("y").Should().Be(2.0);
    }

    [Fact]
    public void GetVariable_NonExisting_ReturnsZero()
    {
        var ctx = new SimulationContext();
        ctx.GetVariable("missing").Should().Be(0.0);
    }

    [Fact]
    public void Run_SimpleStepFunction_Completes()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 0,
            EndTime = 0.1,
            MaxTimeStep = 0.05,
            MaxSteps = 100
        });

        var result = ctx.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            StepCount = state.StepCount + 1
        });

        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Run_SimpleStepFunction_ReturnsPositiveSteps()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 0,
            EndTime = 0.1,
            MaxTimeStep = 0.05,
            MaxSteps = 100
        });

        var result = ctx.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            StepCount = state.StepCount + 1
        });

        result.TotalSteps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Run_SimpleStepFunction_RecordsEvents()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 0,
            EndTime = 0.1,
            MaxTimeStep = 0.05,
            MaxSteps = 100,
            EnableEventDetection = false
        });

        var result = ctx.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            StepCount = state.StepCount + 1
        });

        result.EventCount.Should().Be(0);
    }

    [Fact]
    public void Run_RecordsState_WhenRecordingEnabled()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 0,
            EndTime = 0.5,
            MaxTimeStep = 0.1,
            MaxSteps = 100,
            EnableStateRecording = true,
            RecordingInterval = 1,
            EnableEventDetection = false
        });

        ctx.SetVariable("x", 1.0);
        var result = ctx.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime + dt,
            StepCount = state.StepCount + 1,
            Variables = state.Variables.SetItem("x", state.Variables["x"] + 1)
        });

        result.FinalState.History.Should().NotBeEmpty();
    }

    [Fact]
    public void Run_WithNegativeTimeStep_Completes()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 10,
            EndTime = 0,
            MaxTimeStep = 1,
            MaxSteps = 100
        });

        var result = ctx.Run((state, dt) => state with
        {
            CurrentTime = state.CurrentTime - dt,
            StepCount = state.StepCount + 1
        });

        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Advance_IncreasesTime()
    {
        var ctx = new SimulationContext();
        ctx.State.CurrentTime.Should().Be(0);
        ctx.Advance(0.01);
        ctx.State.CurrentTime.Should().Be(0.01);
    }

    [Fact]
    public void Advance_MultipleTimes_AccumulatesTime()
    {
        var ctx = new SimulationContext();
        ctx.Advance(0.01);
        ctx.Advance(0.02);
        ctx.Advance(0.03);
        ctx.State.CurrentTime.Should().BeApproximately(0.06, 1e-10);
    }

    [Fact]
    public void Advance_UpdatesLastTimeStep()
    {
        var ctx = new SimulationContext();
        ctx.Advance(0.05);
        ctx.State.LastTimeStep.Should().Be(0.05);
    }

    [Fact]
    public void Run_FailingStepFunction_ReturnsFailure()
    {
        var ctx = new SimulationContext(new SimulationOptions
        {
            StartTime = 0,
            EndTime = 1.0,
            MaxTimeStep = 0.1,
            MaxSteps = 100
        });

        var result = ctx.Run((state, dt) => throw new InvalidOperationException("test error"));

        result.Status.Should().Be(SimulationStatus.Failed);
        result.ErrorMessage.Should().Be("test error");
    }
}
