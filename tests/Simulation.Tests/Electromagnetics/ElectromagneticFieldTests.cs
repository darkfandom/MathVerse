namespace MathVerse.Simulation.Tests.Electromagnetics;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public sealed class ElectromagneticFieldTests
{
    [Fact]
    public void ElectromagneticField_Vacuum_ZeroFields()
    {
        var field = ElectromagneticField.Vacuum();
        field.ElectricField.Norm().Should().Be(0);
        field.MagneticField.Norm().Should().Be(0);
        field.ChargeDensity.Should().Be(0);
    }

    [Fact]
    public void ElectromagneticField_Vacuum_CorrectPermittivity()
    {
        var field = ElectromagneticField.Vacuum();
        field.Permittivity.Should().BeApproximately(8.854187817e-12, 1e-20);
    }

    [Fact]
    public void ElectromagneticField_Vacuum_CorrectPermeability()
    {
        var field = ElectromagneticField.Vacuum();
        field.Permeability.Should().BeApproximately(4 * SM.PI * 1e-7, 1e-15);
    }

    [Fact]
    public void ElectricEnergyDensity_CorrectFormula()
    {
        var field = new ElectromagneticField
        {
            ElectricField = new MVVector(100.0, 0.0, 0.0),
            Permittivity = ElectromagneticsEngine.VacuumPermittivity
        };
        double expected = 0.5 * ElectromagneticsEngine.VacuumPermittivity * 10000;
        field.ElectricEnergyDensity.Should().BeApproximately(expected, 1e-15);
    }

    [Fact]
    public void ElectricEnergyDensity_ZeroField_IsZero()
    {
        var field = new ElectromagneticField
        {
            ElectricField = MVVector.Zero,
            Permittivity = ElectromagneticsEngine.VacuumPermittivity
        };
        field.ElectricEnergyDensity.Should().Be(0);
    }

    [Fact]
    public void MagneticEnergyDensity_CorrectFormula()
    {
        var field = new ElectromagneticField
        {
            MagneticField = new MVVector(0.5, 0, 0),
            Permeability = ElectromagneticsEngine.VacuumPermeability
        };
        double expected = 0.5 * 0.25 / ElectromagneticsEngine.VacuumPermeability;
        field.MagneticEnergyDensity.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void MagneticEnergyDensity_ZeroField_IsZero()
    {
        var field = new ElectromagneticField
        {
            MagneticField = MVVector.Zero,
            Permeability = ElectromagneticsEngine.VacuumPermeability
        };
        field.MagneticEnergyDensity.Should().Be(0);
    }

    [Fact]
    public void PoyntingVector_CrossProductOfEAndH()
    {
        var field = new ElectromagneticField
        {
            ElectricField = new MVVector(1, 0, 0),
            MagneticIntensity = new MVVector(0, 1, 0)
        };
        var s = field.PoyntingVector;
        s[2].Should().BeApproximately(1.0, 1e-10);
        s[0].Should().BeApproximately(0, 1e-10);
        s[1].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void PoyntingVector_ZeroFields_IsZero()
    {
        var field = new ElectromagneticField
        {
            ElectricField = MVVector.ZeroOf(3),
            MagneticIntensity = MVVector.ZeroOf(3)
        };
        field.PoyntingVector.Norm().Should().Be(0);
    }

    [Fact]
    public void ElectromagneticSource_DefaultValues()
    {
        var src = new ElectromagneticSource();
        src.Id.Should().Be(string.Empty);
        src.Charge.Should().Be(0);
        src.Frequency.Should().Be(0);
        src.Amplitude.Should().Be(0);
    }

    [Fact]
    public void SourceType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<SourceType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Capacitor_Energy()
    {
        var cap = new Capacitor { Capacitance = 1e-6, Voltage = 10.0, Charge = 1e-5 };
        cap.Energy.Should().BeApproximately(0.5 * 1e-5 * 10.0, 1e-15);
    }

    [Fact]
    public void Inductor_Energy()
    {
        var ind = new Inductor { Inductance = 0.5, Current = 4.0 };
        ind.Energy.Should().BeApproximately(0.5 * 0.5 * 16.0, 1e-15);
    }

    [Fact]
    public void ElectromagneticWave_Wavelength()
    {
        var wave = new ElectromagneticWave { Frequency = 1e8 };
        wave.Wavelength.Should().BeApproximately(ElectromagneticsEngine.SpeedOfLight / 1e8, 1.0);
    }

    [Fact]
    public void ElectromagneticWave_ZeroFrequency_WavelengthZero()
    {
        var wave = new ElectromagneticWave { Frequency = 0 };
        wave.Wavelength.Should().Be(0);
    }

    [Fact]
    public void ElectromagneticWave_WaveNumber()
    {
        var wave = new ElectromagneticWave { Frequency = 1e9 };
        double expected = 2 * SM.PI / (ElectromagneticsEngine.SpeedOfLight / 1e9);
        wave.WaveNumber.Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void PoyntingMagnitude_CorrectValue()
    {
        var e = new MVVector(1, 0, 0);
        var h = new MVVector(0, 1, 0);
        double mag = ElectromagneticsEngine.PoyntingMagnitude(e, h);
        mag.Should().BeApproximately(1.0, 1e-10);
    }
}
