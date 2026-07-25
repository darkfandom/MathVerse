namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class RigidBodyTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void RigidBody_DefaultValues()
    {
        var rb = new RigidBody();
        rb.Id.Should().Be(string.Empty);
        rb.Position.Should().Be(MVVector.Zero);
        rb.Velocity.Should().Be(MVVector.Zero);
        rb.AngularVelocity.Should().Be(MVVector.Zero);
        rb.Mass.Should().Be(0);
        rb.IsFixed.Should().BeFalse();
    }

    [Fact]
    public void RigidBody_SetsId()
    {
        var rb = new RigidBody { Id = "body1" };
        rb.Id.Should().Be("body1");
    }

    [Fact]
    public void RigidBody_SetsMass()
    {
        var rb = new RigidBody { Mass = 5.0 };
        rb.Mass.Should().Be(5.0);
    }

    [Fact]
    public void RigidBody_SetsPosition()
    {
        var pos = Vec3(1, 2, 3);
        var rb = new RigidBody { Position = pos };
        rb.Position.Should().Be(pos);
    }

    [Fact]
    public void RigidBody_SetsVelocity()
    {
        var vel = Vec3(4, 5, 6);
        var rb = new RigidBody { Velocity = vel };
        rb.Velocity.Should().Be(vel);
    }

    [Fact]
    public void RigidBody_SetsAngularVelocity()
    {
        var av = Vec3(0.1, 0.2, 0.3);
        var rb = new RigidBody { AngularVelocity = av };
        rb.AngularVelocity.Should().Be(av);
    }

    [Fact]
    public void RigidBody_Forces_EmptyByDefault()
    {
        var rb = new RigidBody();
        rb.Forces.Should().BeEmpty();
    }

    [Fact]
    public void RigidBody_Torques_EmptyByDefault()
    {
        var rb = new RigidBody();
        rb.Torques.Should().BeEmpty();
    }

    [Fact]
    public void RigidBody_IsFixed_CanBeSet()
    {
        var rb = new RigidBody { IsFixed = true };
        rb.IsFixed.Should().BeTrue();
    }

    [Fact]
    public void RigidBody_WithRecord_CreatesNewInstance()
    {
        var rb1 = new RigidBody { Id = "a", Mass = 1.0 };
        var rb2 = rb1 with { Mass = 2.0 };
        rb2.Mass.Should().Be(2.0);
        rb1.Mass.Should().Be(1.0);
    }
}
