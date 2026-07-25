namespace MathVerse.Simulation.Tests.Core;

using System.Collections.Immutable;

public sealed class SimulationOptionsTests
{
    [Fact]
    public void Default_StartTime_IsZero()
    {
        SimulationOptions.Default.StartTime.Should().Be(0.0);
    }

    [Fact]
    public void Default_EndTime_IsTen()
    {
        SimulationOptions.Default.EndTime.Should().Be(10.0);
    }

    [Fact]
    public void Default_MaxTimeStep_IsPointOne()
    {
        SimulationOptions.Default.MaxTimeStep.Should().Be(0.1);
    }

    [Fact]
    public void Default_MinTimeStep_IsOneEMinusSix()
    {
        SimulationOptions.Default.MinTimeStep.Should().Be(1e-6);
    }

    [Fact]
    public void Default_Tolerance_IsOneEMinusSix()
    {
        SimulationOptions.Default.Tolerance.Should().Be(1e-6);
    }

    [Fact]
    public void Default_Mode_IsFixedTimeStep()
    {
        SimulationOptions.Default.Mode.Should().Be(SimulationMode.FixedTimeStep);
    }

    [Fact]
    public void Default_EnableEventDetection_IsTrue()
    {
        SimulationOptions.Default.EnableEventDetection.Should().BeTrue();
    }

    [Fact]
    public void Default_EnableStateRecording_IsFalse()
    {
        SimulationOptions.Default.EnableStateRecording.Should().BeFalse();
    }

    [Fact]
    public void Default_RecordingInterval_IsOne()
    {
        SimulationOptions.Default.RecordingInterval.Should().Be(1);
    }

    [Fact]
    public void Default_EnableParallelExecution_IsFalse()
    {
        SimulationOptions.Default.EnableParallelExecution.Should().BeFalse();
    }

    [Fact]
    public void Default_MaxSteps_IsOneMillion()
    {
        SimulationOptions.Default.MaxSteps.Should().Be(1000000);
    }

    [Fact]
    public void Default_EnableCheckpointing_IsFalse()
    {
        SimulationOptions.Default.EnableCheckpointing.Should().BeFalse();
    }

    [Fact]
    public void Default_CheckpointInterval_IsOneThousand()
    {
        SimulationOptions.Default.CheckpointInterval.Should().Be(1000);
    }

    [Fact]
    public void CustomOptions_AllProperties_AreSettable()
    {
        var options = new SimulationOptions
        {
            StartTime = 1.0,
            EndTime = 20.0,
            MaxTimeStep = 0.5,
            MinTimeStep = 1e-8,
            Tolerance = 1e-8,
            Mode = SimulationMode.AdaptiveTimeStep,
            EnableEventDetection = false,
            EnableStateRecording = true,
            RecordingInterval = 5,
            EnableParallelExecution = true,
            MaxSteps = 500000,
            EnableCheckpointing = true,
            CheckpointInterval = 500
        };

        options.StartTime.Should().Be(1.0);
        options.EndTime.Should().Be(20.0);
        options.MaxTimeStep.Should().Be(0.5);
        options.MinTimeStep.Should().Be(1e-8);
        options.Tolerance.Should().Be(1e-8);
        options.Mode.Should().Be(SimulationMode.AdaptiveTimeStep);
        options.EnableEventDetection.Should().BeFalse();
        options.EnableStateRecording.Should().BeTrue();
        options.RecordingInterval.Should().Be(5);
        options.EnableParallelExecution.Should().BeTrue();
        options.MaxSteps.Should().Be(500000);
        options.EnableCheckpointing.Should().BeTrue();
        options.CheckpointInterval.Should().Be(500);
    }

    [Fact]
    public void Default_SameReference_ReturnsSameInstance()
    {
        var a = SimulationOptions.Default;
        var b = SimulationOptions.Default;
        a.Should().BeSameAs(b);
    }
}
