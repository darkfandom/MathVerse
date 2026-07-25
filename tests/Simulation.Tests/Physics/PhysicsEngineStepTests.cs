namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class PhysicsEngineStepTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void Step_NoParticles_TimeAdvances()
    {
        var state = PhysicsState.Create(Vec3(0, -9.81, 0));
        var result = PhysicsEngine.Step(state, 0.01);
        result.Time.Should().BeApproximately(0.01, 1e-10);
    }

    [Fact]
    public void Step_FixedParticle_DoesNotMove()
    {
        var particle = Particle.Create("p1", Vec3(1, 2, 3), Vec3(0, 0, 0), 1.0) with { IsFixed = true };
        var state = PhysicsState.Create(Vec3(0, -9.81, 0)) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", particle)
        };
        var result = PhysicsEngine.Step(state, 0.01);
        result.Particles["p1"].Position[0].Should().BeApproximately(1.0, 1e-10);
        result.Particles["p1"].Position[1].Should().BeApproximately(2.0, 1e-10);
        result.Particles["p1"].Position[2].Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void Step_FreeParticle_Accelerates()
    {
        var particle = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var state = PhysicsState.Create(Vec3(0, -9.81, 0)) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", particle)
        };
        var result = PhysicsEngine.Step(state, 0.01);
        result.Particles["p1"].Velocity[1].Should().NotBe(0);
    }

    [Fact]
    public void Step_PreservesParticleCount()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(1, 1, 1), Vec3(0, 0, 0), 2.0);
        var state = PhysicsState.Create(Vec3(0, 0, 0)) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty
                .Add("p1", p1).Add("p2", p2)
        };
        var result = PhysicsEngine.Step(state, 0.01);
        result.Particles.Should().HaveCount(2);
    }

    [Fact]
    public void ComputeGravitationalForce_Attractive()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1e10);
        var p2 = Particle.Create("p2", Vec3(1, 0, 0), Vec3(0, 0, 0), 1e10);
        var force = PhysicsEngine.ComputeGravitationalForce(p1, p2);
        force[0].Should().BePositive();
    }

    [Fact]
    public void ComputeGravitationalForce_ZeroDistance_ReturnsZero()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var force = PhysicsEngine.ComputeGravitationalForce(p1, p2);
        force.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void ComputeGravitationalForce_InverseSquareDistance()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1e10);
        var p2a = Particle.Create("p2a", Vec3(1, 0, 0), Vec3(0, 0, 0), 1e10);
        var p2b = Particle.Create("p2b", Vec3(2, 0, 0), Vec3(0, 0, 0), 1e10);
        var f1 = PhysicsEngine.ComputeGravitationalForce(p1, p2a);
        var f2 = PhysicsEngine.ComputeGravitationalForce(p1, p2b);
        var ratio = f1.Norm() / f2.Norm();
        ratio.Should().BeApproximately(4.0, 0.01);
    }

    [Fact]
    public void ComputeSpringForce_Attractive()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(2, 0, 0), Vec3(0, 0, 0), 1.0);
        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 1.0, 100.0);
        force[0].Should().BeNegative();
    }

    [Fact]
    public void ComputeSpringForce_Compressed_Replusive()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(0.5, 0, 0), Vec3(0, 0, 0), 1.0);
        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 1.0, 100.0);
        force[0].Should().BePositive();
    }

    [Fact]
    public void ComputeSpringForce_AtRestLength_IsZero()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(1, 0, 0), Vec3(0, 0, 0), 1.0);
        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 1.0, 100.0);
        force.Norm().Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void ComputeNetForce_ReturnsZeroForNoForces()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        PhysicsEngine.ComputeNetForce(p).Should().Be(MVVector.Zero);
    }

    [Fact]
    public void ForceType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<ForceType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ForceType_ContainsAllExpected()
    {
        Enum.GetValues<ForceType>().Should().HaveCount(9);
    }
}
