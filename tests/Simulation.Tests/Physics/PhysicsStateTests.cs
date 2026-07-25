namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class PhysicsStateTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void Create_SetGravity()
    {
        var gravity = Vec3(0, -9.81, 0);
        var state = PhysicsState.Create(gravity);
        state.Gravity.Should().Be(gravity);
    }

    [Fact]
    public void Create_EmptyParticles()
    {
        var state = PhysicsState.Create(Vec3(0, -9.81, 0));
        state.Particles.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyConstraints()
    {
        var state = PhysicsState.Create(Vec3(0, -9.81, 0));
        state.Constraints.Should().BeEmpty();
    }

    [Fact]
    public void Create_TimeIsZero()
    {
        var state = PhysicsState.Create(Vec3(0, 0, 0));
        state.Time.Should().Be(0);
    }

    [Fact]
    public void Particle_Create_SetsProperties()
    {
        var pos = Vec3(1, 2, 3);
        var vel = Vec3(4, 5, 6);
        var p = Particle.Create("p1", pos, vel, 2.0, 0.5);
        p.Id.Should().Be("p1");
        p.Position.Should().Be(pos);
        p.Velocity.Should().Be(vel);
        p.Mass.Should().Be(2.0);
        p.Radius.Should().Be(0.5);
    }

    [Fact]
    public void Particle_Create_DefaultRadius_IsPointOne()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.Radius.Should().Be(0.1);
    }

    [Fact]
    public void Particle_KineticEnergy_CalculatedCorrectly()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(1, 0, 0), 2.0);
        p.KineticEnergy.Should().Be(0.5 * 2.0 * 1.0);
    }

    [Fact]
    public void Particle_Momentum_CalculatedCorrectly()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(3, 0, 0), 4.0);
        p.Momentum[0].Should().BeApproximately(12.0, 1e-10);
        p.Momentum[1].Should().BeApproximately(0.0, 1e-10);
        p.Momentum[2].Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Particle_NetForce_NoForces_IsZero()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.NetForce.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void Force_GravityForce_CorrectVector()
    {
        var f = Force.GravityForce(5.0, -9.81);
        f.Vector[0].Should().BeApproximately(0, 1e-10);
        f.Vector[1].Should().BeApproximately(5.0 * -9.81, 1e-10);
        f.Vector[2].Should().BeApproximately(0, 1e-10);
        f.Type.Should().Be(ForceType.Gravity);
    }

    [Fact]
    public void Force_SpringForce_Compresses()
    {
        var displacement = Vec3(0.5, 0, 0);
        var f = Force.SpringForce(displacement, 100.0, 0.0);
        f.Type.Should().Be(ForceType.Spring);
        f.Vector[0].Should().BeLessThan(0);
    }

    [Fact]
    public void Force_DragForce_OpposesVelocity()
    {
        var velocity = Vec3(1, 0, 0);
        var f = Force.DragForce(velocity, 0.5, 1.0);
        f.Type.Should().Be(ForceType.Drag);
        f.Vector[0].Should().BeLessThan(0);
    }

    [Fact]
    public void Force_DragForce_ZeroVelocity_IsZero()
    {
        var velocity = Vec3(0, 0, 0);
        var f = Force.DragForce(velocity, 0.5, 1.0);
        f.Vector.Norm().Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Constraint_Distance_SetsProperties()
    {
        var c = Constraint.Distance("c1", "p1", "p2", 5.0, 200);
        c.Id.Should().Be("c1");
        c.Type.Should().Be(ConstraintType.Distance);
        c.RestLength.Should().Be(5.0);
        c.Stiffness.Should().Be(200);
        c.ParticleIds.Should().HaveCount(2);
        c.ParticleIds[0].Should().Be("p1");
        c.ParticleIds[1].Should().Be("p2");
    }

    [Fact]
    public void Constraint_Distance_DefaultDamping_IsTen()
    {
        var c = Constraint.Distance("c1", "p1", "p2", 1.0);
        c.Damping.Should().Be(10);
    }

    [Fact]
    public void Constraint_Fixed_SetsStiffnessToMillion()
    {
        var c = Constraint.Fixed("c1", "p1", Vec3(0, 0, 0));
        c.Stiffness.Should().Be(1e6);
        c.Type.Should().Be(ConstraintType.Fixed);
    }

    [Fact]
    public void Particle_IsFixed_DefaultIsFalse()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.IsFixed.Should().BeFalse();
    }

    [Fact]
    public void Particle_Charge_DefaultIsZero()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.Charge.Should().Be(0);
    }

    [Fact]
    public void Particle_KineticEnergy_Stationary_IsZero()
    {
        var p = Particle.Create("p1", Vec3(1, 2, 3), Vec3(0, 0, 0), 5.0);
        p.KineticEnergy.Should().Be(0);
    }
}
