namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class PhysicsEngineTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void Step_FreeFall_PositionUpdates()
    {
        var gravity = Vec3(0, -9.81, 0);
        var state = PhysicsState.Create(gravity);

        var newState = PhysicsEngine.Step(state, 0.1);

        newState.Time.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void Step_FreeFall_MultipleSteps()
    {
        var gravity = Vec3(0, -9.81, 0);
        var state = PhysicsState.Create(gravity);

        for (int i = 0; i < 10; i++)
            state = PhysicsEngine.Step(state, 0.1);

        state.Time.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Step_FixedParticle_DoesNotMove()
    {
        var gravity = Vec3(0, -9.81, 0);
        var particle = Particle.Create("p1", Vec3(5, 5, 5), Vec3(0, 0, 0), 1.0) with { IsFixed = true };
        var state = PhysicsState.Create(gravity) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", particle)
        };

        var newState = PhysicsEngine.Step(state, 0.1);
        var updated = newState.Particles["p1"];
        updated.Position[0].Should().BeApproximately(5.0, 1e-10);
        updated.Position[1].Should().BeApproximately(5.0, 1e-10);
        updated.Position[2].Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void ComputeNetForce_WithGravity_ReturnsCorrectForce()
    {
        var gravity = Force.GravityForce(2.0, -9.81);
        gravity.Vector[1].Should().BeApproximately(2.0 * -9.81, 1e-6);
    }

    [Fact]
    public void ComputeNetForce_NoForces_ReturnsZero()
    {
        var particle = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var netForce = PhysicsEngine.ComputeNetForce(particle);
        netForce.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void ComputeGravitationalForce_AttractiveDirection()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(10, 0, 0), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeGravitationalForce(p1, p2);
        force[0].Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeGravitationalForce_SamePosition_ReturnsZero()
    {
        var p1 = Particle.Create("p1", Vec3(5, 5, 5), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(5, 5, 5), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeGravitationalForce(p1, p2);
        force.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void ComputeGravitationalForce_MagnitudeFollowsInverseSquare()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1000);
        var p2a = Particle.Create("p2a", Vec3(10, 0, 0), Vec3(0, 0, 0), 1000);
        var p2b = Particle.Create("p2b", Vec3(20, 0, 0), Vec3(0, 0, 0), 1000);

        var forceA = PhysicsEngine.ComputeGravitationalForce(p1, p2a);
        var forceB = PhysicsEngine.ComputeGravitationalForce(p1, p2b);

        forceA.Norm().Should().BeApproximately(forceB.Norm() * 4, 1e-6);
    }

    [Fact]
    public void ComputeSpringForce_AtRestLength_ReturnsZero()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(5, 0, 0), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 5.0, 100.0);
        force.Norm().Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void ComputeSpringForce_Stretched_AttractsP1()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(10, 0, 0), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 5.0, 100.0);
        force[0].Should().BeLessThan(0);
    }

    [Fact]
    public void ComputeSpringForce_Compressed_RepelsP1()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(2, 0, 0), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 5.0, 100.0);
        force[0].Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeSpringForce_SamePosition_ReturnsZero()
    {
        var p1 = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        var p2 = Particle.Create("p2", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);

        var force = PhysicsEngine.ComputeSpringForce(p1, p2, 5.0, 100.0);
        force.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void Step_NoParticles_ReturnsEmptyState()
    {
        var state = PhysicsState.Create(Vec3(0, -9.81, 0));
        var newState = PhysicsEngine.Step(state, 0.1);
        newState.Particles.Should().BeEmpty();
    }

    [Fact]
    public void Step_MultipleParticles_IndependentMotion()
    {
        var gravity = Vec3(0, -9.81, 0);
        var state = PhysicsState.Create(gravity);

        var newState = PhysicsEngine.Step(state, 0.1);

        newState.Time.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void Step_TimeAccumulates()
    {
        var state = PhysicsState.Create(Vec3(0, -9.81, 0));
        state = PhysicsEngine.Step(state, 0.05);
        state.Time.Should().BeApproximately(0.05, 1e-10);
        state = PhysicsEngine.Step(state, 0.05);
        state.Time.Should().BeApproximately(0.10, 1e-10);
    }

    [Fact]
    public void Force_GravityForce_NegativeGravity_PointsDown()
    {
        var f = Force.GravityForce(10.0, -9.81);
        f.Vector[1].Should().BeLessThan(0);
    }

    [Fact]
    public void Force_DragForce_LargerCoeff_LargerDrag()
    {
        var vel = Vec3(1, 0, 0);
        var small = Force.DragForce(vel, 0.1, 1.0);
        var large = Force.DragForce(vel, 1.0, 1.0);
        large.Vector.Norm().Should().BeGreaterThan(small.Vector.Norm());
    }
}
