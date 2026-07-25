namespace MathVerse.Simulation.Tests.Thermodynamics;

using System.Collections.Immutable;
using ThermodynamicHeatTransfer = MathVerse.Math.Simulation.Thermodynamics.HeatTransfer;

public sealed class ThermodynamicsEngineTests
{
    [Fact]
    public void IdealGasLaw_OneMoleAtSTP()
    {
        double result = ThermodynamicsEngine.IdealGasLaw(101325, 0.022414, 1.0, 273.15);
        result.Should().BeApproximately(1.0, 0.01);
    }

    [Theory]
    [InlineData(101325, 0.022414, 1.0, 273.15)]
    [InlineData(100000, 0.02494, 1.0, 298.15)]
    public void IdealGasLaw_ResultCloseToOne(double p, double v, double n, double t)
    {
        double result = ThermodynamicsEngine.IdealGasLaw(p, v, n, t);
        result.Should().BeApproximately(1.0, 0.05);
    }

    [Fact]
    public void SpecificHeatRatio_DiatomicGas()
    {
        double gamma = ThermodynamicsEngine.SpecificHeatRatio(718, 1005);
        gamma.Should().BeApproximately(1.4, 0.01);
    }

    [Fact]
    public void SpecificHeatRatio_MonatomicGas()
    {
        double gamma = ThermodynamicsEngine.SpecificHeatRatio(3.0 / 2.0 * 8.314, 5.0 / 2.0 * 8.314);
        gamma.Should().BeApproximately(5.0 / 3.0, 1e-6);
    }

    [Fact]
    public void CarnotEfficiency_ExampleCalculation()
    {
        double eta = ThermodynamicsEngine.CarnotEfficiency(600, 300);
        eta.Should().BeApproximately(0.5, 1e-10);
    }

    [Theory]
    [InlineData(1000, 300, 0.7)]
    [InlineData(500, 250, 0.5)]
    public void CarnotEfficiency_VariousTemps(double th, double tc, double expected)
    {
        double eta = ThermodynamicsEngine.CarnotEfficiency(th, tc);
        eta.Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void HeatCapacity_MassTimesSpecific()
    {
        double c = ThermodynamicsEngine.HeatCapacity(2.0, 4186);
        c.Should().Be(2.0 * 4186);
    }

    [Fact]
    public void EntropyChange_ExampleCalculation()
    {
        double ds = ThermodynamicsEngine.EntropyChange(1000, 300);
        ds.Should().BeApproximately(1000.0 / 300.0, 1e-10);
    }

    [Fact]
    public void GibbsFreeEnergy_ExampleCalculation()
    {
        double g = ThermodynamicsEngine.GibbsFreeEnergy(1000, 300, 2);
        g.Should().BeApproximately(400, 1e-10);
    }

    [Fact]
    public void ChemicalPotential_ExampleCalculation()
    {
        double mu0 = 5000;
        double T = 298.15;
        double p = 101325;
        double p0 = 101325;
        double mu = ThermodynamicsEngine.ChemicalPotential(mu0, T, p, p0);
        mu.Should().BeApproximately(mu0, 1e-6);
    }

    [Fact]
    public void ChemicalPressure_EqualPressure_ReturnsMu0()
    {
        double mu = ThermodynamicsEngine.ChemicalPotential(100, 300, 100000, 100000);
        mu.Should().BeApproximately(100, 1e-6);
    }

    [Fact]
    public void PhaseTransitionTemperature_ExampleCalculation()
    {
        double T = ThermodynamicsEngine.PhaseTransitionTemperature(334000, 1220);
        T.Should().BeApproximately(334000.0 / 1220.0, 1e-6);
    }

    [Fact]
    public void ClausiusClapeyron_ExampleCalculation()
    {
        double dpdT = ThermodynamicsEngine.ClausiusClapeyron(2260000, 0.001, 373.15);
        dpdT.Should().BeApproximately(2260000.0 / (373.15 * 0.001), 1e-3);
    }

    [Fact]
    public void AdiabaticIndex_ReturnsInput()
    {
        double gamma = ThermodynamicsEngine.AdiabaticIndex(1.4);
        gamma.Should().Be(1.4);
    }

    [Fact]
    public void IsentropicRelation_ExampleCalculation()
    {
        double gamma = 1.4;
        double result = ThermodynamicsEngine.IsentropicRelation(100000, 200000, gamma);
        double expected = System.Math.Pow(200000.0 / 100000.0, (gamma - 1) / gamma);
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void GasConstant_MatchesKnownValue()
    {
        ThermodynamicsEngine.GasConstant.Should().BeApproximately(8.314462618, 1e-9);
    }

    [Fact]
    public void BoltzmannConstant_MatchesKnownValue()
    {
        ThermodynamicsEngine.BoltzmannConstant.Should().BeApproximately(1.380649e-23, 1e-30);
    }

    [Fact]
    public void AvogadroNumber_MatchesKnownValue()
    {
        ThermodynamicsEngine.AvogadroNumber.Should().BeApproximately(6.02214076e23, 1e14);
    }

    [Fact]
    public void UpdateState_HeatAdded_IncreasesTemperature()
    {
        var state = new ThermodynamicState
        {
            Temperature = 300,
            Pressure = 101325,
            Volume = 0.01,
            InternalEnergy = 1000,
            Mass = 1.0,
            SpecificHeatCapacity = 4186
        };

        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Conduction,
            Coefficient = 100,
            TemperatureDifference = 10,
            Area = 1.0
        };

        var transfers = ImmutableArray.Create(ht);
        var newState = ThermodynamicsEngine.UpdateState(state, 1.0, transfers);
        newState.Temperature.Should().BeGreaterThan(300);
        newState.InternalEnergy.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void HeatTransfer_Conduction_FluxTimesArea()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Conduction,
            Coefficient = 50,
            TemperatureDifference = 20,
            Area = 2.0
        };

        double q = ThermodynamicsEngine.HeatTransfer(ht);
        q.Should().BeApproximately(50 * 20 * 2.0, 1e-6);
    }

    [Fact]
    public void SpecificHeatRatio_ZeroCv_DivideByZero()
    {
        double gamma = ThermodynamicsEngine.SpecificHeatRatio(0, 100);
        gamma.Should().Be(double.PositiveInfinity);
    }
}
