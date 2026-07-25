namespace MathVerse.Simulation.Tests.Electromagnetics;

using MathVerse.Math.Numerics.LinearAlgebra;
using SM = global::System.Math;

public class ElectromagneticsEngineTests
{
    [Fact]
    public void CoulombForce_TwoLikeCharges_ProducesRepulsiveForce()
    {
        var r = new MVVector(1.0, 0.0, 0.0);
        var force = ElectromagneticsEngine.CoulombForce(r, 1e-6, 1e-6);

        force[0].Should().BePositive();
    }

    [Fact]
    public void CoulombForce_TwoOppositeCharges_ProducesAttractiveForce()
    {
        var r = new MVVector(1.0, 0.0, 0.0);
        var force = ElectromagneticsEngine.CoulombForce(r, 1e-6, -1e-6);

        force[0].Should().BeNegative();
    }

    [Fact]
    public void CoulombForce_ZeroSeparation_ReturnsZero()
    {
        var r = new MVVector(0.0, 0.0, 0.0);
        var force = ElectromagneticsEngine.CoulombForce(r, 1e-6, 1e-6);

        force.Norm().Should().Be(0);
    }

    [Fact]
    public void CoulombForce_InverseSquareLaw_DoublesDistanceReducesForceByFour()
    {
        var r1 = new MVVector(1.0, 0.0, 0.0);
        var r2 = new MVVector(2.0, 0.0, 0.0);
        var f1 = ElectromagneticsEngine.CoulombForce(r1, 1e-6, 1e-6);
        var f2 = ElectromagneticsEngine.CoulombForce(r2, 1e-6, 1e-6);

        var ratio = f1.Norm() / f2.Norm();
        ratio.Should().BeApproximately(4.0, 0.01);
    }

    [Fact]
    public void CoulombForce_ForceDirectionAlongSeparationVector()
    {
        var r = new MVVector(0.0, 3.0, 0.0);
        var force = ElectromagneticsEngine.CoulombForce(r, 1e-6, 1e-6);

        force[1].Should().BePositive();
        force[0].Should().BeApproximately(0, 1e-15);
        force[2].Should().BeApproximately(0, 1e-15);
    }

    [Fact]
    public void LorentzForce_PureElectricField_ReturnsChargeTimesField()
    {
        var velocity = new MVVector(1.0, 0.0, 0.0);
        var eField = new MVVector(0.0, 10.0, 0.0);
        var bField = new MVVector(0.0, 0.0, 0.0);
        double charge = 2.0;

        var force = ElectromagneticsEngine.LorentzForce(velocity, eField, bField, charge);

        force[0].Should().BeApproximately(0, 1e-10);
        force[1].Should().BeApproximately(20.0, 1e-10);
        force[2].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void LorentzForce_PureMagneticField_CrossProductForm()
    {
        var velocity = new MVVector(1.0, 0.0, 0.0);
        var eField = new MVVector(0.0, 0.0, 0.0);
        var bField = new MVVector(0.0, 0.0, 1.0);
        double charge = 1.0;

        var force = ElectromagneticsEngine.LorentzForce(velocity, eField, bField, charge);

        force[0].Should().BeApproximately(0, 1e-10);
        force[1].Should().BeApproximately(-1.0, 1e-10);
        force[2].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void LorentzForce_ZeroCharge_ReturnsZero()
    {
        var velocity = new MVVector(1.0, 2.0, 3.0);
        var eField = new MVVector(4.0, 5.0, 6.0);
        var bField = new MVVector(1.0, 0.0, 0.0);

        var force = ElectromagneticsEngine.LorentzForce(velocity, eField, bField, 0.0);

        force.Norm().Should().Be(0);
    }

    [Fact]
    public void BiotSavart_CurrentAlongX_RadiusAlongY_ProducesZField()
    {
        var currentElement = new MVVector(1.0, 0.0, 0.0);
        var r = new MVVector(0.0, 1.0, 0.0);

        var dB = ElectromagneticsEngine.BiotSavart(currentElement, r);

        dB[0].Should().BeApproximately(0, 1e-15);
        dB[1].Should().BeApproximately(0, 1e-15);
        dB[2].Should().BePositive();
    }

    [Fact]
    public void BiotSavart_ZeroDistance_ReturnsZero()
    {
        var currentElement = new MVVector(1.0, 0.0, 0.0);
        var r = new MVVector(0.0, 0.0, 0.0);

        var dB = ElectromagneticsEngine.BiotSavart(currentElement, r);

        dB.Norm().Should().Be(0);
    }

    [Fact]
    public void ElectricFieldPointCharge_RadialField()
    {
        var r = new MVVector(1.0, 0.0, 0.0);
        var field = ElectromagneticsEngine.ElectricFieldPointCharge(r, 1e-9);

        field[0].Should().BePositive();
        field[1].Should().BeApproximately(0, 1e-15);
        field[2].Should().BeApproximately(0, 1e-15);
    }

    [Fact]
    public void ElectricFieldPointCharge_InverseSquareLaw()
    {
        var r1 = new MVVector(1.0, 0.0, 0.0);
        var r2 = new MVVector(3.0, 0.0, 0.0);
        var e1 = ElectromagneticsEngine.ElectricFieldPointCharge(r1, 1e-9);
        var e2 = ElectromagneticsEngine.ElectricFieldPointCharge(r2, 1e-9);

        var ratio = e1.Norm() / e2.Norm();
        ratio.Should().BeApproximately(9.0, 0.01);
    }

    [Fact]
    public void ElectricFieldPointCharge_ZeroDistance_ReturnsZero()
    {
        var r = new MVVector(0.0, 0.0, 0.0);
        var field = ElectromagneticsEngine.ElectricFieldPointCharge(r, 1e-9);

        field.Norm().Should().Be(0);
    }

    [Fact]
    public void Inductance_SoloidFormula_CorrectValue()
    {
        double length = 1.0;
        double radius = 0.01;
        int turns = 100;

        double L = ElectromagneticsEngine.Inductance(length, radius, turns);

        double expected = 4 * SM.PI * 1e-7 * turns * turns * SM.PI * radius * radius / length;
        L.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void Capacitance_ParallelPlate_CorrectValue()
    {
        double area = 0.01;
        double separation = 0.001;
        double permittivity = ElectromagneticsEngine.VacuumPermittivity;

        double C = ElectromagneticsEngine.Capacitance(area, separation, permittivity);

        double expected = permittivity * area / separation;
        C.Should().BeApproximately(expected, 1e-15);
    }

    [Fact]
    public void ResonanceFrequency_LCCircuit_CorrectValue()
    {
        double L = 1e-3;
        double C = 1e-6;

        double f = ElectromagneticsEngine.ResonanceFrequency(L, C);

        double expected = 1.0 / (2 * SM.PI * SM.Sqrt(L * C));
        f.Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void WaveImpedance_FreeSpace_CorrectValue()
    {
        double epsilon = ElectromagneticsEngine.VacuumPermittivity;
        double mu = ElectromagneticsEngine.VacuumPermeability;

        double Z = ElectromagneticsEngine.WaveImpedance(epsilon, mu);

        Z.Should().BeApproximately(376.73, 1.0);
    }

    [Fact]
    public void VacuumPermittivity_MatchesPhysicalConstant()
    {
        ElectromagneticsEngine.VacuumPermittivity.Should().BeApproximately(8.854187817e-12, 1e-20);
    }

    [Fact]
    public void VacuumPermeability_MatchesPhysicalConstant()
    {
        ElectromagneticsEngine.VacuumPermeability.Should().BeApproximately(4 * SM.PI * 1e-7, 1e-15);
    }

    [Fact]
    public void ElectromagneticField_Vacuum_ZeroFields()
    {
        var field = ElectromagneticField.Vacuum();

        field.ElectricField.Norm().Should().Be(0);
        field.MagneticField.Norm().Should().Be(0);
        field.ChargeDensity.Should().Be(0);
    }

    [Fact]
    public void ElectromagneticField_ElectricEnergyDensity_CorrectFormula()
    {
        var field = new ElectromagneticField
        {
            ElectricField = new MVVector(100.0, 0.0, 0.0),
            Permittivity = ElectromagneticsEngine.VacuumPermittivity
        };

        double energy = field.ElectricEnergyDensity;

        energy.Should().BeApproximately(0.5 * ElectromagneticsEngine.VacuumPermittivity * 10000, 1e-15);
    }

    [Fact]
    public void ElectromagneticWave_Wavelength_CorrectFrequencyRelation()
    {
        var wave = new ElectromagneticWave
        {
            Frequency = 1e9,
            Amplitude = 1.0
        };

        double wavelength = wave.Wavelength;

        wavelength.Should().BeApproximately(ElectromagneticsEngine.SpeedOfLight / 1e9, 1.0);
    }

    [Fact]
    public void ElectromagneticWave_ZeroFrequency_WavelengthZero()
    {
        var wave = new ElectromagneticWave { Frequency = 0 };

        wave.Wavelength.Should().Be(0);
    }

    [Fact]
    public void Capacitor_Energy_CorrectFormula()
    {
        var cap = new Capacitor
        {
            Capacitance = 1e-6,
            Voltage = 10.0,
            Charge = 1e-6 * 10.0
        };

        cap.Energy.Should().BeApproximately(0.5 * cap.Charge * cap.Voltage, 1e-15);
    }

    [Fact]
    public void Inductor_Energy_CorrectFormula()
    {
        var ind = new Inductor
        {
            Inductance = 1e-3,
            Current = 2.0
        };

        ind.Energy.Should().BeApproximately(0.5 * 1e-3 * 4.0, 1e-15);
    }
}
