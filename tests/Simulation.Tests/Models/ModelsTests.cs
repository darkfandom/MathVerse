namespace MathVerse.Simulation.Tests.Models;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Simulation.Physics;
using MathVerse.Math.Simulation.Chemistry;
using MathVerse.Math.Simulation.Biology;
using MathVerse.Math.Simulation.Finance;
using MathVerse.Math.Simulation.Electromagnetics;
using MathVerse.Math.Simulation.Solvers;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;
using SysModels = MathVerse.Math.Simulation.Models;

public sealed class ModelsTests
{
    [Fact]
    public void PhysicalSystem_DefaultValues()
    {
        var ps = new SysModels.PhysicalSystem();
        ps.Particles.Should().BeEmpty();
        ps.RigidBodies.Should().BeEmpty();
        ps.Constraints.Should().BeEmpty();
        ps.Forces.Should().BeEmpty();
        ps.Time.Should().Be(0);
    }

    [Fact]
    public void ChemicalSystem_DefaultValues()
    {
        var cs = new SysModels.ChemicalSystem();
        cs.Species.Should().BeEmpty();
        cs.Reactions.Should().BeEmpty();
        cs.Temperature.Should().Be(0);
        cs.Pressure.Should().Be(0);
    }

    [Fact]
    public void BiologicalSystem_DefaultValues()
    {
        var bs = new SysModels.BiologicalSystem();
        bs.Species.Should().BeEmpty();
        bs.Interactions.Should().BeEmpty();
        bs.Populations.Should().BeEmpty();
        bs.Time.Should().Be(0);
    }

    [Fact]
    public void FinancialSystem_DefaultValues()
    {
        var fs = new SysModels.FinancialSystem();
        fs.Assets.Should().BeEmpty();
        fs.Options.Should().BeEmpty();
        fs.RiskFreeRate.Should().Be(0);
        fs.CurrentTime.Should().Be(0);
    }

    [Fact]
    public void ControlSystemModel_DefaultValues()
    {
        var csm = new SysModels.ControlSystemModel();
        csm.Plant.Should().NotBeNull();
        csm.Controller.Should().NotBeNull();
        csm.SampleTime.Should().Be(0);
    }

    [Fact]
    public void MonteCarloExperiment_DefaultValues()
    {
        var mce = new SysModels.MonteCarloExperiment();
        mce.Samples.Should().Be(10000);
        mce.Iterations.Should().Be(1000);
        mce.Confidence.Should().Be(0.95);
    }

    [Fact]
    public void OptimizationProblem_DefaultValues()
    {
        var op = new SysModels.OptimizationProblem();
        op.Method.Should().Be(SolverType.RungeKutta4);
    }

    [Fact]
    public void PDEProblem_DefaultValues()
    {
        var pde = new SysModels.PDEProblem();
        pde.Domain.Should().BeEmpty();
        pde.DiffusionCoefficient.Should().Be(1.0);
    }

    [Fact]
    public void PDEType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<SysModels.PDEType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void PDEType_ContainsExpectedValues()
    {
        Enum.GetValues<SysModels.PDEType>().Should().HaveCount(7);
    }

    [Fact]
    public void TransferFunction_Create_SetsCoefficients()
    {
        var tf = SysModels.TransferFunction.Create(new double[] { 1, 0 }, new double[] { 1, 1 });
        tf.Numerator.Should().HaveCount(2);
        tf.Denominator.Should().HaveCount(2);
    }

    [Fact]
    public void TransferFunction_Evaluate_AtZero()
    {
        var tf = SysModels.TransferFunction.Create(new double[] { 1 }, new double[] { 1, 1 });
        var result = tf.Evaluate(System.Numerics.Complex.Zero);
        result.Real.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void PIDController_Update_ReturnsControlSignal()
    {
        var pid = new SysModels.PIDController { Kp = 1.0, Ki = 0.1, Kd = 0.01, Setpoint = 10.0 };
        double integral = 0, prevError = 0;
        double output = pid.Update(5.0, 0.1, ref integral, ref prevError);
        output.Should().NotBe(0);
    }

    [Fact]
    public void PIDController_PureProportional()
    {
        var pid = new SysModels.PIDController { Kp = 2.0, Ki = 0, Kd = 0, Setpoint = 10.0 };
        double integral = 0, prevError = 0;
        double output = pid.Update(8.0, 0.1, ref integral, ref prevError);
        output.Should().BeApproximately(2.0 * 2.0, 1e-10);
    }

    [Fact]
    public void PIDController_IntegralAccumulation()
    {
        var pid = new SysModels.PIDController { Kp = 0, Ki = 1.0, Kd = 0, Setpoint = 10.0 };
        double integral = 0, prevError = 0;
        pid.Update(0.0, 1.0, ref integral, ref prevError);
        integral.Should().BeApproximately(10.0, 1e-10);
    }

    [Fact]
    public void Asset_DefaultValues()
    {
        var asset = new SysModels.Asset();
        asset.Symbol.Should().Be(string.Empty);
        asset.Price.Should().Be(0);
        asset.Volatility.Should().Be(0);
    }

    [Fact]
    public void OptionContract_DefaultValues()
    {
        var opt = new OptionContract();
        opt.Underlying.Should().Be(string.Empty);
        opt.Strike.Should().Be(0);
        opt.Expiration.Should().Be(0);
    }

    [Fact]
    public void ElectromagneticSystem_DefaultValues()
    {
        var ems = new SysModels.ElectromagneticSystem();
        ems.Sources.Should().BeEmpty();
        ems.Frequency.Should().Be(0);
    }

    [Fact]
    public void Compartment_DefaultValues()
    {
        var c = new SysModels.Compartment();
        c.Id.Should().Be(string.Empty);
        c.Volume.Should().Be(0);
        c.Temperature.Should().Be(0);
    }

    [Fact]
    public void ThermodynamicSystem_DefaultValues()
    {
        var ts = new SysModels.ThermodynamicSystem();
        ts.Compartments.Should().BeEmpty();
        ts.Transfers.Should().BeEmpty();
        ts.Time.Should().Be(0);
    }

    [Fact]
    public void StateSpaceModel_DefaultValues()
    {
        var ssm = new SysModels.StateSpaceModel();
        ssm.A.Should().Be(MVVector.Zero);
        ssm.B.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void PhysicalSystem_WithGravity()
    {
        var ps = new SysModels.PhysicalSystem
        {
            Gravity = new MVVector(0, -9.81, 0)
        };
        ps.Gravity[1].Should().Be(-9.81);
    }

    [Fact]
    public void FinancialSystem_WithRiskFreeRate()
    {
        var fs = new SysModels.FinancialSystem { RiskFreeRate = 0.05 };
        fs.RiskFreeRate.Should().Be(0.05);
    }

    [Fact]
    public void PIDController_DefaultDerivativeFilter()
    {
        var pid = new SysModels.PIDController();
        pid.DerivativeFilter.Should().Be(0.1);
    }

    [Fact]
    public void PIDController_DefaultIntegralLimit()
    {
        var pid = new SysModels.PIDController();
        pid.IntegralLimit.Should().Be(double.MaxValue);
    }

    [Fact]
    public void ChemicalSystem_WithTemperature()
    {
        var cs = new SysModels.ChemicalSystem { Temperature = 298.15, Pressure = 101325 };
        cs.Temperature.Should().Be(298.15);
        cs.Pressure.Should().Be(101325);
    }

    [Fact]
    public void BiologicalSystem_WithTime()
    {
        var bs = new SysModels.BiologicalSystem { Time = 10.0 };
        bs.Time.Should().Be(10.0);
    }

    [Fact]
    public void PDEProblem_WithDiffusion()
    {
        var pde = new SysModels.PDEProblem
        {
            Type = SysModels.PDEType.Heat,
            DiffusionCoefficient = 0.5
        };
        pde.DiffusionCoefficient.Should().Be(0.5);
        pde.Type.Should().Be(SysModels.PDEType.Heat);
    }
}
