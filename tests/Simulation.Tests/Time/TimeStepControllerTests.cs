namespace MathVerse.Simulation.Tests.Time;

using System.Collections.Immutable;
using SM = global::System.Math;

public sealed class TimeStepControllerTests
{
    private static SimulationState StateAt(double time) =>
        SimulationState.Create(time, ImmutableDictionary<string, double>.Empty);

    [Fact]
    public void FixedTimeStepController_GetTimeStep_ReturnsDefault()
    {
        var controller = new FixedTimeStepController();
        controller.GetTimeStep(StateAt(0)).Should().Be(0.01);
    }

    [Fact]
    public void FixedTimeStepController_Advance_IncreasesTime()
    {
        var controller = new FixedTimeStepController();
        var state = StateAt(0);
        controller.Advance(ref state, 0.05);
        state.CurrentTime.Should().Be(0.05);
    }

    [Fact]
    public void FixedTimeStepController_Advance_SetsLastTimeStep()
    {
        var controller = new FixedTimeStepController();
        var state = StateAt(0);
        controller.Advance(ref state, 0.025);
        state.LastTimeStep.Should().Be(0.025);
    }

    [Fact]
    public void FixedTimeStepController_EstimateError_AlwaysZero()
    {
        var controller = new FixedTimeStepController();
        controller.EstimateError(StateAt(0)).Should().Be(0.0);
    }

    [Fact]
    public void FixedTimeStepController_AdjustTimeStep_DoesNotThrow()
    {
        var controller = new FixedTimeStepController();
        Action act = () => controller.AdjustTimeStep(0.5);
        act.Should().NotThrow();
    }

    [Fact]
    public void VariableTimeStepController_GetTimeStep_ReturnsDefault()
    {
        var controller = new VariableTimeStepController();
        controller.GetTimeStep(StateAt(0)).Should().Be(0.01);
    }

    [Fact]
    public void VariableTimeStepController_Advance_IncreasesTime()
    {
        var controller = new VariableTimeStepController();
        var state = StateAt(0);
        controller.Advance(ref state, 0.05);
        state.CurrentTime.Should().Be(0.05);
    }

    [Fact]
    public void VariableTimeStepController_EstimateError_AlwaysZero()
    {
        var controller = new VariableTimeStepController();
        controller.EstimateError(StateAt(0)).Should().Be(0.0);
    }

    [Fact]
    public void VariableTimeStepController_AdjustTimeStep_ReducesStepOnPositiveError()
    {
        var controller = new VariableTimeStepController();
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(1.0);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().BeLessThan(ts1);
    }

    [Fact]
    public void VariableTimeStepController_AdjustTimeStep_MultipleReductions()
    {
        var controller = new VariableTimeStepController();
        controller.AdjustTimeStep(1.0);
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(1.0);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().BeLessThanOrEqualTo(ts1);
    }

    [Fact]
    public void VariableTimeStepController_AdjustTimeStep_ZeroError_DoesNotChange()
    {
        var controller = new VariableTimeStepController();
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(0);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().Be(ts1);
    }

    [Fact]
    public void AdaptiveTimeStepController_GetTimeStep_ReturnsDefault()
    {
        var controller = new AdaptiveTimeStepController();
        controller.GetTimeStep(StateAt(0)).Should().Be(0.01);
    }

    [Fact]
    public void AdaptiveTimeStepController_EstimateError_ReturnsNonZero()
    {
        var controller = new AdaptiveTimeStepController();
        controller.EstimateError(StateAt(0)).Should().Be(1e-8);
    }

    [Fact]
    public void AdaptiveTimeStepController_Advance_IncreasesTime()
    {
        var controller = new AdaptiveTimeStepController();
        var state = StateAt(0);
        controller.Advance(ref state, 0.01);
        state.CurrentTime.Should().Be(0.01);
    }

    [Fact]
    public void AdaptiveTimeStepController_AdjustTimeStep_LargeError_ReducesStep()
    {
        var controller = new AdaptiveTimeStepController();
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(100.0);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().BeLessThanOrEqualTo(ts1);
    }

    [Fact]
    public void AdaptiveTimeStepController_AdjustTimeStep_SmallError_MayIncreaseStep()
    {
        var controller = new AdaptiveTimeStepController();
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(1e-12);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().BeGreaterThanOrEqualTo(ts1);
    }

    [Fact]
    public void AdaptiveTimeStepController_AdjustTimeStep_ZeroError_DoesNotChange()
    {
        var controller = new AdaptiveTimeStepController();
        var ts1 = controller.GetTimeStep(StateAt(0));
        controller.AdjustTimeStep(0);
        var ts2 = controller.GetTimeStep(StateAt(0));
        ts2.Should().Be(ts1);
    }

    [Fact]
    public void EventDrivenTimeController_GetTimeStep_ReturnsDefault()
    {
        var controller = new EventDrivenTimeController();
        controller.GetTimeStep(StateAt(0)).Should().Be(0.01);
    }

    [Fact]
    public void EventDrivenTimeController_Advance_IncreasesTime()
    {
        var controller = new EventDrivenTimeController();
        var state = StateAt(0);
        controller.Advance(ref state, 0.1);
        state.CurrentTime.Should().Be(0.1);
    }

    [Fact]
    public void EventDrivenTimeController_EstimateError_AlwaysZero()
    {
        var controller = new EventDrivenTimeController();
        controller.EstimateError(StateAt(0)).Should().Be(0.0);
    }

    [Fact]
    public void EventDrivenTimeController_NextEventTime_ReturnsInfinity()
    {
        var controller = new EventDrivenTimeController();
        controller.NextEventTime(StateAt(0)).Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void EventDrivenTimeController_AdjustTimeStep_DoesNotThrow()
    {
        var controller = new EventDrivenTimeController();
        Action act = () => controller.AdjustTimeStep(1.0);
        act.Should().NotThrow();
    }

    [Fact]
    public void FixedTimeStepController_MultipleAdvances_Cumulative()
    {
        var controller = new FixedTimeStepController();
        var state = StateAt(0);
        for (int i = 0; i < 10; i++)
            controller.Advance(ref state, 0.01);
        state.CurrentTime.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void FixedTimeStepController_AdvanceFromNonZero()
    {
        var controller = new FixedTimeStepController();
        var state = StateAt(5.0);
        controller.Advance(ref state, 0.5);
        state.CurrentTime.Should().Be(5.5);
    }
}
