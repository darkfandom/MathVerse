namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;

public sealed class SimulationResultTests
{
    [Fact]
    public void Success_SetsStatusToCompleted()
    {
        var state = SimulationState.Create(5.0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 100, 5.0, 0, 200, TimeSpan.FromMilliseconds(50));
        result.Status.Should().Be(SimulationStatus.Completed);
    }

    [Fact]
    public void Success_SetsFinalState()
    {
        var state = SimulationState.Create(10.0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 50, 10.0, 2, 100, TimeSpan.FromSeconds(1));
        result.FinalState.Should().BeSameAs(state);
    }

    [Fact]
    public void Success_SetsTotalSteps()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 42, 0, 0, 42, TimeSpan.Zero);
        result.TotalSteps.Should().Be(42);
    }

    [Fact]
    public void Success_SetsTotalTime()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 0, 7.5, 0, 0, TimeSpan.Zero);
        result.TotalTime.Should().Be(7.5);
    }

    [Fact]
    public void Success_SetsEventCount()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 0, 0, 15, 0, TimeSpan.Zero);
        result.EventCount.Should().Be(15);
    }

    [Fact]
    public void Success_SetsFunctionEvaluations()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 0, 0, 0, 999, TimeSpan.Zero);
        result.FunctionEvaluations.Should().Be(999);
    }

    [Fact]
    public void Success_SetsExecutionTime()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var duration = TimeSpan.FromMilliseconds(123.45);
        var result = SimulationResult.Success(state, 0, 0, 0, 0, duration);
        result.ExecutionTime.Should().Be(duration);
    }

    [Fact]
    public void Failure_SetsStatusToFailed()
    {
        var result = SimulationResult.Failure("diverged", TimeSpan.FromSeconds(2));
        result.Status.Should().Be(SimulationStatus.Failed);
    }

    [Fact]
    public void Failure_SetsErrorMessage()
    {
        var result = SimulationResult.Failure("numerical instability", TimeSpan.Zero);
        result.ErrorMessage.Should().Be("numerical instability");
    }

    [Fact]
    public void Failure_SetsExecutionTime()
    {
        var duration = TimeSpan.FromMilliseconds(99);
        var result = SimulationResult.Failure("timeout", duration);
        result.ExecutionTime.Should().Be(duration);
    }

    [Fact]
    public void Failure_FinalState_HasDefaultValues()
    {
        var result = SimulationResult.Failure("error", TimeSpan.Zero);
        result.FinalState.Should().NotBeNull();
        result.FinalState.Status.Should().Be(SimulationStatus.NotStarted);
    }

    [Fact]
    public void Failure_TotalSteps_IsDefault()
    {
        var result = SimulationResult.Failure("error", TimeSpan.Zero);
        result.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void Failure_TotalTime_IsDefault()
    {
        var result = SimulationResult.Failure("error", TimeSpan.Zero);
        result.TotalTime.Should().Be(0);
    }

    [Fact]
    public void Success_Statistics_DefaultIsEmpty()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 0, 0, 0, 0, TimeSpan.Zero);
        result.Statistics.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithZeroSteps_Succeeds()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty);
        var result = SimulationResult.Success(state, 0, 0, 0, 0, TimeSpan.Zero);
        result.TotalSteps.Should().Be(0);
        result.Status.Should().Be(SimulationStatus.Completed);
    }
}
