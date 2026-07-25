namespace MathVerse.Math.Simulation.Core;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Simulation.Time;

public sealed class SimulationContext
{
    public SimulationOptions Options { get; }
    public SimulationState State { get; private set; }
    public SimulationEngine Engine { get; }
    public ITimeStepController TimeController { get; }
    private SimulationState _state;

    public SimulationContext(SimulationOptions? options = null)
    {
        Options = options ?? SimulationOptions.Default;
        Engine = new SimulationEngine(Options);
        TimeController = new FixedTimeStepController();
        _state = SimulationState.Create(Options.StartTime, ImmutableDictionary<string, double>.Empty);
        State = _state;
    }

    public void Initialize(ImmutableDictionary<string, double> initialVariables)
    {
        _state = SimulationState.Create(Options.StartTime, initialVariables);
        State = _state;
    }

    public void SetVariable(string name, double value) => Engine.SetVariable(name, value);
    public double GetVariable(string name) => Engine.GetVariable(name);

    public SimulationResult Run(Func<SimulationState, double, SimulationState> stepFunction)
        => Engine.Run(stepFunction);

    public void Advance(double timeStep)
    {
        TimeController.Advance(ref _state, timeStep);
        State = _state;
    }
}