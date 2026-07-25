namespace MathVerse.Simulation.Tests.Configuration;

using MathVerse.Math.Simulation.Solvers;

public sealed class SolversConfigurationTests
{
    [Fact]
    public void Default_DefaultMethod_IsRungeKutta4()
    {
        SolversConfiguration.Default.DefaultMethod.Should().Be(SolverType.RungeKutta4);
    }

    [Fact]
    public void Default_DefaultTolerance_IsOneEMinusSix()
    {
        SolversConfiguration.Default.DefaultTolerance.Should().Be(1e-6);
    }

    [Fact]
    public void Default_MaxSteps_IsOneMillion()
    {
        SolversConfiguration.Default.MaxSteps.Should().Be(1000000);
    }

    [Fact]
    public void SolverType_RungeKutta4_HasValue()
    {
        SolverType.RungeKutta4.Should().Be(SolverType.RungeKutta4);
    }

    [Fact]
    public void SolverType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<SolverType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void SolverType_ContainsRungeKutta4()
    {
        Enum.IsDefined(typeof(SolverType), SolverType.RungeKutta4).Should().BeTrue();
    }

    [Fact]
    public void CustomSolversConfiguration_Method()
    {
        var cfg = new SolversConfiguration { DefaultMethod = SolverType.ExplicitEuler };
        cfg.DefaultMethod.Should().Be(SolverType.ExplicitEuler);
    }

    [Fact]
    public void CustomSolversConfiguration_Tolerance()
    {
        var cfg = new SolversConfiguration { DefaultTolerance = 1e-9 };
        cfg.DefaultTolerance.Should().Be(1e-9);
    }

    [Fact]
    public void CustomSolversConfiguration_MaxSteps()
    {
        var cfg = new SolversConfiguration { MaxSteps = 500000 };
        cfg.MaxSteps.Should().Be(500000);
    }

    [Fact]
    public void Default_SameReference()
    {
        var a = SolversConfiguration.Default;
        var b = SolversConfiguration.Default;
        a.Should().BeSameAs(b);
    }
}
