namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class ParticleTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void Create_SetsAllProperties()
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
    public void Create_DefaultRadius_IsPointOne()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.Radius.Should().Be(0.1);
    }

    [Fact]
    public void NetForce_NoForces_IsZero()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.NetForce.Should().Be(MVVector.Zero);
    }

    [Fact]
    public void NetForce_SingleForce_StoresForce()
    {
        var force = Force.GravityForce(1.0, -9.81);
        force.Vector[1].Should().BeApproximately(-9.81, 1e-10);
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0) with
        {
            Forces = ImmutableArray.Create(force)
        };
        p.Forces.Should().HaveCount(1);
    }

    [Fact]
    public void NetForce_MultipleForces_CountsCorrectly()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0) with
        {
            Forces = ImmutableArray.Create(
                Force.GravityForce(1.0, -9.81),
                Force.GravityForce(1.0, -9.81))
        };
        p.Forces.Should().HaveCount(2);
    }

    [Fact]
    public void Momentum_CalculatedCorrectly()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(3, 0, 0), 4.0);
        p.Momentum[0].Should().BeApproximately(12.0, 1e-10);
        p.Momentum[1].Should().BeApproximately(0.0, 1e-10);
        p.Momentum[2].Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void KineticEnergy_CalculatedCorrectly()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(1, 0, 0), 2.0);
        p.KineticEnergy.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void KineticEnergy_3DVelocity()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(1, 2, 3), 2.0);
        double expected = 0.5 * 2.0 * (1 + 4 + 9);
        p.KineticEnergy.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void KineticEnergy_Stationary_IsZero()
    {
        var p = Particle.Create("p1", Vec3(1, 2, 3), Vec3(0, 0, 0), 5.0);
        p.KineticEnergy.Should().Be(0);
    }

    [Fact]
    public void IsFixed_DefaultIsFalse()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.IsFixed.Should().BeFalse();
    }

    [Fact]
    public void Charge_DefaultIsZero()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.Charge.Should().Be(0);
    }

    [Fact]
    public void Properties_DefaultIsEmpty()
    {
        var p = Particle.Create("p1", Vec3(0, 0, 0), Vec3(0, 0, 0), 1.0);
        p.Properties.Should().BeEmpty();
    }

    [Fact]
    public void Force_GravityForce_TypeIsGravity()
    {
        var f = Force.GravityForce(5.0, -9.81);
        f.Type.Should().Be(ForceType.Gravity);
        f.Source.Should().Be("gravity");
    }

    [Fact]
    public void Force_GravityForce_VectorMagnitude()
    {
        var f = Force.GravityForce(2.0, -9.81);
        f.Vector.Norm().Should().BeApproximately(2.0 * 9.81, 1e-10);
    }

    [Fact]
    public void Force_DragForce_OpposesMotion()
    {
        var f = Force.DragForce(Vec3(1, 0, 0), 0.5, 1.0);
        f.Vector[0].Should().BeNegative();
        f.Type.Should().Be(ForceType.Drag);
    }
}
