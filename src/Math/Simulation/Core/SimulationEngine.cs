namespace MathVerse.Math.Simulation.Core;

using System.Diagnostics;
using MathVerse.Math.Foundation;
using MathVerse.Math.Simulation.Time;

public sealed class SimulationEngine
{
    private readonly SimulationOptions _options;
    private readonly ITimeStepController _timeController;
    private SimulationState _state;
    private readonly Stopwatch _stopwatch = new();

    public SimulationEngine(SimulationOptions? options = null)
    {
        _options = options ?? SimulationOptions.Default;
        _timeController = CreateTimeController(_options.Mode);
        _state = SimulationState.Create(_options.StartTime, ImmutableDictionary<string, double>.Empty);
    }

    private static ITimeStepController CreateTimeController(SimulationMode mode) => mode switch
    {
        SimulationMode.FixedTimeStep => new FixedTimeStepController(),
        SimulationMode.VariableTimeStep => new VariableTimeStepController(),
        SimulationMode.AdaptiveTimeStep => new AdaptiveTimeStepController(),
        SimulationMode.EventDriven => new EventDrivenTimeController(),
        _ => new FixedTimeStepController()
    };

    public void SetInitialState(ImmutableDictionary<string, double> variables)
    {
        _state = _state with { Variables = variables, Status = SimulationStatus.NotStarted };
    }

    public void SetVariable(string name, double value)
    {
        _state = _state with { Variables = _state.Variables.SetItem(name, value) };
    }

    public double GetVariable(string name) => _state.Variables.TryGetValue(name, out var v) ? v : 0.0;

    public SimulationResult Run(Func<SimulationState, double, SimulationState> stepFunction)
    {
        _stopwatch.Restart();
        _state = _state with { Status = SimulationStatus.Running };
        int steps = 0;
        int eventCount = 0;
        int functionEvals = 0;

        try
        {
            while (_state.CurrentTime < _options.EndTime && steps < _options.MaxSteps)
            {
                var currentStepResult = stepFunction(_state, _timeController.GetTimeStep(_state));
                _state = currentStepResult;
                steps++;
                functionEvals++;

                if (_options.EnableStateRecording && steps % _options.RecordingInterval == 0)
                {
                    RecordState();
                }

                if (_options.EnableEventDetection)
                {
                    eventCount += DetectEvents();
                }

                _state = _state with { StepCount = steps };
            }

            _state = _state with { Status = SimulationStatus.Completed };
            return SimulationResult.Success(_state, steps, _state.CurrentTime, eventCount, functionEvals, _stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _state = _state with { Status = SimulationStatus.Failed, ErrorMessage = ex.Message };
            return SimulationResult.Failure(ex.Message, _stopwatch.Elapsed);
        }
        finally
        {
            _stopwatch.Stop();
        }
    }

    private void RecordState()
    {
        var history = _state.History.ToBuilder();
        foreach (var kvp in _state.Variables)
        {
            if (!history.TryGetValue(kvp.Key, out var arr))
            {
                history[kvp.Key] = ImmutableArray.Create(kvp.Value);
            }
            else
            {
                history[kvp.Key] = arr.Add(kvp.Value);
            }
        }
        _state = _state with { History = history.ToImmutable() };
    }

    private int DetectEvents() => 0;
}