namespace MathVerse.Simulation.Tests.FluidDynamics;

using MathVerse.Math.Numerics.LinearAlgebra;

public class FluidDynamicsEngineTests
{
    [Theory]
    [InlineData(1000, 1.0, 0.1, 0.001, 100000)]
    [InlineData(1.225, 10.0, 0.1, 1.8e-5, 68055.56)]
    [InlineData(997, 0.5, 0.05, 0.001, 24925)]
    public void ReynoldsNumber_CorrectFormula(double density, double velocity, double length, double viscosity, double expected)
    {
        double re = FluidDynamicsEngine.ReynoldsNumber(density, velocity, length, viscosity);

        re.Should().BeApproximately(expected, 1.0);
    }

    [Theory]
    [InlineData(500, FlowRegime.Laminar)]
    [InlineData(2300, FlowRegime.Transitional)]
    [InlineData(3500, FlowRegime.Transitional)]
    [InlineData(5000, FlowRegime.Turbulent)]
    [InlineData(100000, FlowRegime.Supersonic)]
    public void DetermineRegime_CorrectClassification(double reynolds, FlowRegime expected)
    {
        FluidDynamicsEngine.DetermineRegime(reynolds).Should().Be(expected);
    }

    [Fact]
    public void PressureDrop_DarcyWeisbach_CorrectValue()
    {
        double f = 0.02;
        double L = 100.0;
        double D = 0.1;
        double rho = 1000.0;
        double v = 2.0;

        double dp = FluidDynamicsEngine.PressureDrop(f, L, D, rho, v);

        double expected = f * (L / D) * 0.5 * rho * v * v;
        dp.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void FrictionFactorLaminar_Poiseuille()
    {
        double re = 1000;

        double f = FluidDynamicsEngine.FrictionFactorLaminar(re);

        f.Should().BeApproximately(64.0 / re, 1e-10);
    }

    [Fact]
    public void FrictionFactorLaminar_HigherReLowerFriction()
    {
        double f1 = FluidDynamicsEngine.FrictionFactorLaminar(500);
        double f2 = FluidDynamicsEngine.FrictionFactorLaminar(2000);

        f1.Should().BeGreaterThan(f2);
    }

    [Fact]
    public void FrictionFactorTurbulent_ProducesPositiveValue()
    {
        double re = 10000;
        double roughness = 0.001;
        double diameter = 0.1;

        double f = FluidDynamicsEngine.FrictionFactorTurbulent(re, roughness, diameter);

        f.Should().BePositive();
    }

    [Fact]
    public void MachNumber_CorrectRatio()
    {
        double v = 340.0;
        double c = 340.0;

        double mach = FluidDynamicsEngine.MachNumber(v, c);

        mach.Should().BeApproximately(1.0, 1e-10);
    }

    [Theory]
    [InlineData(0.5, 2.0)]
    [InlineData(1.0, 2.0)]
    [InlineData(2.0, 2.0)]
    public void MachNumber_LinearScaling(double speedOfSound, double expected)
    {
        double mach = FluidDynamicsEngine.MachNumber(2.0 * speedOfSound, speedOfSound);

        mach.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void PrandtlNumber_Water_CorrectValue()
    {
        var water = FluidProperties.Water();

        double pr = FluidDynamicsEngine.PrandtlNumber(water.Viscosity, water.SpecificHeat, water.ThermalConductivity);

        double expected = water.Viscosity * water.SpecificHeat / water.ThermalConductivity;
        pr.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void NusseltNumber_Laminar_ReturnsConstant()
    {
        double nu = FluidDynamicsEngine.NusseltNumber(1000, 7.0, true);

        nu.Should().BeApproximately(3.66, 1e-10);
    }

    [Fact]
    public void NusseltNumber_Turbulent_HigherThanLaminar()
    {
        double nuLaminar = FluidDynamicsEngine.NusseltNumber(1000, 7.0, true);
        double nuTurbulent = FluidDynamicsEngine.NusseltNumber(10000, 7.0, false);

        nuTurbulent.Should().BeGreaterThan(nuLaminar);
    }

    [Fact]
    public void BoundaryLayerThickness_IncreasesWithX()
    {
        double re = 8000;
        double delta1 = FluidDynamicsEngine.BoundaryLayerThickness(0.1, re);
        double delta2 = FluidDynamicsEngine.BoundaryLayerThickness(0.5, re);

        delta2.Should().BeGreaterThan(delta1);
    }

    [Fact]
    public void Water_DensityApproximately997()
    {
        FluidProperties.Water().Density.Should().BeApproximately(997, 1);
    }

    [Fact]
    public void Water_ViscosityApproximately0001()
    {
        FluidProperties.Water().Viscosity.Should().BeApproximately(0.001, 1e-6);
    }

    [Fact]
    public void Air_DensityApproximately1225()
    {
        FluidProperties.Air().Density.Should().BeApproximately(1.225, 0.01);
    }

    [Fact]
    public void Air_ViscosityApproximately18eMinus5()
    {
        FluidProperties.Air().Viscosity.Should().BeApproximately(1.8e-5, 1e-7);
    }

    [Fact]
    public void Air_HasZeroSurfaceTension()
    {
        FluidProperties.Air().SurfaceTension.Should().Be(0);
    }

    [Fact]
    public void VelocityProfileLaminar_ParabolicShape()
    {
        var profile = FluidDynamicsEngine.VelocityProfileLaminar(0.5, 1.0, 1.0);

        profile.Size.Should().Be(3);
        profile[1].Should().BePositive();
    }

    [Fact]
    public void VelocityProfileLaminar_MaxAtCenter()
    {
        var center = FluidDynamicsEngine.VelocityProfileLaminar(0.5, 1.0, 2.0);
        var edge = FluidDynamicsEngine.VelocityProfileLaminar(0.0, 1.0, 2.0);

        center[1].Should().BeGreaterThanOrEqualTo(edge[1]);
    }

    [Fact]
    public void PressureGradient_CorrectFormula()
    {
        double rho = 1000;
        double v = 2.0;
        double r = 0.1;

        double dp = FluidDynamicsEngine.PressureGradient(rho, v, r);

        dp.Should().BeApproximately(rho * v * v / r, 1e-10);
    }

    [Fact]
    public void FlowField_Record_CanBeConstructed()
    {
        var field = new FlowField
        {
            Velocity = new Vector(1.0, 0.0, 0.0),
            Pressure = 101325,
            Temperature = 300,
            Density = 1.225,
            MachNumber = 0.3
        };

        field.Pressure.Should().Be(101325);
        field.Temperature.Should().Be(300);
    }
}
