using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class ControlSystemBenchmarks
{
    private PIDController _pid = null!;
    private TransferFunction _tf1 = null!;
    private TransferFunction _tf2 = null!;
    private TransferFunction _tfHighOrder = null!;

    [GlobalSetup]
    public void Setup()
    {
        _pid = new PIDController
        {
            Kp = 2.0,
            Ki = 0.5,
            Kd = 0.1,
            Setpoint = 10.0,
            IntegralLimit = 100.0,
            DerivativeFilter = 0.1
        };

        _tf1 = TransferFunction.Create(new double[] { 1.0 }, new double[] { 1.0, 1.0 });
        _tf2 = TransferFunction.Create(new double[] { 1.0, 0.0 }, new double[] { 1.0, 2.0, 1.0 });
        _tfHighOrder = TransferFunction.Create(
            new double[] { 1.0, 0.0, 0.0, 0.0 },
            new double[] { 1.0, 4.0, 6.0, 4.0, 1.0 });
    }

    [Benchmark]
    public double PID_Update_SingleStep()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.1, Setpoint = 10.0 };
        return pid.Update(5.0, 0.01);
    }

    [Benchmark]
    public double PID_Update_100Steps()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.1, Setpoint = 10.0 };
        double result = 0;
        for (int i = 0; i < 100; i++)
            result = pid.Update(5.0 + i * 0.05, 0.01);
        return result;
    }

    [Benchmark]
    public void PID_Reset()
    {
        _pid.Update(5.0, 0.01);
        _pid.Update(6.0, 0.01);
        _pid.Reset();
    }

    [Benchmark]
    public double PID_KpOnly()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.0, Kd = 0.0, Setpoint = 10.0 };
        return pid.Update(5.0, 0.01);
    }

    [Benchmark]
    public double PID_KiOnly()
    {
        var pid = new PIDController { Kp = 0.0, Ki = 0.5, Kd = 0.0, Setpoint = 10.0 };
        return pid.Update(5.0, 0.01);
    }

    [Benchmark]
    public double PID_KdOnly()
    {
        var pid = new PIDController { Kp = 0.0, Ki = 0.0, Kd = 0.1, Setpoint = 10.0 };
        return pid.Update(5.0, 0.01);
    }

    [Benchmark]
    public double PID_FullPID()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.1, Setpoint = 10.0 };
        return pid.Update(5.0, 0.01);
    }

    [Benchmark]
    public TransferFunction TransferFunction_Create()
        => TransferFunction.Create(new double[] { 1.0, 1.0 }, new double[] { 1.0, 2.0, 1.0 });

    [Benchmark]
    public Complex TransferFunction_Evaluate_DC() => _tf1.Evaluate(Complex.Zero);

    [Benchmark]
    public Complex TransferFunction_Evaluate_HighFreq() => _tf1.Evaluate(new Complex(0, 1000.0));

    [Benchmark]
    public Complex TransferFunction_Evaluate_ComplexFreq() => _tf2.Evaluate(new Complex(1.0, 2.0));

    [Benchmark]
    public double[] TransferFunction_StepResponse() => _tf1.StepResponse(0.01, 100);

    [Benchmark]
    public TransferFunction TransferFunction_HighOrder_Create()
        => TransferFunction.Create(
            new double[] { 1.0, 0.0, 0.0, 0.0 },
            new double[] { 1.0, 4.0, 6.0, 4.0, 1.0 });

    [Benchmark]
    public Complex TransferFunction_HighOrder_Evaluate() => _tfHighOrder.Evaluate(new Complex(0.5, 0.5));

    [Benchmark]
    public double PIDController_LargeSetpoint()
    {
        var pid = new PIDController { Kp = 10.0, Ki = 1.0, Kd = 0.5, Setpoint = 1000.0 };
        return pid.Update(100.0, 0.01);
    }

    [Benchmark]
    public double PIDController_SmallSetpoint()
    {
        var pid = new PIDController { Kp = 0.1, Ki = 0.01, Kd = 0.001, Setpoint = 0.001 };
        return pid.Update(0.0005, 0.001);
    }

    [Benchmark]
    public double PIDController_MultipleSteps_WithDynamics()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.1, Setpoint = 10.0 };
        double measurement = 0.0;
        double result = 0;
        for (int i = 0; i < 50; i++)
        {
            result = pid.Update(measurement, 0.01);
            measurement += result * 0.001;
        }
        return result;
    }

    [Benchmark]
    public Complex TransferFunction_FirstOrder()
    {
        var tf = TransferFunction.Create(new double[] { 1.0 }, new double[] { 1.0, 1.0 });
        return tf.Evaluate(new Complex(1.0, 0.0));
    }

    [Benchmark]
    public Complex TransferFunction_SecondOrder()
    {
        var tf = TransferFunction.Create(new double[] { 1.0 }, new double[] { 1.0, 0.5, 1.0 });
        return tf.Evaluate(new Complex(0.0, 1.0));
    }

    [Benchmark]
    public Complex TransferFunction_ThirdOrder()
    {
        var tf = TransferFunction.Create(
            new double[] { 1.0, 0.0 },
            new double[] { 1.0, 3.0, 3.0, 1.0 });
        return tf.Evaluate(new Complex(0.5, 0.5));
    }

    [Benchmark]
    public double PID_Kp_Ki_Only()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.0, Setpoint = 10.0 };
        double result = 0;
        for (int i = 0; i < 10; i++)
            result = pid.Update(5.0, 0.01);
        return result;
    }

    [Benchmark]
    public double PID_Kp_Kd_Only()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.0, Kd = 0.1, Setpoint = 10.0 };
        double result = 0;
        for (int i = 0; i < 10; i++)
            result = pid.Update(5.0 + i * 0.1, 0.01);
        return result;
    }

    [Benchmark]
    public double PID_LargeError()
    {
        var pid = new PIDController { Kp = 5.0, Ki = 2.0, Kd = 1.0, Setpoint = 100.0, IntegralLimit = 500.0 };
        return pid.Update(0.0, 0.01);
    }

    [Benchmark]
    public double PID_ZeroError()
    {
        var pid = new PIDController { Kp = 2.0, Ki = 0.5, Kd = 0.1, Setpoint = 10.0 };
        pid.Update(10.0, 0.01);
        return pid.Update(10.0, 0.01);
    }

    [Benchmark]
    public Complex TransferFunction_Magnitude()
    {
        var tf = TransferFunction.Create(
            new double[] { 1.0, 2.0 },
            new double[] { 1.0, 3.0, 2.0 });
        var result = tf.Evaluate(new Complex(0.0, 1.0));
        return result;
    }
}
