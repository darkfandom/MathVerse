namespace MathVerse.Math.Simulation.Configuration;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Simulation.Core;
using MathVerse.Math.Simulation.Solvers;

public sealed record SimulationConfiguration
{
    public SimulationOptions DefaultOptions { get; init; } = SimulationOptions.Default;
    public PhysicsConfiguration Physics { get; init; } = PhysicsConfiguration.Default;
    public ThermodynamicsConfiguration Thermodynamics { get; init; } = ThermodynamicsConfiguration.Default;
    public ElectromagneticsConfiguration Electromagnetics { get; init; } = ElectromagneticsConfiguration.Default;
    public FluidDynamicsConfiguration FluidDynamics { get; init; } = FluidDynamicsConfiguration.Default;
    public ChemistryConfiguration Chemistry { get; init; } = ChemistryConfiguration.Default;
    public BiologyConfiguration Biology { get; init; } = BiologyConfiguration.Default;
    public FinanceConfiguration Finance { get; init; } = FinanceConfiguration.Default;
    public SignalProcessingConfiguration SignalProcessing { get; init; } = SignalProcessingConfiguration.Default;
    public ControlSystemsConfiguration ControlSystems { get; init; } = ControlSystemsConfiguration.Default;
    public MonteCarloConfiguration MonteCarlo { get; init; } = MonteCarloConfiguration.Default;
    public SolversConfiguration Solvers { get; init; } = SolversConfiguration.Default;
    public DiagnosticsConfiguration Diagnostics { get; init; } = DiagnosticsConfiguration.Default;
    public VisualizationConfiguration Visualization { get; init; } = VisualizationConfiguration.Default;

    public static SimulationConfiguration Default { get; } = new();
}

public sealed record PhysicsConfiguration
{
    public double Gravity { get; init; } = 9.80665;
    public double DefaultMass { get; init; } = 1.0;
    public double DefaultTimeStep { get; init; } = 0.01;
    public int MaxParticles { get; init; } = 10000;
    public bool EnableCollisions { get; init; } = true;
    public bool EnableConstraints { get; init; } = true;

    public static PhysicsConfiguration Default { get; } = new();
}

public sealed record ThermodynamicsConfiguration
{
    public double GasConstant { get; init; } = 8.314462618;
    public double StandardTemperature { get; init; } = 273.15;
    public double StandardPressure { get; init; } = 101325;
    public bool EnablePhaseTransitions { get; init; } = true;

    public static ThermodynamicsConfiguration Default { get; } = new();
}

public sealed record ElectromagneticsConfiguration
{
    public double VacuumPermittivity { get; init; } = 8.854187817e-12;
    public double VacuumPermeability { get; init; } = 4 * System.Math.PI * 1e-7;
    public bool EnableRadiation { get; init; } = true;

    public static ElectromagneticsConfiguration Default { get; } = new();
}

public sealed record FluidDynamicsConfiguration
{
    public double DefaultDensity { get; init; } = 1000.0;
    public double DefaultViscosity { get; init; } = 0.001;
    public bool EnableTurbulence { get; init; } = true;
    public int MaxIterations { get; init; } = 1000;

    public static FluidDynamicsConfiguration Default { get; } = new();
}

public sealed record ChemistryConfiguration
{
    public double GasConstant { get; init; } = 8.314462618;
    public bool EnableKinetics { get; init; } = true;
    public bool EnableEquilibrium { get; init; } = true;

    public static ChemistryConfiguration Default { get; } = new();
}

public sealed record BiologyConfiguration
{
    public bool EnablePopulationDynamics { get; init; } = true;
    public bool EnableEpidemiology { get; init; } = true;

    public static BiologyConfiguration Default { get; } = new();
}

public sealed record FinanceConfiguration
{
    public double RiskFreeRate { get; init; } = 0.05;
    public int MonteCarloPaths { get; init; } = 10000;

    public static FinanceConfiguration Default { get; } = new();
}

public sealed record SignalProcessingConfiguration
{
    public int MaxFFTSize { get; init; } = 65536;
    public bool EnableWindowing { get; init; } = true;

    public static SignalProcessingConfiguration Default { get; } = new();
}

public sealed record ControlSystemsConfiguration
{
    public double DefaultSampleTime { get; init; } = 0.01;
    public int MaxOrder { get; init; } = 20;

    public static ControlSystemsConfiguration Default { get; } = new();
}

public sealed record MonteCarloConfiguration
{
    public int DefaultSamples { get; init; } = 10000;
    public int DefaultIterations { get; init; } = 1000;
    public double ConvergenceTolerance { get; init; } = 1e-6;

    public static MonteCarloConfiguration Default { get; } = new();
}

public sealed record SolversConfiguration
{
    public SolverType DefaultMethod { get; init; } = SolverType.RungeKutta4;
    public double DefaultTolerance { get; init; } = 1e-6;
    public int MaxSteps { get; init; } = 1000000;

    public static SolversConfiguration Default { get; } = new();
}

public sealed record DiagnosticsConfiguration
{
    public bool EnableStabilityMonitoring { get; init; } = true;
    public bool EnableEnergyDriftMonitoring { get; init; } = true;
    public bool EnableConstraintMonitoring { get; init; } = true;
    public bool EnableDivergenceDetection { get; init; } = true;
    public double EnergyDriftTolerance { get; init; } = 1e-6;
    public bool EnableConvergenceTracking { get; init; } = true;

    public static DiagnosticsConfiguration Default { get; } = new();
}

public sealed record VisualizationConfiguration
{
    public int MaxFrames { get; init; } = 10000;
    public int MaxDataPoints { get; init; } = 100000;
    public double DefaultColorScaleMin { get; init; } = -1;
    public double DefaultColorScaleMax { get; init; } = 1;

    public static VisualizationConfiguration Default { get; } = new();
}
