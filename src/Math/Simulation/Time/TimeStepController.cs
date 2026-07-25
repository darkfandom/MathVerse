namespace MathVerse.Math.Simulation.Time;

using MathVerse.Math.Simulation.Core;

#pragma warning disable CS0414
public interface ITimeStepController
{
    double GetTimeStep(SimulationState state);
    void Advance(ref SimulationState state, double timeStep);
    double EstimateError(SimulationState state);
    void AdjustTimeStep(double error);
}

public sealed class FixedTimeStepController : ITimeStepController
{
    private double _timeStep = 0.01;

    public double GetTimeStep(SimulationState state) => _timeStep;

    public void Advance(ref SimulationState state, double timeStep)
    {
        state = state with { CurrentTime = state.CurrentTime + timeStep, LastTimeStep = timeStep };
    }

    public double EstimateError(SimulationState state) => 0.0;

    public void AdjustTimeStep(double error) { }
}

public sealed class VariableTimeStepController : ITimeStepController
{
    private double _timeStep = 0.01;
    private double _minStep = 1e-6;
    private double _maxStep = 0.1;

    public double GetTimeStep(SimulationState state) => _timeStep;

    public void Advance(ref SimulationState state, double timeStep)
    {
        state = state with { CurrentTime = state.CurrentTime + timeStep, LastTimeStep = timeStep };
    }

    public double EstimateError(SimulationState state) => 0.0;

    public void AdjustTimeStep(double error)
    {
        if (error > 0)
        {
            _timeStep = System.Math.Max(1e-6, _timeStep * 0.5);
        }
    }
}

public sealed class AdaptiveTimeStepController : ITimeStepController
{
    private double _timeStep = 0.01;
    private double _minStep = 1e-6;
    private double _maxStep = 0.1;
    private double _targetError = 1e-6;

    public double GetTimeStep(SimulationState state) => _timeStep;

    public void Advance(ref SimulationState state, double timeStep)
    {
        state = state with { CurrentTime = state.CurrentTime + timeStep, LastTimeStep = timeStep };
    }

    public double EstimateError(SimulationState state) => 1e-8;

    public void AdjustTimeStep(double error)
    {
        if (error > 0)
        {
            double factor = System.Math.Min(2.0, System.Math.Max(0.5, 0.9 * System.Math.Pow(1e-6 / System.Math.Max(error, 1e-15), 0.2)));
            _timeStep = System.Math.Clamp(_timeStep * factor, 1e-6, 0.1);
        }
    }
}

public sealed class EventDrivenTimeController : ITimeStepController
{
    private double _timeStep = 0.01;

    public double GetTimeStep(SimulationState state) => _timeStep;

    public void Advance(ref SimulationState state, double timeStep)
    {
        state = state with { CurrentTime = state.CurrentTime + timeStep, LastTimeStep = timeStep };
    }

    public double EstimateError(SimulationState state) => 0.0;

    public void AdjustTimeStep(double error) { }

    public double NextEventTime(SimulationState state) => double.PositiveInfinity;
}
#pragma warning restore CS0414