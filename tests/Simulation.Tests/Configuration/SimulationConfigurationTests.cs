namespace MathVerse.Simulation.Tests.Configuration;

public sealed class SimulationConfigurationTests
{
    [Fact]
    public void Default_HasDefaultOptions()
    {
        SimulationConfiguration.Default.DefaultOptions.Should().Be(SimulationOptions.Default);
    }

    [Fact]
    public void Default_HasDefaultPhysics()
    {
        SimulationConfiguration.Default.Physics.Should().Be(PhysicsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultThermodynamics()
    {
        SimulationConfiguration.Default.Thermodynamics.Should().Be(ThermodynamicsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultElectromagnetics()
    {
        SimulationConfiguration.Default.Electromagnetics.Should().Be(ElectromagneticsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultFluidDynamics()
    {
        SimulationConfiguration.Default.FluidDynamics.Should().Be(FluidDynamicsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultChemistry()
    {
        SimulationConfiguration.Default.Chemistry.Should().Be(ChemistryConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultBiology()
    {
        SimulationConfiguration.Default.Biology.Should().Be(BiologyConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultFinance()
    {
        SimulationConfiguration.Default.Finance.Should().Be(FinanceConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultSignalProcessing()
    {
        SimulationConfiguration.Default.SignalProcessing.Should().Be(SignalProcessingConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultControlSystems()
    {
        SimulationConfiguration.Default.ControlSystems.Should().Be(ControlSystemsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultMonteCarlo()
    {
        SimulationConfiguration.Default.MonteCarlo.Should().Be(MonteCarloConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultSolvers()
    {
        SimulationConfiguration.Default.Solvers.Should().Be(SolversConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultDiagnostics()
    {
        SimulationConfiguration.Default.Diagnostics.Should().Be(DiagnosticsConfiguration.Default);
    }

    [Fact]
    public void Default_HasDefaultVisualization()
    {
        SimulationConfiguration.Default.Visualization.Should().Be(VisualizationConfiguration.Default);
    }

    [Fact]
    public void PhysicsConfiguration_DefaultGravity()
    {
        PhysicsConfiguration.Default.Gravity.Should().BeApproximately(9.80665, 1e-5);
    }

    [Fact]
    public void PhysicsConfiguration_DefaultMass()
    {
        PhysicsConfiguration.Default.DefaultMass.Should().Be(1.0);
    }

    [Fact]
    public void PhysicsConfiguration_DefaultTimeStep()
    {
        PhysicsConfiguration.Default.DefaultTimeStep.Should().Be(0.01);
    }

    [Fact]
    public void PhysicsConfiguration_DefaultMaxParticles()
    {
        PhysicsConfiguration.Default.MaxParticles.Should().Be(10000);
    }

    [Fact]
    public void PhysicsConfiguration_EnableCollisions_DefaultTrue()
    {
        PhysicsConfiguration.Default.EnableCollisions.Should().BeTrue();
    }

    [Fact]
    public void PhysicsConfiguration_EnableConstraints_DefaultTrue()
    {
        PhysicsConfiguration.Default.EnableConstraints.Should().BeTrue();
    }

    [Fact]
    public void ThermodynamicsConfiguration_DefaultGasConstant()
    {
        ThermodynamicsConfiguration.Default.GasConstant.Should().BeApproximately(8.314462618, 1e-9);
    }

    [Fact]
    public void ThermodynamicsConfiguration_DefaultStandardTemperature()
    {
        ThermodynamicsConfiguration.Default.StandardTemperature.Should().BeApproximately(273.15, 1e-10);
    }

    [Fact]
    public void ThermodynamicsConfiguration_DefaultStandardPressure()
    {
        ThermodynamicsConfiguration.Default.StandardPressure.Should().Be(101325);
    }

    [Fact]
    public void ThermodynamicsConfiguration_EnablePhaseTransitions_DefaultTrue()
    {
        ThermodynamicsConfiguration.Default.EnablePhaseTransitions.Should().BeTrue();
    }

    [Fact]
    public void ElectromagneticsConfiguration_DefaultVacuumPermittivity()
    {
        ElectromagneticsConfiguration.Default.VacuumPermittivity.Should().BeApproximately(8.854187817e-12, 1e-20);
    }

    [Fact]
    public void ElectromagneticsConfiguration_DefaultVacuumPermeability()
    {
        ElectromagneticsConfiguration.Default.VacuumPermeability.Should().BeApproximately(4 * System.Math.PI * 1e-7, 1e-15);
    }

    [Fact]
    public void ElectromagneticsConfiguration_EnableRadiation_DefaultTrue()
    {
        ElectromagneticsConfiguration.Default.EnableRadiation.Should().BeTrue();
    }

    [Fact]
    public void FluidDynamicsConfiguration_DefaultDensity()
    {
        FluidDynamicsConfiguration.Default.DefaultDensity.Should().Be(1000.0);
    }

    [Fact]
    public void FluidDynamicsConfiguration_DefaultViscosity()
    {
        FluidDynamicsConfiguration.Default.DefaultViscosity.Should().Be(0.001);
    }

    [Fact]
    public void FluidDynamicsConfiguration_EnableTurbulence_DefaultTrue()
    {
        FluidDynamicsConfiguration.Default.EnableTurbulence.Should().BeTrue();
    }

    [Fact]
    public void FluidDynamicsConfiguration_MaxIterations()
    {
        FluidDynamicsConfiguration.Default.MaxIterations.Should().Be(1000);
    }

    [Fact]
    public void ChemistryConfiguration_DefaultGasConstant()
    {
        ChemistryConfiguration.Default.GasConstant.Should().BeApproximately(8.314462618, 1e-9);
    }

    [Fact]
    public void ChemistryConfiguration_EnableKinetics()
    {
        ChemistryConfiguration.Default.EnableKinetics.Should().BeTrue();
    }

    [Fact]
    public void ChemistryConfiguration_EnableEquilibrium()
    {
        ChemistryConfiguration.Default.EnableEquilibrium.Should().BeTrue();
    }

    [Fact]
    public void BiologyConfiguration_EnablePopulationDynamics()
    {
        BiologyConfiguration.Default.EnablePopulationDynamics.Should().BeTrue();
    }

    [Fact]
    public void BiologyConfiguration_EnableEpidemiology()
    {
        BiologyConfiguration.Default.EnableEpidemiology.Should().BeTrue();
    }

    [Fact]
    public void FinanceConfiguration_DefaultRiskFreeRate()
    {
        FinanceConfiguration.Default.RiskFreeRate.Should().Be(0.05);
    }

    [Fact]
    public void FinanceConfiguration_DefaultMonteCarloPaths()
    {
        FinanceConfiguration.Default.MonteCarloPaths.Should().Be(10000);
    }

    [Fact]
    public void SignalProcessingConfiguration_DefaultMaxFFTSize()
    {
        SignalProcessingConfiguration.Default.MaxFFTSize.Should().Be(65536);
    }

    [Fact]
    public void SignalProcessingConfiguration_EnableWindowing()
    {
        SignalProcessingConfiguration.Default.EnableWindowing.Should().BeTrue();
    }

    [Fact]
    public void ControlSystemsConfiguration_DefaultSampleTime()
    {
        ControlSystemsConfiguration.Default.DefaultSampleTime.Should().Be(0.01);
    }

    [Fact]
    public void ControlSystemsConfiguration_MaxOrder()
    {
        ControlSystemsConfiguration.Default.MaxOrder.Should().Be(20);
    }

    [Fact]
    public void MonteCarloConfiguration_DefaultSamples()
    {
        MonteCarloConfiguration.Default.DefaultSamples.Should().Be(10000);
    }

    [Fact]
    public void MonteCarloConfiguration_DefaultIterations()
    {
        MonteCarloConfiguration.Default.DefaultIterations.Should().Be(1000);
    }

    [Fact]
    public void MonteCarloConfiguration_ConvergenceTolerance()
    {
        MonteCarloConfiguration.Default.ConvergenceTolerance.Should().Be(1e-6);
    }

    [Fact]
    public void DiagnosticsConfiguration_AllDefaultsTrue()
    {
        var d = DiagnosticsConfiguration.Default;
        d.EnableStabilityMonitoring.Should().BeTrue();
        d.EnableEnergyDriftMonitoring.Should().BeTrue();
        d.EnableConstraintMonitoring.Should().BeTrue();
        d.EnableDivergenceDetection.Should().BeTrue();
        d.EnableConvergenceTracking.Should().BeTrue();
    }

    [Fact]
    public void DiagnosticsConfiguration_EnergyDriftTolerance()
    {
        DiagnosticsConfiguration.Default.EnergyDriftTolerance.Should().Be(1e-6);
    }

    [Fact]
    public void VisualizationConfiguration_MaxFrames()
    {
        VisualizationConfiguration.Default.MaxFrames.Should().Be(10000);
    }

    [Fact]
    public void VisualizationConfiguration_MaxDataPoints()
    {
        VisualizationConfiguration.Default.MaxDataPoints.Should().Be(100000);
    }

    [Fact]
    public void VisualizationConfiguration_DefaultColorScaleRange()
    {
        var v = VisualizationConfiguration.Default;
        v.DefaultColorScaleMin.Should().Be(-1);
        v.DefaultColorScaleMax.Should().Be(1);
    }
}
