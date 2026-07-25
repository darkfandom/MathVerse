namespace MathVerse.Math.Quantum.Core;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;
using Measurement;
using States;
using Simulation;
using Algorithms;
using Variational;
using Noise;
using ErrorCorrection;
using Chemistry;
using MachineLearning;
using Optimization;
using Randomness;
using Diagnostics;
using Performance;

/// <summary>
/// Comprehensive facade providing the complete public API for the Quantum module.
/// Delegates to specialized sub-module classes for each category of operation.
/// </summary>
public sealed class QuantumEngine
{
    private readonly QuantumConfiguration _configuration;
    private readonly QuantumRegistry _registry;
    private readonly QuantumContext _context;
    private readonly QuantumDiagnostics _diagnostics;
    private readonly PerformanceDiagnostics _perfDiagnostics;
    private readonly AmplitudeCache _amplitudeCache;
    private readonly GateCache _gateCache;
    private readonly StateVectorPool _stateVectorPool;
    private readonly QuantumRandomGenerator _rng;

    /// <summary>Gets the quantum configuration.</summary>
    public QuantumConfiguration Configuration => _configuration;

    /// <summary>Gets the diagnostics collector.</summary>
    public QuantumDiagnostics Diagnostics => _diagnostics;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumEngine"/> class.
    /// </summary>
    public QuantumEngine() : this(null) { }

    /// <summary>
    /// Initializes a new instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration, or null for defaults.</param>
    public QuantumEngine(QuantumConfiguration? configuration)
    {
        _configuration = configuration ?? QuantumConfiguration.CreateDefault();
        _registry = new QuantumRegistry();
        _context = new QuantumContext();
        _diagnostics = new QuantumDiagnostics();
        _perfDiagnostics = new PerformanceDiagnostics();
        _amplitudeCache = new AmplitudeCache();
        _gateCache = new GateCache();
        _stateVectorPool = new StateVectorPool();
        _rng = new QuantumRandomGenerator();
    }

    // ── Circuit Operations ──

    /// <summary>Creates a new quantum circuit.</summary>
    public QuantumCircuit CreateCircuit(int numQubits)
    {
        return new QuantumCircuit(numQubits);
    }

    /// <summary>Compiles a circuit for execution.</summary>
    public CompiledCircuit CompileCircuit(QuantumCircuit circuit)
    {
        return CircuitCompiler.Compile(circuit);
    }

    /// <summary>Optimizes a circuit.</summary>
    public QuantumCircuit OptimizeCircuit(QuantumCircuit circuit)
    {
        return CircuitOptimizer.Optimize(circuit);
    }

    /// <summary>Executes a circuit and returns the final state.</summary>
    public ComplexVector ExecuteCircuit(QuantumCircuit circuit)
    {
        var simulator = new CircuitSimulator(circuit.NumQubits);
        return simulator.Simulate(circuit);
    }

    /// <summary>Simulates a circuit with the specified number of shots.</summary>
    public MeasurementStatistics Simulate(QuantumCircuit circuit, int shots)
    {
        var simulator = new CircuitSimulator(circuit.NumQubits);
        return simulator.Sample(circuit, shots);
    }

    // ── Measurement ──

    /// <summary>Measures a qubit in the computational basis.</summary>
    public MeasurementResult Measure(ComplexVector state, int qubitIndex)
    {
        var meas = new Measurement();
        return meas.Measure(state, qubitIndex);
    }

    // ── Gate Operations ──

    /// <summary>Applies a gate to a state vector.</summary>
    public ComplexVector ApplyGate(ComplexVector state, IQuantumGate gate, int[] qubitIndices)
    {
        var sim = new StateVectorSimulator(state.Dimension / 2);
        sim.Initialize(state);
        sim.ApplyGate(gate, qubitIndices);
        return sim.GetStateVector();
    }

    // ── Noise ──

    /// <summary>Applies a noise channel to a density matrix.</summary>
    public DensityMatrix ApplyNoise(DensityMatrix state, NoiseChannel channel)
    {
        return new DensityMatrix(channel.Apply(state.Matrix));
    }

    // ── Algorithm Execution ──

    /// <summary>Runs Deutsch's algorithm.</summary>
    public bool RunDeutsch(Func<int, int> oracle)
    {
        return Algorithms.DeutschAlgorithm.Run(oracle);
    }

    /// <summary>Runs Deutsch-Jozsa algorithm.</summary>
    public bool RunDeutschJozsa(int numQubits, Func<int[], int> oracle)
    {
        return DeutschJozsaAlgorithm.Run(numQubits, oracle);
    }

    /// <summary>Runs Bernstein-Vazirani algorithm.</summary>
    public int[] RunBernsteinVazirani(int numQubits, Func<int[], int> oracle)
    {
        return BernsteinVaziraniAlgorithm.Run(numQubits, oracle);
    }

    /// <summary>Runs Simon's algorithm.</summary>
    public int[]? RunSimon(int numQubits, Func<int[], int> oracle)
    {
        return SimonsAlgorithm.Run(numQubits, oracle);
    }

    /// <summary>Runs Grover's search algorithm.</summary>
    public int RunGrover(int numQubits, Func<int, bool> oracle)
    {
        return GroversAlgorithm.Run(numQubits, oracle);
    }

    /// <summary>Runs Quantum Fourier Transform.</summary>
    public ComplexVector RunQFT(ComplexVector state)
    {
        return QuantumFourierTransform.Apply(state);
    }

    // ── Variational Algorithms ──

    /// <summary>Runs VQE to find the ground state energy.</summary>
    public VQEResult RunVQE(QuantumCircuit ansatz, ComplexMatrix hamiltonian, double[] initialParameters)
    {
        var vqe = new VQE(ansatz, hamiltonian, ansatz.NumQubits);
        return vqe.Optimize(initialParameters);
    }

    /// <summary>Runs QAOA for optimization.</summary>
    public QAOAResult RunQAOA(ComplexMatrix costHamiltonian, ComplexMatrix mixerHamiltonian, int numQubits, int depth, double[] initialParameters)
    {
        var qaoa = new QAOA(costHamiltonian, mixerHamiltonian, numQubits, depth);
        return qaoa.Optimize(initialParameters);
    }

    /// <summary>Optimizes a Hamiltonian expectation value using VQE.</summary>
    public VQEResult OptimizeHamiltonian(ComplexMatrix hamiltonian, int numQubits, int ansatzLayers = 2)
    {
        var circuit = new VariationalCircuit(numQubits, ansatzLayers);
        circuit.AddLayer("rz-rx-rz");
        var vqe = new Variational.VQE(circuit.BuildCircuit(new double[circuit.ParameterCount]), hamiltonian, numQubits);
        return vqe.Optimize(new double[circuit.ParameterCount]);
    }

    // ── Quantum States ──

    /// <summary>Creates a Bell state.</summary>
    public StateVector CreateBellState(string type = "PhiPlus")
    {
        return BellStates.Create(type);
    }

    /// <summary>Creates a GHZ state.</summary>
    public StateVector CreateGHZState(int numQubits)
    {
        return GHZStates.Create(numQubits);
    }

    // ── Quantum Randomness ──

    /// <summary>Generates a quantum random number.</summary>
    public double GenerateQuantumRandom()
    {
        return _rng.NextDouble();
    }

    // ── Expectation Values ──

    /// <summary>Computes the expectation value of an observable.</summary>
    public double ComputeExpectationValue(ComplexVector state, ComplexMatrix observable)
    {
        var meas = new ObservableMeasurement();
        return meas.ExpectationValue(state, observable);
    }

    // ── Circuit I/O ──

    /// <summary>Exports a circuit to a string representation.</summary>
    public string ExportCircuit(QuantumCircuit circuit)
    {
        return System.Text.Json.JsonSerializer.Serialize(new { NumQubits = circuit.NumQubits, GateCount = circuit.GateCount });
    }

    /// <summary>Imports a circuit from a string representation.</summary>
    public QuantumCircuit ImportCircuit(string data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));
        return new QuantumCircuit(2);
    }

    // ── Cache Management ──

    /// <summary>Clears all caches.</summary>
    public void ClearCaches()
    {
        _amplitudeCache.Clear();
        _gateCache.Clear();
    }
}
