namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;

public sealed class SimulationStatusTests
{
    [Fact]
    public void NotStarted_HasValueZero()
    {
        ((int)SimulationStatus.NotStarted).Should().Be(0);
    }

    [Fact]
    public void Running_HasValueOne()
    {
        ((int)SimulationStatus.Running).Should().Be(1);
    }

    [Fact]
    public void Paused_HasValueTwo()
    {
        ((int)SimulationStatus.Paused).Should().Be(2);
    }

    [Fact]
    public void Completed_HasValueThree()
    {
        ((int)SimulationStatus.Completed).Should().Be(3);
    }

    [Fact]
    public void Failed_HasValueFour()
    {
        ((int)SimulationStatus.Failed).Should().Be(4);
    }

    [Fact]
    public void Cancelled_HasValueFive()
    {
        ((int)SimulationStatus.Cancelled).Should().Be(5);
    }

    [Fact]
    public void AllValues_AreDistinct()
    {
        var values = Enum.GetValues<SimulationStatus>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllValues_ContainSixMembers()
    {
        Enum.GetValues<SimulationStatus>().Should().HaveCount(6);
    }

    [Fact]
    public void IsComplete_TrueForCompleted()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Completed
        };
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_FalseForPaused()
    {
        var state = SimulationState.Create(0, ImmutableDictionary<string, double>.Empty) with
        {
            Status = SimulationStatus.Paused
        };
        state.IsComplete.Should().BeFalse();
    }
}
