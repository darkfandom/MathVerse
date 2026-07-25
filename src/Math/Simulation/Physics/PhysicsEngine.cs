namespace MathVerse.Math.Simulation.Physics;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed record PhysicsState
{
    public ImmutableDictionary<string, Particle> Particles { get; init; } = ImmutableDictionary<string, Particle>.Empty;
    public ImmutableDictionary<string, RigidBody> RigidBodies { get; init; } = ImmutableDictionary<string, RigidBody>.Empty;
    public MVVector Gravity { get; init; } = MVVector.Zero;
    public ImmutableArray<Constraint> Constraints { get; init; } = ImmutableArray<Constraint>.Empty;
    public double Time { get; init; }

    public static PhysicsState Create(MVVector gravity) => new()
    {
        Gravity = gravity,
    };
}

public sealed record Particle
{
    public string Id { get; init; } = string.Empty;
    public MVVector Position { get; init; } = MVVector.Zero;
    public MVVector Velocity { get; init; } = MVVector.Zero;
    public MVVector Acceleration { get; init; } = MVVector.Zero;
    public double Mass { get; init; }
    public double Charge { get; init; }
    public double Radius { get; init; }
    public ImmutableArray<Force> Forces { get; init; } = ImmutableArray<Force>.Empty;
    public ImmutableDictionary<string, object> Properties { get; init; } = ImmutableDictionary<string, object>.Empty;
    public bool IsFixed { get; init; }

    public MVVector NetForce => Forces.IsDefaultOrEmpty ? MVVector.Zero : Forces.Aggregate(MVVector.Zero, (acc, f) => f.Vector.Add(acc));
    public MVVector Momentum => Velocity.Scale(Mass);
    public double KineticEnergy => 0.5 * Mass * Velocity.Dot(Velocity);

    public static Particle Create(string id, MVVector position, MVVector velocity, double mass, double radius = 0.1)
        => new()
        {
            Id = id,
            Position = position,
            Velocity = velocity,
            Mass = mass,
            Radius = radius,
        };
}

public sealed record RigidBody
{
    public string Id { get; init; } = string.Empty;
    public MVVector Position { get; init; } = MVVector.Zero;
    public MVVector Velocity { get; init; } = MVVector.Zero;
    public MVVector AngularVelocity { get; init; } = MVVector.Zero;
    public Quaternion Orientation { get; init; } = Quaternion.Identity;
    public Matrix InertiaTensor { get; init; } = Matrix.Identity(3);
    public double Mass { get; init; }
    public ImmutableArray<Force> Forces { get; init; } = ImmutableArray<Force>.Empty;
    public ImmutableArray<Torque> Torques { get; init; } = ImmutableArray<Torque>.Empty;
    public bool IsFixed { get; init; }
}

public sealed record Force
{
    public MVVector Vector { get; init; } = MVVector.Zero;
    public ForceType Type { get; init; }
    public string Source { get; init; } = string.Empty;
    public ImmutableDictionary<string, object> Parameters { get; init; } = ImmutableDictionary<string, object>.Empty;

    public static Force GravityForce(double mass, double gravityY) => new()
    {
        Vector = new MVVector(0, gravityY * mass, 0),
        Type = ForceType.Gravity,
        Source = "gravity"
    };

    public static Force SpringForce(MVVector displacement, double k, double restLength = 0) => new()
    {
        Vector = VectorOperations.Normalize(displacement).Scale(-k * (displacement.Norm() - restLength)),
        Type = ForceType.Spring,
        Source = "spring"
    };

    public static Force DragForce(MVVector velocity, double dragCoeff, double crossSection) => new()
    {
        Vector = velocity.Scale(-dragCoeff * crossSection * velocity.Norm()),
        Type = ForceType.Drag,
        Source = "drag"
    };
}

public enum ForceType
{
    Gravity,
    Spring,
    Drag,
    Electric,
    Magnetic,
    Contact,
    Friction,
    Tension,
    Custom
}

public sealed record Torque
{
    public MVVector Vector { get; init; } = MVVector.Zero;
    public string Source { get; init; } = string.Empty;
}

public sealed record Constraint
{
    public string Id { get; init; } = string.Empty;
    public ConstraintType Type { get; init; }
    public ImmutableArray<string> ParticleIds { get; init; } = ImmutableArray<string>.Empty;
    public double RestLength { get; init; }
    public double Stiffness { get; init; }
    public double Damping { get; init; }
    public ImmutableDictionary<string, object> Parameters { get; init; } = ImmutableDictionary<string, object>.Empty;
    public bool IsActive { get; init; } = true;

    public static Constraint Distance(string id, string p1, string p2, double length, double stiffness = 1000) =>
        new() { Id = id, Type = ConstraintType.Distance, ParticleIds = ImmutableArray.Create(p1, p2), RestLength = length, Stiffness = stiffness, Damping = 10 };

    public static Constraint Fixed(string id, string particleId, MVVector position) =>
        new() { Id = id, Type = ConstraintType.Fixed, ParticleIds = ImmutableArray.Create(particleId), Stiffness = 1e6 };

    public static Constraint Hinge(string id, string body1, string body2, MVVector anchor1, MVVector anchor2, MVVector axis) =>
        new() { Id = id, Type = ConstraintType.Hinge, ParticleIds = ImmutableArray.Create(body1, body2), Parameters = ImmutableDictionary<string, object>.Empty.Add("anchor1", anchor1).Add("anchor2", anchor2).Add("axis", axis) };
}

public enum ConstraintType
{
    Distance,
    Fixed,
    Hinge,
    Slider,
    BallSocket,
    Universal,
    Custom
}

public static class PhysicsEngine
{
    public static PhysicsState Step(PhysicsState state, double dt)
    {
        var newParticles = state.Particles.ToBuilder();
        foreach (var kvp in state.Particles)
        {
            var particle = kvp.Value;
            if (particle.IsFixed) continue;

            var netForce = particle.NetForce;
            var gravity = state.Gravity;
            if (gravity.Size > 0 && particle.Position.Size > 0)
            {
                var gravForce = gravity.Scale(particle.Mass);
                netForce = netForce.Size > 0 ? netForce.Add(gravForce) : gravForce;
            }
            var accelDim = System.Math.Max(netForce.Size, particle.Position.Size);
            var acceleration = netForce.Size > 0 ? netForce.Scale(1.0 / particle.Mass) : MVVector.ZeroOf(accelDim);
            var newVelocity = particle.Velocity.Add(acceleration.Scale(dt));
            var newPosition = particle.Position.Add(newVelocity.Scale(dt));

            newParticles[kvp.Key] = particle with
            {
                Position = newPosition,
                Velocity = newVelocity,
                Acceleration = acceleration
            };
        }

        return state with
        {
            Particles = newParticles.ToImmutable(),
            Time = state.Time + dt
        };
    }

    public static MVVector ComputeNetForce(Particle particle)
    {
        return particle.NetForce;
    }

    public static MVVector ComputeGravitationalForce(Particle p1, Particle p2)
    {
        var r = p2.Position.Subtract(p1.Position);
        double distance = r.Norm();
        if (distance < 1e-10) return MVVector.Zero;

        double G = 6.67430e-11;
        double forceMag = G * p1.Mass * p2.Mass / (distance * distance);
        return VectorOperations.Normalize(r).Scale(forceMag);
    }

    public static MVVector ComputeSpringForce(Particle p1, Particle p2, double restLength, double stiffness, double damping = 0)
    {
        var displacement = p2.Position.Subtract(p1.Position);
        double distance = displacement.Norm();
        if (distance < 1e-10) return MVVector.Zero;

        double forceMag = -stiffness * (distance - restLength);
        return VectorOperations.Normalize(displacement).Scale(forceMag);
    }
}
