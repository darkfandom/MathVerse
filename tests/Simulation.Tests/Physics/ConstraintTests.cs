namespace MathVerse.Simulation.Tests.Physics;

using System.Collections.Immutable;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed class ConstraintTests
{
    private static MVVector Vec3(double x, double y, double z) => new(x, y, z);

    [Fact]
    public void ConstraintType_Distance_HasCorrectValue()
    {
        ConstraintType.Distance.Should().Be(ConstraintType.Distance);
    }

    [Fact]
    public void ConstraintType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<ConstraintType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ConstraintType_ContainsAllExpectedValues()
    {
        Enum.GetValues<ConstraintType>().Should().HaveCount(7);
    }

    [Fact]
    public void DistanceConstraint_SetsId()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.Id.Should().Be("dist1");
    }

    [Fact]
    public void DistanceConstraint_SetsType()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.Type.Should().Be(ConstraintType.Distance);
    }

    [Fact]
    public void DistanceConstraint_SetsParticleIds()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.ParticleIds.Should().HaveCount(2);
        c.ParticleIds[0].Should().Be("p1");
        c.ParticleIds[1].Should().Be("p2");
    }

    [Fact]
    public void DistanceConstraint_SetsRestLength()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 10.0);
        c.RestLength.Should().Be(10.0);
    }

    [Fact]
    public void DistanceConstraint_SetsStiffness()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0, 500);
        c.Stiffness.Should().Be(500);
    }

    [Fact]
    public void DistanceConstraint_DefaultStiffness()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.Stiffness.Should().Be(1000);
    }

    [Fact]
    public void DistanceConstraint_DefaultDamping()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.Damping.Should().Be(10);
    }

    [Fact]
    public void DistanceConstraint_IsActiveByDefault()
    {
        var c = Constraint.Distance("dist1", "p1", "p2", 5.0);
        c.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FixedConstraint_SetsType()
    {
        var c = Constraint.Fixed("fix1", "p1", Vec3(0, 0, 0));
        c.Type.Should().Be(ConstraintType.Fixed);
    }

    [Fact]
    public void FixedConstraint_SetsSingleParticle()
    {
        var c = Constraint.Fixed("fix1", "p1", Vec3(0, 0, 0));
        c.ParticleIds.Should().HaveCount(1);
        c.ParticleIds[0].Should().Be("p1");
    }

    [Fact]
    public void FixedConstraint_VeryHighStiffness()
    {
        var c = Constraint.Fixed("fix1", "p1", Vec3(1, 2, 3));
        c.Stiffness.Should().Be(1e6);
    }

    [Fact]
    public void HingeConstraint_SetsType()
    {
        var c = Constraint.Hinge("h1", "b1", "b2", Vec3(0, 0, 0), Vec3(0, 0, 0), Vec3(0, 0, 1));
        c.Type.Should().Be(ConstraintType.Hinge);
    }

    [Fact]
    public void HingeConstraint_SetsTwoParticles()
    {
        var c = Constraint.Hinge("h1", "b1", "b2", Vec3(0, 0, 0), Vec3(1, 0, 0), Vec3(0, 0, 1));
        c.ParticleIds.Should().HaveCount(2);
        c.ParticleIds[0].Should().Be("b1");
        c.ParticleIds[1].Should().Be("b2");
    }

    [Fact]
    public void HingeConstraint_StoresAnchorsInParameters()
    {
        var anchor1 = Vec3(0, 0, 0);
        var anchor2 = Vec3(1, 0, 0);
        var c = Constraint.Hinge("h1", "b1", "b2", anchor1, anchor2, Vec3(0, 0, 1));
        c.Parameters.Should().ContainKey("anchor1");
        c.Parameters.Should().ContainKey("anchor2");
    }

    [Fact]
    public void HingeConstraint_StoresAxisInParameters()
    {
        var axis = Vec3(0, 0, 1);
        var c = Constraint.Hinge("h1", "b1", "b2", Vec3(0, 0, 0), Vec3(0, 0, 0), axis);
        c.Parameters.Should().ContainKey("axis");
    }

    [Fact]
    public void CustomConstraint_DefaultProperties()
    {
        var c = new Constraint();
        c.Id.Should().Be(string.Empty);
        c.ParticleIds.Should().BeEmpty();
        c.RestLength.Should().Be(0);
        c.Stiffness.Should().Be(0);
        c.Damping.Should().Be(0);
    }

    [Fact]
    public void ZeroLengthDistanceConstraint()
    {
        var c = Constraint.Distance("z1", "p1", "p2", 0.0);
        c.RestLength.Should().Be(0);
    }
}
