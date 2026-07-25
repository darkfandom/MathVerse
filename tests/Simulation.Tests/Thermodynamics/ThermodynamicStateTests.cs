namespace MathVerse.Simulation.Tests.Thermodynamics;

using ThermodynamicHeatTransfer = MathVerse.Math.Simulation.Thermodynamics.HeatTransfer;
using ThermodynamicPhase = MathVerse.Math.Simulation.Thermodynamics.Phase;

public sealed class ThermodynamicStateTests
{
    [Fact]
    public void DefaultProperties_AreZero()
    {
        var state = new ThermodynamicState();
        state.Temperature.Should().Be(0);
        state.Pressure.Should().Be(0);
        state.Volume.Should().Be(0);
        state.InternalEnergy.Should().Be(0);
        state.Enthalpy.Should().Be(0);
        state.Entropy.Should().Be(0);
        state.Mass.Should().Be(0);
        state.SpecificHeatCapacity.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var state = new ThermodynamicState
        {
            Temperature = 300,
            Pressure = 101325,
            Volume = 0.0224,
            InternalEnergy = 5000,
            Enthalpy = 5500,
            Entropy = 15,
            Mass = 2.0,
            SpecificHeatCapacity = 4186,
            Phase = ThermodynamicPhase.Gas
        };

        state.Temperature.Should().Be(300);
        state.Pressure.Should().Be(101325);
        state.Volume.Should().Be(0.0224);
        state.InternalEnergy.Should().Be(5000);
        state.Enthalpy.Should().Be(5500);
        state.Entropy.Should().Be(15);
        state.Mass.Should().Be(2.0);
        state.SpecificHeatCapacity.Should().Be(4186);
        state.Phase.Should().Be(ThermodynamicPhase.Gas);
    }

    [Fact]
    public void Phase_DefaultIsSolid()
    {
        var state = new ThermodynamicState();
        state.Phase.Should().Be(ThermodynamicPhase.Solid);
    }

    [Fact]
    public void SpeciesConcentrations_DefaultIsEmpty()
    {
        var state = new ThermodynamicState();
        state.SpeciesConcentrations.Should().BeEmpty();
    }

    [Fact]
    public void HeatTransfer_Conduction_FluxCalculation()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Conduction,
            Coefficient = 100,
            TemperatureDifference = 10,
            Area = 1.0
        };
        ht.HeatFlux.Should().BeApproximately(100 * 10, 1e-6);
    }

    [Fact]
    public void HeatTransfer_Convection_FluxCalculation()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Convection,
            Coefficient = 50,
            TemperatureDifference = 5,
            Area = 2.0
        };
        ht.HeatFlux.Should().BeApproximately(50 * 5, 1e-6);
    }

    [Fact]
    public void HeatTransfer_Radiation_FluxCalculation()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Radiation,
            TemperatureHot = 500,
            TemperatureCold = 300,
            Emissivity = 1.0
        };

        double expected = ThermodynamicHeatTransfer.StefanBoltzmannConstant * 1.0 *
            (System.Math.Pow(500, 4) - System.Math.Pow(300, 4));
        ht.HeatFlux.Should().BeApproximately(expected, 1e-3);
    }

    [Fact]
    public void StefanBoltzmannConstant_MatchesKnownValue()
    {
        ThermodynamicHeatTransfer.StefanBoltzmannConstant.Should().BeApproximately(5.670374419e-8, 1e-15);
    }

    [Fact]
    public void HeatTransfer_Radiation_ZeroTempDifference_IsZero()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Radiation,
            TemperatureHot = 300,
            TemperatureCold = 300,
            Emissivity = 0.9
        };
        ht.HeatFlux.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void HeatTransfer_Radiation_DefaultEmissivity_IsPointNine()
    {
        var ht = new ThermodynamicHeatTransfer
        {
            Mode = HeatTransferMode.Radiation,
            TemperatureHot = 500,
            TemperatureCold = 300
        };
        ht.Emissivity.Should().Be(0.9);
    }
}
