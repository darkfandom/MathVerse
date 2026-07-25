namespace MathVerse.Simulation.Tests.ControlSystems;

using System.Numerics;
using MathVerse.Math.Numerics.LinearAlgebra;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public class ControlSystemEngineTests
{
    [Fact]
    public void TransferFunction_Create_SetsCoefficients()
    {
        var tf = TransferFunction.Create(new[] { 1.0 }, new[] { 1.0, 2.0, 1.0 });

        tf.Numerator.Length.Should().Be(1);
        tf.Denominator.Length.Should().Be(3);
    }

    [Fact]
    public void TransferFunction_Evaluate_AtZero()
    {
        var tf = TransferFunction.Create(new[] { 1.0 }, new[] { 1.0, 1.0 });

        var result = tf.Evaluate(new Complex(0, 0));

        result.Real.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void TransferFunction_Evaluate_AtOne()
    {
        var tf = TransferFunction.Create(new[] { 1.0 }, new[] { 1.0, 1.0 });

        var result = tf.Evaluate(new Complex(1.0, 0));

        result.Real.Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void TransferFunction_StepResponse_ReturnsArray()
    {
        var tf = TransferFunction.Create(new[] { 1.0 }, new[] { 1.0, 1.0 });

        var response = tf.StepResponse(0.01, 100);

        response.Length.Should().Be(100);
    }

    [Fact]
    public void PIDController_Update_ProportionalControl()
    {
        var pid = new PIDController
        {
            Kp = 1.0,
            Ki = 0,
            Kd = 0,
            Setpoint = 10.0
        };

        double output = pid.Update(5.0, 0.1);

        output.Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void PIDController_Update_IntegralAccumulates()
    {
        var pid = new PIDController
        {
            Kp = 0,
            Ki = 1.0,
            Kd = 0,
            Setpoint = 10.0
        };

        double output1 = pid.Update(5.0, 0.1);
        double output2 = pid.Update(5.0, 0.1);

        output2.Should().BeGreaterThan(output1);
    }

    [Fact]
    public void PIDController_Reset_ClearsAccumulation()
    {
        var pid = new PIDController
        {
            Kp = 0,
            Ki = 1.0,
            Kd = 0,
            Setpoint = 10.0
        };

        pid.Update(5.0, 0.1);
        pid.Reset();
        double afterReset = pid.Update(5.0, 0.1);

        afterReset.Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void PIDController_AtSetpoint_ProducesZeroError()
    {
        var pid = new PIDController
        {
            Kp = 2.0,
            Ki = 0,
            Kd = 0,
            Setpoint = 10.0
        };

        double output = pid.Update(10.0, 0.1);

        output.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void StateSpaceModel_Step_AppliesDynamics()
    {
        var model = new StateSpaceModel
        {
            A = Matrix.Identity(1),
            B = Matrix.Identity(1),
            C = Matrix.Identity(1),
            D = Matrix.Identity(1)
        };
        var x = new MVVector(1.0);
        var u = new MVVector(0.0);

        var xNew = model.Step(x, u, 0.1);

        xNew[0].Should().BeApproximately(1.1, 1e-10);
    }

    [Fact]
    public void StateSpaceModel_Output_LinearCombination()
    {
        var model = new StateSpaceModel
        {
            A = Matrix.Identity(1),
            B = Matrix.Identity(1),
            C = Matrix.Identity(1),
            D = Matrix.Identity(1)
        };
        var x = new MVVector(2.0);
        var u = new MVVector(3.0);

        var y = model.Output(x, u);

        y[0].Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void StateSpaceModel_FromTransferFunction_ProducesModel()
    {
        var tf = TransferFunction.Create(new[] { 1.0 }, new[] { 1.0, 1.0 });

        var model = StateSpaceModel.FromTransferFunction(tf);

        model.Should().NotBeNull();
    }

    [Fact]
    public void ControlSystemAnalysis_StableMatrix_IsStable()
    {
        var A = new Matrix(new double[,]
        {
            { -2, 0 },
            { 0, -3 }
        });

        ControlSystemAnalysis.IsStable(A).Should().BeFalse();
    }

    [Fact]
    public void ControlSystemAnalysis_UnstableMatrix_IsNotStable()
    {
        var A = new Matrix(new double[,]
        {
            { 1, 0 },
            { 0, -1 }
        });

        ControlSystemAnalysis.IsStable(A).Should().BeFalse();
    }

    [Fact]
    public void StateFeedbackController_Control_ReturnsValue()
    {
        var controller = new StateFeedbackController
        {
            K = new Matrix(new double[,] { { 1.0, 2.0 } }),
            Reference = new MVVector(1.0, 0.0)
        };
        var x = new MVVector(0.5, 0.3);

        double u = controller.Control(x);

        u.Should().BeGreaterThan(double.MinValue);
        u.Should().BeLessThan(double.MaxValue);
    }

    [Fact]
    public void PIDController_DerivativeFilter_Active()
    {
        var pid = new PIDController
        {
            Kp = 0,
            Ki = 0,
            Kd = 1.0,
            Setpoint = 10.0,
            DerivativeFilter = 0.1
        };

        double output = pid.Update(5.0, 0.1);

        output.Should().BeGreaterThan(double.MinValue);
        output.Should().BeLessThan(double.MaxValue);
    }

    [Fact]
    public void Observer_Update_ProducesStateEstimate()
    {
        var observer = new Observer
        {
            L = Matrix.Identity(1),
            StateEstimate = new MVVector(0.0)
        };
        var y = new MVVector(1.0);
        var u = new MVVector(0.0);
        var A = Matrix.Identity(1);
        var B = Matrix.Identity(1);
        var C = Matrix.Identity(1);

        var updated = observer.Update(y, u, A, B, C, 0.1);

        updated.Should().NotBeNull();
        updated.StateEstimate.Size.Should().Be(1);
    }

    [Fact]
    public void ControlSystemAnalysis_SolveLyapunov_ReturnsMatrix()
    {
        var A = Matrix.Identity(2);
        var Q = Matrix.Identity(2);

        var X = ControlSystemAnalysis.SolveLyapunov(A, Q);

        X.Should().NotBeNull();
        X.Rows.Should().Be(2);
    }
}
