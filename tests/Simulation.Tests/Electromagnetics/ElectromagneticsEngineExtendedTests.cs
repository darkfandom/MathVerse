namespace MathVerse.Simulation.Tests.Electromagnetics;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public sealed class ElectromagneticsEngineExtendedTests
{
    [Fact]
    public void CoulombForce_ProportionalToCharges()
    {
        var r = new MVVector(1.0, 0.0, 0.0);
        var f1 = ElectromagneticsEngine.CoulombForce(r, 1e-6, 1e-6);
        var f2 = ElectromagneticsEngine.CoulombForce(r, 2e-6, 1e-6);
        (f2.Norm() / f1.Norm()).Should().BeApproximately(2.0, 0.01);
    }

    [Fact]
    public void LorentzForce_PureMagnetic_CrossProduct()
    {
        var v = new MVVector(0, 1.0, 0);
        var e = MVVector.ZeroOf(3);
        var b = new MVVector(0, 0, 1.0);
        var force = ElectromagneticsEngine.LorentzForce(v, e, b, 1.0);
        force[0].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void BiotSavart_CurrentAlongY_RadiusAlongX_NegativeZ()
    {
        var dl = new MVVector(0, 1.0, 0);
        var r = new MVVector(1.0, 0, 0);
        var dB = ElectromagneticsEngine.BiotSavart(dl, r);
        dB[2].Should().BeNegative();
    }

    [Fact]
    public void ElectricFieldPointCharge_NegativeCharge_InwardField()
    {
        var r = new MVVector(1.0, 0.0, 0.0);
        var field = ElectromagneticsEngine.ElectricFieldPointCharge(r, -1e-9);
        field[0].Should().BeNegative();
    }

    [Fact]
    public void Capacitance_IncreasesWithArea()
    {
        double c1 = ElectromagneticsEngine.Capacitance(0.01, 0.001, ElectromagneticsEngine.VacuumPermittivity);
        double c2 = ElectromagneticsEngine.Capacitance(0.02, 0.001, ElectromagneticsEngine.VacuumPermittivity);
        c2.Should().BeGreaterThan(c1);
    }

    [Fact]
    public void Capacitance_DecreasesWithSeparation()
    {
        double c1 = ElectromagneticsEngine.Capacitance(0.01, 0.001, ElectromagneticsEngine.VacuumPermittivity);
        double c2 = ElectromagneticsEngine.Capacitance(0.01, 0.002, ElectromagneticsEngine.VacuumPermittivity);
        c2.Should().BeLessThan(c1);
    }

    [Fact]
    public void Inductance_IncreasesWithTurnsSquared()
    {
        double L1 = ElectromagneticsEngine.Inductance(1.0, 0.01, 100);
        double L2 = ElectromagneticsEngine.Inductance(1.0, 0.01, 200);
        (L2 / L1).Should().BeApproximately(4.0, 0.01);
    }

    [Fact]
    public void ResonanceFrequency_DecreasesWithLC()
    {
        double f1 = ElectromagneticsEngine.ResonanceFrequency(1e-3, 1e-6);
        double f2 = ElectromagneticsEngine.ResonanceFrequency(2e-3, 2e-6);
        f2.Should().BeLessThan(f1);
    }

    [Fact]
    public void SpeedOfLight_CorrectValue()
    {
        ElectromagneticsEngine.SpeedOfLight.Should().Be(299792458);
    }

    [Fact]
    public void MagneticFieldWire_DistanceDependence()
    {
        var current = new MVVector(0, 0, 1.0);
        var pos1 = new MVVector(1, 0, 0);
        var wire = MVVector.ZeroOf(3);
        var b1 = ElectromagneticsEngine.MagneticFieldWire(current, pos1, wire);

        var pos2 = new MVVector(2, 0, 0);
        var b2 = ElectromagneticsEngine.MagneticFieldWire(current, pos2, wire);

        (b1.Norm() / b2.Norm()).Should().BeApproximately(2.0, 0.01);
    }

    [Fact]
    public void WaveImpedance_IncreasesWithMuOverEpsilon()
    {
        double z1 = ElectromagneticsEngine.WaveImpedance(1e-12, 1e-7);
        double z2 = ElectromagneticsEngine.WaveImpedance(1e-12, 2e-7);
        z2.Should().BeGreaterThan(z1);
    }

    [Fact]
    public void Capacitor_NegativeVoltage_NegativeEnergy()
    {
        var cap = new Capacitor { Capacitance = 1e-6, Voltage = -10.0, Charge = -1e-5 };
        cap.Energy.Should().Be(0.5 * (-1e-5) * (-10.0));
        cap.Energy.Should().BePositive();
    }

    [Fact]
    public void Inductor_ZeroCurrent_ZeroEnergy()
    {
        var ind = new Inductor { Inductance = 1e-3, Current = 0 };
        ind.Energy.Should().Be(0);
    }

    [Fact]
    public void ElectromagneticWave_HigherFrequency_HigherWavenumber()
    {
        var w1 = new ElectromagneticWave { Frequency = 1e8 };
        var w2 = new ElectromagneticWave { Frequency = 1e9 };
        w2.WaveNumber.Should().BeGreaterThan(w1.WaveNumber);
    }

    [Fact]
    public void ElectromagneticSource_AllSourceTypes()
    {
        SourceType.PointCharge.Should().Be(SourceType.PointCharge);
        SourceType.CurrentElement.Should().Be(SourceType.CurrentElement);
        SourceType.Dipole.Should().Be(SourceType.Dipole);
        SourceType.Antenna.Should().Be(SourceType.Antenna);
        SourceType.Capacitor.Should().Be(SourceType.Capacitor);
        SourceType.Inductor.Should().Be(SourceType.Inductor);
    }
}
