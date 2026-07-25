namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;

public sealed class SimulationStateTests
{
    [Fact]
    public void Create_InitialState_HasCorrectTime()
    {
        var vars = ImmutableDictionary<string, double>.Empty;
        var state = SimulationState.Create(1.5, vars);
        state.CurrentTime.Should().Be(1.5);
    }

    [Fact]
    public void Create_InitialState_HasZeroStepCount()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.StepCount.Should().Be(0);
    }

    [Fact]
    public void Create_InitialState_StatusIsNotStarted()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.Status.Should().Be(SimulationStatus.NotStarted);
    }

    [Fact]
    public void Create_InitialState_HasEmptyVariables()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.Variables.Should().BeEmpty();
    }

    [Fact]
    public void Create_InitialState_HasEmptyHistory()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.History.Should().BeEmpty();
    }

    [Fact]
    public void Create_InitialState_LastTimeStepIsZero()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.LastTimeStep.Should().Be(0);
    }

    [Fact]
    public void Create_WithVariables_StoresCorrectly()
    {
        var vars = ImmutableDictionary<string, double>.Empty
            .Add("x", 1.0)
            .Add("v", 2.5);
        var state = SimulationState.Create(0, vars);
        state.Variables.Should().HaveCount(2);
        state.Variables["x"].Should().Be(1.0);
        state.Variables["v"].Should().Be(2.5);
    }

    [Fact]
    public void GetVariable_ExistingVariable_ReturnsValue()
    {
        var vars = ImmutableDictionary<string, double>.Empty.Add("temperature", 300.0);
        var state = SimulationState.Create(0, vars);
        state.GetVariable("temperature").Should().Be(300.0);
    }

    [Fact]
    public void GetVariable_NonExistingVariable_ReturnsNaN()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.GetVariable("missing").Should().Be(double.NaN);
    }

    [Fact]
    public void IsComplete_WhenNotStarted_IsFalse()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WhenRunning_IsFalse()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Running
        };
        state.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WhenCompleted_IsTrue()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Completed
        };
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_WhenFailed_IsTrue()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Failed
        };
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void StatusTransitions_NotStartedToRunning()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.Status.Should().Be(SimulationStatus.NotStarted);
        state = state with { Status = SimulationStatus.Running };
        state.Status.Should().Be(SimulationStatus.Running);
    }

    [Fact]
    public void StatusTransitions_RunningToCompleted()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Running
        };
        state = state with { Status = SimulationStatus.Completed };
        state.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void StatusTransitions_RunningToFailed()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Running
        };
        state = state with { Status = SimulationStatus.Failed, ErrorMessage = "error" };
        state.Status.Should().Be(SimulationStatus.Failed);
        state.ErrorMessage.Should().Be("error");
    }

    [Fact]
    public void StatusTransitions_RunningToCancelled()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Running
        };
        state = state with { Status = SimulationStatus.Cancelled };
        state.Status.Should().Be(SimulationStatus.Cancelled);
    }

    [Fact]
    public void Create_WithNegativeTime_SetsCorrectly()
    {
        var state = SimulationState.Create(-5.0, ImmutableDictionary<string, double>.Empty);
        state.CurrentTime.Should().Be(-5.0);
    }

    [Fact]
    public void CustomData_DefaultIsEmpty()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.CustomData.Should().BeEmpty();
    }

    [Fact]
    public void ErrorMessage_DefaultIsNull()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        state.ErrorMessage.Should().BeNull();
    }
}
